
using System;
using System.Collections.Generic;
using System.Diagnostics;
using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core.SubSystems;

public class LaptopSave
{
    // SetupStoreInventory sets up the initial quantity on hand for all of Bobby Ray's inventory items
    public static void SetupStoreInventory(List<STORE_INVENTORY> pInventoryArray, BOBBY_RAY fUsed)
    {
        int i;
        Items usItemIndex;
        int ubNumBought;

        //loop through all items BR can stock to init a starting quantity on hand 
        for (i = 0; i < LaptopSaveInfo.usInventoryListLength[fUsed]; i++)
        {
            usItemIndex = pInventoryArray[i].usItemIndex;

            ubNumBought = ArmsDealerInit.DetermineInitialInvItems((ARMS_DEALER)(-1), usItemIndex, storeInventory[(int)usItemIndex, (int)fUsed], fUsed);
            if (ubNumBought > 0)
            {
                // If doing used items
                if (fUsed > 0)
                {
                    // then there should only be 1 of them, and it's damaged
                    pInventoryArray[i].ubQtyOnHand = 1;
                    pInventoryArray[i].ubItemQuality = 20 + Globals.Random.Next(60);
                }
                else    // new
                {
                    pInventoryArray[i].ubQtyOnHand = ubNumBought;
                    pInventoryArray[i].ubItemQuality = 100;
                }
            }
            else
            {
                // doesn't sell / not in stock
                pInventoryArray[i].ubQtyOnHand = 0;
                pInventoryArray[i].ubItemQuality = 0;
            }
        }
    }


    bool DoesGunOfSameClassExistInInventory(WeaponClass ubItemIndex, BOBBY_RAY ubDealerID)
    {
        Items i;

        var pInventoryArray = GetPtrToStoreInventory(ubDealerID);
        if (pInventoryArray == null)
        {
            return (false);
        }


        //go through all of the guns
        for (i = 0; i < Items.MAX_WEAPONS; i++)
        {
            //if it's the class we are looking for 
            if (WeaponTypes.Weapon[(int)i].ubWeaponClass == ubItemIndex)
            {
                // and it's a sufficiently cool gun to be counted as good
                if (Item[i].ubCoolness >= 4)
                {
                    //if there is already a qty on hand, then we found a match
                    if (pInventoryArray[(int)i].ubQtyOnHand > 0)
                    {
                        return (true);
                    }
                }
            }
        }
        return (false);
    }





    ////////////////////////////////////////////////////
    ////////////////////////////////////////////////////
    ////////////////////////////////////////////////////
    ////////////////////////////////////////////////////

    public static List<STORE_INVENTORY>? GetPtrToStoreInventory(BOBBY_RAY ubDealerID)
    {
        if (ubDealerID >= BOBBY_RAY.LISTS)
        {
            return (null);
        }

        if (ubDealerID == BOBBY_RAY.NEW)
        {
            return (LaptopSaveInfo.BobbyRayInventory);
        }
        else if (ubDealerID == BOBBY_RAY.USED)
        {
            return (LaptopSaveInfo.BobbyRayUsedInventory);
        }
        else
        {
            Debug.Assert(false);
        }
        //	else
        //		return( gArmsDealersInventory[ ubDealerID - TONYS_ITEMS ] );


        return (null);
    }



    /*
    int	CountNumberOfItemsInStoreInventory( int ubArmsDealerID )
    {
        int	cnt;
        int		ubNumItems=0;

        STORE_INVENTORY *pInventoryArray;

        pInventoryArray = GetPtrToStoreInventory( ubArmsDealerID );
        if( pInventoryArray == null )
            return( -1 );


        for( cnt=0; cnt<MAXITEMS; cnt++ )
        {
            if( pInventoryArray[cnt].ubQtyOnHand > 0 )
                ubNumItems++;
        }

        return( ubNumItems );
    }
    */
    ////////////////////////////////////////////////////
    ////////////////////////////////////////////////////
    ////////////////////////////////////////////////////
    ////////////////////////////////////////////////////

}

