using System;
using SharpAlliance.Core.Managers.VideoSurfaces;
using SixLabors.ImageSharp;
using static SharpAlliance.Core.EnglishText;
using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core.SubSystems.LaptopSubSystem;

// Port of funeral.c: McGillicutty's Mortuary web site.
public partial class Laptop
{
    private const FontStyle FUNERAL_SENTENCE_FONT = FontStyle.FONT12ARIAL;
    private const FontColor FUNERAL_SENTENCE_COLOR = (FontColor)2;      //FONT_MCOLOR_WHITE
    private const FontShadow FUNERAL_SENTENCE_SHADOW_COLOR = (FontShadow)FontColor.FONT_MCOLOR_WHITE;   //FONT_MCOLOR_DKWHITE

    private const FontStyle FUNERAL_SMALL_FONT = FontStyle.FONT10ARIAL;

    private const FontStyle FUNERAL_TITLE_FONT = FontStyle.FONT14ARIAL;
    private const FontColor FUNERAL_TITLE_COLOR = FontColor.FONT_MCOLOR_WHITE;
    private const FontShadow FUNERAL_TITLE_SHADOW_COLOR = (FontShadow)FontColor.FONT_MCOLOR_DKWHITE;

    private const FontShadow FUNERAL_RIP_SHADOW_COLOR = (FontShadow)FontColor.FONT_MCOLOR_DKWHITE;

    private const int FUNERAL_MCGILICUTTYS_SIGN_X = LAPTOP_SCREEN_UL_X + 92;
    private const int FUNERAL_MCGILICUTTYS_SIGN_Y = LAPTOP_SCREEN_WEB_UL_Y + 0;

    private const int FUNERAL_MORTUARY_SIGN_X = LAPTOP_SCREEN_UL_X + 58;
    private const int FUNERAL_MORTUARY_SIGN_Y = LAPTOP_SCREEN_WEB_UL_Y + 43;

    private const int FUNERAL_LEFT_COLUMN_X = LAPTOP_SCREEN_UL_X + 0;
    private const int FUNERAL_LEFT_COLUMN_Y = LAPTOP_SCREEN_WEB_UL_Y + 43;

    private const int FUNERAL_RIGHT_COLUMN_X = LAPTOP_SCREEN_UL_X + 442;
    private const int FUNERAL_RIGHT_COLUMN_Y = LAPTOP_SCREEN_WEB_UL_Y + 43;

    private const int FUNERAL_LINK_1_X = LAPTOP_SCREEN_UL_X + 37;
    private const int FUNERAL_LINK_1_Y = LAPTOP_SCREEN_WEB_UL_Y + 329;
    private const int FUNERAL_LINK_1_WIDTH = 85;
    private const int FUNERAL_LINK_1_HEIGHT = 60;

    private const int FUNERAL_LINK_OFFSET_X = 85;
    private const int FUNERAL_NUMBER_OF_LINKS = 5;

    private const int FUNERAL_LINK_TEXT_OFFSET_X = 4;
    private const int FUNERAL_LINK_TEXT_OFFSET_Y = 17;
    private const int FUNERAL_LINK_TEXT_WIDTH = 76;

    private const int FUNERAL_MARBLE_WIDTH = 125;
    private const int FUNERAL_MARBLE_HEIGHT = 100;

    private const int FUNERAL_SENTENCE_WIDTH = 380;

    private const int FUNERAL_SENTENCE_1_X = LAPTOP_SCREEN_UL_X + 60;
    private const int FUNERAL_SENTENCE_1_Y = LAPTOP_SCREEN_WEB_UL_Y + 164;

    private const int FUNERAL_SENTENCE_2_X = FUNERAL_SENTENCE_1_X;
    private const int FUNERAL_SENTENCE_2_Y = LAPTOP_SCREEN_WEB_UL_Y + 198;

    private const int FUNERAL_SENTENCE_3_X = FUNERAL_SENTENCE_1_X;
    private const int FUNERAL_SENTENCE_3_Y = LAPTOP_SCREEN_WEB_UL_Y + 227;

    private const int FUNERAL_SENTENCE_4_X = FUNERAL_SENTENCE_1_X;
    private const int FUNERAL_SENTENCE_4_Y = LAPTOP_SCREEN_WEB_UL_Y + 261;

