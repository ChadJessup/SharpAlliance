// Functions ported from laptop.c that depend on code that does not exist in the port yet
// (keyboard event queue, frame-buffer blitters, video overlays, ...). They are kept here, translated by
// tools/c2cs.py and compiled out, so that the work is not lost. Move a function back into Laptop.Shell.cs
// once its dependencies exist.
#if false
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SharpAlliance.Core.Managers;
using SharpAlliance.Core.Managers.VideoSurfaces;
using SharpAlliance.Core.Screens;
using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core.SubSystems.LaptopSubSystem;

public partial class Laptop
{
    public void HandleLapTopCursorUpDate()
    {
        if(guiPreviousLapTopCursor==guiCurrentLapTopCursor)
         return;
        switch(guiCurrentLapTopCursor)
        {
            case LAPTOP.PANEL_CURSOR:
                MouseSubSystem.MSYS_SetCurrentCursor(CURSOR.NORMAL);
            break;
            case LAPTOP.SCREEN_CURSOR:
                MouseSubSystem.MSYS_SetCurrentCursor(CURSOR.LAPTOP_SCREEN);
            break;
            case LAPTOP.WWW_CURSOR:
                MouseSubSystem.MSYS_SetCurrentCursor(CURSOR.WWW);
            break;
        }
      guiPreviousLapTopCursor=guiCurrentLapTopCursor;

    }

    public int DrawLapTopText()
    {

        // show balance
        DisplayPlayersBalanceToDate( );


        return (true);
    }

    public int LaptopScreenShutdown()
    {
        InsuranceContractEndGameShutDown();
        BobbyRayMailOrderEndGameShutDown();
        Emails.ShutDownEmailList();

        History.ClearHistoryList( );

      return true;
    }

