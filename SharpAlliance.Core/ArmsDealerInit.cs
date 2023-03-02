using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core;

public class ArmsDealerInit
{
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

    public int uiRepairDoneTime;            // If the item is in for repairs, this holds the time when it will be repaired (in min)
    public bool fActive;                            // TRUE means an item is stored here (empty elements may not always be freed immediately)
    public int ubOwnerProfileId;         // stores which merc previously owned an item being repaired
    public int[] ubPadding;// [6];					// filler
}

public struct DEALER_ITEM_HEADER
{
    // Non-special items are all the identical and are totaled inside ubPerfectItems.
    // Items being repaired are also stored here, with a negative condition.
    // NOTE: special item elements may remain allocated long after item has been removed, to reduce memory fragmentation!!!

    public int ubTotalItems;                 // sum of all the items (all perfect ones + all special ones)
    public int ubPerfectItems;               // non-special (perfect) items held by dealer
    public int ubStrayAmmo;                  // partially-depleted ammo mags are stored here as #bullets, and can be converted to full packs

    public int ubElementsAlloced;        // number of DEALER_SPECIAL_ITEM array elements alloced for the special item array
    public DEALER_SPECIAL_ITEM? SpecialItem;   // dynamic array of special items with this same item index

    public int uiOrderArrivalTime;      // Day the items ordered will arrive on.  It's UINT32 in case we change this to minutes.
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
