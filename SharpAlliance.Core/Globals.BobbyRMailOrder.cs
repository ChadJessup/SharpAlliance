using System;
using System.Collections.Generic;
using SharpAlliance.Core.SubSystems;

namespace SharpAlliance.Core;

public partial class Globals
{
    public const int BOBBYR_ORDER_NUM_SHIPPING_CITIES = 17;
    public const int BOBBYR_NUM_DISPLAYED_CITIES = 10;
    public const int OVERNIGHT_EXPRESS = 1;
    public const int TWO_BUSINESS_DAYS = 2;
    public const int STANDARD_SERVICE = 3;
    public const int MIN_SHIPPING_WEIGHT = 20;
    public const FontStyle BOBBYR_ORDER_TITLE_TEXT_FONT = FontStyle.FONT14ARIAL;
    public const int BOBBYR_ORDER_TITLE_TEXT_COLOR = 157;
    public const int BOBBYR_FONT_BLACK = 2;
    public const FontStyle BOBBYR_ORDER_STATIC_TEXT_FONT = FontStyle.FONT12ARIAL;
    public const int BOBBYR_ORDER_STATIC_TEXT_COLOR = 145;
    public const FontStyle BOBBYR_DISCLAIMER_FONT = FontStyle.FONT10ARIAL;
    public const FontStyle BOBBYR_ORDER_DYNAMIC_TEXT_FONT = FontStyle.FONT12ARIAL;
    public const FontColor BOBBYR_ORDER_DYNAMIC_TEXT_COLOR = FontColor.FONT_MCOLOR_WHITE;
    public const FontColor BOBBYR_ORDER_DROP_DOWN_SELEC_COLOR = FontColor.FONT_MCOLOR_WHITE;
    public const FontStyle BOBBYR_DROPDOWN_FONT = FontStyle.FONT12ARIAL;
    public const int BOBBYR_ORDERGRID_X = LAPTOP_SCREEN_UL_X + 2;
    public const int BOBBYR_ORDERGRID_Y = LAPTOP_SCREEN_WEB_UL_Y + 62;
    public const int BOBBYR_BOBBY_RAY_TITLE_X = LAPTOP_SCREEN_UL_X + 171;
    public const int BOBBYR_BOBBY_RAY_TITLE_Y = LAPTOP_SCREEN_WEB_UL_Y + 3;
    public const int BOBBYR_BOBBY_RAY_TITLE_WIDTH = 160;
    public const int BOBBYR_BOBBY_RAY_TITLE_HEIGHT = 35;
    public const int BOBBYR_LOCATION_BOX_X = LAPTOP_SCREEN_UL_X + 276;
    public const int BOBBYR_LOCATION_BOX_Y = LAPTOP_SCREEN_WEB_UL_Y + 62;
    public const int BOBBYR_DELIVERYSPEED_X = LAPTOP_SCREEN_UL_X + 276;
    public const int BOBBYR_DELIVERYSPEED_Y = LAPTOP_SCREEN_WEB_UL_Y + 149;
    public const int BOBBYR_CLEAR_ORDER_X = LAPTOP_SCREEN_UL_X + 309;
    public const int BOBBYR_CLEAR_ORDER_Y = LAPTOP_SCREEN_WEB_UL_Y + 268;   //LAPTOP_SCREEN_WEB_UL_Y + 252;
    public const int BOBBYR_ACCEPT_ORDER_X = LAPTOP_SCREEN_UL_X + 299;
    public const int BOBBYR_ACCEPT_ORDER_Y = LAPTOP_SCREEN_WEB_UL_Y + 303;  //LAPTOP_SCREEN_WEB_UL_Y + 288;
    public const int BOBBYR_GRID_ROW_OFFSET = 20;
    public const int BOBBYR_GRID_TITLE_OFFSET = 27;
    public const int BOBBYR_GRID_FIRST_COLUMN_X = 3;//BOBBYR_ORDERGRID_X + 3;
    public const int BOBBYR_GRID_FIRST_COLUMN_Y = 37;//BOBBYR_ORDERGRID_Y + 37;
    public const int BOBBYR_GRID_FIRST_COLUMN_WIDTH = 23;
    public const int BOBBYR_GRID_SECOND_COLUMN_X = 28;//BOBBYR_ORDERGRID_X + 28;
    public const int BOBBYR_GRID_SECOND_COLUMN_Y = BOBBYR_GRID_FIRST_COLUMN_Y;
    public const int BOBBYR_GRID_SECOND_COLUMN_WIDTH = 40;
    public const int BOBBYR_GRID_THIRD_COLUMN_X = 70;//BOBBYR_ORDERGRID_X + 70;
    public const int BOBBYR_GRID_THIRD_COLUMN_Y = BOBBYR_GRID_FIRST_COLUMN_Y;
    public const int BOBBYR_GRID_THIRD_COLUMN_WIDTH = 111;
    public const int BOBBYR_GRID_FOURTH_COLUMN_X = 184;//BOBBYR_ORDERGRID_X + 184;
    public const int BOBBYR_GRID_FOURTH_COLUMN_Y = BOBBYR_GRID_FIRST_COLUMN_Y;
    public const int BOBBYR_GRID_FOURTH_COLUMN_WIDTH = 40;
    public const int BOBBYR_GRID_FIFTH_COLUMN_X = 224;//BOBBYR_ORDERGRID_X + 224;
    public const int BOBBYR_GRID_FIFTH_COLUMN_Y = BOBBYR_GRID_FIRST_COLUMN_Y;
    public const int BOBBYR_GRID_FIFTH_COLUMN_WIDTH = 42;
    public const int BOBBYR_SUBTOTAL_WIDTH = 212;
    public const int BOBBYR_SUBTOTAL_X = BOBBYR_GRID_FIRST_COLUMN_X;
    public const int BOBBYR_SUBTOTAL_Y = BOBBYR_GRID_FIRST_COLUMN_Y + BOBBYR_GRID_ROW_OFFSET * 10 + 3;
    public const int BOBBYR_SHIPPING_N_HANDLE_Y = BOBBYR_SUBTOTAL_Y + 17;
    public const int BOBBYR_GRAND_TOTAL_Y = BOBBYR_SHIPPING_N_HANDLE_Y + 20;
    public const int BOBBYR_SHIPPING_LOCATION_TEXT_X = BOBBYR_LOCATION_BOX_X + 8;
    public const int BOBBYR_SHIPPING_LOCATION_TEXT_Y = BOBBYR_LOCATION_BOX_Y + 8;
    public const int BOBBYR_SHIPPING_SPEED_X = BOBBYR_SHIPPING_LOCATION_TEXT_X;
    public const int BOBBYR_SHIPPING_SPEED_Y = BOBBYR_DELIVERYSPEED_Y + 11;
    public const int BOBBYR_SHIPPING_COST_X = BOBBYR_SHIPPING_SPEED_X + 130;
    public const int BOBBYR_OVERNIGHT_EXPRESS_Y = BOBBYR_DELIVERYSPEED_Y + 42;
    public const int BOBBYR_ORDER_FORM_TITLE_X = BOBBYR_BOBBY_RAY_TITLE_X;
    public const int BOBBYR_ORDER_FORM_TITLE_Y = BOBBYR_BOBBY_RAY_TITLE_Y + 37;
    public const int BOBBYR_ORDER_FORM_TITLE_WIDTH = 159;
    public const int BOBBYR_BACK_BUTTON_X = 130;
    public const int BOBBYR_BACK_BUTTON_Y = 400 + LAPTOP_SCREEN_WEB_DELTA_Y + 4;
    public const int BOBBYR_HOME_BUTTON_X = 515;
    public const int BOBBYR_HOME_BUTTON_Y = BOBBYR_BACK_BUTTON_Y;
    public const int BOBBYR_SHIPMENT_BUTTON_X = LAPTOP_SCREEN_UL_X + (LAPTOP_SCREEN_LR_X - LAPTOP_SCREEN_UL_X - 75) / 2;
    public const int BOBBYR_SHIPMENT_BUTTON_Y = BOBBYR_BACK_BUTTON_Y;
    public const int SHIPPING_SPEED_LIGHT_WIDTH = 9;
    public const int SHIPPING_SPEED_LIGHT_HEIGHT = 9;
    public const int BOBBYR_CONFIRM_ORDER_X = 220;
    public const int BOBBYR_CONFIRM_ORDER_Y = 170;
    public const int BOBBYR_CITY_START_LOCATION_X = BOBBYR_LOCATION_BOX_X + 6;
    public const int BOBBYR_CITY_START_LOCATION_Y = BOBBYR_LOCATION_BOX_Y + 61;
    public const int BOBBYR_DROP_DOWN_WIDTH = 182;//203;
    public const int BOBBYR_DROP_DOWN_HEIGHT = 19;
    public const int BOBBYR_CITY_NAME_OFFSET = 6;
    public const int BOBBYR_SCROLL_AREA_X = BOBBYR_CITY_START_LOCATION_X + BOBBYR_DROP_DOWN_WIDTH;
    public const int BOBBYR_SCROLL_AREA_Y = BOBBYR_CITY_START_LOCATION_Y;
    public const int BOBBYR_SCROLL_AREA_WIDTH = 22;
    public const int BOBBYR_SCROLL_AREA_HEIGHT = 139;
    public const int BOBBYR_SCROLL_AREA_HEIGHT_MINUS_ARROWS = BOBBYR_SCROLL_AREA_HEIGHT - (2 * BOBBYR_SCROLL_ARROW_HEIGHT) - 8;
    public const int BOBBYR_SCROLL_UP_ARROW_X = BOBBYR_SCROLL_AREA_X;
    public const int BOBBYR_SCROLL_UP_ARROW_Y = BOBBYR_SCROLL_AREA_Y + 5;
    public const int BOBBYR_SCROLL_DOWN_ARROW_X = BOBBYR_SCROLL_UP_ARROW_X;
    public const int BOBBYR_SCROLL_DOWN_ARROW_Y = BOBBYR_SCROLL_AREA_Y + BOBBYR_SCROLL_AREA_HEIGHT - 24;
    public const int BOBBYR_SCROLL_ARROW_WIDTH = 18;
    public const int BOBBYR_SCROLL_ARROW_HEIGHT = 20;
    public const int BOBBYR_SHIPPING_LOC_AREA_L_X = BOBBYR_LOCATION_BOX_X + 9;
    public const int BOBBYR_SHIPPING_LOC_AREA_T_Y = BOBBYR_LOCATION_BOX_Y + 39;
    public const int BOBBYR_SHIPPING_LOC_AREA_R_X = BOBBYR_LOCATION_BOX_X + 206;
    public const int BOBBYR_SHIPPING_LOC_AREA_B_Y = BOBBYR_LOCATION_BOX_Y + 57;
    public const int BOBBYR_SHIPPING_SPEED_NUMBER_X = BOBBYR_SHIPPING_COST_X;
    public const int BOBBYR_SHIPPING_SPEED_NUMBER_WIDTH = 37;
    public const int BOBBYR_SHIPPING_SPEED_NUMBER_1_Y = BOBBYR_OVERNIGHT_EXPRESS_Y;
    public const int BOBBYR_SHIPPING_SPEED_NUMBER_2_Y = BOBBYR_OVERNIGHT_EXPRESS_Y;
    public const int BOBBYR_SHIPPING_SPEED_NUMBER_3_Y = BOBBYR_OVERNIGHT_EXPRESS_Y;
    public const int BOBBYR_TOTAL_SAVED_AREA_X = BOBBYR_ORDERGRID_X + 221;
    public const int BOBBYR_TOTAL_SAVED_AREA_Y = BOBBYR_ORDERGRID_Y + 237;
    public const int BOBBYR_USED_WARNING_X = 122;
    public const int BOBBYR_USED_WARNING_Y = 382 + LAPTOP_SCREEN_WEB_DELTA_Y;
    public const int BOBBYR_PACKAXGE_WEIGHT_X = BOBBYR_LOCATION_BOX_X;
    public const int BOBBYR_PACKAXGE_WEIGHT_Y = LAPTOP_SCREEN_WEB_UL_Y + 249;
    public const int BOBBYR_PACKAXGE_WEIGHT_WIDTH = 188;

