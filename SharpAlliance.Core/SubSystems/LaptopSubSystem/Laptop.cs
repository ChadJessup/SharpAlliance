using System;
using SharpAlliance.Core.Managers;
using SharpAlliance.Core.Managers.VideoSurfaces;
using SharpAlliance.Core.Interfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Collections.Generic;
using SharpAlliance.Core.Screens;
using SharpAlliance.Core.SubSystems.LaptopSubSystem.BobbyRSubSystem;
using SharpAlliance.Core.SubSystems.LaptopSubSystem.FloristSubSystem;
using SharpAlliance.Core.SubSystems.LaptopSubSystem.InsuranceSubSystem;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Diagnostics;

namespace SharpAlliance.Core.SubSystems.LaptopSubSystem;

public partial class Laptop
{
    private static IVideoManager video;
    private readonly History history;
    private static IFileManager files;
    private static readonly IEnumerable<GUI_BUTTON> buttonList;

    public Laptop(IVideoManager videoManager, History history, IFileManager fileManager)
    {
        video = videoManager;
        this.history = history;
        files = fileManager;
    }

    public static bool LaptopScreenInit()
    {
        //Memset the whole structure, to make sure of no 'JUNK'
        LaptopSaveInfo = new();

        LaptopSaveInfo.gfNewGameLaptop = true;


        Mercs.InitializeNumDaysMercArrive();

        //reset the id of the last hired merc
        LaptopSaveInfo.sLastHiredMerc.iIdOfMerc = -1;

        //reset the flag that enables the 'just hired merc' popup
        LaptopSaveInfo.sLastHiredMerc.fHaveDisplayedPopUpInLaptop = false;

        //Initialize all vars
        guiCurrentLaptopMode = LAPTOP_MODE.EMAIL;
        guiPreviousLaptopMode = LAPTOP_MODE.NONE;
        guiCurrentWWWMode = LAPTOP_MODE.NONE;
        guiCurrentSidePanel = LaptopPanel.FIRST_SIDE_PANEL;
        guiPreviousSidePanel = LaptopPanel.FIRST_SIDE_PANEL;


        gfSideBarFlag = false;
        gfShowBookmarks = false;
        InitBookMarkList();
        GameInitAIM();
        GameInitAIMMembers();
        GameInitAimFacialIndex();
        GameInitAimSort();
        GameInitAimArchives();
        GameInitAimPolicies();
        GameInitAimLinks();
        GameInitAimHistory();
        Mercs.GameInitMercs();
        BobbyR.GameInitBobbyR();
        BobbyR.GameInitBobbyRAmmo();
        BobbyR.GameInitBobbyRArmour();
        BobbyR.GameInitBobbyRGuns();
        BobbyR.GameInitBobbyRMailOrder();
        BobbyR.GameInitBobbyRMisc();
        BobbyR.GameInitBobbyRUsed();
        Emails.GameInitEmail();
        GameInitCharProfile();
        Florist.GameInitFlorist();
        Insurance.GameInitInsurance();
        Insurance.GameInitInsuranceContract();
        GameInitFuneral();
        GameInitSirTech();
        GameInitFiles();
        GameInitPersonnel();

        // init program states
        Array.Fill(gLaptopProgramStates, LAPTOP_PROGRAM_STATES.MINIMIZED);

        gfAtLeastOneMercWasHired = false;

        //No longer inits the laptop screens, now InitLaptopAndLaptopScreens() does

        return true;
    }

    private static void CreateFileAndNewEmailIconFastHelpText(LaptopText uiHelpTextID, bool fClearHelpText)
    {
        MOUSE_REGION pRegion;

        switch (uiHelpTextID)
        {
            case LaptopText.LAPTOP_BN_HLP_TXT_YOU_HAVE_NEW_MAIL:
                pRegion = gNewMailIconRegion;
                break;

            case LaptopText.LAPTOP_BN_HLP_TXT_YOU_HAVE_NEW_FILE:
                pRegion = gNewFileIconRegion;
                break;

            default:
                Debug.Assert(false);
                return;
        }

        if (fClearHelpText)
        {
            MouseSubSystem.SetRegionFastHelpText(pRegion, "");
        }
        else
        {
            MouseSubSystem.SetRegionFastHelpText(pRegion, gzLaptopHelpText[uiHelpTextID]);
        }

        //fUnReadMailFlag
        //fNewFilesInFileViewer
    }

