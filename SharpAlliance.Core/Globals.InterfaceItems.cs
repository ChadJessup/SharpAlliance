using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpAlliance.Core;
using SharpAlliance.Core.SubSystems;
using SixLabors.ImageSharp;
using SharpAlliance.Core.Screens;

using static SharpAlliance.Core.Globals;
using SixLabors.ImageSharp.PixelFormats;

namespace SharpAlliance.Core;

public partial class Globals
{
    public static Dictionary<InventorySlot, MOUSE_REGION> gSMInvRegion = new();// MOUSE_REGION[(int)NUM_INV_SLOTS];
    public static MOUSE_REGION gKeyRingPanel;
    public static MOUSE_REGION gSMInvCamoRegion;
    public static Dictionary<InventorySlot, int> gbCompatibleAmmo = new();// int[(int)NUM_INV_SLOTS];
    public static Dictionary<InventorySlot, int> gbInvalidPlacementSlot = new();// int[(int)NUM_INV_SLOTS];
    public static int[] us16BPPItemCyclePlacedItemColors = new int[20];
    public static Dictionary<SoldierBodyTypes, List<string>> guiBodyInvVO = new();
    public static string guiGoldKeyVO;
    public static int gbCompatibleApplyItem = 0;
    public static REMOVE_MONEY gRemoveMoney;

    public const int NUM_PICKUP_SLOTS = 6;

    public const int ITEMPICK_UP_X = 55;
    public const int ITEMPICK_UP_Y = 5;
    public const int ITEMPICK_DOWN_X = 111;
    public const int ITEMPICK_DOWN_Y = 5;
    public const int ITEMPICK_ALL_X = 79;
    public const int ITEMPICK_ALL_Y = 6;
    public const int ITEMPICK_OK_X = 16;
    public const int ITEMPICK_OK_Y = 6;
    public const int ITEMPICK_CANCEL_X = 141;
    public const int ITEMPICK_CANCEL_Y = 6;
    public const int ITEMPICK_START_X_OFFSET = 10;
    public const int ITEMPICK_START_Y_OFFSET = 20;
    public const int ITEMPICK_GRAPHIC_X = 10;
    public const int ITEMPICK_GRAPHIC_Y = 12;
    public const int ITEMPICK_GRAPHIC_YSPACE = 26;
    public const int ITEMPICK_TEXT_X = 56;
    public const int ITEMPICK_TEXT_Y = 22;
    public const int ITEMPICK_TEXT_YSPACE = 26;
    public const int ITEMPICK_TEXT_WIDTH = 109;
    public const int ITEMPICK_TEXT_HEIGHT = 17;
    public static ITEM_PICKUP_MENU_STRUCT gItemPickupMenu;

    public static string wprintf(string format, params object?[] args) => string.Format(format, args);


    // This definition mimics what is found in WINDOWS.H ( for Direct Draw compatiblity )
    // From RGB to COLORVAL
    public static Rgba32 FROMRGB(byte r, byte g, byte b) => new Rgba32(r, g, b);// ((int)(((r) | ((g) << 8)) | ((b)) << 16));

