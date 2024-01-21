using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using SharpAlliance.Core.Interfaces;
using SharpAlliance.Core.Managers.VideoSurfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace SharpAlliance.Core.SubSystems.LaptopSubSystem.InsuranceSubSystem;

public partial class Insurance
{
    private const FontColor INS_FONT_COLOR = (FontColor)2;
    private const FontColor INS_FONT_COLOR_RED = FontColor.FONT_MCOLOR_RED;
    private const FontStyle INS_FONT_BIG = FontStyle.FONT14ARIAL;
    private const FontStyle INS_FONT_MED = FontStyle.FONT12ARIAL;
    private const FontStyle INS_FONT_SMALL = FontStyle.FONT10ARIAL;
    private const FontColor INS_FONT_BTN_COLOR = FontColor.FONT_MCOLOR_WHITE;
    private const int INS_FONT_BTN_SHADOW_COLOR = 2;
    private const FontShadow INS_FONT_SHADOW = FontShadow.DEFAULT_SHADOW;
    private const int INSURANCE_BULLET_TEXT_OFFSET_X = 21;
    private const int INS_INFO_LEFT_ARROW_BUTTON_X = 71 + LAPTOP_SCREEN_UL_X;
    private const int INS_INFO_LEFT_ARROW_BUTTON_Y = 354 + LAPTOP_SCREEN_WEB_UL_Y;
    private const int INS_INFO_RIGHT_ARROW_BUTTON_X = 409 + LAPTOP_SCREEN_UL_X;
    private const int INS_INFO_RIGHT_ARROW_BUTTON_Y = INS_INFO_LEFT_ARROW_BUTTON_Y;

    private const int INSURANCE_TEXT_SINGLE_LINE_SIZE = 80 * 2;
    private const int INSURANCE_TEXT_MULTI_LINE_SIZE = 5 * 80 * 2;
    private const string INSURANCE_TEXT_SINGLE_FILE = "BINARYDATA\\InsuranceSingle.edt";
    private const string INSURANCE_TEXT_MULTI_FILE = "BINARYDATA\\InsuranceMulti.edt";
    private const int INSURANCE_TEXT_SINGLE = 1;
    private const int INSURANCE_TEXT_MULTI = 2;

