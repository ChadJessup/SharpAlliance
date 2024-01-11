using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Formats.Asn1;
using SharpAlliance.Core.SubSystems;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core;

public class ArmsDealerInit
{
    public static int GetDealersMaxItemAmount(ARMS_DEALER ubDealerID, Items usItemIndex)
    {
        switch (ubDealerID)
        {
            case ARMS_DEALER.TONY:
                return GetMaxItemAmount(gTonyInventory, usItemIndex);

            case ARMS_DEALER.FRANK:
                return GetMaxItemAmount(gFrankInventory, usItemIndex);

            case ARMS_DEALER.MICKY:
                return GetMaxItemAmount(gMickyInventory, usItemIndex);

            case ARMS_DEALER.ARNIE:
                return GetMaxItemAmount(gArnieInventory, usItemIndex);

            case ARMS_DEALER.PERKO:
                return GetMaxItemAmount(gPerkoInventory, usItemIndex);

            case ARMS_DEALER.KEITH:
                return GetMaxItemAmount(gKeithInventory, usItemIndex);

            case ARMS_DEALER.BAR_BRO_1:
                return GetMaxItemAmount(gHerveInventory, usItemIndex);

            case ARMS_DEALER.BAR_BRO_2:
                return GetMaxItemAmount(gPeterInventory, usItemIndex);

            case ARMS_DEALER.BAR_BRO_3:
                return GetMaxItemAmount(gAlbertoInventory, usItemIndex);

            case ARMS_DEALER.BAR_BRO_4:
                return GetMaxItemAmount(gCarloInventory, usItemIndex);

            case ARMS_DEALER.JAKE:
                return GetMaxItemAmount(gJakeInventory, usItemIndex);

            case ARMS_DEALER.FRANZ:
                return GetMaxItemAmount(gFranzInventory, usItemIndex);

            case ARMS_DEALER.HOWARD:
                return GetMaxItemAmount(gHowardInventory, usItemIndex);

            case ARMS_DEALER.SAM:
                return GetMaxItemAmount(gSamInventory, usItemIndex);

            case ARMS_DEALER.FREDO:
                return GetMaxItemAmount(gFredoInventory, usItemIndex);

            case ARMS_DEALER.GABBY:
                return GetMaxItemAmount(gGabbyInventory, usItemIndex);

            case ARMS_DEALER.DEVIN:
                return GetMaxItemAmount(gDevinInventory, usItemIndex);

            case ARMS_DEALER.ELGIN:
                return GetMaxItemAmount(gElginInventory, usItemIndex);

            case ARMS_DEALER.MANNY:
                return GetMaxItemAmount(gMannyInventory, usItemIndex);

            default:
                Debug.Assert(false);
                return 0;
        }
    }


    public static int GetMaxItemAmount(DEALER_POSSIBLE_INV[] pInv, Items usItemIndex)
    {
        int usCnt = 0;

        //loop through the array until a the LAST_DEALER_ITEM is hit
        while (pInv[usCnt].sItemIndex != LAST_DEALER_ITEM)
        {
            //if this item is the one we want
            if (pInv[usCnt].sItemIndex == usItemIndex)
            {
                return pInv[usCnt].ubOptimalNumber;
            }

            // move to the next item
            usCnt++;
        }

        return NO_DEALER_ITEM;
    }


    public static DEALER_POSSIBLE_INV[] GetPointerToDealersPossibleInventory(ARMS_DEALER ubArmsDealerID)
    {
        switch (ubArmsDealerID)
        {
            case ARMS_DEALER.TONY:
                return gTonyInventory;

            case ARMS_DEALER.FRANK:
                return gFrankInventory;

            case ARMS_DEALER.MICKY:
                return gMickyInventory;

            case ARMS_DEALER.ARNIE:
                return gArnieInventory;

            case ARMS_DEALER.PERKO:
                return gPerkoInventory;

            case ARMS_DEALER.KEITH:
                return gKeithInventory;

            case ARMS_DEALER.BAR_BRO_1:
                return gHerveInventory;

            case ARMS_DEALER.BAR_BRO_2:
                return gPeterInventory;

            case ARMS_DEALER.BAR_BRO_3:
                return gAlbertoInventory;

            case ARMS_DEALER.BAR_BRO_4:
                return gCarloInventory;

            case ARMS_DEALER.JAKE:
                return gJakeInventory;

            case ARMS_DEALER.FRANZ:
                return gFranzInventory;

            case ARMS_DEALER.HOWARD:
                return gHowardInventory;

            case ARMS_DEALER.SAM:
                return gSamInventory;

            case ARMS_DEALER.FREDO:
                return gFredoInventory;

            case ARMS_DEALER.GABBY:
                return gGabbyInventory;

            case ARMS_DEALER.DEVIN:
                return gDevinInventory;

            case ARMS_DEALER.ELGIN:
                return gElginInventory;

            case ARMS_DEALER.MANNY:
                return gMannyInventory;

            default:
                throw new ArgumentNullException();
        }
    }


    public static int GetCurrentSuitabilityForItem(ARMS_DEALER bArmsDealer, Items usItemIndex)
    {
        int ubItemCoolness;
        int ubMinCoolness, ubMaxCoolness;

        // item suitability varies with the player's maximum progress through the game.  The farther he gets, the better items
        // we make available.  Weak items become more and more infrequent later in the game, although they never quite vanish.

        // items illegal in this game are unsuitable [this checks guns vs. current GunSet!]
        if (!ItemSubSystem.ItemIsLegal(usItemIndex))
        {
            return ITEM_SUITABILITY_NONE;
        }

        // items normally not sold at shops are unsuitable
        if (Item[usItemIndex].fFlags.HasFlag(ItemAttributes.ITEM_NOT_BUYABLE))
        {
            return ITEM_SUITABILITY_NONE;
        }


        ubItemCoolness = Item[usItemIndex].ubCoolness;

        if (ubItemCoolness == 0)
        {
            // items without a coolness rating can't be sold to the player by shopkeepers
            return ITEM_SUITABILITY_NONE;
        }

        // the following staple items are always deemed highly suitable regardless of player's progress:
        switch (usItemIndex)
        {
            case Items.CLIP38_6:
            case Items.CLIP9_15:
            case Items.CLIP9_30:
            case Items.CLIP357_6:
            case Items.CLIP357_9:
            case Items.CLIP45_7:
            case Items.CLIP45_30:
            case Items.CLIP12G_7:
            case Items.CLIP12G_7_BUCKSHOT:
            case Items.CLIP545_30_HP:
            case Items.CLIP556_30_HP:
            case Items.CLIP762W_10_HP:
            case Items.CLIP762W_30_HP:
            case Items.CLIP762N_5_HP:
            case Items.CLIP762N_20_HP:
            case Items.FIRSTAIDKIT:
            case Items.MEDICKIT:
            case Items.TOOLKIT:
            case Items.LOCKSMITHKIT:
            case Items.CANTEEN:
            case Items.CROWBAR:
            case Items.JAR:
            case Items.JAR_ELIXIR:
            case Items.JAR_CREATURE_BLOOD:
                return ITEM_SUITABILITY_ALWAYS;
        }


        // If it's not BobbyRay, Tony, or Devin
        if ((bArmsDealer != (ARMS_DEALER)(-1)) && (bArmsDealer != ARMS_DEALER.TONY) && (bArmsDealer != ARMS_DEALER.DEVIN))
        {
            // all the other dealers have very limited inventories, so their suitability remains constant at all times in game
            return ITEM_SUITABILITY_HIGH;
        }


        // figure out the appropriate range of coolness based on player's maximum progress so far

        ubMinCoolness = Campaign.HighestPlayerProgressPercentage() / 10;
        ubMaxCoolness = (Campaign.HighestPlayerProgressPercentage() / 10) + 1;

        // Tony has the better stuff sooner (than Bobby R's)
        if (bArmsDealer == ARMS_DEALER.TONY)
        {
            ubMinCoolness += 1;
            ubMaxCoolness += 1;
        }
        else if (bArmsDealer == ARMS_DEALER.DEVIN)
        {
            // almost everything Devin sells is pretty cool (4+), so gotta apply a minimum or he'd have nothing early on
            if (ubMinCoolness < 3)
            {
                ubMinCoolness = 3;
                ubMaxCoolness = 4;
            }
        }


        ubMinCoolness = Math.Max(1, Math.Min(9, ubMinCoolness));
        ubMaxCoolness = Math.Max(2, Math.Min(10, ubMaxCoolness));


        // if item is too cool for current level of progress
        if (ubItemCoolness > ubMaxCoolness)
        {
            return ITEM_SUITABILITY_NONE;
        }

        // if item is exactly within the current coolness window
        if ((ubItemCoolness >= ubMinCoolness) && (ubItemCoolness <= ubMaxCoolness))
        {
            return ITEM_SUITABILITY_HIGH;
        }

        // if item is still relatively close to low end of the window
        if ((ubItemCoolness + 2) >= ubMinCoolness)
        {
            return ITEM_SUITABILITY_MEDIUM;
        }

        // item is way uncool for player's current progress, but it's still possible for it to make an appearance
        return ITEM_SUITABILITY_LOW;
    }