    public const FontStyle ITEMDESC_FONT = FontStyle.BLOCKFONT2;
    public const FontShadow ITEMDESC_FONTSHADOW1 = FontShadow.MILITARY_SHADOW;
    public const FontShadow ITEMDESC_FONTSHADOW2 = (FontShadow)32;
    public const FontShadow ITEMDESC_FONTSHADOW3 = (FontShadow)34;
    public const FontColor ITEMDESC_FONTFORE1 = (FontColor)33;
    public const FontColor ITEMDESC_FONTFORE2 = (FontColor)32;
    public const FontColor ITEMDESC_FONTAPFORE = (FontColor)218;
    public const FontColor ITEMDESC_FONTHPFORE = (FontColor)24;
    public const FontColor ITEMDESC_FONTBSFORE = (FontColor)125;
    public const FontColor ITEMDESC_FONTHEFORE = (FontColor)75;
    public const FontColor ITEMDESC_FONTHEAPFORE = (FontColor)76;
    public const FontColor ITEMDESC_AMMO_FORE = (FontColor)209;
    public const FontColor ITEMDESC_FONTHIGHLIGHT = FontColor.FONT_MCOLOR_WHITE;
    public static readonly Rgba32 STATUS_BAR_SHADOW = FROMRGB(140, 136, 119);
    public static readonly Rgba32 STATUS_BAR = FROMRGB(201, 172, 133);
    public static readonly Rgba32 DESC_STATUS_BAR_SHADOW = STATUS_BAR_SHADOW;
    public static readonly Rgba32 DESC_STATUS_BAR = STATUS_BAR;
    public const int MIN_LOB_RANGE = 4;
    public const int INV_BAR_DX = 5;
    public const int INV_BAR_DY = 21;
    public const int RENDER_ITEM_NOSTATUS = 20;
    public const int RENDER_ITEM_ATTACHMENT1 = 200;
    public const int ITEM_STATS_WIDTH = 26;
    public const int ITEM_STATS_HEIGHT = 8;
    public const int ITEMDESC_START_X = 214;
    public const int ITEMDESC_START_Y = 1 + INV_INTERFACE_START_Y;
    public const int ITEMDESC_HEIGHT = 133;
    public const int ITEMDESC_WIDTH = 320;
    public const int MAP_ITEMDESC_HEIGHT = 268;
    public const int MAP_ITEMDESC_WIDTH = 272;
    public static int ITEMDESC_NAME_X = 16 + gsInvDescX;
    public static int ITEMDESC_NAME_Y = 67 + gsInvDescY;
    public static int ITEMDESC_CALIBER_X = 162 + gsInvDescX;
    public static int ITEMDESC_CALIBER_Y = 67 + gsInvDescY;
    public const int ITEMDESC_CALIBER_WIDTH = 142;
    public static int MAP_ITEMDESC_CALIBER_X = 105 + gsInvDescX;
    public static int MAP_ITEMDESC_CALIBER_Y = 66 + gsInvDescY;
    public const int MAP_ITEMDESC_CALIBER_WIDTH = 149;
    public static int ITEMDESC_ITEM_X = 8 + gsInvDescX;
    public static int ITEMDESC_ITEM_Y = 11 + gsInvDescY;
    public const int CAMO_REGION_HEIGHT = 75;
    public const int CAMO_REGION_WIDTH = 75;
    public static int BULLET_SING_X = 222 + gsInvDescX;
    public static int BULLET_SING_Y = 49 + gsInvDescY;
    public static int BULLET_BURST_X = 263 + gsInvDescX;
    public static int BULLET_BURST_Y = 49 + gsInvDescY;
    public const int BULLET_WIDTH = 3;
    public const int BULLET_GAP = 5;
    public readonly static int MAP_BULLET_SING_X = 77 + gsInvDescX;
    public readonly static int MAP_BULLET_SING_Y = 135 + gsInvDescY;
    public readonly static int MAP_BULLET_BURST_X = 117 + gsInvDescX;
    public readonly static int MAP_BULLET_BURST_Y = 135 + gsInvDescY;
    public readonly static int MAP_ITEMDESC_NAME_X = 7 + gsInvDescX;
    public readonly static int MAP_ITEMDESC_NAME_Y = 65 + gsInvDescY;
    public readonly static int MAP_ITEMDESC_ITEM_X = 25 + gsInvDescX;
    public readonly static int MAP_ITEMDESC_ITEM_Y = 6 + gsInvDescY;
    public readonly static int ITEMDESC_DESC_START_X = 11 + gsInvDescX;
    public readonly static int ITEMDESC_DESC_START_Y = 80 + gsInvDescY;
    public readonly static int ITEMDESC_PROS_START_X = 11 + gsInvDescX;
    public readonly static int ITEMDESC_PROS_START_Y = 110 + gsInvDescY;
    public readonly static int ITEMDESC_CONS_START_X = 11 + gsInvDescX;
    public readonly static int ITEMDESC_CONS_START_Y = 120 + gsInvDescY;
    public readonly static int ITEMDESC_ITEM_STATUS_X = 6 + gsInvDescX;
    public readonly static int ITEMDESC_ITEM_STATUS_Y = 60 + gsInvDescY;
    public const string DOTDOTDOT = "...";
    public const string COMMA_AND_SPACE = ", ";
    public static bool ITEM_PROS_AND_CONS(Items usItem) => Item[usItem].usItemClass.HasFlag(IC.GUN);
    public static int MAP_ITEMDESC_DESC_START_X = 23 + gsInvDescX;
    public static int MAP_ITEMDESC_DESC_START_Y = 170 + gsInvDescY;
    public static int MAP_ITEMDESC_PROS_START_X = 23 + gsInvDescX;
    public static int MAP_ITEMDESC_PROS_START_Y = 230 + gsInvDescY;
    public static int MAP_ITEMDESC_CONS_START_X = 23 + gsInvDescX;
    public static int MAP_ITEMDESC_CONS_START_Y = 240 + gsInvDescY;
    public static int MAP_ITEMDESC_ITEM_STATUS_X = 18 + gsInvDescX;
    public static int MAP_ITEMDESC_ITEM_STATUS_Y = 53 + gsInvDescY;
    public const int ITEMDESC_ITEM_STATUS_WIDTH = 2;
    public const int ITEMDESC_ITEM_STATUS_HEIGHT = 50;
    public const int ITEMDESC_ITEM_STATUS_HEIGHT_MAP = 40;
    public const int ITEMDESC_DESC_WIDTH = 301;
    public const int MAP_ITEMDESC_DESC_WIDTH = 220;
    public const int ITEMDESC_ITEM_WIDTH = 117;
    public const int ITEMDESC_ITEM_HEIGHT = 54;
    public static int ITEMDESC_AMMO_X = 10 + gsInvDescX;
    public static int ITEMDESC_AMMO_Y = 50 + gsInvDescY;
    public static int MAP_ITEMDESC_AMMO_X = 28 + gsInvDescX;
    public static int MAP_ITEMDESC_AMMO_Y = 45 + gsInvDescY;
    public const int ITEMDESC_AMMO_TEXT_X = 3;
    public const int ITEMDESC_AMMO_TEXT_Y = 1;
    public const int ITEMDESC_AMMO_TEXT_WIDTH = 31;
    public const int WORD_WRAP_INV_WIDTH = 58;
    public const int ITEM_BAR_WIDTH = 2;
    public const int ITEM_BAR_HEIGHT = 20;
    public const FontStyle ITEM_FONT = FontStyle.TINYFONT1;
    public const int EXCEPTIONAL_DAMAGE = 30;
    public const int EXCEPTIONAL_WEIGHT = 20;
    public const int EXCEPTIONAL_RANGE = 300;
    public const int EXCEPTIONAL_MAGAZINE = 30;
    public const int EXCEPTIONAL_AP_COST = 7;
    public const int EXCEPTIONAL_BURST_SIZE = 5;
    public const int EXCEPTIONAL_RELIABILITY = 2;
    public const int EXCEPTIONAL_REPAIR_EASE = 2;
    public const int BAD_DAMAGE = 23;
    public const int BAD_WEIGHT = 45;
    public const int BAD_RANGE = 150;
    public const int BAD_MAGAZINE = 10;
    public const int BAD_AP_COST = 11;
    public const int BAD_RELIABILITY = -2;
    public const int BAD_REPAIR_EASE = -2;
    public const int KEYRING_X = 487;
    public const int KEYRING_Y = 445;
    public const int MAP_KEYRING_X = 217;
    public const int MAP_KEYRING_Y = 271;
    public const int KEYRING_WIDTH = 517 - 487;
    public const int KEYRING_HEIGHT = 469 - 445;
    public const int TACTICAL_INVENTORY_KEYRING_GRAPHIC_OFFSET_X = 215;

