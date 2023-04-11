using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SharpAlliance.Core.Interfaces;
using SharpAlliance.Core.Managers;
using SharpAlliance.Core.Screens;
using SharpAlliance.Platform;
using SharpAlliance.Platform.Interfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Veldrid;
using Point = SixLabors.ImageSharp.Point;
using Rectangle = SixLabors.ImageSharp.Rectangle;
using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core.SubSystems;

public delegate void ButtonCallback(ref GUI_BUTTON btn, MSYS_CALLBACK_REASON reason);
public class MouseSubSystem : ISharpAllianceManager
{
    public static GUI_CALLBACK DefaultMoveCallback { get; private set; }
    private readonly ILogger<MouseSubSystem> logger;
    private readonly IClockManager clock;
    private static CursorSubSystem cursors;
    private static GameContext gameContext;
    private const int MSYS_DOUBLECLICK_DELAY = 400;

    private static Point CurrentCoord = new(0, 0);

    private static ButtonMasks CurrentButtons;
    private static MouseDos MouseAction = 0;

    private static bool MouseSystemInitialized = false;
    private static bool UseMouseHandlerHook = false;

    private static bool WasMouseGrabbed = false;
    private static MOUSE_REGION? GrabbedRegion = null;

    private static MOUSE_REGION? RegionList = null;

    public static MOUSE_REGION? PreviousRegion = null;
    public static MOUSE_REGION? CurrentRegion = null;

    MOUSE_REGION SystemBaseRegion = new(nameof(SystemBaseRegion))
    {
        IdNumber = MSYS_ID.SYSTEM,
        PriorityLevel = MSYS_PRIORITY.SYSTEM,
        IsEnabled = true,
        uiFlags = MouseRegionFlags.BASE_REGION_FLAGS,
        Bounds = new()
        {
            X = -32767,
            Y = -32767,
            Height = 32767,
            Width = 32767,
        },
        MousePos = new(0, 0),
        RelativeMousePos = new(0, 0),
        ButtonState = ButtonMasks.None,
        Cursor = CURSOR.None,
        MovementCallback = null,
        ButtonCallback = null,
        UserData = new object[] { 0, 0, 0, 0 },
        FastHelpTimer = 0,
        FastHelpText = string.Empty,
        FastHelpRect = -1,
        HelpDoneCallback = null,// MouseCallbackReasons.NO_CALLBACK, 
    };

    public static List<MOUSE_REGION> Regions { get; set; } = new(100);
    public bool IsInitialized { get; }

    public MouseSubSystem(
        ILogger<MouseSubSystem> logger,
        GameContext gameContext,
        IClockManager clockManager,
        CursorSubSystem cursorSubSystem)
    {
        logger = logger;

        DefaultMoveCallback = BtnGenericMouseMoveButtonCallback;
        logger.LogDebug(LoggingEventId.MouseSystem, "Mouse Region System");
        this.clock = clockManager;
        cursors = cursorSubSystem;
        gameContext = gameContext;

        if (RegionList is not null)
        {
            ClearRegionList();
        }

        CurrentCoord = new(0, 0);
        CurrentButtons = 0;
        MouseAction = MouseDos.NO_ACTION;

        PreviousRegion = null;
        MouseSystemInitialized = true;
        UseMouseHandlerHook = false;

        WasMouseGrabbed = false;
        GrabbedRegion = null;

        // Setup the system's background region
        SystemBaseRegion.IdNumber = MSYS_ID.SYSTEM;
        SystemBaseRegion.PriorityLevel = MSYS_PRIORITY.SYSTEM;
        SystemBaseRegion.uiFlags = MouseRegionFlags.BASE_REGION_FLAGS;
        SystemBaseRegion.Bounds = new(-32767, -32767, 32767, 32767);

        SystemBaseRegion.MousePos = new(0, 0);
        SystemBaseRegion.RelativeMousePos = new(0, 0);

        SystemBaseRegion.ButtonState = 0;
        SystemBaseRegion.Cursor = 0;
        SystemBaseRegion.UserData[0] = 0;
        SystemBaseRegion.UserData[1] = 0;
        SystemBaseRegion.UserData[2] = 0;
        SystemBaseRegion.UserData[3] = 0;
        SystemBaseRegion.MovementCallback = null;
        SystemBaseRegion.ButtonCallback = null;

        SystemBaseRegion.FastHelpTimer = 0;
        SystemBaseRegion.FastHelpText = string.Empty;

        SystemBaseRegion.FastHelpRect = -1;

        AddRegionToList(SystemBaseRegion);

        UseMouseHandlerHook = true;
    }