    public static int[] gShippingSpeedAreas = {585, 218 + LAPTOP_SCREEN_WEB_DELTA_Y,
                                                                585, 238 + LAPTOP_SCREEN_WEB_DELTA_Y,
                                                                585, 258 + LAPTOP_SCREEN_WEB_DELTA_Y};

    // Identifier for the images
    public static int guiBobbyRayTitle;
    public static int guiBobbyROrderGrid;
    public static int guiBobbyRLocationGraphic;
    public static int guiDeliverySpeedGraphic;
    public static int guiConfirmGraphic;
    public static int guiTotalSaveArea;        //used as a savebuffer for the subtotal, s&h, and grand total values
    public static int guiDropDownBorder;
    public static int guiGoldArrowImages;
    public static int guiPackageWeightImage;
    public static bool gfReDrawBobbyOrder = false;
    public static bool gfDrawConfirmOrderGrpahic;
    public static bool gfDestroyConfirmGrphiArea;
    public static bool gfCanAcceptOrder;
    public static bool gfRemoveItemsFromStock = false;
    public static int giGrandTotal;
    public static int guiShippingCost;
    public static int guiSubTotal;
    public static int gubSelectedLight;
    public static int gubDropDownAction;
    public static int gbSelectedCity = -1;               //keeps track of the currently selected city
    public static int gubCityAtTopOfList;
    public static int giNumberOfNewBobbyRShipment;
    public static List<NewBobbyRayOrderStruct> gpNewBobbyrShipments = new();