    private const int INSURANCE_BACKGROUND_WIDTH = 125;
    private const int INSURANCE_BACKGROUND_HEIGHT = 100;
    private const int INSURANCE_BIG_TITLE_X = 95 + LAPTOP_SCREEN_UL_X;
    private const int INSURANCE_BIG_TITLE_Y = 4 + LAPTOP_SCREEN_WEB_UL_Y;
    private const int INSURANCE_RED_BAR_X = LAPTOP_SCREEN_UL_X;
    private const int INSURANCE_RED_BAR_Y = LAPTOP_SCREEN_WEB_UL_Y;
    private const int INSURANCE_TOP_RED_BAR_X = LAPTOP_SCREEN_UL_X + 66;
    private const int INSURANCE_TOP_RED_BAR_Y = 109 + LAPTOP_SCREEN_WEB_UL_Y;
    private const int INSURANCE_TOP_RED_BAR_Y1 = 31 + LAPTOP_SCREEN_WEB_UL_Y;
    private const int INSURANCE_BOTTOM_RED_BAR_Y = 345 + LAPTOP_SCREEN_WEB_UL_Y;
    private const int INSURANCE_BOTTOM_LINK_RED_BAR_X = 77 + LAPTOP_SCREEN_UL_X;
    private const int INSURANCE_BOTTOM_LINK_RED_BAR_Y = 392 + LAPTOP_SCREEN_WEB_UL_Y;
    private const int INSURANCE_BOTTOM_LINK_RED_BAR_WIDTH = 107;
    private const int INSURANCE_BOTTOM_LINK_RED_BAR_OFFSET = 148;
    private const int INSURANCE_BOTTOM_LINK_RED_BAR_X_2 = INSURANCE_BOTTOM_LINK_RED_BAR_X + INSURANCE_BOTTOM_LINK_RED_BAR_OFFSET;
    private const int INSURANCE_BOTTOM_LINK_RED_BAR_X_3 = INSURANCE_BOTTOM_LINK_RED_BAR_X_2 + INSURANCE_BOTTOM_LINK_RED_BAR_OFFSET;
    private const int INSURANCE_LINK_TEXT_WIDTH = INSURANCE_BOTTOM_LINK_RED_BAR_WIDTH;
    private const int INSURANCE_LINK_TEXT_1_X = INSURANCE_BOTTOM_LINK_RED_BAR_X;
    private const int INSURANCE_LINK_TEXT_1_Y = INSURANCE_BOTTOM_LINK_RED_BAR_Y - 36;
    private const int INSURANCE_LINK_TEXT_2_X = INSURANCE_LINK_TEXT_1_X + INSURANCE_BOTTOM_LINK_RED_BAR_OFFSET;
    private const int INSURANCE_LINK_TEXT_2_Y = INSURANCE_LINK_TEXT_1_Y;
    private const int INSURANCE_LINK_TEXT_3_X = INSURANCE_LINK_TEXT_2_X + INSURANCE_BOTTOM_LINK_RED_BAR_OFFSET;
    private const int INSURANCE_LINK_TEXT_3_Y = INSURANCE_LINK_TEXT_1_Y;
    private const int INSURANCE_SUBTITLE_X = INSURANCE_BOTTOM_LINK_RED_BAR_X + 15;
    private const int INSURANCE_SUBTITLE_Y = 150 + LAPTOP_SCREEN_WEB_UL_Y;
    private const int INSURANCE_BULLET_TEXT_1_Y = 188 + LAPTOP_SCREEN_WEB_UL_Y;
    private const int INSURANCE_BULLET_TEXT_2_Y = 215 + LAPTOP_SCREEN_WEB_UL_Y;
    private const int INSURANCE_BULLET_TEXT_3_Y = 242 + LAPTOP_SCREEN_WEB_UL_Y;
    private const int INSURANCE_BOTTOM_SLOGAN_X = INSURANCE_SUBTITLE_X;
    private const int INSURANCE_BOTTOM_SLOGAN_Y = 285 + LAPTOP_SCREEN_WEB_UL_Y;
    private const int INSURANCE_BOTTOM_SLOGAN_WIDTH = 370;
    private const int INSURANCE_SMALL_TITLE_X = 64 + LAPTOP_SCREEN_UL_X;
    private const int INSURANCE_SMALL_TITLE_Y = 5 + LAPTOP_SCREEN_WEB_UL_Y;
    private const int INSURANCE_SMALL_TITLE_WIDTH = 434 - 170;
    private const int INSURANCE_SMALL_TITLE_HEIGHT = 40 - 10;

    private static bool gubCurrentInsInfoSubPage = false;

    private static bool[] InsuranceInfoSubPagesVisitedFlag = new bool[(int)INS_INFO.LAST_PAGE - 1];
    private readonly ILogger<Insurance> logger;
    private static IVideoManager video;
    private static IFileManager files;
    private static HVOBJECT guiInsuranceTitleImage;
    private static HVOBJECT guiInsuranceBulletImage;

    private static MOUSE_REGION[] gSelectedInsuranceLinkRegion = new MOUSE_REGION[3];
    private static MOUSE_REGION gSelectedInsuranceTitleLinkRegion;
    private static HVOBJECT guiInsuranceSmallTitleImage;
    private static HVOBJECT guiInsuranceBackGround;
    private static HVOBJECT guiInsuranceRedBarImage;
    private static HVOBJECT guiInsuranceBigRedLineImage;
    private static List<GUI_BUTTON> buttonList = [];

    public Insurance(
        ILogger<Insurance> logger,
        IVideoManager videoManager,
        IFileManager fileManager)
    {
        this.logger = logger;
        Insurance.video = videoManager;
        files = fileManager;
    }

    public static void GameInitInsurance()
    {
    }

    internal static void EnterInitInsuranceInfo()
    {
        Array.Fill(InsuranceInfoSubPagesVisitedFlag, false);
    }