public struct LIFE_INSURANCE_PAYOUT
{
    public bool fActive;
    public int ubSoldierID;
    public int ubMercID;
    public int iPayOutPrice;
}


public struct LAST_HIRED_MERC_STRUCT
{
    public bool fHaveDisplayedPopUpInLaptop;        // Is set when the popup gets displayed, reset when entering laptop again.
    public int iIdOfMerc;
    public int uiArrivalTime;
}


public struct BobbyRayPurchaseStruct
{
    public int usItemIndex;
    public int ubNumberPurchased;
    public int bItemQuality;
    public int usBobbyItemIndex;                        //Item number in the BobbyRayInventory structure
    public bool fUsed;											//Indicates wether or not the item is from the used inventory or the regular inventory
}


public class LaptopSaveInfoStruct
{
    //General Laptop Info
    public bool gfNewGameLaptop;                                    //Is it the firs time in Laptop
    public bool[] fVisitedBookmarkAlready = new bool[20];            // have we visitied this site already?
    public BOOKMARK[] iBookMarkList = new BOOKMARK[MAX_BOOKMARKS];
    public int iCurrentBalance;                                  // current players balance

    //IMP Information
    bool fIMPCompletedFlag;                      // Has the player Completed the IMP process
    bool fSentImpWarningAlready;             // Has the Imp email warning already been sent


    //Personnel Info
    int[] ubDeadCharactersList = new int[256];
    int[] ubLeftCharactersList = new int[256];
    int[] ubOtherCharactersList = new int[256];

    // Aim Site


    // BobbyRay Site
    public List<STORE_INVENTORY> BobbyRayInventory = new();
    public List<STORE_INVENTORY> BobbyRayUsedInventory = new();
    public BobbyRayOrderStruct? BobbyRayOrdersOnDeliveryArray;
    public int usNumberOfBobbyRayOrderItems;             // The number of elements in the array
    public int usNumberOfBobbyRayOrderUsed;              // The number of items in the array that are used

    // Flower Site
    //NONE

    // Insurance Site
    public LIFE_INSURANCE_PAYOUT? pLifeInsurancePayouts;
    public int ubNumberLifeInsurancePayouts;             // The number of elements in the array
    public int ubNumberLifeInsurancePayoutUsed;      // The number of items in the array that are used
    public bool fBobbyRSiteCanBeAccessed;
    public int ubPlayerBeenToMercSiteStatus;
    public bool fFirstVisitSinceServerWentDown;
    public bool fNewMercsAvailableAtMercSite;
    public bool fSaidGenericOpeningInMercSite;
    public bool fSpeckSaidFloMarriedCousinQuote;
    public bool fHasAMercDiedAtMercSite;
    public int gbNumDaysTillFirstMercArrives;
    public int gbNumDaysTillSecondMercArrives;
    public int gbNumDaysTillThirdMercArrives;
    public int gbNumDaysTillFourthMercArrives;
    public int guiNumberOfMercPaymentsInDays;               // Keeps track of each day of payment the MERC site gets
    public Dictionary<BOBBY_RAY, int> usInventoryListLength = new();
    public int iVoiceId;
    public BOBBYR_VISITS ubHaveBeenToBobbyRaysAtLeastOnceWhileUnderConstruction;
    public bool fMercSiteHasGoneDownYet;
    public int ubSpeckCanSayPlayersLostQuote;
    public LAST_HIRED_MERC_STRUCT sLastHiredMerc;
    public int iCurrentHistoryPage;
    public int iCurrentFinancesPage;
    public int iCurrentEmailPage;
    public int uiSpeckQuoteFlags;
    public int uiFlowerOrderNumber;
    public int uiTotalMoneyPaidToSpeck;
    public int ubLastMercAvailableId;
    public int[] bPadding = new int[86];
}
