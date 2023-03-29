using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpAlliance.Core.SubSystems;

namespace SharpAlliance.Core;

public partial class Globals
{
    public const int SKI_NUM_TRADING_INV_SLOTS = 12;
    public const int SKI_NUM_TRADING_INV_ROWS = 2;
    public const int SKI_NUM_TRADING_INV_COLS = 6;

    public static INVENTORY_IN_SLOT[] PlayersOfferArea = new INVENTORY_IN_SLOT[SKI_NUM_TRADING_INV_SLOTS];
    public static int giShopKeepDialogueEventinProgress;
    //extern	BOOLEAN		gfRedrawSkiScreen;
    public SKI_DIRTY gubSkiDirtyLevel;
    public static OBJECTTYPE? gpHighLightedItemObject;
    public static INVENTORY_IN_SLOT gMoveingItem;
    public static OBJECTTYPE? pShopKeeperItemDescObject;
}

//Enums used for when the user clicks on an item and the item goes to..
public enum ShopKeeperScreen
{
    ARMS_DEALER_INVENTORY,
    ARMS_DEALER_OFFER_AREA,
    PLAYERS_OFFER_AREA,
    PLAYERS_INVENTORY,
};

public enum SKI_DIRTY
{
    LEVEL0,   // no redraw
    LEVEL1,   // redraw only items
    LEVEL2, // redraw everything
};

public struct INVENTORY_IN_SLOT
{
    public bool fActive;
    public int sItemIndex;
    public int uiFlags;
    OBJECTTYPE ItemObject;
    public int ubLocationOfObject;                   //An enum value for the location of the item ( either in the arms dealers inventory, one of the offer areas or in the users inventory)
    public int bSlotIdInOtherLocation;
    public int ubIdOfMercWhoOwnsTheItem;
    public int uiItemPrice;                             //Only used for the players item that have been evaluated
    public int sSpecialItemElement;              // refers to which special item element an item in a dealer's inventory area
                                                 // occupies.  -1 Means the item is "perfect" and has no associated special item.
}

[Flags]
public enum ARMS_INV
{
    ITEM_SELECTED = 0x00000001, // The item has been placed into the offer area
    //PLAYERS_ITEM_SELECTED = 0x00000002, // The source location for the item has been selected
    PLAYERS_ITEM_HAS_VALUE = 0x00000004, // The Players item is worth something to this dealer
    //ITEM_HIGHLIGHTED = 0x00000008, // If the items is highlighted
    ITEM_NOT_REPAIRED_YET = 0x00000010, // The item is in for repairs but not repaired yet
    ITEM_REPAIRED = 0x00000020, // The item is repaired
    JUST_PURCHASED = 0x00000040, // The item was just purchased
    PLAYERS_ITEM_HAS_BEEN_EVALUATED = 0x00000080, // The Players item has been evaluated
}