    //Generic Button Movement Callback to reset the mouse button if the mouse is no longer
    //in the button region.
    public static void BtnGenericMouseMoveButtonCallback(ref GUI_BUTTON btn, MSYS_CALLBACK_REASON reasonValue)
    {
        MSYS_CALLBACK_REASON reason = reasonValue;

        //If the button isn't the anchored button, then we don't want to modify the button state.
        if (btn != Globals.gpAnchoredButton)
        {
            return;
        }

        if (reason.HasFlag(MSYS_CALLBACK_REASON.LOST_MOUSE))
        {
            if (!Globals.gfAnchoredState)
            {
                btn.uiFlags &= ~ButtonFlags.BUTTON_CLICKED_ON;
                if (btn.ubSoundSchemeID != 0)
                {
                    ButtonSubSystem.PlayButtonSound(btn, ButtonSounds.BUTTON_SOUND_CLICKED_OFF);
                }
            }
        }
        else if (reason.HasFlag(MSYS_CALLBACK_REASON.GAIN_MOUSE))
        {
            btn.uiFlags |= ButtonFlags.BUTTON_CLICKED_ON;
            if (btn.ubSoundSchemeID != 0)
            {
                ButtonSubSystem.PlayButtonSound(btn, ButtonSounds.BUTTON_SOUND_CLICKED_ON);
            }
        }
    }

    public static void MouseHook(MouseEvents mouseEvent, Point coord, bool leftButtonDown, bool rightButtonDown)
    {
        // If the mouse system isn't initialized, get out o' here
        if (!MouseSystemInitialized)
        {
            return;
        }

        // If we're not using the handler stuff, ignore this call
        if (!UseMouseHandlerHook)
        {
            return;
        }

        coord.Y = 480 - coord.Y;

        if (coord != CurrentCoord)
        {
            // Console.WriteLine($"Mouse: {coord} L: {leftButtonDown} R: {rightButtonDown}");
        }


        MouseAction = MouseDos.NO_ACTION;
        switch (mouseEvent)
        {
            case MouseEvents.LEFT_BUTTON_DOWN:
            case MouseEvents.LEFT_BUTTON_UP:
            case MouseEvents.RIGHT_BUTTON_DOWN:
            case MouseEvents.RIGHT_BUTTON_UP:
                //MSYS_Action|=MouseDos.BUTTONS;
                if (mouseEvent == MouseEvents.LEFT_BUTTON_DOWN)
                {
                    MouseAction |= MouseDos.LBUTTON_DWN;
                }
                else if (mouseEvent == MouseEvents.LEFT_BUTTON_UP)
                {
                    MouseAction |= MouseDos.LBUTTON_UP;
                    //Kris:
                    //Used only if applicable.  This is used for that special button that is locked with the
                    //mouse press -- just like windows.  When you release the button, the previous state
                    //of the button is restored if you released the mouse outside of it's boundaries.  If
                    //you release inside of the button, the action is selected -- but later in the code.
                    //NOTE:  It has to be here, because the mouse can be released anywhere regardless of
                    //regions, buttons, etc.

                    ButtonSubSystem.ReleaseAnchorMode(coord);
                }
                else if (mouseEvent == MouseEvents.RIGHT_BUTTON_DOWN)
                {
                    MouseAction |= MouseDos.RBUTTON_DWN;
                }
                else if (mouseEvent == MouseEvents.RIGHT_BUTTON_UP)
                {
                    MouseAction |= MouseDos.RBUTTON_UP;
                }

                if (leftButtonDown)
                {
                    CurrentButtons |= ButtonMasks.MSYS_LEFT_BUTTON;
                }
                else
                {
                    CurrentButtons &= ~ButtonMasks.MSYS_LEFT_BUTTON;
                }

                if (rightButtonDown)
                {
                    CurrentButtons |= ButtonMasks.MSYS_RIGHT_BUTTON;
                }
                else
                {
                    CurrentButtons &= ~ButtonMasks.MSYS_RIGHT_BUTTON;
                }

                if (coord != CurrentCoord)
                {
                    MouseAction |= MouseDos.MOVE;
                    CurrentCoord = coord;
                }

                UpdateMouseRegion();
                break;

            // ATE: Checks here for mouse button repeats.....
            // Call mouse region with new reason
            case MouseEvents.LEFT_BUTTON_REPEAT:
            case MouseEvents.RIGHT_BUTTON_REPEAT:

                if (mouseEvent == MouseEvents.LEFT_BUTTON_REPEAT)
                {
                    MouseAction |= MouseDos.LBUTTON_REPEAT;
                }
                else if (mouseEvent == MouseEvents.RIGHT_BUTTON_REPEAT)
                {
                    MouseAction |= MouseDos.RBUTTON_REPEAT;
                }

                if (coord != CurrentCoord)
                {
                    MouseAction |= MouseDos.MOVE;
                    CurrentCoord = coord;
                }

                UpdateMouseRegion();
                break;

            case MouseEvents.MousePosition:
                if (coord != CurrentCoord || Globals.gfRefreshUpdate)
                {
                    MouseAction |= MouseDos.MOVE;
                    CurrentCoord = coord;

                    Globals.gfRefreshUpdate = false;

                    UpdateMouseRegion();
                }
                break;

            default:
                //DbgMessage(TOPIC_MOUSE_SYSTEM, DBG_LEVEL_0, "ERROR -- MSYS 2 SGP Mouse Hook got bad type");
                break;
        }
    }