    public int LaptopScreenHandle()
    {
        //User just changed modes.  This is determined by the button callbacks
        //created in LaptopScreenInit()

        // just entered
        if(gfEnterLapTop)
        {
         EnterLaptop();
         CreateLaptopButtons();
       gfEnterLapTop=false;
        }

        if( gfStartMapScreenToLaptopTransition )
        { //Everything is set up to start the transition animation.
            SGPRect SrcRect1, SrcRect2, DstRect;
            int iPercentage, iScalePercentage, iFactor;
            int uiStartTime, uiTimeRange, uiCurrTime;
            int iX, iY, iWidth, iHeight;

            int iRealPercentage;

            SetCurrentCursorFromDatabase( CURSOR.VIDEO_NO_CURSOR );
            //Step 1:  Build the laptop image into the save buffer.
            gfStartMapScreenToLaptopTransition = false;
            RestoreBackgroundRects();
            RenderLapTopImage();
            HighLightRegion(giCurrentRegion);
            RenderLaptop();
            ButtonSubSystem.RenderButtons();
            PrintDate( );
            PrintBalance( );
            PrintNumberOnTeam( );
            ShowLights();

            //Step 2:  The mapscreen image is in the EXTRABUFFER, and laptop is in the SAVEBUFFER
            //         Start transitioning the screen.
            DstRect.iLeft = 0;
            DstRect.iTop = 0;
            DstRect.iRight = 640;
            DstRect.iBottom = 480;
            uiTimeRange = 1000;
            iPercentage = iRealPercentage = 0;
            uiStartTime = (int)GetJA2Clock();
            BlitBufferToBuffer( SurfaceType.FRAME_BUFFER, guiSAVEBUFFER, 0, 0, 640, 480 );
            BlitBufferToBuffer( guiEXTRABUFFER, SurfaceType.FRAME_BUFFER, 0, 0, 640, 480 );
            PlayJA2SampleFromFile( "LibraryNames.SOUNDS\\Laptop power up (8-11).wav", RATE_11025, HIGHVOLUME, 1, SoundPans.MIDDLEPAN );
            while( iRealPercentage < 100  )
            {
                uiCurrTime = (int)GetJA2Clock();
                iPercentage = (uiCurrTime-uiStartTime) * 100 / uiTimeRange;
                iPercentage = Math.Min( iPercentage, 100 );

                iRealPercentage = iPercentage;

                //Factor the percentage so that it is modified by a gravity falling acceleration effect.
                iFactor = (iPercentage - 50) * 2;
                if( iPercentage < 50 )
                    iPercentage = (int)(iPercentage + iPercentage * iFactor * 0.01 + 0.5);
                else
                    iPercentage = (int)(iPercentage + (100-iPercentage) * iFactor * 0.01 + 0.5);

                //Mapscreen source rect
                SrcRect1.iLeft = 464 * iPercentage / 100;
                SrcRect1.iRight = 640 - 163 * iPercentage / 100;
                SrcRect1.iTop = 417 * iPercentage / 100;
                SrcRect1.iBottom = 480 - 55 * iPercentage / 100;
                //Laptop source rect
                if( iPercentage < 99 )
                    iScalePercentage = 10000 / (100-iPercentage);
                else
                    iScalePercentage = 5333;
                iWidth = 12 * iScalePercentage / 100;
                iHeight = 9 * iScalePercentage / 100;
                iX = 472 - (472-320) * iScalePercentage / 5333;
                iY = 424 - (424-240) * iScalePercentage / 5333;

                SrcRect2.iLeft = iX - iWidth / 2;
                SrcRect2.iRight = SrcRect2.iLeft + iWidth;
                SrcRect2.iTop = iY - iHeight / 2;
                SrcRect2.iBottom = SrcRect2.iTop + iHeight;
                //SrcRect2.iLeft = 464 - 464 * iScalePercentage / 100;
                //SrcRect2.iRight = 477 + 163 * iScalePercentage / 100;
                //SrcRect2.iTop = 417 - 417 * iScalePercentage / 100;
                //SrcRect2.iBottom = 425 + 55 * iScalePercentage / 100;

                //BltStretchVideoSurface( FRAME_BUFFER, guiEXTRABUFFER, 0, 0, 0, &SrcRect1, &DstRect );

                //FontSubSystem.SetFont( FONT10ARIAL );
                //FontSubSystem.SetFontForeground( FONT_YELLOW );
                //FontSubSystem.SetFontShadow( FONT_NEARBLACK );
                //mprintf( 10, 10, L"%d -> %d", iRealPercentage, iPercentage );
                //pDestBuf = LockVideoSurface( FRAME_BUFFER, out uiDestPitchBYTES );
                //SetClippingRegionAndImageWidth( uiDestPitchBYTES, 0, 0, 640, 480 );
                //RectangleDraw( TRUE, SrcRect1.iLeft, SrcRect1.iTop, SrcRect1.iRight, SrcRect1.iBottom, Get16BPPColor( FROMRGB( 255, 100, 0 ) ), pDestBuf );
                //RectangleDraw( TRUE, SrcRect2.iLeft, SrcRect2.iTop, SrcRect2.iRight, SrcRect2.iBottom, Get16BPPColor( FROMRGB( 100, 255, 0 ) ), pDestBuf );
                //UnLockVideoSurface( FRAME_BUFFER );

                BltStretchVideoSurface( SurfaceType.FRAME_BUFFER, guiSAVEBUFFER, 0, 0, 0, &DstRect, &SrcRect2 );  // TODO(port): address-of
                InvalidateScreen();
                //gfPrintFrameBuffer = TRUE;
                RefreshScreen( null );
            }
            fReDrawScreenFlag = true;
        }


        //DO NOT MOVE THIS FUNCTION CALL!!!

        //This determines if the help screen should be active
        if( HelpScreen.ShouldTheHelpScreenComeUp( HELP_SCREEN.LAPTOP, false ) )
        {
            // handle the help screen
            HelpScreen.HelpScreenHandler();
            return( LAPTOP_SCREEN );
        }

        RestoreBackgroundRects();

        // lock cursor to screen
        RestrictMouseCursor( LaptopScreenRect );



        // handle animated cursors
        HandleAnimatedCursors( );
        // Deque all game events
        DequeAllGameEvents( true );

        // handle sub sites..like BR Guns, BR Ammo, Armour, Misc...for WW Wait..since they are not true sub pages
        // and are not individual sites
        HandleWWWSubSites( );
        UpdateStatusOfDisplayingBookMarks( );

        // check if we need to reset new WWW mode
        CheckIfNewWWWW( );

        if( guiCurrentLaptopMode != guiPreviousLaptopMode )
        {
            if( guiCurrentLaptopMode <=LAPTOP_MODE.WWW )
            {
                fLoadPendingFlag=false;
            }

            if( ( fMaximizingProgram == false ) && ( fMinizingProgram == false ) )
            {
                if( guiCurrentLaptopMode <= LAPTOP_MODE.WWW )
                {
                    EnterNewLaptopMode();
                    if( ( fMaximizingProgram == false ) && ( fMinizingProgram == false ) )
                    {
                        guiPreviousLaptopMode = guiCurrentLaptopMode;
                    }
                }
            else
                {
                     if(!fLoadPendingFlag)
                     {
                        EnterNewLaptopMode();
                        guiPreviousLaptopMode = guiCurrentLaptopMode;
                     }
                }
            }
      }
        if( fPausedReDrawScreenFlag )
        {
            fReDrawScreenFlag = true;
            fPausedReDrawScreenFlag = false;
        }

      if( fReDrawScreenFlag )
        {
       RenderLapTopImage();
         HighLightRegion(giCurrentRegion);
         RenderLaptop();

        }


        // are we about to leave laptop
        if( fExitingLaptopFlag )
        {
            if( fLoadPendingFlag == true )
            {
              fLoadPendingFlag = false;
              fExitDuringLoad = true;
            }
            LeaveLapTopScreen( );
        }

        if( fExitingLaptopFlag == false )
        {
          // handle handles for laptop input stream
          HandleLapTopHandles();
        }

        // get keyboard input, handle it
        GetLaptopKeyboardInput();





        // check to see if new mail box needs to be displayed
        Emails.DisplayNewMailBox( );
      Emails.CreateDestroyNewMailButton( );

        // create various mouse regions that are global to laptop system
      CreateDestoryBookMarkRegions( );
        CreateDestroyErrorButton( );

        // check to see if error box needs to be displayed
        DisplayErrorBox();

        // check to see if buttons marked dirty
      CheckMarkButtonsDirtyFlag( );

      // check to see if new mail box needs to be displayed
        ShouldNewMailBeDisplayed();

        // check to see if new mail box needs to be displayed
        Emails.ReDrawNewMailBox( );


        // look for unread email
        LookForUnread();
        //Handle keyboard shortcuts...

        // mouse regions
      //HandleLapTopScreenMouseUi();
      //ButtonSubSystem.RenderButtons();
        //RenderButtonsFastHelp( );



        if( ( fLoadPendingFlag == false )||( Emails.fNewMailFlag  ) )
        {
          // render buttons marked dirty
          ButtonSubSystem.RenderButtons( );

        // render fast help 'quick created' buttons
    //      RenderFastHelp( );
    //    RenderButtonsFastHelp( );
      }

        // show text on top of buttons
        if( ( fMaximizingProgram == false ) && ( fMinizingProgram == false ) )
        {
          DrawButtonText();
        }


        // check to see if bookmarks need to be displayed
        if(gfShowBookmarks)
        {
            if( fExitingLaptopFlag )
                gfShowBookmarks = false;
            else
                DisplayBookMarks();
        }

        // check to see if laod pending flag is set
        DisplayLoadPending( );

        // check if we are showing message?
        DisplayWebBookMarkNotify( );

        if( ( fIntermediateReDrawFlag )||( fReDrawPostButtonRender ) )
        {
          // rendering AFTER buttons and button text
            if( ( fMaximizingProgram == false ) && ( fMinizingProgram == false ) )
            {
              PostButtonRendering( );
            }
      }
        //PrintBalance( );

        PrintDate( );

        PrintBalance( );

        PrintNumberOnTeam( );
        DisplayTaskBarIcons();

        // handle if we are maximizing a program from a minimized state or vice versa
        HandleSlidingTitleBar( );


        // flicker HD light as nessacary
        FlickerHDLight( );

        // display power and HD lights
        ShowLights( );


        // render frame rate
        DisplayFrameRate( );

        // invalidate screen if redrawn
        if( fReDrawScreenFlag == true )
        {
            video.InvalidateRegion(0,0,640,480);
        fReDrawScreenFlag = false;
        }

        ExecuteVideoOverlays();

        SaveBackgroundRects( );
    //  RenderButtonsFastHelp();
        RenderFastHelp( );

      // ex SAVEBUFFER queue
        ExecuteBaseDirtyRectQueue();
        ResetInterface();
        EndFrameBufferRender( );
        return (LAPTOP_SCREEN);
    }

