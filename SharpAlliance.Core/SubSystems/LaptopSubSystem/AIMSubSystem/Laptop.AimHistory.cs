using System;
using SharpAlliance.Core.Managers;
using SharpAlliance.Core.Managers.VideoSurfaces;
using SixLabors.ImageSharp;
using static SharpAlliance.Core.EnglishText;
using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core.SubSystems.LaptopSubSystem;

// Port of AimHistory.c: the A.I.M. history pages (a table of contents plus five text pages).
public partial class Laptop
{
    private const FontStyle AIM_HISTORY_TITLE_FONT = FontStyle.FONT14ARIAL;
    private const FontColor AIM_HISTORY_TITLE_COLOR = AIM_GREEN;
    private const FontStyle AIM_HISTORY_TEXT_FONT = FontStyle.FONT10ARIAL;
    private const FontColor AIM_HISTORY_TEXT_COLOR = FontColor.FONT_MCOLOR_WHITE;
    private const FontStyle AIM_HISTORY_TOC_TEXT_FONT = FontStyle.FONT12ARIAL;
    private const FontColor AIM_HISTORY_TOC_TEXT_COLOR = FontColor.FONT_MCOLOR_WHITE;
    private const FontStyle AIM_HISTORY_PARAGRAPH_TITLE_FONT = FontStyle.FONT12ARIAL;
    private const FontColor AIM_HISTORY_PARAGRAPH_TITLE_COLOR = FontColor.FONT_MCOLOR_WHITE;

    private const int AIM_HISTORY_MENU_X = LAPTOP_SCREEN_UL_X + 40;
    private const int AIM_HISTORY_MENU_Y = 370 + LAPTOP_SCREEN_WEB_DELTA_Y;
    private const int AIM_HISTORY_MENU_BUTTON_AMOUNT = 4;
    private const int AIM_HISTORY_GAP_X = 40 + BOTTOM_BUTTON_START_WIDTH;
    private const int AIM_HISTORY_MENU_END_X = AIM_HISTORY_MENU_X + AIM_HISTORY_GAP_X * (AIM_HISTORY_MENU_BUTTON_AMOUNT + 2);
    private const int AIM_HISTORY_MENU_END_Y = AIM_HISTORY_MENU_Y + BOTTOM_BUTTON_START_HEIGHT;

    private const int AIM_HISTORY_TEXT_X = IMAGE_OFFSET_X + 149;
    private const int AIM_HISTORY_TEXT_Y = AIM_SYMBOL_Y + AIM_SYMBOL_HEIGHT + 45;
    private const int AIM_HISTORY_TEXT_WIDTH = AIM_SYMBOL_WIDTH;

    private const int AIM_HISTORY_PARAGRAPH_X = LAPTOP_SCREEN_UL_X + 20;
    private const int AIM_HISTORY_SUBTITLE_Y = 150 + LAPTOP_SCREEN_WEB_DELTA_Y;
    private const int AIM_HISTORY_PARAGRAPH_Y = AIM_HISTORY_SUBTITLE_Y + 18;
    private const int AIM_HISTORY_PARAGRAPH_WIDTH = 460;

    private const int AIM_HISTORY_CONTENTBUTTON_X = 259;
    private const int AIM_HISTORY_CONTENTBUTTON_Y = AIM_HISTORY_SUBTITLE_Y;

    private const int AIM_HISTORY_TOC_X = AIM_HISTORY_CONTENTBUTTON_X;
    private const int AIM_HISTORY_TOC_Y = 5;
    private const int AIM_HISTORY_TOC_GAP_Y = 25;

    private const int AIM_HISTORY_SPACE_BETWEEN_PARAGRAPHS = 8;

    // These represent which paragraph they are located in the AimHist.edt file
    private const int IN_THE_BEGINNING = 6;
    private const int THE_ISLAND_METAVIRA = IN_THE_BEGINNING + 3;
    private const int GUS_TARBALLS = THE_ISLAND_METAVIRA + 3;
    private const int WORD_FROM_FOUNDER = GUS_TARBALLS + 3;
    private const int COLONEL_MOHANNED = WORD_FROM_FOUNDER + 2;
    private const int INCORPORATION = COLONEL_MOHANNED + 1;
    private const int DUNN_AND_BRADROAD = INCORPORATION + 3;
    private const int INCORPORATION_3 = DUNN_AND_BRADROAD + 1;

