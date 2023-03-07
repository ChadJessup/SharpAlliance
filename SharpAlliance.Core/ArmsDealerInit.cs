using System;
using System.Collections.Generic;
using System.Diagnostics;
using SharpAlliance.Core.SubSystems;
using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core;

public class ArmsDealerInit
{
    int GetDealersMaxItemAmount(ARMS_DEALER ubDealerID, Items usItemIndex)
    {
        switch (ubDealerID)
        {
            case ARMS_DEALER.TONY:
                return (GetMaxItemAmount(gTonyInventory, usItemIndex));
                break;

            case ARMS_DEALER.FRANK:
                return (GetMaxItemAmount(gFrankInventory, usItemIndex));
                break;

            case ARMS_DEALER.MICKY:
                return (GetMaxItemAmount(gMickyInventory, usItemIndex));
                break;

            case ARMS_DEALER.ARNIE:
                return (GetMaxItemAmount(gArnieInventory, usItemIndex));
                break;

            case ARMS_DEALER.PERKO:
                return (GetMaxItemAmount(gPerkoInventory, usItemIndex));
                break;

            case ARMS_DEALER.KEITH:
                return (GetMaxItemAmount(gKeithInventory, usItemIndex));
                break;

            case ARMS_DEALER.BAR_BRO_1:
                return (GetMaxItemAmount(gHerveInventory, usItemIndex));
                break;

            case ARMS_DEALER.BAR_BRO_2:
                return (GetMaxItemAmount(gPeterInventory, usItemIndex));
                break;

            case ARMS_DEALER.BAR_BRO_3:
                return (GetMaxItemAmount(gAlbertoInventory, usItemIndex));
                break;

            case ARMS_DEALER.BAR_BRO_4:
                return (GetMaxItemAmount(gCarloInventory, usItemIndex));
                break;

            case ARMS_DEALER.JAKE:
                return (GetMaxItemAmount(gJakeInventory, usItemIndex));
                break;

            case ARMS_DEALER.FRANZ:
                return (GetMaxItemAmount(gFranzInventory, usItemIndex));
                break;

            case ARMS_DEALER.HOWARD:
                return (GetMaxItemAmount(gHowardInventory, usItemIndex));
                break;

            case ARMS_DEALER.SAM:
                return (GetMaxItemAmount(gSamInventory, usItemIndex));
                break;

            case ARMS_DEALER.FREDO:
                return (GetMaxItemAmount(gFredoInventory, usItemIndex));
                break;

            case ARMS_DEALER.GABBY:
                return (GetMaxItemAmount(gGabbyInventory, usItemIndex));
                break;

            case ARMS_DEALER.DEVIN:
                return (GetMaxItemAmount(gDevinInventory, usItemIndex));
                break;

            case ARMS_DEALER.ELGIN:
                return (GetMaxItemAmount(gElginInventory, usItemIndex));
                break;

            case ARMS_DEALER.MANNY:
                return (GetMaxItemAmount(gMannyInventory, usItemIndex));
                break;

            default:
                Debug.Assert(false);
                return (0);
                break;
        }
    }


    int GetMaxItemAmount(DEALER_POSSIBLE_INV[] pInv, Items usItemIndex)
    {
        int usCnt = 0;

        //loop through the array until a the LAST_DEALER_ITEM is hit
        while (pInv[usCnt].sItemIndex != LAST_DEALER_ITEM)
        {
            //if this item is the one we want
            if (pInv[usCnt].sItemIndex == usItemIndex)
            {
                return (pInv[usCnt].ubOptimalNumber);
            }

            // move to the next item
            usCnt++;
        }

        return (NO_DEALER_ITEM);
    }


