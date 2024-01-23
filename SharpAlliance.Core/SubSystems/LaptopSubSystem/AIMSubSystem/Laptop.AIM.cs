using System;

namespace SharpAlliance.Core.SubSystems.LaptopSubSystem;

public partial class Laptop
{
    private static bool gfInitAdArea;
    private const int NUM_AIM_HISTORY_PAGES = 5;
    // Aim Screen Handle
    private static HVOBJECT guiAimSymbol;
    private static HVOBJECT guiRustBackGround;
    private static HVOBJECT guiMemberCard;
    private static HVOBJECT guiPolicies;
    private static HVOBJECT guiHistory;
    private static HVOBJECT guiLinks;
    private static HVOBJECT guiWarning;
    private static HVOBJECT guiBottomButton;
    private static HVOBJECT guiBottomButton2;
    private static HVOBJECT guiFlowerAdvertisement;
    private static HVOBJECT guiAdForAdsImages;
    private static HVOBJECT guiInsuranceAdImages;
    private static HVOBJECT guiFuneralAdImages;
    private static HVOBJECT guiBobbyRAdImages;
    private static MOUSE_REGION gSelectedMemberCardRegion = new (nameof(gSelectedMemberCardRegion));
    private static MOUSE_REGION gSelectedPoliciesRegion = new (nameof(gSelectedPoliciesRegion));
    private static MOUSE_REGION gSelectedHistoryRegion = new (nameof(gSelectedHistoryRegion));
    private static MOUSE_REGION gSelectedLinksRegion = new (nameof(gSelectedLinksRegion));
    private static MOUSE_REGION gSelectedBannerRegion = new (nameof(gSelectedBannerRegion));
    private static int gubWarningTimer;
    private static AIM_AD gubCurrentAdvertisment;
    private static int gubAimMenuButtonDown;
    private static bool fFirstTimeIn;
    private static MOUSE_REGION? gSelectedAimLogo;

    // Link Images
    private const int IMAGE_OFFSET_X = LAPTOP_SCREEN_UL_X;//111
    private const int IMAGE_OFFSET_Y = LAPTOP_SCREEN_WEB_UL_Y;//24

    //262, 28
    private const int AIM_SYMBOL_X = IMAGE_OFFSET_X + 149;
    private const int AIM_SYMBOL_Y = IMAGE_OFFSET_Y + 3;
    private const int AIM_SYMBOL_WIDTH = 203;
    private const int AIM_SYMBOL_HEIGHT = 51;


    private const int LINK_SIZE_X = 101;
    private const int LINK_SIZE_Y = 76;
    private const int MEMBERCARD_X = IMAGE_OFFSET_X + 118;
    private const int MEMBERCARD_Y = IMAGE_OFFSET_Y + 190;
    private const int POLICIES_X = IMAGE_OFFSET_X + 284;
    private const int POLICIES_Y = MEMBERCARD_Y;
    private const int HISTORY_X = MEMBERCARD_X;
    private const int HISTORY_Y = IMAGE_OFFSET_Y + 279;
    private const int LINKS_X = POLICIES_X;
    private const int LINKS_Y = HISTORY_Y;
    private const int WARNING_X = IMAGE_OFFSET_X + 126;
    private const int WARNING_Y = IMAGE_OFFSET_Y + 80 - 1;
    private const int MEMBERS_TEXT_Y = MEMBERCARD_Y + 77;
    private const int HISTORY_TEXT_Y = HISTORY_Y + 77;
    private const int POLICIES_TEXT_Y = MEMBERS_TEXT_Y;
    private const int LINK_TEXT_Y = HISTORY_TEXT_Y;
    private const int AIM_WARNING_TEXT_X = WARNING_X + 15;
    private const int AIM_WARNING_TEXT_Y = WARNING_Y + 46;
    private const int AIM_WARNING_TEXT_WIDTH = 220;
    private const int AIM_FLOWER_LINK_TEXT_Y = AIM_WARNING_TEXT_Y + 25;
    private const int AIM_BOBBYR1_LINK_TEXT_X = WARNING_X + 20;
    private const int AIM_BOBBYR1_LINK_TEXT_Y = WARNING_Y + 20;
    private const int AIM_BOBBYR2_LINK_TEXT_X = WARNING_X + 50;
    private const int AIM_BOBBYR2_LINK_TEXT_Y = WARNING_Y + 58;
    private const int AIM_BOBBYR3_LINK_TEXT_X = WARNING_X + 20;
    private const int AIM_BOBBYR3_LINK_TEXT_Y = WARNING_Y + 20;
    private const int AIM_AD_TOP_LEFT_X = WARNING_X;
    private const int AIM_AD_TOP_LEFT_Y = WARNING_Y;
    private const int AIM_AD_BOTTOM_RIGHT_X = AIM_AD_TOP_LEFT_X + 248;
    private const int AIM_AD_BOTTOM_RIGHT_Y = AIM_AD_TOP_LEFT_Y + 110;
    private const int AIM_COPYRIGHT_X = 160;
    private const int AIM_COPYRIGHT_Y = 396 + LAPTOP_SCREEN_WEB_DELTA_Y;
    private const int AIM_COPYRIGHT_WIDTH = 400;
    private const int AIM_COPYRIGHT_GAP = 9;
    private const int AIM_WARNING_TIME = 10000;
    private const int AIM_ADVERTISING_DELAY = 500;
    private const int AIM_FLOWER_AD_DELAY = 150;
    private const int AIM_FLOWER_NUM_SUBIMAGES = 16;
    private const int AIM_AD_FOR_ADS_DELAY = 150;
    private const int AIM_AD_FOR_ADS__NUM_SUBIMAGES = 13;
    private const int AIM_AD_INSURANCE_AD_DELAY = 150;
    private const int AIM_AD_INSURANCE_AD__NUM_SUBIMAGES = 10;
    private const int AIM_AD_FUNERAL_AD_DELAY = 250;
    private const int AIM_AD_FUNERAL_AD__NUM_SUBIMAGES = 9;
    private const int AIM_AD_BOBBYR_AD_STARTS = 2;
    private const int AIM_AD_DAY_FUNERAL_AD_STARTS = 4;
    private const int AIM_AD_DAY_FLOWER_AD_STARTS = 7;
    private const int AIM_AD_DAY_INSURANCE_AD_STARTS = 12;
    private const int AIM_AD_BOBBYR_AD_DELAY = 300;
    private const int AIM_AD_BOBBYR_AD__NUM_SUBIMAGES = 21;
    private const int AIM_AD_BOBBYR_AD_NUM_DUCK_SUBIMAGES = 6;

