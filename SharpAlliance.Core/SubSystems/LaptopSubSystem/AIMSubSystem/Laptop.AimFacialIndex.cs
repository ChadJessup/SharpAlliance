using System;
using System.Linq;
using SharpAlliance.Core.Managers;
using SharpAlliance.Core.Managers.VideoSurfaces;
using SixLabors.ImageSharp;
using static SharpAlliance.Core.EnglishText;
using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core.SubSystems.LaptopSubSystem;

// Port of AimFacialIndex.c: the A.I.M. mug shot index.
public partial class Laptop
{
    public const int MAX_NUMBER_MERCS = 40;

    private const int AIM_FI_NUM_MUHSHOTS_X = 8;
    private const int AIM_FI_NUM_MUHSHOTS_Y = 5;
    private const int AIM_FI_PORTRAIT_WIDTH = 52;
    private const int AIM_FI_PORTRAIT_HEIGHT = 48;
    private const int AIM_FI_FIRST_MUGSHOT_X = IMAGE_OFFSET_X + 6;
    private const int AIM_FI_FIRST_MUGSHOT_Y = IMAGE_OFFSET_Y + 69;
    private const int AIM_FI_MUGSHOT_GAP_X = 10;
    private const int AIM_FI_MUGSHOT_GAP_Y = 13;
    private const int AIM_FI_FACE_OFFSET = 2;
    private const int AIM_FI_NNAME_OFFSET_X = 2;
    private const int AIM_FI_NNAME_OFFSET_Y = AIM_FI_PORTRAIT_HEIGHT + 1;
    private const int AIM_FI_NNAME_WIDTH = AIM_FI_PORTRAIT_WIDTH + 4;
    private const int AIM_FI_MEMBER_TEXT_X = IMAGE_OFFSET_X + 155;
    private const int AIM_FI_MEMBER_TEXT_Y = AIM_SYMBOL_Y + AIM_SYMBOL_HEIGHT + 1;
    private const int AIM_FI_MEMBER_TEXT_WIDTH = 190;
    private const int AIM_FI_AWAY_TEXT_OFFSET_X = 3;
    private const int AIM_FI_AWAY_TEXT_OFFSET_Y = 23;
    private const int AIM_FI_AWAY_TEXT_OFFSET_WIDTH = 48;

    private const int AIM_FI_LEFT_CLICK_TEXT_X = 116;
    private const int AIM_FI_LEFT_CLICK_TEXT_Y = 35 + LAPTOP_SCREEN_WEB_DELTA_Y;
    private const int AIM_FI_CLICK_DESC_TEXT_Y_OFFSET = 16;
    private const int AIM_FI_RIGHT_CLICK_TEXT_X = 500;
    private const int AIM_FI_CLICK_TEXT_WIDTH = 110;
    private const FontStyle AIM_FI_HELP_FONT = FontStyle.FONT10ARIAL;
    private const FontStyle AIM_FI_HELP_TITLE_FONT = FontStyle.FONT12ARIAL;

    // indices into AimFiText
    private const int AIM_FI_AIM_MEMBERS_SORTED_ASCENDING = 6;
    private const int AIM_FI_AIM_MEMBERS_SORTED_DESCENDING = 7;
    private const int AIM_FI_LEFT_CLICK = 8;
    private const int AIM_FI_TO_SELECT = 9;
    private const int AIM_FI_RIGHT_CLICK = 10;
    private const int AIM_FI_TO_ENTER_SORT_PAGE = 11;
    private const int AIM_FI_AWAY = 12;
    private const int AIM_FI_DEAD = 13;

    private const int MERC_FILES_ALREADY_HIRED = 16;

    public const int DEAD_MERC_COLOR_RED = 255;
    public const int DEAD_MERC_COLOR_GREEN = 55;
    public const int DEAD_MERC_COLOR_BLUE = 55;

    /// <summary>The A.I.M. profile ids in the order they are listed (sorted by AimSort).</summary>
    public static int[] AimMercArray = Enumerable.Range(0, MAX_NUMBER_MERCS).ToArray();