    private const int FUNERAL_SENTENCE_5_X = FUNERAL_SENTENCE_1_X;
    private const int FUNERAL_SENTENCE_5_Y = LAPTOP_SCREEN_WEB_UL_Y + 303;

    private const int FUNERAL_CLOSED_RIP_SIGN_X = LAPTOP_SCREEN_UL_X + 72;
    private const int FUNERAL_CLOSED_RIP_SIGN_Y = LAPTOP_SCREEN_WEB_UL_Y + 151;
    private const int FUNERAL_CLOSED_WIDTH = 364;
    private const int FUNERAL_CLOSED_HEIGHT = 204;

    private const int FUNERAL_RIP_SENTENCE_WIDTH = 260;

    private const int FUNERAL_RIP_SENTENCE_1_X = FUNERAL_CLOSED_RIP_SIGN_X + 55;
    private const int FUNERAL_RIP_SENTENCE_1_Y = FUNERAL_CLOSED_RIP_SIGN_Y + 98;

    private const int FUNERAL_RIP_SENTENCE_2_X = FUNERAL_RIP_SENTENCE_1_X;
    private const int FUNERAL_RIP_SENTENCE_2_Y = FUNERAL_CLOSED_RIP_SIGN_Y + 162;

    //Image Identifiers
    private static HVOBJECT guiClosedSign = null!;
    private static HVOBJECT guiLeftColumn = null!;
    private static HVOBJECT guiLinkCarving = null!;
    private static HVOBJECT guiMarbleBackground = null!;
    private static HVOBJECT guiMcGillicuttys = null!;
    private static HVOBJECT guiMortuary = null!;
    private static HVOBJECT guiRightColumn = null!;

    //Clicking on Funeral link
    private static readonly MOUSE_REGION[] gSelectedFuneralLinkRegion = CreateFuneralLinkRegions();

    //Clicking on rip sign to make it disappear
    private static MOUSE_REGION gSelectedRipSignRegion = new(nameof(gSelectedRipSignRegion));

    private static MOUSE_REGION[] CreateFuneralLinkRegions()
    {
        var regions = new MOUSE_REGION[FUNERAL_NUMBER_OF_LINKS];
        for (int i = 0; i < regions.Length; i++)
        {
            regions[i] = new($"{nameof(gSelectedFuneralLinkRegion)}-{i}");
        }

        return regions;
    }

    public static void GameInitFuneral()
    {
    }

    private static bool EnterFuneral()
    {
        int usPosX, i;

        // load the Closed graphic and add it
        MultilanguageGraphicUtils.GetMLGFilename(out string closedSign, MLG.CLOSED);
        guiClosedSign = video.GetVideoObject(closedSign);

        // load the Left column graphic and add it
        guiLeftColumn = video.GetVideoObject(Utils.FilenameForBPP("LAPTOP\\LeftColumn.sti"));

        // load the Link carving graphic and add it
        guiLinkCarving = video.GetVideoObject(Utils.FilenameForBPP("LAPTOP\\LinkCarving.sti"));

        // load the Marble graphic and add it
        guiMarbleBackground = video.GetVideoObject(Utils.FilenameForBPP("LAPTOP\\Marble.sti"));

        // load the McGillicuttys sign graphic and add it
        MultilanguageGraphicUtils.GetMLGFilename(out string mcGillicuttys, MLG.MCGILLICUTTYS);
        guiMcGillicuttys = video.GetVideoObject(mcGillicuttys);

        // load the Mortuary  graphic and add it
        MultilanguageGraphicUtils.GetMLGFilename(out string mortuary, MLG.MORTUARY);
        guiMortuary = video.GetVideoObject(mortuary);

        // load the right column graphic and add it
        guiRightColumn = video.GetVideoObject(Utils.FilenameForBPP("LAPTOP\\RightColumn.sti"));

        usPosX = FUNERAL_LINK_1_X;
        for (i = 0; i < FUNERAL_NUMBER_OF_LINKS; i++)
        {
            //Mouse region for the bottom links
            MouseSubSystem.MSYS_DefineRegion(
                gSelectedFuneralLinkRegion[i],
                new Rectangle(usPosX, FUNERAL_LINK_1_Y, FUNERAL_LINK_1_WIDTH, FUNERAL_LINK_1_HEIGHT),
                MSYS_PRIORITY.HIGH,
                CURSOR.WWW,
                MSYS_NO_CALLBACK,
                SelectFuneralLinkRegionCallBack);
            MouseSubSystem.MSYS_AddRegion(ref gSelectedFuneralLinkRegion[i]);
            MouseSubSystem.MSYS_SetRegionUserData(gSelectedFuneralLinkRegion[i], 0, i);

            usPosX += FUNERAL_LINK_OFFSET_X;
        }

        MouseSubSystem.MSYS_DefineRegion(
            gSelectedRipSignRegion,
            new Rectangle(FUNERAL_CLOSED_RIP_SIGN_X, FUNERAL_CLOSED_RIP_SIGN_Y, FUNERAL_CLOSED_WIDTH, FUNERAL_CLOSED_HEIGHT),
            MSYS_PRIORITY.HIGH + 1,
            CURSOR.LAPTOP_SCREEN,
            MSYS_NO_CALLBACK,
            SelectRipSignRegionCallBack);
        MouseSubSystem.MSYS_AddRegion(ref gSelectedRipSignRegion);
        MouseSubSystem.MSYS_DisableRegion(gSelectedRipSignRegion);

        SetBookMark(BOOKMARK.FUNERAL_BOOKMARK);

        return true;
    }