    // AN ARRAY OF MOUSE REGIONS, ONE FOR EACH OBJECT POSITION ON BUDDY
    public static MOUSE_REGION[] gInvRegions = new MOUSE_REGION[(int)NUM_INV_SLOTS];

    public static MOUSE_REGION gMPanelRegion;
    public static bool gfAddingMoneyToMercFromPlayersAccount;
    public static SOLDIERTYPE? gpSMCurrentMerc;
    public static int gubSelectSMPanelToMerc;
    public static int guiMapInvSecondHandBlockout;

    public static MOUSE_REGION gInvDesc;

    public static OBJECTTYPE gItemPointer;
    public static bool gfItemPointerDifferentThanDefault = false;
    public static SOLDIERTYPE? gpItemPointerSoldier;
    public static InventorySlot gbItemPointerSrcSlot;
    public static TileIndexes gusItemPointer = (TileIndexes)255;
    public static int usItemSnapCursor;
    public static uint guiNewlyPlacedItemTimer = 0;
    public static bool gfBadThrowItemCTGH;
    public static bool gfDontChargeAPsToPickup = false;
    public static bool gbItemPointerLocateGood = false;

    // ITEM DESCRIPTION BOX STUFF
    public static int guiItemDescBox;
    public static int guiMapItemDescBox;
    public static int guiItemGraphic;
    public static int guiMoneyGraphicsForDescBox;
    public static int guiBullet;
    public static bool gfInItemDescBox = false;
    public static ScreenName guiCurrentItemDescriptionScreen = 0;
    public static OBJECTTYPE? gpItemDescObject = null;
    public static bool gfItemDescObjectIsAttachment = false;
    public static int[] gzItemName = new int[SIZE_ITEM_NAME];
    public static int[] gzItemDesc = new int[SIZE_ITEM_INFO];
    public static int[] gzItemPros = new int[SIZE_ITEM_PROS];
    public static int[] gzItemCons = new int[SIZE_ITEM_CONS];
    public static int[] gzFullItemPros = new int[SIZE_ITEM_PROS];
    public static int[] gzFullItemCons = new int[SIZE_ITEM_PROS];
    public static int[] gzFullItemTemp = new int[SIZE_ITEM_PROS]; // necessary, unfortunately
    public static int gsInvDescX;
    public static int gsInvDescY;
    public static int gubItemDescStatusIndex;
    public static int giItemDescAmmoButtonImages;
    public static int giItemDescAmmoButton;
    public static bool gfItemAmmoDown = false;
    public static SOLDIERTYPE? gpItemDescSoldier;
    public static bool fItemDescDelete = false;
    public static MOUSE_REGION[] gItemDescAttachmentRegions = new MOUSE_REGION[4];
    public static MOUSE_REGION[] gProsAndConsRegions = new MOUSE_REGION[2];