    internal static void EnterInsurance()
    {
        int usPosX, i;

        Laptop.SetBookMark(BOOKMARK.INSURANCE_BOOKMARK);

        InitInsuranceDefaults();

        // load the Insurance title graphic and add it
        MultilanguageGraphicUtils.GetMLGFilename(out string insuranceTitle, MLG.INSURANCETITLE);
        guiInsuranceTitleImage = video.GetVideoObject(insuranceTitle);

        // load the red bar on the side of the page and add it
        guiInsuranceBulletImage = video.GetVideoObject(Utils.FilenameForBPP("LAPTOP\\Bullet.sti"));

        usPosX = INSURANCE_BOTTOM_LINK_RED_BAR_X;
        for (i = 0; i < 3; i++)
        {
            gSelectedInsuranceLinkRegion[i] = new($"{nameof(gSelectedInsuranceLinkRegion)}-{i}");

            MouseSubSystem.MSYS_DefineRegion(
                gSelectedInsuranceLinkRegion[i],
                new Rectangle(usPosX, INSURANCE_BOTTOM_LINK_RED_BAR_Y - 37, (usPosX + INSURANCE_BOTTOM_LINK_RED_BAR_WIDTH), INSURANCE_BOTTOM_LINK_RED_BAR_Y + 2),
                MSYS_PRIORITY.HIGH,
                CURSOR.WWW,
                MSYS_NO_CALLBACK,
                SelectInsuranceRegionCallBack);

            MouseSubSystem.MSYS_AddRegion(ref gSelectedInsuranceLinkRegion[i]);
            MouseSubSystem.MSYS_SetRegionUserData(gSelectedInsuranceLinkRegion[i], 0, i);

            usPosX += INSURANCE_BOTTOM_LINK_RED_BAR_OFFSET;
        }

        RenderInsurance();

        // reset the current merc index on the insurance contract page
        gsCurrentInsuranceMercIndex = 0;
    }

