using System;
using SharpAlliance.Core.Managers;
using SharpAlliance.Core.Managers.VideoSurfaces;
using SharpAlliance.Core.Screens;
using SixLabors.ImageSharp;
using static SharpAlliance.Core.EnglishText;
using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core.SubSystems.LaptopSubSystem;

// Port of the rest of aim.c: the menu bar along the bottom of every AIM page, the rotating banner ads and
// the click handlers on the AIM home page.
public partial class Laptop
{
    private const int NUM_AIM_SCREENS = 6;

    // Bottom buttons
    private const int NUM_AIM_BOTTOMBUTTONS = 6;
    private const int BOTTOM_BUTTON_START_WIDTH = 75;
    private const int BOTTOM_BUTTON_START_HEIGHT = 18;
    private const int BOTTOM_BUTTON_START_X = LAPTOP_SCREEN_UL_X + 25;
    private const int BOTTOM_BUTTON_START_Y = LAPTOP_SCREEN_WEB_LR_Y - BOTTOM_BUTTON_START_HEIGHT - 3;
    private const int BOTTOM_BUTTON_AMOUNT = NUM_AIM_SCREENS;
    private const int BOTTOM_BUTTON_END_X = BOTTOM_BUTTON_START_X + BOTTOM_BUTTON_START_WIDTH * 6;
    private const int BOTTOM_BUTTON_END_Y = BOTTOM_BUTTON_START_Y + BOTTOM_BUTTON_START_HEIGHT;

    private const FontStyle BOBBYR_UNDER_CONSTRUCTION_AD_FONT = FontStyle.FONT14HUMANIST;
    private const FontColor BOBBYR_UNDER_CONSTRUCTION_AD_COLOR = FontColor.FONT_MCOLOR_DKRED;

    // indices into AimScreenText
    private const int AIM_INFO_6 = 3;
    private const int AIM_INFO_7 = 4;
    private const int AIM_BOBBYR_ADD1 = 5;
    private const int AIM_BOBBYR_ADD2 = 6;

    private static readonly LAPTOP_MODE[] gCurrentAimPage =
    {
        LAPTOP_MODE.AIM,
        LAPTOP_MODE.AIM_MEMBERS_SORTED_FILES,
        LAPTOP_MODE.AIM_MEMBERS_ARCHIVES,
        LAPTOP_MODE.AIM_POLICIES,
        LAPTOP_MODE.AIM_HISTORY,
        LAPTOP_MODE.AIM_LINKS,
    };

    private static readonly GUI_BUTTON[] guiBottomButtons = new GUI_BUTTON[NUM_AIM_SCREENS];
    private static ButtonPic guiBottomButtonImage = null!;

    // -- home page click handlers ---------------------------------------------------------------------------------

