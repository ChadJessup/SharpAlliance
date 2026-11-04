using System;
using System.Collections.Generic;
using System.Linq;
using SharpAlliance.Core.Managers;
using SharpAlliance.Core.Managers.VideoSurfaces;
using static SharpAlliance.Core.EnglishText;
using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core.SubSystems.LaptopSubSystem;

// Port of AimSort.c: the page where the player chooses how the A.I.M. members are sorted.
public partial class Laptop
{
    private const FontStyle AIM_SORT_FONT_TITLE = FontStyle.FONT14ARIAL;
    private const FontStyle AIM_SORT_FONT_SORT_TEXT = FontStyle.FONT10ARIAL;

    private const FontColor AIM_SORT_COLOR_SORT_TEXT = AIM_FONT_MCOLOR_WHITE;
    private const FontColor AIM_SORT_SORT_BY_COLOR = (FontColor)146;
    private const FontColor AIM_SORT_LINK_TEXT_COLOR = (FontColor)146;

    private const int AIM_SORT_GAP_BN_ICONS = 60;
    private const int AIM_SORT_CHECKBOX_SIZE = 10;
    private const int AIM_SORT_ON = 0;
    private const int AIM_SORT_OFF = 1;

    private const int AIM_SORT_SORT_BY_X = IMAGE_OFFSET_X + 155;
    private const int AIM_SORT_SORT_BY_Y = IMAGE_OFFSET_Y + 96;
    private const int AIM_SORT_SORT_BY_WIDTH = 190;

    private const int AIM_SORT_TO_MUGSHOTS_X = IMAGE_OFFSET_X + 89;
    private const int AIM_SORT_TO_MUGSHOTS_Y = IMAGE_OFFSET_Y + 184;
    private const int AIM_SORT_TO_MUGSHOTS_SIZE = 54;

    private const int AIM_SORT_TO_STATS_X = AIM_SORT_TO_MUGSHOTS_X;
    private const int AIM_SORT_TO_STATS_Y = AIM_SORT_TO_MUGSHOTS_Y + AIM_SORT_GAP_BN_ICONS;
    private const int AIM_SORT_TO_STATS_SIZE = AIM_SORT_TO_MUGSHOTS_SIZE;

    private const int AIM_SORT_TO_ALUMNI_X = AIM_SORT_TO_MUGSHOTS_X;
    private const int AIM_SORT_TO_ALUMNI_Y = AIM_SORT_TO_STATS_Y + AIM_SORT_GAP_BN_ICONS;
    private const int AIM_SORT_TO_ALUMNI_SIZE = AIM_SORT_TO_MUGSHOTS_SIZE;

    private const int AIM_SORT_AIM_MEMBER_X = AIM_SORT_SORT_BY_X;
    private const int AIM_SORT_AIM_MEMBER_Y = 105 + LAPTOP_SCREEN_WEB_DELTA_Y;
    private const int AIM_SORT_AIM_MEMBER_WIDTH = AIM_SORT_SORT_BY_WIDTH;

    private const int AIM_SORT_SORT_BY_TEXT_X = AIM_SORT_SORT_BY_X + 9;
    private const int AIM_SORT_SORT_BY_TEXT_Y = AIM_SORT_SORT_BY_Y + 8;

    private const int AIM_SORT_PRICE_TEXT_X = AIM_SORT_SORT_BY_X + 22;
    private const int AIM_SORT_PRICE_TEXT_Y = AIM_SORT_SORT_BY_Y + 36;

    private const int AIM_SORT_EXP_TEXT_X = AIM_SORT_PRICE_TEXT_X;
    private const int AIM_SORT_EXP_TEXT_Y = AIM_SORT_PRICE_TEXT_Y + 13;

    private const int AIM_SORT_MARKMNSHP_TEXT_X = AIM_SORT_PRICE_TEXT_X;
    private const int AIM_SORT_MARKMNSHP_TEXT_Y = AIM_SORT_EXP_TEXT_Y + 13;

    private const int AIM_SORT_MEDICAL_X = AIM_SORT_SORT_BY_X + 125;
    private const int AIM_SORT_MEDICAL_Y = AIM_SORT_PRICE_TEXT_Y;

    private const int AIM_SORT_EXPLOSIVES_X = AIM_SORT_MEDICAL_X;
    private const int AIM_SORT_EXPLOSIVES_Y = AIM_SORT_EXP_TEXT_Y;