    private static void RenderInsurance()
    {
        string sText = string.Empty;

        DisplayInsuranceDefaults();

        FontSubSystem.SetFontShadow(INS_FONT_SHADOW);

        //Get and display the insurance title
        video.BltVideoObject(SurfaceType.FRAME_BUFFER, guiInsuranceTitleImage, 0, INSURANCE_BIG_TITLE_X, INSURANCE_BIG_TITLE_Y, VO_BLT.SRCTRANSPARENCY);

        //Display the title slogan
        GetInsuranceText(INS.SNGL_WERE_LISTENING, out sText);
        FontSubSystem.DrawTextToScreen(
            sText,
            new(LAPTOP_SCREEN_UL_X, INSURANCE_TOP_RED_BAR_Y - 35),
            LAPTOP_SCREEN_LR_X - LAPTOP_SCREEN_UL_X,
            INS_FONT_BIG,
            INS_FONT_COLOR,
            FontColor.FONT_MCOLOR_BLACK,
            TextJustifies.CENTER_JUSTIFIED);

        //Display the subtitle slogan
        GetInsuranceText(INS.SNGL_LIFE_INSURANCE_SPECIALISTS, out sText);
        FontSubSystem.DrawTextToScreen(
            sText,
            new(INSURANCE_SUBTITLE_X, INSURANCE_SUBTITLE_Y),
            0,
            INS_FONT_BIG,
            INS_FONT_COLOR,
            FontColor.FONT_MCOLOR_BLACK,
            TextJustifies.LEFT_JUSTIFIED);

        //Display the bulleted text 1
        video.BltVideoObject(SurfaceType.FRAME_BUFFER, guiInsuranceBulletImage, 0, INSURANCE_SUBTITLE_X, INSURANCE_BULLET_TEXT_1_Y, VO_BLT.SRCTRANSPARENCY);
        GetInsuranceText(INS.MLTI_EMPLOY_HIGH_RISK, out sText);
        FontSubSystem.DrawTextToScreen(
            sText,
            new(INSURANCE_SUBTITLE_X + INSURANCE_BULLET_TEXT_OFFSET_X, INSURANCE_BULLET_TEXT_1_Y),
            0,
            INS_FONT_MED,
            INS_FONT_COLOR,
            FontColor.FONT_MCOLOR_BLACK,
            TextJustifies.LEFT_JUSTIFIED);

        //Display the bulleted text 2
        video.BltVideoObject(SurfaceType.FRAME_BUFFER, guiInsuranceBulletImage, 0, INSURANCE_SUBTITLE_X, INSURANCE_BULLET_TEXT_2_Y, VO_BLT.SRCTRANSPARENCY);
        GetInsuranceText(INS.MLTI_HIGH_FATALITY_RATE, out sText);
        FontSubSystem.DrawTextToScreen(
            sText,
            new(INSURANCE_SUBTITLE_X + INSURANCE_BULLET_TEXT_OFFSET_X, INSURANCE_BULLET_TEXT_2_Y),
            0,
            INS_FONT_MED,
            INS_FONT_COLOR,
            FontColor.FONT_MCOLOR_BLACK,
            TextJustifies.LEFT_JUSTIFIED);

        //Display the bulleted text 3
        video.BltVideoObject(SurfaceType.FRAME_BUFFER, guiInsuranceBulletImage, 0, INSURANCE_SUBTITLE_X, INSURANCE_BULLET_TEXT_3_Y, VO_BLT.SRCTRANSPARENCY);
        GetInsuranceText(INS.MLTI_DRAIN_SALARY, out sText);
        FontSubSystem.DrawTextToScreen(
            sText,
            new(INSURANCE_SUBTITLE_X + INSURANCE_BULLET_TEXT_OFFSET_X, INSURANCE_BULLET_TEXT_3_Y),
            0,
            INS_FONT_MED,
            INS_FONT_COLOR,
            FontColor.FONT_MCOLOR_BLACK,
            TextJustifies.LEFT_JUSTIFIED);

        //Display the bottom slogan
        GetInsuranceText(INS.MLTI_IF_ANSWERED_YES, out sText);
        FontSubSystem.DrawTextToScreen(
            sText,
            new(INSURANCE_BOTTOM_SLOGAN_X, INSURANCE_BOTTOM_SLOGAN_Y),
            INSURANCE_BOTTOM_SLOGAN_WIDTH,
            INS_FONT_MED,
            INS_FONT_COLOR,
            FontColor.FONT_MCOLOR_BLACK,
            TextJustifies.CENTER_JUSTIFIED);

        //Display the red bar under the link at the bottom.  and the text
        DisplaySmallRedLineWithShadow(
            new(INSURANCE_BOTTOM_LINK_RED_BAR_X, INSURANCE_BOTTOM_LINK_RED_BAR_Y),
            new(INSURANCE_BOTTOM_LINK_RED_BAR_X + INSURANCE_BOTTOM_LINK_RED_BAR_WIDTH, INSURANCE_BOTTOM_LINK_RED_BAR_Y));

        GetInsuranceText(INS.SNGL_COMMENTSFROM_CLIENTS, out sText);
        FontSubSystem.DisplayWrappedString(
            new(INSURANCE_LINK_TEXT_1_X, INSURANCE_LINK_TEXT_1_Y),
            INSURANCE_LINK_TEXT_WIDTH,
            2,
            INS_FONT_MED,
            INS_FONT_COLOR,
            sText,
            FontColor.FONT_MCOLOR_BLACK,
            TextJustifies.CENTER_JUSTIFIED);

        //Display the red bar under the link at the bottom
        DisplaySmallRedLineWithShadow(
            new(INSURANCE_BOTTOM_LINK_RED_BAR_X_2, INSURANCE_BOTTOM_LINK_RED_BAR_Y),
            new(INSURANCE_BOTTOM_LINK_RED_BAR_X_2 + INSURANCE_BOTTOM_LINK_RED_BAR_WIDTH, INSURANCE_BOTTOM_LINK_RED_BAR_Y));

        GetInsuranceText(INS.SNGL_HOW_DOES_INS_WORK, out sText);
        FontSubSystem.DisplayWrappedString(
            new(INSURANCE_LINK_TEXT_2_X, INSURANCE_LINK_TEXT_2_Y + 7),
            INSURANCE_LINK_TEXT_WIDTH,
            2,
            INS_FONT_MED, INS_FONT_COLOR,
            sText,
            FontColor.FONT_MCOLOR_BLACK,
            TextJustifies.CENTER_JUSTIFIED);

        //Display the red bar under the link at the bottom
        DisplaySmallRedLineWithShadow(
            new(INSURANCE_BOTTOM_LINK_RED_BAR_X_3, INSURANCE_BOTTOM_LINK_RED_BAR_Y),
            new(INSURANCE_BOTTOM_LINK_RED_BAR_X_3 + INSURANCE_BOTTOM_LINK_RED_BAR_WIDTH, INSURANCE_BOTTOM_LINK_RED_BAR_Y));

        GetInsuranceText(INS.SNGL_TO_ENTER_REVIEW, out sText);
        FontSubSystem.DisplayWrappedString(
            new(INSURANCE_LINK_TEXT_3_X, INSURANCE_LINK_TEXT_3_Y + 7),
            INSURANCE_LINK_TEXT_WIDTH,
            2,
            INS_FONT_MED,
            INS_FONT_COLOR,
            sText,
            FontColor.FONT_MCOLOR_BLACK,
            TextJustifies.CENTER_JUSTIFIED);

        FontSubSystem.SetFontShadow(FontShadow.DEFAULT_SHADOW);

        ButtonSubSystem.MarkButtonsDirty(buttonList);
        Laptop.RenderWWWProgramTitleBar();
        video.InvalidateRegion(LAPTOP_SCREEN_UL_X, LAPTOP_SCREEN_WEB_UL_Y, LAPTOP_SCREEN_LR_X, LAPTOP_SCREEN_WEB_LR_Y);
    }