    /// <summary>Index into <see cref="AimMercArray"/> of the merc whose file is shown on the members page.</summary>
    public static int gbCurrentIndex;

    private static HVOBJECT guiMugShotBorder = null!;
    private static readonly HVOBJECT[] guiAimFiFace = new HVOBJECT[MAX_NUMBER_MERCS];
    private static readonly MOUSE_REGION[] gMercFaceMouseRegions = CreateRegions(MAX_NUMBER_MERCS, nameof(gMercFaceMouseRegions));
    private static MOUSE_REGION gScreenMouseRegions = new(nameof(gScreenMouseRegions));

    private static void GameInitAimFacialIndex()
    {
    }

    public static bool EnterAimFacialIndex()
    {
        const string sFaceLoc = "FACES\\";

        // load the Portait graphic and add it
        guiMugShotBorder = video.GetVideoObject(Utils.FilenameForBPP("LAPTOP\\MugShotBorder3.sti"));

        int usPosX = AIM_FI_FIRST_MUGSHOT_X;
        int usPosY = AIM_FI_FIRST_MUGSHOT_Y;
        int i = 0;
        for (int y = 0; y < AIM_FI_NUM_MUHSHOTS_Y; y++)
        {
            for (int x = 0; x < AIM_FI_NUM_MUHSHOTS_X; x++)
            {
                MouseSubSystem.MSYS_DefineRegion(
                    gMercFaceMouseRegions[i],
                    usPosX,
                    usPosY,
                    usPosX + AIM_FI_PORTRAIT_WIDTH,
                    usPosY + AIM_FI_PORTRAIT_HEIGHT,
                    MSYS_PRIORITY.HIGH,
                    CURSOR.WWW,
                    SelectMercFaceMoveRegionCallBack,
                    SelectMercFaceRegionCallBack);

                MouseSubSystem.MSYS_AddRegion(ref gMercFaceMouseRegions[i]);
                MouseSubSystem.MSYS_SetRegionUserData(gMercFaceMouseRegions[i], 0, i);

                string sTemp = wprintf("%s%02d.sti", sFaceLoc, AimMercArray[i]);
                guiAimFiFace[i] = video.GetVideoObject(Utils.FilenameForBPP(sTemp));

                usPosX += AIM_FI_PORTRAIT_WIDTH + AIM_FI_MUGSHOT_GAP_X;
                i++;
            }

            usPosX = AIM_FI_FIRST_MUGSHOT_X;
            usPosY += AIM_FI_PORTRAIT_HEIGHT + AIM_FI_MUGSHOT_GAP_Y;
        }

        MouseSubSystem.MSYS_DefineRegion(
            gScreenMouseRegions,
            LAPTOP_SCREEN_UL_X,
            LAPTOP_SCREEN_WEB_UL_Y,
            LAPTOP_SCREEN_LR_X,
            LAPTOP_SCREEN_WEB_LR_Y,
            MSYS_PRIORITY.HIGH - 1,
            CURSOR.LAPTOP_SCREEN,
            MSYS_NO_CALLBACK,
            SelectScreenRegionCallBack);

        MouseSubSystem.MSYS_AddRegion(ref gScreenMouseRegions);

        InitAimMenuBar();
        InitAimDefaults();

        RenderAimFacialIndex();

        return true;
    }

    public static void ExitAimFacialIndex()
    {
        RemoveAimDefaults();

        video.DeleteVideoObject(guiMugShotBorder);

        for (int i = 0; i < MAX_NUMBER_MERCS; i++)
        {
            video.DeleteVideoObject(guiAimFiFace[i]);
            MouseSubSystem.MSYS_RemoveRegion(gMercFaceMouseRegions[i]);
        }

        ExitAimMenuBar();

        MouseSubSystem.MSYS_RemoveRegion(gScreenMouseRegions);
    }

    public static void HandleAimFacialIndex()
    {
    }

