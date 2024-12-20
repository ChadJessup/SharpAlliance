﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SharpAlliance.Core.Interfaces;
using SharpAlliance.Core.Managers;
using SharpAlliance.Core.Managers.VideoSurfaces;
using SharpAlliance.Core.Screens;
using SharpAlliance.Core.SubSystems.LaptopSubSystem.BobbyRSubSystem;
using SharpAlliance.Core.SubSystems.LaptopSubSystem.FloristSubSystem;
using SharpAlliance.Core.SubSystems.LaptopSubSystem.IMPSubSystem;
using SharpAlliance.Core.SubSystems.LaptopSubSystem.InsuranceSubSystem;
using SixLabors.ImageSharp;

namespace SharpAlliance.Core.SubSystems.LaptopSubSystem;

public partial class Laptop : IScreen
{
    private readonly ILogger<Laptop> logger;
    private static IVideoManager video;
    private readonly IScreenManager screens;

    public bool IsInitialized { get; set; }
    public ScreenState State { get; set; }
    private static History history;
    private static IFileManager files;
    public static readonly List<GUI_BUTTON> buttonList = [];

    public Laptop(
        ILogger<Laptop> logger,
        IVideoManager videoManager,
        IScreenManager screenManager,
        History history,
        IFileManager fileManager)
    {
        this.logger = logger;
        files = fileManager;
        Laptop.history = history;
        video = videoManager;
        screens = screenManager;
    }

    public ValueTask Activate()
    {
        return ValueTask.CompletedTask;
    }

    public ValueTask<bool> Initialize()
    {
        return ValueTask.FromResult(true);
    }

    public void Dispose()
    {
    }

    public ValueTask Deactivate()
    {
        return ValueTask.CompletedTask;
    }

    public ValueTask<ScreenName> Handle()
    {
        //User just changed modes.  This is determined by the button callbacks 
        //created in LaptopScreenInit()

        // just entered
        if (gfEnterLapTop)
        {
            Laptop.EnterLaptop();
            CreateLaptopButtons();
            gfEnterLapTop = false;
        }

        if (gfStartMapScreenToLaptopTransition)
        { //Everything is set up to start the transition animation.
            Rectangle SrcRect1, SrcRect2, DstRect;
            double iPercentage;
            double iScalePercentage, iFactor;
            uint uiStartTime, uiTimeRange, uiCurrTime;
            int iX, iY, iWidth, iHeight;

            double iRealPercentage;

            CursorSubSystem.SetCurrentCursorFromDatabase(CURSOR.VIDEO_NO_CURSOR);
            //Step 1:  Build the laptop image into the save buffer.
            gfStartMapScreenToLaptopTransition = false;
            video.RestoreBackgroundRects();
            RenderLapTopImage();
            HighLightRegion(giCurrentRegion);
            RenderLaptop();
            ButtonSubSystem.RenderButtons(buttonList);
            PrintDate();
            PrintBalance();
            PrintNumberOnTeam();
            ShowLights();

            //Step 2:  The mapscreen image is in the EXTRABUFFER, and laptop is in the SAVEBUFFER
            //         Start transitioning the screen.
            DstRect = new()
            {
                X = 0,
                Y = 0,
                Width = 640,
                Height = 480
            };

            uiTimeRange = 1000;
            iPercentage = iRealPercentage = 0;
            uiStartTime = GetJA2Clock();
            video.BlitBufferToBuffer(SurfaceType.FRAME_BUFFER, SurfaceType.SAVE_BUFFER, new(0, 0, 640, 480));
            video.BlitBufferToBuffer(guiEXTRABUFFER, SurfaceType.FRAME_BUFFER, new(0, 0, 640, 480));
            //PlayJA2SampleFromFile("SOUNDS\\Laptop power up (8-11).wav", RATE_11025, HIGHVOLUME, 1, MIDDLEPAN);
            while (iRealPercentage < 100)
            {
                uiCurrTime = GetJA2Clock();
                iPercentage = (uiCurrTime - uiStartTime) * 100 / uiTimeRange;
                iPercentage = Math.Min(iPercentage, 100);

                iRealPercentage = iPercentage;

                //Factor the percentage so that it is modified by a gravity falling acceleration effect.
                iFactor = (iPercentage - 50) * 2;
                if (iPercentage < 50)
                {
                    iPercentage = (iPercentage + iPercentage * iFactor * 0.01 + 0.5);
                }
                else
                {
                    iPercentage = (iPercentage + (100 - iPercentage) * iFactor * 0.01 + 0.5);
                }

                //Mapscreen source rect
                SrcRect1 = new()
                {
                    X = (int)(464 * iPercentage / 100),
                    Width = (int)(640 - 163 * iPercentage / 100),
                    Y = (int)(417 * iPercentage / 100),
                    Height = (int)(480 - 55 * iPercentage / 100)
                };

                //Laptop source rect
                if (iPercentage < 99)
                {
                    iScalePercentage = 10000 / (100 - iPercentage);
                }
                else
                {
                    iScalePercentage = 5333;
                }

                iWidth = (int)(12 * iScalePercentage / 100);
                iHeight = (int)(9 * iScalePercentage / 100);
                iX = (int)(472 - (472 - 320) * iScalePercentage / 5333);
                iY = (int)(424 - (424 - 240) * iScalePercentage / 5333);

                SrcRect2 = new();
                SrcRect2.X = iX - iWidth / 2;
                SrcRect2.Width = SrcRect2.X + iWidth;
                SrcRect2.Y = iY - iHeight / 2;
                SrcRect2.Height = SrcRect2.Y + iHeight;

                //SrcRect2.X = 464 - 464 * iScalePercentage / 100;
                //SrcRect2.Width = 477 + 163 * iScalePercentage / 100;
                //SrcRect2.Y = 417 - 417 * iScalePercentage / 100;
                //SrcRect2.Height = 425 + 55 * iScalePercentage / 100;

                //BltStretchVideoSurface( FRAME_BUFFER, guiEXTRABUFFER, 0, 0, 0, &SrcRect1, &DstRect );

                //SetFont( FONT10ARIAL );
                //SetFontForeground( FONT_YELLOW );
                //SetFontShadow( FONT_NEARBLACK );
                //mprintf( 10, 10, L"%d . %d", iRealPercentage, iPercentage );
                //pDestBuf = LockVideoSurface( FRAME_BUFFER, &uiDestPitchBYTES );
                //SetClippingRegionAndImageWidth( uiDestPitchBYTES, 0, 0, 640, 480 );
                //RectangleDraw( true, SrcRect1.X, SrcRect1.Y, SrcRect1.Width, SrcRect1.Height, Get16BPPColor( FROMRGB( 255, 100, 0 ) ), pDestBuf );
                //RectangleDraw( true, SrcRect2.X, SrcRect2.Y, SrcRect2.Width, SrcRect2.Height, Get16BPPColor( FROMRGB( 100, 255, 0 ) ), pDestBuf );
                //UnLockVideoSurface( FRAME_BUFFER );

                video.BltStretchVideoSurface(SurfaceType.FRAME_BUFFER, SurfaceType.SAVE_BUFFER, new(0, 0), 0, DstRect, SrcRect2);
                video.InvalidateScreen();
                //gfPrintFrameBuffer = true;
                video.RefreshScreen();
            }
            fReDrawScreenFlag = true;
        }


        //DO NOT MOVE THIS FUNCTION CALL!!!

        //This determines if the help screen should be active
        if (HelpScreen.ShouldTheHelpScreenComeUp(HELP_SCREEN.LAPTOP, false))
        {
            // handle the help screen
            HelpScreen.HelpScreenHandler();

            return ValueTask.FromResult(ScreenName.LAPTOP_SCREEN);
        }

        video.RestoreBackgroundRects();

        // lock cursor to screen
        MouseSubSystem.RestrictMouseCursor(LaptopScreenRect);



        // handle animated cursors
        CursorSubSystem.HandleAnimatedCursors();
        // Deque all game events
        EventPump.DequeAllGameEvents(true);

        // handle sub sites..like BR Guns, BR Ammo, Armour, Misc...for WW Wait..since they are not true sub pages
        // and are not individual sites
        HandleWWWSubSites();
        UpdateStatusOfDisplayingBookMarks();

        // check if we need to reset new WWW mode
        CheckIfNewWWWW();

        if (guiCurrentLaptopMode != guiPreviousLaptopMode)
        {
            if (guiCurrentLaptopMode <= LAPTOP_MODE.WWW)
            {
                fLoadPendingFlag = false;
            }

            if ((fMaximizingProgram == false) && (fMinizingProgram == false))
            {
                if (guiCurrentLaptopMode <= LAPTOP_MODE.WWW)
                {
                    EnterNewLaptopMode();
                    if ((fMaximizingProgram == false) && (fMinizingProgram == false))
                    {
                        guiPreviousLaptopMode = guiCurrentLaptopMode;
                    }
                }
                else
                {
                    if (!fLoadPendingFlag)
                    {
                        EnterNewLaptopMode();
                        guiPreviousLaptopMode = guiCurrentLaptopMode;
                    }
                }
            }
        }
        if (fPausedReDrawScreenFlag)
        {
            fReDrawScreenFlag = true;
            fPausedReDrawScreenFlag = false;
        }

        if (fReDrawScreenFlag)
        {
            Laptop.RenderLapTopImage();
            HighLightRegion(giCurrentRegion);
            Laptop.RenderLaptop();
        }

        // are we about to leave laptop
        if (fExitingLaptopFlag)
        {
            if (fLoadPendingFlag == true)
            {
                fLoadPendingFlag = false;
                fExitDuringLoad = true;
            }

            LeaveLapTopScreen();
        }

        if (fExitingLaptopFlag == false)
        {
            // handle handles for laptop input stream
            HandleLapTopHandles();
        }

        // get keyboard input, handle it
        GetLaptopKeyboardInput();

        // check to see if new mail box needs to be displayed
        Emails.DisplayNewMailBox();
        Emails.CreateDestroyNewMailButton();

        // create various mouse regions that are global to laptop system
        CreateDestoryBookMarkRegions();
        CreateDestroyErrorButton();

        // check to see if error box needs to be displayed
        DisplayErrorBox();

        // check to see if buttons marked dirty
        CheckMarkButtonsDirtyFlag();

        // check to see if new mail box needs to be displayed
        ShouldNewMailBeDisplayed();

        // check to see if new mail box needs to be displayed
        Emails.ReDrawNewMailBox();


        // look for unread email
        LookForUnread();
        //Handle keyboard shortcuts...

        // mouse regions
        //HandleLapTopScreenMouseUi(); 
        //RenderButtons();
        //RenderButtonsFastHelp( );



        if ((fLoadPendingFlag == false) || (Emails.fNewMailFlag))
        {
            // render buttons marked dirty
            ButtonSubSystem.RenderButtons(buttonList);

            // render fast help 'quick created' buttons
            //		RenderFastHelp( );
            //	  RenderButtonsFastHelp( );
        }

        // show text on top of buttons
        if ((fMaximizingProgram == false) && (fMinizingProgram == false))
        {
            DrawButtonText();
        }


        // check to see if bookmarks need to be displayed
        if (gfShowBookmarks)
        {
            if (fExitingLaptopFlag)
            {
                gfShowBookmarks = false;
            }
            else
            {
                DisplayBookMarks();
            }
        }

        // check to see if laod pending flag is set
        DisplayLoadPending();

        // check if we are showing message?
        DisplayWebBookMarkNotify();

        if ((fIntermediateReDrawFlag) || (fReDrawPostButtonRender))
        {
            // rendering AFTER buttons and button text
            if ((fMaximizingProgram == false) && (fMinizingProgram == false))
            {
                PostButtonRendering();
            }
        }
        //PrintBalance( );

        PrintDate();

        PrintBalance();

        PrintNumberOnTeam();
        DisplayTaskBarIcons();

        // handle if we are maximizing a program from a minimized state or vice versa
        HandleSlidingTitleBar();


        // flicker HD light as nessacary
        FlickerHDLight();

        // display power and HD lights
        ShowLights();


        // render frame rate
        DisplayFrameRate();

        // invalidate screen if redrawn
        if (fReDrawScreenFlag == true)
        {
            video.InvalidateRegion(0, 0, 640, 480);
            fReDrawScreenFlag = false;
        }

        RenderDirty.ExecuteVideoOverlays();

        video.SaveBackgroundRects();
        //	RenderButtonsFastHelp();
        MouseSubSystem.RenderFastHelp();

        // ex SAVEBUFFER queue
        video.ExecuteBaseDirtyRectQueue();
        Interface.ResetInterface();
        video.EndFrameBufferRender();


        return ValueTask.FromResult(ScreenName.LAPTOP_SCREEN);
    }

    private void HandleSlidingTitleBar()
    {
        if ((fMaximizingProgram == false) && (fMinizingProgram == false))
        {
            return;
        }

        if (fExitingLaptopFlag)
        {
            return;
        }

        if (fMaximizingProgram)
        {
            switch (bProgramBeingMaximized)
            {
                case (LAPTOP_PROGRAM.MAILER):
                    fMaximizingProgram = !DisplayTitleBarMaximizeGraphic(true, fInitTitle, 29, 66, 29 + 20);
                    if (fMaximizingProgram == false)
                    {
                        RemoveTitleBarMaximizeGraphics();
                        fEnteredNewLapTopDueToHandleSlidingBars = true;
                        EnterNewLaptopMode();
                        fEnteredNewLapTopDueToHandleSlidingBars = false;
                        fPausedReDrawScreenFlag = true;
                    }
                    break;
                case (LAPTOP_PROGRAM.FILES):
                    fMaximizingProgram = !DisplayTitleBarMaximizeGraphic(true, fInitTitle, 29, 120, 29 + 20);
                    if (fMaximizingProgram == false)
                    {
                        RemoveTitleBarMaximizeGraphics();
                        fEnteredNewLapTopDueToHandleSlidingBars = true;
                        EnterNewLaptopMode();
                        fEnteredNewLapTopDueToHandleSlidingBars = false;
                        fPausedReDrawScreenFlag = true;
                    }
                    break;
                case (LAPTOP_PROGRAM.FINANCES):
                    fMaximizingProgram = !DisplayTitleBarMaximizeGraphic(true, fInitTitle, 29, 226, 29 + 20);
                    if (fMaximizingProgram == false)
                    {
                        RemoveTitleBarMaximizeGraphics();
                        fEnteredNewLapTopDueToHandleSlidingBars = true;
                        EnterNewLaptopMode();
                        fEnteredNewLapTopDueToHandleSlidingBars = false;
                        fPausedReDrawScreenFlag = true;
                    }
                    break;
                case (LAPTOP_PROGRAM.PERSONNEL):
                    fMaximizingProgram = !DisplayTitleBarMaximizeGraphic(true, fInitTitle, 29, 194, 29 + 20);
                    if (fMaximizingProgram == false)
                    {
                        RemoveTitleBarMaximizeGraphics();
                        fEnteredNewLapTopDueToHandleSlidingBars = true;
                        EnterNewLaptopMode();
                        fEnteredNewLapTopDueToHandleSlidingBars = false;
                        fPausedReDrawScreenFlag = true;
                    }
                    break;
                case (LAPTOP_PROGRAM.HISTORY):
                    fMaximizingProgram = !DisplayTitleBarMaximizeGraphic(true, fInitTitle, 29, 162, 29 + 20);
                    if (fMaximizingProgram == false)
                    {
                        RemoveTitleBarMaximizeGraphics();
                        fEnteredNewLapTopDueToHandleSlidingBars = true;
                        EnterNewLaptopMode();
                        fEnteredNewLapTopDueToHandleSlidingBars = false;
                        fPausedReDrawScreenFlag = true;
                    }
                    break;
                case (LAPTOP_PROGRAM.WEB_BROWSER):
                    fMaximizingProgram = !DisplayTitleBarMaximizeGraphic(true, fInitTitle, 29, 99, 29 + 20);
                    if (fMaximizingProgram == false)
                    {
                        RemoveTitleBarMaximizeGraphics();
                        fEnteredNewLapTopDueToHandleSlidingBars = true;
                        EnterNewLaptopMode();
                        fEnteredNewLapTopDueToHandleSlidingBars = false;
                        fPausedReDrawScreenFlag = true;
                    }
                    break;
            }

            ButtonSubSystem.MarkButtonsDirty(buttonList);
        }
        else
        {
            // minimizing
            switch (bProgramBeingMaximized)
            {
                case (LAPTOP_PROGRAM.MAILER):
                    fMinizingProgram = !DisplayTitleBarMaximizeGraphic(false, fInitTitle, 29, 66, 29 + 20);
                    if (fMinizingProgram == false)
                    {
                        RemoveTitleBarMaximizeGraphics();
                        EnterNewLaptopMode();
                        fEnteredNewLapTopDueToHandleSlidingBars = false;
                        fPausedReDrawScreenFlag = true;
                    }
                    break;
                case (LAPTOP_PROGRAM.FILES):
                    fMinizingProgram = !DisplayTitleBarMaximizeGraphic(false, fInitTitle, 29, 130, 29 + 20);
                    if (fMinizingProgram == false)
                    {
                        RemoveTitleBarMaximizeGraphics();
                        EnterNewLaptopMode();
                        fEnteredNewLapTopDueToHandleSlidingBars = false;
                        fPausedReDrawScreenFlag = true;
                    }
                    break;
                case (LAPTOP_PROGRAM.FINANCES):
                    fMinizingProgram = !DisplayTitleBarMaximizeGraphic(false, fInitTitle, 29, 226, 29 + 20);
                    if (fMinizingProgram == false)
                    {
                        RemoveTitleBarMaximizeGraphics();
                        EnterNewLaptopMode();
                        fEnteredNewLapTopDueToHandleSlidingBars = false;
                        fPausedReDrawScreenFlag = true;
                    }
                    break;
                case (LAPTOP_PROGRAM.PERSONNEL):
                    fMinizingProgram = !DisplayTitleBarMaximizeGraphic(false, fInitTitle, 29, 194, 29 + 20);
                    if (fMinizingProgram == false)
                    {
                        RemoveTitleBarMaximizeGraphics();
                        EnterNewLaptopMode();
                        fEnteredNewLapTopDueToHandleSlidingBars = false;
                        fPausedReDrawScreenFlag = true;
                    }
                    break;
                case (LAPTOP_PROGRAM.HISTORY):
                    fMinizingProgram = !DisplayTitleBarMaximizeGraphic(false, fInitTitle, 29, 162, 29 + 20);
                    if (fMinizingProgram == false)
                    {
                        RemoveTitleBarMaximizeGraphics();
                        EnterNewLaptopMode();
                        fEnteredNewLapTopDueToHandleSlidingBars = false;
                        fPausedReDrawScreenFlag = true;
                    }
                    break;
                case (LAPTOP_PROGRAM.WEB_BROWSER):
                    fMinizingProgram = !DisplayTitleBarMaximizeGraphic(false, fInitTitle, 29, 99, 29 + 20);
                    if (fMinizingProgram == false)
                    {
                        RemoveTitleBarMaximizeGraphics();
                        EnterNewLaptopMode();
                        fEnteredNewLapTopDueToHandleSlidingBars = false;
                        fPausedReDrawScreenFlag = true;
                    }
                    break;
            }
        }


        // reset init
        fInitTitle = false;
    }