    private static void DisplaySmallRedLineWithShadow(Point usStart, Point End)
    {
        Image<Rgba32> pDestBuf;

        pDestBuf = video.Surfaces[SurfaceType.FRAME_BUFFER];

        // chad: guessing on width
        video.SetClippingRegionAndImageWidth(8, new(0, 0, 640, 480));

        // draw the red line 
        video.LineDraw(false, usStart, End, FROMRGB(255, 0, 0), pDestBuf);

        // draw the black shadow line 
        video.LineDraw(false, new(usStart.X + 1, usStart.Y + 1), new(End.X + 1, End.Y + 1), FROMRGB(0, 0, 0), pDestBuf);

        // unlock frame buffer
        //video.UnLockVideoSurface(FRAME_BUFFER);
    }

    private static void GetInsuranceText(INS ubNumber, out string pString)
    {
        int uiStartLoc = 0;

        if (ubNumber < INS.MULTI_LINE_BEGINS)
        {
            //Get and display the card saying
            uiStartLoc = INSURANCE_TEXT_SINGLE_LINE_SIZE * (int)ubNumber;
            files.LoadEncryptedDataFromFile(INSURANCE_TEXT_SINGLE_FILE, out pString, uiStartLoc, INSURANCE_TEXT_SINGLE_LINE_SIZE);
        }
        else
        {
            //Get and display the card saying
            uiStartLoc = INSURANCE_TEXT_MULTI_LINE_SIZE * ((int)ubNumber - (int)(INS.MULTI_LINE_BEGINS) - 1);
            files.LoadEncryptedDataFromFile(INSURANCE_TEXT_MULTI_FILE, out pString, uiStartLoc, INSURANCE_TEXT_MULTI_LINE_SIZE);
        }
    }

    private static void DisplayInsuranceDefaults()
    {
        int i;
        int usPosY;

        Laptop.WebPageTileBackground(4, 4, INSURANCE_BACKGROUND_WIDTH, INSURANCE_BACKGROUND_HEIGHT, guiInsuranceBackGround);

        usPosY = INSURANCE_RED_BAR_Y;

        for (i = 0; i < 4; i++)
        {
            video.BltVideoObject(SurfaceType.FRAME_BUFFER, guiInsuranceRedBarImage, 0, INSURANCE_RED_BAR_X, usPosY, VO_BLT.SRCTRANSPARENCY);
            usPosY += INSURANCE_BACKGROUND_HEIGHT;
        }

        //display the top red bar
        switch (guiCurrentLaptopMode)
        {
            case LAPTOP_MODE.INSURANCE:
                usPosY = INSURANCE_TOP_RED_BAR_Y;

                //display the top red bar
                video.BltVideoObject(SurfaceType.FRAME_BUFFER, guiInsuranceBigRedLineImage, 0, INSURANCE_TOP_RED_BAR_X, usPosY, VO_BLT.SRCTRANSPARENCY);

                break;

            case LAPTOP_MODE.INSURANCE_INFO:
            case LAPTOP_MODE.INSURANCE_CONTRACT:
                usPosY = INSURANCE_TOP_RED_BAR_Y1;
                break;
        }

        //display the Bottom red bar
        video.BltVideoObject(SurfaceType.FRAME_BUFFER, guiInsuranceBigRedLineImage, 0, INSURANCE_TOP_RED_BAR_X, INSURANCE_BOTTOM_RED_BAR_Y, VO_BLT.SRCTRANSPARENCY);

        //if it is not the first page, display the small title
        if (guiCurrentLaptopMode != LAPTOP_MODE.INSURANCE)
        {
            //display the small title bar
            video.BltVideoObject(SurfaceType.FRAME_BUFFER, guiInsuranceSmallTitleImage, 0, INSURANCE_SMALL_TITLE_X, INSURANCE_SMALL_TITLE_Y, VO_BLT.SRCTRANSPARENCY);
        }
    }