    public static int[] guiMoneyButtonBtn = new int[MAX_ATTACHMENTS];
    public static int guiMoneyButtonImage;
    public static int guiMoneyDoneButtonImage;

    public static Items[] gusOriginalAttachItem = new Items[MAX_ATTACHMENTS];
    public static Items[] gbOriginalAttachStatus = new Items[MAX_ATTACHMENTS];
    public static SOLDIERTYPE? gpAttachSoldier;

    public static MoneyLoc gMoneyButtonLoc = new(343, 351);
    public static MoneyLoc gMapMoneyButtonLoc = new(174, 115);
    public static MoneyLoc[] gMoneyButtonOffsets =
    {
        new(0, 0),
        new(34, 0),
        new(0, 32),
        new(34, 32),
        new(8, 22),
    };

    // number of keys on keyring, temp for now
    public const int NUMBER_KEYS_ON_KEYRING = 28;
    public const int KEY_RING_ROW_WIDTH = 7;
    public const int MAP_KEY_RING_ROW_WIDTH = 4;

    // ITEM STACK POPUP STUFF
    public static bool gfInItemStackPopup = false;
    public static int guiItemPopupBoxes;
    public static OBJECTTYPE? gpItemPopupObject;
    public static int gsItemPopupWidth;
    public static int gsItemPopupHeight;
    public static int gsItemPopupX;
    public static int gsItemPopupY;
    public static MOUSE_REGION[] gItemPopupRegions = new MOUSE_REGION[8];
    public static MOUSE_REGION[] gKeyRingRegions = new MOUSE_REGION[NUMBER_KEYS_ON_KEYRING];
    public static bool gfInKeyRingPopup = false;
    public static int gubNumItemPopups = 0;
    public static MOUSE_REGION gItemPopupRegion;
    public static int gsItemPopupInvX;
    public static int gsItemPopupInvY;
    public static int gsItemPopupInvWidth;
    public static int gsItemPopupInvHeight;