    public void DeleteLapTopButtons()
    {
      int cnt;
    /*  for ( cnt = 0; cnt < MAX_BUTTON_COUNT; cnt++ )
        {
            if (giLapTopButton[ cnt ] != -1 )
            {
                ButtonSubSystem.RemoveButton( giLapTopButton[ cnt ] );
            }
        }


        for ( cnt = 0; cnt < MAX_BUTTON_COUNT; cnt++ )
        {
            ButtonSubSystem.UnloadButtonImage( giLapTopButtonImage[ cnt ] );
        }

    */
        for( cnt = 0; cnt < 7; cnt++ )
        {
            ButtonSubSystem.RemoveButton( gLaptopButton[ cnt ] );
            ButtonSubSystem.UnloadButtonImage( gLaptopButtonImage[ cnt ] );
        }
    }

    public void HaventMadeImpMercEmailCallBack()
    {
        //if the player STILL hasnt made an imp merc yet
        if( ( LaptopSaveInfo.fIMPCompletedFlag == false ) && ( LaptopSaveInfo.fSentImpWarningAlready == false ) )
        {
            LaptopSaveInfo.fSentImpWarningAlready = true;
            Emails.AddEmail(IMP_EMAIL_AGAIN,IMP_EMAIL_AGAIN_LENGTH,1, GameClock.GetWorldTotalMin( ) );
        }
    }
    public void EmailRegionMvtCallback(MOUSE_REGION? pRegion, MSYS_CALLBACK_REASON iReason)
    {


        int iDifference=0;
      HVOBJECT hLapTopIconHandle;
        if (iReason.HasFlag(MSYS_CALLBACK_REASON.LOST_MOUSE) )
        {
            EmailRegionMvtCallback_iBaseTime=0;
            EmailRegionMvtCallback_iFrame=0;
        DrawLapTopText();
          hLapTopIconHandle = guiMAILICON;
          video.BltVideoObject(SurfaceType.FRAME_BUFFER, hLapTopIconHandle, (int)EmailRegionMvtCallback_iFrame,LAPTOP_ICONS_X, LAPTOP_ICONS_MAIL_Y, VO_BLT.SRCTRANSPARENCY,null);
            if(fUnReadMailFlag)
            {
           hLapTopIconHandle = guiUNREAD;
           video.BltVideoObject(SurfaceType.FRAME_BUFFER, hLapTopIconHandle, 0,LAPTOP_ICONS_X+CHECK_X, LAPTOP_ICONS_MAIL_Y+CHECK_Y, VO_BLT.SRCTRANSPARENCY,null);
            }
            DrawLapTopText();
            HighLightRegion(giCurrentRegion);
            video.InvalidateRegion(0,0,640,480);
        }
    }