    private static void InitInsuranceDefaults()
    {
        // load the Flower Account Box graphic and add it
        guiInsuranceBackGround = video.GetVideoObject(Utils.FilenameForBPP("LAPTOP\\BackGroundTile.sti"));

        // load the red bar on the side of the page and add it
        guiInsuranceRedBarImage = video.GetVideoObject(Utils.FilenameForBPP("LAPTOP\\LeftTile.sti"));

        // load the red bar on the side of the page and add it
        guiInsuranceBigRedLineImage = video.GetVideoObject(Utils.FilenameForBPP("LAPTOP\\LargeBar.sti"));

        //if it is not the first page, display the small title
        if (guiCurrentLaptopMode != LAPTOP_MODE.INSURANCE)
        {
            // load the small title for the every page other then the first page
            MultilanguageGraphicUtils.GetMLGFilename(out string insuranceSmallTitle, MLG.SMALLTITLE);
            guiInsuranceSmallTitleImage = video.GetVideoObject(insuranceSmallTitle);

            //create the link to the home page on the small titles
            MouseSubSystem.MSYS_DefineRegion(
                gSelectedInsuranceTitleLinkRegion,
                new Rectangle(INSURANCE_SMALL_TITLE_X + 85, INSURANCE_SMALL_TITLE_Y, (INSURANCE_SMALL_TITLE_X + INSURANCE_SMALL_TITLE_WIDTH), (INSURANCE_SMALL_TITLE_Y + INSURANCE_SMALL_TITLE_HEIGHT)),
                MSYS_PRIORITY.HIGH,
                CURSOR.WWW,
                MSYS_NO_CALLBACK,
                SelectInsuranceTitleLinkRegionCallBack);

            MouseSubSystem.MSYS_AddRegion(ref gSelectedInsuranceTitleLinkRegion);
        }
    }

    private static void SelectInsuranceTitleLinkRegionCallBack(ref MOUSE_REGION region, MSYS_CALLBACK_REASON iReason)
    {
        if (iReason.HasFlag(MSYS_CALLBACK_REASON.INIT))
        {
        }
        else if (iReason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP))
        {
            guiCurrentLaptopMode = LAPTOP_MODE.INSURANCE;
        }
        else if (iReason.HasFlag(MSYS_CALLBACK_REASON.RBUTTON_UP))
        {
        }
    }

    private static void SelectInsuranceRegionCallBack(ref MOUSE_REGION pRegion, MSYS_CALLBACK_REASON iReason)
    {
        if (iReason.HasFlag(MSYS_CALLBACK_REASON.INIT))
        {
        }
        else if (iReason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP))
        {
            int uiInsuranceLink = (int)MouseSubSystem.MSYS_GetRegionUserData(ref pRegion, 0);

            if (uiInsuranceLink == 0)
            {
                guiCurrentLaptopMode = LAPTOP_MODE.INSURANCE_COMMENTS;
            }
            else if (uiInsuranceLink == 1)
            {
                guiCurrentLaptopMode = LAPTOP_MODE.INSURANCE_INFO;
            }
            else if (uiInsuranceLink == 2)
            {
                guiCurrentLaptopMode = LAPTOP_MODE.INSURANCE_CONTRACT;
            }
        }
        else if (iReason.HasFlag(MSYS_CALLBACK_REASON.RBUTTON_UP))
        {
        }
    }

    internal static void EnterInsuranceComments()
    {
        throw new NotImplementedException();
    }

    internal static void EnterInsuranceContract()
    {
        throw new NotImplementedException();
    }

    internal static void EnterInsuranceInfo()
    {
        throw new NotImplementedException();
    }

    internal static void ExitInsurance()
    {
        throw new NotImplementedException();
    }

    internal static void ExitInsuranceComments()
    {
        throw new NotImplementedException();
    }

    internal static void ExitInsuranceContract()
    {
        throw new NotImplementedException();
    }

    internal static void ExitInsuranceInfo()
    {
        throw new NotImplementedException();
    }

    internal static void HandleInsurance()
    {
        throw new NotImplementedException();
    }

    internal static void HandleInsuranceInfo()
    {
        throw new NotImplementedException();
    }

    internal static void HandleInsuranceContract()
    {
        throw new NotImplementedException();
    }

    internal static void HandleInsuranceComments()
    {
        throw new NotImplementedException();
    }
}
//The list of Info sub pages
public enum INS_INFO
{
    INFO_TOC,
    SUBMIT_CLAIM,
    PREMIUMS,
    RENEWL,
    CANCELATION,
    LAST_PAGE,
};