    private const int AIM_SORT_MECHANICAL_X = AIM_SORT_MEDICAL_X;
    private const int AIM_SORT_MECHANICAL_Y = AIM_SORT_MARKMNSHP_TEXT_Y;

    private const int AIM_SORT_ASC_DESC_WIDTH = 100;
    private const int AIM_SORT_ASCEND_TEXT_X = AIM_SORT_SORT_BY_X + 154 - AIM_SORT_ASC_DESC_WIDTH - 4 + 18;
    private const int AIM_SORT_ASCEND_TEXT_Y = 128 + LAPTOP_SCREEN_WEB_DELTA_Y;

    private const int AIM_SORT_DESCEND_TEXT_X = AIM_SORT_ASCEND_TEXT_X;
    private const int AIM_SORT_DESCEND_TEXT_Y = 141 + LAPTOP_SCREEN_WEB_DELTA_Y;

    private const int AIM_SORT_MUGSHOT_TEXT_X = 266;
    private const int AIM_SORT_MUGSHOT_TEXT_Y = 230 + LAPTOP_SCREEN_WEB_DELTA_Y;

    private const int AIM_SORT_MERC_STATS_TEXT_X = AIM_SORT_MUGSHOT_TEXT_X;
    private const int AIM_SORT_MERC_STATS_TEXT_Y = 293 + LAPTOP_SCREEN_WEB_DELTA_Y;

    private const int AIM_SORT_ALUMNI_TEXT_X = AIM_SORT_MUGSHOT_TEXT_X;
    private const int AIM_SORT_ALUMNI_TEXT_Y = 351 + LAPTOP_SCREEN_WEB_DELTA_Y;

    // indices into AimSortText
    private const int AIM_AIMMEMBERS = 0;
    private const int SORT_BY = 1;
    private const int AIMSORT_PRICE = 2;
    private const int AIMSORT_EXPERIENCE = 3;
    private const int AIMSORT_MARKSMANSHIP = 4;
    private const int AIMSORT_MEDICAL = 5;
    private const int AIMSORT_EXPLOSIVES = 6;
    private const int AIMSORT_MECHANICAL = 7;
    private const int MUGSHOT_INDEX = 8;
    private const int MERCENARY_FILES = 9;
    private const int ALUMNI_GALLERY = 10;
    private const int AIMSORT_ASCENDING = 11;
    private const int AIMSORT_DESCENDING = 12;

    public const int AIM_ASCEND = 6;
    public const int AIM_DESCEND = 7;

    // x,y of each check box: price, experience, marksmanship, medical, explosives, mechanical, ascend, descend
    private static readonly int[] AimSortCheckBoxLoc =
    {
        AIM_SORT_SORT_BY_X + 9, AIM_SORT_SORT_BY_Y + 34,
        AIM_SORT_SORT_BY_X + 9, AIM_SORT_SORT_BY_Y + 47,
        AIM_SORT_SORT_BY_X + 9, AIM_SORT_SORT_BY_Y + 60,
        AIM_SORT_SORT_BY_X + 111, AIM_SORT_SORT_BY_Y + 34,
        AIM_SORT_SORT_BY_X + 111, AIM_SORT_SORT_BY_Y + 47,
        AIM_SORT_SORT_BY_X + 111, AIM_SORT_SORT_BY_Y + 60,
        AIM_SORT_SORT_BY_X + 172, AIM_SORT_SORT_BY_Y + 4,
        AIM_SORT_SORT_BY_X + 172, AIM_SORT_SORT_BY_Y + 17,
    };

    private static int gubCurrentSortMode;
    private static int gubOldSortMode;
    private static int gubCurrentListMode;
    private static int gubOldListMode;

    private static readonly MOUSE_REGION gSelectedToMugShotRegion = new(nameof(gSelectedToMugShotRegion));
    private static readonly MOUSE_REGION gSelectedToStatsRegion = new(nameof(gSelectedToStatsRegion));
    private static readonly MOUSE_REGION gSelectedToArchiveRegion = new(nameof(gSelectedToArchiveRegion));

    // one region per sort check box (0-5 sort modes), then ascend and descend
    private static readonly MOUSE_REGION[] gSelectedSortBoxRegion = CreateRegions(8, nameof(gSelectedSortBoxRegion));