    public void CheckIfMouseLeaveScreen()
    {
        POINT  MousePos;
      GetCursorPos( out MousePos );
      if((MousePos.x >LAPTOP_SCREEN_LR_X )||(MousePos.x<LAPTOP_UL_X)||(MousePos.y<LAPTOP_UL_Y )||(MousePos.y >LAPTOP_SCREEN_LR_Y))
      {
       guiCurrentLapTopCursor=LAPTOP.PANEL_CURSOR;
      }

    }

    public bool RemoveBookMark(BOOKMARK iBookId)
    {
      int iCounter=0;

        //Loop through the bookmarks to get to the desired bookmark
        while( LaptopSaveInfo.iBookMarkList[iCounter] != iBookId )
        {
            iCounter++;
        }

        //Did we find the right one?
        if( LaptopSaveInfo.iBookMarkList[iCounter] == iBookId )
        {
            //Reset it
            LaptopSaveInfo.iBookMarkList[iCounter] = -1;

            //return true signifing that we found it
            return( true );
        }

        //nope, we didnt find it.
        return( false );
    }

    public void RemoveBookmark(BOOKMARK iBookId)
    {
      int iCounter=0;
        if(iBookId==-2)
            return;
        while( LaptopSaveInfo.iBookMarkList[iCounter]!=-1)
        {
            if( LaptopSaveInfo.iBookMarkList[iCounter]==iBookId)
            {
         // found, move everyone back
                for(iCounter=iCounter+1; iCounter <MAX_BOOKMARKS; iCounter++)
                {
                    LaptopSaveInfo.iBookMarkList[iCounter-1] = LaptopSaveInfo.iBookMarkList[iCounter];
                }
                return;
            }
            iCounter++;
        }
        return;
    }
    public void ScrollDisplayText(int iY)
    {


        int sY=(int)iY;

        // if we are just enetering, set basetime to current clock value
        if(ScrollDisplayText_iBaseTime==0)
            ScrollDisplayText_iBaseTime=(int)GetJA2Clock();

        // long enough time has passed, shift string
        if((int)GetJA2Clock()-ScrollDisplayText_iBaseTime >SCROLL_DIFFERENCE)
        {

       // reset postion, if scrolled too far
         if(ScrollDisplayText_sCurX <SCROLL_MIN)
             ScrollDisplayText_sCurX=BOOK_X+BOOK_WIDTH;
         else
             ScrollDisplayText_sCurX--;

         // reset base time
         ScrollDisplayText_iBaseTime=(int)GetJA2Clock();
        }

       // font stuff
       FontSubSystem.SetFontDestBuffer(SurfaceType.FRAME_BUFFER, BOOK_X,0,BOOK_X+BOOK_WIDTH,480, false);
         FontSubSystem.SetFontForeground(FontColor.FONT_BLACK);
         FontSubSystem.SetFontBackground(FontColor.FONT_BLACK);

         // print the scrolling string for bookmarks
         mprintf(ScrollDisplayText_sCurX, iY, pBookmarkTitle[1]);

         // reset buffer
         FontSubSystem.SetFontDestBuffer(SurfaceType.FRAME_BUFFER, 0,0,640,480, false);

         // invalidate region
         video.InvalidateRegion(BOOK_X,iY, BOOK_X+BOOK_WIDTH, iY+BOOK_HEIGHT);

    }

    public void DisplayPlayersBalanceToDate()
    {
      // print players balance to date
      char[] sString = new char[ 100 ];
      int sX, sY;

        // initialize string
        memset( sString, 0, sizeof( sString ) );  // TODO(port): mem*, sizeof

        // font stuff
        FontSubSystem.SetFont( FontStyle.FONT10ARIAL);
        FontSubSystem.SetFontForeground( 142 );
      FontSubSystem.SetFontShadow(FontShadow.NO_SHADOW);

        // parse straigth number
        sString = wprintf( "%d", LaptopSaveInfo.iCurrentBalance );

        // put in commas, then dollar sign
        Finances.InsertCommasForDollarFigure( sString );
        Finances.InsertDollarSignInToString( sString );

        // get center
        FontSubSystem.FindFontCenterCoordinates( (int)LAPTOP_ICON_TEXT_X, 0, (int)(LAPTOP_ICON_TEXT_WIDTH), (int)(LAPTOP_ICON_TEXT_HEIGHT), sString, LAPTOPICONFONT, out sX, out sY );
    //  gprintfdirty( sX , LAPTOP_ICON_TEXT_FINANCIAL_Y + 10, sString );
        // printf it!
        if( ButtonList[ gLaptopButton[ 5 ] ].uiFlags.HasFlag(ButtonFlags.BUTTON_CLICKED_ON) )
        {
            mprintf( sX + 5, LAPTOP_ICON_TEXT_FINANCIAL_Y + 10 + 5, sString);
        }
        else
        {
            mprintf( sX, LAPTOP_ICON_TEXT_FINANCIAL_Y + 10, sString);
        }

        // reset shadow
      FontSubSystem.SetFontShadow(FontShadow.DEFAULT_SHADOW);

        return;
    }

