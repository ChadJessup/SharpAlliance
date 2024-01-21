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
using System.Diagnostics;
using static System.Runtime.InteropServices.JavaScript.JSType;
using SharpAlliance.Core.SubSystems.LaptopSubSystem.IMPSubSystem;

namespace SharpAlliance.Core.SubSystems.LaptopSubSystem;

public partial class Laptop
{
    private const int NUM_AIM_POLICY_PAGES = 11;

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

        foreach (var key in Enum.GetValues<LAPTOP_PROGRAM>())
        {
            gLaptopProgramStates[key] = LAPTOP_PROGRAM_STATES.MINIMIZED;
        }

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

    public static bool InitLaptopAndLaptopScreens()
    {
        Finances.GameInitFinances();
        history.GameInitHistory();

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
        // will blit the icons for the title bar of the program we are in
        switch (guiCurrentLaptopMode)
        {
            case LAPTOP_MODE.HISTORY:
                video.BltVideoObject(SurfaceType.FRAME_BUFFER, guiTITLEBARICONS, 4, LAPTOP_TITLE_ICONS_X, LAPTOP_TITLE_ICONS_Y, VO_BLT.SRCTRANSPARENCY, null);
                break;
            case LAPTOP_MODE.EMAIL:
                video.BltVideoObject(SurfaceType.FRAME_BUFFER, guiTITLEBARICONS, 0, LAPTOP_TITLE_ICONS_X, LAPTOP_TITLE_ICONS_Y, VO_BLT.SRCTRANSPARENCY, null);
                break;
            case LAPTOP_MODE.PERSONNEL:
                video.BltVideoObject(SurfaceType.FRAME_BUFFER, guiTITLEBARICONS, 3, LAPTOP_TITLE_ICONS_X, LAPTOP_TITLE_ICONS_Y, VO_BLT.SRCTRANSPARENCY, null);
                break;
            case LAPTOP_MODE.FINANCES:
                video.BltVideoObject(SurfaceType.FRAME_BUFFER, guiTITLEBARICONS, 5, LAPTOP_TITLE_ICONS_X, LAPTOP_TITLE_ICONS_Y, VO_BLT.SRCTRANSPARENCY, null);
                break;
            case LAPTOP_MODE.FILES:
                video.BltVideoObject(SurfaceType.FRAME_BUFFER, guiTITLEBARICONS, 2, LAPTOP_TITLE_ICONS_X, LAPTOP_TITLE_ICONS_Y, VO_BLT.SRCTRANSPARENCY, null);
                break;
            case LAPTOP_MODE.NONE:
                // do nothing
                break;
            default:
                // www pages
                video.BltVideoObject(SurfaceType.FRAME_BUFFER, guiTITLEBARICONS, 1, LAPTOP_TITLE_ICONS_X, LAPTOP_TITLE_ICONS_Y, VO_BLT.SRCTRANSPARENCY, null);
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

    private static bool fEnteredFromGameStartup = true;

    public static bool EnterLaptop()
    {
        //Create, load, initialize data -- just entered the laptop.

        VOBJECT_DESC VObjectDesc;
        int iCounter = 0;

        // we are re entering due to message box, leave NOW!
        if (fExitDueToMessageBox == true)
        {

            return (true);
        }

        //if the radar map mouse region is still active, disable it.
        if (gRadarRegion.uiFlags.HasFlag(MouseRegionFlags.MSYS_REGION_ENABLED))
        {
            MouseSubSystem.MSYS_DisableRegion(ref gRadarRegion);
            /*
                    #ifdef JA2BETAVERSION
                        DoLapTopMessageBox( MSG_BOX_LAPTOP_DEFAULT, L"Mapscreen's radar region is still active, please tell Dave how you entered Laptop.", LAPTOP_SCREEN, MSG_BOX_FLAG_OK, NULL );
                    #endif
            */
        }

        gfDontStartTransitionFromLaptop = false;

        //Since we are coming in from MapScreen, uncheck the flag
        guiTacticalInterfaceFlags &= ~INTERFACE.MAPSCREEN;

        // ATE: Disable messages....
        Messages.DisableScrollMessages();

        // Stop any person from saying anything
        DialogControl.StopAnyCurrentlyTalkingSpeech();

        // Don't play music....
        //        SetMusicMode(MUSIC_LAPTOP);

        // Stop ambients...
        StopAmbients();

        //if its raining, start the rain showers
        if (IsItRaining())
        {
            //Enable the rain delay warning
            giRainDelayInternetSite = BOOKMARK.UNSET;

            //lower the volume 
            //            guiRainLoop = PlayJA2Ambient(RAIN_1, LOWVOLUME, 0);
        }


        //open the laptop library
        //	OpenLibrary( LIBRARY_LAPTOP );

        //pause the game because we dont want time to advance in the laptop
        GameClock.PauseGame();

        // set the fact we are currently in laptop, for rendering purposes
        fCurrentlyInLaptop = true;



        // clear guiSAVEBUFFER
        //ColorFillVideoSurfaceArea(guiSAVEBUFFER,	0, 0, 640, 480, Get16BPPColor(FROMRGB(0, 0, 0)) );
        // disable characters panel buttons

        // reset redraw flag and redraw new mail
        fReDrawScreenFlag = false;
        Emails.fReDrawNewMailFlag = true;

        // setup basic cursors
        guiCurrentLapTopCursor = LAPTOP_CURSOR.PANEL_CURSOR;
        guiPreviousLapTopCursor = LAPTOP_CURSOR.NO_CURSOR;

        // sub page
        giCurrentSubPage = 0;
        giCurrentRegion = LaptopRegions.EMAIL_REGION;

        // load the laptop graphic and add it

        guiLAPTOP = video.GetVideoObject(Utils.FilenameForBPP("LAPTOP\\laptop3.sti"));

        // background for panel
        guiLaptopBACKGROUND = video.GetVideoObject(Utils.FilenameForBPP("LAPTOP\\taskbar.sti"));

        // background for panel
        guiTITLEBARLAPTOP = video.GetVideoObject(Utils.FilenameForBPP("LAPTOP\\programtitlebar.sti"));

        // lights for power and HD
        guiLIGHTS = video.GetVideoObject(Utils.FilenameForBPP("LAPTOP\\lights.sti"));

        // icons for title bars
        guiTITLEBARICONS = video.GetVideoObject(Utils.FilenameForBPP("LAPTOP\\ICONS.sti"));

        // load, blt and delete graphics
        guiEmailWarning = video.GetVideoObject(Utils.FilenameForBPP("LAPTOP\\NewMailWarning.sti"));

        // load background
        LoadDesktopBackground();


        guiCurrentLaptopMode = LAPTOP_MODE.NONE;
        //MSYS_SetCurrentCursor(CURSOR_NORMAL);

        guiCurrentLaptopMode = LAPTOP_MODE.NONE;
        guiPreviousLaptopMode = LAPTOP_MODE.NONE;
        guiCurrentWWWMode = LAPTOP_MODE.NONE;
        guiCurrentSidePanel = LaptopPanel.FIRST_SIDE_PANEL;
        guiPreviousSidePanel = LaptopPanel.FIRST_SIDE_PANEL;
        gfSideBarFlag = false;
        CreateLapTopMouseRegions();
        RenderLapTopImage();
        HighLightRegion(giCurrentRegion);
        //AddEmailMessage(L"Entered LapTop",L"Entered", 0, 0);
        //for(iCounter=0; iCounter <10; iCounter++)
        //{
        //AddEmail(3,5,0,0);
        //}
        // the laptop mouse region


        // reset bookmarks flags
        fFirstTimeInLaptop = true;

        // reset all bookmark visits
        foreach (var key in Enum.GetValues<BOOKMARK>())
        {
            LaptopSaveInfo.fVisitedBookmarkAlready[key] = false;
        }

        // init program states
        foreach (var key in Enum.GetValues<LAPTOP_PROGRAM>())
        {
            gLaptopProgramStates[key] = LAPTOP_PROGRAM_STATES.MINIMIZED;
        }

        // turn the power on
        fPowerLightOn = true;

        // we are not exiting laptop right now, we just got here
        fExitingLaptopFlag = false;

        // reset program we are maximizing
        bProgramBeingMaximized = (LAPTOP_PROGRAM)(-1);

        // reset fact we are maximizing/ mining
        fMaximizingProgram = false;
        fMinizingProgram = false;


        // initialize open queue
        InitLaptopOpenQueue();


        gfShowBookmarks = false;
        LoadBookmark();
        SetBookMark(BOOKMARK.AIM_BOOKMARK);
        LoadLoadPending();

        DrawDeskTopBackground();

        // create region for new mail icon
        CreateDestroyMouseRegionForNewMailIcon();

        //DEF: Added to Init things in various laptop pages
        EnterLaptopInitLaptopPages();
        InitalizeSubSitesList();

        fShowAtmPanelStartButton = true;

        video.InvalidateRegion(0, 0, 640, 480);

        return (true);
    }

    private static void StopAmbients()
    {
        //SoundStopAllRandom();
    }

    private static bool LoadDesktopBackground()
    {
        // load desktop background
        VSURFACE_DESC vs_desc;

        string path;
        MultilanguageGraphicUtils.GetMLGFilename(out path, MLG.DESKTOP);
        var obj = video.GetVideoObject(path);
        guiDESKTOP = video.Surfaces.CreateSurface(obj);

        return true;

    }

    private static void CreateLapTopMouseRegions()
    {
        // the entire laptop display region
        MouseSubSystem.MSYS_DefineRegion(
            gLapTopScreenRegion,
            LaptopScreenRect,
            MSYS_PRIORITY.NORMAL + 1,
            CURSOR.LAPTOP_SCREEN,
            ScreenRegionMvtCallback,
            LapTopScreenCallBack);
    }

    private static void ScreenRegionMvtCallback(ref MOUSE_REGION region, MSYS_CALLBACK_REASON iReason)
    {
        if (iReason.HasFlag(MSYS_CALLBACK_REASON.INIT))
        {
            return;
        }
    }

    private static void InitLaptopOpenQueue()
    {
        foreach(var program in Enum.GetValues<LAPTOP_PROGRAM>())
        {
            gLaptopProgramQueueList[program] = 1;
        }
    }

    private static void LoadBookmark()
    {
        VOBJECT_DESC VObjectDesc;


        // grab download bars too

        guiDOWNLOADTOP = video.GetVideoObject(Utils.FilenameForBPP("LAPTOP\\downloadtop.sti"));
        guiDOWNLOADMID = video.GetVideoObject(Utils.FilenameForBPP("LAPTOP\\downloadmid.sti"));
        guiDOWNLOADBOT = video.GetVideoObject(Utils.FilenameForBPP("LAPTOP\\downloadbot.sti"));
        guiBOOKTOP = video.GetVideoObject(Utils.FilenameForBPP("LAPTOP\\bookmarktop.sti"));
        guiBOOKMID = video.GetVideoObject(Utils.FilenameForBPP("LAPTOP\\bookmarkmiddle.sti"));
        guiBOOKMARK = video.GetVideoObject(Utils.FilenameForBPP("LAPTOP\\webpages.sti"));
        guiBOOKHIGH = video.GetVideoObject(Utils.FilenameForBPP("LAPTOP\\hilite.sti"));
        guiBOOKBOT = video.GetVideoObject(Utils.FilenameForBPP("LAPTOP\\Bookmarkbottom.sti"));
    }

    private static void LoadLoadPending()
    {
        // function will load the load pending graphics
        // reuse bookmark
        // load graph window and bar
        guiGRAPHWINDOW = video.GetVideoObject(Utils.FilenameForBPP("LAPTOP\\graphwindow.sti"));
        guiGRAPHBAR = video.GetVideoObject(Utils.FilenameForBPP("LAPTOP\\graphsegment.sti"));
    }

    private static void InitalizeSubSitesList()
    {
        // init all subsites list to not visited
        for (var iCounter = LAPTOP_MODE.WWW + 1; iCounter <= LAPTOP_MODE.SIRTECH; iCounter++)
        {
            gfWWWaitSubSitesVisitedFlags[iCounter - (LAPTOP_MODE.WWW + 1)] = false;
        }
    }

    private static void EnterLaptopInitLaptopPages()
    {
        EnterInitAimMembers();
        EnterInitAimArchives();
        EnterInitAimPolicies();
        EnterInitAimHistory();
        Florist.EnterInitFloristGallery();
        Insurance.EnterInitInsuranceInfo();
        BobbyR.EnterInitBobbyRayOrder();
        Mercs.EnterInitMercSite();

        // init sub pages for WW Wait
        IMP.InitIMPSubPageList();
    }

    private static void EnterInitAimHistory()
    {
        Array.Fill(AimHistorySubPagesVisitedFlag, false);
    }

    private static void EnterInitAimPolicies()
    {
        Array.Fill(AimPoliciesSubPagesVisitedFlag, false);
    }

    private static void EnterInitAimArchives()
    {
        gfDrawPopUpBox = false;
        gfDestroyPopUpBox = false;

        Array.Fill(AimArchivesSubPagesVisitedFlag, false);
        AimArchivesSubPagesVisitedFlag[0] = true;
    }

    private static void EnterInitAimMembers()
    {
        gubVideoConferencingMode = AIM_VIDEO.NOT_DISPLAYED_MODE;
        gubVideoConferencingPreviousMode = AIM_VIDEO.NOT_DISPLAYED_MODE;
        gfVideoFaceActive = false;
        //fShouldMercTalk = FALSE;	
        gubPopUpBoxAction = AIM_POPUP.NOTHING;
        gfRedrawScreen = false;
        giContractAmount = 0;
        giMercFaceIndex = 0;
        guiLastHandleMercTime = GetJA2Clock();
        gubCurrentCount = 0;
        gfFirstTimeInContactScreen = true;

        //reset the variable so a pop up can be displyed this time in laptop
        LaptopSaveInfo.sLastHiredMerc.fHaveDisplayedPopUpInLaptop = false;

        //reset the id of the last merc
        LaptopSaveInfo.sLastHiredMerc.iIdOfMerc = -1;
    }

    private static bool fCreated = false;
    private static bool fUnReadMailFlag;
    private static bool fOpenMostRecentUnReadFlag;
    private static bool fEnteredFileViewerFromNewFileIcon;
    private static AIM_VIDEO gubVideoConferencingPreviousMode;
    private static bool gfVideoFaceActive;
    private static AIM_POPUP gubPopUpBoxAction;
    private static bool gfRedrawScreen;
    private static int giContractAmount;
    private static int giMercFaceIndex;
    private static uint guiLastHandleMercTime;
    private static int gubCurrentCount;
    private static bool gfFirstTimeInContactScreen;

    public static bool[] AimArchivesSubPagesVisitedFlag = new bool[3];
    private static bool gfDrawPopUpBox;
    private static bool gfDestroyPopUpBox;
    private static bool[] AimPoliciesSubPagesVisitedFlag = new bool[NUM_AIM_POLICY_PAGES];
    private static bool[] AimHistorySubPagesVisitedFlag = new bool[NUM_AIM_HISTORY_PAGES];

    private static void CreateDestroyMouseRegionForNewMailIcon()
    {

        //. will toggle creation/destruction of the mouse regions used by the new mail icon

        if (fCreated == false)
        {
            fCreated = true;
            MouseSubSystem.MSYS_DefineRegion(
                gNewMailIconRegion,
                new Rectangle(LAPTOP__NEW_EMAIL_ICON_X, LAPTOP__NEW_EMAIL_ICON_Y + 5, LAPTOP__NEW_EMAIL_ICON_X + 16, LAPTOP__NEW_EMAIL_ICON_Y + 16),
                MSYS_PRIORITY.HIGHEST - 3,
                CURSOR.LAPTOP_SCREEN,
                MSYS_NO_CALLBACK,
                NewEmailIconCallback);

            CreateFileAndNewEmailIconFastHelpText(
                LaptopText.LAPTOP_BN_HLP_TXT_YOU_HAVE_NEW_MAIL,
                (fUnReadMailFlag == false));

            MouseSubSystem.MSYS_DefineRegion(
                gNewFileIconRegion,
                new Rectangle(LAPTOP__NEW_FILE_ICON_X, LAPTOP__NEW_FILE_ICON_Y + 5, LAPTOP__NEW_FILE_ICON_X + 16, LAPTOP__NEW_FILE_ICON_Y + 16),
                MSYS_PRIORITY.HIGHEST - 3,
                CURSOR.LAPTOP_SCREEN,
                MSYS_NO_CALLBACK,
                NewFileIconCallback);

            CreateFileAndNewEmailIconFastHelpText(
                LaptopText.LAPTOP_BN_HLP_TXT_YOU_HAVE_NEW_FILE,
                (fNewFilesInFileViewer == false));
        }
        else
        {
            fCreated = false;
            MouseSubSystem.MSYS_RemoveRegion(gNewMailIconRegion);
            MouseSubSystem.MSYS_RemoveRegion(gNewFileIconRegion);
        }
    }

    private static void NewFileIconCallback(ref MOUSE_REGION region, MSYS_CALLBACK_REASON iReason)
    {
        if (iReason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP))
        {
            if (fNewFilesInFileViewer)
            {
                fEnteredFileViewerFromNewFileIcon = true;
                guiCurrentLaptopMode = LAPTOP_MODE.FILES;
            }
        }
    }

    private static void NewEmailIconCallback(ref MOUSE_REGION region, MSYS_CALLBACK_REASON iReason)
    {
        if (iReason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP))
        {
            if (fUnReadMailFlag)
            {
                fOpenMostRecentUnReadFlag = true;
                guiCurrentLaptopMode = LAPTOP_MODE.EMAIL;
            }
        }

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
    UNSET = -1,
    AIM_BOOKMARK = 0,
    BOBBYR_BOOKMARK,
    IMP_BOOKMARK,
    MERC_BOOKMARK,
    FUNERAL_BOOKMARK,
    FLORIST_BOOKMARK,
    INSURANCE_BOOKMARK,
    CANCEL_STRING,
};

public enum LAPTOP_CURSOR
{
    NO_CURSOR = 0,
    PANEL_CURSOR,
    SCREEN_CURSOR,
    WWW_CURSOR,
};

// enumerated types used for the Video Conferencing Display
public enum AIM_VIDEO
{
    NOT_DISPLAYED_MODE,                           // The video popup is not displayed
    POPUP_MODE,                                           // The title bar pops up out of the Contact button
    INIT_MODE,                                            // When the player first tries to contact the merc, it will be snowy for a bit
    FIRST_CONTACT_MERC_MODE,              // The popup that is displayed when first contactinf the merc
    HIRE_MERC_MODE,                                   // The popup which deals with the contract length, and transfer funds
    MERC_ANSWERING_MACHINE_MODE,      // The popup which will be instread of the AIM_VIDEO_FIRST_CONTACT_MERC_MODE if the merc is not there
    MERC_UNAVAILABLE_MODE,                    // The popup which will be instread of the AIM_VIDEO_FIRST_CONTACT_MERC_MODE if the merc is unavailable
    POPDOWN_MODE,										// The title bars pops down to the contact button
};
