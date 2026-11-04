namespace SharpAlliance.Core.SubSystems.LaptopSubSystem.BobbyRSubSystem;

// Text used by the Bobby Ray's catalogue pages (from Text.h and _EnglishText.c).
public partial class BobbyR
{
    private const int BOBBYR_GUNS_TO_ORDER = 0;
    private const int BOBBYR_GUNS_CLICK_ON_ITEMS = 1;
    private const int BOBBYR_GUNS_PREVIOUS_ITEMS = 2;
    private const int BOBBYR_GUNS_GUNS = 3;
    private const int BOBBYR_GUNS_AMMO = 4;
    private const int BOBBYR_GUNS_ARMOR = 5;
    private const int BOBBYR_GUNS_MISC = 6;
    private const int BOBBYR_GUNS_USED = 7;
    private const int BOBBYR_GUNS_MORE_ITEMS = 8;
    private const int BOBBYR_GUNS_ORDER_FORM = 9;
    private const int BOBBYR_GUNS_HOME = 10;
    private const int BOBBYR_GUNS_NUM_GUNS_THAT_USE_AMMO_1 = 11;
    private const int BOBBYR_GUNS_NUM_GUNS_THAT_USE_AMMO_2 = 12;
    private const int BOBBYR_GUNS_WGHT = 13;
    private const int BOBBYR_GUNS_CALIBRE = 14;
    private const int BOBBYR_GUNS_MAGAZINE = 15;
    private const int BOBBYR_GUNS_RANGE = 16;
    private const int BOBBYR_GUNS_DAMAGE = 17;
    private const int BOBBYR_GUNS_ROF = 18;
    private const int BOBBYR_GUNS_COST = 19;
    private const int BOBBYR_GUNS_IN_STOCK = 20;
    private const int BOBBYR_GUNS_QTY_ON_ORDER = 21;
    private const int BOBBYR_GUNS_DAMAGED = 22;
    private const int BOBBYR_GUNS_WEIGHT = 23;
    private const int BOBBYR_GUNS_SUB_TOTAL = 24;
    private const int BOBBYR_GUNS_PERCENT_FUNCTIONAL = 25;
    private const int BOBBYR_MORE_THEN_10_PURCHASES = 26;
    private const int BOBBYR_MORE_NO_MORE_IN_STOCK = 27;
    private const int BOBBYR_NO_MORE_STOCK = 28;

    public static readonly string[] BobbyRText =
    {
        "To Order",
        "Click on the item(s). For more than one, keep on clicking. Right click for less. Once you've selected all you'd like to buy, go on to the order form.",
        "Previous Items",
        "Guns",
        "Ammo",
        "Armor",
        "Misc.",
        "Used",
        "More Items",
        "ORDER FORM",
        "Home",
        "Your team has",
        "weapon(s) that use this type of ammo",
        "Weight:",
        "Cal:",
        "Mag:",
        "Rng:",
        "Dam:",
        "ROF:",
        "Cost:",
        "In stock:",
        "Qty on Order:",
        "Damaged",
        "Weight:",
        "SubTotal:",
        "* %% Functional",
        "Darn!  This here on-line order form will only accept 10 items per order.  If you're looking to order more stuff (and we hope you are), kindly make a separate order and accept our apologies.",
        "Sorry. We don't have any more of that in stock right now. Please try again later.",
        "Sorry, but we are currently out of stock on all items of that type.",
    };

    public static readonly string[] BobbyRayAmmoCaliber =
    {
        "0",
        ".38 cal",
        "9mm",
        ".45 cal",
        ".357 cal",
        "12 gauge",
        "CAWS",
        "5.45mm",
        "5.56mm",
        "7.62mm N.",
        "7.62mm WP",
        "4.7mm",
        "5.7mm",
        "Monster",
        "Rocket",
        "",
    };

    public static readonly string[] AmmoCaliber =
    {
        "0",
        ".38 cal",
        "9mm",
        ".45 cal",
        ".357 cal",
        "12 gauge",
        "CAWS",
        "5.45mm",
        "5.56mm",
        "7.62mm NATO",
        "7.62mm WP",
        "4.7mm",
        "5.7mm",
        "Monster",
        "Rocket",
        "",
        "",
    };
}