    // note: zString: pointer-to-primitive -> ref (use out if only written)
    public bool DoLapTopSystemMessageBox(MessageBoxStyle ubStyle, ref int zString, ScreenName uiExitScreen, int usFlags, MSGBOX_CALLBACK ReturnCallback)
    {
      SGPRect CenteringRect= {0, 0, 640, INV_INTERFACE_START_Y };
        // reset exit mode
        fExitDueToMessageBox = true;

        // do message box and return
      iLaptopMessageBox = DoMessageBox( ubStyle, zString, uiExitScreen, ( int ) ( usFlags| MSG_BOX_FLAG.USE_CENTERING_RECT ), ReturnCallback, ref CenteringRect );

        // send back return state
        return( ( iLaptopMessageBox != -1 ) );
    }

    public void DeleteDesktopBackground()
    {
        // delete desktop

        DeleteVideoSurfaceFromIndex(guiDESKTOP);
        return;
    }

    public void HandleKeyBoardShortCutsForLapTop(int usEvent, int usParam, int usKeyState)
    {

        // will handle keyboard shortcuts for the laptop ... to be added to later

        if( ( fExitingLaptopFlag == true )||( fTabHandled ) )
        {
            return;
        }

        if ( (usEvent == KEY_DOWN ) && (usParam == ESC ) )
        {
            // esc hit, check to see if boomark list is shown, if so, get rid of it, otherwise, leave
        HandleLapTopESCKey( );
        }
        else if( (usEvent == KEY_DOWN ) && ( usParam == TAB ) )
        {
            if(usKeyState & CTRL_DOWN )  // TODO(port): bit test: check flag type
            {
                HandleShiftAltTabKeyInLaptop( );
            }
            else
            {
                HandleAltTabKeyInLaptop( );
            }

            fTabHandled = true;
        }

    // #ifdef JA2TESTVERSION

    // else if ((usEvent == KEY_DOWN )&& ( usParam == 'm' ) )
    // {
    // if( ( usKeyState & ALT_DOWN ) )
    // {
    // CheatToGetAll5Merc();
    // }
    // }
    // #endif

        else if( (usEvent == KEY_DOWN )&& ( usParam == 'b' ) )
        {
            if( CHEATER_CHEAT_LEVEL( ) )
            {
                if( ( usKeyState & ALT_DOWN ) )  // TODO(port): bit test: check flag type
                    LaptopSaveInfo.fBobbyRSiteCanBeAccessed = true;
                else if( usKeyState & CTRL_DOWN )  // TODO(port): bit test: check flag type
                {
                    guiCurrentLaptopMode = LAPTOP_MODE.BROKEN_LINK;
                }
            }
        }

        else if ((usEvent == KEY_DOWN )&& ( usParam == 'x' ) )
        {
            if( ( usKeyState & ALT_DOWN ) )  // TODO(port): bit test: check flag type
            {
          HandleShortCutExitState( );
            }
               //LeaveLapTopScreen( );
        }
    // #ifdef JA2TESTVERSION
    // else if ((usEvent == KEY_DOWN )&& ( usParam == 'q' ))
    // {
    // //if we dont currently have mercs on the team, hire some
    // if( NumberOfMercsOnPlayerTeam() == 0 )
    // {
    // UINT8 ubRand = (UINT8) Random( 2 ) + 2;
    // TempHiringOfMercs( ubRand, FALSE );
    // //   QuickStartGame( );
    // }
    // ButtonSubSystem.MarkButtonsDirty(buttonList);
    // fExitingLaptopFlag = TRUE;
    // }
    // else if ((usEvent == KEY_DOWN )&& ( usParam == 's' ))
    // {
    // if( ( usKeyState & ALT_DOWN ) )
    // {
    // SetBookMark( AIM_BOOKMARK );
    // SetBookMark( BOBBYR_BOOKMARK );
    // SetBookMark( IMP_BOOKMARK );
    // SetBookMark( MERC_BOOKMARK );
    // SetBookMark( FUNERAL_BOOKMARK );
    // SetBookMark( FLORIST_BOOKMARK );
    // SetBookMark( INSURANCE_BOOKMARK );
    // }
    // }

    // //help screen stuff
    // else
    // #endif
        if( (usEvent == KEY_DOWN ) && ( ( usParam == 'h' ) || ( usParam == 'H' ) ) )
        {
            HelpScreen.ShouldTheHelpScreenComeUp( HELP_SCREEN.LAPTOP, true );
        }


    // #if defined ( JA2TESTVERSION ) || defined ( JA2DEMO )
    // else if ((usEvent == KEY_DOWN )&& ( usParam == 'w' ))
    // {
    // DemoHiringOfMercs( );
    // }
    // #endif

    // #ifdef JA2BETAVERSION
    // //adding all emails
    // else if ((usEvent == KEY_DOWN )&& ( usParam == 'e' ))
    // {
    // if( CHEATER_CHEAT_LEVEL( ) )
    // {
    // if( ( usKeyState & ALT_DOWN ) )
    // {
    // AddAllEmails();
    // }
    // }
    // }
    // #endif

        //adding money
        else if ((usEvent == KEY_DOWN )&& ( usParam == '=' ))
        {
            if( CHEATER_CHEAT_LEVEL( ) )
            {
                AddTransactionToPlayersBook( FinanceEvent.ANONYMOUS_DEPOSIT, 0, GameClock.GetWorldTotalMin(), 100000 );
                ButtonSubSystem.MarkButtonsDirty(buttonList);
            }
        }

        //subtracting money
        else if( (usEvent == KEY_DOWN ) && ( usParam == '-' ) )
        {
            if( CHEATER_CHEAT_LEVEL( ) )
            {
                AddTransactionToPlayersBook( FinanceEvent.ANONYMOUS_DEPOSIT, 0, GameClock.GetWorldTotalMin(), -10000 );
                ButtonSubSystem.MarkButtonsDirty(buttonList);
            }
        }


    // #ifdef JA2TESTVERSION
    // else if ((usEvent == KEY_DOWN )&& ( usParam == 'd' ))
    // {
    // if( gfTemporaryDisablingOfLoadPendingFlag )
    // gfTemporaryDisablingOfLoadPendingFlag = FALSE;
    // else
    // gfTemporaryDisablingOfLoadPendingFlag = TRUE;
    // }
    // else if ((usEvent == KEY_DOWN ) && ( usParam == '+' ))
    // {
    // if( usKeyState & ALT_DOWN )
    // {
    // gStrategicStatus.ubHighestProgress += 10;
    // if (gStrategicStatus.ubHighestProgress > 100)
    // gStrategicStatus.ubHighestProgress = 100;

    // InitAllArmsDealers();
    // InitBobbyRayInventory();
    // }
    // }
    // else if ((usEvent == KEY_DOWN ) && ( usParam == '-' ))
    // {
    // if( usKeyState & ALT_DOWN )
    // {
    // if (gStrategicStatus.ubHighestProgress >= 10)
    // gStrategicStatus.ubHighestProgress -= 10;
    // else
    // gStrategicStatus.ubHighestProgress = 0;

    // InitAllArmsDealers();
    // InitBobbyRayInventory();
    // }
    // }
    // else if ((usEvent == KEY_DOWN ) && ( usParam == '*' ))
    // {
    // if( usKeyState & ALT_DOWN )
    // {
    // DeleteAllStrategicEventsOfType( EVENT_EVALUATE_QUEEN_SITUATION );
    // AdvanceToNextDay();
    // }
    // }
    // else if( usEvent == KEY_DOWN && usParam == 'v' && usKeyState & CTRL_DOWN )
    // {
    // #ifdef SGP_VIDEO_DEBUGGING
    // ScreenMsg( FONT_MCOLOR_LTYELLOW, MSG_INTERFACE, L"VObjects:  %d", guiVObjectSize );
    // ScreenMsg( FONT_MCOLOR_LTYELLOW, MSG_INTERFACE, L"VSurfaces:  %d", guiVSurfaceSize );
    // ScreenMsg( FONT_MCOLOR_LTYELLOW, MSG_INTERFACE, L"SGPVideoDump.txt updated..." );
    // PerformVideoInfoDumpIntoFile( "SGPVideoDump.txt", TRUE );
    // #endif
    // }
    // else
    // {
    // if ((usEvent == KEY_DOWN )&& ( usParam == '1' ))
    // {
    // TempHiringOfMercs( 1, FALSE );
    // }

    // if ((usEvent == KEY_DOWN )&& ( usParam == '2' ))
    // {
    // TempHiringOfMercs( 2, FALSE );
    // }

    // if ((usEvent == KEY_DOWN )&& ( usParam == '3' ))
    // {
    // TempHiringOfMercs( 3, FALSE );
    // }

    // if ((usEvent == KEY_DOWN )&& ( usParam == '4' ))
    // {
    // TempHiringOfMercs( 4, FALSE );
    // }

    // if ((usEvent == KEY_DOWN )&& ( usParam == '5' ))
    // {
    // TempHiringOfMercs( 5, FALSE );
    // }

    // if ((usEvent == KEY_DOWN )&& ( usParam == '6' ))
    // {
    // TempHiringOfMercs( 6, FALSE );
    // }

    // if ((usEvent == KEY_DOWN )&& ( usParam == '7' ))
    // {
    // TempHiringOfMercs( 7, FALSE );
    // }

    // if ((usEvent == KEY_DOWN )&& ( usParam == '8' ))
    // {
    // TempHiringOfMercs( 8, FALSE);
    // }

    // if ((usEvent == KEY_DOWN )&& ( usParam == '9' ))
    // {
    // TempHiringOfMercs( 9, FALSE );
    // }

    // if ((usEvent == KEY_DOWN )&& ( usParam == '0' ))
    // {
    // TempHiringOfMercs( 10, FALSE );
    // }
    // }
    // #endif

            return;
    }