    public static int ChanceOfItemTransaction(ARMS_DEALER bArmsDealer, Items usItemIndex, int fDealerIsSelling, BOBBY_RAY fUsed)
    {
        int ubItemCoolness;
        int ubChance = 0;
        bool fBobbyRay = false;


        // make sure dealers don't carry used items that they shouldn't
        if (fUsed > 0 && fDealerIsSelling == 0 && !CanDealerItemBeSoldUsed(usItemIndex))
        {
            return 0;
        }

        if (bArmsDealer == (ARMS_DEALER)(-1))
        {
            // Bobby Ray has an easier time getting resupplied than the local dealers do
            fBobbyRay = true;
        }

        ubItemCoolness = Item[usItemIndex].ubCoolness;

        switch (GetCurrentSuitabilityForItem(bArmsDealer, usItemIndex))
        {
            case ITEM_SUITABILITY_NONE:
                if (fDealerIsSelling > 0)
                {
                    // dealer always gets rid of stuff that is too advanced or inappropriate ASAP
                    ubChance = 100;
                }
                else // dealer is buying
                {
                    // can't get these at all
                    ubChance = 0;
                }
                break;

            case ITEM_SUITABILITY_LOW:
                ubChance = fBobbyRay ? 25 : 15;
                break;

            case ITEM_SUITABILITY_MEDIUM:
                ubChance = fBobbyRay ? 50 : 30;
                break;

            case ITEM_SUITABILITY_HIGH:
                ubChance = fBobbyRay ? 75 : 50;
                break;

            case ITEM_SUITABILITY_ALWAYS:
                if (fDealerIsSelling > 0)
                {
                    // sells just like suitability high
                    ubChance = 75;
                }
                else // dealer is buying
                {
                    // dealer can always get a (re)supply of these
                    ubChance = 100;
                }
                break;

            default:
                Debug.Assert(false);
                break;
        }


        // if there's any uncertainty
        if ((ubChance > 0) && (ubChance < 100))
        {
            // cooler items sell faster
            if (fDealerIsSelling > 0)
            {
                ubChance += 5 * ubItemCoolness;

                // ARM: New - keep stuff on the shelves longer
                ubChance /= 2;
            }

            // used items are traded more rarely
            if (fUsed > 0)
            {
                ubChance /= 2;
            }
        }


        return ubChance;
    }

    public static bool ItemTransactionOccurs(ARMS_DEALER bArmsDealer, Items usItemIndex, int fDealerIsSelling, BOBBY_RAY fUsed)
    {
        int ubChance;
        int sInventorySlot;


        ubChance = ChanceOfItemTransaction(bArmsDealer, usItemIndex, fDealerIsSelling, fUsed);

        // if the dealer is buying, and a chance exists (i.e. the item is "eligible")
        if (fDealerIsSelling == 0 && (ubChance > 0))
        {
            // mark it as such
            if (bArmsDealer == (ARMS_DEALER)(-1))
            {
                if (fUsed > 0)
                {
                    sInventorySlot = BobbyR.GetInventorySlotForItem(LaptopSaveInfo.BobbyRayUsedInventory, usItemIndex, fUsed);
                    LaptopSaveInfo.BobbyRayUsedInventory[sInventorySlot].fPreviouslyEligible = true;
                }
                else
                {
                    sInventorySlot = BobbyR.GetInventorySlotForItem(LaptopSaveInfo.BobbyRayInventory, usItemIndex, fUsed);
                    LaptopSaveInfo.BobbyRayInventory[sInventorySlot].fPreviouslyEligible = true;
                }
            }
            else
            {
                gArmsDealersInventory[bArmsDealer][usItemIndex].fPreviouslyEligible = true;
            }
        }

        // roll to see if a transaction occurs
        if (Globals.Random.Next(100) < ubChance)
        {
            return true;
        }
        else
        {
            return false;
        }
    }



    public static int DetermineInitialInvItems(ARMS_DEALER bArmsDealerID, Items usItemIndex, int ubChances, BOBBY_RAY fUsed)
    {
        int ubNumBought;
        int ubCnt;

        // initial inventory is now rolled for one item at a time, instead of one type at a time, to improve variety
        ubNumBought = 0;
        for (ubCnt = 0; ubCnt < ubChances; ubCnt++)
        {
            if (ItemTransactionOccurs(bArmsDealerID, usItemIndex, DEALER_BUYING, fUsed))
            {
                ubNumBought++;
            }
        }

        return ubNumBought;
    }

    public static int HowManyItemsAreSold(ARMS_DEALER bArmsDealerID, Items usItemIndex, int ubNumInStock, BOBBY_RAY fUsed)
    {
        int ubNumSold;
        int ubCnt;

        // items are now virtually "sold" one at a time
        ubNumSold = 0;
        for (ubCnt = 0; ubCnt < ubNumInStock; ubCnt++)
        {
            if (ItemTransactionOccurs(bArmsDealerID, usItemIndex, DEALER_SELLING, fUsed))
            {
                ubNumSold++;
            }
        }

        return ubNumSold;
    }



    public static int HowManyItemsToReorder(int ubWanted, int ubStillHave)
    {
        int ubNumReordered;

        Debug.Assert(ubStillHave <= ubWanted);

        ubNumReordered = ubWanted - ubStillHave;

        //randomize the amount. 33% of the time we add to it, 33% we subtract from it, rest leave it alone
        switch (Globals.Random.Next(3))
        {
            case 0:
                ubNumReordered += ubNumReordered / 2;
                break;
            case 1:
                ubNumReordered -= ubNumReordered / 2;
                break;
        }

        return ubNumReordered;
    }



    private static int BobbyRayItemQsortCompare(STORE_INVENTORY pArg1, STORE_INVENTORY pArg2)
    {
        Items usItem1Index;
        Items usItem2Index;
        int ubItem1Quality;
        int ubItem2Quality;

        usItem1Index = pArg1.usItemIndex;
        usItem2Index = pArg2.usItemIndex;

        ubItem1Quality = pArg1.ubItemQuality;
        ubItem2Quality = pArg2.ubItemQuality;

        return CompareItemsForSorting(usItem1Index, usItem2Index, ubItem1Quality, ubItem2Quality);
    }



    int ArmsDealerItemQsortCompare(INVENTORY_IN_SLOT pArg1, INVENTORY_IN_SLOT pArg2)
    {
        Items usItem1Index;
        Items usItem2Index;
        int ubItem1Quality;
        int ubItem2Quality;

        usItem1Index = pArg1.sItemIndex;
        usItem2Index = pArg2.sItemIndex;

        ubItem1Quality = pArg1.ItemObject.bStatus[0];
        ubItem2Quality = pArg2.ItemObject.bStatus[0];

        return CompareItemsForSorting(usItem1Index, usItem2Index, ubItem1Quality, ubItem2Quality);
    }

    int RepairmanItemQsortCompare(INVENTORY_IN_SLOT pArg1, INVENTORY_IN_SLOT pArg2)
    {
        INVENTORY_IN_SLOT pInvSlot1;
        INVENTORY_IN_SLOT pInvSlot2;
        int uiRepairTime1;
        int uiRepairTime2;


        pInvSlot1 = pArg1;
        pInvSlot2 = pArg2;

        //        Debug.Assert(pInvSlot1.sSpecialItemElement != -1);
        //        Debug.Assert(pInvSlot2.sSpecialItemElement != -1);
        //
        //        uiRepairTime1 = gArmsDealersInventory[gbSelectedArmsDealerID][pInvSlot1.sItemIndex].SpecialItem[pInvSlot1.sSpecialItemElement].uiRepairDoneTime;
        //        uiRepairTime2 = gArmsDealersInventory[gbSelectedArmsDealerID][pInvSlot2.sItemIndex].SpecialItem[pInvSlot2.sSpecialItemElement].uiRepairDoneTime;


        // lower reapir time first
        //        if (uiRepairTime1 < uiRepairTime2)
        //        {
        //            return (-1);
        //        }
        //        else if (uiRepairTime1 > uiRepairTime2)
        //        {
        //            return (1);
        //        }
        //        else
        {
            return 0;
        }
    }



    private static int CompareItemsForSorting(Items usItem1Index, Items usItem2Index, int ubItem1Quality, int ubItem2Quality)
    {
        int ubItem1Category;
        int ubItem2Category;
        int usItem1Price;
        int usItem2Price;
        int ubItem1Coolness;
        int ubItem2Coolness;

        ubItem1Category = GetDealerItemCategoryNumber(usItem1Index);
        ubItem2Category = GetDealerItemCategoryNumber(usItem2Index);

        // lower category first
        if (ubItem1Category < ubItem2Category)
        {
            return -1;
        }
        else
        if (ubItem1Category > ubItem2Category)
        {
            return 1;
        }
        else
        {
            // the same category 
            if (Item[usItem1Index].usItemClass == IC.AMMO && Item[usItem2Index].usItemClass == IC.AMMO)
            {
                CaliberType ubItem1Calibre;
                CaliberType ubItem2Calibre;
                int ubItem1MagSize;
                int ubItem2MagSize;

                // AMMO is sorted by caliber first
                ubItem1Calibre = WeaponTypes.Magazine[Item[usItem1Index].ubClassIndex].ubCalibre;
                ubItem2Calibre = WeaponTypes.Magazine[Item[usItem2Index].ubClassIndex].ubCalibre;
                if (ubItem1Calibre > ubItem2Calibre)
                {
                    return -1;
                }
                else
                if (ubItem1Calibre < ubItem2Calibre)
                {
                    return 1;
                }
                // the same caliber - compare size of magazine, then fall out of if statement
                ubItem1MagSize = WeaponTypes.Magazine[Item[usItem1Index].ubClassIndex].ubMagSize;
                ubItem2MagSize = WeaponTypes.Magazine[Item[usItem2Index].ubClassIndex].ubMagSize;
                if (ubItem1MagSize > ubItem2MagSize)
                {
                    return -1;
                }
                else
                if (ubItem1MagSize < ubItem2MagSize)
                {
                    return 1;
                }

            }
            else
            {
                // items other than ammo are compared on coolness first
                ubItem1Coolness = Item[usItem1Index].ubCoolness;
                ubItem2Coolness = Item[usItem2Index].ubCoolness;

                // higher coolness first
                if (ubItem1Coolness > ubItem2Coolness)
                {
                    return -1;
                }
                else
                if (ubItem1Coolness < ubItem2Coolness)
                {
                    return 1;
                }
            }

            // the same coolness/caliber - compare base prices then
            usItem1Price = Item[usItem1Index].usPrice;
            usItem2Price = Item[usItem2Index].usPrice;

            // higher price first
            if (usItem1Price > usItem2Price)
            {
                return -1;
            }
            else
            if (usItem1Price < usItem2Price)
            {
                return 1;
            }
            else
            {
                // the same price - compare item #s, then

                // lower index first
                if (usItem1Index < usItem2Index)
                {
                    return -1;
                }
                else
                if (usItem1Index > usItem2Index)
                {
                    return 1;
                }
                else
                {
                    // same item type = compare item quality, then

                    // higher quality first
                    if (ubItem1Quality > ubItem2Quality)
                    {
                        return -1;
                    }
                    else
                    if (ubItem1Quality < ubItem2Quality)
                    {
                        return 1;
                    }
                    else
                    {
                        // identical items!
                        return 0;
                    }
                }
            }
        }
    }