    //======================================================================================================
    //	UpdateMouseRegion
    //
    //	Searches the list for the highest priority region and updates it's info. It also dispatches
    //	the callback functions
    //
    private static void UpdateMouseRegion()
    {
        int ButtonReason;

        // Check previous region!
        if (WasMouseGrabbed)
        {
            CurrentRegion = GrabbedRegion;
        }

        var foundRegions = Regions.Where(r =>
            r.IsEnabled
            && r.Bounds.Contains(CurrentCoord));

        foundRegions = foundRegions
            .OrderByDescending(r => r.PriorityLevel)
            .ThenByDescending(r => r.IdNumber);

        if (!foundRegions.Any())
        {
            return;
        }

        CurrentRegion = foundRegions.First();

        if (CurrentRegion != PreviousRegion)
        {
            Console.WriteLine($"In MouseRegion: {CurrentRegion}");
        }

        if (PreviousRegion is not null)
        {
            PreviousRegion.HasMouse = false;

            if (PreviousRegion != CurrentRegion)
            {
                //Remove the help text for the previous region if one is currently being displayed.
                if (PreviousRegion.FastHelpText is not null)
                {
                    //ExecuteMouseHelpEndCallBack( MSYS_PrevRegion );

                    PreviousRegion.uiFlags &= ~MouseRegionFlags.GOT_BACKGROUND;
                    PreviousRegion.uiFlags &= ~MouseRegionFlags.FASTHELP_RESET;
                }

                CurrentRegion.FastHelpTimer = Globals.gsFastHelpDelay;

                // Force a callbacks to happen on previous region to indicate that
                // the mouse has left the old region
                if (PreviousRegion.HasMoveCallback
                    && PreviousRegion.IsEnabled)
                {
                    PreviousRegion.MovementCallback?.Invoke(ref PreviousRegion, MSYS_CALLBACK_REASON.LOST_MOUSE);
                }
            }
        }

        if (CurrentRegion != PreviousRegion)
        {
            //Kris -- October 27, 1997
            //Implemented gain mouse region

            if (CurrentRegion.HasMoveCallback)
            {
                if (CurrentRegion.FastHelpText is not null
                    && !CurrentRegion.uiFlags.HasFlag(MouseRegionFlags.FASTHELP_RESET))
                {
                    //ExecuteMouseHelpEndCallBack( MSYS_CurrRegion );
                    CurrentRegion.FastHelpTimer = Globals.gsFastHelpDelay;
                    CurrentRegion.uiFlags &= ~MouseRegionFlags.GOT_BACKGROUND;
                    CurrentRegion.uiFlags |= MouseRegionFlags.FASTHELP_RESET;
                }

                if (CurrentRegion.IsEnabled)
                {
                    CurrentRegion.MovementCallback?.Invoke(ref CurrentRegion, MSYS_CALLBACK_REASON.GAIN_MOUSE);
                }
            }

            // if the cursor is set and is not set to no cursor
            if (CurrentRegion.IsEnabled
                && CurrentRegion.uiFlags.HasFlag(MouseRegionFlags.SET_CURSOR)
                && CurrentRegion.Cursor != 0)
            {
                SetCurrentCursor(CurrentRegion.Cursor);
            }
            else
            {
                // Addition Oct 10/1997 Carter, patch for mouse cursor
                // start at region and find another region encompassing
                var regionWithCursor = foundRegions.FirstOrDefault(r =>
                    r.IsEnabled
                    && r.uiFlags.HasFlag(MouseRegionFlags.SET_CURSOR));

                if (regionWithCursor is not null)
                {
                    SetCurrentCursor(regionWithCursor.Cursor);
                }
            }
        }

        // OK, if we do not have a button down, any button is game!
        if (!Globals.gfClickedModeOn
            || (Globals.gfClickedModeOn && Globals.gusClickedIDNumber == CurrentRegion.IdNumber))
        {
            CurrentRegion.HasMouse = true;

            CurrentRegion.MousePos = CurrentCoord;

            CurrentRegion.RelativeMousePos = new(
                CurrentCoord.X - CurrentRegion.Bounds.X,
                CurrentCoord.Y - CurrentRegion.Bounds.Y);

            CurrentRegion.ButtonState = CurrentButtons;

            if (CurrentRegion.IsEnabled
                && CurrentRegion.HasMoveCallback
                && MouseAction.HasFlag(MouseDos.MOVE))
            {
                IVideoManager.DebugRenderer.DrawRectangle(CurrentRegion.Bounds, Color.Yellow);
                CurrentRegion.MovementCallback?.Invoke(ref CurrentRegion, MSYS_CALLBACK_REASON.MOVE);
            }

            //ExecuteMouseHelpEndCallBack( MSYS_CurrRegion );
            //MSYS_CurrRegion.FastHelpTimer = gsFastHelpDelay;

            MouseAction &= ~MouseDos.MOVE;

            if (CurrentRegion.HasButtonCallback
                && (MouseAction & MouseDos.BUTTONS) != 0)
            {
                if (CurrentRegion.IsEnabled)
                {
                    ButtonReason = (int)MSYS_CALLBACK_REASON.NONE;
                    if (MouseAction.HasFlag(MouseDos.LBUTTON_DWN))
                    {
                        ButtonReason |= (int)MSYS_CALLBACK_REASON.LBUTTON_DWN;
                        Globals.gfClickedModeOn = true;
                        // Set global ID
                        Globals.gusClickedIDNumber = CurrentRegion.IdNumber;
                    }

                    if (MouseAction.HasFlag(MouseDos.LBUTTON_UP))
                    {
                        ButtonReason |= (int)MSYS_CALLBACK_REASON.LBUTTON_UP;
                        Globals.gfClickedModeOn = false;
                    }

                    if (MouseAction.HasFlag(MouseDos.RBUTTON_DWN))
                    {
                        ButtonReason |= (int)MSYS_CALLBACK_REASON.RBUTTON_DWN;
                        Globals.gfClickedModeOn = true;
                        // Set global ID
                        Globals.gusClickedIDNumber = CurrentRegion.IdNumber;
                    }

                    if (MouseAction.HasFlag(MouseDos.RBUTTON_UP))
                    {
                        ButtonReason |= (int)MSYS_CALLBACK_REASON.RBUTTON_UP;
                        Globals.gfClickedModeOn = false;
                    }

                    // ATE: Added repeat resons....
                    if (MouseAction.HasFlag(MouseDos.LBUTTON_REPEAT))
                    {
                        ButtonReason |= (int)MSYS_CALLBACK_REASON.LBUTTON_REPEAT;
                    }

                    if (MouseAction.HasFlag(MouseDos.RBUTTON_REPEAT))
                    {
                        ButtonReason |= (int)MSYS_CALLBACK_REASON.RBUTTON_REPEAT;
                    }

                    if (ButtonReason != (int)MSYS_CALLBACK_REASON.NONE)
                    {

                        if (CurrentRegion.uiFlags.HasFlag(MouseRegionFlags.FASTHELP))
                        {
                            // Button was clicked so remove any FastHelp text
                            CurrentRegion.uiFlags &= ~MouseRegionFlags.FASTHELP;

                            CurrentRegion.uiFlags &= ~MouseRegionFlags.GOT_BACKGROUND;

                            CurrentRegion.FastHelpTimer = Globals.gsFastHelpDelay;
                            CurrentRegion.uiFlags &= ~MouseRegionFlags.FASTHELP_RESET;
                        }

                        //Kris: Nov 31, 1999 -- Added support for double click events.
                        //This is where double clicks are checked and passed down.
                        if (ButtonReason == (int)MSYS_CALLBACK_REASON.LBUTTON_DWN)
                        {
                            long uiCurrTime = ClockManager.GetClock();
                            if (Globals.gpRegionLastLButtonDown == CurrentRegion
                                && Globals.gpRegionLastLButtonUp == CurrentRegion
                                && uiCurrTime <= Globals.guiRegionLastLButtonDownTime + MSYS_DOUBLECLICK_DELAY)
                            { //Sequential left click on same button within the maximum time allowed for a double click
                              //Double click check succeeded, set flag and reset double click globals.
                                ButtonReason |= (int)MSYS_CALLBACK_REASON.LBUTTON_DOUBLECLICK;
                                Globals.gpRegionLastLButtonDown = null;
                                Globals.gpRegionLastLButtonUp = null;
                                Globals.guiRegionLastLButtonDownTime = 0;
                            }
                            else
                            { //First click, record time and region pointer (to check if 2nd click detected later)
                                Globals.gpRegionLastLButtonDown = CurrentRegion;
                                Globals.guiRegionLastLButtonDownTime = ClockManager.GetClock();
                            }
                        }
                        else if (ButtonReason == (int)MSYS_CALLBACK_REASON.LBUTTON_UP)
                        {
                            long uiCurrTime = ClockManager.GetClock();
                            if (Globals.gpRegionLastLButtonDown == CurrentRegion
                                && uiCurrTime <= Globals.guiRegionLastLButtonDownTime + MSYS_DOUBLECLICK_DELAY)
                            {
                                //Double click is Left down, then left up, then left down.  We have just detected the left up here (step 2).
                                Globals.gpRegionLastLButtonUp = CurrentRegion;
                            }
                            else
                            {
                                //User released mouse outside of current button, so kill any chance of a double click happening.
                                Globals.gpRegionLastLButtonDown = null;
                                Globals.gpRegionLastLButtonUp = null;
                                Globals.guiRegionLastLButtonDownTime = 0;
                            }
                        }

                        // TODO: Cast to MouseCallbackReasons shouldn't be here, move to two sep callbacks.
                        CurrentRegion.ButtonCallback?.Invoke(ref CurrentRegion, (MSYS_CALLBACK_REASON)ButtonReason);
                    }
                }
            }

            MouseAction &= ~MouseDos.BUTTONS;
        }
        else if (CurrentRegion.IsEnabled)
        {
            // OK here, if we have release a button, UNSET LOCK wherever you are....
            // Just don't give this button the message....
            if (MouseAction.HasFlag(MouseDos.RBUTTON_UP))
            {
                Globals.gfClickedModeOn = false;
            }

            if (MouseAction.HasFlag(MouseDos.LBUTTON_UP))
            {
                Globals.gfClickedModeOn = false;
            }

            // OK, you still want move messages however....
            CurrentRegion.HasMouse = true;
            CurrentRegion.MousePos = CurrentCoord;
            CurrentRegion.RelativeMousePos = new(
                CurrentCoord.X - CurrentRegion.Bounds.X,
                CurrentCoord.Y - CurrentRegion.Bounds.Y);

            if (CurrentRegion.HasMoveCallback && MouseAction.HasFlag(MouseDos.MOVE))
            {
                CurrentRegion.MovementCallback?.Invoke(ref CurrentRegion, MSYS_CALLBACK_REASON.MOVE);
            }

            MouseAction &= ~MouseDos.MOVE;
        }

        PreviousRegion = CurrentRegion;
    }