    public void HandleDefaultWebpageForLaptop()
    {

        // go to first page in bookmark list
        if( guiCurrentLaptopMode == LAPTOP_MODE.WWW )
        {
            // if valid entry go there
            if( LaptopSaveInfo.iBookMarkList[ 0 ] != -1 )
            {
              GoToWebPage( LaptopSaveInfo.iBookMarkList[ 0 ] );
            }
        }

        return;
    }

    public bool SaveLaptopInfoToSavedGame(Stream hFile)
    {
        int uiNumBytesWritten=0;
        int uiSize;



        // Save The laptop information
        FileWrite( hFile, &LaptopSaveInfo, sizeof( LaptopSaveInfoStruct ), &uiNumBytesWritten );  // TODO(port): address-of, file io, sizeof
        if( uiNumBytesWritten != sizeof( LaptopSaveInfoStruct ) )  // TODO(port): sizeof
        {
            return(false);
        }



        //If there is anything in the Bobby Ray Orders on Delivery
        if( LaptopSaveInfo.usNumberOfBobbyRayOrderUsed )
        {
            //Allocate memory for the information
            uiSize = sizeof( BobbyRayOrderStruct ) * LaptopSaveInfo.usNumberOfBobbyRayOrderItems;  // TODO(port): sizeof

            // Load The laptop information
            FileWrite( hFile, LaptopSaveInfo.BobbyRayOrdersOnDeliveryArray, uiSize, &uiNumBytesWritten );  // TODO(port): address-of, file io
            if( uiNumBytesWritten != uiSize)
            {
                return(false);
            }
        }


        //If there is any Insurance Payouts in progress
        if( LaptopSaveInfo.ubNumberLifeInsurancePayoutUsed )
        {
            //Allocate memory for the information
            uiSize = sizeof( LIFE_INSURANCE_PAYOUT ) * LaptopSaveInfo.ubNumberLifeInsurancePayouts;  // TODO(port): sizeof

            // Load The laptop information
            FileWrite( hFile, LaptopSaveInfo.pLifeInsurancePayouts, uiSize, &uiNumBytesWritten );  // TODO(port): address-of, file io
            if( uiNumBytesWritten != uiSize )
            {
                return(false);
            }
        }

        return( true );
    }

