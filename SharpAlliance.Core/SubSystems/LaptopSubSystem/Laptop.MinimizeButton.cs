using System;
using System.Collections.Generic;
using SixLabors.ImageSharp;
using static SharpAlliance.Core.EnglishText;
using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core.SubSystems.LaptopSubSystem;

// Port of the laptop's "minimize program" button (the little X at the top right of a running program)
// and the matching click area over the program's icon in the title bar. From laptop.c.
public partial class Laptop
{
    // CreateDestroyMinimizeButtonForCurrentMode keeps track of whether the button exists
    private static bool fMinimizeButtonAlreadyCreated = false;

    private void CreateDestroyMinimizeButtonForCurrentMode()
    {
        // will create the minimize button

        // check to see if created, if so, do nothing

        // check current mode
        if ((guiCurrentLaptopMode == LAPTOP_MODE.NONE) && (guiPreviousLaptopMode != LAPTOP_MODE.NONE))
        {
            fCreateMinimizeButton = false;
        }
        else if (guiCurrentLaptopMode != LAPTOP_MODE.NONE)
        {
            fCreateMinimizeButton = true;
        }
        else if (guiPreviousLaptopMode != LAPTOP_MODE.NONE)
        {
            fCreateMinimizeButton = false;
        }

        // leaving laptop, get rid of the button
        if (fExitingLaptopFlag == true)
        {
            fCreateMinimizeButton = false;
        }

        if ((fMinimizeButtonAlreadyCreated == false) && (fCreateMinimizeButton == true))
        {
            // not created, create
            fMinimizeButtonAlreadyCreated = true;
            CreateMinimizeButtonForCurrentMode();
            CreateMinimizeRegionsForLaptopProgramIcons();
        }
        else if ((fMinimizeButtonAlreadyCreated == true) && (fCreateMinimizeButton == false))
        {
            // created and must be destroyed
            fMinimizeButtonAlreadyCreated = false;
            DestroyMinimizeButtonForCurrentMode();
            DestroyMinimizeRegionsForLaptopProgramIcons();
        }
        else
        {
            // do nothing
        }
    }

    private void CreateMinimizeButtonForCurrentMode()
    {
        // create minimize button
        gLaptopMinButtonImage[0] = ButtonSubSystem.LoadButtonImage("LAPTOP\\x.sti", -1, 0, -1, 1, -1);
        gLaptopMinButton[0] = ButtonSubSystem.QuickCreateButton(
            gLaptopMinButtonImage[0],
            new Point(590, 30),
            ButtonFlags.BUTTON_TOGGLE,
            MSYS_PRIORITY.HIGH,
            BtnGenericMouseMoveButtonCallback,
            LaptopMinimizeProgramButtonCallback);

        ButtonSubSystem.SetButtonCursor(gLaptopMinButton[0], CURSOR.LAPTOP_SCREEN);

        buttonList.Add(gLaptopMinButton[0]);
    }

    private void DestroyMinimizeButtonForCurrentMode()
    {
        // destroy minimize button
        buttonList.Remove(gLaptopMinButton[0]);
        ButtonSubSystem.RemoveButton(gLaptopMinButton[0]);
        ButtonSubSystem.UnloadButtonImage(gLaptopMinButtonImage[0]);
    }