    private static int ubCount;
    private static Rectangle LastRect;

    private bool DisplayTitleBarMaximizeGraphic(bool fForward, bool fInit, int usTopLeftX, int usTopLeftY, int usTopRightX)
    {
        int sPosX, sPosY, sPosRightX, sPosBottomY, sWidth, sHeight;
        Rectangle SrcRect;
        Rectangle DestRect;
        float dTemp;

        if (fInit)
        {
            if (gfTitleBarSurfaceAlreadyActive)
            {
                return (false);
            }

            gfTitleBarSurfaceAlreadyActive = true;
            if (fForward)
            {
                ubCount = 1;
            }
            else
            {
                ubCount = NUMBER_OF_LAPTOP_TITLEBAR_ITERATIONS - 1;
            }
        }

        dTemp = (LAPTOP_TITLE_BAR_TOP_LEFT_X - usTopLeftX) / (float)NUMBER_OF_LAPTOP_TITLEBAR_ITERATIONS;
        sPosX = (int)(usTopLeftX + dTemp * ubCount);

        dTemp = (LAPTOP_TITLE_BAR_TOP_RIGHT_X - usTopRightX) / (float)NUMBER_OF_LAPTOP_TITLEBAR_ITERATIONS;
        sPosRightX = (int)(usTopRightX + dTemp * ubCount);

        dTemp = (LAPTOP_TITLE_BAR_TOP_LEFT_Y - usTopLeftY) / (float)NUMBER_OF_LAPTOP_TITLEBAR_ITERATIONS;
        sPosY = (int)(usTopLeftY + dTemp * ubCount);


        sPosBottomY = LAPTOP_TITLE_BAR_HEIGHT;

        SrcRect = new()
        {
            X = 0,
            Y = 0,
            Width = LAPTOP_TITLE_BAR_WIDTH,
            Height = LAPTOP_TITLE_BAR_HEIGHT
        };

        //if its the last fram, bit the tittle bar to the final position
        if (ubCount == NUMBER_OF_LAPTOP_TITLEBAR_ITERATIONS)
        {
            DestRect = new()
            {
                X = LAPTOP_TITLE_BAR_TOP_LEFT_X,
                Y = LAPTOP_TITLE_BAR_TOP_LEFT_Y,
                Width = LAPTOP_TITLE_BAR_TOP_RIGHT_X
            };

            DestRect.Height = DestRect.Y + sPosBottomY;
        }
        else
        {
            DestRect = new()
            {
                X = sPosX,
                Y = sPosY,
                Width = sPosRightX
            };
            DestRect.Height = DestRect.X + sPosBottomY;
        }

        if (fForward)
        {
            //Restore the old rect
            if (ubCount > 1)
            {
                sWidth = (LastRect.Width - LastRect.X);
                sHeight = (LastRect.Height - LastRect.Y);
                video.BlitBufferToBuffer(SurfaceType.SAVE_BUFFER, SurfaceType.RENDER_BUFFER, new(LastRect.X, LastRect.Y, sWidth, sHeight));
            }

            //Save rectangle
            if (ubCount > 0)
            {
                sWidth = (DestRect.Width - DestRect.X);
                sHeight = (DestRect.Height - DestRect.Y);
                video.BlitBufferToBuffer(SurfaceType.RENDER_BUFFER, SurfaceType.SAVE_BUFFER, new(DestRect.X, DestRect.Y, sWidth, sHeight));
            }
        }
        else
        {
            //Restore the old rect
            if (ubCount < NUMBER_OF_LAPTOP_TITLEBAR_ITERATIONS - 1)
            {
                sWidth = (LastRect.Width - LastRect.X);
                sHeight = (LastRect.Height - LastRect.Y);
                video.BlitBufferToBuffer(SurfaceType.SAVE_BUFFER, SurfaceType.RENDER_BUFFER, new(LastRect.X, LastRect.Y, sWidth, sHeight));
            }

            //Save rectangle
            if (ubCount < NUMBER_OF_LAPTOP_TITLEBAR_ITERATIONS)
            {
                sWidth = (DestRect.Width - DestRect.X);
                sHeight = (DestRect.Height - DestRect.Y);
                video.BlitBufferToBuffer(SurfaceType.RENDER_BUFFER, SurfaceType.SAVE_BUFFER, new(DestRect.X, DestRect.Y, sWidth, sHeight));
            }
        }


        video.BltStretchVideoSurface(SurfaceType.FRAME_BUFFER, guiTitleBarSurface, new(0, 0), VO_BLT.SRCTRANSPARENCY, SrcRect, DestRect);

        video.InvalidateRegion(DestRect.X, DestRect.Y, DestRect.Width, DestRect.Height);
        video.InvalidateRegion(LastRect.X, LastRect.Y, LastRect.Width, LastRect.Height);

        LastRect = DestRect;

        if (fForward)
        {
            if (ubCount == NUMBER_OF_LAPTOP_TITLEBAR_ITERATIONS)
            {
                gfTitleBarSurfaceAlreadyActive = false;
                return (true);
            }
            else
            {
                ubCount++;
                return (false);
            }
        }
        else
        {
            if (ubCount == 0)
            {
                gfTitleBarSurfaceAlreadyActive = false;
                return (true);
            }
            else
            {
                ubCount--;
                return (false);
            }
        }

        return (true);
    }

    private void RemoveTitleBarMaximizeGraphics()
    {
        video.DeleteVideoSurfaceFromIndex(guiTitleBarSurface);
    }

    private void DisplayTaskBarIcons()
    {
        if (fNewFilesInFileViewer)
        {
            //display the files icon, if there is any
            video.BltVideoObject(SurfaceType.FRAME_BUFFER, guiTITLEBARICONS, 7, LAPTOP__NEW_FILE_ICON_X, LAPTOP__NEW_FILE_ICON_Y, VO_BLT.SRCTRANSPARENCY);
        }

        //display the email icon, if there is email
        if (fUnReadMailFlag)
        {
            //		usPosX -= 16;
            video.BltVideoObject(SurfaceType.FRAME_BUFFER, guiTITLEBARICONS, 6, LAPTOP__NEW_EMAIL_ICON_X, LAPTOP__NEW_EMAIL_ICON_Y, VO_BLT.SRCTRANSPARENCY);
        }
    }

    private void DisplayFrameRate()
    {
    }

    private void LookForUnread()
    {
        bool fStatusOfNewEmailFlag = fUnReadMailFlag;

        // simply runrs through list of messages, if any unread, set unread flag
        fUnReadMailFlag = pEmailList.Any(e => !e.fRead);

        if (fStatusOfNewEmailFlag != fUnReadMailFlag)
        {
            //Since there is no new email, get rid of the hepl text
            CreateFileAndNewEmailIconFastHelpText(
                LaptopText.LAPTOP_BN_HLP_TXT_YOU_HAVE_NEW_MAIL,
                !fUnReadMailFlag);
        }

        return;
    }

    private void ShouldNewMailBeDisplayed()
    {
        bool fReDraw = false;
        switch (guiCurrentLaptopMode)
        {
            case LAPTOP_MODE.AIM_MEMBERS:
                fReDraw = Laptop.DisableNewMailMessage();
                break;
        }
        /*
            if(fReDraw)
            {
                RenderLapTopImage();
                HighLightRegion(giCurrentRegion);
                RenderLaptop(); 
            }
         */
    }

    private static bool DisableNewMailMessage()
    {
        if (Emails.fNewMailFlag && gubVideoConferencingMode != 0)
        {
            Laptop.gfIsNewMailFlagSet = true;
            Emails.fNewMailFlag = false;
            gfRedrawScreen = true;

            return true;
        }

        return false;
    }

    private void DisplayErrorBox()
    {
        // this function will display the error graphic
        HVOBJECT hLapTopIconHandle;
        if (!fErrorFlag)
        {
            return;
        }

        // get and blt top portion
        video.BltVideoObject(SurfaceType.FRAME_BUFFER, guiBOOKTOP, 0, ERROR_X, ERROR_Y, VO_BLT.SRCTRANSPARENCY);

        // middle * 5
        video.BltVideoObject(SurfaceType.FRAME_BUFFER, guiBOOKMID, 0, ERROR_X, ERROR_Y + BOOK_HEIGHT, VO_BLT.SRCTRANSPARENCY);
        video.BltVideoObject(SurfaceType.FRAME_BUFFER, guiBOOKMID, 0, ERROR_X, ERROR_Y + 2 * BOOK_HEIGHT, VO_BLT.SRCTRANSPARENCY);
        video.BltVideoObject(SurfaceType.FRAME_BUFFER, guiBOOKMID, 0, ERROR_X, ERROR_Y + 3 * BOOK_HEIGHT, VO_BLT.SRCTRANSPARENCY);
        video.BltVideoObject(SurfaceType.FRAME_BUFFER, guiBOOKMID, 0, ERROR_X, ERROR_Y + 4 * BOOK_HEIGHT, VO_BLT.SRCTRANSPARENCY);
        video.BltVideoObject(SurfaceType.FRAME_BUFFER, guiBOOKMID, 0, ERROR_X, ERROR_Y + 5 * BOOK_HEIGHT, VO_BLT.SRCTRANSPARENCY);
        video.BltVideoObject(SurfaceType.FRAME_BUFFER, guiBOOKBOT, 0, ERROR_X, ERROR_Y + 6 * BOOK_HEIGHT, VO_BLT.SRCTRANSPARENCY);

        // font stuff
        FontSubSystem.SetFont(ERROR_TITLE_FONT);
        FontSubSystem.SetFontForeground(FontColor.FONT_WHITE);
        FontSubSystem.SetFontBackground(FontColor.FONT_BLACK);
        FontSubSystem.SetFontShadow(FontShadow.NO_SHADOW);

        // print title
        mprintf(ERROR_TITLE_X, ERROR_TITLE_Y, pErrorStrings[0]);
        FontSubSystem.SetFontForeground(FontColor.FONT_BLACK);
        FontSubSystem.SetFont(ERROR_FONT);

        // display error string
        FontSubSystem.DisplayWrappedString(
            new Point(
                ERROR_X + ERROR_TEXT_X,(ERROR_Y + ERROR_TEXT_Y + FontSubSystem.DisplayWrappedString(
                    new Point(ERROR_X + ERROR_TEXT_X, ERROR_Y + ERROR_TEXT_Y),
                    BOOK_WIDTH,
                    2,
                    ERROR_FONT,
                    FontColor.FONT_BLACK,
                    pErrorStrings[1],
                    FontColor.FONT_BLACK,
                    TextJustifies.CENTER_JUSTIFIED))),
                BOOK_WIDTH,
                2,
                ERROR_FONT,
                FontColor.FONT_BLACK,
                pErrorStrings[2],
                FontColor.FONT_BLACK,
                TextJustifies.CENTER_JUSTIFIED);

        FontSubSystem.SetFontShadow(FontShadow.DEFAULT_SHADOW);
    }

    private static bool fOldShow = false;
    private void DisplayWebBookMarkNotify()
    {
        HVOBJECT hLapTopIconHandle;


        // handle the timer for this thing
        HandleWebBookMarkNotifyTimer();

        // are we about to start showing box? or did we just stop?
        if (((fOldShow == false) || (fReDrawBookMarkInfo)) && (fShowBookmarkInfo))
        {
            fOldShow = true;
            fReDrawBookMarkInfo = false;

            // show background objects
            video.BltVideoObject(SurfaceType.FRAME_BUFFER, guiDOWNLOADTOP, 0, DOWNLOAD_X, DOWNLOAD_Y, VO_BLT.SRCTRANSPARENCY);
            video.BltVideoObject(SurfaceType.FRAME_BUFFER, guiDOWNLOADMID, 0, DOWNLOAD_X, DOWNLOAD_Y + DOWN_HEIGHT, VO_BLT.SRCTRANSPARENCY);
            video.BltVideoObject(SurfaceType.FRAME_BUFFER, guiDOWNLOADBOT, 0, DOWNLOAD_X, DOWNLOAD_Y + 2 * DOWN_HEIGHT, VO_BLT.SRCTRANSPARENCY);
            video.BltVideoObject(SurfaceType.FRAME_BUFFER, guiTITLEBARICONS, 1, DOWNLOAD_X + 4, DOWNLOAD_Y + 1, VO_BLT.SRCTRANSPARENCY);

            //	MSYS_DefineRegion( &gLapTopScreenRegion, ( UINT16 )( LaptopScreenRect.iLeft ),( UINT16 )( LaptopScreenRect.iTop ),( UINT16 ) ( LaptopScreenRect.iRight ),( UINT16 )( LaptopScreenRect.iBottom ), MSYS_PRIORITY_NORMAL+1,
            //					CURSOR_LAPTOP_SCREEN, ScreenRegionMvtCallback, LapTopScreenCallBack ); 

            // font stuff
            FontSubSystem.SetFont(DOWNLOAD_FONT);
            FontSubSystem.SetFontForeground(FontColor.FONT_WHITE);
            FontSubSystem.SetFontBackground(FontColor.FONT_BLACK);
            FontSubSystem.SetFontShadow(FontShadow.NO_SHADOW);


            // display download string
            mprintf(DOWN_STRING_X, DOWN_STRING_Y, pShowBookmarkString[0]);

            FontSubSystem.SetFont(BOOK_FONT);
            FontSubSystem.SetFontForeground(FontColor.FONT_BLACK);
            FontSubSystem.SetFontBackground(FontColor.FONT_BLACK);
            FontSubSystem.SetFontShadow(FontShadow.NO_SHADOW);

            // now draw the message
            FontSubSystem.DisplayWrappedString(
                new Point((DOWN_STRING_X - 42), (DOWN_STRING_Y + 20)),
                BOOK_WIDTH + 45,
                2,
                BOOK_FONT,
                FontColor.FONT_BLACK,
                pShowBookmarkString[1],
                FontColor.FONT_BLACK,
                TextJustifies.CENTER_JUSTIFIED);

            // invalidate region
            video.InvalidateRegion(DOWNLOAD_X, DOWNLOAD_Y, DOWNLOAD_X + 150, DOWNLOAD_Y + 100);

        }
        else if ((fOldShow) && (fShowBookmarkInfo == false))
        {
            //MSYS_RemoveRegion( &gLapTopScreenRegion );
            fOldShow = false;
            fPausedReDrawScreenFlag = true;
        }

        FontSubSystem.SetFontShadow(FontShadow.DEFAULT_SHADOW);
    }