    public bool LoadLaptopInfoFromSavedGame(Stream hFile)
    {
        int uiNumBytesRead=0;
        int uiSize;


        //if there is memory allocated for the BobbyR orders
        if( LaptopSaveInfo.usNumberOfBobbyRayOrderItems )
        {
    //      if( !LaptopSaveInfo.BobbyRayOrdersOnDeliveryArray )
    //          System.Diagnostics.Debug.Assert( 0 );    //Should never happen

            //Free the memory
            if( LaptopSaveInfo.BobbyRayOrdersOnDeliveryArray )
                MemFree( LaptopSaveInfo.BobbyRayOrdersOnDeliveryArray );  // TODO(port): manual memory
            LaptopSaveInfo.BobbyRayOrdersOnDeliveryArray = null;
        }

        //if there is memory allocated for life insurance payouts
        if( LaptopSaveInfo.ubNumberLifeInsurancePayouts )
        {
            if( !LaptopSaveInfo.pLifeInsurancePayouts )
                System.Diagnostics.Debug.Assert( 0 );    //Should never happen

            //Free the memory
            MemFree( LaptopSaveInfo.pLifeInsurancePayouts );  // TODO(port): manual memory
            LaptopSaveInfo.pLifeInsurancePayouts = null;
        }



        // Load The laptop information
        FileRead( hFile, ref LaptopSaveInfo, sizeof( LaptopSaveInfoStruct ), out uiNumBytesRead );  // TODO(port): file io, sizeof
        if( uiNumBytesRead != sizeof( LaptopSaveInfoStruct ) )  // TODO(port): sizeof
        {
            return(false);
        }



        //If there is anything in the Bobby Ray Orders on Delivery
        if( LaptopSaveInfo.usNumberOfBobbyRayOrderUsed )
        {
            //Allocate memory for the information
            uiSize = sizeof( BobbyRayOrderStruct ) * LaptopSaveInfo.usNumberOfBobbyRayOrderItems;  // TODO(port): sizeof

            LaptopSaveInfo.BobbyRayOrdersOnDeliveryArray = MemAlloc( uiSize );  // TODO(port): manual memory
            System.Diagnostics.Debug.Assert( LaptopSaveInfo.BobbyRayOrdersOnDeliveryArray );

            // Load The laptop information
            FileRead( hFile, LaptopSaveInfo.BobbyRayOrdersOnDeliveryArray, uiSize, out uiNumBytesRead );  // TODO(port): file io
            if( uiNumBytesRead != uiSize )
            {
                return(false);
            }
        }
        else
        {
            LaptopSaveInfo.usNumberOfBobbyRayOrderItems = 0;
            LaptopSaveInfo.BobbyRayOrdersOnDeliveryArray = null;
        }


        //If there is any Insurance Payouts in progress
        if( LaptopSaveInfo.ubNumberLifeInsurancePayoutUsed )
        {
            //Allocate memory for the information
            uiSize = sizeof( LIFE_INSURANCE_PAYOUT ) * LaptopSaveInfo.ubNumberLifeInsurancePayouts;  // TODO(port): sizeof

            LaptopSaveInfo.pLifeInsurancePayouts = MemAlloc( uiSize );  // TODO(port): manual memory
            System.Diagnostics.Debug.Assert( LaptopSaveInfo.pLifeInsurancePayouts );

            // Load The laptop information
            FileRead( hFile, LaptopSaveInfo.pLifeInsurancePayouts, uiSize, out uiNumBytesRead );  // TODO(port): file io
            if( uiNumBytesRead != uiSize )
            {
                return(false);
            }
        }
        else
        {
            LaptopSaveInfo.ubNumberLifeInsurancePayouts = 0;
            LaptopSaveInfo.pLifeInsurancePayouts = null;
        }


        return( true );
    }
    public void WWWRegionMvtCallback(MOUSE_REGION? pRegion, MSYS_CALLBACK_REASON iReason)
    {


        int iDifference=0;
      HVOBJECT hLapTopIconHandle;
        if (iReason.HasFlag(MSYS_CALLBACK_REASON.LOST_MOUSE) )
        {
            WWWRegionMvtCallback_iBaseTime=0;
            WWWRegionMvtCallback_iFrame=0;
        hLapTopIconHandle = guiWWWICON;
          video.BltVideoObject(SurfaceType.FRAME_BUFFER, hLapTopIconHandle, (int)WWWRegionMvtCallback_iFrame,LAPTOP_ICONS_X, LAPTOP_ICONS_WWW_Y, VO_BLT.SRCTRANSPARENCY,null);
          DrawLapTopText();
            HighLightRegion(giCurrentRegion);
            video.InvalidateRegion(0,0,640,480);
        }
    }
    public void FinancialRegionMvtCallback(MOUSE_REGION? pRegion, MSYS_CALLBACK_REASON iReason)
    {


        int iDifference=0;
      HVOBJECT hLapTopIconHandle;
        if (iReason.HasFlag(MSYS_CALLBACK_REASON.LOST_MOUSE) )
        {
            FinancialRegionMvtCallback_iBaseTime=0;
            FinancialRegionMvtCallback_iFrame=0;
        hLapTopIconHandle = guiFINANCIALICON;
          video.BltVideoObject(SurfaceType.FRAME_BUFFER, hLapTopIconHandle, (int)FinancialRegionMvtCallback_iFrame,LAPTOP_ICONS_X-4, LAPTOP_ICONS_FINANCIAL_Y, VO_BLT.SRCTRANSPARENCY,null);
            DrawLapTopText();
            HighLightRegion(giCurrentRegion);
            video.InvalidateRegion(0,0,640,480);
        }
    }
    public void HistoryRegionMvtCallback(MOUSE_REGION? pRegion, MSYS_CALLBACK_REASON iReason)
    {


        int iDifference=0;
      HVOBJECT hLapTopIconHandle;
        if (iReason.HasFlag(MSYS_CALLBACK_REASON.LOST_MOUSE) )
        {
            HistoryRegionMvtCallback_iBaseTime=0;
            HistoryRegionMvtCallback_iFrame=0;

            hLapTopIconHandle = guiHISTORYICON;
          video.BltVideoObject(SurfaceType.FRAME_BUFFER, hLapTopIconHandle, (int)HistoryRegionMvtCallback_iFrame,LAPTOP_ICONS_X, LAPTOP_ICONS_HISTORY_Y, VO_BLT.SRCTRANSPARENCY,null);
        DrawLapTopText();
            HighLightRegion(giCurrentRegion);
            video.InvalidateRegion(0,0,640,480);
        }
    }
    public void FilesRegionMvtCallback(MOUSE_REGION? pRegion, MSYS_CALLBACK_REASON iReason)
    {


        int iDifference=0;
      HVOBJECT hLapTopIconHandle;
        if (iReason.HasFlag(MSYS_CALLBACK_REASON.LOST_MOUSE) )
        {
            FilesRegionMvtCallback_iBaseTime=0;
            FilesRegionMvtCallback_iFrame=0;
        hLapTopIconHandle = guiFILESICON;
          video.BltVideoObject(SurfaceType.FRAME_BUFFER, hLapTopIconHandle, (int)FilesRegionMvtCallback_iFrame,LAPTOP_ICONS_X, LAPTOP_ICONS_FILES_Y+7, VO_BLT.SRCTRANSPARENCY,null);
            DrawLapTopText();
            HighLightRegion(giCurrentRegion);
            video.InvalidateRegion(0,0,640,480);
        }
    }
    public void PersonnelRegionMvtCallback(MOUSE_REGION? pRegion, MSYS_CALLBACK_REASON iReason)
    {


        int iDifference=0;
      HVOBJECT hLapTopIconHandle;
        if (iReason.HasFlag(MSYS_CALLBACK_REASON.LOST_MOUSE) )
        {
            PersonnelRegionMvtCallback_iBaseTime=0;
            PersonnelRegionMvtCallback_iFrame=0;

            hLapTopIconHandle = guiPERSICON;
          video.BltVideoObject(SurfaceType.FRAME_BUFFER, hLapTopIconHandle, (int)PersonnelRegionMvtCallback_iFrame,LAPTOP_ICONS_X, LAPTOP_ICONS_PERSONNEL_Y, VO_BLT.SRCTRANSPARENCY,null);
            DrawLapTopText();
            HighLightRegion(giCurrentRegion);
            video.InvalidateRegion(0,0,640,480);
        }
    }

}
#endif