    //=================================================================================================
    //	MSYS_DisableRegion
    //
    //	Disables a mouse region without removing it from the system list.
    //
    public static void MSYS_DisableRegion(ref MOUSE_REGION region)
    {
        region.uiFlags &= (~MouseRegionFlags.REGION_ENABLED);
    }

    public static void SetRegionFastHelpText(MOUSE_REGION region, string fastHelpText)
    {
        region.FastHelpText = null;
        //	region.FastHelpTimer = 0;
        if (!(region.uiFlags.HasFlag(MouseRegionFlags.REGION_EXISTS)))
        {
            return;
            //AssertMsg( 0, String( "Attempting to set fast help text, \"%S\" to an inactive region.", szText ) );
        }

        if (string.IsNullOrWhiteSpace(fastHelpText))
        {
            return; //blank (or clear)
        }

        region.FastHelpText = fastHelpText;

        // ATE: We could be replacing already existing, active text
        // so let's remove the region so it be rebuilt...

        if (gameContext.Services.GetRequiredService<ScreenManager>().CurrentScreenName != ScreenName.MAP_SCREEN)
        {
            region.uiFlags &= (~MouseRegionFlags.GOT_BACKGROUND);
            region.uiFlags &= (~MouseRegionFlags.FASTHELP_RESET);
        }
    }

