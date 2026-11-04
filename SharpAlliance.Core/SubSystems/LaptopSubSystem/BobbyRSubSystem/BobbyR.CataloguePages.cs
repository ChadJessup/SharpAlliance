using SharpAlliance.Core.Managers;
using SharpAlliance.Core.Managers.VideoSurfaces;
using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core.SubSystems.LaptopSubSystem.BobbyRSubSystem;

// Ports of BobbyRAmmo.c, BobbyRArmour.c, BobbyRMisc.c and BobbyRUsed.c. They differ from the guns page only in
// their graphics and the item class they list, so they all share the catalogue code in BobbyR.Catalogue.cs.
public partial class BobbyR
{
    private static HVOBJECT guiAmmoBackground = null!;
    private static HVOBJECT guiAmmoGrid = null!;
    private static HVOBJECT guiArmourBackground = null!;
    private static HVOBJECT guiArmourGrid = null!;
    private static HVOBJECT guiMiscBackground = null!;
    private static HVOBJECT guiMiscGrid = null!;
    private static HVOBJECT guiUsedBackground = null!;
    private static HVOBJECT guiUsedGrid = null!;

    // ---- ammo ----

    public static bool EnterBobbyRAmmo()
    {
        guiAmmoBackground = video.GetVideoObject(Utils.FilenameForBPP("LAPTOP\\ammobackground.sti"));
        guiAmmoGrid = video.GetVideoObject(Utils.FilenameForBPP("LAPTOP\\ammogrid.sti"));

        InitBobbyBrTitle();
        SetFirstLastPagesForNew(IC.AMMO);
        InitBobbyMenuBar();

        RenderBobbyRAmmo();
        return true;
    }

    public static void ExitBobbyRAmmo()
    {
        video.DeleteVideoObject(guiAmmoBackground);
        video.DeleteVideoObject(guiAmmoGrid);
        DeleteBobbyMenuBar();
        DeleteBobbyBrTitle();
        DeleteMouseRegionForBigImage();
        giCurrentSubPage = gusCurWeaponIndex;
        guiLastBobbyRayPage = LAPTOP_MODE.BOBBY_R_AMMO;
    }

    public static void HandleBobbyRAmmo()
    {
    }

    public static void RenderBobbyRAmmo()
        => RenderCatalogue(guiAmmoBackground, guiAmmoGrid, IC.AMMO, invalidateAll: false);

    // ---- armour ----

    public static bool EnterBobbyRArmour()
    {
        guiArmourBackground = video.GetVideoObject(Utils.FilenameForBPP("LAPTOP\\armourbackground.sti"));
        guiArmourGrid = video.GetVideoObject(Utils.FilenameForBPP("LAPTOP\\armourgrid.sti"));

        InitBobbyBrTitle();
        SetFirstLastPagesForNew(IC.ARMOUR);
        InitBobbyMenuBar();

        RenderBobbyRArmour();
        return true;
    }

    public static void ExitBobbyRArmour()
    {
        video.DeleteVideoObject(guiArmourBackground);
        video.DeleteVideoObject(guiArmourGrid);
        DeleteBobbyBrTitle();
        DeleteBobbyMenuBar();
        DeleteMouseRegionForBigImage();
        giCurrentSubPage = gusCurWeaponIndex;
        guiLastBobbyRayPage = LAPTOP_MODE.BOBBY_R_ARMOR;
    }

    public static void HandleBobbyRArmour()
    {
    }

    public static void RenderBobbyRArmour()
        => RenderCatalogue(guiArmourBackground, guiArmourGrid, IC.ARMOUR, invalidateAll: false);

    // ---- misc ----

    public static bool EnterBobbyRMisc()
    {
        guiMiscBackground = video.GetVideoObject(Utils.FilenameForBPP("LAPTOP\\miscbackground.sti"));
        guiMiscGrid = video.GetVideoObject(Utils.FilenameForBPP("LAPTOP\\miscgrid.sti"));

        InitBobbyBrTitle();
        SetFirstLastPagesForNew(IC.BOBBY_MISC);
        InitBobbyMenuBar();

        RenderBobbyRMisc();
        return true;
    }

    public static void ExitBobbyRMisc()
    {
        video.DeleteVideoObject(guiMiscBackground);
        video.DeleteVideoObject(guiMiscGrid);
        DeleteBobbyBrTitle();
        DeleteBobbyMenuBar();
        DeleteMouseRegionForBigImage();
        giCurrentSubPage = gusCurWeaponIndex;
        guiLastBobbyRayPage = LAPTOP_MODE.BOBBY_R_MISC;
    }

    public static void HandleBobbyRMisc()
    {
    }

    public static void RenderBobbyRMisc()
        => RenderCatalogue(guiMiscBackground, guiMiscGrid, IC.BOBBY_MISC, invalidateAll: false);

    // ---- used ----

    public static bool EnterBobbyRUsed()
    {
        guiUsedBackground = video.GetVideoObject(Utils.FilenameForBPP("LAPTOP\\usedbackground.sti"));
        guiUsedGrid = video.GetVideoObject(Utils.FilenameForBPP("LAPTOP\\usedgrid.sti"));

        InitBobbyBrTitle();
        SetFirstLastPagesForUsed();
        InitBobbyMenuBar();

        RenderBobbyRUsed();
        return true;
    }

    public static void ExitBobbyRUsed()
    {
        video.DeleteVideoObject(guiUsedBackground);
        video.DeleteVideoObject(guiUsedGrid);
        DeleteBobbyMenuBar();
        DeleteBobbyBrTitle();
        DeleteMouseRegionForBigImage();
        giCurrentSubPage = gusCurWeaponIndex;
        guiLastBobbyRayPage = LAPTOP_MODE.BOBBY_R_USED;
    }

    public static void HandleBobbyRUsed()
    {
    }

    public static void RenderBobbyRUsed()
        => RenderCatalogue(guiUsedBackground, guiUsedGrid, BOBBYR_USED_ITEMS, invalidateAll: false);

    /// <summary>The drawing common to all the catalogue pages.</summary>
    private static void RenderCatalogue(HVOBJECT background, HVOBJECT grid, IC itemClass, bool invalidateAll, bool used = false)
    {
        WebPageTileBackground(BOBBYR_NUM_HORIZONTAL_TILES, BOBBYR_NUM_VERTICAL_TILES, BOBBYR_BACKGROUND_WIDTH, BOBBYR_BACKGROUND_HEIGHT, background);

        // Display title at top of page
        DisplayBobbyRBrTitle();

        // the item grid
        video.BltVideoObject(SurfaceType.FRAME_BUFFER, grid, 0, BOBBYR_GRIDLOC_X, BOBBYR_GRIDLOC_Y, VO_BLT.SRCTRANSPARENCY);

        DisplayItemInfo(itemClass);

        UpdateButtonText(guiCurrentLaptopMode);

        ButtonSubSystem.MarkButtonsDirty(Laptop.buttonList);

        Laptop.RenderWWWProgramTitleBar();

        video.InvalidateRegion(LAPTOP_SCREEN_UL_X, LAPTOP_SCREEN_WEB_UL_Y, LAPTOP_SCREEN_LR_X, LAPTOP_SCREEN_WEB_LR_Y);
    }
}