    public static int GetDealerItemCategoryNumber(Items usItemIndex)
    {
        IC uiItemClass;
        WeaponClass ubWeaponClass;
        int ubCategory = 0;


        uiItemClass = Item[usItemIndex].usItemClass;

        if (usItemIndex < Items.MAX_WEAPONS)
        {
            ubWeaponClass = WeaponTypes.Weapon[usItemIndex].ubWeaponClass;
        }
        else
        {
            // not a weapon, so no weapon class, this won't be needed
            ubWeaponClass = 0;
        }


        ubCategory = 0;

        // search table until end-of-list marker is encountered
        while (DealerItemSortInfo[ubCategory].uiItemClass != IC.NONE)
        {
            if (DealerItemSortInfo[ubCategory].uiItemClass == uiItemClass)
            {
                // if not a type of gun
                if (uiItemClass != IC.GUN)
                {
                    // then we're found it
                    return ubCategory;
                }
                else
                {
                    // for guns, must also match on weapon class
                    if (DealerItemSortInfo[ubCategory].ubWeaponClass == ubWeaponClass)
                    {
                        // then we're found it
                        return ubCategory;
                    }
                }
            }

            // check vs. next category in the list
            ubCategory++;
        }

        // should never be trying to locate an item that's not covered in the table!
        Debug.Assert(false);
        return 0;
    }

    public static bool ItemContainsLiquid(Items usItemIndex)
    {
        switch (usItemIndex)
        {
            case Items.CANTEEN:
            case Items.BEER:
            case Items.ALCOHOL:
            case Items.JAR_HUMAN_BLOOD:
            case Items.JAR_CREATURE_BLOOD:
            case Items.JAR_QUEEN_CREATURE_BLOOD:
            case Items.JAR_ELIXIR:
            case Items.GAS_CAN:
                return true;
        }

        return false;
    }

    public static int DetermineDealerItemCondition(ARMS_DEALER ubArmsDealer, Items usItemIndex)
    {
        int ubCondition = 100;

        // if it's a damagable item, and not a liquid (those are always sold full)
        if (Item[usItemIndex].fFlags.HasFlag(ItemAttributes.ITEM_DAMAGEABLE) && !ItemContainsLiquid(usItemIndex))
        {
            // if he ONLY has used items, or 50% of the time if he carries both used & new items
            if (ArmsDealerInfo[ubArmsDealer].uiFlags.HasFlag(ARMS_DEALER_ITEM.ONLY_USED_ITEMS) ||
                 (ArmsDealerInfo[ubArmsDealer].uiFlags.HasFlag(ARMS_DEALER_ITEM.SOME_USED_ITEMS) && (Globals.Random.Next(100) < 50)))
            {
                // make the item a used one
                ubCondition = 20 + Globals.Random.Next(60);
            }
        }

        return ubCondition;
    }

    public static void SetSpecialItemInfoToDefaults(out SPECIAL_ITEM_INFO pSpclItemInfo)
    {
        int ubCnt;

        //memset(pSpclItemInfo, 0, sizeof(SPECIAL_ITEM_INFO));
        pSpclItemInfo = new()
        {
            bItemCondition = 100,
            ubImprintID = NO_PROFILE,
        };

        for (ubCnt = 0; ubCnt < MAX_ATTACHMENTS; ubCnt++)
        {
            pSpclItemInfo.usAttachment[ubCnt] = (int)Items.NONE;
            pSpclItemInfo.bAttachmentStatus[ubCnt] = 0;
        }
    }

    public static void ArmsDealerGetsFreshStock(ARMS_DEALER ubArmsDealer, Items usItemIndex, int ubNumItems)
    {
        int ubCnt;
        int ubItemCondition;
        int ubPerfectOnes = 0;

        // create item info describing a perfect item
        SetSpecialItemInfoToDefaults(out SPECIAL_ITEM_INFO? SpclItemInfo);


        // determine the condition of each one, counting up new ones, but adding damaged ones right away
        for (ubCnt = 0; ubCnt < ubNumItems; ubCnt++)
        {
            ubItemCondition = DetermineDealerItemCondition(ubArmsDealer, usItemIndex);

            // if the item is brand new
            if (ubItemCondition == 100)
            {
                ubPerfectOnes++;
            }
            else
            {
                // add a used item with that condition to his inventory
                SpclItemInfo.bItemCondition = ubItemCondition;
                AddItemToArmsDealerInventory(ubArmsDealer, usItemIndex, SpclItemInfo, 1);
            }
        }

        // now add all the perfect ones, in one shot
        if (ubPerfectOnes > 0)
        {
            SpclItemInfo.bItemCondition = 100;
            AddItemToArmsDealerInventory(ubArmsDealer, usItemIndex, SpclItemInfo, ubPerfectOnes);
        }
    }

    //Use AddObjectToArmsDealerInventory() instead of this when converting a complex item in OBJECTTYPE format.
    public static void AddItemToArmsDealerInventory(ARMS_DEALER ubArmsDealer, Items usItemIndex, SPECIAL_ITEM_INFO? pSpclItemInfo, int ubHowMany)
    {
        int ubRoomLeft;
        int ubElement;
        int ubElementsToAdd;
        bool fFoundOne;
        bool fSuccess;

        Debug.Assert(ubHowMany > 0);

        ubRoomLeft = 255 - gArmsDealersInventory[ubArmsDealer][usItemIndex].ubTotalItems;

        if (ubHowMany > ubRoomLeft)
        {
            // not enough room to store that many, any extras vanish into thin air!
            ubHowMany = ubRoomLeft;
        }

        if (ubHowMany == 0)
        {
            return;
        }


        // decide whether this item is "special" or not
        if (IsItemInfoSpecial(pSpclItemInfo))
        {
            // Anything that's used/damaged or imprinted is store as a special item in the SpecialItem array,
            // exactly one item per element.  We (re)allocate memory dynamically as necessary to hold the additional items.

            do
            {
                // search for an already allocated, empty element in the special item array
                fFoundOne = false;
                for (ubElement = 0; ubElement < gArmsDealersInventory[ubArmsDealer][usItemIndex].ubElementsAlloced; ubElement++)
                {
                    if (!gArmsDealersInventory[ubArmsDealer][usItemIndex].SpecialItem[ubElement].fActive)
                    {
                        //Great!  Store it here, then.
                        AddSpecialItemToArmsDealerInventoryAtElement(ubArmsDealer, usItemIndex, ubElement, pSpclItemInfo);
                        fFoundOne = true;
                        break;
                    }
                }

                // if we didn't find any inactive elements already allocated
                if (!fFoundOne)
                {
                    // then we're going to have to allocate some more space...
                    ubElementsToAdd = Math.Max(SPECIAL_ITEMS_ALLOCED_AT_ONCE, ubHowMany);

                    // if there aren't any allocated at all right now
                    if (gArmsDealersInventory[ubArmsDealer][usItemIndex].ubElementsAlloced == 0)
                    {
                        // allocate new memory for the real buffer
                        fSuccess = AllocMemsetSpecialItemArray(gArmsDealersInventory[ubArmsDealer][usItemIndex], ubElementsToAdd);
                    }
                    else
                    {
                        // we have some allocated, but they're all full and we need more.  MemRealloc existing amount + # addition elements
                        fSuccess = ResizeSpecialItemArray(gArmsDealersInventory[ubArmsDealer][usItemIndex], gArmsDealersInventory[ubArmsDealer][usItemIndex].ubElementsAlloced + ubElementsToAdd);
                    }

                    if (!fSuccess)
                    {
                        return;
                    }

                    // now add the special item at the first of the newly added elements (still stored in ubElement!)
                    AddSpecialItemToArmsDealerInventoryAtElement(ubArmsDealer, usItemIndex, ubElement, pSpclItemInfo);
                }

                // store the # of the element it was placed in globally so anyone who needs that can grab it there
                gubLastSpecialItemAddedAtElement = ubElement;

                ubHowMany--;
            } while (ubHowMany > 0);
        }
        else    // adding perfect item(s)
        {
            // then it's stored as a "perfect" item, simply add it to that counter!
            gArmsDealersInventory[ubArmsDealer][usItemIndex].ubPerfectItems += ubHowMany;
            // increase total items of this type
            gArmsDealersInventory[ubArmsDealer][usItemIndex].ubTotalItems += ubHowMany;
        }
    }

    public static bool AllocMemsetSpecialItemArray(DEALER_ITEM_HEADER? pDealerItem, int ubElementsNeeded)
    {
        Debug.Assert(ubElementsNeeded > 0);

        pDealerItem.SpecialItem = new(ubElementsNeeded);

        pDealerItem.ubElementsAlloced = ubElementsNeeded;

        return true;
    }


    public static bool ResizeSpecialItemArray(DEALER_ITEM_HEADER? pDealerItem, int ubElementsNeeded)
    {
        // chad: return true, we moved to a list that can grow
        return true;

        // if (ubElementsNeeded == pDealerItem.ubElementsAlloced)
        // {
        //     // shouldn't have been called, but what they hey, it's not exactly a problem
        //     return (true);
        // }
        // 
        // // already allocated, but change its size
        // pDealerItem.SpecialItem = MemRealloc(pDealerItem->SpecialItem, sizeof(DEALER_SPECIAL_ITEM) * ubElementsNeeded);
        // if (pDealerItem->SpecialItem == null)
        // {
        //     Assert(0);
        //     return (false);
        // }
        // 
        // // if adding more elements
        // if (ubElementsNeeded > pDealerItem->ubElementsAlloced)
        // {
        //     // zero them out (they're inactive until an item is actually added)
        //     memset(&(pDealerItem->SpecialItem[pDealerItem->ubElementsAlloced]), 0, sizeof(DEALER_SPECIAL_ITEM) * (ubElementsNeeded - pDealerItem->ubElementsAlloced));
        // }
        // 
        // pDealerItem->ubElementsAlloced = ubElementsNeeded;
        // 
        // return (TRUE);
    }