    private static HVOBJECT guiSortByBox = null!;
    private static HVOBJECT guiToAlumni = null!;
    private static HVOBJECT guiToMugShots = null!;
    private static HVOBJECT guiToStats = null!;
    private static HVOBJECT guiSelectLight = null!;

    private static void GameInitAimSort()
    {
        gubCurrentSortMode = 0;
        gubOldSortMode = 0;
        gubCurrentListMode = AIM_DESCEND;
        gubOldListMode = AIM_DESCEND;
    }

    private static void DefineLinkRegion(MOUSE_REGION region, int x, int y, int size, MouseCallback callback)
    {
        MouseSubSystem.MSYS_DefineRegion(region, x, y, x + size, y + size, MSYS_PRIORITY.HIGH, CURSOR.WWW, MSYS_NO_CALLBACK, callback);
        MouseSubSystem.MSYS_AddRegion(ref region);
    }

    public static bool EnterAimSort()
    {
        // Every time into Aim Sort, reset the array.
        for (int i = 0; i < MAX_NUMBER_MERCS; i++)
        {
            AimMercArray[i] = i;
        }

        InitAimDefaults();

        guiSortByBox = video.GetVideoObject(Utils.FilenameForBPP("LAPTOP\\SortBy.sti"));
        guiToAlumni = video.GetVideoObject(MultilanguageGraphicUtils.GetMLGFilename(MLG.TOALUMNI));
        guiToMugShots = video.GetVideoObject(MultilanguageGraphicUtils.GetMLGFilename(MLG.TOMUGSHOTS));
        guiToStats = video.GetVideoObject(MultilanguageGraphicUtils.GetMLGFilename(MLG.TOSTATS));
        guiSelectLight = video.GetVideoObject(Utils.FilenameForBPP("LAPTOP\\SelectLight.sti"));

        // ** Mouse Regions **
        DefineLinkRegion(gSelectedToMugShotRegion, AIM_SORT_TO_MUGSHOTS_X, AIM_SORT_TO_MUGSHOTS_Y, AIM_SORT_TO_MUGSHOTS_SIZE, SelectToMugShotRegionCallBack);
        DefineLinkRegion(gSelectedToStatsRegion, AIM_SORT_TO_STATS_X, AIM_SORT_TO_STATS_Y, AIM_SORT_TO_STATS_SIZE, SelectToStatsRegionCallBack);
        DefineLinkRegion(gSelectedToArchiveRegion, AIM_SORT_TO_ALUMNI_X, AIM_SORT_TO_ALUMNI_Y, AIM_SORT_TO_ALUMNI_SIZE, SelectToArchiveRegionCallBack);

        // the six sort-mode check boxes (the right edge is computed from the label width, as in the original)
        int[] labelIndex = { AIMSORT_PRICE, AIMSORT_EXPERIENCE, AIMSORT_MARKSMANSHIP, AIMSORT_MEDICAL, AIMSORT_EXPLOSIVES, AIMSORT_MECHANICAL };
        int[] labelX = { AIM_SORT_PRICE_TEXT_X, AIM_SORT_PRICE_TEXT_X, AIM_SORT_PRICE_TEXT_X, AIM_SORT_MEDICAL_X, AIM_SORT_MEDICAL_X, AIM_SORT_MEDICAL_X };
        for (int mode = 0; mode < 6; mode++)
        {
            int ubCurNumber = mode * 2;
            int boxX = AimSortCheckBoxLoc[ubCurNumber];
            int boxY = AimSortCheckBoxLoc[ubCurNumber + 1];
            int ubWidth = FontSubSystem.StringPixLength(AimSortText[labelIndex[mode]], AIM_SORT_FONT_SORT_TEXT) + boxX + (labelX[mode] - boxX) - 3;

            MouseSubSystem.MSYS_DefineRegion(
                gSelectedSortBoxRegion[mode],
                boxX,
                boxY,
                ubWidth,
                boxY + AIM_SORT_CHECKBOX_SIZE,
                MSYS_PRIORITY.HIGH,
                MSYS_NO_CURSOR,
                MSYS_NO_CALLBACK,
                SelectSortBoxRegionCallBack);
            MouseSubSystem.MSYS_AddRegion(ref gSelectedSortBoxRegion[mode]);
            MouseSubSystem.MSYS_SetRegionUserData(gSelectedSortBoxRegion[mode], 0, mode);
        }

        // ascend / descend check boxes
        for (int list = AIM_ASCEND; list <= AIM_DESCEND; list++)
        {
            int ubCurNumber = list * 2;
            int label = list == AIM_ASCEND ? AIMSORT_ASCENDING : AIMSORT_DESCENDING;
            int left = AimSortCheckBoxLoc[ubCurNumber] - FontSubSystem.StringPixLength(AimSortText[label], AIM_SORT_FONT_SORT_TEXT) - 6;

            MouseSubSystem.MSYS_DefineRegion(
                gSelectedSortBoxRegion[list],
                left,
                AimSortCheckBoxLoc[ubCurNumber + 1],
                AimSortCheckBoxLoc[ubCurNumber] + AIM_SORT_CHECKBOX_SIZE,
                AimSortCheckBoxLoc[ubCurNumber + 1] + AIM_SORT_CHECKBOX_SIZE,
                MSYS_PRIORITY.HIGH,
                MSYS_NO_CURSOR,
                MSYS_NO_CALLBACK,
                SelectSortListRegionCallBack);
            MouseSubSystem.MSYS_AddRegion(ref gSelectedSortBoxRegion[list]);
            MouseSubSystem.MSYS_SetRegionUserData(gSelectedSortBoxRegion[list], 0, list);
        }

        InitAimMenuBar();

        RenderAimSort();

        return true;
    }