    DEALER_POSSIBLE_INV[] GetPointerToDealersPossibleInventory(ARMS_DEALER ubArmsDealerID)
    {
        switch (ubArmsDealerID)
        {
            case ARMS_DEALER.TONY:
                return (gTonyInventory);
                break;

            case ARMS_DEALER.FRANK:
                return (gFrankInventory);
                break;

            case ARMS_DEALER.MICKY:
                return (gMickyInventory);
                break;

            case ARMS_DEALER.ARNIE:
                return (gArnieInventory);
                break;

            case ARMS_DEALER.PERKO:
                return (gPerkoInventory);
                break;

            case ARMS_DEALER.KEITH:
                return (gKeithInventory);
                break;

            case ARMS_DEALER.BAR_BRO_1:
                return (gHerveInventory);
                break;

            case ARMS_DEALER.BAR_BRO_2:
                return (gPeterInventory);
                break;

            case ARMS_DEALER.BAR_BRO_3:
                return (gAlbertoInventory);
                break;

            case ARMS_DEALER.BAR_BRO_4:
                return (gCarloInventory);
                break;

            case ARMS_DEALER.JAKE:
                return (gJakeInventory);
                break;

            case ARMS_DEALER.FRANZ:
                return (gFranzInventory);
                break;

            case ARMS_DEALER.HOWARD:
                return (gHowardInventory);
                break;

            case ARMS_DEALER.SAM:
                return (gSamInventory);
                break;

            case ARMS_DEALER.FREDO:
                return (gFredoInventory);
                break;

            case ARMS_DEALER.GABBY:
                return (gGabbyInventory);
                break;

            case ARMS_DEALER.DEVIN:
                return (gDevinInventory);
                break;

            case ARMS_DEALER.ELGIN:
                return (gElginInventory);
                break;

            case ARMS_DEALER.MANNY:
                return (gMannyInventory);
                break;

            default:
                return (null);
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
            return (ITEM_SUITABILITY_NONE);
        }

        // items normally not sold at shops are unsuitable
        if (Item[usItemIndex].fFlags.HasFlag(ItemAttributes.ITEM_NOT_BUYABLE))
        {
            return (ITEM_SUITABILITY_NONE);
        }


        ubItemCoolness = Item[usItemIndex].ubCoolness;

        if (ubItemCoolness == 0)
        {
            // items without a coolness rating can't be sold to the player by shopkeepers
            return (ITEM_SUITABILITY_NONE);
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
                return (ITEM_SUITABILITY_ALWAYS);
        }


        // If it's not BobbyRay, Tony, or Devin
        if ((bArmsDealer != (ARMS_DEALER)(-1)) && (bArmsDealer != ARMS_DEALER.TONY) && (bArmsDealer != ARMS_DEALER.DEVIN))
        {
            // all the other dealers have very limited inventories, so their suitability remains constant at all times in game
            return (ITEM_SUITABILITY_HIGH);
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
            return (ITEM_SUITABILITY_NONE);
        }

        // if item is exactly within the current coolness window
        if ((ubItemCoolness >= ubMinCoolness) && (ubItemCoolness <= ubMaxCoolness))
        {
            return (ITEM_SUITABILITY_HIGH);
        }

        // if item is still relatively close to low end of the window
        if ((ubItemCoolness + 2) >= ubMinCoolness)
        {
            return (ITEM_SUITABILITY_MEDIUM);
        }

        // item is way uncool for player's current progress, but it's still possible for it to make an appearance
        return (ITEM_SUITABILITY_LOW);
    }

    public static int ChanceOfItemTransaction(ARMS_DEALER bArmsDealer, Items usItemIndex, int fDealerIsSelling, BOBBY_RAY fUsed)
    {
        int ubItemCoolness;
        int ubChance = 0;
        bool fBobbyRay = false;


        // make sure dealers don't carry used items that they shouldn't
        if (fUsed > 0 && fDealerIsSelling == 0 && !CanDealerItemBeSoldUsed(usItemIndex))
        {
            return (0);
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
                ubChance = (fBobbyRay) ? 25 : 15;
                break;

            case ITEM_SUITABILITY_MEDIUM:
                ubChance = (fBobbyRay) ? 50 : 30;
                break;

            case ITEM_SUITABILITY_HIGH:
                ubChance = (fBobbyRay) ? 75 : 50;
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
                ubChance += (5 * ubItemCoolness);

                // ARM: New - keep stuff on the shelves longer
                ubChance /= 2;
            }

            // used items are traded more rarely
            if (fUsed > 0)
            {
                ubChance /= 2;
            }
        }