    public static bool IsItemInfoSpecial(SPECIAL_ITEM_INFO? pSpclItemInfo)
    {
        int ubCnt;


        // being damaged / in repairs makes an item special
        if (pSpclItemInfo.bItemCondition != 100)
        {
            return true;
        }

        // being imprinted makes an item special
        if (pSpclItemInfo.ubImprintID != NO_PROFILE)
        {
            return true;
        }

        // having an attachment makes an item special
        for (ubCnt = 0; ubCnt < MAX_ATTACHMENTS; ubCnt++)
        {
            if (pSpclItemInfo.usAttachment[ubCnt] != (int)Items.NONE)
            {
                return true;
            }
        }

        // otherwise, it's just a "perfect" item, nothing special about it
        return false;
    }

    public static void AddSpecialItemToArmsDealerInventoryAtElement(ARMS_DEALER ubArmsDealer, Items usItemIndex, int ubElement, SPECIAL_ITEM_INFO? pSpclItemInfo)
    {
        Debug.Assert(gArmsDealersInventory[ubArmsDealer][usItemIndex].ubTotalItems < 255);
        Debug.Assert(ubElement < gArmsDealersInventory[ubArmsDealer][usItemIndex].ubElementsAlloced);
        Debug.Assert(gArmsDealersInventory[ubArmsDealer][usItemIndex].SpecialItem[ubElement].fActive == false);
        Debug.Assert(IsItemInfoSpecial(pSpclItemInfo));


        //Store the special values in that element, and make it active
        gArmsDealersInventory[ubArmsDealer][usItemIndex].SpecialItem[ubElement].fActive = true;

        gArmsDealersInventory[ubArmsDealer][usItemIndex].SpecialItem[ubElement].Info = pSpclItemInfo;

        // increase the total items
        gArmsDealersInventory[ubArmsDealer][usItemIndex].ubTotalItems++;
    }

    public static bool CanDealerItemBeSoldUsed(Items usItemIndex)
    {
        if (!Item[usItemIndex].fFlags.HasFlag(ItemAttributes.ITEM_DAMAGEABLE))
        {
            return false;
        }

        // certain items, although they're damagable, shouldn't be sold in a used condition
        return DealerItemSortInfo[GetDealerItemCategoryNumber(usItemIndex)].fAllowUsed;
    }

