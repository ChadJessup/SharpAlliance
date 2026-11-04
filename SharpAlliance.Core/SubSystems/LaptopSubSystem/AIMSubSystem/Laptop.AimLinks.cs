using System;
using SharpAlliance.Core.Managers;
using SharpAlliance.Core.Managers.VideoSurfaces;
using static SharpAlliance.Core.EnglishText;
using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core.SubSystems.LaptopSubSystem;

// Port of AimLinks.c: the A.I.M. "Links" page with banners for Bobby Ray's, the mortuary and the insurance company.
public partial class Laptop
{
    private const FontStyle AIM_LINK_TITLE_FONT = FontStyle.FONT14ARIAL;
    private const FontColor AIM_LINK_TITLE_COLOR = AIM_GREEN;

    private const int AIM_LINK_NUM_LINKS = 3;

    private const int AIM_LINK_LINK_OFFSET_Y = 94;

    private const int AIM_LINK_LINK_WIDTH = 420;
    private const int AIM_LINK_LINK_HEIGHT = 70;

    private const int AIM_LINK_BOBBY_LINK_X = LAPTOP_SCREEN_UL_X + 40;
    private const int AIM_LINK_BOBBY_LINK_Y = LAPTOP_SCREEN_WEB_UL_Y + 91;

    private const int AIM_LINK_FUNERAL_LINK_X = AIM_LINK_BOBBY_LINK_X;
    private const int AIM_LINK_FUNERAL_LINK_Y = AIM_LINK_BOBBY_LINK_Y + AIM_LINK_LINK_OFFSET_Y;

    private const int AIM_LINK_INSURANCE_LINK_X = AIM_LINK_BOBBY_LINK_X;
    private const int AIM_LINK_INSURANCE_LINK_Y = AIM_LINK_FUNERAL_LINK_Y + AIM_LINK_LINK_OFFSET_Y;

    private const int AIM_LINK_TITLE_X = IMAGE_OFFSET_X + 149;
    private const int AIM_LINK_TITLE_Y = AIM_SYMBOL_Y + AIM_SYMBOL_HEIGHT + 10;
    private const int AIM_LINK_TITLE_WIDTH = AIM_SYMBOL_WIDTH;

    private static HVOBJECT guiBobbyLink = null!;
    private static HVOBJECT guiFuneralLink = null!;
    private static HVOBJECT guiInsuranceLink = null!;

    // the web page each banner leads to
    private static readonly BOOKMARK[] gubLinkPages =
    {
        BOOKMARK.BOBBYR_BOOKMARK,
        BOOKMARK.FUNERAL_BOOKMARK,
        BOOKMARK.INSURANCE_BOOKMARK,
    };

    // clicking on a banner
    private static readonly MOUSE_REGION[] gSelectedLinkRegion = CreateLinkRegions();

    private static MOUSE_REGION[] CreateLinkRegions()
    {
        var regions = new MOUSE_REGION[AIM_LINK_NUM_LINKS];
        for (int i = 0; i < regions.Length; i++)
        {
            regions[i] = new MOUSE_REGION($"gSelectedLinkRegion[{i}]");
        }

        return regions;
    }

    public static bool EnterAimLinks()
    {
        InitAimDefaults();
        InitAimMenuBar();

        // load the Bobby link graphic and add it
        guiBobbyLink = video.GetVideoObject(MultilanguageGraphicUtils.GetMLGFilename(MLG.BOBBYRAYLINK));

        // load the Funeral graphic and add it
        guiFuneralLink = video.GetVideoObject(MultilanguageGraphicUtils.GetMLGFilename(MLG.MORTUARYLINK));

        // load the Insurance graphic and add it
        guiInsuranceLink = video.GetVideoObject(MultilanguageGraphicUtils.GetMLGFilename(MLG.INSURANCELINK));

        int usPosY = AIM_LINK_BOBBY_LINK_Y;
        for (int i = 0; i < AIM_LINK_NUM_LINKS; i++)
        {
            MouseSubSystem.MSYS_DefineRegion(
                gSelectedLinkRegion[i],
                AIM_LINK_BOBBY_LINK_X,
                usPosY,
                AIM_LINK_BOBBY_LINK_X + AIM_LINK_LINK_WIDTH,
                usPosY + AIM_LINK_LINK_HEIGHT,
                MSYS_PRIORITY.HIGH,
                CURSOR.WWW,
                MSYS_NO_CALLBACK,
                SelectLinkRegionCallBack);
            MouseSubSystem.MSYS_AddRegion(ref gSelectedLinkRegion[i]);
            MouseSubSystem.MSYS_SetRegionUserData(gSelectedLinkRegion[i], 0, gubLinkPages[i]);
            usPosY += AIM_LINK_LINK_OFFSET_Y;
        }

        RenderAimLinks();
        return true;
    }

    public static void ExitAimLinks()
    {
        RemoveAimDefaults();

        video.DeleteVideoObject(guiBobbyLink);
        video.DeleteVideoObject(guiFuneralLink);
        video.DeleteVideoObject(guiInsuranceLink);

        for (int i = 0; i < AIM_LINK_NUM_LINKS; i++)
        {
            MouseSubSystem.MSYS_RemoveRegion(gSelectedLinkRegion[i]);
        }

        ExitAimMenuBar();
    }

    public static void HandleAimLinks()
    {
    }

    public static void RenderAimLinks()
    {
        DrawAimDefaults();
        DisableAimButton();

        video.BltVideoObject(SurfaceType.FRAME_BUFFER, guiBobbyLink, 0, AIM_LINK_BOBBY_LINK_X, AIM_LINK_BOBBY_LINK_Y, VO_BLT.SRCTRANSPARENCY);
        video.BltVideoObject(SurfaceType.FRAME_BUFFER, guiFuneralLink, 0, AIM_LINK_FUNERAL_LINK_X, AIM_LINK_FUNERAL_LINK_Y, VO_BLT.SRCTRANSPARENCY);
        video.BltVideoObject(SurfaceType.FRAME_BUFFER, guiInsuranceLink, 0, AIM_LINK_INSURANCE_LINK_X, AIM_LINK_INSURANCE_LINK_Y, VO_BLT.SRCTRANSPARENCY);

        // Draw Link Title
        FontSubSystem.DrawTextToScreen(AimLinkText[0], new(AIM_LINK_TITLE_X, AIM_LINK_TITLE_Y), AIM_LINK_TITLE_WIDTH, AIM_LINK_TITLE_FONT, AIM_LINK_TITLE_COLOR, FontColor.FONT_MCOLOR_BLACK, TextJustifies.CENTER_JUSTIFIED);

        ButtonSubSystem.MarkButtonsDirty(buttonList);
        RenderWWWProgramTitleBar();
        video.InvalidateRegion(LAPTOP_SCREEN_UL_X, LAPTOP_SCREEN_WEB_UL_Y, LAPTOP_SCREEN_LR_X, LAPTOP_SCREEN_WEB_LR_Y);
    }

    private static void SelectLinkRegionCallBack(ref MOUSE_REGION pRegion, MSYS_CALLBACK_REASON iReason)
    {
        if (iReason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP))
        {
            var gNextLaptopPage = (BOOKMARK)MouseSubSystem.MSYS_GetRegionUserData(ref pRegion, 0);

            GoToWebPage(gNextLaptopPage);
        }
    }
}