    private static void ExitFuneral()
    {
        video.DeleteVideoObject(guiClosedSign);
        video.DeleteVideoObject(guiLeftColumn);
        video.DeleteVideoObject(guiLinkCarving);
        video.DeleteVideoObject(guiMarbleBackground);
        video.DeleteVideoObject(guiMcGillicuttys);
        video.DeleteVideoObject(guiMortuary);
        video.DeleteVideoObject(guiRightColumn);

        for (int i = 0; i < FUNERAL_NUMBER_OF_LINKS; i++)
        {
            MouseSubSystem.MSYS_RemoveRegion(gSelectedFuneralLinkRegion[i]);
        }

        MouseSubSystem.MSYS_RemoveRegion(gSelectedRipSignRegion);
    }

    private static void HandleFuneral()
    {
    }

    private static void RenderFuneral()
    {
        int i, usPosX, usStringHeight;

        WebPageTileBackground(4, 4, FUNERAL_MARBLE_WIDTH, FUNERAL_MARBLE_HEIGHT, guiMarbleBackground);

        // LeftColumn
        video.BltVideoObject(SurfaceType.FRAME_BUFFER, guiLeftColumn, 0, FUNERAL_LEFT_COLUMN_X, FUNERAL_LEFT_COLUMN_Y, VO_BLT.SRCTRANSPARENCY, null);

        // Mcgillicuttys
        video.BltVideoObject(SurfaceType.FRAME_BUFFER, guiMcGillicuttys, 0, FUNERAL_MCGILICUTTYS_SIGN_X, FUNERAL_MCGILICUTTYS_SIGN_Y, VO_BLT.SRCTRANSPARENCY, null);

        // Mortuary
        video.BltVideoObject(SurfaceType.FRAME_BUFFER, guiMortuary, 0, FUNERAL_MORTUARY_SIGN_X, FUNERAL_MORTUARY_SIGN_Y, VO_BLT.SRCTRANSPARENCY, null);

        // right column
        video.BltVideoObject(SurfaceType.FRAME_BUFFER, guiRightColumn, 0, FUNERAL_RIGHT_COLUMN_X, FUNERAL_RIGHT_COLUMN_Y, VO_BLT.SRCTRANSPARENCY, null);

        // LinkCarving
        usPosX = FUNERAL_LINK_1_X;
        for (i = 0; i < FUNERAL_NUMBER_OF_LINKS; i++)
        {
            video.BltVideoObject(SurfaceType.FRAME_BUFFER, guiLinkCarving, 0, usPosX, FUNERAL_LINK_1_Y, VO_BLT.SRCTRANSPARENCY, null);

            //Calculate the height of the string, as it needs to be vertically centered.
            usStringHeight = WordWrap.IanWrappedStringHeight(
                0, 0, FUNERAL_LINK_TEXT_WIDTH, 2,
                FUNERAL_SENTENCE_FONT, 0, sFuneralString[i + (int)FUNERAL.SEND_FLOWERS],
                0, false, 0);

            FontSubSystem.DisplayWrappedString(
                new(usPosX + FUNERAL_LINK_TEXT_OFFSET_X, FUNERAL_LINK_1_Y + (FUNERAL_LINK_1_HEIGHT - usStringHeight) / 2),
                FUNERAL_LINK_TEXT_WIDTH,
                2,
                FUNERAL_SENTENCE_FONT,
                FUNERAL_TITLE_COLOR,
                sFuneralString[i + (int)FUNERAL.SEND_FLOWERS],
                FontColor.FONT_MCOLOR_BLACK,
                TextJustifies.CENTER_JUSTIFIED);

            usPosX += FUNERAL_LINK_OFFSET_X;
        }

        //display all the sentences

        // sentence 1
        FontSubSystem.DisplayWrappedString(new(FUNERAL_SENTENCE_1_X, FUNERAL_SENTENCE_1_Y), FUNERAL_SENTENCE_WIDTH, 2, FUNERAL_TITLE_FONT, FUNERAL_TITLE_COLOR, sFuneralString[(int)FUNERAL.INTRO_1], FontColor.FONT_MCOLOR_BLACK, TextJustifies.CENTER_JUSTIFIED);

        FontSubSystem.SetFontShadow(FUNERAL_SENTENCE_SHADOW_COLOR);

        // sentence 2
        FontSubSystem.DisplayWrappedString(new(FUNERAL_SENTENCE_2_X, FUNERAL_SENTENCE_2_Y), FUNERAL_SENTENCE_WIDTH, 2, FUNERAL_SENTENCE_FONT, FUNERAL_SENTENCE_COLOR, sFuneralString[(int)FUNERAL.INTRO_2], FontColor.FONT_MCOLOR_BLACK, TextJustifies.CENTER_JUSTIFIED);

        // sentence 3
        FontSubSystem.DisplayWrappedString(new(FUNERAL_SENTENCE_3_X, FUNERAL_SENTENCE_3_Y), FUNERAL_SENTENCE_WIDTH, 2, FUNERAL_SENTENCE_FONT, FUNERAL_SENTENCE_COLOR, sFuneralString[(int)FUNERAL.INTRO_3], FontColor.FONT_MCOLOR_BLACK, TextJustifies.CENTER_JUSTIFIED);

        // sentence 4
        FontSubSystem.DisplayWrappedString(new(FUNERAL_SENTENCE_4_X, FUNERAL_SENTENCE_4_Y), FUNERAL_SENTENCE_WIDTH, 2, FUNERAL_SENTENCE_FONT, FUNERAL_SENTENCE_COLOR, sFuneralString[(int)FUNERAL.INTRO_4], FontColor.FONT_MCOLOR_BLACK, TextJustifies.CENTER_JUSTIFIED);

        // sentence 5
        FontSubSystem.DisplayWrappedString(new(FUNERAL_SENTENCE_5_X, FUNERAL_SENTENCE_5_Y), FUNERAL_SENTENCE_WIDTH, 2, FUNERAL_SENTENCE_FONT, FUNERAL_SENTENCE_COLOR, sFuneralString[(int)FUNERAL.INTRO_5], FontColor.FONT_MCOLOR_BLACK, TextJustifies.CENTER_JUSTIFIED);

        FontSubSystem.SetFontShadow(FontShadow.DEFAULT_SHADOW);

        ButtonSubSystem.MarkButtonsDirty(buttonList);
        RenderWWWProgramTitleBar();
        video.InvalidateRegion(LAPTOP_SCREEN_UL_X, LAPTOP_SCREEN_WEB_UL_Y, LAPTOP_SCREEN_LR_X, LAPTOP_SCREEN_WEB_LR_Y);
    }