    public static bool RenderAimFacialIndex()
    {
        DrawAimDefaults();

        // Display the 'A.I.M. Members Sorted Ascending By Price' type string
        string sString = gubCurrentListMode == AIM_ASCEND
            ? wprintf(AimFiText[AIM_FI_AIM_MEMBERS_SORTED_ASCENDING], AimFiText[gubCurrentSortMode])
            : wprintf(AimFiText[AIM_FI_AIM_MEMBERS_SORTED_DESCENDING], AimFiText[gubCurrentSortMode]);

        FontSubSystem.DrawTextToScreen(sString, new(AIM_FI_MEMBER_TEXT_X, AIM_FI_MEMBER_TEXT_Y), AIM_FI_MEMBER_TEXT_WIDTH, AIM_MAINTITLE_FONT, AIM_MAINTITLE_COLOR, FontColor.FONT_MCOLOR_BLACK, TextJustifies.CENTER_JUSTIFIED);

        // Draw the mug shot border and face
        int usPosX = AIM_FI_FIRST_MUGSHOT_X;
        int usPosY = AIM_FI_FIRST_MUGSHOT_Y;

        int i = 0;
        for (int y = 0; y < AIM_FI_NUM_MUHSHOTS_Y; y++)
        {
            for (int x = 0; x < AIM_FI_NUM_MUHSHOTS_X; x++)
            {
                DrawMercsFaceToScreen(i, usPosX, usPosY, 1);
                FontSubSystem.DrawTextToScreen(gMercProfiles[(NPCID)AimMercArray[i]].zNickname, new(usPosX - AIM_FI_NNAME_OFFSET_X, usPosY + AIM_FI_NNAME_OFFSET_Y), AIM_FI_NNAME_WIDTH, AIM_FONT12ARIAL, AIM_FONT_MCOLOR_WHITE, FontColor.FONT_MCOLOR_BLACK, TextJustifies.CENTER_JUSTIFIED);

                usPosX += AIM_FI_PORTRAIT_WIDTH + AIM_FI_MUGSHOT_GAP_X;
                i++;
            }

            usPosX = AIM_FI_FIRST_MUGSHOT_X;
            usPosY += AIM_FI_PORTRAIT_HEIGHT + AIM_FI_MUGSHOT_GAP_Y;
        }

        DisableAimButton();

        // display the 'left and right click' onscreen help msg
        FontSubSystem.DrawTextToScreen(AimFiText[AIM_FI_LEFT_CLICK], new(AIM_FI_LEFT_CLICK_TEXT_X, AIM_FI_LEFT_CLICK_TEXT_Y), AIM_FI_CLICK_TEXT_WIDTH, AIM_FI_HELP_TITLE_FONT, AIM_FONT_MCOLOR_WHITE, FontColor.FONT_MCOLOR_BLACK, TextJustifies.CENTER_JUSTIFIED);
        FontSubSystem.DrawTextToScreen(AimFiText[AIM_FI_TO_SELECT], new(AIM_FI_LEFT_CLICK_TEXT_X, AIM_FI_LEFT_CLICK_TEXT_Y + AIM_FI_CLICK_DESC_TEXT_Y_OFFSET), AIM_FI_CLICK_TEXT_WIDTH, AIM_FI_HELP_FONT, AIM_FONT_MCOLOR_WHITE, FontColor.FONT_MCOLOR_BLACK, TextJustifies.CENTER_JUSTIFIED);

        FontSubSystem.DrawTextToScreen(AimFiText[AIM_FI_RIGHT_CLICK], new(AIM_FI_RIGHT_CLICK_TEXT_X, AIM_FI_LEFT_CLICK_TEXT_Y), AIM_FI_CLICK_TEXT_WIDTH, AIM_FI_HELP_TITLE_FONT, AIM_FONT_MCOLOR_WHITE, FontColor.FONT_MCOLOR_BLACK, TextJustifies.CENTER_JUSTIFIED);
        FontSubSystem.DrawTextToScreen(AimFiText[AIM_FI_TO_ENTER_SORT_PAGE], new(AIM_FI_RIGHT_CLICK_TEXT_X, AIM_FI_LEFT_CLICK_TEXT_Y + AIM_FI_CLICK_DESC_TEXT_Y_OFFSET), AIM_FI_CLICK_TEXT_WIDTH, AIM_FI_HELP_FONT, AIM_FONT_MCOLOR_WHITE, FontColor.FONT_MCOLOR_BLACK, TextJustifies.CENTER_JUSTIFIED);

        ButtonSubSystem.MarkButtonsDirty(buttonList);

        RenderWWWProgramTitleBar();

        video.InvalidateRegion(LAPTOP_SCREEN_UL_X, LAPTOP_SCREEN_WEB_UL_Y, LAPTOP_SCREEN_LR_X, LAPTOP_SCREEN_WEB_LR_Y);
        return true;
    }