        return (ubChance);
    }

    public static bool ItemTransactionOccurs(ARMS_DEALER bArmsDealer, Items usItemIndex, int fDealerIsSelling, BOBBY_RAY fUsed)
    {
        int ubChance;
        int sInventorySlot;


        ubChance = ChanceOfItemTransaction(bArmsDealer, usItemIndex, fDealerIsSelling, fUsed);

        // if the dealer is buying, and a chance exists (i.e. the item is "eligible")
        if (fDealerIsSelling == 0&& (ubChance > 0))
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
                gArmsDealersInventory[(int)bArmsDealer, (int)usItemIndex].fPreviouslyEligible = true;
            }
        }

        // roll to see if a transaction occurs
        if (Globals.Random.Next(100) < ubChance)
        {
            return (true);
        }
        else
        {
            return (false);
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

        return (ubNumBought);
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

        return (ubNumSold);
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

        return (ubNumReordered);
    }



    int BobbyRayItemQsortCompare(STORE_INVENTORY pArg1, STORE_INVENTORY pArg2)
    {
        Items usItem1Index;
        Items usItem2Index;
        int ubItem1Quality;
        int ubItem2Quality;

        usItem1Index = (pArg1).usItemIndex;
        usItem2Index = (pArg2).usItemIndex;

        ubItem1Quality = (pArg1).ubItemQuality;
        ubItem2Quality = (pArg2).ubItemQuality;

        return (CompareItemsForSorting(usItem1Index, usItem2Index, ubItem1Quality, ubItem2Quality));
    }



    int ArmsDealerItemQsortCompare(INVENTORY_IN_SLOT pArg1, INVENTORY_IN_SLOT pArg2)
    {
        Items usItem1Index;
        Items usItem2Index;
        int ubItem1Quality;
        int ubItem2Quality;

        usItem1Index = (pArg1).sItemIndex;
        usItem2Index = (pArg2).sItemIndex;

        ubItem1Quality = (pArg1).ItemObject.bStatus[0];
        ubItem2Quality = (pArg2).ItemObject.bStatus[0];

        return (CompareItemsForSorting(usItem1Index, usItem2Index, ubItem1Quality, ubItem2Quality));
    }



    int RepairmanItemQsortCompare(INVENTORY_IN_SLOT pArg1, INVENTORY_IN_SLOT pArg2)
    {
        INVENTORY_IN_SLOT? pInvSlot1;
        INVENTORY_IN_SLOT? pInvSlot2;
        int uiRepairTime1;
        int uiRepairTime2;


        pInvSlot1 = pArg1;
        pInvSlot2 = pArg2;

        Debug.Assert(pInvSlot1.sSpecialItemElement != -1);
        Debug.Assert(pInvSlot2.sSpecialItemElement != -1);

        uiRepairTime1 = gArmsDealersInventory[gbSelectedArmsDealerID, (int)pInvSlot1.sItemIndex].SpecialItem[pInvSlot1.sSpecialItemElement].uiRepairDoneTime;
        uiRepairTime2 = gArmsDealersInventory[gbSelectedArmsDealerID, (int)pInvSlot2.sItemIndex].SpecialItem[pInvSlot2.sSpecialItemElement].uiRepairDoneTime;


        // lower reapir time first
        if (uiRepairTime1 < uiRepairTime2)
        {
            return (-1);
        }
        else
        if (uiRepairTime1 > uiRepairTime2)
        {
            return (1);
        }
        else
        {
            return (0);
        }
    }



    int CompareItemsForSorting(Items usItem1Index, Items usItem2Index, int ubItem1Quality, int ubItem2Quality)
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
            return (-1);
        }
        else
        if (ubItem1Category > ubItem2Category)
        {
            return (1);
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
                    return (-1);
                }
                else
                if (ubItem1Calibre < ubItem2Calibre)
                {
                    return (1);
                }
                // the same caliber - compare size of magazine, then fall out of if statement
                ubItem1MagSize = WeaponTypes.Magazine[Item[usItem1Index].ubClassIndex].ubMagSize;
                ubItem2MagSize = WeaponTypes.Magazine[Item[usItem2Index].ubClassIndex].ubMagSize;
                if (ubItem1MagSize > ubItem2MagSize)
                {
                    return (-1);
                }
                else
                if (ubItem1MagSize < ubItem2MagSize)
                {
                    return (1);
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
                    return (-1);
                }
                else
                if (ubItem1Coolness < ubItem2Coolness)
                {
                    return (1);
                }
            }

            // the same coolness/caliber - compare base prices then
            usItem1Price = Item[usItem1Index].usPrice;
            usItem2Price = Item[usItem2Index].usPrice;

            // higher price first
            if (usItem1Price > usItem2Price)
            {
                return (-1);
            }
            else
            if (usItem1Price < usItem2Price)
            {
                return (1);
            }
            else
            {
                // the same price - compare item #s, then

                // lower index first
                if (usItem1Index < usItem2Index)
                {
                    return (-1);
                }
                else
                if (usItem1Index > usItem2Index)
                {
                    return (1);
                }
                else
                {
                    // same item type = compare item quality, then

                    // higher quality first
                    if (ubItem1Quality > ubItem2Quality)
                    {
                        return (-1);
                    }
                    else
                    if (ubItem1Quality < ubItem2Quality)
                    {
                        return (1);
                    }
                    else
                    {
                        // identical items!
                        return (0);
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
            ubWeaponClass = WeaponTypes.Weapon[(int)usItemIndex].ubWeaponClass;
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
                    return (ubCategory);
                }
                else
                {
                    // for guns, must also match on weapon class
                    if (DealerItemSortInfo[ubCategory].ubWeaponClass == ubWeaponClass)
                    {
                        // then we're found it
                        return (ubCategory);
                    }
                }
            }

            // check vs. next category in the list
            ubCategory++;
        }

        // should never be trying to locate an item that's not covered in the table!
        Debug.Assert(false);
        return (0);
    }



    public static bool CanDealerItemBeSoldUsed(Items usItemIndex)
    {
        if (!(Item[usItemIndex].fFlags.HasFlag(ItemAttributes.ITEM_DAMAGEABLE)))
        {
            return (false);
        }

        // certain items, although they're damagable, shouldn't be sold in a used condition
        return (DealerItemSortInfo[GetDealerItemCategoryNumber(usItemIndex)].fAllowUsed);
    }
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
public struct ARMS_DEALER_INFO
{
    public float dBuyModifier;             // The price modifier used when this dealer is BUYING something.
    public float dSellModifier;         // The price modifier used when this dealer is SELLING something.
    public float dRepairSpeed;             // Modifier to the speed at which a repairman repairs things
    public float dRepairCost;               // Modifier to the price a repairman charges for repairs
    public int ubShopKeeperID;                   // Merc Id for the dealer
    public int ubTypeOfArmsDealer;           // Whether he buys/sells, sells, buys, or repairs
    public int iInitialCash;                     // How much cash dealer starts with (we now reset to this amount once / day)
    public int uiFlags;								// various flags which control the dealer's operations
}


// THIS STRUCTURE GETS SAVED/RESTORED/RESET
public class ARMS_DEALER_STATUS
{
    public int uiArmsDealersCash;           // How much money the arms dealer currently has
    public int ubSpecificDealerFlags;    // Misc state flags for specific dealers
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
    public int ubImprintID;                  // imprint ID for imprinted items (during repair!)
    public int[] bAttachmentStatus = new int[MAX_ATTACHMENTS];    // status of any attachments on the item
    public int[] ubPadding = new int[2];					// filler
}

public struct DEALER_SPECIAL_ITEM
{
    // Individual "special" items are stored here as needed, *one* per slot
    // An item is special if it is used (status < 100), has been imprinted, or has a permanent attachment

    public SPECIAL_ITEM_INFO Info;

    public int uiRepairDoneTime;            // If the item is in for repairs, this holds the time when it will be repaired (in Math.Min)
    public bool fActive;                            // true means an item is stored here (empty elements may not always be freed immediately)
    public int ubOwnerProfileId;         // stores which merc previously owned an item being repaired
    public int[] ubPadding;// [6];					// filler
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
public enum ARMS_DEALDER_ITEM : uint
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