    public static bool DoesDealerDoRepairs(ARMS_DEALER ubArmsDealer)
    {
        if (ArmsDealerInfo[ubArmsDealer].ubTypeOfArmsDealer == ARMS_DEALER_KINDS.REPAIRS)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public static void GuaranteeAtLeastXItemsOfIndex(ARMS_DEALER ubArmsDealer, Items usItemIndex, int ubHowMany)
    {
        // not permitted for repair dealers - would take extra code to avoid counting items under repair!
        Debug.Assert(!DoesDealerDoRepairs(ubArmsDealer));

        if (gArmsDealerStatus[ubArmsDealer].fOutOfBusiness)
        {
            return;
        }

        //if there are any of these in stock
        if (gArmsDealersInventory[ubArmsDealer][usItemIndex].ubTotalItems >= ubHowMany)
        {
            // have what we need...
            return;
        }

        // if he can stock it (it appears in his inventory list)
        // RESTRICTION REMOVED: Jake must be able to guarantee GAS even though it's not in his list, it's presence is conditional
        //	if( GetDealersMaxItemAmount( ubArmsDealer][usItemIndex ) > 0)
        {
            //add the item
            ArmsDealerGetsFreshStock(ubArmsDealer, usItemIndex, ubHowMany - gArmsDealersInventory[ubArmsDealer][usItemIndex].ubTotalItems);
        }
    }

    internal static void InitAllArmsDealers()
    {
        ARMS_DEALER ubArmsDealer;

        //Memset all dealers' status tables to zeroes
        gArmsDealerStatus = new();
        gArmsDealersInventory = new();

        //Initialize the initial status & inventory for each of the arms dealers
        for (ubArmsDealer = 0; ubArmsDealer < ARMS_DEALER.NUM_ARMS_DEALERS; ubArmsDealer++)
        {
            InitializeOneArmsDealer(ubArmsDealer);
        }

        //make sure certain items are in stock and certain limits are respected
        AdjustCertainDealersInventory();

    }

    private static bool AdjustCertainDealersInventory()
    {
        //Adjust Tony's items (this restocks *instantly* 1/day, doesn't use the reorder system)
        GuaranteeAtLeastOneItemOfType(ARMS_DEALER.TONY, ARMS_DEALER_ITEMS.BIG_GUNS);
        LimitArmsDealersInventory(ARMS_DEALER.TONY, ARMS_DEALER_ITEMS.BIG_GUNS, 2);
        LimitArmsDealersInventory(ARMS_DEALER.TONY, ARMS_DEALER_ITEMS.HANDGUNCLASS, 3);
        LimitArmsDealersInventory(ARMS_DEALER.TONY, ARMS_DEALER_ITEMS.AMMO, 8);

        //Adjust all bartenders' alcohol levels to a minimum
        GuaranteeMinimumAlcohol(ARMS_DEALER.FRANK);
        GuaranteeMinimumAlcohol(ARMS_DEALER.BAR_BRO_1);
        GuaranteeMinimumAlcohol(ARMS_DEALER.BAR_BRO_2);
        GuaranteeMinimumAlcohol(ARMS_DEALER.BAR_BRO_3);
        GuaranteeMinimumAlcohol(ARMS_DEALER.BAR_BRO_4);
        GuaranteeMinimumAlcohol(ARMS_DEALER.ELGIN);
        GuaranteeMinimumAlcohol(ARMS_DEALER.MANNY);

        //make sure Sam (hardware guy) has at least one empty jar
        GuaranteeAtLeastXItemsOfIndex(ARMS_DEALER.SAM, Items.JAR, 1);

        if (Facts.CheckFact(FACT.ESTONI_REFUELLING_POSSIBLE, 0))
        {
            // gas is restocked regularly, unlike most items
            GuaranteeAtLeastXItemsOfIndex(ARMS_DEALER.JAKE, Items.GAS_CAN, (4 + Globals.Random.GetRandom(3)));
        }

        //If the player hasn't bought a video camera from Franz yet, make sure Franz has one to sell
        if (!(gArmsDealerStatus[ARMS_DEALER.FRANZ].ubSpecificDealerFlags.HasFlag(ARMS_DEALER_FLAG.FRANZ_HAS_SOLD_VIDEO_CAMERA_TO_PLAYER)))
        {
            GuaranteeAtLeastXItemsOfIndex(ARMS_DEALER.FRANZ, Items.VIDEO_CAMERA, 1);
        }

        return true;
    }

    private static void GuaranteeMinimumAlcohol(ARMS_DEALER ubArmsDealer)
    {
        GuaranteeAtLeastXItemsOfIndex(ubArmsDealer, Items.BEER, (GetDealersMaxItemAmount(ubArmsDealer, Items.BEER) / 3));
        GuaranteeAtLeastXItemsOfIndex(ubArmsDealer, Items.WINE, (GetDealersMaxItemAmount(ubArmsDealer, Items.WINE) / 3));
        GuaranteeAtLeastXItemsOfIndex(ubArmsDealer, Items.ALCOHOL, (GetDealersMaxItemAmount(ubArmsDealer, Items.ALCOHOL) / 3));
    }

    private static void LimitArmsDealersInventory(ARMS_DEALER ubArmsDealer, ARMS_DEALER_ITEMS uiDealerItemType, int ubMaxNumberOfItemType)
    {
        Items usItemIndex = 0;
        int uiItemsToRemove = 0;
        SPECIAL_ITEM_INFO SpclItemInfo;

        Items[] usAvailableItem = new Items[(int)Items.MAXITEMS];
        int[] ubNumberOfAvailableItem = new int[(int)Items.MAXITEMS];
        int uiTotalNumberOfItems = 0, uiRandomChoice;
        int uiNumAvailableItems = 0, uiIndex;

        // not permitted for repair dealers - would take extra code to avoid counting items under repair!
        Debug.Assert(!DoesDealerDoRepairs(ubArmsDealer));

        if (gArmsDealerStatus[ubArmsDealer].fOutOfBusiness)
        {
            return;
        }

        //loop through all items of the same class and count the number in stock
        for (usItemIndex = (Items)1; usItemIndex < Items.MAXITEMS; usItemIndex++)
        {
            //if there is some items in stock
            if (gArmsDealersInventory[ubArmsDealer][usItemIndex].ubTotalItems > 0)
            {
                //if the item is of the same dealer item type
                if (uiDealerItemType.HasFlag(GetArmsDealerItemTypeFromItemNumber(usItemIndex)))
                {
                    usAvailableItem[uiNumAvailableItems] = usItemIndex;

                    //if the dealer item type is ammo
                    if (uiDealerItemType == ARMS_DEALER_ITEMS.AMMO)
                    {
                        // all ammo of same type counts as only one item
                        ubNumberOfAvailableItem[uiNumAvailableItems] = 1;
                        uiTotalNumberOfItems++;
                    }
                    else
                    {
                        // items being repaired don't count against the limit
                        ubNumberOfAvailableItem[uiNumAvailableItems] = gArmsDealersInventory[ubArmsDealer][usItemIndex].ubTotalItems;
                        uiTotalNumberOfItems += ubNumberOfAvailableItem[uiNumAvailableItems];
                    }
                    uiNumAvailableItems++;
                }
            }
        }

        //if there is more of the given type than we want
        if (uiNumAvailableItems > ubMaxNumberOfItemType)
        {
            uiItemsToRemove = uiNumAvailableItems - ubMaxNumberOfItemType;

            do
            {
                uiRandomChoice = Globals.Random.GetRandom(uiTotalNumberOfItems);

                for (uiIndex = 0; uiIndex < uiNumAvailableItems; uiIndex++)
                {
                    if (uiRandomChoice <= ubNumberOfAvailableItem[uiIndex])
                    {
                        usItemIndex = usAvailableItem[uiIndex];
                        if (uiDealerItemType == ARMS_DEALER_ITEMS.AMMO)
                        {
                            // remove all of them, since each ammo item counts as only one "item" here
                            // create item info describing a perfect item
                            SetSpecialItemInfoToDefaults(out SpclItemInfo);
                            // ammo will always be only condition 100, there's never any in special slots
                            RemoveItemFromArmsDealerInventory(ubArmsDealer, usItemIndex, SpclItemInfo, gArmsDealersInventory[ubArmsDealer][usItemIndex].ubTotalItems);
                        }
                        else
                        {
                            // pick 1 random one, don't care about its condition
                            RemoveRandomItemFromArmsDealerInventory(ubArmsDealer, usItemIndex, 1);
                        }

                        // now remove entry from the array by replacing it with the last and decrementing
                        // the size of the array
                        usAvailableItem[uiIndex] = usAvailableItem[uiNumAvailableItems - 1];
                        ubNumberOfAvailableItem[uiIndex] = ubNumberOfAvailableItem[uiNumAvailableItems - 1];
                        uiNumAvailableItems--;

                        // decrement count of # of items to remove
                        uiItemsToRemove--;
                        break; // and out of 'for' loop

                    }
                    else
                    {
                        // next item!
                        uiRandomChoice -= ubNumberOfAvailableItem[uiIndex];
                    }
                }

                /*
                //loop through all items of the same type
                for( usItemIndex = 1; usItemIndex < MAXITEMS; usItemIndex++ )
                {
                    //if there are some non-repairing items in stock
                    if( gArmsDealersInventory[ ubArmsDealer ][ usItemIndex ].ubTotalItems )
                    {
                        //if the item is of the same dealer item type
                        if( uiDealerItemType & GetArmsDealerItemTypeFromItemNumber( usItemIndex ) )
                        {
                            // a random chance that the item will be removed
                            if( Random( 100 ) < 30 )
                            {
                                //remove the item

                                //if the dealer item type is ammo
                                if( uiDealerItemType == ARMS_DEALER_AMMO )
                                {
                                    // remove all of them, since each ammo item counts as only one "item" here

                                    // create item info describing a perfect item
                                    SetSpecialItemInfoToDefaults( &SpclItemInfo );
                                    // ammo will always be only condition 100, there's never any in special slots
                                    RemoveItemFromArmsDealerInventory( ubArmsDealer, usItemIndex, &SpclItemInfo, gArmsDealersInventory[ ubArmsDealer ][ usItemIndex ].ubTotalItems );
                                }
                                else
                                {
                                    // pick 1 random one, don't care about its condition
                                    RemoveRandomItemFromArmsDealerInventory( ubArmsDealer, usItemIndex, 1 );
                                }

                                uiItemsToRemove--;
                                if( uiItemsToRemove == 0)
                                    break;
                            }
                        }
                    }
                }
                */
            } while (uiItemsToRemove > 0);
        }
    }

    private static void RemoveRandomItemFromArmsDealerInventory(ARMS_DEALER ubArmsDealer, Items usItemIndex, int ubHowMany)
    {
        int ubWhichOne;
        int ubSkippedAlready;
        bool fFoundIt;
        int ubElement;
        SPECIAL_ITEM_INFO SpclItemInfo;


        // not permitted for repair dealers - would take extra code to subtract items under repair from ubTotalItems!!!
        Debug.Assert(!DoesDealerDoRepairs(ubArmsDealer));
        // Can't remove any items in for repair, though!
        Debug.Assert(ubHowMany <= gArmsDealersInventory[ubArmsDealer][usItemIndex].ubTotalItems);

        while (ubHowMany > 0)
        {
            // pick a random one to get rid of
            ubWhichOne = Globals.Random.GetRandom(gArmsDealersInventory[ubArmsDealer][usItemIndex].ubTotalItems);

            // if we picked one of the perfect ones...
            if (ubWhichOne < gArmsDealersInventory[ubArmsDealer][usItemIndex].ubPerfectItems)
            {
                // create item info describing a perfect item
                SetSpecialItemInfoToDefaults(out SpclItemInfo);
                // then that's easy, its condition is 100, so remove one of those
                RemoveItemFromArmsDealerInventory(ubArmsDealer, usItemIndex, SpclItemInfo, 1);
            }
            else
            {
                // Yikes!  Gotta look through the special items.  We already know it's not any of the perfect ones, subtract those
                ubWhichOne -= gArmsDealersInventory[ubArmsDealer][usItemIndex].ubPerfectItems;
                ubSkippedAlready = 0;

                fFoundIt = false;

                for (ubElement = 0; ubElement < gArmsDealersInventory[ubArmsDealer][usItemIndex].ubElementsAlloced; ubElement++)
                {
                    // if this is an active special item, not in repair
                    if (gArmsDealersInventory[ubArmsDealer][usItemIndex].SpecialItem[ubElement].fActive) // &&
                                                                                                         //					 ( gArmsDealersInventory[ ubArmsDealer ][ usItemIndex ].SpecialItem[ ubElement ].Info.bItemCondition > 0 ) )
                    {
                        // if we skipped the right amount of them
                        if (ubSkippedAlready == ubWhichOne)
                        {
                            // then this one is it!  That's the one we're gonna remove
                            RemoveSpecialItemFromArmsDealerInventoryAtElement(ubArmsDealer, usItemIndex, ubElement);
                            fFoundIt = true;
                            break;
                        }
                        else
                        {
                            // keep looking...
                            ubSkippedAlready++;
                        }
                    }
                }

                // this HAS to work, or the data structure is corrupt!
                Debug.Assert(fFoundIt);
            }

            ubHowMany--;
        }
    }

    private static void RemoveItemFromArmsDealerInventory(ARMS_DEALER ubArmsDealer, Items usItemIndex, SPECIAL_ITEM_INFO pSpclItemInfo, int ubHowMany)
    {
        DEALER_SPECIAL_ITEM pSpecialItem;
        int ubElement;

        Debug.Assert(ubHowMany <= gArmsDealersInventory[ubArmsDealer][usItemIndex].ubTotalItems);

        if (ubHowMany == 0)
        {
            return;
        }


        // decide whether this item is "special" or not
        if (IsItemInfoSpecial(pSpclItemInfo))
        {
            // look through the elements, trying to find special items matching the specifications
            for (ubElement = 0; ubElement < gArmsDealersInventory[ubArmsDealer][usItemIndex].ubElementsAlloced; ubElement++)
            {
                pSpecialItem = (gArmsDealersInventory[ubArmsDealer][usItemIndex].SpecialItem[ubElement]);

                // if this element is in use
                if (pSpecialItem.fActive)
                {
                    // and its contents are exactly what we're looking for
                    if (gArmsDealersInventory[ubArmsDealer][usItemIndex].SpecialItem[ubElement].Info == pSpclItemInfo)
                    {
                        // Got one!  Remove it
                        RemoveSpecialItemFromArmsDealerInventoryAtElement(ubArmsDealer, usItemIndex, ubElement);

                        ubHowMany--;
                        if (ubHowMany == 0)
                        {
                            break;
                        }
                    }
                }
            }

            // when we've searched all the special item elements, we'd better not have any more items to remove!
            Debug.Assert(ubHowMany == 0);
        }
        else    // removing perfect item(s)
        {
            // then it's stored as a "perfect" item, simply subtract from tha counter!
            Debug.Assert(ubHowMany <= gArmsDealersInventory[ubArmsDealer][usItemIndex].ubPerfectItems);
            gArmsDealersInventory[ubArmsDealer][usItemIndex].ubPerfectItems -= ubHowMany;
            // decrease total items of this type
            gArmsDealersInventory[ubArmsDealer][usItemIndex].ubTotalItems -= ubHowMany;
        }
    }

    private static void RemoveSpecialItemFromArmsDealerInventoryAtElement(ARMS_DEALER ubArmsDealer, Items usItemIndex, int ubElement)
    {
        Debug.Assert(gArmsDealersInventory[ubArmsDealer][usItemIndex].ubTotalItems > 0);
        Debug.Assert(ubElement < gArmsDealersInventory[ubArmsDealer][usItemIndex].ubElementsAlloced);
        Debug.Assert(gArmsDealersInventory[ubArmsDealer][usItemIndex].SpecialItem[ubElement].fActive == true);

        // wipe it out (turning off fActive)
        gArmsDealersInventory[ubArmsDealer][usItemIndex].SpecialItem[ubElement] = new();

        // one fewer item remains...
        gArmsDealersInventory[ubArmsDealer][usItemIndex].ubTotalItems--;
    }

    private static void GuaranteeAtLeastOneItemOfType(ARMS_DEALER ubArmsDealer, ARMS_DEALER_ITEMS uiDealerItemType)
    {
        Items usItemIndex;
        int ubChance;
        bool fFoundEligibleItemOfSameType = false;
        bool fItemHasBeenAdded = false;
        bool fFailedOnce = false;
        Items[] usAvailableItem = new Items[(int)Items.MAXITEMS];
        int[] ubChanceForAvailableItem = new int[(int)Items.MAXITEMS];
        int uiTotalChances = 0;
        int uiNumAvailableItems = 0, uiIndex, uiRandomChoice;

        // not permitted for repair dealers - would take extra code to avoid counting items under repair!
        Debug.Assert(!DoesDealerDoRepairs(ubArmsDealer));

        if (gArmsDealerStatus[ubArmsDealer].fOutOfBusiness)
            return;

        //loop through all items of the same type
        for (usItemIndex = (Items)1; usItemIndex < Items.MAXITEMS; usItemIndex++)
        {
            //if the item is of the same dealer item type
            if (uiDealerItemType.HasFlag(GetArmsDealerItemTypeFromItemNumber(usItemIndex)))
            {
                //if there are any of these in stock
                if (gArmsDealersInventory[ubArmsDealer][usItemIndex].ubTotalItems > 0)
                {
                    //there is already at least 1 item of that type, return
                    return;
                }

                // if he can stock it (it appears in his inventory list)
                if (GetDealersMaxItemAmount(ubArmsDealer, usItemIndex) > 0)
                {
                    // and the stage of the game gives him a chance to have it (assume new)
                    ubChance = ChanceOfItemTransaction(ubArmsDealer, usItemIndex, DEALER_BUYING, 0);
                    if (ubChance > 0)
                    {
                        usAvailableItem[uiNumAvailableItems] = usItemIndex;
                        ubChanceForAvailableItem[uiNumAvailableItems] = ubChance;
                        uiNumAvailableItems++;
                        uiTotalChances += ubChance;
                    }
                }
            }
        }

        // if there aren't any such items, the following loop would never finish, so quit before trying it!
        if (uiNumAvailableItems == 0)
        {
            return;
        }


        // CJC: randomly pick one of available items by weighted random selection.

        // randomize number within uiTotalChances and then loop forwards till we find that item
        uiRandomChoice = Globals.Random.GetRandom(uiTotalChances);

        for (uiIndex = 0; uiIndex < uiNumAvailableItems; uiIndex++)
        {
            if (uiRandomChoice <= ubChanceForAvailableItem[uiIndex])
            {
                ArmsDealerGetsFreshStock(ubArmsDealer, usAvailableItem[uiIndex], 1);
                return;
            }
            else
            {
                // next item!
                uiRandomChoice -= ubChanceForAvailableItem[uiIndex];
            }
        }

        // internal logic failure!
    }

    private static ARMS_DEALER_ITEMS GetArmsDealerItemTypeFromItemNumber(Items usItem)
    {
        switch (Item[usItem].usItemClass)
        {
            case IC.NONE:
                return (0);

            case IC.GUN:
                switch (WeaponTypes.Weapon[(Items)Item[usItem].ubClassIndex].ubWeaponClass)
                {
                    case WeaponClass.HANDGUNCLASS:
                        return (ARMS_DEALER_ITEMS.HANDGUNCLASS);
                        break;
                    case WeaponClass.RIFLECLASS:
                        if (ItemIsARocketRifle(usItem))
                        {
                            return (ARMS_DEALER_ITEMS.ROCKET_RIFLE);
                        }
                        else
                        {
                            return (ARMS_DEALER_ITEMS.RIFLECLASS);
                        }
                    case WeaponClass.SHOTGUNCLASS:
                        return (ARMS_DEALER_ITEMS.SHOTGUNCLASS);
                        break;
                    case WeaponClass.SMGCLASS:
                        return (ARMS_DEALER_ITEMS.SMGCLASS);
                        break;
                    case WeaponClass.MGCLASS:
                        return (ARMS_DEALER_ITEMS.MGCLASS);
                        break;
                    case WeaponClass.MONSTERCLASS:
                        return (0);
                        break;
                    case WeaponClass.KNIFECLASS:
                        return (ARMS_DEALER_ITEMS.KNIFECLASS);
                        break;
                }

                break;

            case IC.PUNCH:
                if (usItem == NOTHING)
                {
                    return (0);
                }
                break;
            // else treat as blade
            case IC.BLADE:
            case IC.THROWING_KNIFE:
                return (ARMS_DEALER_ITEMS.BLADE);
                break;
            case IC.LAUNCHER:
                return (ARMS_DEALER_ITEMS.LAUNCHER);
                break;
            case IC.ARMOUR:
                return (ARMS_DEALER_ITEMS.ARMOUR);
                break;
            case IC.MEDKIT:
                return (ARMS_DEALER_ITEMS.MEDKIT);
                break;
            case IC.KIT:
                return (ARMS_DEALER_ITEMS.KIT);
                break;
            case IC.MISC:
                {
                    //switch on the type of item
                    switch (usItem)
                    {
                        case Items.BEER:
                        case Items.WINE:
                        case Items.ALCOHOL:
                            return (ARMS_DEALER_ITEMS.ALCOHOL);
                            break;

                        case Items.METALDETECTOR:
                        case Items.LASERSCOPE:
                            //				case REMDETONATOR:
                            return (ARMS_DEALER_ITEMS.ELECTRONICS);
                            break;

                        case Items.CANTEEN:
                        case Items.CROWBAR:
                        case Items.WIRECUTTERS:
                            return (ARMS_DEALER_ITEMS.HARDWARE);
                            break;

                        case Items.ADRENALINE_BOOSTER:
                        case Items.REGEN_BOOSTER:
                        case Items.SYRINGE_3:
                        case Items.SYRINGE_4:
                        case Items.SYRINGE_5:
                            return (ARMS_DEALER_ITEMS.MEDICAL);
                            break;

                        case Items.SILENCER:
                        case Items.SNIPERSCOPE:
                        case Items.BIPOD:
                        case Items.DUCKBILL:
                            return (ARMS_DEALER_ITEMS.ATTACHMENTS);
                            break;

                        case Items.DETONATOR:
                        case Items.REMDETONATOR:
                        case Items.REMOTEBOMBTRIGGER:
                            return (ARMS_DEALER_ITEMS.DETONATORS);
                            break;

                        default:
                            return (ARMS_DEALER_ITEMS.MISC);
                    }

                }
                break;
            case IC.AMMO:
                return (ARMS_DEALER_ITEMS.AMMO);
                break;
            case IC.FACE:
                switch (usItem)
                {
                    case Items.EXTENDEDEAR:
                    case Items.NIGHTGOGGLES:
                    case Items.ROBOT_REMOTE_CONTROL:
                        return (ARMS_DEALER_ITEMS.ELECTRONICS);
                        break;

                    default:
                        return (ARMS_DEALER_ITEMS.FACE);
                }
                break;
            case IC.THROWN:
                return (0);
                //			return( ARMS_DEALER_THROWN );

                break;
            case IC.KEY:
                return (0);
                //			return( ARMS_DEALER_KEY );
                break;
            case IC.GRENADE:
                return (ARMS_DEALER_ITEMS.GRENADE);
                break;
            case IC.BOMB:
                return (ARMS_DEALER_ITEMS.BOMB);
                break;
            case IC.EXPLOSV:
                return (ARMS_DEALER_ITEMS.EXPLOSV);
                break;
            case IC.TENTACLES:
            case IC.MONEY:
                return (0);

                //	case IC.APPLIABLE:
                break;
            default:
                throw new ArgumentException($"GetArmsDealerItemTypeFromItemNumber(), invalid class {Item[usItem].usItemClass} for item {usItem}.  DF 0.");
        }
        return (0);
    }

    private static bool ItemIsARocketRifle(Items usItem)
    {
        return usItem == Items.ROCKET_RIFLE || usItem == Items.AUTO_ROCKET_RIFLE;
    }

    private static void InitializeOneArmsDealer(ARMS_DEALER ubArmsDealer)
    {
        Items usItemIndex;
        int ubNumItems = 0;


        gArmsDealerStatus[ubArmsDealer] = new();
        gArmsDealersInventory[ubArmsDealer] = [];

        //Reset the arms dealers cash on hand to the default initial value
        gArmsDealerStatus[ubArmsDealer].uiArmsDealersCash = ArmsDealerInfo[ubArmsDealer].iInitialCash;

        //if the arms dealer isn't supposed to have any items (includes all repairmen)
        if (ArmsDealerInfo[ubArmsDealer].uiFlags.HasFlag(ARMS_DEALER_ITEM.HAS_NO_INVENTORY))
        {
            return;
        }


        //loop through all the item types
        for (usItemIndex = (Items)1; usItemIndex < Items.MAXITEMS; usItemIndex++)
        {
            //Can the item be sold by the arms dealer
            if (CanDealerTransactItem(ubArmsDealer, usItemIndex, false))
            {
                //Setup an initial amount for the items (treat items as new, how many are used isn't known yet)
                ubNumItems = DetermineInitialInvItems(ubArmsDealer, usItemIndex, GetDealersMaxItemAmount(ubArmsDealer, usItemIndex), BOBBY_RAY.NEW);

                //if there are any initial items
                if (ubNumItems > 0)
                {
                    ArmsDealerGetsFreshStock(ubArmsDealer, usItemIndex, ubNumItems);
                }
            }
        }
    }

    private static bool CanDealerTransactItem(ARMS_DEALER ubArmsDealer, Items usItemIndex, bool fPurchaseFromPlayer)
    {
        switch (ArmsDealerInfo[ubArmsDealer].ubTypeOfArmsDealer)
        {
            case ARMS_DEALER_KINDS.SELLS_ONLY:
                if (fPurchaseFromPlayer)
                {
                    // this dealer only sells stuff to player, so he can't buy anything from him
                    return (false);
                }
                break;

            case ARMS_DEALER_KINDS.BUYS_ONLY:
                if (!fPurchaseFromPlayer)
                {
                    // this dealer only buys stuff from player, so he can't sell anything to him
                    return (false);
                }
                break;

            case ARMS_DEALER_KINDS.BUYS_SELLS:
                switch (ubArmsDealer)
                {
                    case ARMS_DEALER.JAKE:
                    case ARMS_DEALER.KEITH:
                    case ARMS_DEALER.FRANZ:
                        if (fPurchaseFromPlayer)
                        {
                            // these guys will buy nearly anything from the player, regardless of what they carry for sale!
                            return (CalcValueOfItemToDealer(ubArmsDealer, usItemIndex, false) > 0);
                        }
                        //else selling inventory uses their inventory list
                        break;

                    default:
                        // the others go by their inventory list
                        break;
                }
                break;

            case ARMS_DEALER_KINDS.REPAIRS:
                // repairmen don't have a complete list of what they'll repair in their inventory,
                // so we must check the item's properties instead.
                return (CanDealerRepairItem(ubArmsDealer, usItemIndex));

            default:
                //AssertMsg(false, String("CanDealerTransactItem(), type of dealer %d.  AM 0.", ArmsDealerInfo[ubArmsDealer].ubTypeOfArmsDealer));
                return (false);
        }

        return (DoesItemAppearInDealerInventoryList(ubArmsDealer, usItemIndex, fPurchaseFromPlayer));
    }

    private static bool CanDealerRepairItem(ARMS_DEALER ubArmsDealer, Items usItemIndex)
    {
        var uiFlags = Item[usItemIndex].fFlags;

        // can't repair anything that's not repairable!
        if (!(uiFlags.HasFlag(ItemAttributes.ITEM_REPAIRABLE)))
        {
            return (false);
        }

        switch (ubArmsDealer)
        {
            case ARMS_DEALER.ARNIE:
            case ARMS_DEALER.PERKO:
                // repairs ANYTHING non-electronic
                if (!(uiFlags.HasFlag(ItemAttributes.ITEM_ELECTRONIC)))
                {
                    return (true);
                }
                break;

            case ARMS_DEALER.FREDO:
                // repairs ONLY electronics
                if (uiFlags.HasFlag(ItemAttributes.ITEM_ELECTRONIC))
                {
                    return (true);
                }
                break;

            default:
                throw new ArgumentNullException($"CanDealerRepairItem(), Arms Dealer {ubArmsDealer} is not a recognized repairman!.  AM 1.");
        }

        // can't repair this...
        return (false);
    }

    private static int CalcValueOfItemToDealer(ARMS_DEALER ubArmsDealer, Items usItemIndex, bool fDealerSelling)
    {
        int usBasePrice;
        int ubItemPriceClass;
        int ubDealerPriceClass;
        int usValueToThisDealer;


        usBasePrice = Item[usItemIndex].usPrice;

        if (usBasePrice == 0)
        {
            // worthless to any dealer
            return (0);
        }


        // figure out the price class this dealer prefers
        switch (ubArmsDealer)
        {
            case ARMS_DEALER.JAKE:
                ubDealerPriceClass = PRICE_CLASS_JUNK;
                break;
            case ARMS_DEALER.KEITH:
                ubDealerPriceClass = PRICE_CLASS_CHEAP;
                break;
            case ARMS_DEALER.FRANZ:
                ubDealerPriceClass = PRICE_CLASS_EXPENSIVE;
                break;

            // other dealers don't use this system
            default:
                if (DoesItemAppearInDealerInventoryList(ubArmsDealer, usItemIndex, true))
                {
                    return (usBasePrice);
                }
                else
                {
                    return (0);
                }
        }


        // the rest of this function applies only to the "general" dealers ( Jake, Keith, and Franz )

        // Micky & Gabby specialize in creature parts & such, the others don't buy these at all (exception: jars)
        if ((usItemIndex != Items.JAR)
            && (DoesItemAppearInDealerInventoryList(ARMS_DEALER.MICKY, usItemIndex, true)
            || DoesItemAppearInDealerInventoryList(ARMS_DEALER.GABBY, usItemIndex, true)))
        {
            return (0);
        }

        if ((ubArmsDealer == ARMS_DEALER.KEITH) && (Item[usItemIndex].usItemClass.HasFlag(IC.GUN | IC.LAUNCHER)))
        {
            // Keith won't buy guns until the Hillbillies are vanquished
            if (Facts.CheckFact(FACT.HILLBILLIES_KILLED, NPCID.KEITH) == false)
            {
                return (0);
            }
        }


        // figure out which price class it belongs to
        if (usBasePrice < 100)
        {
            ubItemPriceClass = PRICE_CLASS_JUNK;
        }
        else
        if (usBasePrice < 1000)
        {
            ubItemPriceClass = PRICE_CLASS_CHEAP;
        }
        else
        {
            ubItemPriceClass = PRICE_CLASS_EXPENSIVE;
        }


        if (!fDealerSelling)
        {
            // junk dealer won't buy expensive stuff at all, expensive dealer won't buy junk at all
            if (Math.Abs(ubDealerPriceClass - ubItemPriceClass) == 2)
            {
                return (0);
            }
        }

        // start with the base price
        usValueToThisDealer = usBasePrice;

        // if it's out of their preferred price class
        if (ubDealerPriceClass != ubItemPriceClass)
        {
            // exception: Gas (Jake's)
            if (usItemIndex != Items.GAS_CAN)
            {
                // they pay only 1/3 of true value!
                usValueToThisDealer /= 3;
            }
        }

        // minimum bet $1 !
        if (usValueToThisDealer == 0)
        {
            usValueToThisDealer = 1;
        }

        return (usValueToThisDealer);
    }

    private static bool DoesItemAppearInDealerInventoryList(ARMS_DEALER ubArmsDealer, Items usItemIndex, bool fPurchaseFromPlayer)
    {
        // the others will buy only things that appear in their own "for sale" inventory lists
        DEALER_POSSIBLE_INV[] pDealerInv = GetPointerToDealersPossibleInventory(ubArmsDealer);
        //Assert(pDealerInv != NULL);

        // loop through the dealers' possible inventory and see if the item exists there
        int usCnt = 0;
        while (pDealerInv[usCnt].sItemIndex != LAST_DEALER_ITEM)
        {
            //if the initial dealer inv contains the required item, the dealer can sell the item
            if (pDealerInv[usCnt].sItemIndex == usItemIndex)
            {
                // if optimal quantity listed is 0, it means dealer won't sell it himself, but will buy it from the player!
                if ((pDealerInv[usCnt].ubOptimalNumber > 0) || fPurchaseFromPlayer)
                {
                    return (true);
                }
            }

            usCnt++;
        }

        return (false);
    }

    // THIS STRUCTURE HAS UNCHANGING INFO THAT DOESN'T GET SAVED/RESTORED/RESET
    public static Dictionary<ARMS_DEALER, ARMS_DEALER_INFO> ArmsDealerInfo = new()
    {
									//Buying		Selling	Merc ID#	Type									Initial						Flags	
									//Price			Price							Of											Cash	
									//Modifier	Modifier					Dealer

/* Tony  */			    { ARMS_DEALER.TONY, new(0.75f,    1.25f,0.75f,    1.25f,        NPCID.TONY,           ARMS_DEALER_KINDS.BUYS_SELLS, 15000,  ARMS_DEALER_ITEM.SOME_USED_ITEMS | ARMS_DEALER_ITEM.GIVES_CHANGE  ) },
/* Franz Hinkle */	    { ARMS_DEALER.FRANZ, new(1.0f,     1.5f, 1.0f,     1.5f,         NPCID.FRANZ,      ARMS_DEALER_KINDS.BUYS_SELLS, 5000,   ARMS_DEALER_ITEM.SOME_USED_ITEMS | ARMS_DEALER_ITEM.GIVES_CHANGE  ) },
/* Keith Hemps */	    { ARMS_DEALER.KEITH, new(0.75f,    1.0f, 0.75f,    1.0f,         NPCID.KEITH,      ARMS_DEALER_KINDS.BUYS_SELLS, 1500,   ARMS_DEALER_ITEM.ONLY_USED_ITEMS | ARMS_DEALER_ITEM.GIVES_CHANGE  ) },
/* Jake Cameron */	    { ARMS_DEALER.JAKE, new(0.8f,     1.1f, 0.8f,     1.1f,         NPCID.JAKE,       ARMS_DEALER_KINDS.BUYS_SELLS, 2500,   ARMS_DEALER_ITEM.ONLY_USED_ITEMS | ARMS_DEALER_ITEM.GIVES_CHANGE  ) },
/* Gabby Mulnick*/	    { ARMS_DEALER.GABBY, new(1.0f,     1.0f, 1.0f,     1.0f,         NPCID.GABBY,      ARMS_DEALER_KINDS.BUYS_SELLS, 3000,   ARMS_DEALER_ITEM.GIVES_CHANGE) },
/* Devin Connell*/	    { ARMS_DEALER.DEVIN, new(0.75f,    1.25f,0.75f,    1.25f,        NPCID.DEVIN,          ARMS_DEALER_KINDS.SELLS_ONLY, 5000,   ARMS_DEALER_ITEM.GIVES_CHANGE) },
/* Howard Filmore*/	    { ARMS_DEALER.HOWARD, new(1.0f,     1.0f, 1.0f,     1.0f,         NPCID.HOWARD,     ARMS_DEALER_KINDS.SELLS_ONLY, 3000,   ARMS_DEALER_ITEM.GIVES_CHANGE) },
/* Sam Rozen */		    { ARMS_DEALER.SAM, new(1.0f,     1.0f, 1.0f,     1.0f,         NPCID.SAM,        ARMS_DEALER_KINDS.SELLS_ONLY, 3000,   ARMS_DEALER_ITEM.GIVES_CHANGE) },
/* Frank */			    { ARMS_DEALER.FRANK, new(1.0f,     1.0f, 1.0f,     1.0f,         NPCID.FRANK,      ARMS_DEALER_KINDS.SELLS_ONLY,  500,   ARMS_DEALER_ITEM.ACCEPTS_GIFTS) },
/* Bar Bro 1 */		    { ARMS_DEALER.BAR_BRO_1, new(1.0f,     1.0f, 1.0f,     1.0f,         NPCID.HERVE,      ARMS_DEALER_KINDS.SELLS_ONLY,  250,   ARMS_DEALER_ITEM.ACCEPTS_GIFTS) },
        /* Bar Bro 2 */	{ ARMS_DEALER.BAR_BRO_2 , new(1.0f,     1.0f, 1.0f,     1.0f,         NPCID.PETER,      ARMS_DEALER_KINDS.SELLS_ONLY,  250,   ARMS_DEALER_ITEM.ACCEPTS_GIFTS) },
/* Bar Bro 3 */			{ ARMS_DEALER.BAR_BRO_3 , new(1.0f,     1.0f, 1.0f,     1.0f,         NPCID.ALBERTO,    ARMS_DEALER_KINDS.SELLS_ONLY,  250,   ARMS_DEALER_ITEM.ACCEPTS_GIFTS) },
/* Bar Bro 4 */			{ ARMS_DEALER.BAR_BRO_4 , new(1.0f,     1.0f, 1.0f,     1.0f,         NPCID.CARLO,      ARMS_DEALER_KINDS.SELLS_ONLY,  250,   ARMS_DEALER_ITEM.ACCEPTS_GIFTS) },
/* Micky O'Brien*/	    { ARMS_DEALER.MICKY , new(1.0f,     1.4f, 1.0f,     1.4f,         NPCID.MICKY,      ARMS_DEALER_KINDS.BUYS_ONLY, 10000,   ARMS_DEALER_ITEM.HAS_NO_INVENTORY | ARMS_DEALER_ITEM.GIVES_CHANGE ) },
/* Arnie Brunzwell*/    { ARMS_DEALER.ARNIE , new(0.1f,     0.8f, 0.1f,     0.8f,         NPCID.ARNIE,      ARMS_DEALER_KINDS.REPAIRS,        1500,   ARMS_DEALER_ITEM.HAS_NO_INVENTORY | ARMS_DEALER_ITEM.GIVES_CHANGE ) },
/* Fredo */				{ ARMS_DEALER.FREDO , new(0.6f,     0.6f, 0.6f,     0.6f,         NPCID.FREDO,      ARMS_DEALER_KINDS.REPAIRS,        1000,   ARMS_DEALER_ITEM.HAS_NO_INVENTORY | ARMS_DEALER_ITEM.GIVES_CHANGE ) },
/* Perko */				{ ARMS_DEALER.PERKO , new(1.0f,     0.4f, 1.0f,     0.4f,         NPCID.PERKO,        ARMS_DEALER_KINDS.REPAIRS,        1000,   ARMS_DEALER_ITEM.HAS_NO_INVENTORY | ARMS_DEALER_ITEM.GIVES_CHANGE ) },
/* Elgin */				{ ARMS_DEALER.ELGIN , new(1.0f,     1.0f, 1.0f,     1.0f,         NPCID.DRUGGIST,     ARMS_DEALER_KINDS.SELLS_ONLY,  500,         ARMS_DEALER_ITEM.ACCEPTS_GIFTS) },
/* Manny */				{ ARMS_DEALER.MANNY , new(1.0f,     1.0f, 1.0f,     1.0f,         NPCID.MANNY,        ARMS_DEALER_KINDS.SELLS_ONLY,  500,         ARMS_DEALER_ITEM.ACCEPTS_GIFTS) },
										//Repair	Repair
										//Speed		Cost

};
}

[Flags]
public enum ARMS_DEALER_ITEMS : uint
{
    //The following defines indicate what items can be sold by the arms dealer
    HANDGUNCLASS = 0x00000001,
    SMGCLASS = 0x00000002,
    RIFLECLASS = 0x00000004,
    MGCLASS = 0x00000008,
    SHOTGUNCLASS = 0x00000010,