    // indices into AimHistoryText
    private const int AIM_HISTORY_TITLE = 0;
    private const int AIM_HISTORY_PREVIOUS = 1;

    private static HVOBJECT guiContentButton = null!;

    private static int gubCurPageNum;
    private static bool gfInToc = false;
    private static int gubAimHistoryMenuButtonDown = 255;
    private static bool gfExitingAimHistory;

    private static readonly MOUSE_REGION[] gSelectedHistoryTocMenuRegion = CreateRegions(NUM_AIM_HISTORY_PAGES, nameof(gSelectedHistoryTocMenuRegion));
    private static readonly GUI_BUTTON[] guiHistoryMenuButton = new GUI_BUTTON[AIM_HISTORY_MENU_BUTTON_AMOUNT];
    private static ButtonPic guiHistoryMenuButtonImage = null!;

    /// <summary>An array of named mouse regions (C declares these as MOUSE_REGION name[N]).</summary>
    private static MOUSE_REGION[] CreateRegions(int count, string name)
    {
        var regions = new MOUSE_REGION[count];
        for (int i = 0; i < count; i++)
        {
            regions[i] = new MOUSE_REGION($"{name}[{i}]");
        }

        return regions;
    }

    public static bool EnterAimHistory()
    {
        gfExitingAimHistory = false;
        InitAimDefaults();
        InitAimHistoryMenuBar();

        // load the Content Buttons graphic and add it
        guiContentButton = video.GetVideoObject(Utils.FilenameForBPP("LAPTOP\\ContentButton.sti"));

        gubCurPageNum = giCurrentSubPage;
        RenderAimHistory();

        DisableAimHistoryButton();
        gubAimHistoryMenuButtonDown = 255;

        return true;
    }

    public static void ExitAimHistory()
    {
        gfExitingAimHistory = true;
        RemoveAimDefaults();
        ExitAimHistoryMenuBar();

        video.DeleteVideoObject(guiContentButton);
        giCurrentSubPage = gubCurPageNum;

        if (gfInToc)
        {
            ExitTocMenu();
        }
    }

    public static void HandleAimHistory()
    {
    }

    private static string LoadAimHistoryLine(int line)
    {
        files.LoadEncryptedDataFromFile(AIMHISTORYFILE, out string sText, AIM_HISTORY_LINE_SIZE * line, AIM_HISTORY_LINE_SIZE);
        return sText;
    }

    public static void RenderAimHistory()
    {
        DrawAimDefaults();
        DisplayAimSlogan();
        DisplayAimCopyright();

        FontSubSystem.DrawTextToScreen(AimHistoryText[AIM_HISTORY_TITLE], new(AIM_HISTORY_TEXT_X, AIM_HISTORY_TEXT_Y), AIM_HISTORY_TEXT_WIDTH, AIM_HISTORY_TITLE_FONT, AIM_HISTORY_TITLE_COLOR, FontColor.FONT_MCOLOR_BLACK, TextJustifies.CENTER_JUSTIFIED);

        switch (gubCurPageNum)
        {
            // History Page TOC
            case 0:
                InitTocMenu();
                break;

            // Load and Display the begining
            case 1:
                DisplayAimHistoryParagraph(IN_THE_BEGINNING, 2);
                break;

            // Load and Display the island of metavira
            case 2:
                DisplayAimHistoryParagraph(THE_ISLAND_METAVIRA, 2);
                break;

            // Load and Display the gus tarballs
            case 3:
                DisplayAimHistoryParagraph(GUS_TARBALLS, 2);
                break;

            // Load and Display the founder
            case 4:
                DisplayAimHistoryParagraph(WORD_FROM_FOUNDER, 1);

                // display coloniel Mohanned...
                FontSubSystem.DisplayWrappedString(new(AIM_HISTORY_PARAGRAPH_X, 210 + LAPTOP_SCREEN_WEB_DELTA_Y), AIM_HISTORY_PARAGRAPH_WIDTH, 2, AIM_HISTORY_TEXT_FONT, AIM_HISTORY_TEXT_COLOR, LoadAimHistoryLine(COLONEL_MOHANNED), FontColor.FONT_MCOLOR_BLACK, TextJustifies.RIGHT_JUSTIFIED);
                break;

            // Load and Display the incorporation
            case 5:
                DisplayAimHistoryParagraph(INCORPORATION, 2);

                // display dunn and bradbord...
                FontSubSystem.DisplayWrappedString(new(AIM_HISTORY_PARAGRAPH_X, 270 + LAPTOP_SCREEN_WEB_DELTA_Y), AIM_HISTORY_PARAGRAPH_WIDTH, 2, AIM_HISTORY_TEXT_FONT, AIM_HISTORY_TEXT_COLOR, LoadAimHistoryLine(DUNN_AND_BRADROAD), FontColor.FONT_MCOLOR_BLACK, TextJustifies.RIGHT_JUSTIFIED);

                FontSubSystem.DisplayWrappedString(new(AIM_HISTORY_PARAGRAPH_X, 290 + LAPTOP_SCREEN_WEB_DELTA_Y), AIM_HISTORY_PARAGRAPH_WIDTH, 2, AIM_HISTORY_TEXT_FONT, AIM_HISTORY_TEXT_COLOR, LoadAimHistoryLine(INCORPORATION_3), FontColor.FONT_MCOLOR_BLACK, TextJustifies.LEFT_JUSTIFIED);
                break;
        }

        ButtonSubSystem.MarkButtonsDirty(buttonList);

        RenderWWWProgramTitleBar();

        video.InvalidateRegion(LAPTOP_SCREEN_UL_X, LAPTOP_SCREEN_WEB_UL_Y, LAPTOP_SCREEN_LR_X, LAPTOP_SCREEN_WEB_LR_Y);
    }

