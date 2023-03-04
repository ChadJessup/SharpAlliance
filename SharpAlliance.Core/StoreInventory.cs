
using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core;

public class StoreInventory
{
}

public class STORE_INVENTORY
{
    public Items usItemIndex;                             //Index into the item table
    public int ubQtyOnHand;
    public int ubQtyOnOrder;                             //The number of items on order
    public int ubItemQuality;                            // the % damaged listed from 0 to 100
    public int filler;
    public bool fPreviouslyEligible;                // whether or not dealer has been eligible to sell this item in days prior to today
}


//Enums used for the access the MAX dealers array
public enum BOBBY_RAY
{
    NEW,
    USED,
    LISTS,
};