    private static void InitBookMarkList()
    {
        // sets bookmark list to -1
        Array.Fill(LaptopSaveInfo.iBookMarkList, (BOOKMARK)(-1));
    }

    public bool InitLaptopAndLaptopScreens()
    {
        Finances.GameInitFinances();
        this.history.GameInitHistory();

        //Reset the flag so we can create a new IMP character
        LaptopSaveInfo.fIMPCompletedFlag = false;

        //Reset the flag so that BOBBYR's isnt available at the begining of the game
        LaptopSaveInfo.fBobbyRSiteCanBeAccessed = false;

        return true;
    }

    public static bool DrawLapTopIcons()
    {
        return true;
    }

    public static void SetBookMark(BOOKMARK iBookId)
    {
        // find first empty spot, set to iBookId
        int iCounter = 0;
        if (iBookId != (BOOKMARK)(-2))
        {
            while (LaptopSaveInfo.iBookMarkList[iCounter] != (BOOKMARK)(-1))
            {
                // move trhough list until empty
                if (LaptopSaveInfo.iBookMarkList[iCounter] == iBookId)
                {
                    // found it, return
                    return;
                }

                iCounter++;
            }

            LaptopSaveInfo.iBookMarkList[iCounter] = iBookId;
        }

        return;
    }

    public static void LapTopScreenCallBack(ref MOUSE_REGION pRegion, MSYS_CALLBACK_REASON iReason)
    {

        if (iReason.HasFlag(MSYS_CALLBACK_REASON.INIT))
        {

            return;
        }

        if (iReason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP))
        {
            HandleLeftButtonUpEvent();
            return;
        }
        if (iReason.HasFlag(MSYS_CALLBACK_REASON.RBUTTON_UP))
        {
            //            HandleRightButtonUpEvent();
            return;
        }