    private static bool InitAimHistoryMenuBar()
    {
        int usPosX = AIM_HISTORY_MENU_X;

        guiHistoryMenuButtonImage = ButtonSubSystem.LoadButtonImage("LAPTOP\\BottomButtons2.sti", -1, 0, -1, 1, -1);

        for (int i = 0; i < AIM_HISTORY_MENU_BUTTON_AMOUNT; i++)
        {
            guiHistoryMenuButton[i] = ButtonSubSystem.CreateIconAndTextButton(
                guiHistoryMenuButtonImage,
                AimHistoryText[i + AIM_HISTORY_PREVIOUS],
                FontStyle.FONT10ARIAL,
                AIM_BUTTON_ON_COLOR,
                FontShadow.DEFAULT_SHADOW,
                AIM_BUTTON_OFF_COLOR,
                FontShadow.DEFAULT_SHADOW,
                ButtonTextJustifies.TEXT_CJUSTIFIED,
                new Point(usPosX, AIM_HISTORY_MENU_Y),
                ButtonFlags.BUTTON_TOGGLE,
                MSYS_PRIORITY.HIGH,
                MouseSubSystem.DefaultMoveCallback,
                BtnHistoryMenuButtonCallback);
            ButtonSubSystem.SetButtonCursor(guiHistoryMenuButton[i], CURSOR.WWW);
            ButtonSubSystem.MSYS_SetBtnUserData(guiHistoryMenuButton[i], 0, i + 1);

            buttonList.Add(guiHistoryMenuButton[i]);

            usPosX += AIM_HISTORY_GAP_X;
        }

        return true;
    }

    private static bool ExitAimHistoryMenuBar()
    {
        ButtonSubSystem.UnloadButtonImage(guiHistoryMenuButtonImage);

        for (int i = 0; i < AIM_HISTORY_MENU_BUTTON_AMOUNT; i++)
        {
            buttonList.Remove(guiHistoryMenuButton[i]);
            ButtonSubSystem.RemoveButton(guiHistoryMenuButton[i]);
        }

        return true;
    }