    public static int gsKeyRingPopupInvX;
    public static int gsKeyRingPopupInvY;
    public static int gsKeyRingPopupInvWidth;
    public static int gsKeyRingPopupInvHeight;

    public static SOLDIERTYPE? gpItemPopupSoldier;

    // inventory description done button for mapscreen
    public static int giMapInvDescButtonImage;
    public static int giMapInvDescButton = -1;
    public static bool gfItemPopupRegionCallbackEndFix = false;

    public static int[] ubRGBItemCyclePlacedItemColors =
    {
        25,     25,     25,
        50,     50,     50,
        75,     75,     75,
        100,    100,    100,
        125,    125,    125,
        150,    150,    150,
        175,    175,    175,
        200,    200,    200,
        225,    225,    225,
        250,    250,    250,
        250,    250,    250,
        225,    225,    225,
        200,    200,    200,
        175,    175,    175,
        150,    150,    150,
        125,    125,    125,
        100,    100,    100,
        75,     75,     75,
        50,     50,     50,
        25,     25,     25
    };

    public const int NUM_INV_HELPTEXT_ENTRIES = 1;

    public static INV_DESC_STATS[] gWeaponStats =
    {
        new(202, 25, 83),
        new(202, 15, 83),
        new(202, 15, 83),
        new(265, 40, 20),
        new(202, 40, 32),
        new(202, 50, 32),
        new(265, 50, 20),
        new(234, 50, 0),
        new(290, 50, 0),
    };


    // displayed AFTER the mass/weight/"Kg" line
    public static INV_DESC_STATS[] gMoneyStats =
    {
        new(202, 14, 78),
        new(212, 25, 78),
        new(202, 40, 78),
        new(212, 51, 78),
    };

    // displayed AFTER the mass/weight/"Kg" line
    public static INV_DESC_STATS[] gMapMoneyStats =
    {
        new(51,     97,             45),
        new(61,     107,            75),
        new(51,     125,            45),
        new(61,     135,            70),
    };

    public static INV_DESC_STATS[] gMapWeaponStats =
    {
        new(72 - 20,        20+80+8,        80),
        new(72 - 20,        20+80-2,        80),
        new(72 - 20,        20+80-2,        80),
        new(72+65 - 20, 40+80+4,        21),
        new(72 - 20,        40+80+4,        30),
        new(72 - 20,        53+80+2,            30),
        new(72+65 - 20, 53+80+2,            25),
        new(86,                 53+80+2,            0),
        new(145,                53+80+2,            0),
    };


    public static INV_ATTACHXY[] gItemDescAttachmentsXY =
    {
        new(129,        12,     SM_INV_SLOT_HEIGHT,     SM_INV_SLOT_WIDTH,      INV_BAR_DX-1,       INV_BAR_DY+1),
        new(163,        12,     SM_INV_SLOT_HEIGHT,     SM_INV_SLOT_WIDTH,      INV_BAR_DX-1,       INV_BAR_DY+1),
        new(129,        39,     SM_INV_SLOT_HEIGHT,     SM_INV_SLOT_WIDTH,      INV_BAR_DX-1,       INV_BAR_DY+1),
        new(163,        39,     SM_INV_SLOT_HEIGHT,     SM_INV_SLOT_WIDTH,      INV_BAR_DX-1,       INV_BAR_DY+1),
    };

    public static INV_ATTACHXY[] gMapItemDescAttachmentsXY =
    {
        new(173,      10,     SM_INV_SLOT_HEIGHT,     26,     INV_BAR_DX + 2,     INV_BAR_DY),
        new(211,        10,     SM_INV_SLOT_HEIGHT,     26,     INV_BAR_DX + 2,     INV_BAR_DY),
        new(173,        36,     SM_INV_SLOT_HEIGHT,     26,     INV_BAR_DX + 2,     INV_BAR_DY),
        new(211,        36,     SM_INV_SLOT_HEIGHT,     26,     INV_BAR_DX + 2,     INV_BAR_DY),
    };