    private static uint iBaseTimeHandleWebBookMarkNotifyTimer = 0;
    private static bool fOldShowBookMarkInfo = false;
    private void HandleWebBookMarkNotifyTimer()
    {
        uint iDifference = 0;

        // check if maxing or mining?
        if ((fMaximizingProgram) || (fMinizingProgram))
        {
            fOldShowBookMarkInfo |= fShowBookmarkInfo;
            fShowBookmarkInfo = false;
            return;
        }

        // if we were going to show this pop up, but were delayed, then do so now
        fShowBookmarkInfo |= fOldShowBookMarkInfo;

        // reset old flag
        fOldShowBookMarkInfo = false;

        // if current mode is too low, then reset
        if (guiCurrentLaptopMode < LAPTOP_MODE.WWW)
        {
            fShowBookmarkInfo = false;
        }

        // if showing bookmarks, don't show help
        if (gfShowBookmarks)
        {
            fShowBookmarkInfo = false;
        }

        // check if flag false, is so, leave
        if (fShowBookmarkInfo == false)
        {
            iBaseTimeHandleWebBookMarkNotifyTimer = 0;
            return;
        }

        // check if this is the first time in here
        if (iBaseTimeHandleWebBookMarkNotifyTimer == 0)
        {
            iBaseTimeHandleWebBookMarkNotifyTimer = GetJA2Clock();
            return;
        }

        iDifference = GetJA2Clock() - iBaseTimeHandleWebBookMarkNotifyTimer;

        fReDrawBookMarkInfo = true;

        if (iDifference > DISPLAY_TIME_FOR_WEB_BOOKMARK_NOTIFY)
        {
            // waited long enough, stop showing
            iBaseTimeHandleWebBookMarkNotifyTimer = 0;
            fShowBookmarkInfo = false;
        }
    }

    private static uint iTotalTime = 0;
    private bool DisplayLoadPending()
    {
        // this function will display the load pending and return if the load is done
        uint iTempTime = 0;
        int iCounter = 0;
        uint iDifference = 0;
        HVOBJECT hLapTopIconHandle;
        uint iLoadTime;
        uint iUnitTime;
        uint uiTempLaptopMode = 0;
        uint uiTempWWWMode = 0;
        int sXPosition = 0, sYPosition = 0;

        // if merc webpage, make it longer
        //TEMP disables the loadpending
        if (gfTemporaryDisablingOfLoadPendingFlag)
        {
            iLoadTime = 1;
            iUnitTime = 1;
        }
        else
        {
            if ((fFastLoadFlag == true) && (fConnectingToSubPage == true))
            {
                iUnitTime = FASTEST_UNIT_TIME;
            }
            else if (fFastLoadFlag == true)
            {
                iUnitTime = FAST_UNIT_TIME;
            }
            else if (fConnectingToSubPage == true)
            {
                iUnitTime = ALMOST_FAST_UNIT_TIME;
            }

            //if we are connecting the MERC site, and the MERC site hasnt yet moved to their new site, have the sloooww wait
            else if (guiCurrentLaptopMode == LAPTOP_MODE.MERC && !LaptopSaveInfo.fMercSiteHasGoneDownYet)
            {
                iUnitTime = LONG_UNIT_TIME;
            }
            else
            {
                iUnitTime = UNIT_TIME;
            }

            iUnitTime += WWaitDelayIncreasedIfRaining(iUnitTime);

            iLoadTime = iUnitTime * 30;
        }


        // we are now waiting on a web page to download, reset counter
        if (!fLoadPendingFlag)
        {
            fDoneLoadPending = false;
            fFastLoadFlag = false;
            fConnectingToSubPage = false;
            iBaseTime = 0;
            iTotalTime = 0;
            return (false);
        }
        // if total time is exceeded, return (true)
        if (iBaseTime == 0)
        {
            iBaseTime = GetJA2Clock();
        }



        if (iTotalTime >= iLoadTime)
        {
            // done loading, redraw screen
            fLoadPendingFlag = false;
            fFastLoadFlag = false;
            iTotalTime = 0;
            iBaseTime = 0;
            fDoneLoadPending = true;
            fConnectingToSubPage = false;
            fPausedReDrawScreenFlag = true;

            return (true);
        }


        iDifference = GetJA2Clock() - iBaseTime;



        // difference has been long enough or we are redrawing the screen
        if ((iDifference) > iUnitTime)
        {
            // LONG ENOUGH TIME PASSED
            iCounter = 0;
            iBaseTime = GetJA2Clock();
            iTotalTime += iDifference;
            iTempTime = iTotalTime;

        }

        // new mail, don't redraw
        if (Emails.fNewMailFlag == true)
        {
            return (false);
        }

        ButtonSubSystem.RenderButtons(buttonList);

        //	RenderFastHelp( );
        //	RenderButtonsFastHelp( );


        // display top middle and bottom of box
        video.BltVideoObject(SurfaceType.FRAME_BUFFER, guiDOWNLOADTOP, 0, DOWNLOAD_X, DOWNLOAD_Y, VO_BLT.SRCTRANSPARENCY);
        video.BltVideoObject(SurfaceType.FRAME_BUFFER, guiDOWNLOADMID, 0, DOWNLOAD_X, DOWNLOAD_Y + DOWN_HEIGHT, VO_BLT.SRCTRANSPARENCY);
        video.BltVideoObject(SurfaceType.FRAME_BUFFER, guiDOWNLOADBOT, 0, DOWNLOAD_X, DOWNLOAD_Y + 2 * DOWN_HEIGHT, VO_BLT.SRCTRANSPARENCY);
        video.BltVideoObject(SurfaceType.FRAME_BUFFER, guiTITLEBARICONS, 1, DOWNLOAD_X + 4, DOWNLOAD_Y + 1, VO_BLT.SRCTRANSPARENCY);


        // font stuff
        FontSubSystem.SetFont(DOWNLOAD_FONT);
        FontSubSystem.SetFontForeground(FontColor.FONT_WHITE);
        FontSubSystem.SetFontBackground(FontColor.FONT_BLACK);
        FontSubSystem.SetFontShadow(FontShadow.NO_SHADOW);

        // reload or download?
        if (fFastLoadFlag == true)
        {
            FontSubSystem.FindFontCenterCoordinates(328, 0, 446 - 328, 0, pDownloadString[1], DOWNLOAD_FONT, out sXPosition, out sYPosition);

            // display download string
            mprintf(sXPosition, DOWN_STRING_Y, pDownloadString[1]);
        }
        else
        {
            FontSubSystem.FindFontCenterCoordinates(328, 0, 446 - 328, 0, pDownloadString[0], DOWNLOAD_FONT, out sXPosition, out sYPosition);

            // display download string
            mprintf(sXPosition, DOWN_STRING_Y, pDownloadString[0]);
        }

        // get and blt the window video object
        video.BltVideoObject(SurfaceType.FRAME_BUFFER, guiGRAPHWINDOW, 0, LAPTOP_WINDOW_X, LAPTOP_WINDOW_Y, VO_BLT.SRCTRANSPARENCY);

        // check to see if we are only updating screen, but not passed a new element in the load pending display


        iTempTime = iTotalTime;
        // decide how many time units are to be displayed, based on amount of time passed
        while (iTempTime > 0)
        {
            video.BltVideoObject(SurfaceType.FRAME_BUFFER, guiGRAPHBAR, 0, LAPTOP_BAR_X + (UNIT_WIDTH * iCounter), LAPTOP_BAR_Y, VO_BLT.SRCTRANSPARENCY);
            iTempTime -= iUnitTime;
            iCounter++;

            // have we gone too far?
            if (iCounter > 30)
            {
                iTempTime = 0;
            }
        }

        video.InvalidateRegion(DOWNLOAD_X, DOWNLOAD_Y, DOWNLOAD_X + 150, DOWNLOAD_Y + 100);

        // re draw screen and new mail warning box
        FontSubSystem.SetFontShadow(FontShadow.DEFAULT_SHADOW);

        ButtonSubSystem.MarkButtonsDirty(buttonList);

        DisableMercSiteButton();

        return (false);
    }

    private void DisableMercSiteButton()
    {
        if (Mercs.iMercPopUpBox != -1)
        {
            Mercs.guiAccountBoxButton.uiFlags |= ButtonFlags.BUTTON_FORCE_UNDIRTY;
        }
    }

    private uint WWaitDelayIncreasedIfRaining(uint iUnitTime)
    {
        uint iRetVal = 0;

        if (guiEnvWeather.HasFlag(WEATHER_FORECAST.THUNDERSHOWERS))
        {
            iRetVal = (uint)(iUnitTime * 0.80);
        }
        else if (guiEnvWeather.HasFlag(WEATHER_FORECAST.SHOWERS))
        {
            iRetVal = (uint)(iUnitTime * 0.6);
        }

        return (iRetVal);
    }

    private void DisplayBookMarks()
    {
        // will look at bookmarklist and set accordingly
        int iCounter = 1;
        // load images
        HVOBJECT hLapTopIconHandle;
        // laptop icons
        int sX, sY;


        // check if we are maximizing or minimizing.. if so, do not display
        if ((fMaximizingProgram) || (fMinizingProgram))
        {
            return;
        }


        // font stuff
        FontSubSystem.SetFont(BOOK_FONT);
        FontSubSystem.SetFontForeground(FontColor.FONT_WHITE);
        FontSubSystem.SetFontBackground(FontColor.FONT_BLACK);
        FontSubSystem.SetFontShadow(FontShadow.NO_SHADOW);

        // dirty and print to screen
        //	gprintfdirty( sX, sY, pBookmarkTitle[0]);
        //	mprintf(sX, sY,pBookmarkTitle[0] );

        // set buffer
        FontSubSystem.SetFontDestBuffer(SurfaceType.FRAME_BUFFER, BOOK_X, BOOK_TOP_Y, BOOK_X + BOOK_WIDTH - 10, 480, false);


        // blt in book mark background
        while (LaptopSaveInfo.iBookMarkList[iCounter - 1] != BOOKMARK.UNSET)
        {

            if (iHighLightBookLine == iCounter - 1)
            {
                video.BltVideoObject(SurfaceType.FRAME_BUFFER, guiBOOKHIGH, 0, BOOK_X, BOOK_TOP_Y + (iCounter * (BOOK_HEIGHT + 6)) + 6, VO_BLT.SRCTRANSPARENCY);
            }
            else
            {
                video.BltVideoObject(SurfaceType.FRAME_BUFFER, guiBOOKMARK, 0, BOOK_X, BOOK_TOP_Y + (iCounter * (BOOK_HEIGHT + 6)) + 6, VO_BLT.SRCTRANSPARENCY);
            }

            if (iHighLightBookLine == iCounter - 1)
            {
                // blit in text
                FontSubSystem.SetFontForeground(FontColor.FONT_WHITE);
                FontSubSystem.SetFontBackground(FontColor.FONT_BLACK);
            }
            else
            {
                // blit in text
                FontSubSystem.SetFontForeground(FontColor.FONT_BLACK);
                FontSubSystem.SetFontBackground(FontColor.FONT_BLACK);
            }

            FontSubSystem.FindFontCenterCoordinates(BOOK_X + 3, (BOOK_TOP_Y + 2 + (iCounter * (BOOK_HEIGHT + 6)) + 6), BOOK_WIDTH - 3, BOOK_HEIGHT + 6, pBookMarkStrings[(int)LaptopSaveInfo.iBookMarkList[iCounter - 1]], BOOK_FONT, out sX, out sY);

            mprintf(sX, sY, pBookMarkStrings[(int)LaptopSaveInfo.iBookMarkList[iCounter - 1]]);
            iCounter++;
        }

        // blit one more


        if (iHighLightBookLine == iCounter - 1)
        {
            video.BltVideoObject(SurfaceType.FRAME_BUFFER, guiBOOKHIGH, 0, BOOK_X, BOOK_TOP_Y + (iCounter * (BOOK_HEIGHT + 6)) + 6, VO_BLT.SRCTRANSPARENCY);
        }
        else
        {
            video.BltVideoObject(SurfaceType.FRAME_BUFFER, guiBOOKMARK, 0, BOOK_X, BOOK_TOP_Y + (iCounter * (BOOK_HEIGHT + 6)) + 6, VO_BLT.SRCTRANSPARENCY);
        }

        if (iHighLightBookLine == iCounter - 1)
        {
            // blit in text
            FontSubSystem.SetFontForeground(FontColor.FONT_WHITE);
            FontSubSystem.SetFontBackground(FontColor.FONT_BLACK);
        }
        else
        {
            // blit in text
            FontSubSystem.SetFontForeground(FontColor.FONT_BLACK);
            FontSubSystem.SetFontBackground(FontColor.FONT_BLACK);
        }

        FontSubSystem.FindFontCenterCoordinates(BOOK_X + 3, (BOOK_TOP_Y + 2 + (iCounter * (BOOK_HEIGHT + 6)) + 6), BOOK_WIDTH - 3, BOOK_HEIGHT + 6, pBookMarkStrings[(int)BOOKMARK.CANCEL_STRING], BOOK_FONT, out sX, out sY);
        mprintf(sX, sY, pBookMarkStrings[(int)BOOKMARK.CANCEL_STRING]);
        iCounter++;

        FontSubSystem.SetFontDestBuffer(SurfaceType.FRAME_BUFFER, 0, 0, 640, 480, false);

        //GetVideoObject(&hLapTopIconHandle, guiBOOKBOT);
        //BltVideoObject(FRAME_BUFFER, hLapTopIconHandle, 0,BOOK_X, 6+BOOK_TOP_Y+(iCounter)*BOOK_HEIGHT, VO_BLT.SRCTRANSPARENCY,null);

        /*if(fNewWWWDisplay)
       ScrollDisplayText(BOOK_TOP_Y+2+((iCounter)*BOOK_HEIGHT)+6);
        else
     */
        video.InvalidateRegion(BOOK_X, BOOK_TOP_Y + ((iCounter) * BOOK_HEIGHT) + 12, BOOK_X + BOOK_WIDTH, BOOK_TOP_Y + ((iCounter + 1) * BOOK_HEIGHT) + 16);
        FontSubSystem.SetFontShadow(FontShadow.DEFAULT_SHADOW);


        video.InvalidateRegion(BOOK_X, BOOK_TOP_Y, BOOK_X + BOOK_WIDTH, BOOK_TOP_Y + (iCounter + 6) * BOOK_HEIGHT + 16);
        return;
    }

    private void HandleWWWSubSites()
    {
        // check to see if WW Wait is needed for a sub site within the Web Browser

        if ((guiCurrentLaptopMode == guiPreviousLaptopMode)
            || (guiCurrentLaptopMode < LAPTOP_MODE.WWW)
            || (fLoadPendingFlag)
            || (fDoneLoadPending)
            || (guiPreviousLaptopMode < LAPTOP_MODE.WWW))
        {
            // no go, leave
            return;
        }

        fLoadPendingFlag = true;
        fConnectingToSubPage = true;

        // fast or slow load?
        if (gfWWWaitSubSitesVisitedFlags[guiCurrentLaptopMode - (LAPTOP_MODE.WWW + 1)] == true)
        {
            fFastLoadFlag = true;
        }

        // set fact we were here
        gfWWWaitSubSitesVisitedFlags[guiCurrentLaptopMode - (LAPTOP_MODE.WWW + 1)] = true;

        //Dont show the dlownload screen when switching between these pages
        if ((guiCurrentLaptopMode == LAPTOP_MODE.AIM_MEMBERS)
            && (guiPreviousLaptopMode == LAPTOP_MODE.AIM_MEMBERS_FACIAL_INDEX)
            || (guiCurrentLaptopMode == LAPTOP_MODE.AIM_MEMBERS_FACIAL_INDEX)
            && (guiPreviousLaptopMode == LAPTOP_MODE.AIM_MEMBERS))
        {
            fFastLoadFlag = false;
            fLoadPendingFlag = false;

            // set fact we were here
            gfWWWaitSubSitesVisitedFlags[LAPTOP_MODE.AIM_MEMBERS_FACIAL_INDEX - (LAPTOP_MODE.WWW + 1)] = true;
            gfWWWaitSubSitesVisitedFlags[LAPTOP_MODE.AIM_MEMBERS - (LAPTOP_MODE.WWW + 1)] = true;
        }
    }