    //Clear Order Button
    public static int guiBobbyRClearOrder;
    public static int guiBobbyRClearOrderImage;

    //Accept Order Button
    public static int guiBobbyRAcceptOrder;
    public static int guiBobbyRAcceptOrderImage;

    //Back Button
    public static int guiBobbyRBack;
    public static int guiBobbyRBackImage;

    //Home Button
    public static int guiBobbyRHome;
    public static int guiBobbyRHomeImage;

    //Goto Shipment Page Button
    public static int guiBobbyRGotoShipmentPage;
    public static int giBobbyRGotoShipmentPageImage;

    //mouse region for the shipping speed selection area
    public static MOUSE_REGION[] gSelectedShippingSpeedRegion = new MOUSE_REGION[3];

    //mouse region for the confirm area
    public static MOUSE_REGION? gSelectedConfirmOrderRegion;

    //mouse region for the drop down city location area
    public static MOUSE_REGION[] gSelectedDropDownRegion = new MOUSE_REGION[BOBBYR_ORDER_NUM_SHIPPING_CITIES];

    //mouse region for scroll area for the drop down city location area
    public static MOUSE_REGION[] gSelectedScrollAreaDropDownRegion = new MOUSE_REGION[BOBBYR_ORDER_NUM_SHIPPING_CITIES];