    private static bool DisplayAimHistoryParagraph(int ubPageNum, int ubNumParagraphs)
    {
        int usPosY = 0;
        int usNumPixels = 0;

        // title
        FontSubSystem.DrawTextToScreen(LoadAimHistoryLine(ubPageNum), new(AIM_HISTORY_PARAGRAPH_X, AIM_HISTORY_SUBTITLE_Y), 0, AIM_HISTORY_PARAGRAPH_TITLE_FONT, AIM_HISTORY_PARAGRAPH_TITLE_COLOR, FontColor.FONT_MCOLOR_BLACK, TextJustifies.LEFT_JUSTIFIED);

        if (ubNumParagraphs >= 1)
        {
            usPosY = AIM_HISTORY_PARAGRAPH_Y;

            // 1st paragraph
            usNumPixels = FontSubSystem.DisplayWrappedString(new(AIM_HISTORY_PARAGRAPH_X, usPosY), AIM_HISTORY_PARAGRAPH_WIDTH, 2, AIM_HISTORY_TEXT_FONT, AIM_HISTORY_TEXT_COLOR, LoadAimHistoryLine(ubPageNum + 1), FontColor.FONT_MCOLOR_BLACK, TextJustifies.LEFT_JUSTIFIED);
        }

        if (ubNumParagraphs >= 2)
        {
            // 2nd paragraph
            usPosY += usNumPixels + AIM_HISTORY_SPACE_BETWEEN_PARAGRAPHS;
            FontSubSystem.DisplayWrappedString(new(AIM_HISTORY_PARAGRAPH_X, usPosY), AIM_HISTORY_PARAGRAPH_WIDTH, 2, AIM_HISTORY_TEXT_FONT, AIM_HISTORY_TEXT_COLOR, LoadAimHistoryLine(ubPageNum + 2), FontColor.FONT_MCOLOR_BLACK, TextJustifies.LEFT_JUSTIFIED);
        }

        if (ubNumParagraphs >= 3)
        {
            // 3rd paragraph
            usPosY += usNumPixels + AIM_HISTORY_SPACE_BETWEEN_PARAGRAPHS;
            FontSubSystem.DisplayWrappedString(new(AIM_HISTORY_PARAGRAPH_X, usPosY), AIM_HISTORY_PARAGRAPH_WIDTH, 2, AIM_HISTORY_TEXT_FONT, AIM_HISTORY_TEXT_COLOR, LoadAimHistoryLine(ubPageNum + 3), FontColor.FONT_MCOLOR_BLACK, TextJustifies.LEFT_JUSTIFIED);
        }

        return true;
    }

    private static readonly int[] gubHistoryTocLocInFile =
    {
        IN_THE_BEGINNING,
        THE_ISLAND_METAVIRA,
        GUS_TARBALLS,
        WORD_FROM_FOUNDER,
        INCORPORATION,
    };

    private static bool InitTocMenu()
    {
        int usPosY = AIM_HISTORY_CONTENTBUTTON_Y;

        for (int i = 0; i < NUM_AIM_HISTORY_PAGES; i++)
        {
            string sText = LoadAimHistoryLine(gubHistoryTocLocInFile[i]);

            // if the mouse regions havent been inited, init them
            if (!gfInToc)
            {
                // Mouse region for the history toc buttons
                MouseSubSystem.MSYS_DefineRegion(
                    gSelectedHistoryTocMenuRegion[i],
                    AIM_HISTORY_TOC_X,
                    usPosY,
                    AIM_HISTORY_TOC_X + (int)AIM_CONTENTBUTTON_WIDTH,
                    usPosY + (int)AIM_CONTENTBUTTON_HEIGHT,
                    MSYS_PRIORITY.HIGH,
                    CURSOR.WWW,
                    MSYS_NO_CALLBACK,
                    SelectHistoryTocMenuRegionCallBack);
                MouseSubSystem.MSYS_AddRegion(ref gSelectedHistoryTocMenuRegion[i]);
                MouseSubSystem.MSYS_SetRegionUserData(gSelectedHistoryTocMenuRegion[i], 0, i + 1);
            }

            video.BltVideoObject(SurfaceType.FRAME_BUFFER, guiContentButton, 0, AIM_HISTORY_TOC_X, usPosY, VO_BLT.SRCTRANSPARENCY);
            FontSubSystem.DrawTextToScreen(sText, new(AIM_HISTORY_TOC_X, usPosY + AIM_HISTORY_TOC_Y), (int)AIM_CONTENTBUTTON_WIDTH, AIM_HISTORY_TOC_TEXT_FONT, AIM_HISTORY_TOC_TEXT_COLOR, FontColor.FONT_MCOLOR_BLACK, TextJustifies.CENTER_JUSTIFIED);

            usPosY += AIM_HISTORY_TOC_GAP_Y;
        }

        gfInToc = true;
        return true;
    }

    private static bool ExitTocMenu()
    {
        if (gfInToc)
        {
            gfInToc = false;
            for (int i = 0; i < NUM_AIM_HISTORY_PAGES; i++)
            {
                MouseSubSystem.MSYS_RemoveRegion(gSelectedHistoryTocMenuRegion[i]);
            }
        }

        return true;
    }