    private void UpdateStatusOfDisplayingBookMarks()
    {
        // this function will disable showing of bookmarks if in process of download or if we miniming web browser
        if ((fLoadPendingFlag)
            || (guiCurrentLaptopMode < LAPTOP_MODE.WWW))
        {
            gfShowBookmarks = false;
        }
    }

    private void HandleLapTopHandles()
    {
        if (fLoadPendingFlag)
            return;

        if ((fMaximizingProgram)
            || (fMinizingProgram))
        {
            return;
        }


        switch (guiCurrentLaptopMode)
        {
            case LAPTOP_MODE.NONE:
                break;

            case LAPTOP_MODE.AIM:

                HandleAIM();
                break;
            case LAPTOP_MODE.AIM_MEMBERS:
                HandleAIMMembers();
                break;
            case LAPTOP_MODE.AIM_MEMBERS_FACIAL_INDEX:
                HandleAimFacialIndex();
                break;
            case LAPTOP_MODE.AIM_MEMBERS_SORTED_FILES:
                HandleAimSort();
                break;
            case LAPTOP_MODE.AIM_MEMBERS_ARCHIVES:
                HandleAimArchives();
                break;
            case LAPTOP_MODE.AIM_POLICIES:
                HandleAimPolicies();
                break;
            case LAPTOP_MODE.AIM_LINKS:
                HandleAimLinks();
                break;
            case LAPTOP_MODE.AIM_HISTORY:
                HandleAimHistory();
                break;

            case LAPTOP_MODE.MERC:
                Mercs.HandleMercs();
                break;
            case LAPTOP_MODE.MERC_FILES:
                Mercs.HandleMercsFiles();
                break;
            case LAPTOP_MODE.MERC_ACCOUNT:
                Mercs.HandleMercsAccount();
                break;
            case LAPTOP_MODE.MERC_NO_ACCOUNT:
                Mercs.HandleMercsNoAccount();
                break;


            case LAPTOP_MODE.BOBBY_R:
                BobbyR.HandleBobbyR();
                break;
            case LAPTOP_MODE.BOBBY_R_GUNS:
                BobbyR.HandleBobbyRGuns();
                break;
            case LAPTOP_MODE.BOBBY_R_AMMO:
                BobbyR.HandleBobbyRAmmo();
                break;
            case LAPTOP_MODE.BOBBY_R_ARMOR:
                BobbyR.HandleBobbyRArmour();
                break;
            case LAPTOP_MODE.BOBBY_R_MISC:
                BobbyR.HandleBobbyRMisc();
                break;
            case LAPTOP_MODE.BOBBY_R_USED:
                BobbyR.HandleBobbyRUsed();
                break;
            case LAPTOP_MODE.BOBBY_R_MAILORDER:
                BobbyR.HandleBobbyRMailOrder();
                break;


            case LAPTOP_MODE.CHAR_PROFILE:
                HandleCharProfile();
                break;
            case LAPTOP_MODE.FLORIST:
                Florist.HandleFlorist();
                break;
            case LAPTOP_MODE.FLORIST_FLOWER_GALLERY:
                Florist.HandleFloristGallery();
                break;
            case LAPTOP_MODE.FLORIST_ORDERFORM:
                Florist.HandleFloristOrderForm();
                break;
            case LAPTOP_MODE.FLORIST_CARD_GALLERY:
                Florist.HandleFloristCards();
                break;

            case LAPTOP_MODE.INSURANCE:
                Insurance.HandleInsurance();
                break;

            case LAPTOP_MODE.INSURANCE_INFO:
                Insurance.HandleInsuranceInfo();
                break;

            case LAPTOP_MODE.INSURANCE_CONTRACT:
                Insurance.HandleInsuranceContract();
                break;
            case LAPTOP_MODE.INSURANCE_COMMENTS:
                Insurance.HandleInsuranceComments();
                break;

            case LAPTOP_MODE.FUNERAL:
                HandleFuneral();
                break;
            case LAPTOP_MODE.SIRTECH:
                HandleSirTech();
                break;
            case LAPTOP_MODE.FINANCES:
                Finances.HandleFinances();
                break;
            case LAPTOP_MODE.PERSONNEL:
                HandlePersonnel();
                break;
            case LAPTOP_MODE.HISTORY:
                HandleHistory();
                break;
            case LAPTOP_MODE.FILES:
                HandleFiles();
                break;
            case LAPTOP_MODE.EMAIL:
                Emails.HandleEmail();
                break;

            case LAPTOP_MODE.BROKEN_LINK:
                HandleBrokenLink();
                break;

            case LAPTOP_MODE.BOBBYR_SHIPMENTS:
                BobbyR.HandleBobbyRShipments();
                break;
        }
    }

    private void HandleBrokenLink()
    {
        throw new NotImplementedException();
    }

    private void HandleFiles()
    {
        throw new NotImplementedException();
    }

    private void HandleHistory()
    {
        throw new NotImplementedException();
    }

    private void HandlePersonnel()
    {
        throw new NotImplementedException();
    }

    private void HandleSirTech()
    {
        throw new NotImplementedException();
    }

    private void HandleFuneral()
    {
        throw new NotImplementedException();
    }

    private void HandleCharProfile()
    {
        throw new NotImplementedException();
    }

    private void HandleAimHistory()
    {
        throw new NotImplementedException();
    }

    private void HandleAimLinks()
    {
        throw new NotImplementedException();
    }

    private void HandleAimPolicies()
    {
        throw new NotImplementedException();
    }

    private void HandleAimArchives()
    {
        throw new NotImplementedException();
    }

    private void HandleAimSort()
    {
        throw new NotImplementedException();
    }

    private void HandleAimFacialIndex()
    {
        throw new NotImplementedException();
    }

    private void HandleAIMMembers()
    {
        throw new NotImplementedException();
    }

    private void HandleAIM()
    {
    }

    private void GetLaptopKeyboardInput()
    {
    }

    private static bool fOldErrorFlag = false;
    private void CreateDestroyErrorButton()
    {
        if ((fErrorFlag) && (!fOldErrorFlag))
        {
            // create inventory button
            fOldErrorFlag = true;

            // load image and create error confirm button
            giErrorButtonImage[0] = ButtonSubSystem.LoadButtonImage("LAPTOP\\errorbutton.sti", -1, 0, -1, 1, -1);
            giErrorButton[0] = ButtonSubSystem.QuickCreateButton(
                giErrorButtonImage[0],
                new Point(ERROR_X + ERROR_BTN_X, ERROR_BTN_Y),
                ButtonFlags.BUTTON_TOGGLE,
                MSYS_PRIORITY.HIGHEST,
                BtnGenericMouseMoveButtonCallback,
                BtnErrorCallback);

            // define the cursor
            ButtonSubSystem.SetButtonCursor(giErrorButton[0], CURSOR.LAPTOP_SCREEN);

            // define the screen mask
            MouseSubSystem.MSYS_DefineRegion(
                pScreenMask,
                new Rectangle(0, 0, 640, 480),
                MSYS_PRIORITY.HIGHEST - 3,
                CURSOR.LAPTOP_SCREEN,
                MSYS_NO_CALLBACK,
                LapTopScreenCallBack);

            // add region 
            MouseSubSystem.MSYS_AddRegion(ref pScreenMask);

            buttonList.AddRange(giErrorButton);
        }
        else if ((!fErrorFlag) && (fOldErrorFlag))
        {
            // done dsiplaying, get rid of button and screen mask
            fOldErrorFlag = false;

            ButtonSubSystem.RemoveButton(giErrorButton[0]);
            ButtonSubSystem.UnloadButtonImage(giErrorButtonImage[0]);

            MouseSubSystem.MSYS_RemoveRegion(pScreenMask);

            // re draw screen
            fReDrawScreenFlag = true;
        }
    }

    private void BtnErrorCallback(ref GUI_BUTTON button, MSYS_CALLBACK_REASON reason)
    {
        throw new NotImplementedException();
    }

    private void BtnGenericMouseMoveButtonCallback(ref GUI_BUTTON button, MSYS_CALLBACK_REASON reason)
    {
        throw new NotImplementedException();
    }

    private static bool fOldShowBookmarks = false;
    private void CreateDestoryBookMarkRegions()
    {
        // checks to see if a bookmark needs to be created or destroyed

        if ((gfShowBookmarks) && (!fOldShowBookmarks))
        {
            // create regions
            CreateBookMarkMouseRegions();
            fOldShowBookmarks = true;
        }
        else if ((!gfShowBookmarks) && (fOldShowBookmarks))
        {
            // destroy bookmarks
            DeleteBookmarkRegions();
            fOldShowBookmarks = false;
        }
    }

    void CreateBookMarkMouseRegions()
    {
        int iCounter = 0;
        // creates regions based on number of entries
        while (LaptopSaveInfo.iBookMarkList[iCounter] != (BOOKMARK)(-1))
        {
            MouseSubSystem.MSYS_DefineRegion(
                gBookmarkMouseRegions[iCounter],
                new Rectangle(BOOK_X,
                    (BOOK_TOP_Y + ((iCounter + 1) * (BOOK_HEIGHT + 6)) + 6),
                    BOOK_X + BOOK_WIDTH,
                    (BOOK_TOP_Y + ((iCounter + 2) * (BOOK_HEIGHT + 6)) + 6)),
                MSYS_PRIORITY.HIGHEST - 2,
                CURSOR.LAPTOP_SCREEN,
                BookmarkMvtCallBack,
                BookmarkCallBack);

            //MSYS_AddRegion(&gBookmarkMouseRegions[iCounter]);
            MouseSubSystem.MSYS_SetRegionUserData(gBookmarkMouseRegions[iCounter], 0, iCounter);
            MouseSubSystem.MSYS_SetRegionUserData(gBookmarkMouseRegions[iCounter], 1, 0);

            //Create the regions help text
            CreateBookMarkHelpText(gBookmarkMouseRegions[iCounter], LaptopSaveInfo.iBookMarkList[iCounter]);

            iCounter++;
        }
        // now add one more
        // for the cancel button
        MouseSubSystem.MSYS_DefineRegion(
            gBookmarkMouseRegions[iCounter],
            new Rectangle(BOOK_X,
                (BOOK_TOP_Y + ((iCounter + 1) * (BOOK_HEIGHT + 6)) + 6),
                BOOK_X + BOOK_WIDTH,
                (BOOK_TOP_Y + ((iCounter + 2) * (BOOK_HEIGHT + 6)) + 6)),
            MSYS_PRIORITY.HIGHEST - 2,
            CURSOR.LAPTOP_SCREEN,
            BookmarkMvtCallBack,
            BookmarkCallBack);

        //MSYS_AddRegion(&gBookmarkMouseRegions[iCounter]);
        MouseSubSystem.MSYS_SetRegionUserData(gBookmarkMouseRegions[iCounter], 0, iCounter);
        MouseSubSystem.MSYS_SetRegionUserData(gBookmarkMouseRegions[iCounter], 1, BOOKMARK.CANCEL_STRING);
    }

    private void BookmarkMvtCallBack(ref MOUSE_REGION pRegion, MSYS_CALLBACK_REASON iReason)
    {
        if (iReason == MSYS_CALLBACK_REASON.MOVE)
        {
            iHighLightBookLine = (int)MouseSubSystem.MSYS_GetRegionUserData(ref pRegion, 0);
        }
        if (iReason == MSYS_CALLBACK_REASON.LOST_MOUSE)
        {
            iHighLightBookLine = -1;
        }
    }

