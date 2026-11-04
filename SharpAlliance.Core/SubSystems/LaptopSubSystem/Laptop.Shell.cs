using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SharpAlliance.Core.Screens;
using SharpAlliance.Core.Managers;
using SharpAlliance.Core.Managers.VideoSurfaces;
using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core.SubSystems.LaptopSubSystem;

// Ported from laptop.c: the parts of the laptop shell that were still missing (mouse/keyboard handling,
// minimizing programs, bookmarks, saving and loading laptop state).
public partial class Laptop
{

    public int RenderLaptopPanel()
    {

        return 0;
    }

    public bool DeleteLapTopMouseRegions()
    {

     MouseSubSystem.MSYS_RemoveRegion( gLapTopScreenRegion );

     return (true);
    }

    public void HandleLapTopScreenMouseUi()
    {
     if ( MouseRegionFlags.IN_AREA.HasFlag(gEmailRegion.uiFlags) )
     {
      giHighLightRegion=LaptopRegions.EMAIL_REGION;
     }
     else if ( MouseRegionFlags.IN_AREA.HasFlag(gPersonnelRegion.uiFlags) )
     {
      giHighLightRegion=LaptopRegions.PERSONNEL_REGION;
     }
     else if ( MouseRegionFlags.IN_AREA.HasFlag(gFinancialRegion.uiFlags) )
     {
      giHighLightRegion=LaptopRegions.FINANCIAL_REGION;
     }
     else if ( MouseRegionFlags.IN_AREA.HasFlag(gWWWRegion.uiFlags) )
     {
      giHighLightRegion=LaptopRegions.WWW_REGION;
     }
     else if ( MouseRegionFlags.IN_AREA.HasFlag(gFilesRegion.uiFlags) )
     {
      giHighLightRegion=LaptopRegions.FILES_REGION;
     }
     else if ( MouseRegionFlags.IN_AREA.HasFlag(gHistoryRegion.uiFlags) )
     {
      giHighLightRegion=LaptopRegions.HISTORY_REGION;
     }
     else
      giHighLightRegion=LaptopRegions.NO_REGION;
     DrawHighLightRegionBox();
    }

    public void DrawHighLightRegionBox()
    {

        return;
    }

    public void HandleAnimatedButtons()
    {

        return;


    }

    public void AnimateButton(int uiIconID, int usX, int usY)
    {
      return;
    }

    // was a function-local static of WWWRegionMvtCallback
    private static int WWWRegionMvtCallback_iBaseTime =0;
    // was a function-local static of WWWRegionMvtCallback
    private static int WWWRegionMvtCallback_iFrame =0;

    // was a function-local static of EmailRegionMvtCallback
    private static int EmailRegionMvtCallback_iBaseTime =0;
    // was a function-local static of EmailRegionMvtCallback
    private static int EmailRegionMvtCallback_iFrame =0;

    // was a function-local static of FinancialRegionMvtCallback
    private static int FinancialRegionMvtCallback_iBaseTime =0;
    // was a function-local static of FinancialRegionMvtCallback
    private static int FinancialRegionMvtCallback_iFrame =0;

    // was a function-local static of HistoryRegionMvtCallback
    private static int HistoryRegionMvtCallback_iBaseTime =0;
    // was a function-local static of HistoryRegionMvtCallback
    private static int HistoryRegionMvtCallback_iFrame =0;

    // was a function-local static of FilesRegionMvtCallback
    private static int FilesRegionMvtCallback_iBaseTime =0;
    // was a function-local static of FilesRegionMvtCallback
    private static int FilesRegionMvtCallback_iFrame =0;

    // was a function-local static of PersonnelRegionMvtCallback
    private static int PersonnelRegionMvtCallback_iBaseTime =0;
    // was a function-local static of PersonnelRegionMvtCallback
    private static int PersonnelRegionMvtCallback_iFrame =0;

    public void ReDrawHighLight()
    {
      HighLightRegion(giCurrentRegion);
        return;
    }

    public void DeleteBookmark()
    {
      video.DeleteVideoObject(guiBOOKTOP);
        video.DeleteVideoObject(guiBOOKMID);
        video.DeleteVideoObject(guiBOOKHIGH);
        video.DeleteVideoObject(guiBOOKBOT);
      video.DeleteVideoObject(guiBOOKMARK);

        video.DeleteVideoObject(guiDOWNLOADTOP);
        video.DeleteVideoObject(guiDOWNLOADMID);
        video.DeleteVideoObject(guiDOWNLOADBOT);


    }

    // was a function-local static of ScrollDisplayText
    private static int ScrollDisplayText_iBaseTime =0;
    // was a function-local static of ScrollDisplayText
    private static int ScrollDisplayText_sCurX;

    public void DeleteLoadPending()
    {
        // this funtion will delete the load pending graphics
        // reuse bookmark
      video.DeleteVideoObject(guiGRAPHBAR);
      video.DeleteVideoObject(guiGRAPHWINDOW);
        return;
    }

    public void HandleLapTopESCKey()
    {

        // will handle esc key events, since handling depends on state of laptop


        if( Emails.fNewMailFlag )
        {
            // get rid of new mail warning box
          Emails.fNewMailFlag=false;
        Emails.CreateDestroyNewMailButton();

            // force redraw
          fReDrawScreenFlag = true;
          RenderLaptop( );
        }
        else if(Emails.fDeleteMailFlag)
        {
        // get rid of delete mail box
          Emails.fDeleteMailFlag=false;
          Emails.CreateDestroyDeleteNoticeMailButton();

            // force redraw
          fReDrawScreenFlag = true;
          RenderLaptop( );
        }
        else if( fErrorFlag )
        {
            // get rid of error warning box
        fErrorFlag=false;
          CreateDestroyErrorButton();

            // force redraw
          fReDrawScreenFlag = true;
          RenderLaptop( );
        }

        else if( gfShowBookmarks )
        {
            // get rid of bookmarks
            gfShowBookmarks = false;

            // force redraw
          fReDrawScreenFlag = true;
            RenderLapTopImage( );
          RenderLaptop( );
        }
        else
        {
        // leave
            fExitingLaptopFlag = true;
            HandleExit( );
        }



        return;
    }