    public static void ExitAimSort()
    {
        // Sort the merc array
        SortMercArray();
        RemoveAimDefaults();

        video.DeleteVideoObject(guiSortByBox);
        video.DeleteVideoObject(guiToAlumni);
        video.DeleteVideoObject(guiToMugShots);
        video.DeleteVideoObject(guiToStats);
        video.DeleteVideoObject(guiSelectLight);

        MouseSubSystem.MSYS_RemoveRegion(gSelectedToMugShotRegion);
        MouseSubSystem.MSYS_RemoveRegion(gSelectedToStatsRegion);
        MouseSubSystem.MSYS_RemoveRegion(gSelectedToArchiveRegion);

        foreach (MOUSE_REGION region in gSelectedSortBoxRegion)
        {
            MouseSubSystem.MSYS_RemoveRegion(region);
        }

        ExitAimMenuBar();
    }

    public static void HandleAimSort()
    {
    }

    private static void DrawSortText(int textIndex, int x, int y, int width, FontStyle font, FontColor color, TextJustifies justification)
        => FontSubSystem.DrawTextToScreen(AimSortText[textIndex], new(x, y), width, font, color, FontColor.FONT_MCOLOR_BLACK, justification);

    public static void RenderAimSort()
    {
        DrawAimDefaults();

        video.BltVideoObject(SurfaceType.FRAME_BUFFER, guiSortByBox, 0, AIM_SORT_SORT_BY_X, AIM_SORT_SORT_BY_Y, VO_BLT.SRCTRANSPARENCY);
        video.BltVideoObject(SurfaceType.FRAME_BUFFER, guiToMugShots, 0, AIM_SORT_TO_MUGSHOTS_X, AIM_SORT_TO_MUGSHOTS_Y, VO_BLT.SRCTRANSPARENCY);
        video.BltVideoObject(SurfaceType.FRAME_BUFFER, guiToStats, 0, AIM_SORT_TO_STATS_X, AIM_SORT_TO_STATS_Y, VO_BLT.SRCTRANSPARENCY);
        video.BltVideoObject(SurfaceType.FRAME_BUFFER, guiToAlumni, 0, AIM_SORT_TO_ALUMNI_X, AIM_SORT_TO_ALUMNI_Y, VO_BLT.SRCTRANSPARENCY);

        // Draw the aim slogan under the symbol
        DisplayAimSlogan();

        DrawSortText(AIM_AIMMEMBERS, AIM_SORT_AIM_MEMBER_X, AIM_SORT_AIM_MEMBER_Y, AIM_SORT_AIM_MEMBER_WIDTH, AIM_MAINTITLE_FONT, AIM_MAINTITLE_COLOR, TextJustifies.CENTER_JUSTIFIED);
        DrawSortText(SORT_BY, AIM_SORT_SORT_BY_TEXT_X, AIM_SORT_SORT_BY_TEXT_Y, 0, AIM_SORT_FONT_TITLE, AIM_SORT_SORT_BY_COLOR, TextJustifies.LEFT_JUSTIFIED);

        // all the sort by text
        DrawSortText(AIMSORT_PRICE, AIM_SORT_PRICE_TEXT_X, AIM_SORT_PRICE_TEXT_Y, 0, AIM_SORT_FONT_SORT_TEXT, AIM_SORT_COLOR_SORT_TEXT, TextJustifies.LEFT_JUSTIFIED);
        DrawSortText(AIMSORT_EXPERIENCE, AIM_SORT_EXP_TEXT_X, AIM_SORT_EXP_TEXT_Y, 0, AIM_SORT_FONT_SORT_TEXT, AIM_SORT_COLOR_SORT_TEXT, TextJustifies.LEFT_JUSTIFIED);
        DrawSortText(AIMSORT_MARKSMANSHIP, AIM_SORT_MARKMNSHP_TEXT_X, AIM_SORT_MARKMNSHP_TEXT_Y, 0, AIM_SORT_FONT_SORT_TEXT, AIM_SORT_COLOR_SORT_TEXT, TextJustifies.LEFT_JUSTIFIED);
        DrawSortText(AIMSORT_MEDICAL, AIM_SORT_MEDICAL_X, AIM_SORT_MEDICAL_Y, 0, AIM_SORT_FONT_SORT_TEXT, AIM_SORT_COLOR_SORT_TEXT, TextJustifies.LEFT_JUSTIFIED);
        DrawSortText(AIMSORT_EXPLOSIVES, AIM_SORT_EXPLOSIVES_X, AIM_SORT_EXPLOSIVES_Y, 0, AIM_SORT_FONT_SORT_TEXT, AIM_SORT_COLOR_SORT_TEXT, TextJustifies.LEFT_JUSTIFIED);
        DrawSortText(AIMSORT_MECHANICAL, AIM_SORT_MECHANICAL_X, AIM_SORT_MECHANICAL_Y, 0, AIM_SORT_FONT_SORT_TEXT, AIM_SORT_COLOR_SORT_TEXT, TextJustifies.LEFT_JUSTIFIED);

        DrawSortText(AIMSORT_ASCENDING, AIM_SORT_ASCEND_TEXT_X, AIM_SORT_ASCEND_TEXT_Y, AIM_SORT_ASC_DESC_WIDTH, AIM_SORT_FONT_SORT_TEXT, AIM_SORT_COLOR_SORT_TEXT, TextJustifies.RIGHT_JUSTIFIED);
        DrawSortText(AIMSORT_DESCENDING, AIM_SORT_DESCEND_TEXT_X, AIM_SORT_DESCEND_TEXT_Y, AIM_SORT_ASC_DESC_WIDTH, AIM_SORT_FONT_SORT_TEXT, AIM_SORT_COLOR_SORT_TEXT, TextJustifies.RIGHT_JUSTIFIED);

        // text for the 3 icons
        DrawSortText(MUGSHOT_INDEX, AIM_SORT_MUGSHOT_TEXT_X, AIM_SORT_MUGSHOT_TEXT_Y, 0, AIM_SORT_FONT_SORT_TEXT, AIM_SORT_LINK_TEXT_COLOR, TextJustifies.LEFT_JUSTIFIED);
        DrawSortText(MERCENARY_FILES, AIM_SORT_MERC_STATS_TEXT_X, AIM_SORT_MERC_STATS_TEXT_Y, 0, AIM_SORT_FONT_SORT_TEXT, AIM_SORT_LINK_TEXT_COLOR, TextJustifies.LEFT_JUSTIFIED);
        DrawSortText(ALUMNI_GALLERY, AIM_SORT_ALUMNI_TEXT_X, AIM_SORT_ALUMNI_TEXT_Y, 0, AIM_SORT_FONT_SORT_TEXT, AIM_SORT_LINK_TEXT_COLOR, TextJustifies.LEFT_JUSTIFIED);

        DrawSelectLight(gubCurrentSortMode, AIM_SORT_ON);
        DrawSelectLight(gubCurrentListMode, AIM_SORT_ON);

        DisableAimButton();

        ButtonSubSystem.MarkButtonsDirty(buttonList);

        RenderWWWProgramTitleBar();

        video.InvalidateRegion(LAPTOP_SCREEN_UL_X, LAPTOP_SCREEN_WEB_UL_Y, LAPTOP_SCREEN_LR_X, LAPTOP_SCREEN_WEB_LR_Y);
    }