    public static Rectangle[] gItemDescProsConsRects =
    {// NB the left value is calculated based on the width of the 'pros' and 'cons' labels
    	new(0, 111, 313, 118),
        new(0, 119, 313, 126),
    };

    public static Rectangle[] gMapItemDescProsConsRects =
    {
        new(0, 231, 313, 238),
        new(0, 239, 313, 246),
    };


    public static INV_HELPTEXT[] gItemDescHelpText =
    {
        new(new [] { 69 }, // x locations
    	    new [] { 12 }, // y locations
    	    new [] { 170 }, // widths
    	    EnglishText.Message[(int)STRINGS.ATTACHMENT_HELP],
            EnglishText.Message[(int)STRINGS.ATTACHMENT_INVALID_HELP])
    };

    public static bool gfItemDescHelpTextOffset = false;

    // ARRAY FOR INV PANEL INTERFACE ITEM POSITIONS (sX,sY get set via InitInvSlotInterface() )
    public static Dictionary<InventorySlot, INV_REGIONS> gSMInvData = new()
    {
        { InventorySlot.HELMETPOS,      new(false, INV_BAR_DX,     INV_BAR_DY,     HEAD_INV_SLOT_WIDTH,    HEAD_INV_SLOT_HEIGHT, 0,  0) },
        { InventorySlot.VESTPOS,        new(false, INV_BAR_DX,     INV_BAR_DY,     VEST_INV_SLOT_WIDTH,    VEST_INV_SLOT_HEIGHT, 0,  0) },
        { InventorySlot.LEGPOS,         new(false, INV_BAR_DX,     INV_BAR_DY,     LEGS_INV_SLOT_WIDTH,    LEGS_INV_SLOT_HEIGHT, 0,  0) },
        { InventorySlot.HEAD1POS,       new(false, INV_BAR_DX,     INV_BAR_DY,     SM_INV_SLOT_WIDTH,      SM_INV_SLOT_HEIGHT,   0,  0) },
        { InventorySlot.HEAD2POS,       new(false, INV_BAR_DX,     INV_BAR_DY,     SM_INV_SLOT_WIDTH,      SM_INV_SLOT_HEIGHT,   0,  0) },
        { InventorySlot.HANDPOS,        new(true,  INV_BAR_DX,     INV_BAR_DY,     BIG_INV_SLOT_WIDTH,     BIG_INV_SLOT_HEIGHT,  0,  0) },
        { InventorySlot.SECONDHANDPOS,  new(true,  INV_BAR_DX,     INV_BAR_DY,     BIG_INV_SLOT_WIDTH,     BIG_INV_SLOT_HEIGHT,  0,  0) },
        { InventorySlot.BIGPOCK1POS,    new(true,  INV_BAR_DX,     INV_BAR_DY,     BIG_INV_SLOT_WIDTH,     BIG_INV_SLOT_HEIGHT,  0,  0) },
        { InventorySlot.BIGPOCK2POS,    new(true,  INV_BAR_DX,     INV_BAR_DY,     BIG_INV_SLOT_WIDTH,     BIG_INV_SLOT_HEIGHT,  0,  0) },
        { InventorySlot.BIGPOCK3POS,    new(true,  INV_BAR_DX,     INV_BAR_DY,     BIG_INV_SLOT_WIDTH,     BIG_INV_SLOT_HEIGHT,  0,  0) },
        { InventorySlot.BIGPOCK4POS,    new(true,  INV_BAR_DX,     INV_BAR_DY,     BIG_INV_SLOT_WIDTH,     BIG_INV_SLOT_HEIGHT,  0,  0) },
        { InventorySlot.SMALLPOCK1POS,  new(false, INV_BAR_DX,     INV_BAR_DY,     SM_INV_SLOT_WIDTH,      SM_INV_SLOT_HEIGHT,   0,  0) },
        { InventorySlot.SMALLPOCK2POS,  new(false, INV_BAR_DX,     INV_BAR_DY,     SM_INV_SLOT_WIDTH,      SM_INV_SLOT_HEIGHT,   0,  0) },
        { InventorySlot.SMALLPOCK3POS,  new(false, INV_BAR_DX,     INV_BAR_DY,     SM_INV_SLOT_WIDTH,      SM_INV_SLOT_HEIGHT,   0,  0) },
        { InventorySlot.SMALLPOCK4POS,  new(false, INV_BAR_DX,     INV_BAR_DY,     SM_INV_SLOT_WIDTH,      SM_INV_SLOT_HEIGHT,   0,  0) },
        { InventorySlot.SMALLPOCK5POS,  new(false, INV_BAR_DX,     INV_BAR_DY,     SM_INV_SLOT_WIDTH,      SM_INV_SLOT_HEIGHT,   0,  0) },
        { InventorySlot.SMALLPOCK6POS,  new(false, INV_BAR_DX,     INV_BAR_DY,     SM_INV_SLOT_WIDTH,      SM_INV_SLOT_HEIGHT,   0,  0) },
        { InventorySlot.SMALLPOCK7POS,  new(false, INV_BAR_DX,     INV_BAR_DY,     SM_INV_SLOT_WIDTH,      SM_INV_SLOT_HEIGHT,   0,  0) },
        { InventorySlot.SMALLPOCK8POS,  new(false, INV_BAR_DX,     INV_BAR_DY,     SM_INV_SLOT_WIDTH,      SM_INV_SLOT_HEIGHT,   0,  0) },
    };

