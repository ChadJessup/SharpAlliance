using System;
using System.Threading.Tasks;
using SharpAlliance.Core.Interfaces;
using SharpAlliance.Core.Managers;
using SixLabors.ImageSharp;

namespace SharpAlliance.Core.Screens;

public class HelpScreen : IScreen
{
    public bool IsInitialized { get; set; }
    public ScreenState State { get; set; }


    public static bool ShouldTheHelpScreenComeUp(HELP_SCREEN ubScreenID, bool fForceHelpScreenToComeUp)
    {

        //if the screen is being forsced to come up ( user pressed 'h' )
        if (fForceHelpScreenToComeUp)
        {
            //Set thefact that the user broughtthe help screen up
            gHelpScreen.fForceHelpScreenToComeUp = true;

            goto HELP_SCREEN_SHOULD_COME_UP;
        }

        //if we are already in the help system, return true
        if (gHelpScreen.uiFlags.HasFlag(HELP_SCREEN_ACTIVE.Yes))
        {
            return (true);
        }

        //has the player been in the screen before
        if ((gHelpScreen.usHasPlayerSeenHelpScreenInCurrentScreen))// >> ubScreenID) & 0x01)
        {
            goto HELP_SCREEN_WAIT_1_FRAME;
        }

        //if we have already been in the screen, and the user DIDNT press 'h', leave
        if (gHelpScreen.fHaveAlreadyBeenInHelpScreenSinceEnteringCurrenScreen)
        {
            return (false);
        }

        //should the screen come up, based on the users choice for it automatically coming up
        //	if( !( gHelpScreen.fHideHelpInAllScreens ) )
        {
            //		goto HELP_SCREEN_WAIT_1_FRAME;
        }

        //the help screen shouldnt come up
        return (false);

    HELP_SCREEN_WAIT_1_FRAME:

        // we have to wait 1 frame while the screen renders
        if (gHelpScreen.bDelayEnteringHelpScreenBy1FrameCount < 2)
        {
            gHelpScreen.bDelayEnteringHelpScreenBy1FrameCount += 1;

//            ButtonSubSystem.UnmarkButtonsDirty();

            return (false);
        }

    HELP_SCREEN_SHOULD_COME_UP:

        //Record which screen it is

        //if its mapscreen
        if (ubScreenID == HELP_SCREEN.MAPSCREEN)
        {
            //determine which screen it is ( is any mercs hired, did game just start )
            gHelpScreen.bCurrentHelpScreen = HelpScreenDetermineWhichMapScreenHelpToShow();
        }
        else
        {
            gHelpScreen.bCurrentHelpScreen = ubScreenID;
        }

        //mark it that the help screnn is enabled
        gHelpScreen.uiFlags |= HELP_SCREEN_ACTIVE.Yes;

        // reset
        gHelpScreen.bDelayEnteringHelpScreenBy1FrameCount = 0;

        return (true);
    }

    internal static void HelpScreenHandler()
    {
        throw new NotImplementedException();
    }

    private static HELP_SCREEN HelpScreenDetermineWhichMapScreenHelpToShow()
    {
        if (fShowMapInventoryPool)
        {
            return (HELP_SCREEN.MAPSCREEN_SECTOR_INVENTORY);
        }

        if (GameInit.AnyMercsHired() == false)
        {
            return (HELP_SCREEN.MAPSCREEN_NO_ONE_HIRED);
        }

        if (gTacticalStatus.fDidGameJustStart)
        {
            return (HELP_SCREEN.MAPSCREEN_NOT_IN_ARULCO);
        }

        return (HELP_SCREEN.MAPSCREEN);
    }

    public ValueTask Activate()
    {
        return ValueTask.CompletedTask;
    }

    public ValueTask Deactivate()
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
    }

    public void Draw(IVideoManager videoManager)
    {
        throw new NotImplementedException();
    }

    public ValueTask<ScreenName> Handle()
    {
        return ValueTask.FromResult(ScreenName.HelpScreen);
    }

    public ValueTask<bool> Initialize()
    {
        return ValueTask.FromResult(true);
    }
}

//enum used for the different help screens that can come up
public enum HELP_SCREEN
{
    LAPTOP,
    MAPSCREEN,
    MAPSCREEN_NO_ONE_HIRED,
    MAPSCREEN_NOT_IN_ARULCO,
    MAPSCREEN_SECTOR_INVENTORY,
    TACTICAL,
    OPTIONS,
    LOAD_GAME,

    NUMBER_OF_HELP_SCREENS,
};

public enum HELP_SCREEN_ACTIVE
{
    No = 0,
    Yes = 1,
}


public class HELP_SCREEN_STRUCT
{
    public HELP_SCREEN bCurrentHelpScreen;
    public HELP_SCREEN_ACTIVE uiFlags;
    public bool usHasPlayerSeenHelpScreenInCurrentScreen;
    public bool ubHelpScreenDirty;
    public Point usScreenLoc;
    public Size usScreenSize;
    public int iLastMouseClickY;         //last position the mouse was clicked ( if != -1 )
    public int bCurrentHelpScreenActiveSubPage;  //used to keep track of the current page being displayed
    public int bNumberOfButtons;

    //used so if the user checked the box to show the help, it doesnt automatically come up every frame
    public bool fHaveAlreadyBeenInHelpScreenSinceEnteringCurrenScreen;
    public int bDelayEnteringHelpScreenBy1FrameCount;
    public int usLeftMarginPosX;
    public CURSOR usCursor;
    public bool fWasTheGamePausedPriorToEnteringHelpScreen;

    //scroll variables
    public int usTotalNumberOfPixelsInBuffer;
    public int iLineAtTopOfTextBuffer;
    public int usTotalNumberOfLinesInBuffer;
    public bool fForceHelpScreenToComeUp;
}