    public void HandleRightButtonUpEvent()
    {

        // will handle the right button up event
        if( Emails.fNewMailFlag )
        {
            // get rid of new mail warning box
          Emails.fNewMailFlag=false;
        Emails.CreateDestroyNewMailButton();

            // force redraw
          fReDrawScreenFlag = true;
          RenderLaptop( );
        }
        else if(Emails.fDeleteMailFlag)
        {
        // get rid of delete mail box
          Emails.fDeleteMailFlag=false;
          Emails.CreateDestroyDeleteNoticeMailButton();

            // force redraw
          fReDrawScreenFlag = true;
          RenderLaptop( );
        }
        else if( fErrorFlag )
        {
            // get rid of error warning box
        fErrorFlag=false;
          CreateDestroyErrorButton();

            // force redraw
          fReDrawScreenFlag = true;
          RenderLaptop( );
        }

        else if( gfShowBookmarks )
        {
            // get rid of bookmarks
            gfShowBookmarks = false;

            // force redraw
          fReDrawScreenFlag = true;
            RenderLapTopImage( );
          RenderLaptop( );
        }
        else if( fDisplayMessageFlag )
        {
            fDisplayMessageFlag = false;

                // force redraw
          fReDrawScreenFlag = true;
            RenderLapTopImage( );
          RenderLaptop( );

        }
        else if( fShowBookmarkInfo )
        {
            fShowBookmarkInfo = false;
        }

    }

    public void DestroyMinimizeRegionsForLaptopProgramIcons()
    {
        // will destroy the minizmize regions to be placed over the laptop icons that will be
        // displayed on the top of the laptop program bar

        MouseSubSystem.MSYS_RemoveRegion( gLapTopProgramMinIcon );

        return;
    }

    public void HandleShiftAltTabKeyInLaptop()
    {
        // will handle the alt tab keying in laptop

        // move to next program
        if( fMaximizingProgram == true )
        {
            return;
        }

        switch( guiCurrentLaptopMode )
        {
            case( LAPTOP_MODE.FINANCES ):
                guiCurrentLaptopMode = LAPTOP_MODE.PERSONNEL;
            break;
            case( LAPTOP_MODE.PERSONNEL ):
                guiCurrentLaptopMode = LAPTOP_MODE.HISTORY;
            break;
            case( LAPTOP_MODE.HISTORY ):
                guiCurrentLaptopMode = LAPTOP_MODE.FILES;
            break;
            case( LAPTOP_MODE.EMAIL ):
                guiCurrentLaptopMode = LAPTOP_MODE.FINANCES;
            break;
            case( LAPTOP_MODE.FILES ):
                guiCurrentLaptopMode = LAPTOP_MODE.WWW;
            break;
            case( LAPTOP_MODE.NONE ):
                guiCurrentLaptopMode = LAPTOP_MODE.FINANCES;
            break;
            case( LAPTOP_MODE.WWW ):
                guiCurrentLaptopMode = LAPTOP_MODE.EMAIL;
            break;
            default:
                guiCurrentLaptopMode = LAPTOP_MODE.EMAIL;
            break;
        }

        fPausedReDrawScreenFlag = true;
    }

    public void HandleAltTabKeyInLaptop()
    {
        // will handle the alt tab keying in laptop

        // move to next program
        // move to next program
        if( fMaximizingProgram == true )
        {
            return;
        }

        switch( guiCurrentLaptopMode )
        {
            case( LAPTOP_MODE.FINANCES ):
                guiCurrentLaptopMode = LAPTOP_MODE.EMAIL;
            break;
            case( LAPTOP_MODE.PERSONNEL ):
                guiCurrentLaptopMode = LAPTOP_MODE.FINANCES;
            break;

            case( LAPTOP_MODE.HISTORY ):
                guiCurrentLaptopMode = LAPTOP_MODE.PERSONNEL;
            break;
            case( LAPTOP_MODE.EMAIL ):
                guiCurrentLaptopMode = LAPTOP_MODE.WWW;
            break;
            case( LAPTOP_MODE.FILES ):
                guiCurrentLaptopMode = LAPTOP_MODE.HISTORY;
            break;
            case( LAPTOP_MODE.NONE ):
                guiCurrentLaptopMode = LAPTOP_MODE.EMAIL;
            break;
            default:
                guiCurrentLaptopMode = LAPTOP_MODE.FILES;
            break;
        }

        fPausedReDrawScreenFlag = true;
    }

    public void ClearOutTempLaptopFiles()
    {
        // clear out all temp files from laptop

        // file file
        if (  (files.FileExists( "files.dat" ) == true ) )
        {
             files.FileClearAttributes( "files.dat" );
             files.FileDelete( "files.dat" );
        }

        // finances
        if (  (files.FileExists( "finances.dat" ) == true ) )
        {
             files.FileClearAttributes( "finances.dat" );
             files.FileDelete( "finances.dat" );
        }

        // email
        if (  (files.FileExists( "email.dat" ) == true ) )
        {
             files.FileClearAttributes( "email.dat" );
             files.FileDelete( "email.dat" );
        }

        // history
        if (  (files.FileExists( "history.dat" ) == true ) )
        {
             files.FileClearAttributes( "history.dat" );
             files.FileDelete( "history.dat" );
        }

    }

    public void LaptopSaveVariablesInit()
    {
    }
}
