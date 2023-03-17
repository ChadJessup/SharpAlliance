using System;

using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core.SubSystems;

public class ShopKeeper
{
    public void ShopKeeperScreenInit()
    {
    }
}

public class INVENTORY_IN_SLOT
{
    public bool fActive;
    public Items sItemIndex;
    public int uiFlags;
    public OBJECTTYPE ItemObject = new();
    public int ubLocationOfObject;                   //An enum value for the location of the item ( either in the arms dealers inventory, one of the offer areas or in the users inventory)
    public int bSlotIdInOtherLocation;
    public int ubIdOfMercWhoOwnsTheItem;
    public int uiItemPrice;                             //Only used for the players item that have been evaluated
    public int sSpecialItemElement;             // refers to which special item element an item in a dealer's inventory area
                                                // occupies.  -1 Means the item is "perfect" and has no associated special item.
}