    private static void GameInitAIM()
    {
        LaptopInitAim();
    }

    private static void LaptopInitAim()
    {
        gfInitAdArea = true;
    }

    private static bool RenderAIMMembersTopLevel()
    {
        InitCreateDeleteAimPopUpBox(AIM_POPUP.DISPLAY, null, null, 0, 0, 0);

        return true;
    }

    private static void InitCreateDeleteAimPopUpBox(AIM_POPUP ubFlag, string? sString1, string? sString2, int usPosX, int usPosY, int ubData)
    {
    }

    public static void EnterAIM()
    {
        gubWarningTimer = 0;
        gubCurrentAdvertisment = AIM_AD.WARNING_BOX;
        LaptopInitAim();

        InitAimDefaults();

        // load the MemberShipcard graphic and add it
        guiMemberCard = video.GetVideoObject(Utils.FilenameForBPP("LAPTOP\\membercard.sti"));

        // load the Policies graphic and add it
        guiPolicies = video.GetVideoObject(Utils.FilenameForBPP("LAPTOP\\Policies.sti"));

        // load the Links graphic and add it
        guiLinks = video.GetVideoObject(Utils.FilenameForBPP("LAPTOP\\Links.sti"));

        // load the History graphic and add it
        guiHistory = video.GetVideoObject(MultilanguageGraphicUtils.GetMLGFilename(MLG.HISTORY));

        // load the Wanring graphic and add it
        guiWarning = video.GetVideoObject(MultilanguageGraphicUtils.GetMLGFilename(MLG.WARNING));

        // load the flower advertisment and add it
        guiFlowerAdvertisement = video.GetVideoObject(Utils.FilenameForBPP("LAPTOP\\flowerad_16.sti"));

        // load the your ad advertisment and add it
        guiAdForAdsImages = video.GetVideoObject(MultilanguageGraphicUtils.GetMLGFilename(MLG.YOURAD13));

        // load the insurance advertisment and add it
        guiInsuranceAdImages = video.GetVideoObject(MultilanguageGraphicUtils.GetMLGFilename(MLG.INSURANCEAD10));

        // load the funeral advertisment and add it
        guiFuneralAdImages = video.GetVideoObject(MultilanguageGraphicUtils.GetMLGFilename(MLG.FUNERALAD9));

        // load the funeral advertisment and add it
        guiBobbyRAdImages = video.GetVideoObject(MultilanguageGraphicUtils.GetMLGFilename(MLG.BOBBYRAYAD21));

        //** Mouse Regions **

        //Mouse region for the MebershipCard
        MouseSubSystem.MSYS_DefineRegion(
            gSelectedMemberCardRegion,
            new(MEMBERCARD_X, MEMBERCARD_Y, (MEMBERCARD_X + LINK_SIZE_X), (MEMBERCARD_Y + LINK_SIZE_Y)),
            MSYS_PRIORITY.HIGH,
            CURSOR.WWW,
            MSYS_NO_CALLBACK,
            SelectMemberCardRegionCallBack);
        MouseSubSystem.MSYS_AddRegion(ref gSelectedMemberCardRegion);

        //Mouse region for the Policies
        MouseSubSystem.MSYS_DefineRegion(
            gSelectedPoliciesRegion,
            new(POLICIES_X, POLICIES_Y, (POLICIES_X + LINK_SIZE_X), (POLICIES_Y + LINK_SIZE_Y)),
            MSYS_PRIORITY.HIGH,
            CURSOR.WWW,
            MSYS_NO_CALLBACK,
            SelectPoliciesRegionCallBack);
        MouseSubSystem.MSYS_AddRegion(ref gSelectedPoliciesRegion);

        //Mouse region for the History
        MouseSubSystem.MSYS_DefineRegion(
            gSelectedHistoryRegion,
            new(HISTORY_X, HISTORY_Y, (HISTORY_X + LINK_SIZE_X), (HISTORY_Y + LINK_SIZE_Y)),
            MSYS_PRIORITY.HIGH,
            CURSOR.WWW,
            MSYS_NO_CALLBACK,
            SelectHistoryRegionCallBack);
        MouseSubSystem.MSYS_AddRegion(ref gSelectedHistoryRegion);

        //Mouse region for the Links
        MouseSubSystem.MSYS_DefineRegion(
            gSelectedLinksRegion,
            new(LINKS_X, LINKS_Y, (LINKS_X + LINK_SIZE_X), (LINKS_Y + LINK_SIZE_Y)),
            MSYS_PRIORITY.HIGH,
            CURSOR.WWW,
            MSYS_NO_CALLBACK,
            SelectLinksRegionCallBack);
        MouseSubSystem.MSYS_AddRegion(ref gSelectedLinksRegion);

        //Mouse region for the Links
        MouseSubSystem.MSYS_DefineRegion(
            gSelectedBannerRegion,
            new(AIM_AD_TOP_LEFT_X,
                AIM_AD_TOP_LEFT_Y,
                AIM_AD_BOTTOM_RIGHT_X,
                AIM_AD_BOTTOM_RIGHT_Y),
            MSYS_PRIORITY.HIGH,
            CURSOR.WWW,
            MSYS_NO_CALLBACK,
            SelectBannerRegionCallBack);

        MouseSubSystem.MSYS_AddRegion(ref gSelectedBannerRegion);

        // disable the region because only certain banners will be 'clickable'
        MouseSubSystem.MSYS_DisableRegion(gSelectedBannerRegion);

        gubAimMenuButtonDown = 255;

        fFirstTimeIn = false;
        RenderAIM();
    }