    public static void Draw(SpriteRenderer sr, GraphicsDevice gd, CommandList cl)
    {
        cursors.Draw(sr, gd, cl);
    }

    public static void Draw(
        MouseCursorBackground mouseCursorBackground,
        Rectangle region,
        GraphicsDevice graphicDevice,
        CommandList commandList)
    {

    }

    //=================================================================================================
    //	MSYS_SetCurrentCursor
    //
    //	Sets the mouse cursor to the regions defined value.
    //
    private static void SetCurrentCursor(CURSOR cursor)
    {
        cursors.SetCurrentCursorFromDatabase(cursor);
    }

    public static void SetRegionUserData(MOUSE_REGION region, int index, object userdata)
        => SetRegionUserData(ref region, index, userdata);

    public static void SetRegionUserData(ref MOUSE_REGION region, int index, object userdata)
    {
        if (index < 0 || index > 3)
        {
            // TODO: log
            string str = $"Attempting MSYS_SetRegionUserData() with out of range index {index}.";

            return;
        }

        region.UserData[index] = userdata;
    }

    //======================================================================================================
    //	MSYS_TrashRegList
    //
    //	Deletes the entire region list.
    //
    private void ClearRegionList()
    {
        Regions.Clear();
    }

    //=================================================================================================
    //	MSYS_RemoveRegion
    //
    //	Removes a region from the list, disables it, then calls the callback functions for
    //	de-initialization.
    //
    public static void MSYS_RemoveRegion(MOUSE_REGION? region)
    {
        if (region == null)
        {
            return;
        }

        region.FastHelpText = null;

        MSYS_DeleteRegionFromList(ref region);

        //if the previous region is the one that we are deleting, reset the previous region
        if (PreviousRegion == region)
        {
            PreviousRegion = null;
        }

        //if the current region is the one that we are deleting, then clear it.
        if (CurrentRegion == region)
        {
            CurrentRegion = null;
        }

        //dirty our update flag
        Globals.gfRefreshUpdate = true;

        // Check if this is a locked region, and unlock if so
        if (Globals.gfClickedModeOn)
        {
            // Set global ID
            if (Globals.gusClickedIDNumber == region.IdNumber)
            {
                Globals.gfClickedModeOn = false;
            }
        }

        //clear all internal values (including the region exists flag)
        //memset(region, 0, sizeof(MOUSE_REGION));
    }