        return;
    }

    public static void HandleLeftButtonUpEvent()
    {

        // will handle the left button up event

        if (gfShowBookmarks)
        {
            // get rid of bookmarks
            gfShowBookmarks = false;

            // force redraw
            fReDrawScreenFlag = true;
            RenderLapTopImage();
            RenderLaptop();
        }
        else if (fShowBookmarkInfo)
        {
            fShowBookmarkInfo = false;
        }
    }

    public static void RenderLapTopImage()
    {


        if (fMaximizingProgram == true || fMinizingProgram == true)
        {
            return;
        }

        //        video.GetVideoObject(out HVOBJECT? hLapTopHandle, guiLAPTOP);
        //        VideoObjectManager.BltVideoObject(SurfaceType.FRAME_BUFFER, hLapTopHandle, 0, LAPTOP_X, LAPTOP_Y, VO_BLT.SRCTRANSPARENCY, null);


        //        hLapTopHandle = video.GetVideoObject(guiLaptopBACKGROUND);
        //        VideoObjectManager.BltVideoObject(SurfaceType.FRAME_BUFFER, hLapTopHandle, 1, 25, 23, VO_BLT.SRCTRANSPARENCY, null);


        ButtonSubSystem.MarkButtonsDirty(buttonList);
    }

    public static void RenderLaptop()
    {
        LAPTOP_MODE uiTempMode = 0;

        if (fMaximizingProgram == true || fMinizingProgram == true)
        {
            gfShowBookmarks = false;
            return;
        }

        if (fLoadPendingFlag)
        {
            uiTempMode = guiCurrentLaptopMode;
            guiCurrentLaptopMode = guiPreviousLaptopMode;
        }

        switch (guiCurrentLaptopMode)
        {
            case LAPTOP_MODE.NONE:
                DrawDeskTopBackground();
                break;
            case LAPTOP_MODE.AIM:
                //                RenderAIM();
                break;
            case LAPTOP_MODE.AIM_MEMBERS:
                //                RenderAIMMembers();
                break;
            case LAPTOP_MODE.AIM_MEMBERS_FACIAL_INDEX:
                //                RenderAimFacialIndex();
                break;
            case LAPTOP_MODE.AIM_MEMBERS_SORTED_FILES:
                //                RenderAimSort();
                break;
            case LAPTOP_MODE.AIM_MEMBERS_ARCHIVES:
                //                RenderAimArchives();
                break;
            case LAPTOP_MODE.AIM_POLICIES:
                //                RenderAimPolicies();
                break;
            case LAPTOP_MODE.AIM_LINKS:
                //                RenderAimLinks();
                break;
            case LAPTOP_MODE.AIM_HISTORY:
                //                RenderAimHistory();
                break;
            case LAPTOP_MODE.MERC:
                //                RenderMercs();
                break;
            case LAPTOP_MODE.MERC_FILES:
                //                RenderMercsFiles();
                break;
            case LAPTOP_MODE.MERC_ACCOUNT:
                //                RenderMercsAccount();
                break;
            case LAPTOP_MODE.MERC_NO_ACCOUNT:
                //                RenderMercsNoAccount();
                break;

            case LAPTOP_MODE.BOBBY_R:
                //                RenderBobbyR();
                break;

            case LAPTOP_MODE.BOBBY_R_GUNS:
                //                RenderBobbyRGuns();
                break;
            case LAPTOP_MODE.BOBBY_R_AMMO:
                //                RenderBobbyRAmmo();
                break;
            case LAPTOP_MODE.BOBBY_R_ARMOR:
                //                RenderBobbyRArmour();
                break;
            case LAPTOP_MODE.BOBBY_R_MISC:
                //                RenderBobbyRMisc();
                break;
            case LAPTOP_MODE.BOBBY_R_USED:
                //                RenderBobbyRUsed();
                break;
            case LAPTOP_MODE.BOBBY_R_MAILORDER:
                //                RenderBobbyRMailOrder();
                break;
            case LAPTOP_MODE.CHAR_PROFILE:
                //                RenderCharProfile();
                break;
            case LAPTOP_MODE.FLORIST:
                //                RenderFlorist();
                break;
            case LAPTOP_MODE.FLORIST_FLOWER_GALLERY:
                //                RenderFloristGallery();
                break;
            case LAPTOP_MODE.FLORIST_ORDERFORM:
                //                RenderFloristOrderForm();
                break;
            case LAPTOP_MODE.FLORIST_CARD_GALLERY:
                //                RenderFloristCards();
                break;

            case LAPTOP_MODE.INSURANCE:
                //                RenderInsurance();
                break;

            case LAPTOP_MODE.INSURANCE_INFO:
                //                RenderInsuranceInfo();
                break;

            case LAPTOP_MODE.INSURANCE_CONTRACT:
                //                RenderInsuranceContract();
                break;

            case LAPTOP_MODE.INSURANCE_COMMENTS:
                //                RenderInsuranceComments();
                break;

            case LAPTOP_MODE.FUNERAL:
                //                RenderFuneral();
                break;
            case LAPTOP_MODE.SIRTECH:
                //                RenderSirTech();
                break;
            case LAPTOP_MODE.FINANCES:
                //                RenderFinances();
                break;
            case LAPTOP_MODE.PERSONNEL:
                //                RenderPersonnel();
                break;
            case LAPTOP_MODE.HISTORY:
                //                RenderHistory();
                break;
            case LAPTOP_MODE.FILES:
                //                RenderFiles();
                break;
            case LAPTOP_MODE.EMAIL:
                //                RenderEmail();
                break;
            case LAPTOP_MODE.WWW:
                DrawDeskTopBackground();
                RenderWWWProgramTitleBar();
                break;
            case LAPTOP_MODE.BROKEN_LINK:
                //                RenderBrokenLink();
                break;

            case LAPTOP_MODE.BOBBYR_SHIPMENTS:
                //                RenderBobbyRShipments();
                break;
        }

        if (guiCurrentLaptopMode >= LAPTOP_MODE.WWW)
        {
            // render program bar for www program
            RenderWWWProgramTitleBar();
        }



        if (fLoadPendingFlag)
        {
            guiCurrentLaptopMode = uiTempMode;
            return;
        }

        //        DisplayProgramBoundingBox(false);

        // mark the buttons dirty at this point
        ButtonSubSystem.MarkButtonsDirty(buttonList);
    }

    private static bool DrawDeskTopBackground()
    {
        int uiDestPitchBYTES;
        int uiSrcPitchBYTES;
        Image<Rgba32> pDestBuf;
        Image<Rgba32> pSrcBuf;

        Rectangle clip = new()
        {
            // set clipping region
            X = 0,
            Width = 506,
            Y = 0,
            Height = 408 + 19,
        };

        // get surfaces
        //        pDestBuf = video.LockVideoSurface(Surfaces.FRAME_BUFFER, out uiDestPitchBYTES);
        CHECKF(video.GetVideoSurface(out HVSURFACE hSrcVSurface, guiDESKTOP));
        //        pSrcBuf = video.LockVideoSurface(guiDESKTOP, out uiSrcPitchBYTES);


        // blit .pcx for the background onto desktop
        //        video.Blt8BPPDataSubTo16BPPBuffer(
        //            pDestBuf,
        //            uiDestPitchBYTES,
        //            hSrcVSurface,
        //            pSrcBuf,
        //            uiSrcPitchBYTES,
        //            LAPTOP_SCREEN_UL_X - 2,
        //            LAPTOP_SCREEN_UL_Y - 3,
        //            out clip);


        // release surfaces
        //        video.UnLockVideoSurface(guiDESKTOP);
        //        video.UnLockVideoSurface(Surfaces.FRAME_BUFFER);

        return true;
    }

    public static bool RenderWWWProgramTitleBar()
    {
        // will render the title bar for the www program
        HVOBJECT hHandle;
        int iIndex = 0;
        string sString = string.Empty;

        // title bar - load
        CHECKF(video.GetVideoObject("LAPTOP\\programtitlebar.sti", out string uiTITLEFORWWW));

        // blit title
        hHandle = video.GetVideoObject(uiTITLEFORWWW);
        VideoObjectManager.BltVideoObject(SurfaceType.FRAME_BUFFER, hHandle, 0, LAPTOP_SCREEN_UL_X, LAPTOP_SCREEN_UL_Y - 2, VO_BLT.SRCTRANSPARENCY, null);


        // now delete
        video.DeleteVideoObjectFromIndex(uiTITLEFORWWW);

        // now slapdown text
        FontSubSystem.SetFont(FontStyle.FONT14ARIAL);
        FontSubSystem.SetFontForeground(FontColor.FONT_WHITE);
        FontSubSystem.SetFontBackground(FontColor.FONT_BLACK);

        // display title

        // no page loaded yet, do not handle yet


        if (guiCurrentLaptopMode == LAPTOP_MODE.WWW)
        {
            //            mprintf(140, 33, pWebTitle[0]);
        }

        else
        {
            iIndex = guiCurrentLaptopMode - LAPTOP_MODE.WWW - 1;

            //            wprintf(sString, "%s  -  %s", pWebTitle[0], pWebPagesTitles[iIndex]);
            mprintf(140, 33, sString);
        }

        BlitTitleBarIcons();

        //        DisplayProgramBoundingBox(false);

        //InvalidateRegion( 0, 0, 640, 480 );
        return true;
    }

    public static void BlitTitleBarIcons()
    {
        HVOBJECT? hHandle;
        // will blit the icons for the title bar of the program we are in
        switch (guiCurrentLaptopMode)
        {
            case LAPTOP_MODE.HISTORY:
                hHandle = hHandle = video.GetVideoObject(guiTITLEBARICONS);
                VideoObjectManager.BltVideoObject(SurfaceType.FRAME_BUFFER, hHandle, 4, LAPTOP_TITLE_ICONS_X, LAPTOP_TITLE_ICONS_Y, VO_BLT.SRCTRANSPARENCY, null);
                break;
            case LAPTOP_MODE.EMAIL:
                hHandle = hHandle = video.GetVideoObject(guiTITLEBARICONS);
                VideoObjectManager.BltVideoObject(SurfaceType.FRAME_BUFFER, hHandle, 0, LAPTOP_TITLE_ICONS_X, LAPTOP_TITLE_ICONS_Y, VO_BLT.SRCTRANSPARENCY, null);
                break;
            case LAPTOP_MODE.PERSONNEL:
                hHandle = video.GetVideoObject(guiTITLEBARICONS);
                VideoObjectManager.BltVideoObject(SurfaceType.FRAME_BUFFER, hHandle, 3, LAPTOP_TITLE_ICONS_X, LAPTOP_TITLE_ICONS_Y, VO_BLT.SRCTRANSPARENCY, null);
                break;
            case LAPTOP_MODE.FINANCES:
                hHandle = hHandle = video.GetVideoObject(guiTITLEBARICONS);
                VideoObjectManager.BltVideoObject(SurfaceType.FRAME_BUFFER, hHandle, 5, LAPTOP_TITLE_ICONS_X, LAPTOP_TITLE_ICONS_Y, VO_BLT.SRCTRANSPARENCY, null);
                break;
            case LAPTOP_MODE.FILES:
                hHandle = video.GetVideoObject(guiTITLEBARICONS);
                VideoObjectManager.BltVideoObject(SurfaceType.FRAME_BUFFER, hHandle, 2, LAPTOP_TITLE_ICONS_X, LAPTOP_TITLE_ICONS_Y, VO_BLT.SRCTRANSPARENCY, null);
                break;
            case LAPTOP_MODE.NONE:
                // do nothing
                break;
            default:
                // www pages
                hHandle = video.GetVideoObject(guiTITLEBARICONS);
                VideoObjectManager.BltVideoObject(SurfaceType.FRAME_BUFFER, hHandle, 1, LAPTOP_TITLE_ICONS_X, LAPTOP_TITLE_ICONS_Y, VO_BLT.SRCTRANSPARENCY, null);
                break;
        }
    }

    public static void SetLaptopNewGameFlag()
    {
        LaptopSaveInfo.gfNewGameLaptop = true;
    }

    public static void SetLaptopExitScreen(ScreenName exitScreen)
    {
        guiExitScreen = exitScreen;
    }

    internal static void DoLapTopSystemMessageBoxWithRect(MessageBoxStyle mSG_BOX_LAPTOP_DEFAULT, string zString, ScreenName lAPTOP_SCREEN, MSG_BOX_FLAG usFlags, MSGBOX_CALLBACK? returnCallback, Rectangle? pCenteringRect)
    {
        throw new NotImplementedException();
    }
}