    private static void SelectToMugShotRegionCallBack(ref MOUSE_REGION pRegion, MSYS_CALLBACK_REASON iReason)
    {
        if (iReason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP))
        {
            guiCurrentLaptopMode = LAPTOP_MODE.AIM_MEMBERS_FACIAL_INDEX;
        }
    }

    private static void SelectToStatsRegionCallBack(ref MOUSE_REGION pRegion, MSYS_CALLBACK_REASON iReason)
    {
        if (iReason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP))
        {
            guiCurrentLaptopMode = LAPTOP_MODE.AIM_MEMBERS;
        }
    }

    private static void SelectToArchiveRegionCallBack(ref MOUSE_REGION pRegion, MSYS_CALLBACK_REASON iReason)
    {
        if (iReason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP))
        {
            guiCurrentLaptopMode = LAPTOP_MODE.AIM_MEMBERS_ARCHIVES;
        }
    }

    // price / experience / marksmanship / medical / explosives / mechanical check boxes
    private static void SelectSortBoxRegionCallBack(ref MOUSE_REGION pRegion, MSYS_CALLBACK_REASON iReason)
    {
        if (!iReason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP))
        {
            return;
        }

        int mode = (int)MouseSubSystem.MSYS_GetRegionUserData(ref pRegion, 0);
        if (gubCurrentSortMode != mode)
        {
            gubCurrentSortMode = mode;
            DrawSelectLight(gubCurrentSortMode, AIM_SORT_ON);
            DrawSelectLight(gubOldSortMode, AIM_SORT_OFF);
            gubOldSortMode = gubCurrentSortMode;
        }
    }

    // ascending / descending check boxes
    private static void SelectSortListRegionCallBack(ref MOUSE_REGION pRegion, MSYS_CALLBACK_REASON iReason)
    {
        if (!iReason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP))
        {
            return;
        }

        int list = (int)MouseSubSystem.MSYS_GetRegionUserData(ref pRegion, 0);
        if (gubCurrentListMode != list)
        {
            gubCurrentListMode = list;
            DrawSelectLight(gubCurrentListMode, AIM_SORT_ON);
            DrawSelectLight(gubOldListMode, AIM_SORT_OFF);
            gubOldListMode = gubCurrentListMode;
        }
    }

    private static void DrawSelectLight(int ubMode, int ubImage)
    {
        ubMode *= 2;

        video.BltVideoObject(SurfaceType.FRAME_BUFFER, guiSelectLight, ubImage, AimSortCheckBoxLoc[ubMode], AimSortCheckBoxLoc[ubMode + 1], VO_BLT.SRCTRANSPARENCY);

        video.InvalidateRegion(AimSortCheckBoxLoc[ubMode], AimSortCheckBoxLoc[ubMode + 1], AimSortCheckBoxLoc[ubMode] + AIM_SORT_CHECKBOX_SIZE, AimSortCheckBoxLoc[ubMode + 1] + AIM_SORT_CHECKBOX_SIZE);
    }

    private static bool SortMercArray()
    {
        // qsort is not stable but OrderBy is; ties keep their profile order
        int[] sorted = AimMercArray.OrderBy(m => m, Comparer<int>.Create(QsortCompare)).ToArray();
        Array.Copy(sorted, AimMercArray, sorted.Length);

        return true;
    }

    private static int QsortCompare(int num1, int num2)
    {
        MERCPROFILESTRUCT a = gMercProfiles[(NPCID)num1];
        MERCPROFILESTRUCT b = gMercProfiles[(NPCID)num2];

        return gubCurrentSortMode switch
        {
            0 => CompareValue(a.uiWeeklySalary, b.uiWeeklySalary),
            1 => CompareValue(a.bExpLevel, b.bExpLevel),
            2 => CompareValue(a.bMarksmanship, b.bMarksmanship),
            3 => CompareValue(a.bMedical, b.bMedical),
            4 => CompareValue(a.bExplosive, b.bExplosive),
            5 => CompareValue(a.bMechanical, b.bMechanical),
            _ => 0,
        };
    }

    private static int CompareValue(int num1, int num2)
    {
        if (gubCurrentListMode == AIM_ASCEND)
        {
            return num1.CompareTo(num2);
        }

        if (gubCurrentListMode == AIM_DESCEND)
        {
            return num2.CompareTo(num1);
        }

        return 0;
    }
}