    // DEFINES FOR ITEM SLOT SIZES IN PIXELS
    public const int BIG_INV_SLOT_WIDTH = 61;
    public const int BIG_INV_SLOT_HEIGHT = 22;
    public const int SM_INV_SLOT_WIDTH = 30;
    public const int SM_INV_SLOT_HEIGHT = 23;
    public const int VEST_INV_SLOT_WIDTH = 43;
    public const int VEST_INV_SLOT_HEIGHT = 24;
    public const int LEGS_INV_SLOT_WIDTH = 43;
    public const int LEGS_INV_SLOT_HEIGHT = 24;
    public const int HEAD_INV_SLOT_WIDTH = 43;
    public const int HEAD_INV_SLOT_HEIGHT = 24;
}



// A STRUCT USED INTERNALLY FOR INV SLOT REGIONS
public record INV_REGIONS
{
    public INV_REGIONS(
        bool a,
        int b,
        int c,
        int d,
        int e,
        int f,
        int g)
    {
        this.fBigPocket = a;
        this.sBarDx = b;
        this.sBarDy = c;
        this.sWidth = d;
        this.sHeight = e;
        this.sX = f;
        this.sY = g;
    }

    public bool fBigPocket { get; set; }
    public int sBarDx { get; set; }
    public int sBarDy { get; set; }
    public int sWidth { get; set; }
    public int sHeight { get; set; }
    public int sX { get; set; }               // starts at 0, gets set via InitInvSlotInterface()
    public int sY { get; set; }                // starts at 0, gets set via InitInvSlotInterface()
}

// USED TO SETUP REGION POSITIONS, ETC
public struct INV_REGION_DESC
{
    public int sX;
    public int sY;
}

public struct REMOVE_MONEY
{
    public int uiTotalAmount;
    public int uiMoneyRemaining;
    public int uiMoneyRemoving;
}

//enum used for the money buttons
public enum MoneyButtons
{
    M_1000,
    M_100,
    M_10,
    M_DONE,
};

public record INV_HELPTEXT(
    int[] iXPosition,
    int[] iYPosition,
    int[] iWidth,
    string sString1,
    string sString2);

public record MoneyLoc(
    int x,
    int y);

public record INV_DESC_STATS(
    int sX,
    int sY,
    int sValDx);


public record INV_ATTACHXY(
    int sX,
    int sY,
    int sHeight,
    int sWidth,
    int sBarDx,
    int sBarDy);