    private static void SelectHistoryTocMenuRegionCallBack(ref MOUSE_REGION pRegion, MSYS_CALLBACK_REASON iReason)
    {
        if (gfInToc && iReason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP))
        {
            gubCurPageNum = (int)MouseSubSystem.MSYS_GetRegionUserData(ref pRegion, 0);
            ChangingAimHistorySubPage(gubCurPageNum);

            ExitTocMenu();
            ResetAimHistoryButtons();
            DisableAimHistoryButton();
        }
    }

    private static void BtnHistoryMenuButtonCallback(ref GUI_BUTTON btn, MSYS_CALLBACK_REASON reason)
    {
        int ubRetValue = (int)ButtonSubSystem.MSYS_GetBtnUserData(btn, 0);
        gubAimHistoryMenuButtonDown = 255;

        if (reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_DWN))
        {
            btn.uiFlags |= ButtonFlags.BUTTON_CLICKED_ON;

            gubAimHistoryMenuButtonDown = ubRetValue;

            video.InvalidateRegion(AIM_HISTORY_MENU_X, AIM_HISTORY_MENU_Y, AIM_HISTORY_MENU_END_X, AIM_HISTORY_MENU_END_Y);
        }

        if (reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP))
        {
            if (btn.uiFlags.HasFlag(ButtonFlags.BUTTON_CLICKED_ON))
            {
                btn.uiFlags &= ~ButtonFlags.BUTTON_CLICKED_ON;
                ResetAimHistoryButtons();

                if (ubRetValue == 1)
                {
                    // Previous Page
                    if (gubCurPageNum > 0)
                    {
                        gubCurPageNum--;
                        ChangingAimHistorySubPage(gubCurPageNum);

                        ResetAimHistoryButtons();
                    }
                    else
                    {
                        btn.uiFlags |= ButtonFlags.BUTTON_CLICKED_ON;
                    }
                }
                else if (ubRetValue == 2)
                {
                    // Home Page
                    guiCurrentLaptopMode = LAPTOP_MODE.AIM;
                }
                else if (ubRetValue == 3)
                {
                    // Company policies
                    guiCurrentLaptopMode = LAPTOP_MODE.AIM_MEMBERS_ARCHIVES;
                }
                else if (ubRetValue == 4)
                {
                    // Next Page
                    if (gubCurPageNum < NUM_AIM_HISTORY_PAGES)
                    {
                        gubCurPageNum++;
                        ChangingAimHistorySubPage(gubCurPageNum);

                        if (gfInToc)
                        {
                            ExitTocMenu();
                        }
                    }
                    else
                    {
                        btn.uiFlags |= ButtonFlags.BUTTON_CLICKED_ON;
                    }
                }

                DisableAimHistoryButton();

                video.InvalidateRegion(AIM_HISTORY_MENU_X, AIM_HISTORY_MENU_Y, AIM_HISTORY_MENU_END_X, AIM_HISTORY_MENU_END_Y);
            }
        }

        if (reason.HasFlag(MSYS_CALLBACK_REASON.LOST_MOUSE))
        {
            btn.uiFlags &= ~ButtonFlags.BUTTON_CLICKED_ON;

            DisableAimHistoryButton();

            video.InvalidateRegion(AIM_HISTORY_MENU_X, AIM_HISTORY_MENU_Y, AIM_HISTORY_MENU_END_X, AIM_HISTORY_MENU_END_Y);
        }
    }

    private static void ResetAimHistoryButtons()
    {
        for (int i = 0; i < AIM_HISTORY_MENU_BUTTON_AMOUNT; i++)
        {
            guiHistoryMenuButton[i].uiFlags &= ~ButtonFlags.BUTTON_CLICKED_ON;
        }
    }

    private static void DisableAimHistoryButton()
    {
        if (gfExitingAimHistory)
        {
            return;
        }

        if (gubCurPageNum == 0)
        {
            guiHistoryMenuButton[0].uiFlags |= ButtonFlags.BUTTON_CLICKED_ON;
        }
        else if (gubCurPageNum == 5)
        {
            guiHistoryMenuButton[AIM_HISTORY_MENU_BUTTON_AMOUNT - 1].uiFlags |= ButtonFlags.BUTTON_CLICKED_ON;
        }
    }

    private static void ChangingAimHistorySubPage(int ubSubPageNumber)
    {
        fLoadPendingFlag = true;

        if (!AimHistorySubPagesVisitedFlag[ubSubPageNumber])
        {
            fConnectingToSubPage = true;
            fFastLoadFlag = false;

            AimHistorySubPagesVisitedFlag[ubSubPageNumber] = true;
        }
        else
        {
            fConnectingToSubPage = true;
            fFastLoadFlag = true;
        }
    }
}