// icons text id's
public enum LaptopIcons
{
    MAIL = 0,
    WWW,
    FINANCIAL,
    PERSONNEL,
    HISTORY,
    FILES,
    MAX_ICON_COUNT,
};

public enum LaptopRegions
{
    NO_REGION = 0,
    EMAIL_REGION,
    WWW_REGION,
    FINANCIAL_REGION,
    PERSONNEL_REGION,
    HISTORY_REGION,
    FILES_REGION,
};

// laptop programs
public enum LAPTOP_PROGRAM
{
    MAILER,
    WEB_BROWSER,
    FILES,
    PERSONNEL,
    FINANCES,
    HISTORY,

};

// laptop program states
public enum LAPTOP_PROGRAM_STATES
{
    MINIMIZED,
    OPEN,
};

public enum LAPTOP_MODE
{
    NONE = 0,
    FINANCES,
    PERSONNEL,
    HISTORY,
    FILES,
    FILES_ENRICO,
    FILES_PLANS,
    EMAIL,
    EMAIL_NEW,
    EMAIL_VIEW,
    WWW,
    AIM,
    AIM_MEMBERS,
    AIM_MEMBERS_FACIAL_INDEX,
    AIM_MEMBERS_SORTED_FILES,
    AIM_MEMBERS_SORTED_FILES_VIDEO,
    AIM_MEMBERS_ARCHIVES,
    AIM_POLICIES,
    AIM_HISTORY,
    AIM_LINKS,
    MERC,
    MERC_ACCOUNT,
    MERC_NO_ACCOUNT,
    MERC_FILES,
    BOBBY_R,
    BOBBY_R_GUNS,
    BOBBY_R_AMMO,
    BOBBY_R_ARMOR,
    BOBBY_R_MISC,
    BOBBY_R_USED,
    BOBBY_R_MAILORDER,
    CHAR_PROFILE,
    CHAR_PROFILE_QUESTIONAIRE,
    FLORIST,
    FLORIST_FLOWER_GALLERY,
    FLORIST_ORDERFORM,
    FLORIST_CARD_GALLERY,
    INSURANCE,
    INSURANCE_INFO,
    INSURANCE_CONTRACT,
    INSURANCE_COMMENTS,
    FUNERAL,
    SIRTECH,
    BROKEN_LINK,
    BOBBYR_SHIPMENTS,
};

// bookamrks for WWW bookmark list
public enum LaptopPanel
{
    FIRST_SIDE_PANEL = 1,
    SECOND_SIDE_PANEL,
};

public enum LAPTOP
{
    NO_CURSOR = 0,
    PANEL_CURSOR,
    SCREEN_CURSOR,
    WWW_CURSOR,
};

// the bookmark values, move cancel down as bookmarks added

public enum BOOKMARK
{
    AIM_BOOKMARK = 0,
    BOBBYR_BOOKMARK,
    IMP_BOOKMARK,
    MERC_BOOKMARK,
    FUNERAL_BOOKMARK,
    FLORIST_BOOKMARK,
    INSURANCE_BOOKMARK,
    CANCEL_STRING,
};