    private static void SelectMemberCardRegionCallBack(ref MOUSE_REGION pRegion, MSYS_CALLBACK_REASON iReason)
    {
        if (iReason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP) && !fFirstTimeIn)
        {
            guiCurrentLaptopMode = LAPTOP_MODE.AIM_MEMBERS_SORTED_FILES;
        }
    }

    private static void SelectPoliciesRegionCallBack(ref MOUSE_REGION pRegion, MSYS_CALLBACK_REASON iReason)
    {
        if (iReason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP))
        {
            guiCurrentLaptopMode = LAPTOP_MODE.AIM_POLICIES;
        }
    }

    private static void SelectHistoryRegionCallBack(ref MOUSE_REGION pRegion, MSYS_CALLBACK_REASON iReason)
    {
        if (iReason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP))
        {
            guiCurrentLaptopMode = LAPTOP_MODE.AIM_HISTORY;
        }
    }

    private static void SelectLinksRegionCallBack(ref MOUSE_REGION pRegion, MSYS_CALLBACK_REASON iReason)
    {
        if (iReason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP))
        {
            guiCurrentLaptopMode = LAPTOP_MODE.AIM_LINKS;
        }
    }

    private static void SelectAimLogoRegionCallBack(ref MOUSE_REGION pRegion, MSYS_CALLBACK_REASON iReason)
    {
        if (iReason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP))
        {
            guiCurrentLaptopMode = LAPTOP_MODE.AIM;
        }
    }

    /// <summary>Clicking a banner ad takes the player to the advertised site.</summary>
    private static void SelectBannerRegionCallBack(ref MOUSE_REGION pRegion, MSYS_CALLBACK_REASON iReason)
    {
        if (!iReason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP))
        {
            return;
        }

        switch (gubCurrentAdvertisment)
        {
            case AIM_AD.BOBBY_RAY_AD:
                guiCurrentLaptopMode = LAPTOP_MODE.BOBBY_R;
                break;
            case AIM_AD.FUNERAL_ADS:
                guiCurrentLaptopMode = LAPTOP_MODE.FUNERAL;
                break;
            case AIM_AD.FLOWER_SHOP:
                guiCurrentLaptopMode = LAPTOP_MODE.FLORIST;
                break;
            case AIM_AD.INSURANCE_AD:
                guiCurrentLaptopMode = LAPTOP_MODE.INSURANCE;
                break;
        }
    }

    public static void ExitAIM()
    {
        RemoveAimDefaults();

        video.DeleteVideoObject(guiMemberCard);
        video.DeleteVideoObject(guiPolicies);
        video.DeleteVideoObject(guiLinks);
        video.DeleteVideoObject(guiHistory);
        video.DeleteVideoObject(guiWarning);
        video.DeleteVideoObject(guiFlowerAdvertisement);
        video.DeleteVideoObject(guiAdForAdsImages);
        video.DeleteVideoObject(guiInsuranceAdImages);
        video.DeleteVideoObject(guiFuneralAdImages);
        video.DeleteVideoObject(guiBobbyRAdImages);

        // Remove Mouse Regions
        MouseSubSystem.MSYS_RemoveRegion(gSelectedMemberCardRegion);
        MouseSubSystem.MSYS_RemoveRegion(gSelectedPoliciesRegion);
        MouseSubSystem.MSYS_RemoveRegion(gSelectedLinksRegion);
        MouseSubSystem.MSYS_RemoveRegion(gSelectedHistoryRegion);
        MouseSubSystem.MSYS_RemoveRegion(gSelectedBannerRegion);
    }

    public static void HandleAIM()
    {
        HandleAdAndWarningArea(gfInitAdArea, false);
        gfInitAdArea = false;
    }

    private static bool RemoveAimDefaults()
    {
        video.DeleteVideoObject(guiRustBackGround);
        video.DeleteVideoObject(guiAimSymbol);
        MouseSubSystem.MSYS_RemoveRegion(gSelectedAimLogo);

        return true;
    }

    // -- menu bar ---------------------------------------------------------------------------------------------------

    public static bool InitAimMenuBar()
    {
        int usPosX = BOTTOM_BUTTON_START_X;

        guiBottomButtonImage = ButtonSubSystem.LoadButtonImage("LAPTOP\\BottomButtons2.sti", -1, 0, -1, 1, -1);

        for (int i = 0; i < BOTTOM_BUTTON_AMOUNT; i++)
        {
            guiBottomButtons[i] = ButtonSubSystem.CreateIconAndTextButton(
                guiBottomButtonImage,
                AimBottomMenuText[i],
                FontStyle.FONT10ARIAL,
                AIM_BUTTON_ON_COLOR,
                FontShadow.DEFAULT_SHADOW,
                AIM_BUTTON_OFF_COLOR,
                FontShadow.DEFAULT_SHADOW,
                ButtonTextJustifies.TEXT_CJUSTIFIED,
                new Point(usPosX, BOTTOM_BUTTON_START_Y),
                ButtonFlags.BUTTON_TOGGLE,
                MSYS_PRIORITY.HIGH,
                MouseSubSystem.DefaultMoveCallback,
                BtnAimBottomButtonsCallback);
            ButtonSubSystem.SetButtonCursor(guiBottomButtons[i], CURSOR.LAPTOP_SCREEN);

            ButtonSubSystem.MSYS_SetBtnUserData(guiBottomButtons[i], 0, (int)gCurrentAimPage[i]);
            ButtonSubSystem.MSYS_SetBtnUserData(guiBottomButtons[i], 1, i);

            // the laptop only draws the buttons it has in its list
            buttonList.Add(guiBottomButtons[i]);

            usPosX += BOTTOM_BUTTON_START_WIDTH;
        }

        return true;
    }

    public static bool ExitAimMenuBar()
    {
        ButtonSubSystem.UnloadButtonImage(guiBottomButtonImage);

        for (int i = 0; i < BOTTOM_BUTTON_AMOUNT; i++)
        {
            buttonList.Remove(guiBottomButtons[i]);
            ButtonSubSystem.RemoveButton(guiBottomButtons[i]);
        }

        return true;
    }

    private static void BtnAimBottomButtonsCallback(ref GUI_BUTTON btn, MSYS_CALLBACK_REASON reason)
    {
        gubAimMenuButtonDown = 255;

        if (reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_DWN))
        {
            btn.uiFlags |= ButtonFlags.BUTTON_CLICKED_ON;

            gubAimMenuButtonDown = (int)ButtonSubSystem.MSYS_GetBtnUserData(btn, 1);
            video.InvalidateRegion(BOTTOM_BUTTON_START_X, BOTTOM_BUTTON_START_Y, BOTTOM_BUTTON_END_X, BOTTOM_BUTTON_END_Y);
        }

        if (reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP))
        {
            if (btn.uiFlags.HasFlag(ButtonFlags.BUTTON_CLICKED_ON))
            {
                ResetAimButtons(guiBottomButtons, NUM_AIM_BOTTOMBUTTONS);

                guiCurrentLaptopMode = (LAPTOP_MODE)(int)ButtonSubSystem.MSYS_GetBtnUserData(btn, 0);

                video.InvalidateRegion(BOTTOM_BUTTON_START_X, BOTTOM_BUTTON_START_Y, BOTTOM_BUTTON_END_X, BOTTOM_BUTTON_END_Y);
            }
        }

        if (reason.HasFlag(MSYS_CALLBACK_REASON.LOST_MOUSE))
        {
            btn.uiFlags &= ~ButtonFlags.BUTTON_CLICKED_ON;
            video.InvalidateRegion(BOTTOM_BUTTON_START_X, BOTTOM_BUTTON_START_Y, BOTTOM_BUTTON_END_X, BOTTOM_BUTTON_END_Y);
        }

        DisableAimButton();
    }

    public static void ResetAimButtons(GUI_BUTTON[] buttons, int uNumberOfButtons)
    {
        for (int cnt = 0; cnt < uNumberOfButtons; cnt++)
        {
            buttons[cnt].uiFlags &= ~ButtonFlags.BUTTON_CLICKED_ON;
        }
    }

    /// <summary>Shows the button for the AIM page that is on screen as pressed.</summary>
    public static void DisableAimButton()
    {
        for (int i = 0; i < NUM_AIM_BOTTOMBUTTONS; i++)
        {
            if (gCurrentAimPage[i] == guiCurrentLaptopMode)
            {
                guiBottomButtons[i].uiFlags |= ButtonFlags.BUTTON_CLICKED_ON;
            }
        }
    }

    // -- banner ads -------------------------------------------------------------------------------------------------

    private static uint DisplayFlowerAd_uiLastTime;
    private static int DisplayFlowerAd_ubSubImage;
    private static int DisplayFlowerAd_ubCount;

    private static AIM_AD DisplayFlowerAd(bool fInit, bool fRedraw)
    {
        uint uiCurTime = GetJA2Clock();

        if (fInit)
        {
            DisplayFlowerAd_uiLastTime = 0;
            DisplayFlowerAd_ubSubImage = 0;
            DisplayFlowerAd_ubCount = 0;
            MouseSubSystem.MSYS_EnableRegion(gSelectedBannerRegion);
        }

        if (((uiCurTime - DisplayFlowerAd_uiLastTime) > AIM_FLOWER_AD_DELAY) || fRedraw)
        {
            if (DisplayFlowerAd_ubSubImage == AIM_FLOWER_NUM_SUBIMAGES)
            {
                if (DisplayFlowerAd_ubCount == 0 || fRedraw)
                {
                    // Blit the blue sky frame with text on top
                    video.BltVideoObject(SurfaceType.FRAME_BUFFER, guiFlowerAdvertisement, 0, WARNING_X, WARNING_Y, VO_BLT.SRCTRANSPARENCY);

                    // redraw new mail warning, and create new mail button, if nessacary
                    Emails.fReDrawNewMailFlag = true;

                    // Display Aim Warning Text
                    FontSubSystem.DisplayWrappedString(new(AIM_WARNING_TEXT_X, AIM_WARNING_TEXT_Y), AIM_WARNING_TEXT_WIDTH, 2, FontStyle.FONT14ARIAL, FontColor.FONT_GREEN, AimScreenText[AIM_INFO_6], FontColor.FONT_MCOLOR_BLACK, TextJustifies.CENTER_JUSTIFIED);

                    // Display Aim Warning Text
                    FontSubSystem.SetFontShadow((FontShadow)FontColor.FONT_MCOLOR_WHITE);
                    FontSubSystem.DisplayWrappedString(new(AIM_WARNING_TEXT_X, AIM_FLOWER_LINK_TEXT_Y), AIM_WARNING_TEXT_WIDTH, 2, FontStyle.FONT12ARIAL, (FontColor)2, AimScreenText[AIM_INFO_7], FontColor.FONT_MCOLOR_BLACK, TextJustifies.CENTER_JUSTIFIED);
                    FontSubSystem.SetFontShadow(FontShadow.DEFAULT_SHADOW);

                    video.InvalidateRegion(AIM_AD_TOP_LEFT_X, AIM_AD_TOP_LEFT_Y, AIM_AD_BOTTOM_RIGHT_X, AIM_AD_BOTTOM_RIGHT_Y);
                }

                DisplayFlowerAd_uiLastTime = GetJA2Clock();

                DisplayFlowerAd_ubCount++;
                return DisplayFlowerAd_ubCount > 40 ? AIM_AD.DONE : AIM_AD.NOT_DONE;
            }
            else
            {
                video.BltVideoObject(SurfaceType.FRAME_BUFFER, guiFlowerAdvertisement, DisplayFlowerAd_ubSubImage, WARNING_X, WARNING_Y, VO_BLT.SRCTRANSPARENCY);

                // redraw new mail warning, and create new mail button, if nessacary
                Emails.fReDrawNewMailFlag = true;

                DisplayFlowerAd_ubSubImage++;
            }

            DisplayFlowerAd_uiLastTime = GetJA2Clock();
            video.InvalidateRegion(AIM_AD_TOP_LEFT_X, AIM_AD_TOP_LEFT_Y, AIM_AD_BOTTOM_RIGHT_X, AIM_AD_BOTTOM_RIGHT_Y);
        }

        return AIM_AD.NOT_DONE;
    }

    private static uint DrawWarningBox_uiLastTime;

    private static AIM_AD DrawWarningBox(bool fInit, bool fRedraw)
    {
        uint uiCurTime = GetJA2Clock();

        if (fInit || fRedraw)
        {
            // Warning
            video.BltVideoObject(SurfaceType.FRAME_BUFFER, guiWarning, 0, WARNING_X, WARNING_Y, VO_BLT.SRCTRANSPARENCY);

            int uiStartLoc = AIM_HISTORY_LINE_SIZE * (int)AIM_COPYRIGHT.WARNING_1;
            files.LoadEncryptedDataFromFile(AIMHISTORYFILE, out string sText, uiStartLoc, AIM_HISTORY_LINE_SIZE);

            // Display Aim Warning Text
            FontSubSystem.DisplayWrappedString(new(AIM_WARNING_TEXT_X, AIM_WARNING_TEXT_Y), AIM_WARNING_TEXT_WIDTH, 2, AIM_WARNING_FONT, FontColor.FONT_RED, sText, FontColor.FONT_MCOLOR_BLACK, TextJustifies.CENTER_JUSTIFIED);

            video.InvalidateRegion(AIM_AD_TOP_LEFT_X, AIM_AD_TOP_LEFT_Y, AIM_AD_BOTTOM_RIGHT_X, AIM_AD_BOTTOM_RIGHT_Y);

            // redraw new mail warning, and create new mail button, if nessacary
            Emails.fReDrawNewMailFlag = true;

            if (fInit)
            {
                DrawWarningBox_uiLastTime = uiCurTime;
            }
        }

        return (uiCurTime - DrawWarningBox_uiLastTime) > AIM_WARNING_TIME ? AIM_AD.DONE : AIM_AD.NOT_DONE;
    }

    private static uint DisplayAd_uiLastTime;
    private static int DisplayAd_ubSubImage;
    private static int DisplayAd_ubCount;

    private static AIM_AD DisplayAd(bool fInit, bool fRedraw, int usDelay, int usNumberOfSubImages, HVOBJECT hAdHandle)
    {
        uint uiCurTime = GetJA2Clock();
        AIM_AD ubRetVal = 0;

        if (fInit)
        {
            DisplayAd_uiLastTime = 0;
            DisplayAd_ubSubImage = 0;
            DisplayAd_ubCount = 0;
        }

        if (((uiCurTime - DisplayAd_uiLastTime) > usDelay) || fRedraw)
        {
            if (DisplayAd_ubSubImage == 0)
            {
                if (DisplayAd_ubCount == 0 || fRedraw)
                {
                    // Blit the ad
                    video.BltVideoObject(SurfaceType.FRAME_BUFFER, hAdHandle, 0, WARNING_X, WARNING_Y, VO_BLT.SRCTRANSPARENCY);

                    // redraw new mail warning, and create new mail button, if nessacary
                    Emails.fReDrawNewMailFlag = true;

                    video.InvalidateRegion(AIM_AD_TOP_LEFT_X, AIM_AD_TOP_LEFT_Y, AIM_AD_BOTTOM_RIGHT_X, AIM_AD_BOTTOM_RIGHT_Y);
                }

                DisplayAd_uiLastTime = GetJA2Clock();

                // display first frame longer then rest
                DisplayAd_ubCount++;
                if (DisplayAd_ubCount > 12)
                {
                    DisplayAd_ubCount = 0;
                    DisplayAd_ubSubImage++;
                }

                ubRetVal = AIM_AD.NOT_DONE;
            }
            else if (DisplayAd_ubSubImage == usNumberOfSubImages - 1)
            {
                if (DisplayAd_ubCount == 0 || fRedraw)
                {
                    // Blit the ad
                    video.BltVideoObject(SurfaceType.FRAME_BUFFER, hAdHandle, DisplayAd_ubSubImage, WARNING_X, WARNING_Y, VO_BLT.SRCTRANSPARENCY);

                    // redraw new mail warning, and create new mail button, if nessacary
                    Emails.fReDrawNewMailFlag = true;

                    video.InvalidateRegion(AIM_AD_TOP_LEFT_X, AIM_AD_TOP_LEFT_Y, AIM_AD_BOTTOM_RIGHT_X, AIM_AD_BOTTOM_RIGHT_Y);
                }

                DisplayAd_uiLastTime = GetJA2Clock();

                // display last frame longer then rest
                DisplayAd_ubCount++;
                if (DisplayAd_ubCount > 12)
                {
                    ubRetVal = AIM_AD.DONE;
                }
            }
            else
            {
                video.BltVideoObject(SurfaceType.FRAME_BUFFER, hAdHandle, DisplayAd_ubSubImage, WARNING_X, WARNING_Y, VO_BLT.SRCTRANSPARENCY);

                // redraw new mail warning, and create new mail button, if nessacary
                Emails.fReDrawNewMailFlag = true;

                DisplayAd_ubSubImage++;
            }

            // if the add it to have text on it, then put the text on it.
            HandleTextOnAimAdd(DisplayAd_ubSubImage);

            DisplayAd_uiLastTime = GetJA2Clock();
            video.InvalidateRegion(AIM_AD_TOP_LEFT_X, AIM_AD_TOP_LEFT_Y, AIM_AD_BOTTOM_RIGHT_X, AIM_AD_BOTTOM_RIGHT_Y);
        }

        return ubRetVal;
    }

    private static void HandleTextOnAimAdd(int ubCurSubImage)
    {
        if (gubCurrentAdvertisment != AIM_AD.BOBBY_RAY_AD)
        {
            return;
        }

        if (ubCurSubImage <= AIM_AD_BOBBYR_AD_NUM_DUCK_SUBIMAGES)
        {
            // the first couple of frames
            FontSubSystem.SetFontShadow((FontShadow)2);
            FontSubSystem.DisplayWrappedString(new(AIM_BOBBYR1_LINK_TEXT_X, AIM_BOBBYR1_LINK_TEXT_Y), AIM_WARNING_TEXT_WIDTH, 2, BOBBYR_UNDER_CONSTRUCTION_AD_FONT, BOBBYR_UNDER_CONSTRUCTION_AD_COLOR, AimScreenText[AIM_BOBBYR_ADD1], FontColor.FONT_MCOLOR_BLACK, TextJustifies.LEFT_JUSTIFIED | TextJustifies.INVALIDATE_TEXT);
            FontSubSystem.SetFontShadow(FontShadow.DEFAULT_SHADOW);
        }
        else if (ubCurSubImage >= AIM_AD_BOBBYR_AD__NUM_SUBIMAGES - 5)
        {
            // the last frames
            FontSubSystem.SetFontShadow((FontShadow)2);
            FontSubSystem.DisplayWrappedString(new(AIM_BOBBYR2_LINK_TEXT_X, AIM_BOBBYR2_LINK_TEXT_Y), AIM_WARNING_TEXT_WIDTH, 2, BOBBYR_UNDER_CONSTRUCTION_AD_FONT, BOBBYR_UNDER_CONSTRUCTION_AD_COLOR, AimScreenText[AIM_BOBBYR_ADD2], FontColor.FONT_MCOLOR_BLACK, TextJustifies.LEFT_JUSTIFIED | TextJustifies.INVALIDATE_TEXT);
            FontSubSystem.SetFontShadow(FontShadow.DEFAULT_SHADOW);
        }
    }

    private static uint DisplayBobbyRAd_uiLastTime;
    private static int DisplayBobbyRAd_ubSubImage;
    private static int DisplayBobbyRAd_ubDuckCount;
    private static int DisplayBobbyRAd_ubCount;

    private static AIM_AD DisplayBobbyRAd(bool fInit, bool fRedraw)
    {
        uint uiCurTime = GetJA2Clock();
        AIM_AD ubRetVal = 0;
        int usDelay = AIM_AD_BOBBYR_AD_DELAY;

        if (fInit)
        {
            DisplayBobbyRAd_ubDuckCount = 0;
            DisplayBobbyRAd_uiLastTime = 0;
            DisplayBobbyRAd_ubSubImage = 0;
            DisplayBobbyRAd_ubCount = 0;
        }

        if (((uiCurTime - DisplayBobbyRAd_uiLastTime) > usDelay) || fRedraw)
        {
            // Loop through the first 6 images twice, then start into the later ones
            HVOBJECT hAdHandle = guiBobbyRAdImages;

            // if we are still looping through the first 6 animations
            if (DisplayBobbyRAd_ubDuckCount < 2)
            {
                video.BltVideoObject(SurfaceType.FRAME_BUFFER, hAdHandle, DisplayBobbyRAd_ubSubImage, WARNING_X, WARNING_Y, VO_BLT.SRCTRANSPARENCY);

                DisplayBobbyRAd_ubSubImage++;

                video.InvalidateRegion(AIM_AD_TOP_LEFT_X, AIM_AD_TOP_LEFT_Y, AIM_AD_BOTTOM_RIGHT_X, AIM_AD_BOTTOM_RIGHT_Y);

                // if we do the first set of images
                if (DisplayBobbyRAd_ubSubImage > AIM_AD_BOBBYR_AD_NUM_DUCK_SUBIMAGES)
                {
                    DisplayBobbyRAd_ubDuckCount++;

                    DisplayBobbyRAd_ubSubImage = DisplayBobbyRAd_ubDuckCount < 2
                        ? 0
                        : AIM_AD_BOBBYR_AD_NUM_DUCK_SUBIMAGES + 1;
                }

                ubRetVal = AIM_AD.NOT_DONE;
            }
            else
            {
                // Blit the ad
                video.BltVideoObject(SurfaceType.FRAME_BUFFER, hAdHandle, DisplayBobbyRAd_ubSubImage, WARNING_X, WARNING_Y, VO_BLT.SRCTRANSPARENCY);

                DisplayBobbyRAd_ubSubImage++;

                if (DisplayBobbyRAd_ubSubImage >= AIM_AD_BOBBYR_AD__NUM_SUBIMAGES - 1)
                {
                    // display last frame longer then rest
                    DisplayBobbyRAd_ubCount++;
                    if (DisplayBobbyRAd_ubCount > 12)
                    {
                        ubRetVal = AIM_AD.DONE;
                    }

                    DisplayBobbyRAd_ubSubImage = AIM_AD_BOBBYR_AD__NUM_SUBIMAGES - 1;
                }

                // redraw new mail warning, and create new mail button, if nessacary
                Emails.fReDrawNewMailFlag = true;

                video.InvalidateRegion(AIM_AD_TOP_LEFT_X, AIM_AD_TOP_LEFT_Y, AIM_AD_BOTTOM_RIGHT_X, AIM_AD_BOTTOM_RIGHT_Y);
            }

            // if the add it to have text on it, then put the text on it.
            HandleTextOnAimAdd(DisplayBobbyRAd_ubSubImage);

            DisplayBobbyRAd_uiLastTime = GetJA2Clock();
            video.InvalidateRegion(AIM_AD_TOP_LEFT_X, AIM_AD_TOP_LEFT_Y, AIM_AD_BOTTOM_RIGHT_X, AIM_AD_BOTTOM_RIGHT_Y);
        }

        return ubRetVal;
    }

    /// <summary>Which banner follows the one that just finished, depending on how far into the game we are.</summary>
    public static AIM_AD GetNextAimAd(AIM_AD ubCurrentAd)
    {
        if (ubCurrentAd != AIM_AD.WARNING_BOX)
        {
            return AIM_AD.WARNING_BOX;
        }

        uint uiDay = GameClock.GetWorldDay();

        if (uiDay < AIM_AD_BOBBYR_AD_STARTS)
        {
            // if the player has NOT ever been to drassen
            return LaptopSaveInfo.fBobbyRSiteCanBeAccessed ? AIM_AD.BOBBY_RAY_AD : AIM_AD.FOR_ADS;
        }

        if (uiDay < AIM_AD_DAY_FUNERAL_AD_STARTS)
        {
            return AIM_AD.FUNERAL_ADS;
        }

        if (uiDay < AIM_AD_DAY_FLOWER_AD_STARTS)
        {
            return AIM_AD.FLOWER_SHOP;
        }

        return AIM_AD.INSURANCE_AD;
    }
}
