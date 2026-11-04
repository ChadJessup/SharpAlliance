using SharpAlliance.Core.Managers;
using SharpAlliance.Core.Screens;

namespace SharpAlliance.Core.SubSystems.LaptopSubSystem;

public partial class Laptop
{
    /// <summary>
    /// Tears the laptop down when the player leaves it (ExitLaptop in laptop.c): closes the current page, removes
    /// the laptop's buttons and mouse regions and lets the game run again.
    /// TODO(port): music / ambient sound / rain restore.
    /// </summary>
    private void ExitLaptop()
    {
        // exit is called due to message box, leave
        if (fExitDueToMessageBox)
        {
            fExitDueToMessageBox = false;
            return;
        }

        // nothing to tear down if the laptop was never entered
        if (!fCurrentlyInLaptop)
        {
            return;
        }

        // release cursor
        CursorSubSystem.FreeMouseCursor();

        // set the fact we are currently not in laptop, for rendering purposes
        fCurrentlyInLaptop = false;

        //Deallocate, save data -- leaving laptop.
        RenderWorld.SetRenderFlags(RenderingFlags.FULL);

        if (fExitDuringLoad == false)
        {
            ExitLaptopMode(guiCurrentLaptopMode);
        }

        fExitDuringLoad = false;
        fLoadPendingFlag = false;

        // destroy region for new mail icon
        CreateDestroyMouseRegionForNewMailIcon();

        if (fErrorFlag)
        {
            fErrorFlag = false;
            CreateDestroyErrorButton();
        }

        if (Emails.fDeleteMailFlag)
        {
            Emails.fDeleteMailFlag = false;
            Emails.CreateDestroyDeleteNoticeMailButton();
        }

        if (Emails.fNewMailFlag)
        {
            Emails.fNewMailFlag = false;
            Emails.CreateDestroyNewMailButton();
        }

        // get rid of minimize button
        CreateDestroyMinimizeButtonForCurrentMode();

        gfEnterLapTop = true;
        DeleteLapTopButtons();
        DeleteLapTopMouseRegions();

        gfShowBookmarks = false;
        CreateDestoryBookMarkRegions();

        fNewWWW = true;
        DeleteBookmark();
        DeleteLoadPending();
        Emails.fReDrawNewMailFlag = false;

        //Since we are going to MapScreen, check the flag
        guiTacticalInterfaceFlags |= INTERFACE.MAPSCREEN;

        //pause the game because we dont want time to advance in the laptop
        GameClock.UnPauseGame();
    }

    private void DeleteLapTopButtons()
    {
        for (int cnt = 0; cnt < gLaptopButtons.Length; cnt++)
        {
            if (gLaptopButtons[cnt] is not null)
            {
                buttonList.Remove(gLaptopButtons[cnt]);
                ButtonSubSystem.RemoveButton(gLaptopButtons[cnt]);
                gLaptopButtons[cnt] = null!;
            }

            if (gLaptopButtonImage[cnt] is not null)
            {
                ButtonSubSystem.UnloadButtonImage(gLaptopButtonImage[cnt]);
                gLaptopButtonImage[cnt] = null!;
            }
        }
    }
}