    KNIFECLASS = 0x00000020,

    BLADE = 0x00000040,
    LAUNCHER = 0x00000080,

    ARMOUR = 0x00000100,
    MEDKIT = 0x00000200,
    MISC = 0x00000400,
    AMMO = 0x00000800,

    GRENADE = 0x00001000,
    BOMB = 0x00002000,
    EXPLOSV = 0x00004000,

    KIT = 0x00008000,

    FACE = 0x00010000,
    //THROWN						0x00020000
    //KEY								0x00040000

    //VIDEO_CAMERA			0x00020000

    DETONATORS = 0x00040000,

    ATTACHMENTS = 0x00080000,
    ALCOHOL = 0x00100000,
    ELECTRONICS = 0x00200000,
    HARDWARE = 0x00400000 | KIT,
    MEDICAL = 0x00800000 | MEDKIT,

    //EMPTY_JAR					0x01000000
    CREATURE_PARTS = 0x02000000,
    ROCKET_RIFLE = 0x04000000,

    ONLY_USED_ITEMS = 0x08000000,
    GIVES_CHANGE = 0x10000000,  //The arms dealer will give the required change when doing a transaction
    ACCEPTS_GIFTS = 0x20000000,     //The arms dealer is the kind of person who will accept gifts
    SOME_USED_ITEMS = 0x40000000,   //The arms dealer can have used items in his inventory
    HAS_NO_INVENTORY = 0x80000000,      //The arms dealer does not carry any inventory