    private static void DisplayFuneralRipTombStone()
    {
        // rip tombstone
        // (the original first blits a shadow of the sign, BltVideoObjectOutlineShadowFromIndex, offset by 5 pixels;
        //  that blitter is not ported yet)
        video.BltVideoObject(SurfaceType.FRAME_BUFFER, guiClosedSign, 0, FUNERAL_CLOSED_RIP_SIGN_X, FUNERAL_CLOSED_RIP_SIGN_Y, VO_BLT.SRCTRANSPARENCY, null);

        FontSubSystem.SetFontShadow(FUNERAL_RIP_SHADOW_COLOR);

        // sentence 10
        FontSubSystem.DisplayWrappedString(new(FUNERAL_RIP_SENTENCE_1_X, FUNERAL_RIP_SENTENCE_1_Y), FUNERAL_RIP_SENTENCE_WIDTH, 2, FUNERAL_SMALL_FONT, FUNERAL_SENTENCE_COLOR, sFuneralString[(int)FUNERAL.OUR_CONDOLENCES], FontColor.FONT_MCOLOR_BLACK, TextJustifies.CENTER_JUSTIFIED);

        // sentence 11
        FontSubSystem.DisplayWrappedString(new(FUNERAL_RIP_SENTENCE_2_X, FUNERAL_RIP_SENTENCE_2_Y), FUNERAL_RIP_SENTENCE_WIDTH, 2, FUNERAL_SMALL_FONT, FUNERAL_SENTENCE_COLOR, sFuneralString[(int)FUNERAL.OUR_SYMPATHIES], FontColor.FONT_MCOLOR_BLACK, TextJustifies.CENTER_JUSTIFIED);

        FontSubSystem.SetFontShadow(FontShadow.DEFAULT_SHADOW);

        video.InvalidateRegion(FUNERAL_CLOSED_RIP_SIGN_X, FUNERAL_CLOSED_RIP_SIGN_Y, FUNERAL_CLOSED_RIP_SIGN_X + FUNERAL_CLOSED_WIDTH + 5, FUNERAL_CLOSED_RIP_SIGN_Y + FUNERAL_CLOSED_HEIGHT + 5);

        //enable the region to make the sign disappear
        MouseSubSystem.MSYS_EnableRegion(gSelectedRipSignRegion);
    }