    //======================================================================================================
    //	MSYS_AddRegionToList
    //
    //	Add a region struct to the current list. The list is sorted by priority levels. If two entries
    //	have the same priority level, then the latest to enter the list gets the higher priority.
    //
    public static void AddRegionToList(MOUSE_REGION region)
    {
        // Set an ID number!
        region.IdNumber = GetNewId();

        Regions.Add(region);
    }

    public static object GetRegionUserData(ref MOUSE_REGION reg, int index)
        => reg.UserData[index];

    //======================================================================================================
    //	GetNewID
    //
    //	Returns a unique ID number for region nodes. If no new ID numbers can be found, the MAX value
    //	is returned.
    //
    private static int GetNewId() => Regions.Count;

    //=================================================================================================
    //	MSYS_DefineRegion
    //
    //	Inits a MOUSE_REGION structure for use with the mouse system
    //

    public static void MSYS_DefineRegion(
        MOUSE_REGION region,
        Rectangle bounds,
        MSYS_PRIORITY priority,
        CURSOR crsr,
        MouseCallback? movecallback,
        MouseCallback? buttoncallback)
        => MSYS_DefineRegion(
            ref region,
            bounds,
            priority,
            crsr,
            movecallback,
            buttoncallback);

    public static void MSYS_DefineRegion(
        ref MOUSE_REGION region,
        Rectangle bounds,
        MSYS_PRIORITY priority,
        CURSOR crsr,
        MouseCallback? movecallback,
        MouseCallback? buttoncallback)
    {
        region.IdNumber = MSYS_ID.BASE;

        region.PriorityLevel = priority switch
        {
            MSYS_PRIORITY.AUTO => MSYS_PRIORITY.BASE,
            MSYS_PRIORITY.LOWEST when priority <= MSYS_PRIORITY.LOWEST => MSYS_PRIORITY.LOWEST,
            MSYS_PRIORITY.HIGHEST when priority >= MSYS_PRIORITY.HIGHEST => MSYS_PRIORITY.HIGHEST,
            MSYS_PRIORITY.LOW => MSYS_PRIORITY.LOW,
            MSYS_PRIORITY.BASE => MSYS_PRIORITY.BASE,
            MSYS_PRIORITY.HIGH => MSYS_PRIORITY.HIGH,
            _ => priority,
        };

        region.uiFlags = MouseRegionFlags.NO_FLAGS;

        region.MovementCallback = movecallback;
        region.ButtonCallback = buttoncallback;

        region.Cursor = crsr;
        if (crsr != CURSOR.MSYS_NO_CURSOR)
        {
            region.uiFlags |= MouseRegionFlags.SET_CURSOR;
        }

        region.Bounds = bounds;

        region.MousePos = new();
        region.RelativeMousePos = new();
        region.ButtonState = ButtonMasks.None;

        //Init fasthelp
        region.FastHelpText = null;
        region.FastHelpTimer = 0;

        region.HelpDoneCallback = null;

        //Add region to system list
        region.IsEnabled = true;
        region.uiFlags |= MouseRegionFlags.REGION_EXISTS;
        AddRegionToList(region);

        // Dirty our update flag
        Globals.gfRefreshUpdate = true;
    }