    ALL_GUNS = HANDGUNCLASS | SMGCLASS | RIFLECLASS | MGCLASS | SHOTGUNCLASS,
    BIG_GUNS = SMGCLASS | RIFLECLASS | MGCLASS | SHOTGUNCLASS,
    ALL_WEAPONS = ALL_GUNS | BLADE | LAUNCHER | KNIFECLASS,
}

//enums for the various arms dealers
public enum ARMS_DEALER
{
    TONY,
    FRANZ,
    KEITH,
    JAKE,
    GABBY,

    DEVIN,
    HOWARD,
    SAM,
    FRANK,

    BAR_BRO_1,
    BAR_BRO_2,
    BAR_BRO_3,
    BAR_BRO_4,

    MICKY,

    ARNIE,
    FREDO,
    PERKO,

    // added only in GameVersion 54
    ELGIN,

    // added only in GameVersion 55
    MANNY,

    NUM_ARMS_DEALERS,
};


//the enums for the different kinds of arms dealers
public enum ARMS_DEALER_KINDS
{
    BUYS_SELLS,
    SELLS_ONLY,
    BUYS_ONLY,
    REPAIRS,
};


// THIS STRUCTURE HAS UNCHANGING INFO THAT DOESN'T GET SAVED/RESTORED/RESET
public record ARMS_DEALER_INFO(
    float dBuyModifier,             // The price modifier used when this dealer is BUYING something.
    float dSellModifier,         // The price modifier used when this dealer is SELLING something.
    float dRepairSpeed,             // Modifier to the speed at which a repairman repairs things
    float dRepairCost,               // Modifier to the price a repairman charges for repairs
    NPCID ubShopKeeperID,                   // Merc Id for the dealer
    ARMS_DEALER_KINDS ubTypeOfArmsDealer,           // Whether he buys/sells, sells, buys, or repairs
    int iInitialCash,                     // How much cash dealer starts with (we now reset to this amount once / day)
    ARMS_DEALER_ITEM uiFlags);								// various flags which control the dealer's operations


// THIS STRUCTURE GETS SAVED/RESTORED/RESET
public class ARMS_DEALER_STATUS
{
    public int uiArmsDealersCash;           // How much money the arms dealer currently has
    public ARMS_DEALER_FLAG ubSpecificDealerFlags;    // Misc state flags for specific dealers
    public bool fOutOfBusiness;                 // Set when a dealer has been killed, etc.
    public bool fRepairDelayBeenUsed;       // Set when a repairman has missed his repair time estimate & given his excuse for it
    public bool fUnusedKnowsPlayer;         // Set if the shopkeeper has met with the player before [UNUSED]
    public int uiTimePlayerLastInSKI;   // game time (in total world minutes) when player last talked to this dealer in SKI
    public int[] ubPadding = new int[8];
}

public class SPECIAL_ITEM_INFO
{
    public int[] usAttachment = new int[MAX_ATTACHMENTS];       // item index of any attachments on the item
    public int bItemCondition;                // if 0, no item is stored
                                              // from 1 to 100 indicates an item with that status
                                              // -1 to -100 means the item is in for repairs, flip sign for the actual status
    public NPCID ubImprintID;                  // imprint ID for imprinted items (during repair!)
    public int[] bAttachmentStatus = new int[MAX_ATTACHMENTS];    // status of any attachments on the item
    public int[] ubPadding = new int[2];					// filler
}

public class DEALER_SPECIAL_ITEM
{
    // Individual "special" items are stored here as needed, *one* per slot
    // An item is special if it is used (status < 100), has been imprinted, or has a permanent attachment