    //mouse region to activate the shipping location drop down
    public static MOUSE_REGION? gSelectedActivateCityDroDownRegion;

    //mouse region to close the drop down menu
    public static MOUSE_REGION? gSelectedCloseDropDownRegion;

    //mouse region to click on the title to go to the home page
    public static MOUSE_REGION? gSelectedTitleLinkRegion;

    //mouse region to click on the up or down arrow on the scroll area
    public static MOUSE_REGION[] gSelectedUpDownArrowOnScrollAreaRegion = new MOUSE_REGION[2];

    public const int MAX_PURCHASE_AMOUNT = 10;

    public const FontStyle BOBBIES_SIGN_FONT = FontStyle.FONT14ARIAL;
    public const FontColor BOBBIES_SIGN_COLOR = (FontColor)2;
    public const FontColor BOBBIES_SIGN_BACKCOLOR = FontColor.FONT_MCOLOR_BLACK;
    public const FontShadow BOBBIES_SIGN_BACKGROUNDCOLOR = (FontShadow)78;//NO_SHADOW;
    public const int BOBBIES_NUMBER_SIGNS = 5;
    public const FontStyle BOBBIES_SENTENCE_FONT = FontStyle.FONT12ARIAL;
    public const FontColor BOBBIES_SENTENCE_COLOR = FontColor.FONT_MCOLOR_WHITE;
    public const FontShadow BOBBIES_SENTENCE_BACKGROUNDCOLOR = (FontShadow)2;//NO_SHADOW//226;
    public const int BOBBY_WOOD_BACKGROUND_X = LAPTOP_SCREEN_UL_X;
    public const int BOBBY_WOOD_BACKGROUND_Y = LAPTOP_SCREEN_WEB_UL_Y;
    public const int BOBBY_WOOD_BACKGROUND_WIDTH = 125;
    public const int BOBBY_WOOD_BACKGROUND_HEIGHT = 100;
    public const int BOBBY_RAYS_NAME_X = LAPTOP_SCREEN_UL_X + 77;
    public const int BOBBY_RAYS_NAME_Y = LAPTOP_SCREEN_WEB_UL_Y + 0;
    public const int BOBBY_RAYS_NAME_WIDTH = 344;
    public const int BOBBY_RAYS_NAME_HEIGHT = 66;
    public const int BOBBYS_PLAQUES_X = LAPTOP_SCREEN_UL_X + 39;
    public const int BOBBYS_PLAQUES_Y = LAPTOP_SCREEN_WEB_UL_Y + 174;
    public const int BOBBYS_PLAQUES_WIDTH = 414;
    public const int BOBBYS_PLAQUES_HEIGHT = 190;
    public const int BOBBIES_TOPHINGE_X = LAPTOP_SCREEN_UL_X;
    public const int BOBBIES_TOPHINGE_Y = LAPTOP_SCREEN_WEB_UL_Y + 42;
    public const int BOBBIES_BOTTOMHINGE_X = LAPTOP_SCREEN_UL_X;
    public const int BOBBIES_BOTTOMHINGE_Y = LAPTOP_SCREEN_WEB_UL_Y + 338;
    public const int BOBBIES_STORE_PLAQUE_X = LAPTOP_SCREEN_UL_X + 148;
    public const int BOBBIES_STORE_PLAQUE_Y = LAPTOP_SCREEN_WEB_UL_Y + 66;
    public const int BOBBIES_STORE_PLAQUE_HEIGHT = 93;
    public const int BOBBIES_HANDLE_X = LAPTOP_SCREEN_UL_X + 457;
    public const int BOBBIES_HANDLE_Y = LAPTOP_SCREEN_WEB_UL_Y + 147;
    public const int BOBBIES_FIRST_SENTENCE_X = LAPTOP_SCREEN_UL_X;
    public const int BOBBIES_FIRST_SENTENCE_Y = BOBBIES_STORE_PLAQUE_Y + BOBBIES_STORE_PLAQUE_HEIGHT - 3;
    public const int BOBBIES_FIRST_SENTENCE_WIDTH = 500;
    public const int BOBBIES_2ND_SENTENCE_X = LAPTOP_SCREEN_UL_X;
    public const int BOBBIES_2ND_SENTENCE_Y = BOBBIES_FIRST_SENTENCE_Y + 13;
    public const int BOBBIES_2ND_SENTENCE_WIDTH = 500;
    public const int BOBBIES_CENTER_SIGN_OFFSET_Y = 23;
    public const int BOBBIES_USED_SIGN_X = BOBBYS_PLAQUES_X + 93;
    public const int BOBBIES_USED_SIGN_Y = BOBBYS_PLAQUES_Y + 32;
    public const int BOBBIES_USED_SIGN_WIDTH = 92;
    public const int BOBBIES_USED_SIGN_HEIGHT = 50;
    public const int BOBBIES_USED_SIGN_TEXT_OFFSET = BOBBIES_USED_SIGN_Y + 10;
    public const int BOBBIES_MISC_SIGN_X = BOBBYS_PLAQUES_X + 238;
    public const int BOBBIES_MISC_SIGN_Y = BOBBYS_PLAQUES_Y + 27;
    public const int BOBBIES_MISC_SIGN_WIDTH = 103;
    public const int BOBBIES_MISC_SIGN_HEIGHT = 57;
    public const int BOBBIES_MISC_SIGN_TEXT_OFFSET = BOBBIES_MISC_SIGN_Y + BOBBIES_CENTER_SIGN_OFFSET_Y;
    public const int BOBBIES_GUNS_SIGN_X = BOBBYS_PLAQUES_X + 3;
    public const int BOBBIES_GUNS_SIGN_Y = BOBBYS_PLAQUES_Y + 102;
    public const int BOBBIES_GUNS_SIGN_WIDTH = 116;
    public const int BOBBIES_GUNS_SIGN_HEIGHT = 75;
    public const int BOBBIES_GUNS_SIGN_TEXT_OFFSET = BOBBIES_GUNS_SIGN_Y + BOBBIES_CENTER_SIGN_OFFSET_Y;
    public const int BOBBIES_AMMO_SIGN_X = BOBBYS_PLAQUES_X + 150;
    public const int BOBBIES_AMMO_SIGN_Y = BOBBYS_PLAQUES_Y + 105;
    public const int BOBBIES_AMMO_SIGN_WIDTH = 112;
    public const int BOBBIES_AMMO_SIGN_HEIGHT = 71;
    public const int BOBBIES_AMMO_SIGN_TEXT_OFFSET = BOBBIES_AMMO_SIGN_Y + BOBBIES_CENTER_SIGN_OFFSET_Y;
    public const int BOBBIES_ARMOUR_SIGN_X = BOBBYS_PLAQUES_X + 290;
    public const int BOBBIES_ARMOUR_SIGN_Y = BOBBYS_PLAQUES_Y + 108;
    public const int BOBBIES_ARMOUR_SIGN_WIDTH = 114;
    public const int BOBBIES_ARMOUR_SIGN_HEIGHT = 70;
    public const int BOBBIES_ARMOUR_SIGN_TEXT_OFFSET = BOBBIES_ARMOUR_SIGN_Y + BOBBIES_CENTER_SIGN_OFFSET_Y;
    public const int BOBBIES_3RD_SENTENCE_X = LAPTOP_SCREEN_UL_X;
    public const int BOBBIES_3RD_SENTENCE_Y = BOBBIES_BOTTOMHINGE_Y + 40;
    public const int BOBBIES_3RD_SENTENCE_WIDTH = 500;
    public const int BOBBY_R_NEW_PURCHASE_ARRIVAL_TIME = 1 * 60 * 24; // minutes in 1 day;
    public const int BOBBY_R_USED_PURCHASE_OFFSET = 1000;
    public const int BOBBYR_UNDERCONSTRUCTION_ANI_DELAY = 150;
    public const int BOBBYR_UNDERCONSTRUCTION_NUM_FRAMES = 5;
    public const int BOBBYR_UNDERCONSTRUCTION_X = LAPTOP_SCREEN_UL_X + (LAPTOP_SCREEN_LR_X - LAPTOP_SCREEN_UL_X - BOBBYR_UNDERCONSTRUCTION_WIDTH) / 2;
    public const int BOBBYR_UNDERCONSTRUCTION_Y = 175;
    public const int BOBBYR_UNDERCONSTRUCTION1_Y = 378;
    public const int BOBBYR_UNDERCONSTRUCTION_WIDTH = 414;
    public const int BOBBYR_UNDERCONSTRUCTION_HEIGHT = 64;
    public const int BOBBYR_UNDER_CONSTRUCTION_TEXT_X = LAPTOP_SCREEN_UL_X;
    public const int BOBBYR_UNDER_CONSTRUCTION_TEXT_Y = BOBBYR_UNDERCONSTRUCTION_Y + 62 + 60;
    public const int BOBBYR_UNDER_CONSTRUCTION_TEXT_WIDTH = LAPTOP_SCREEN_LR_X - LAPTOP_SCREEN_UL_X;