    //======================================================================================================
    //	MSYS_DeleteRegionFromList
    //
    //	Removes a region from the current list.
    //
    private static void MSYS_DeleteRegionFromList(ref MOUSE_REGION region)
    {
        if (Regions.Contains(region))
        {
            Regions.Remove(region);
        }
    }

    public void Dispose()
    {
    }

    public ValueTask<bool> Initialize()
    {
        // VeldridVideoManager = gameContext.Services.GetRequiredService<IVideoManager>();

        return ValueTask.FromResult(true);
    }

    public static void SimulateMouseMovement(int x, int y)
    {
    }

    public static void RestrictMouseCursor(Rectangle messageBoxRestrictedCursorRegion)
    {
    }

    public static void MSYS_AddRegion(ref MOUSE_REGION gUserTurnRegion)
    {
        // this didn't do anything in original code?
    }

    public static void MSYS_ChangeRegionCursor(MOUSE_REGION? region, CURSOR crsr)
    {
        if (region is null)
        {
            return;
        }

        region.uiFlags &= (~MouseRegionFlags.SET_CURSOR);
        region.Cursor = crsr;
        if (crsr != CURSOR.None)
        {
            region.uiFlags |= MouseRegionFlags.SET_CURSOR;

            // If we are not in the region, donot update!
            if (!(region.uiFlags.HasFlag(MouseRegionFlags.IN_AREA)))
            {
                return;
            }

            // Update cursor
            MouseSubSystem.MSYS_SetCurrentCursor(crsr);
        }
    }

    public static void MSYS_SetCurrentCursor(CURSOR crsr)
    {
        cursors.SetCurrentCursorFromDatabase(crsr);
    }

    internal static void MSYS_DisableRegion(MOUSE_REGION mOUSE_REGION)
    {
        throw new NotImplementedException();
    }
}

public class MOUSE_REGION
{
    public MOUSE_REGION(string name) => Name = name;
    public string Name { get; private set; }
    public int IdNumber;                        // Region's ID number, set by mouse system
    public MSYS_PRIORITY PriorityLevel;         // Region's Priority, set by system and/or caller
    public MouseRegionFlags uiFlags;                     // Region's state flags