//Single line
public enum INS
{
    SNGL_MALLEUS_INCUS_STAPES,
    SNGL_WERE_LISTENING,
    SNGL_LIFE_INSURANCE_SPECIALISTS,
    SNGL_COMMENTSFROM_CLIENTS,
    SNGL_HOW_DOES_INS_WORK,
    SNGL_TO_ENTER_REVIEW,
    SNGL_GUS_TARBALLS,
    SNGL_ALI_HUSSEAN,
    SNGL_LANCE_ALLOT,
    SNGL_FRED_COUSTEAU,
    SNGL_WE_CAN_OFFER_U,
    SNGL_PREMIUMS,
    SNGL_RENEWL_PREMIUMS,
    SNGL_LOWER_PREMIUMS_4_RENEWING,
    SNGL_POLICY_CANCELATIONS,
    SNGL_SUBMITTING_CLAIM,
    SNGL_FRAUD,
    SNGL_ENTERING_REVIEWING_CLAIM,
    SNGL_EMPLOYMENT_CONTRACT,
    SNGL_LENGTH,
    SNGL_DAYS_REMAINING,
    SNGL_INSURANCE_CONTRACT,
    SNGL_PREMIUM_OWING,
    SNGL_PREMIUM_REFUND,
    SNGL_CHANGE_LENGTH,
    SNGL_CONTRACT,
    SNGL_NOCONTRACT,
    SNGL_DEAD_WITH_CONTRACT,
    SNGL_DEAD_NO_CONTRACT,
    SNGL_PARTIALLY_INSURED,

    //Multi line
    MULTI_LINE_BEGINS,

    MLTI_EMPLOY_HIGH_RISK,
    MLTI_HIGH_FATALITY_RATE,
    MLTI_DRAIN_SALARY,
    MLTI_IF_ANSWERED_YES,
    MLTI_GUS_SPEECH,
    MLTI_ALI_HUSSEAN_SPEECH,
    MLTI_LANCE_ALLOT_SPEECH,
    MLTI_FRED_COUSTEAU_SPEECH,
    MLTI_HIRING_4_SHORT_TERM_HIGH_RISK_1,
    MLTI_HIRING_4_SHORT_TERM_HIGH_RISK_2,
    MLTI_REASONABLE_AND_FLEXIBLE,
    MLTI_EQUITABLE_RENEWL_PREMIUMS,
    MLTI_PREROGATIVE_TO_CANCEL,
    MLTI_QUICKLY_AND_EFFICIENT,
    MLTI_EACH_TIME_U_COME_TO_US,
    MLTI_LENGTH_OF_EMPLOYMENT_CONTRACT,
    MLTI_EMPLOYEES_AGE_AND_HEALTH,
    MLTI_EMPLOOYEES_TRAINING_AND_EXP,
    MLTI_WHEN_IT_COMES_TIME_TO_RENEW,
    MLTI_SHOULD_THE_PROJECT_BE_GOING_WELL,
    MLTI_IF_U_EXTEND_THE_CONTRACT,
    MLTI_WE_WILL_ACCEPT_INS_CANCELATION,
    MLTI_U_CAN_REST_ASSURED,
    MLTI_HAD_U_HIRED_AN_INDIVIDUAL,
    MLTI_WE_RESERVE_THE_RIGHT,
    MLTI_SHOULD_THERE_BE_GROUNDS,
    MLTI_SHOULD_SUCH_A_SITUATION,
    MLTI_TO_PURCHASE_INSURANCE,
    MLTI_ONCE_SATISFIED_CLICK_ACCEPT,
    MLTI_SORRY_U_CANT_CANCEL,
    MLTI_ARE_U_SURE_U_CANCEL,
    MLTI_NO_QUALIFIED_MERCS,
    MLTI_NOT_ENOUGH_FUNDS,
    MLTI_ALL_AIM_MERCS_ON_SHORT_CONTRACT,
    MLTI_1_HOUR_EXCLUSION_A,
    MLTI_1_HOUR_EXCLUSION_B,
};