    private void BookmarkCallBack(ref MOUSE_REGION pRegion, MSYS_CALLBACK_REASON iReason)
    {
        int iCount;

        if (iReason.HasFlag(MSYS_CALLBACK_REASON.INIT))
        {
            return;
        }

        // we are in process of loading
        if (fLoadPendingFlag == true)
        {
            return;
        }

        if (iReason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP))
        {
            iCount = (int)MouseSubSystem.MSYS_GetRegionUserData(ref pRegion, 0);
            if ((BOOKMARK)MouseSubSystem.MSYS_GetRegionUserData(ref pRegion, 1) == BOOKMARK.CANCEL_STRING)
            {
                gfShowBookmarks = false;
                fReDrawScreenFlag = true;
            }
            if (LaptopSaveInfo.iBookMarkList[iCount] != (BOOKMARK)(-1))
            {
                GoToWebPage(LaptopSaveInfo.iBookMarkList[iCount]);
            }
            else
            {
                return;
            }
        }
        else if (iReason.HasFlag(MSYS_CALLBACK_REASON.RBUTTON_UP))
        {
            iCount = (int)MouseSubSystem.MSYS_GetRegionUserData(ref pRegion, 0);

        }
        return;
    }

    private void GoToWebPage(BOOKMARK iPageId)
    {
        //if it is raining, popup a warning first saying connection time may be slow
        if (IsItRaining())
        {
            if (giRainDelayInternetSite == BOOKMARK.UNSET)
            {
                DoLapTopMessageBox(
                    MessageBoxStyle.MSG_BOX_LAPTOP_DEFAULT,
                    pErrorStrings[4],
                    ScreenName.LAPTOP_SCREEN,
                    MSG_BOX_FLAG.OK,
                    InternetRainDelayMessageBoxCallBack);

                giRainDelayInternetSite = iPageId;
                return;
            }
        }
        else
        {
            giRainDelayInternetSite = BOOKMARK.UNSET;
        }

        switch (iPageId)
        {
            case BOOKMARK.AIM_BOOKMARK:
                guiCurrentWWWMode = LAPTOP_MODE.AIM;
                guiCurrentLaptopMode = LAPTOP_MODE.AIM;

                // do we have to have a World Wide Wait
                if (LaptopSaveInfo.fVisitedBookmarkAlready[BOOKMARK.AIM_BOOKMARK] == false)
                {
                    // reset flag and set load pending flag
                    LaptopSaveInfo.fVisitedBookmarkAlready[BOOKMARK.AIM_BOOKMARK] = true;
                    fLoadPendingFlag = true;
                }
                else
                {
                    // fast reload
                    fLoadPendingFlag = true;
                    fFastLoadFlag = true;
                }
                break;
            case BOOKMARK.BOBBYR_BOOKMARK:
                guiCurrentWWWMode = LAPTOP_MODE.BOBBY_R;
                guiCurrentLaptopMode = LAPTOP_MODE.BOBBY_R;

                // do we have to have a World Wide Wait
                if (LaptopSaveInfo.fVisitedBookmarkAlready[BOOKMARK.BOBBYR_BOOKMARK] == false)
                {
                    // reset flag and set load pending flag
                    LaptopSaveInfo.fVisitedBookmarkAlready[BOOKMARK.BOBBYR_BOOKMARK] = true;
                    fLoadPendingFlag = true;
                }
                else
                {
                    // fast reload
                    fLoadPendingFlag = true;
                    fFastLoadFlag = true;
                }
                break;
            case (BOOKMARK.IMP_BOOKMARK):
                guiCurrentWWWMode = LAPTOP_MODE.CHAR_PROFILE;
                guiCurrentLaptopMode = LAPTOP_MODE.CHAR_PROFILE;

                // do we have to have a World Wide Wait
                if (LaptopSaveInfo.fVisitedBookmarkAlready[BOOKMARK.IMP_BOOKMARK] == false)
                {
                    // reset flag and set load pending flag
                    LaptopSaveInfo.fVisitedBookmarkAlready[BOOKMARK.IMP_BOOKMARK] = true;
                    fLoadPendingFlag = true;
                }
                else
                {
                    // fast reload
                    fLoadPendingFlag = true;
                    fFastLoadFlag = true;
                }
                IMP.iCurrentImpPage = IMP_PAGE.IMP_HOME_PAGE;
                break;
            case (BOOKMARK.MERC_BOOKMARK):

                //if the mercs server has gone down, but hasnt come up yet
                if (LaptopSaveInfo.fMercSiteHasGoneDownYet == true && LaptopSaveInfo.fFirstVisitSinceServerWentDown == 0)
                {
                    guiCurrentWWWMode = LAPTOP_MODE.BROKEN_LINK;
                    guiCurrentLaptopMode = LAPTOP_MODE.BROKEN_LINK;
                }
                else
                {
                    guiCurrentWWWMode = LAPTOP_MODE.MERC;
                    guiCurrentLaptopMode = LAPTOP_MODE.MERC;
                }


                // do we have to have a World Wide Wait
                if (LaptopSaveInfo.fVisitedBookmarkAlready[BOOKMARK.MERC_BOOKMARK] == false)
                {
                    // reset flag and set load pending flag
                    LaptopSaveInfo.fVisitedBookmarkAlready[BOOKMARK.MERC_BOOKMARK] = true;
                    fLoadPendingFlag = true;
                }
                else
                {
                    // fast reload
                    fLoadPendingFlag = true;
                    fFastLoadFlag = true;
                }
                break;
            case (BOOKMARK.FUNERAL_BOOKMARK):
                guiCurrentWWWMode = LAPTOP_MODE.FUNERAL;
                guiCurrentLaptopMode = LAPTOP_MODE.FUNERAL;

                // do we have to have a World Wide Wait
                if (LaptopSaveInfo.fVisitedBookmarkAlready[BOOKMARK.FUNERAL_BOOKMARK] == false)
                {
                    // reset flag and set load pending flag
                    LaptopSaveInfo.fVisitedBookmarkAlready[BOOKMARK.FUNERAL_BOOKMARK] = true;
                    fLoadPendingFlag = true;
                }
                else
                {
                    // fast reload
                    fLoadPendingFlag = true;
                    fFastLoadFlag = true;
                }
                break;
            case (BOOKMARK.FLORIST_BOOKMARK):
                guiCurrentWWWMode = LAPTOP_MODE.FLORIST;
                guiCurrentLaptopMode = LAPTOP_MODE.FLORIST;

                // do we have to have a World Wide Wait
                if (LaptopSaveInfo.fVisitedBookmarkAlready[BOOKMARK.FLORIST_BOOKMARK] == false)
                {
                    // reset flag and set load pending flag
                    LaptopSaveInfo.fVisitedBookmarkAlready[BOOKMARK.FLORIST_BOOKMARK] = true;
                    fLoadPendingFlag = true;
                }
                else
                {
                    // fast reload
                    fLoadPendingFlag = true;
                    fFastLoadFlag = true;
                }
                break;

            case (BOOKMARK.INSURANCE_BOOKMARK):
                guiCurrentWWWMode = LAPTOP_MODE.INSURANCE;
                guiCurrentLaptopMode = LAPTOP_MODE.INSURANCE;

                // do we have to have a World Wide Wait
                if (LaptopSaveInfo.fVisitedBookmarkAlready[BOOKMARK.INSURANCE_BOOKMARK] == false)
                {
                    // reset flag and set load pending flag
                    LaptopSaveInfo.fVisitedBookmarkAlready[BOOKMARK.INSURANCE_BOOKMARK] = true;
                    fLoadPendingFlag = true;
                }
                else
                {
                    // fast reload
                    fLoadPendingFlag = true;
                    fFastLoadFlag = true;
                }
                break;

        }

        gfShowBookmarks = false;
        fReDrawScreenFlag = true;
    }

    void InternetRainDelayMessageBoxCallBack(MessageBoxReturnCode bExitValue)
    {
        GoToWebPage(giRainDelayInternetSite);

        //Set to -2 so we dont due the message for this occurence of laptop
        giRainDelayInternetSite = (BOOKMARK)(-2);
    }


    private void DoLapTopMessageBox(MessageBoxStyle mSG_BOX_LAPTOP_DEFAULT, string v, ScreenName lAPTOP_SCREEN, MSG_BOX_FLAG oK, MSGBOX_CALLBACK internetRainDelayMessageBoxCallBack)
    {
        throw new NotImplementedException();
    }

    private static bool IsItRaining()
    {
        if (guiEnvWeather.HasFlag(WEATHER_FORECAST.SHOWERS) || guiEnvWeather.HasFlag(WEATHER_FORECAST.THUNDERSHOWERS))
        {
            return (true);
        }
        else
        {
            return (false);
        }
    }

    private void CreateBookMarkHelpText(MOUSE_REGION pRegion, BOOKMARK uiBookMarkID)
    {
        MouseSubSystem.SetRegionFastHelpText(pRegion, gzLaptopHelpText[LaptopText.BOOKMARK_TEXT_ASSOCIATION_OF_INTERNATION_MERCENARIES + (int)uiBookMarkID]);
    }

    private static void DeleteBookmarkRegions()
    {
        int iCounter = 0;
        //deletes bookmark regions
        while (LaptopSaveInfo.iBookMarkList[iCounter] != (BOOKMARK)(-1))
        {
            MouseSubSystem.MSYS_RemoveRegion(gBookmarkMouseRegions[iCounter]);
            iCounter++;
        }

        // now one for the cancel
        MouseSubSystem.MSYS_RemoveRegion(gBookmarkMouseRegions[iCounter]);
    }

    private void PostButtonRendering()
    {
        switch (guiCurrentLaptopMode)
        {
            case LAPTOP_MODE.AIM:
                //	    RenderCharProfilePostButton( );
                break;

            case LAPTOP_MODE.AIM_MEMBERS:
                RenderAIMMembersTopLevel();
                break;

        }
    }

    private void CheckMarkButtonsDirtyFlag()
    {
        // this function checks the fMarkButtonsDirtyFlag, if true, mark buttons dirty
        if (fMarkButtonsDirtyFlag)
        {
            // flag set, mark buttons and reset
            ButtonSubSystem.MarkButtonsDirty(buttonList);
            fMarkButtonsDirtyFlag = false;
        }
    }

    private void CheckIfNewWWWW()
    {
        // if no www mode, set new www flag..until new www mode that is not 0

        if (guiCurrentWWWMode == LAPTOP_MODE.NONE)
        {
            fNewWWW = true;
        }
        else
        {
            fNewWWW = false;
        }
    }

    private void DrawButtonText()
    {
        if (fErrorFlag)
        {
            DrawTextOnErrorButton();
        }

        switch (guiCurrentLaptopMode)
        {
            case LAPTOP_MODE.EMAIL:
                Emails.DisplayEmailHeaders();
                break;
        }
        return;

    }

    private void DrawTextOnErrorButton()
    {
        // draws text on error button
        FontSubSystem.SetFont(ERROR_TITLE_FONT);
        FontSubSystem.SetFontForeground(FontColor.FONT_BLACK);
        FontSubSystem.SetFontBackground(FontColor.FONT_BLACK);
        FontSubSystem.SetFontShadow(FontShadow.NO_SHADOW);
        mprintf(ERROR_X + ERROR_BTN_X + ERROR_BTN_TEXT_X, ERROR_BTN_Y + ERROR_BTN_TEXT_Y, pErrorStrings[3]);
        FontSubSystem.SetFontShadow(FontShadow.DEFAULT_SHADOW);

        video.InvalidateRegion(ERROR_X, ERROR_Y, ERROR_X + BOOK_WIDTH, ERROR_Y + 6 * BOOK_HEIGHT);
        return;
    }

    private static uint iBaseTime = 0;
    private static uint iTotalDifference = 0;
    private void FlickerHDLight()
    {
        uint iDifference = 0;

        if (fLoadPendingFlag == true)
        {
            fFlickerHD = true;
        }

        if (fFlickerHD == false)
        {
            return;
        }


        if (iBaseTime == 0)
        {
            iBaseTime = GetJA2Clock();
        }

        iDifference = GetJA2Clock() - iBaseTime;

        if ((iTotalDifference > HD_FLICKER_TIME) && (fLoadPendingFlag == false))
        {
            iBaseTime = GetJA2Clock();
            fHardDriveLightOn = false;
            iBaseTime = 0;
            iTotalDifference = 0;
            fFlickerHD = false;
            video.InvalidateRegion(88, 466, 102, 477);
            return;
        }

        if (iDifference > FLICKER_TIME)
        {
            iTotalDifference += iDifference;

            if (fLoadPendingFlag)
            {
                iTotalDifference = 0;
            }

            if ((Globals.Random.GetRandom(2)) == 0)
            {
                fHardDriveLightOn = true;
            }
            else
            {
                fHardDriveLightOn = false;
            }

            video.InvalidateRegion(88, 466, 102, 477);
        }

        return;
    }

    public static void PrintBalance()
    {
        string pString;
        //	UINT16 usX, usY;

        FontSubSystem.SetFont(FontStyle.FONT10ARIAL);
        FontSubSystem.SetFontForeground(FontColor.FONT_BLACK);
        FontSubSystem.SetFontBackground(FontColor.FONT_BLACK);
        FontSubSystem.SetFontShadow(FontShadow.NO_SHADOW);

        pString = Finances.InsertCommasForDollarFigure(LaptopSaveInfo.iCurrentBalance);
        Finances.InsertDollarSignInToString(pString);

        if (gLaptopButtons[5].uiFlags.HasFlag(ButtonFlags.BUTTON_CLICKED_ON))
        {
            //		gprintfdirty(47 +1, 257 +15 + 1,pString);
            mprintf(47 + 1, 257 + 15 + 1, pString);
        }
        else
        {
            //		gprintfdirty(47, 257 +15 ,pString);
            mprintf(47, 257 + 15, pString);
        }

        FontSubSystem.SetFontShadow(FontShadow.DEFAULT_SHADOW);
    }

    public static void PrintDate()
    {
        FontSubSystem.SetFont(FontStyle.FONT10ARIAL);
        FontSubSystem.SetFontForeground(FontColor.FONT_BLACK);
        FontSubSystem.SetFontBackground(FontColor.FONT_BLACK);

        FontSubSystem.SetFontShadow(FontShadow.NO_SHADOW);

        FontSubSystem.DrawTextToScreen(
            gswzWorldTimeStr,
            new(30 + (70 - FontSubSystem.StringPixLength(gswzWorldTimeStr, FontStyle.FONT10ARIAL)) / 2, 433));

        FontSubSystem.SetFontShadow(FontShadow.DEFAULT_SHADOW);

        //	RenderClock( 35, 414 );

        /*
            def: removed 3/8/99.
         Now use the render clock function used every where else

            CHAR16 pString[ 32 ];
        //	UINT16 usX, usY;

            SetFont( FONT10ARIAL );
            SetFontForeground( FONT_BLACK );
            SetFontBackground( FONT_BLACK );

            SetFontShadow( NO_SHADOW );

            wprintf(pString, L"%s %d", pMessageStrings[ MSG_DAY ], GetWorldDay( ) ); 

        //	gprintfdirty(35, 413 + 19,pString);
            mprintf(35, 413 + 19,pString);

            SetFontShadow( DEFAULT_SHADOW );
        */
    }

    private void ShowLights()
    {
        if (fPowerLightOn)
        {
            video.BltVideoObject(SurfaceType.FRAME_BUFFER, guiLIGHTS, 0, 44, 466, VO_BLT.SRCTRANSPARENCY);
        }
        else
        {
            video.BltVideoObject(SurfaceType.FRAME_BUFFER, guiLIGHTS, 1, 44, 466, VO_BLT.SRCTRANSPARENCY);
        }

        if (fHardDriveLightOn)
        {
            video.BltVideoObject(SurfaceType.FRAME_BUFFER, guiLIGHTS, 0, 88, 466, VO_BLT.SRCTRANSPARENCY);
        }
        else
        {
            video.BltVideoObject(SurfaceType.FRAME_BUFFER, guiLIGHTS, 1, 88, 466, VO_BLT.SRCTRANSPARENCY);
        }
    }

    private bool LeaveLapTopScreen()
    {
        if (ExitLaptopDone())
        {

            // exit screen is set
            // set new screen
            //if( ( LaptopSaveInfo.gfNewGameLaptop != true ) || !( AnyMercsHired() ) )
            //	{
            SetLaptopExitScreen(ScreenName.MAP_SCREEN);
            //}
            //if( ( LaptopSaveInfo.gfNewGameLaptop )&&( AnyMercsHired() ) )
            //{
            //	SetLaptopExitScreen( GAME_SCREEN );
            //	}

            if (gfAtLeastOneMercWasHired == true)
            {
                if (LaptopSaveInfo.gfNewGameLaptop)
                {
                    LaptopSaveInfo.gfNewGameLaptop = false;
                    fExitingLaptopFlag = true;
                    /*guiExitScreen = GAME_SCREEN; */
                    GameInit.InitNewGame(false);
                    gfDontStartTransitionFromLaptop = true;
                    /*InitHelicopterEntranceByMercs( );
                    fFirstTimeInGameScreen = true;*/
                    return (true);
                }
            }
            else
            {
                gfDontStartTransitionFromLaptop = true;
            }

            screens.SetPendingNewScreen(guiExitScreen);

            if (!gfDontStartTransitionFromLaptop)
            {
                Rectangle SrcRect1, SrcRect2, DstRect;
                int iPercentage, iScalePercentage, iFactor;
                uint uiStartTime, uiTimeRange, uiCurrTime;
                int iX, iY, iWidth, iHeight;
                int iRealPercentage;

                gfDontStartTransitionFromLaptop = true;
                CursorSubSystem.SetCurrentCursorFromDatabase(CURSOR.VIDEO_NO_CURSOR);
                //Step 1:  Build the laptop image into the save buffer.
                video.RestoreBackgroundRects();
                RenderLapTopImage();
                HighLightRegion(giCurrentRegion);
                RenderLaptop();
                ButtonSubSystem.RenderButtons(null);
                PrintDate();
                PrintBalance();
                PrintNumberOnTeam();
                ShowLights();

                //Step 2:  The mapscreen image is in the EXTRABUFFER, and laptop is in the SAVEBUFFER
                //         Start transitioning the screen.
                DstRect = new()
                {
                    X = 0,
                    Y = 0,
                    Width = 640,
                    Height = 480
                };

                uiTimeRange = 1000;
                iPercentage = iRealPercentage = 100;
                uiStartTime = GetJA2Clock();
                video.BlitBufferToBuffer(SurfaceType.FRAME_BUFFER, SurfaceType.SAVE_BUFFER, new(0, 0, 640, 480));
                //                PlayJA2SampleFromFile("SOUNDS\\Laptop power down (8-11).wav", RATE_11025, HIGHVOLUME, 1, MIDDLEPAN);
                while (iRealPercentage > 0)
                {
                    video.BlitBufferToBuffer(guiEXTRABUFFER, SurfaceType.FRAME_BUFFER, new(0, 0, 640, 480));

                    uiCurrTime = GetJA2Clock();
                    iPercentage = (int)((uiCurrTime - uiStartTime) * 100 / uiTimeRange);
                    iPercentage = Math.Min(iPercentage, 100);
                    iPercentage = 100 - iPercentage;

                    iRealPercentage = iPercentage;

                    //Factor the percentage so that it is modified by a gravity falling acceleration effect.
                    iFactor = (iPercentage - 50) * 2;
                    if (iPercentage < 50)
                    {
                        iPercentage = (int)(iPercentage + iPercentage * iFactor * 0.01 + 0.5);
                    }
                    else
                    {
                        iPercentage = (int)(iPercentage + (100 - iPercentage) * iFactor * 0.01 + 0.5);
                    }

                    //Mapscreen source rect
                    SrcRect1 = new()
                    {
                        X = 464 * iPercentage / 100,
                        Width = 640 - 163 * iPercentage / 100,
                        Y = 417 * iPercentage / 100,
                        Height = 480 - 55 * iPercentage / 100
                    };

                    //Laptop source rect
                    if (iPercentage < 99)
                    {
                        iScalePercentage = 10000 / (100 - iPercentage);
                    }
                    else
                    {
                        iScalePercentage = 5333;
                    }

                    iWidth = 12 * iScalePercentage / 100;
                    iHeight = 9 * iScalePercentage / 100;
                    iX = 472 - (472 - 320) * iScalePercentage / 5333;
                    iY = 424 - (424 - 240) * iScalePercentage / 5333;

                    SrcRect2 = new();
                    SrcRect2.X = iX - iWidth / 2;
                    SrcRect2.Width = SrcRect2.X + iWidth;
                    SrcRect2.Y = iY - iHeight / 2;
                    SrcRect2.Height = SrcRect2.Y + iHeight;
                    //SrcRect2.X = 464 - 464 * iScalePercentage / 100;
                    //SrcRect2.Width = 477 + 163 * iScalePercentage / 100;
                    //SrcRect2.Y = 417 - 417 * iScalePercentage / 100;
                    //SrcRect2.Height = 425 + 55 * iScalePercentage / 100;

                    //BltStretchVideoSurface( FRAME_BUFFER, guiEXTRABUFFER, 0, 0, 0, &SrcRect1, &DstRect );

                    //SetFont( FONT10ARIAL );
                    //SetFontForeground( FONT_YELLOW );
                    //SetFontShadow( FONT_NEARBLACK );
                    //mprintf( 10, 10, L"%d . %d", iRealPercentage, iPercentage );
                    //pDestBuf = LockVideoSurface( FRAME_BUFFER, &uiDestPitchBYTES );
                    //SetClippingRegionAndImageWidth( uiDestPitchBYTES, 0, 0, 640, 480 );
                    //RectangleDraw( true, SrcRect1.X, SrcRect1.Y, SrcRect1.Width, SrcRect1.Height, Get16BPPColor( FROMRGB( 255, 100, 0 ) ), pDestBuf );
                    //RectangleDraw( true, SrcRect2.X, SrcRect2.Y, SrcRect2.Width, SrcRect2.Height, Get16BPPColor( FROMRGB( 100, 255, 0 ) ), pDestBuf );
                    //UnLockVideoSurface( FRAME_BUFFER );

                    video.BltStretchVideoSurface(SurfaceType.FRAME_BUFFER, SurfaceType.SAVE_BUFFER, new(0, 0), 0, DstRect, SrcRect2);
                    video.InvalidateScreen();
                    //gfPrintFrameBuffer = true;
                    video.RefreshScreen();
                }
            }
        }
        return (true);
    }

    private static bool fOldLeaveLaptopState = false;
    private static uint iBaseTimeExit = 0;
    private bool ExitLaptopDone()
    {
        // check if this is the first time, to reset counter

        uint iDifference = 0;

        if (fOldLeaveLaptopState == false)
        {
            fOldLeaveLaptopState = true;
            iBaseTimeExit = GetJA2Clock();
        }

        fPowerLightOn = false;


        video.InvalidateRegion(44, 466, 58, 477);
        // get the current difference
        iDifference = GetJA2Clock() - iBaseTimeExit;


        // did we wait long enough?
        if (iDifference > EXIT_LAPTOP_DELAY_TIME)
        {
            iBaseTimeExit = 0;
            fOldLeaveLaptopState = false;
            return true;
        }
        else
        {
            return false;
        }
    }

    public static void PrintNumberOnTeam()
    {
        string pString;
        SOLDIERTYPE? pSoldier;
        int cnt = 0;
        int iCounter = 0;
        int usPosX, usPosY, usFontHeight, usStrLength;


        FontSubSystem.SetFont(FontStyle.FONT10ARIAL);
        FontSubSystem.SetFontForeground(FontColor.FONT_BLACK);
        FontSubSystem.SetFontBackground(FontColor.FONT_BLACK);
        FontSubSystem.SetFontShadow(FontShadow.NO_SHADOW);

        // grab number on team
        pSoldier = MercPtrs[0];

        // for (pTeamSoldier = MercPtrs[cnt]; cnt <= gTacticalStatus.Team[pSoldier.bTeam].bLastID; cnt++, pTeamSoldier++)
        foreach (var pTeamSoldier in MercPtrs)
        {
            // pTeamSoldier = MercPtrs[cnt];

            if ((pTeamSoldier.bActive) && (!(pTeamSoldier.uiStatusFlags.HasFlag(SOLDIER.VEHICLE))))
            {
                iCounter++;
            }
        }

        pString = wprintf("%s %d", pPersonnelString[0], iCounter);

        usFontHeight = FontSubSystem.GetFontHeight(FontStyle.FONT10ARIAL);
        usStrLength = FontSubSystem.StringPixLength(pString, FontStyle.FONT10ARIAL);

        if (gLaptopButtons[3].uiFlags.HasFlag(ButtonFlags.BUTTON_CLICKED_ON))
        {
            usPosX = 47 + 1;
            usPosY = 194 + 30 + 1;
            //		gprintfdirty(47 + 1, 194 +30 +1  ,pString);
            //		mprintf(47 + 1, 194 + 30 + 1,pString);
        }
        else
        {
            usPosX = 47;
            usPosY = 194 + 30;
            //		gprintfdirty(47, 194 +30 ,pString);
            //		mprintf(47, 194 + 30,pString);
        }

        //	RestoreExternBackgroundRect( usPosX, usPosY, usStrLength, usFontHeight );
        //	gprintfdirty( usPosX, usPosY, pString);
        mprintf(usPosX, usPosY, pString);

        FontSubSystem.SetFontShadow(FontShadow.DEFAULT_SHADOW);
    }

    private static bool fOldLoadFlag = false;
    private static bool gfIsNewMailFlagSet;

    private void EnterNewLaptopMode()
    {


        if (fExitingLaptopFlag)
        {
            return;
        }
        // cause flicker, as we are going to a new program/WEB page
        fFlickerHD = true;

        // handle maximizing of programs
        switch (guiCurrentLaptopMode)
        {
            case LAPTOP_MODE.NONE:
                break;
            case (LAPTOP_MODE.EMAIL):
                if (gLaptopProgramStates[LAPTOP_PROGRAM.MAILER] == LAPTOP_PROGRAM_STATES.MINIMIZED)
                {
                    // minized, maximized
                    if (fMaximizingProgram == false)
                    {
                        fInitTitle = true;
                        InitTitleBarMaximizeGraphics(guiTITLEBARLAPTOP, pLaptopTitles[0], guiTITLEBARICONS, 0);
                        ExitLaptopMode(guiPreviousLaptopMode);

                    }
                    fMaximizingProgram = true;
                    bProgramBeingMaximized = LAPTOP_PROGRAM.MAILER;
                    gLaptopProgramStates[LAPTOP_PROGRAM.MAILER] = LAPTOP_PROGRAM_STATES.OPEN;

                    return;
                }
                break;
            case (LAPTOP_MODE.FILES):
                if (gLaptopProgramStates[LAPTOP_PROGRAM.FILES] == LAPTOP_PROGRAM_STATES.MINIMIZED)
                {
                    // minized, maximized
                    if (fMaximizingProgram == false)
                    {
                        fInitTitle = true;
                        InitTitleBarMaximizeGraphics(guiTITLEBARLAPTOP, pLaptopTitles[1], guiTITLEBARICONS, 2);
                        ExitLaptopMode(guiPreviousLaptopMode);

                    }

                    // minized, maximized
                    fMaximizingProgram = true;
                    bProgramBeingMaximized = LAPTOP_PROGRAM.FILES;
                    gLaptopProgramStates[LAPTOP_PROGRAM.FILES] = LAPTOP_PROGRAM_STATES.OPEN;
                    return;
                }
                break;
            case (LAPTOP_MODE.PERSONNEL):
                if (gLaptopProgramStates[LAPTOP_PROGRAM.PERSONNEL] == LAPTOP_PROGRAM_STATES.MINIMIZED)
                {

                    // minized, maximized
                    if (fMaximizingProgram == false)
                    {
                        fInitTitle = true;
                        InitTitleBarMaximizeGraphics(guiTITLEBARLAPTOP, pLaptopTitles[2], guiTITLEBARICONS, 3);
                        ExitLaptopMode(guiPreviousLaptopMode);

                    }

                    // minized, maximized
                    fMaximizingProgram = true;
                    bProgramBeingMaximized = LAPTOP_PROGRAM.PERSONNEL;
                    gLaptopProgramStates[LAPTOP_PROGRAM.PERSONNEL] = LAPTOP_PROGRAM_STATES.OPEN;
                    return;
                }
                break;
            case (LAPTOP_MODE.FINANCES):
                if (gLaptopProgramStates[LAPTOP_PROGRAM.FINANCES] == LAPTOP_PROGRAM_STATES.MINIMIZED)
                {

                    // minized, maximized
                    if (fMaximizingProgram == false)
                    {
                        fInitTitle = true;
                        InitTitleBarMaximizeGraphics(guiTITLEBARLAPTOP, pLaptopTitles[3], guiTITLEBARICONS, 5);
                        ExitLaptopMode(guiPreviousLaptopMode);


                    }

                    // minized, maximized
                    fMaximizingProgram = true;
                    bProgramBeingMaximized = LAPTOP_PROGRAM.FINANCES;
                    gLaptopProgramStates[LAPTOP_PROGRAM.FINANCES] = LAPTOP_PROGRAM_STATES.OPEN;
                    return;
                }
                break;
            case (LAPTOP_MODE.HISTORY):
                if (gLaptopProgramStates[LAPTOP_PROGRAM.HISTORY] == LAPTOP_PROGRAM_STATES.MINIMIZED)
                {
                    // minized, maximized
                    if (fMaximizingProgram == false)
                    {
                        fInitTitle = true;
                        InitTitleBarMaximizeGraphics(guiTITLEBARLAPTOP, pLaptopTitles[4], guiTITLEBARICONS, 4);
                        ExitLaptopMode(guiPreviousLaptopMode);


                    }
                    // minized, maximized
                    fMaximizingProgram = true;
                    bProgramBeingMaximized = LAPTOP_PROGRAM.HISTORY;
                    gLaptopProgramStates[LAPTOP_PROGRAM.HISTORY] = LAPTOP_PROGRAM_STATES.OPEN;
                    return;
                }
                break;
            default:
                if (gLaptopProgramStates[LAPTOP_PROGRAM.WEB_BROWSER] == LAPTOP_PROGRAM_STATES.MINIMIZED)
                {
                    // minized, maximized
                    if (fMaximizingProgram == false)
                    {
                        fInitTitle = true;
                        InitTitleBarMaximizeGraphics(guiTITLEBARLAPTOP, pWebTitle[0], guiTITLEBARICONS, 1);
                        ExitLaptopMode(guiPreviousLaptopMode);
                    }
                    // minized, maximized
                    fMaximizingProgram = true;
                    bProgramBeingMaximized = LAPTOP_PROGRAM.WEB_BROWSER;
                    gLaptopProgramStates[LAPTOP_PROGRAM.WEB_BROWSER] = LAPTOP_PROGRAM_STATES.OPEN;
                    return;
                }
                break;

        }

        if ((fMaximizingProgram == true) || (fMinizingProgram == true))
        {
            return;
        }

        if ((fOldLoadFlag) && (!fLoadPendingFlag))
        {
            fOldLoadFlag = false;
        }
        else if ((fLoadPendingFlag) && (!fOldLoadFlag))
        {
            ExitLaptopMode(guiPreviousLaptopMode);
            fOldLoadFlag = true;
            return;
        }
        else if ((fOldLoadFlag) && (fLoadPendingFlag))
        {
            return;
        }
        else
        {
            // do not exit previous mode if coming from sliding bar handler
            if ((fEnteredNewLapTopDueToHandleSlidingBars == false))
            {
                ExitLaptopMode(guiPreviousLaptopMode);
            }
        }



        if ((guiCurrentWWWMode == LAPTOP_MODE.NONE) && (guiCurrentLaptopMode >= LAPTOP_MODE.WWW))
        {
            RenderLapTopImage();
            giCurrentRegion = LaptopRegions.WWW_REGION;
            RestoreOldRegion(giOldRegion);
            guiCurrentLaptopMode = LAPTOP_MODE.WWW;
            HighLightRegion(giCurrentRegion);
        }
        else
        {
            if (guiCurrentLaptopMode > LAPTOP_MODE.WWW)
            {
                if (guiPreviousLaptopMode < LAPTOP_MODE.WWW)
                {
                    guiCurrentLaptopMode = guiCurrentWWWMode;
                }
                else
                {
                    guiCurrentWWWMode = guiCurrentLaptopMode;
                    giCurrentSubPage = 0;
                }
            }
        }

        if (guiCurrentLaptopMode >= LAPTOP_MODE.WWW)
        {
            RenderWWWProgramTitleBar();
        }



        if ((guiCurrentLaptopMode >= LAPTOP_MODE.WWW) && (guiPreviousLaptopMode >= LAPTOP_MODE.WWW))
        {
            gfShowBookmarks = false;
        }



        //Initialize the new mode.
        switch (guiCurrentLaptopMode)
        {

            case LAPTOP_MODE.AIM:
                EnterAIM();
                break;
            case LAPTOP_MODE.AIM_MEMBERS:
                EnterAIMMembers();
                break;
            case LAPTOP_MODE.AIM_MEMBERS_FACIAL_INDEX:
                EnterAimFacialIndex();
                break;
            case LAPTOP_MODE.AIM_MEMBERS_SORTED_FILES:
                EnterAimSort();
                break;

            case LAPTOP_MODE.AIM_MEMBERS_ARCHIVES:
                EnterAimArchives();
                break;
            case LAPTOP_MODE.AIM_POLICIES:
                EnterAimPolicies();
                break;
            case LAPTOP_MODE.AIM_LINKS:
                EnterAimLinks();
                break;
            case LAPTOP_MODE.AIM_HISTORY:
                EnterAimHistory();
                break;

            case LAPTOP_MODE.MERC:
                EnterMercs();
                break;
            case LAPTOP_MODE.MERC_FILES:
                EnterMercsFiles();
                break;
            case LAPTOP_MODE.MERC_ACCOUNT:
                EnterMercsAccount();
                break;
            case LAPTOP_MODE.MERC_NO_ACCOUNT:
                EnterMercsNoAccount();
                break;

            case LAPTOP_MODE.BOBBY_R:
                EnterBobbyR();
                break;
            case LAPTOP_MODE.BOBBY_R_GUNS:
                EnterBobbyRGuns();
                break;
            case LAPTOP_MODE.BOBBY_R_AMMO:
                EnterBobbyRAmmo();
                break;
            case LAPTOP_MODE.BOBBY_R_ARMOR:
                EnterBobbyRArmour();
                break;
            case LAPTOP_MODE.BOBBY_R_MISC:
                EnterBobbyRMisc();
                break;
            case LAPTOP_MODE.BOBBY_R_USED:
                EnterBobbyRUsed();
                break;
            case LAPTOP_MODE.BOBBY_R_MAILORDER:
                EnterBobbyRMailOrder();
                break;
            case LAPTOP_MODE.CHAR_PROFILE:
                EnterCharProfile();
                break;

            case LAPTOP_MODE.FLORIST:
                Florist.EnterFlorist();
                break;
            case LAPTOP_MODE.FLORIST_FLOWER_GALLERY:
                Florist.EnterFloristGallery();
                break;
            case LAPTOP_MODE.FLORIST_ORDERFORM:
                Florist.EnterFloristOrderForm();
                break;
            case LAPTOP_MODE.FLORIST_CARD_GALLERY:
                Florist.EnterFloristCards();
                break;

            case LAPTOP_MODE.INSURANCE:
                Insurance.EnterInsurance();
                break;
            case LAPTOP_MODE.INSURANCE_INFO:
                Insurance.EnterInsuranceInfo();
                break;
            case LAPTOP_MODE.INSURANCE_CONTRACT:
                Insurance.EnterInsuranceContract();
                break;
            case LAPTOP_MODE.INSURANCE_COMMENTS:
                Insurance.EnterInsuranceComments();
                break;

            case LAPTOP_MODE.FUNERAL:
                EnterFuneral();
                break;
            case LAPTOP_MODE.SIRTECH:
                EnterSirTech();
                break;
            case LAPTOP_MODE.FINANCES:
                Finances.EnterFinances();
                break;
            case LAPTOP_MODE.PERSONNEL:
                EnterPersonnel();
                break;
            case LAPTOP_MODE.HISTORY:
                EnterHistory();
                break;
            case LAPTOP_MODE.FILES:
                EnterFiles();
                break;
            case LAPTOP_MODE.EMAIL:
                Emails.EnterEmail();
                break;
            case LAPTOP_MODE.BROKEN_LINK:
                EnterBrokenLink();
                break;
            case LAPTOP_MODE.BOBBYR_SHIPMENTS:
                EnterBobbyRShipments();
                break;

        }

        // first time using webbrowser in this laptop session
        if ((fFirstTimeInLaptop == true) && (guiCurrentLaptopMode >= LAPTOP_MODE.WWW))
        {
            // show bookmarks
            gfShowBookmarks = true;

            // reset flag 
            fFirstTimeInLaptop = false;
        }

        if ((!fLoadPendingFlag))
        {
            CreateDestroyMinimizeButtonForCurrentMode();
            guiPreviousLaptopMode = guiCurrentLaptopMode;
            SetSubSiteAsVisted();
        }

        DisplayProgramBoundingBox(true);


        // check to see if we need to go to there default web page of not
        //HandleDefaultWebpageForLaptop( );

    }

    private void EnterBobbyRShipments()
    {
        throw new NotImplementedException();
    }

    private void EnterBrokenLink()
    {
        throw new NotImplementedException();
    }

    private void EnterFiles()
    {
        throw new NotImplementedException();
    }

    private void EnterHistory()
    {
        throw new NotImplementedException();
    }

    private void EnterPersonnel()
    {
        throw new NotImplementedException();
    }

    private void EnterSirTech()
    {
        throw new NotImplementedException();
    }

    private void EnterFuneral()
    {
        throw new NotImplementedException();
    }

    private void EnterCharProfile()
    {
        throw new NotImplementedException();
    }

    private void EnterBobbyRMailOrder()
    {
        throw new NotImplementedException();
    }

    private void EnterBobbyRUsed()
    {
        throw new NotImplementedException();
    }

    private void EnterBobbyRMisc()
    {
        throw new NotImplementedException();
    }

    private void EnterBobbyRArmour()
    {
        throw new NotImplementedException();
    }

    private void EnterBobbyRAmmo()
    {
        throw new NotImplementedException();
    }

    private void EnterBobbyRGuns()
    {
        throw new NotImplementedException();
    }

    private void EnterBobbyR()
    {
        throw new NotImplementedException();
    }

    private void EnterMercsNoAccount()
    {
        throw new NotImplementedException();
    }

    private void EnterMercsAccount()
    {
        throw new NotImplementedException();
    }

    private void EnterMercsFiles()
    {
        throw new NotImplementedException();
    }

    private void EnterMercs()
    {
        throw new NotImplementedException();
    }

    private void EnterAimHistory()
    {
        throw new NotImplementedException();
    }

    private void EnterAimLinks()
    {
        throw new NotImplementedException();
    }

    private void EnterAimPolicies()
    {
        throw new NotImplementedException();
    }

    private void EnterAimSort()
    {
        throw new NotImplementedException();
    }

    private void EnterAIMMembers()
    {
        throw new NotImplementedException();
    }

    private void EnterAimFacialIndex()
    {
        throw new NotImplementedException();
    }

    private void DisplayProgramBoundingBox(bool v)
    {
        throw new NotImplementedException();
    }

    private void SetSubSiteAsVisted()
    {
        throw new NotImplementedException();
    }

    private void CreateDestroyMinimizeButtonForCurrentMode()
    {
        throw new NotImplementedException();
    }

    private bool InitTitleBarMaximizeGraphics(HVOBJECT uiBackgroundGraphic, string pTitle, HVOBJECT uiIconGraphic, int usIconGraphicIndex)
    {
        HVOBJECT hImageHandle;

        // Create a background video surface to blt the title bar onto
        VSURFACE_DESC vs_desc = new()
        {
            fCreateFlags = VSurfaceCreateFlags.VSURFACE_CREATE_DEFAULT | VSurfaceCreateFlags.VSURFACE_SYSTEM_MEM_USAGE,
            usWidth = LAPTOP_TITLE_BAR_WIDTH,
            usHeight = LAPTOP_TITLE_BAR_HEIGHT,
            ubBitDepth = 16
        };

        var tex = video.Surfaces.CreateSurface(vs_desc);
        guiTitleBarSurface = tex.SurfaceType;

        //blit the toolbar grapgucs onto the surface
        video.BltVideoObject(guiTitleBarSurface, uiBackgroundGraphic, 0, 0, 0, VO_BLT.SRCTRANSPARENCY, null);

        //blit th icon onto the tool bar
        video.BltVideoObject(guiTitleBarSurface, uiIconGraphic, usIconGraphicIndex, LAPTOP_TITLE_BAR_ICON_OFFSET_X, LAPTOP_TITLE_BAR_ICON_OFFSET_Y, VO_BLT.SRCTRANSPARENCY, null);

        FontSubSystem.SetFontDestBuffer(guiTitleBarSurface, 0, 0, vs_desc.usWidth, vs_desc.usHeight, false);
        video.DrawTextToScreen(
            pTitle,
            LAPTOP_TITLE_BAR_TEXT_OFFSET_X,
            LAPTOP_TITLE_BAR_TEXT_OFFSET_Y,
            0,
            FontStyle.FONT14ARIAL,
            FontColor.FONT_MCOLOR_WHITE,
            FontColor.FONT_MCOLOR_BLACK,
            TextJustifies.LEFT_JUSTIFIED);

        FontSubSystem.SetFontDestBuffer(SurfaceType.FRAME_BUFFER, 0, 0, 640, 480, false);

        return (true);
    }

    private bool ExitLaptopMode(LAPTOP_MODE uiMode)
    {
        {
            //Deallocate the previous mode that you were in.

            switch (uiMode)
            {
                case LAPTOP_MODE.AIM:
                    ExitAIM();
                    break;
                case LAPTOP_MODE.AIM_MEMBERS:
                    ExitAIMMembers();
                    break;
                case LAPTOP_MODE.AIM_MEMBERS_FACIAL_INDEX:
                    ExitAimFacialIndex();
                    break;
                case LAPTOP_MODE.AIM_MEMBERS_SORTED_FILES:
                    ExitAimSort();
                    break;
                case LAPTOP_MODE.AIM_MEMBERS_ARCHIVES:
                    ExitAimArchives();
                    break;
                case LAPTOP_MODE.AIM_POLICIES:
                    ExitAimPolicies();
                    break;
                case LAPTOP_MODE.AIM_LINKS:
                    ExitAimLinks();
                    break;
                case LAPTOP_MODE.AIM_HISTORY:
                    ExitAimHistory();
                    break;

                case LAPTOP_MODE.MERC:
                    Mercs.ExitMercs();
                    break;
                case LAPTOP_MODE.MERC_FILES:
                    Mercs.ExitMercsFiles();
                    break;
                case LAPTOP_MODE.MERC_ACCOUNT:
                    Mercs.ExitMercsAccount();
                    break;
                case LAPTOP_MODE.MERC_NO_ACCOUNT:
                    Mercs.ExitMercsNoAccount();
                    break;


                case LAPTOP_MODE.BOBBY_R:
                    BobbyR.ExitBobbyR();
                    break;
                case LAPTOP_MODE.BOBBY_R_GUNS:
                    BobbyR.ExitBobbyRGuns();
                    break;
                case LAPTOP_MODE.BOBBY_R_AMMO:
                    BobbyR.ExitBobbyRAmmo();
                    break;
                case LAPTOP_MODE.BOBBY_R_ARMOR:
                    BobbyR.ExitBobbyRArmour();
                    break;
                case LAPTOP_MODE.BOBBY_R_MISC:
                    BobbyR.ExitBobbyRMisc();
                    break;
                case LAPTOP_MODE.BOBBY_R_USED:
                    BobbyR.ExitBobbyRUsed();
                    break;
                case LAPTOP_MODE.BOBBY_R_MAILORDER:
                    BobbyR.ExitBobbyRMailOrder();
                    break;


                case LAPTOP_MODE.CHAR_PROFILE:
                    ExitCharProfile();
                    break;
                case LAPTOP_MODE.FLORIST:
                    Florist.ExitFlorist();
                    break;
                case LAPTOP_MODE.FLORIST_FLOWER_GALLERY:
                    Florist.ExitFloristGallery();
                    break;
                case LAPTOP_MODE.FLORIST_ORDERFORM:
                    Florist.ExitFloristOrderForm();
                    break;
                case LAPTOP_MODE.FLORIST_CARD_GALLERY:
                    Florist.ExitFloristCards();
                    break;

                case LAPTOP_MODE.INSURANCE:
                    Insurance.ExitInsurance();
                    break;

                case LAPTOP_MODE.INSURANCE_INFO:
                    Insurance.ExitInsuranceInfo();
                    break;

                case LAPTOP_MODE.INSURANCE_CONTRACT:
                    Insurance.ExitInsuranceContract();
                    break;
                case LAPTOP_MODE.INSURANCE_COMMENTS:
                    Insurance.ExitInsuranceComments();
                    break;

                case LAPTOP_MODE.FUNERAL:
                    ExitFuneral();
                    break;
                case LAPTOP_MODE.SIRTECH:
                    ExitSirTech();
                    break;
                case LAPTOP_MODE.FINANCES:
                    Finances.ExitFinances();
                    break;
                case LAPTOP_MODE.PERSONNEL:
                    ExitPersonnel();
                    break;
                case LAPTOP_MODE.HISTORY:
                    ExitHistory();
                    break;
                case LAPTOP_MODE.FILES:
                    ExitFiles();
                    break;
                case LAPTOP_MODE.EMAIL:
                    Emails.ExitEmail();
                    break;
                case LAPTOP_MODE.BROKEN_LINK:
                    ExitBrokenLink();
                    break;

                case LAPTOP_MODE.BOBBYR_SHIPMENTS:
                    BobbyR.ExitBobbyRShipments();
                    break;
            }

            if ((uiMode != LAPTOP_MODE.NONE) && (uiMode < LAPTOP_MODE.WWW))
            {
                CreateDestroyMinimizeButtonForCurrentMode();
            }

            return (true);
        }
    }

    private void ExitBrokenLink()
    {
        throw new NotImplementedException();
    }

    private void ExitFiles()
    {
        throw new NotImplementedException();
    }

    private void ExitHistory()
    {
        throw new NotImplementedException();
    }

    private void ExitPersonnel()
    {
        throw new NotImplementedException();
    }

    private void ExitSirTech()
    {
        throw new NotImplementedException();
    }

    private void ExitFuneral()
    {
        throw new NotImplementedException();
    }

    private void ExitCharProfile()
    {
        throw new NotImplementedException();
    }

    private void ExitAimHistory()
    {
        throw new NotImplementedException();
    }

    private void ExitAimLinks()
    {
        throw new NotImplementedException();
    }

    private void ExitAimPolicies()
    {
        throw new NotImplementedException();
    }

    private void ExitAimSort()
    {
        throw new NotImplementedException();
    }

    private void ExitAIMMembers()
    {
        throw new NotImplementedException();
    }

    private void ExitAimFacialIndex()
    {
        throw new NotImplementedException();
    }

    private bool CreateLaptopButtons()
    {
        Array.Fill(giLapTopButton, -1);

        /*giLapTopButtonImage[ON_BUTTON]=  LoadButtonImage( "LAPTOP\\button.sti" ,-1,1,-1,0,-1 );
        giLapTopButton[ON_BUTTON] = QuickCreateButton( giLapTopButtonImage[ON_BUTTON], ON_X, ON_Y,
                                               BUTTON_TOGGLE, MSYS_PRIORITY_HIGHEST,
                                               (GUI_CALLBACK)BtnGenericMouseMoveButtonCallback, (GUI_CALLBACK)BtnOnCallback);
         */


        // the program buttons


        gLaptopButtonImage[0] = ButtonSubSystem.LoadButtonImage("LAPTOP\\buttonsforlaptop.sti", -1, 0, -1, 8, -1);
        gLaptopButtons[0] = ButtonSubSystem.QuickCreateButton(
            gLaptopButtonImage[0],
            new(29, 66),
            ButtonFlags.BUTTON_TOGGLE,
            MSYS_PRIORITY.HIGH,
            MouseSubSystem.BtnGenericMouseMoveButtonCallback,
            EmailRegionButtonCallback);

        CreateLaptopButtonHelpText(gLaptopButtons[0], LaptopText.LAPTOP_BN_HLP_TXT_VIEW_EMAIL);

        ButtonSubSystem.SpecifyButtonText(gLaptopButtons[0], pLaptopIcons[0]);
        ButtonSubSystem.SpecifyButtonFont(gLaptopButtons[0], FontStyle.FONT10ARIAL);
        ButtonSubSystem.SpecifyButtonTextOffsets(gLaptopButtons[0], 30, 11, true);
        ButtonSubSystem.SpecifyButtonDownTextColors(gLaptopButtons[0], (FontColor)2, 0);
        ButtonSubSystem.SpecifyButtonUpTextColors(gLaptopButtons[0], (FontColor)2, 0);

        gLaptopButtonImage[1] = ButtonSubSystem.LoadButtonImage("LAPTOP\\buttonsforlaptop.sti", -1, 1, -1, 9, -1);
        gLaptopButtons[1] = ButtonSubSystem.QuickCreateButton(
            gLaptopButtonImage[1],
            new(29, 98),
            ButtonFlags.BUTTON_TOGGLE,
            MSYS_PRIORITY.HIGH,
            MouseSubSystem.BtnGenericMouseMoveButtonCallback,
            WWWRegionButtonCallback);

        CreateLaptopButtonHelpText(gLaptopButtons[1], LaptopText.LAPTOP_BN_HLP_TXT_BROWSE_VARIOUS_WEB_SITES);

        ButtonSubSystem.SpecifyButtonText(gLaptopButtons[1], pLaptopIcons[1]);
        ButtonSubSystem.SpecifyButtonFont(gLaptopButtons[1], FontStyle.FONT10ARIAL);
        ButtonSubSystem.SpecifyButtonTextOffsets(gLaptopButtons[1], 30, 11, true);
        ButtonSubSystem.SpecifyButtonUpTextColors(gLaptopButtons[1], (FontColor)2, 0);
        ButtonSubSystem.SpecifyButtonDownTextColors(gLaptopButtons[1], (FontColor)2, 0);

        gLaptopButtonImage[2] = ButtonSubSystem.LoadButtonImage("LAPTOP\\buttonsforlaptop.sti", -1, 2, -1, 10, -1);
        gLaptopButtons[2] = ButtonSubSystem.QuickCreateButton(
            gLaptopButtonImage[2],
            new(29, 130),
            ButtonFlags.BUTTON_TOGGLE,
            MSYS_PRIORITY.HIGH,
            MouseSubSystem.BtnGenericMouseMoveButtonCallback,
            FilesRegionButtonCallback);

        CreateLaptopButtonHelpText(gLaptopButtons[2], LaptopText.LAPTOP_BN_HLP_TXT_VIEW_FILES_AND_EMAIL_ATTACHMENTS);

        ButtonSubSystem.SpecifyButtonText(gLaptopButtons[2], pLaptopIcons[5]);
        ButtonSubSystem.SpecifyButtonFont(gLaptopButtons[2], FontStyle.FONT10ARIAL);
        ButtonSubSystem.SpecifyButtonTextOffsets(gLaptopButtons[2], 30, 11, true);
        ButtonSubSystem.SpecifyButtonUpTextColors(gLaptopButtons[2], (FontColor)2, 0);
        ButtonSubSystem.SpecifyButtonDownTextColors(gLaptopButtons[2], (FontColor)2, 0);


        gLaptopButtonImage[3] = ButtonSubSystem.LoadButtonImage("LAPTOP\\buttonsforlaptop.sti", -1, 3, -1, 11, -1);
        gLaptopButtons[3] = ButtonSubSystem.QuickCreateButton(
            gLaptopButtonImage[3],
            new(29, 194),
            ButtonFlags.BUTTON_TOGGLE,
            MSYS_PRIORITY.HIGH,
            MouseSubSystem.BtnGenericMouseMoveButtonCallback,
            PersonnelRegionButtonCallback);

        CreateLaptopButtonHelpText(gLaptopButtons[3], LaptopText.LAPTOP_BN_HLP_TXT_VIEW_TEAM_INFO);

        ButtonSubSystem.SpecifyButtonText(gLaptopButtons[3], pLaptopIcons[3]);
        ButtonSubSystem.SpecifyButtonFont(gLaptopButtons[3], FontStyle.FONT10ARIAL);
        ButtonSubSystem.SpecifyButtonTextOffsets(gLaptopButtons[3], 30, 11, true);
        ButtonSubSystem.SpecifyButtonUpTextColors(gLaptopButtons[3], (FontColor)2, 0);
        ButtonSubSystem.SpecifyButtonDownTextColors(gLaptopButtons[3], (FontColor)2, 0);


        gLaptopButtonImage[4] = ButtonSubSystem.LoadButtonImage("LAPTOP\\buttonsforlaptop.sti", -1, 4, -1, 12, -1);
        gLaptopButtons[4] = ButtonSubSystem.QuickCreateButton(
            gLaptopButtonImage[4],
            new(29, 162),
            ButtonFlags.BUTTON_TOGGLE,
            MSYS_PRIORITY.HIGH,
            MouseSubSystem.BtnGenericMouseMoveButtonCallback,
            (GUI_CALLBACK)HistoryRegionButtonCallback);

        CreateLaptopButtonHelpText(gLaptopButtons[4], LaptopText.LAPTOP_BN_HLP_TXT_READ_LOG_OF_EVENTS);

        ButtonSubSystem.SpecifyButtonText(gLaptopButtons[4], pLaptopIcons[4]);
        ButtonSubSystem.SpecifyButtonFont(gLaptopButtons[4], FontStyle.FONT10ARIAL);
        ButtonSubSystem.SpecifyButtonTextOffsets(gLaptopButtons[4], 30, 11, true);
        ButtonSubSystem.SpecifyButtonUpTextColors(gLaptopButtons[4], (FontColor)2, 0);
        ButtonSubSystem.SpecifyButtonDownTextColors(gLaptopButtons[4], (FontColor)2, 0);


        gLaptopButtonImage[5] = ButtonSubSystem.LoadButtonImage("LAPTOP\\buttonsforlaptop.sti", -1, 5, -1, 13, -1);
        gLaptopButtons[5] = ButtonSubSystem.QuickCreateButton(
            gLaptopButtonImage[5],
            new Point(29, 226 + 15),
            ButtonFlags.BUTTON_TOGGLE,
            MSYS_PRIORITY.HIGH,
            MouseSubSystem.BtnGenericMouseMoveButtonCallback,
            FinancialRegionButtonCallback);

        CreateLaptopButtonHelpText(gLaptopButtons[5], LaptopText.LAPTOP_BN_HLP_TXT_VIEW_FINANCIAL_SUMMARY_AND_HISTORY);

        ButtonSubSystem.SpecifyButtonText(gLaptopButtons[5], pLaptopIcons[2]);
        ButtonSubSystem.SpecifyButtonFont(gLaptopButtons[5], FontStyle.FONT10ARIAL);
        ButtonSubSystem.SpecifyButtonTextOffsets(gLaptopButtons[5], 30, 11, true);
        ButtonSubSystem.SpecifyButtonUpTextColors(gLaptopButtons[5], (FontColor)2, 0);
        ButtonSubSystem.SpecifyButtonDownTextColors(gLaptopButtons[5], (FontColor)2, 0);


        gLaptopButtonImage[6] = ButtonSubSystem.LoadButtonImage("LAPTOP\\buttonsforlaptop.sti", -1, 6, -1, 14, -1);
        gLaptopButtons[6] = ButtonSubSystem.QuickCreateButton(
            gLaptopButtonImage[6],
            new(29, 371 + 7), //DEF: was 19
            ButtonFlags.BUTTON_TOGGLE,
            MSYS_PRIORITY.HIGH,
            MouseSubSystem.BtnGenericMouseMoveButtonCallback,
            BtnOnCallback);

        CreateLaptopButtonHelpText(gLaptopButtons[6], LaptopText.LAPTOP_BN_HLP_TXT_CLOSE_LAPTOP);

        ButtonSubSystem.SpecifyButtonText(gLaptopButtons[6], pLaptopIcons[6]);
        ButtonSubSystem.SpecifyButtonFont(gLaptopButtons[6], FontStyle.FONT10ARIAL);
        ButtonSubSystem.SpecifyButtonTextOffsets(gLaptopButtons[6], 25, 11, true);
        ButtonSubSystem.SpecifyButtonUpTextColors(gLaptopButtons[6], (FontColor)2, 0);
        ButtonSubSystem.SpecifyButtonDownTextColors(gLaptopButtons[6], (FontColor)2, 0);

        // define the cursor
        ButtonSubSystem.SetButtonCursor(gLaptopButtons[0], CURSOR.LAPTOP_SCREEN);
        ButtonSubSystem.SetButtonCursor(gLaptopButtons[1], CURSOR.LAPTOP_SCREEN);
        ButtonSubSystem.SetButtonCursor(gLaptopButtons[2], CURSOR.LAPTOP_SCREEN);
        ButtonSubSystem.SetButtonCursor(gLaptopButtons[3], CURSOR.LAPTOP_SCREEN);
        ButtonSubSystem.SetButtonCursor(gLaptopButtons[4], CURSOR.LAPTOP_SCREEN);
        ButtonSubSystem.SetButtonCursor(gLaptopButtons[5], CURSOR.LAPTOP_SCREEN);
        ButtonSubSystem.SetButtonCursor(gLaptopButtons[6], CURSOR.LAPTOP_SCREEN);

        buttonList.AddRange(gLaptopButtons);

        return (true);
    }

    private void PersonnelRegionButtonCallback(ref GUI_BUTTON button, MSYS_CALLBACK_REASON reason)
    {
        throw new NotImplementedException();
    }

    private void HistoryRegionButtonCallback(ref GUI_BUTTON button, MSYS_CALLBACK_REASON reason)
    {
        throw new NotImplementedException();
    }

    private void BtnOnCallback(ref GUI_BUTTON btn, MSYS_CALLBACK_REASON reason)
    {
        if (!(btn.uiFlags.HasFlag(ButtonFlags.BUTTON_ENABLED)))
        {
            return;
        }

        if (reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_DWN))
        {
            if (!(btn.uiFlags.HasFlag(ButtonFlags.BUTTON_CLICKED_ON)))
            {
                btn.uiFlags |= (ButtonFlags.BUTTON_CLICKED_ON);
            }

            video.InvalidateRegion(0, 0, 640, 480);

        }
        else if (reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP))
        {
            if (btn.uiFlags.HasFlag(ButtonFlags.BUTTON_CLICKED_ON))
            {
                if (HandleExit())
                {
                    //			 btn.uiFlags&=~(BUTTON_CLICKED_ON);
                    fExitingLaptopFlag = true;
                    video.InvalidateRegion(0, 0, 640, 480);
                }
            }
            btn.uiFlags &= ~(ButtonFlags.BUTTON_CLICKED_ON);
        }

    }

    private bool HandleExit()
    {
        //	static BOOLEAN fSentImpWarningAlready = false;


        // remind player about IMP
        if (LaptopSaveInfo.gfNewGameLaptop)
        {
            if (!GameInit.AnyMercsHired())
            {
                //AddEmail(0,1, GAME_HELP, GetWorldTotalMin( ) );
                //fExitingLaptopFlag = false;
                //return( false );
            }
        }

        // new game, send email 
        if (LaptopSaveInfo.gfNewGameLaptop)
        {
            // Set an event to send this email ( day 2 8:00-12:00 )
            if ((LaptopSaveInfo.fIMPCompletedFlag == false) && (LaptopSaveInfo.fSentImpWarningAlready == false))
            {
                GameEvents.AddFutureDayStrategicEvent(EVENT.HAVENT_MADE_IMP_CHARACTER_EMAIL, (uint)(8 + Globals.Random.GetRandom(4)) * 60, 0, 1);

                /*
                 Moved to an event that gets triggered the next day: HaventMadeImpMercEmailCallBack()

                            LaptopSaveInfo.fSentImpWarningAlready = true;
                            AddEmail(IMP_EMAIL_AGAIN,IMP_EMAIL_AGAIN_LENGTH,1, GetWorldTotalMin( ) );
                */
                fExitingLaptopFlag = true;

                return (false);
            }
        }


        return (true);
    }

    private void WWWRegionButtonCallback(ref GUI_BUTTON btn, MSYS_CALLBACK_REASON reason)
    {
        if (!(btn.uiFlags.HasFlag(ButtonFlags.BUTTON_ENABLED)))
        {
            return;
        }

        if (reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_DWN))
        {
            if (!(btn.uiFlags.HasFlag(ButtonFlags.BUTTON_CLICKED_ON)))
            {
                btn.uiFlags |= (ButtonFlags.BUTTON_CLICKED_ON);
            }
        }
        else if (reason.HasFlag(MSYS_CALLBACK_REASON.RBUTTON_DWN))
        {
            if (!(btn.uiFlags.HasFlag(ButtonFlags.BUTTON_CLICKED_ON)))
            {
                btn.uiFlags |= (ButtonFlags.BUTTON_CLICKED_ON);
            }
        }
        else if (reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP))
        {
            if (btn.uiFlags.HasFlag(ButtonFlags.BUTTON_CLICKED_ON))
            {
                btn.uiFlags &= ~(ButtonFlags.BUTTON_CLICKED_ON);
                if (giCurrentRegion != LaptopRegions.WWW_REGION)
                {
                    giOldRegion = giCurrentRegion;
                }

                if (!fNewWWW)
                {
                    fNewWWWDisplay = false;
                }

                // reset show bookmarks
                if (guiCurrentLaptopMode < LAPTOP_MODE.WWW)
                {
                    gfShowBookmarks = false;
                    fShowBookmarkInfo = true;
                }
                else
                {
                    gfShowBookmarks = !gfShowBookmarks;
                }


                if ((gfShowBookmarks) && (!fNewWWW))
                {

                    fReDrawScreenFlag = true;
                    fNewWWWDisplay = false;
                }
                else if (fNewWWW)
                {

                    // no longer a new WWW mode
                    fNewWWW = false;

                    // new WWW to display
                    fNewWWWDisplay = true;

                    // make sure program is maximized
                    if (gLaptopProgramStates[LAPTOP_PROGRAM.WEB_BROWSER] == LAPTOP_PROGRAM_STATES.OPEN)
                    {
                        // re render laptop region
                        RenderLapTopImage();

                        // re render background
                        DrawDeskTopBackground();

                    }
                }
                giCurrentRegion = LaptopRegions.WWW_REGION;
                RestoreOldRegion(giOldRegion);
                if (guiCurrentWWWMode != LAPTOP_MODE.NONE)
                {
                    guiCurrentLaptopMode = guiCurrentWWWMode;
                }
                else
                {
                    guiCurrentLaptopMode = LAPTOP_MODE.WWW;
                }

                UpdateListToReflectNewProgramOpened(LAPTOP_PROGRAM.WEB_BROWSER);
                HighLightRegion(giCurrentRegion);
                fReDrawScreenFlag = true;
            }
        }
        else if (reason.HasFlag(MSYS_CALLBACK_REASON.RBUTTON_UP))
        {
            if (btn.uiFlags.HasFlag(ButtonFlags.BUTTON_CLICKED_ON))
            {
                btn.uiFlags &= ~(ButtonFlags.BUTTON_CLICKED_ON);
                // nothing yet


                if (giCurrentRegion != LaptopRegions.WWW_REGION)
                {
                    giOldRegion = giCurrentRegion;
                }

                giCurrentRegion = LaptopRegions.WWW_REGION;

                RestoreOldRegion(giOldRegion);

                if (guiCurrentWWWMode != LAPTOP_MODE.NONE)
                {
                    guiCurrentLaptopMode = guiCurrentWWWMode;
                }
                else
                {
                    guiCurrentLaptopMode = LAPTOP_MODE.WWW;
                }

                HighLightRegion(giCurrentRegion);

                fReDrawScreenFlag = true;
            }
        }
    }

    private void FilesRegionButtonCallback(ref GUI_BUTTON btn, MSYS_CALLBACK_REASON reason)
    {
        if (!(btn.uiFlags.HasFlag(ButtonFlags.BUTTON_ENABLED)))
        {
            return;
        }

        if (reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_DWN))
        {
            if (!(btn.uiFlags.HasFlag(ButtonFlags.BUTTON_CLICKED_ON)))
            {
                btn.uiFlags |= (ButtonFlags.BUTTON_CLICKED_ON);
            }
        }
        else if (reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP))
        {
            if (btn.uiFlags.HasFlag(ButtonFlags.BUTTON_CLICKED_ON))
            {
                btn.uiFlags &= ~(ButtonFlags.BUTTON_CLICKED_ON);
                // reset old region
                if (giCurrentRegion != LaptopRegions.FILES_REGION)
                {
                    giOldRegion = giCurrentRegion;
                }

                // stop showing WWW bookmarks
                if (gfShowBookmarks)
                {
                    gfShowBookmarks = false;
                    fReDrawScreenFlag = true;
                }

                // set new region
                giCurrentRegion = LaptopRegions.FILES_REGION;

                // restore old highlight region
                RestoreOldRegion(giOldRegion);

                // highlight new region
                HighLightRegion(giCurrentRegion);

                guiCurrentLaptopMode = LAPTOP_MODE.FILES;

                UpdateListToReflectNewProgramOpened(LAPTOP_PROGRAM.FILES);

                //redraw screen
                fReDrawScreenFlag = true;
            }
        }
    }

    private void CreateLaptopButtonHelpText(GUI_BUTTON iButtonIndex, LaptopText text)
    {
        ButtonSubSystem.SetButtonFastHelpText(iButtonIndex, gzLaptopHelpText[text]);
    }

    private void EmailRegionButtonCallback(ref GUI_BUTTON btn, MSYS_CALLBACK_REASON reason)
    {
        if (!(btn.uiFlags.HasFlag(ButtonFlags.BUTTON_ENABLED)))
        {
            return;
        }

        if (reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_DWN))
        {
            if (!(btn.uiFlags.HasFlag(ButtonFlags.BUTTON_CLICKED_ON)))
            {
                btn.uiFlags |= (ButtonFlags.BUTTON_CLICKED_ON);
            }
        }
        else if (reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP))
        {
            if (btn.uiFlags.HasFlag(ButtonFlags.BUTTON_CLICKED_ON))
            {
                btn.uiFlags &= ~(ButtonFlags.BUTTON_CLICKED_ON);
                // set old region
                if (giCurrentRegion != LaptopRegions.EMAIL_REGION)
                {
                    giOldRegion = giCurrentRegion;
                }

                // stop showing WWW bookmarks
                if (gfShowBookmarks)
                {
                    gfShowBookmarks = false;
                }

                // set current highlight region
                giCurrentRegion = LaptopRegions.EMAIL_REGION;

                // restore old region
                RestoreOldRegion(giOldRegion);

                // set up current mode
                guiCurrentLaptopMode = LAPTOP_MODE.EMAIL;

                UpdateListToReflectNewProgramOpened(LAPTOP_PROGRAM.MAILER);

                // highlight current region
                HighLightRegion(giCurrentRegion);

                //redraw screen
                fReDrawScreenFlag = true;
            }
        }
    }

    private static void HighLightRegion(LaptopRegions iCurrentRegion)
    {
    }

    private void RestoreOldRegion(LaptopRegions iOldRegion)
    {
    }

    private void FinancialRegionButtonCallback(ref GUI_BUTTON btn, MSYS_CALLBACK_REASON reason)
    {

        if (!(btn.uiFlags.HasFlag(ButtonFlags.BUTTON_ENABLED)))
        {
            return;
        }

        if (reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_DWN))
        {
            if (!(btn.uiFlags.HasFlag(ButtonFlags.BUTTON_CLICKED_ON)))
            {
                btn.uiFlags |= (ButtonFlags.BUTTON_CLICKED_ON);
            }
        }
        else if (reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP))
        {
            if (btn.uiFlags.HasFlag(ButtonFlags.BUTTON_CLICKED_ON))
            {

                btn.uiFlags &= ~(ButtonFlags.BUTTON_CLICKED_ON);
                if (giCurrentRegion != LaptopRegions.FINANCIAL_REGION)
                {
                    giOldRegion = giCurrentRegion;
                }

                giCurrentRegion = LaptopRegions.FINANCIAL_REGION;
                if (gfShowBookmarks)
                {
                    gfShowBookmarks = false;
                    fReDrawScreenFlag = true;
                }
                guiCurrentLaptopMode = LAPTOP_MODE.FINANCES;

                UpdateListToReflectNewProgramOpened(LAPTOP_PROGRAM.FINANCES);

            }
        }
    }

    private void UpdateListToReflectNewProgramOpened(LAPTOP_PROGRAM iOpenedProgram)
    {
        // will update queue of opened programs to show thier states
        // set iOpenedProgram to 1, and update others

        // increment everyone
        foreach (var program in Enum.GetValues<LAPTOP_PROGRAM>())
        {
            gLaptopProgramQueueList[program]++;
        }

        gLaptopProgramQueueList[iOpenedProgram] = 1;
    }

}