    // Screen area affected by this region (absolute coordinates)
    public Rectangle Bounds;

    // Mouse's Coordinates in absolute screen coordinates
    public Point MousePos;

    // Mouse's Coordinates relative to the Top-Left corner of the region
    public Point RelativeMousePos;

    public ButtonMasks ButtonState;             // Current state of the mouse buttons
    public CURSOR Cursor;                          // Cursor to use when mouse in this region (see flags)
    public MouseCallback? MovementCallback;        // Pointer to callback function if movement occured in this region
    public MouseCallback? ButtonCallback;      // Pointer to callback function if button action occured in this region
    public object[] UserData = new object[4];        // User Data, can be set to anything!

    //Fast help vars.
    public int FastHelpTimer;        // Countdown timer for FastHelp text
    public string? FastHelpText;       // Text string for the FastHelp (describes buttons if left there a while)
    public int FastHelpRect;
    public MOUSE_HELPTEXT_DONE_CALLBACK? HelpDoneCallback;

    public bool IsEnabled { get; set; }
    public bool HasMoveCallback => MovementCallback is not null;
    public bool HasButtonCallback => ButtonCallback is not null;

    public bool HasMouse { get; internal set; }

    public override string ToString()
    {
        return $"{Name}: {IdNumber}: {PriorityLevel}: {Bounds}";
    }
}

[Flags]
public enum ButtonMasks
{
    // Mouse system button masks
    None = 0,
    MSYS_LEFT_BUTTON = 1,
    MSYS_RIGHT_BUTTON = 2,
}

[Flags]
public enum MouseRegionFlags
{
    NO_FLAGS = 0x00000000,
    IN_AREA = 0x00000001,
    SET_CURSOR = 0x00000002,
    REGION_EXISTS = 0x00000010,
    SYSTEM_INIT = 0x00000020,
    MSYS_REGION_ENABLED = 0x00000040,
    FASTHELP = 0x00000080,
    GOT_BACKGROUND = 0x00000100,
    HAS_BACKRECT = 0x00000200,
    FASTHELP_RESET = 0x00000400,
    ALLOW_DISABLED_FASTHELP = 0x00000800,

    BASE_REGION_FLAGS = SET_CURSOR,
}

public enum MSYS_PRIORITY
{
    LOWEST = 0,
    LOW = 15,
    BASE = 31,
    NORMAL = 31,
    HIGH = 63,
    HIGHEST = 127,
    SYSTEM = -1,
    AUTO = -1,
}

public static class MSYS_ID
{
    public const int BASE = 1;
    public const int MAX = int.MaxValue; // ( int max )
    public const int SYSTEM = 0;
}

[Flags]
public enum MSYS_CALLBACK_REASON
{
    NONE = 0,
    INIT = 1,
    MOVE = 2,
    LBUTTON_DWN = 4,
    LBUTTON_UP = 8,
    RBUTTON_DWN = 16,
    RBUTTON_UP = 32,
    BUTTONS = LBUTTON_DWN | LBUTTON_UP | RBUTTON_DWN | RBUTTON_UP,
    LOST_MOUSE = 64,
    GAIN_MOUSE = 128,

    LBUTTON_REPEAT = 256,
    RBUTTON_REPEAT = 512,
    LBUTTON_DOUBLECLICK = 1024,

    NO_CURSOR = 65534,
}

[Flags]
public enum MouseDos
{
    NO_ACTION = 0,
    MOVE = 1,
    LBUTTON_DWN = 2,
    LBUTTON_UP = 4,
    RBUTTON_DWN = 8,
    RBUTTON_UP = 16,
    LBUTTON_REPEAT = 32,
    RBUTTON_REPEAT = 64,

    BUTTONS = LBUTTON_DWN | LBUTTON_UP | RBUTTON_DWN | RBUTTON_UP | RBUTTON_REPEAT | LBUTTON_REPEAT,
}

public struct MouseCursorBackground
{
    public bool fRestore;
    public int usMouseXPos, usMouseYPos;
    public int usLeft, usTop, usRight, usBottom;
    public Rectangle Region;
    public Texture _pSurface;
    public Texture pSurface;
}

public delegate void MouseCallback(ref MOUSE_REGION region, MSYS_CALLBACK_REASON callbackReason);
public delegate void MOUSE_HELPTEXT_DONE_CALLBACK();