    private void LaptopMinimizeProgramButtonCallback(ref GUI_BUTTON btn, MSYS_CALLBACK_REASON reason)
    {
        if (!btn.uiFlags.HasFlag(ButtonFlags.BUTTON_ENABLED))
        {
            return;
        }

        if (reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_DWN))
        {
            if (!btn.uiFlags.HasFlag(ButtonFlags.BUTTON_CLICKED_ON))
            {
                btn.uiFlags |= ButtonFlags.BUTTON_CLICKED_ON;
            }
        }
        else if (reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP))
        {
            if (btn.uiFlags.HasFlag(ButtonFlags.BUTTON_CLICKED_ON))
            {
                btn.uiFlags &= ~ButtonFlags.BUTTON_CLICKED_ON;

                MinimizeCurrentProgram(pLaptopIcons[7], fHideBookmarkInfo: false);
            }
        }
    }

    private void CreateMinimizeRegionsForLaptopProgramIcons()
    {
        // will create the minizing region to lie over the icon for this particular laptop program
        MouseSubSystem.MSYS_DefineRegion(
            gLapTopProgramMinIcon,
            LAPTOP_PROGRAM_ICON_X,
            LAPTOP_PROGRAM_ICON_Y,
            LAPTOP_PROGRAM_ICON_X + LAPTOP_PROGRAM_ICON_WIDTH,
            LAPTOP_PROGRAM_ICON_Y + LAPTOP_PROGRAM_ICON_HEIGHT,
            MSYS_PRIORITY.NORMAL + 1,
            CURSOR.LAPTOP_SCREEN,
            MSYS_NO_CALLBACK,
            LaptopProgramIconMinimizeCallback);
    }

    private void LaptopProgramIconMinimizeCallback(ref MOUSE_REGION pRegion, MSYS_CALLBACK_REASON iReason)
    {
        // callback handler for the minize region that is attatched to the laptop program icon
        if (iReason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP))
        {
            MinimizeCurrentProgram(pWebTitle[0], fHideBookmarkInfo: true);
        }
    }

    /// <summary>
    /// What both the minimize button and the title-bar icon do: mark the program that is on screen as
    /// minimized, start the title bar animation, and fall back to the program opened before it.
    /// </summary>
    /// <param name="webTitle">Title shown while a web page (any other mode) is minimized.</param>
    /// <param name="fHideBookmarkInfo">The icon callback also hides the bookmark tooltip.</param>
    private void MinimizeCurrentProgram(string webTitle, bool fHideBookmarkInfo)
    {
        LAPTOP_PROGRAM program;
        string title;
        int iconIndex;

        switch (guiCurrentLaptopMode)
        {
            case LAPTOP_MODE.EMAIL:
                program = LAPTOP_PROGRAM.MAILER;
                title = pLaptopIcons[0];
                iconIndex = 0;
                break;
            case LAPTOP_MODE.FILES:
                program = LAPTOP_PROGRAM.FILES;
                title = pLaptopIcons[5];
                iconIndex = 2;
                break;
            case LAPTOP_MODE.FINANCES:
                program = LAPTOP_PROGRAM.FINANCES;
                title = pLaptopIcons[2];
                iconIndex = 5;
                break;
            case LAPTOP_MODE.HISTORY:
                program = LAPTOP_PROGRAM.HISTORY;
                title = pLaptopIcons[4];
                iconIndex = 4;
                break;
            case LAPTOP_MODE.PERSONNEL:
                program = LAPTOP_PROGRAM.PERSONNEL;
                title = pLaptopIcons[3];
                iconIndex = 3;
                break;
            case LAPTOP_MODE.NONE:
                // nothing
                return;
            default:
                program = LAPTOP_PROGRAM.WEB_BROWSER;
                title = webTitle;
                iconIndex = 1;
                break;
        }

        gLaptopProgramStates[program] = LAPTOP_PROGRAM_STATES.MINIMIZED;
        InitTitleBarMaximizeGraphics(guiTITLEBARLAPTOP, title, guiTITLEBARICONS, iconIndex);
        SetCurrentToLastProgramOpened();

        if (program == LAPTOP_PROGRAM.WEB_BROWSER)
        {
            gfShowBookmarks = false;
            if (fHideBookmarkInfo)
            {
                fShowBookmarkInfo = false;
            }
        }

        fMinizingProgram = true;
        fInitTitle = true;
    }

    /// <summary>Id of the most recently opened program that is not minimized, or UNSET when all are.</summary>
    private static LAPTOP_PROGRAM FindLastProgramStillOpen()
    {
        int iLowestValue = 6;
        LAPTOP_PROGRAM iLowestValueProgram = LAPTOP_PROGRAM.UNSET;

        // returns ID of last program open and not minimized
        for (LAPTOP_PROGRAM iCounter = 0; iCounter <= LAPTOP_PROGRAM.HISTORY; iCounter++)
        {
            bool minimized = gLaptopProgramStates.GetValueOrDefault(iCounter, LAPTOP_PROGRAM_STATES.MINIMIZED) == LAPTOP_PROGRAM_STATES.MINIMIZED;
            if (!minimized)
            {
                int queuePosition = gLaptopProgramQueueList.GetValueOrDefault(iCounter, 6);
                if (queuePosition < iLowestValue)
                {
                    iLowestValue = queuePosition;
                    iLowestValueProgram = iCounter;
                }
            }
        }

        return iLowestValueProgram;
    }

    private static void SetCurrentToLastProgramOpened()
    {
        guiCurrentLaptopMode = LAPTOP_MODE.NONE;

        switch (FindLastProgramStillOpen())
        {
            case LAPTOP_PROGRAM.HISTORY:
                guiCurrentLaptopMode = LAPTOP_MODE.HISTORY;
                break;
            case LAPTOP_PROGRAM.MAILER:
                guiCurrentLaptopMode = LAPTOP_MODE.EMAIL;
                break;
            case LAPTOP_PROGRAM.PERSONNEL:
                guiCurrentLaptopMode = LAPTOP_MODE.PERSONNEL;
                break;
            case LAPTOP_PROGRAM.FINANCES:
                guiCurrentLaptopMode = LAPTOP_MODE.FINANCES;
                break;
            case LAPTOP_PROGRAM.FILES:
                guiCurrentLaptopMode = LAPTOP_MODE.FILES;
                break;
            case LAPTOP_PROGRAM.WEB_BROWSER:
                // last www mode
                if (guiCurrentWWWMode != 0)
                {
                    guiCurrentLaptopMode = guiCurrentWWWMode;
                }
                else
                {
                    guiCurrentLaptopMode = LAPTOP_MODE.WWW;
                }

                //gfShowBookmarks = TRUE;
                fShowBookmarkInfo = true;
                break;
        }
    }
}