    private static void SelectMercFaceRegionCallBack(ref MOUSE_REGION pRegion, MSYS_CALLBACK_REASON iReason)
    {
        if (iReason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP))
        {
            guiCurrentLaptopMode = LAPTOP_MODE.AIM_MEMBERS;
            gbCurrentIndex = (int)MouseSubSystem.MSYS_GetRegionUserData(ref pRegion, 0);
        }
        else if (iReason.HasFlag(MSYS_CALLBACK_REASON.RBUTTON_UP))
        {
            guiCurrentLaptopMode = LAPTOP_MODE.AIM_MEMBERS_SORTED_FILES;
        }
    }

    private static void SelectScreenRegionCallBack(ref MOUSE_REGION pRegion, MSYS_CALLBACK_REASON iReason)
    {
        if (iReason.HasFlag(MSYS_CALLBACK_REASON.RBUTTON_UP))
        {
            guiCurrentLaptopMode = LAPTOP_MODE.AIM_MEMBERS_SORTED_FILES;
        }
    }

    private static void SelectMercFaceMoveRegionCallBack(ref MOUSE_REGION pRegion, MSYS_CALLBACK_REASON reason)
    {
        int ubMercNum = (int)MouseSubSystem.MSYS_GetRegionUserData(ref pRegion, 0);

        int usPosY = AIM_FI_FIRST_MUGSHOT_Y + ((AIM_FI_PORTRAIT_HEIGHT + AIM_FI_MUGSHOT_GAP_Y) * (ubMercNum / AIM_FI_NUM_MUHSHOTS_X));
        int usPosX = AIM_FI_FIRST_MUGSHOT_X + ((AIM_FI_PORTRAIT_WIDTH + AIM_FI_MUGSHOT_GAP_X) * (ubMercNum % AIM_FI_NUM_MUHSHOTS_X));

        if (reason.HasFlag(MSYS_CALLBACK_REASON.LOST_MOUSE))
        {
            DrawMercsFaceToScreen(ubMercNum, usPosX, usPosY, 1);
            video.InvalidateRegion(pRegion.Bounds);
        }
        else if (reason.HasFlag(MSYS_CALLBACK_REASON.GAIN_MOUSE))
        {
            DrawMercsFaceToScreen(ubMercNum, usPosX, usPosY, 0);
            video.InvalidateRegion(pRegion.Bounds);
        }
    }

    public static bool IsMercDead(int ubMercID)
        => gMercProfiles[(NPCID)ubMercID].bMercStatus == MercStatus.MERC_IS_DEAD;

    public static bool IsMercHireable(int ubMercID)
    {
        MERCPROFILESTRUCT p = gMercProfiles[(NPCID)ubMercID];

        // If the merc has an .EDT file, is not away on assignment, isn't already hired (but not arrived yet),
        // is not dead and isn't returning home
        return !(p.bMercStatus == MercStatus.MERC_HAS_NO_TEXT_FILE
            || p.bMercStatus > 0
            || p.bMercStatus == MercStatus.MERC_HIRED_BUT_NOT_ARRIVED_YET
            || p.bMercStatus == MercStatus.MERC_IS_DEAD
            || p.uiDayBecomesAvailable > 0
            || p.bMercStatus == MercStatus.MERC_WORKING_ELSEWHERE
            || p.bMercStatus == MercStatus.MERC_FIRED_AS_A_POW
            || p.bMercStatus == MercStatus.MERC_RETURNING_HOME);
    }

    private static void ShadeFace(int x, int y)
    {
        VideoSurfaceManager.ShadowVideoSurfaceRect(
            SurfaceType.FRAME_BUFFER,
            Rectangle.FromLTRB(x + AIM_FI_FACE_OFFSET, y + AIM_FI_FACE_OFFSET, x + 48 + AIM_FI_FACE_OFFSET, y + 43 + AIM_FI_FACE_OFFSET));
    }

    private static void DrawFaceOverlay(string text, int usPosX, int usPosY)
    {
        FontSubSystem.DrawTextToScreen(text, new(usPosX + AIM_FI_AWAY_TEXT_OFFSET_X, usPosY + AIM_FI_AWAY_TEXT_OFFSET_Y), AIM_FI_AWAY_TEXT_OFFSET_WIDTH, FontStyle.FONT10ARIAL, (FontColor)145, FontColor.FONT_MCOLOR_BLACK, TextJustifies.CENTER_JUSTIFIED);
    }

    public static bool DrawMercsFaceToScreen(int ubMercID, int usPosX, int usPosY, int ubImage)
    {
        int profile = AimMercArray[ubMercID];
        SOLDIERTYPE? pSoldier = SoldierProfileSubSystem.FindSoldierByProfileID((NPCID)profile, true);

        // Blt the portrait background
        video.BltVideoObject(SurfaceType.FRAME_BUFFER, guiMugShotBorder, ubImage, usPosX, usPosY, VO_BLT.SRCTRANSPARENCY);

        // Blt face to screen
        HVOBJECT hFaceHandle = guiAimFiFace[ubMercID];
        video.BltVideoObject(SurfaceType.FRAME_BUFFER, hFaceHandle, 0, usPosX + AIM_FI_FACE_OFFSET, usPosY + AIM_FI_FACE_OFFSET, VO_BLT.SRCTRANSPARENCY);

        if (IsMercDead(profile))
        {
            // shade the face red (to signify that he is dead)
            hFaceHandle.pShades[0] = video.Create16BPPPaletteShaded(hFaceHandle.Palette, DEAD_MERC_COLOR_RED, DEAD_MERC_COLOR_GREEN, DEAD_MERC_COLOR_BLUE, true);
            hFaceHandle.ShadeCurrentPixels = hFaceHandle.pShades[0];

            video.BltVideoObject(SurfaceType.FRAME_BUFFER, hFaceHandle, 0, usPosX + AIM_FI_FACE_OFFSET, usPosY + AIM_FI_FACE_OFFSET, VO_BLT.SRCTRANSPARENCY);

            DrawFaceOverlay(AimFiText[AIM_FI_DEAD], usPosX, usPosY);
        }
        else if (gMercProfiles[(NPCID)profile].bMercStatus == MercStatus.MERC_FIRED_AS_A_POW
            || (pSoldier is not null && pSoldier.bAssignment == Assignment.ASSIGNMENT_POW))
        {
            // the merc is currently a POW, or was fired as a POW
            ShadeFace(usPosX, usPosY);
            DrawFaceOverlay(pPOWStrings[0], usPosX, usPosY);
        }
        else if (pSoldier is not null)
        {
            // the merc is on our team
            ShadeFace(usPosX, usPosY);
            DrawFaceOverlay(MercInfo[MERC_FILES_ALREADY_HIRED], usPosX, usPosY);
        }
        else if (!IsMercHireable(profile))
        {
            // the merc is away: shadow the face and blit 'away' over the top
            ShadeFace(usPosX, usPosY);
            DrawFaceOverlay(AimFiText[AIM_FI_DEAD + 1], usPosX, usPosY);
        }

        return true;
    }
}