    private static void SelectFuneralLinkRegionCallBack(ref MOUSE_REGION pRegion, MSYS_CALLBACK_REASON iReason)
    {
        if (iReason.HasFlag(MSYS_CALLBACK_REASON.INIT))
        {
        }
        else if (iReason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP))
        {
            int uiUserData = (int)MouseSubSystem.MSYS_GetRegionUserData(ref pRegion, 0);

            if (uiUserData == 0)
            {
                GoToWebPage(BOOKMARK.FLORIST_BOOKMARK);
            }
            else
            {
                RenderFuneral();
                DisplayFuneralRipTombStone();
            }
        }
        else if (iReason.HasFlag(MSYS_CALLBACK_REASON.RBUTTON_UP))
        {
        }
    }

    private static void SelectRipSignRegionCallBack(ref MOUSE_REGION pRegion, MSYS_CALLBACK_REASON iReason)
    {
        if (iReason.HasFlag(MSYS_CALLBACK_REASON.INIT))
        {
        }
        else if (iReason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP))
        {
            MouseSubSystem.MSYS_DisableRegion(gSelectedRipSignRegion);
            fPausedReDrawScreenFlag = true;
        }
        else if (iReason.HasFlag(MSYS_CALLBACK_REASON.RBUTTON_UP))
        {
        }
    }

    /// <summary>Tiles a web page background graphic over the web area of the laptop screen.</summary>
    public static bool WebPageTileBackground(int ubNumX, int ubNumY, int usWidth, int usHeight, HVOBJECT uiBackgroundIdentifier)
    {
        int uiPosY = LAPTOP_SCREEN_WEB_UL_Y;
        for (int y = 0; y < ubNumY; y++)
        {
            int uiPosX = LAPTOP_SCREEN_UL_X;
            for (int x = 0; x < ubNumX; x++)
            {
                video.BltVideoObject(SurfaceType.FRAME_BUFFER, uiBackgroundIdentifier, 0, uiPosX, uiPosY, VO_BLT.SRCTRANSPARENCY, null);
                uiPosX += usWidth;
            }

            uiPosY += usHeight;
        }

        return true;
    }
}

public enum FUNERAL
{
    INTRO_1,
    INTRO_2,
    INTRO_3,
    INTRO_4,
    INTRO_5,
    SEND_FLOWERS,
    CASKET_URN,
    CREMATION,
    PRE_FUNERAL,
    FUNERAL_ETTIQUETTE,
    OUR_CONDOLENCES,
    OUR_SYMPATHIES,
}