    public static string guiBobbyName;
    public static string guiPlaque;
    public static string guiTopHinge;
    public static string guiBottomHinge;
    public static string guiStorePlaque;
    public static string guiHandle;
    public static string guiWoodBackground;
    public static string guiUnderConstructionImage;

    public static LAPTOP_MODE guiLastBobbyRayPage;

    public static LAPTOP_MODE[] gubBobbyRPages =
    {
        LAPTOP_MODE.BOBBY_R_USED,
        LAPTOP_MODE.BOBBY_R_MISC,
        LAPTOP_MODE.BOBBY_R_GUNS,
        LAPTOP_MODE.BOBBY_R_AMMO,
        LAPTOP_MODE.BOBBY_R_ARMOR,
    };

    //Bobby's Sign menu mouse regions
    public static List<MOUSE_REGION> gSelectedBobbiesSignMenuRegion = new();
}

public class NewBobbyRayOrderStruct
{
    public bool fActive;
    public BR ubDeliveryLoc;                // the city the shipment is going to
    public int ubDeliveryMethod;         // type of delivery: next day, 2 days ...
    public BobbyRayPurchaseStruct[] BobbyRayPurchase = new BobbyRayPurchaseStruct[Globals.MAX_PURCHASE_AMOUNT];
    public int ubNumberPurchases;
    public int uiPackageWeight;
    public uint uiOrderedOnDayNum;
    public bool fDisplayedInShipmentPage;
    public int[] ubFiller = new int[7];
}

public struct BobbyRayPurchaseStruct
{
    public int usItemIndex;
    public int ubNumberPurchased;
    public int bItemQuality;
    public int usBobbyItemIndex;                        //Item number in the BobbyRayInventory structure
    public bool fUsed;											//Indicates wether or not the item is from the used inventory or the regular inventory
}

public class BobbyRayOrderStruct
{
    public bool fActive;
    public BobbyRayPurchaseStruct[] BobbyRayPurchase = new BobbyRayPurchaseStruct[Globals.MAX_PURCHASE_AMOUNT];
    public int ubNumberPurchases;
}