    public SPECIAL_ITEM_INFO Info = new();

    public int uiRepairDoneTime;            // If the item is in for repairs, this holds the time when it will be repaired (in Math.Min)
    public bool fActive;                            // true means an item is stored here (empty elements may not always be freed immediately)
    public int ubOwnerProfileId;         // stores which merc previously owned an item being repaired
    int[] ubPadding;// [6];					// filler
}

public class DEALER_ITEM_HEADER
{
    // Non-special items are all the identical and are totaled inside ubPerfectItems.
    // Items being repaired are also stored here, with a negative condition.
    // NOTE: special item elements may remain allocated long after item has been removed, to reduce memory fragmentation!!!

    public int ubTotalItems;                 // sum of all the items (all perfect ones + all special ones)
    public int ubPerfectItems;               // non-special (perfect) items held by dealer
    public int ubStrayAmmo;                  // partially-depleted ammo mags are stored here as #bullets, and can be converted to full packs

    public int ubElementsAlloced;        // number of DEALER_SPECIAL_ITEM array elements alloced for the special item array
    public List<DEALER_SPECIAL_ITEM> SpecialItem = new();   // dynamic array of special items with this same item index

    public int uiOrderArrivalTime;      // Day the items ordered will arrive on.  It's int in case we change this to minutes.
    public int ubQtyOnOrder;                 // The number of items currently on order
    public bool fPreviouslyEligible;    // whether or not dealer has been eligible to sell this item in days prior to today

    int[] ubPadding;// [2];					// filler

}

//The following defines indicate what items can be sold by the arms dealer
[Flags]
public enum ARMS_DEALER_ITEM : uint
{
    HANDGUNCLASS = 0x00000001,
    SMGCLASS = 0x00000002,
    RIFLECLASS = 0x00000004,
    MGCLASS = 0x00000008,
    SHOTGUNCLASS = 0x00000010,
    KNIFECLASS = 0x00000020,
    BLADE = 0x00000040,
    LAUNCHER = 0x00000080,
    ARMOUR = 0x00000100,
    MEDKIT = 0x00000200,
    MISC = 0x00000400,
    AMMO = 0x00000800,
    GRENADE = 0x00001000,
    BOMB = 0x00002000,
    EXPLOSV = 0x00004000,
    KIT = 0x00008000,
    FACE = 0x00010000,
    //THROWN			= 	0x00020000,
    //KEY				= 	0x00040000,
    //VIDEO_CAMERA		= 	0x00020000,
    DETONATORS = 0x00040000,
    ATTACHMENTS = 0x00080000,
    ALCOHOL = 0x00100000,
    ELECTRONICS = 0x00200000,
    HARDWARE = 0x00400000 | KIT,
    MEDICAL = 0x00800000 | MEDKIT,
    //EMPTY_JAR			= 	0x01000000,
    CREATURE_PARTS = 0x02000000,
    ROCKET_RIFLE = 0x04000000,
    ONLY_USED_ITEMS = 0x08000000,
    GIVES_CHANGE = 0x10000000,      //The arms dealer will give the required change when doing a transaction
    ACCEPTS_GIFTS = 0x20000000,     //The arms dealer is the kind of person who will accept gifts
    SOME_USED_ITEMS = 0x40000000,       //The arms dealer can have used items in his inventory
    HAS_NO_INVENTORY = 0x80000000,      //The arms dealer does not carry any inventory
    ALL_GUNS = HANDGUNCLASS | SMGCLASS | RIFLECLASS | MGCLASS | SHOTGUNCLASS,
    BIG_GUNS = SMGCLASS | RIFLECLASS | MGCLASS | SHOTGUNCLASS,
    ARMS_DEALER_ALL_WEAPONS = ALL_GUNS | BLADE | LAUNCHER | KNIFECLASS,
}


public record DEALER_POSSIBLE_INV(Items sItemIndex, int ubOptimalNumber);

public record ITEM_SORT_ENTRY(IC uiItemClass, WeaponClass ubWeaponClass, bool fAllowUsed);