    private static void SelectHistoryRegionCallBack(ref MOUSE_REGION pRegion, MSYS_CALLBACK_REASON iReason)
    {
        throw new NotImplementedException();
    }

    private static void SelectPoliciesRegionCallBack(ref MOUSE_REGION pRegion, MSYS_CALLBACK_REASON iReason)
    {
        throw new NotImplementedException();
    }

    private static void SelectLinksRegionCallBack(ref MOUSE_REGION pRegion, MSYS_CALLBACK_REASON iReason)
    {
        throw new NotImplementedException();
    }

    private static void SelectBannerRegionCallBack(ref MOUSE_REGION pRegion, MSYS_CALLBACK_REASON iReason)
    {
        throw new NotImplementedException();
    }

    private static void SelectMemberCardRegionCallBack(ref MOUSE_REGION pRegion, MSYS_CALLBACK_REASON iReason)
    {
        throw new NotImplementedException();
    }

    private static void InitAimDefaults()
    {
        // load the Rust bacground graphic and add it
        guiRustBackGround = video.GetVideoObject(Utils.FilenameForBPP("LAPTOP\\rustbackground.sti"));

        // load the Aim Symbol graphic and add it
        guiAimSymbol = video.GetVideoObject(MultilanguageGraphicUtils.GetMLGFilename(MLG.AIMSYMBOL));

        //Mouse region for the Links
        MouseSubSystem.MSYS_DefineRegion(
            gSelectedAimLogo,
            new(AIM_SYMBOL_X, AIM_SYMBOL_Y, AIM_SYMBOL_X + AIM_SYMBOL_WIDTH, AIM_SYMBOL_Y + AIM_SYMBOL_HEIGHT),
            MSYS_PRIORITY.HIGH,
            CURSOR.WWW,
            MSYS_NO_CALLBACK,
            SelectAimLogoRegionCallBack);

        MouseSubSystem.MSYS_AddRegion(ref gSelectedAimLogo);
    }

    private static void SelectAimLogoRegionCallBack(ref MOUSE_REGION pRegion, MSYS_CALLBACK_REASON iReason)
    {
        throw new NotImplementedException();
    }

    public static void ExitAIM() { }
    public static void EnterAimArchives() { }
    public static void ExitAimArchives() { }

}

// Enumerated types used for the Pop Up Box
public enum AIM_POPUP
{
    NOTHING,
    CREATE,
    DISPLAY,
    DELETE,
};

public enum AIM_AD
{
    NOT_DONE,
    DONE,
    WARNING_BOX,
    FOR_ADS,
    BOBBY_RAY_AD,
    FUNERAL_ADS,
    FLOWER_SHOP,
    INSURANCE_AD,
    LAST_AD
};
