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

namespace SharpAlliance.Core.SubSystems
{
    public delegate void ButtonCallback(ref GUI_BUTTON btn, MouseCallbackReasons reason);
    public class MouseSubSystem : ISharpAllianceManager
    {
        public static GuiCallback DefaultMoveCallback { get; private set; }
        private readonly ILogger<MouseSubSystem> logger;
        private readonly IClockManager clock;
        private readonly ButtonSubSystem buttons;
        private readonly CursorSubSystem cursors;
        private readonly GameContext gameContext;
        private const int MSYS_DOUBLECLICK_DELAY = 400;

        //Records and stores the last place the user clicked.  These values are compared to the current
        //click to determine if a double click event has been detected.
        private MouseRegion? gpRegionLastLButtonDown = null;
        private MouseRegion? gpRegionLastLButtonUp = null;
        private long guiRegionLastLButtonDownTime = 0;

        private Point CurrentCoord = new(0, 0);

        private ButtonMasks CurrentButtons;
        private MouseDos MouseAction = 0;

        private bool MouseSystemInitialized = false;
        private bool UseMouseHandlerHook = false;

        private bool WasMouseGrabbed = false;
        private MouseRegion? GrabbedRegion = null;

        private int gusClickedIDNumber;
        private bool gfClickedModeOn = false;

        private MouseRegion? RegionList = null;

        public MouseRegion? PreviousRegion = null;
        public MouseRegion? CurrentRegion = null;

        //When set, the fast help text will be instantaneous, if consecutive regions with help text are
        //hilighted.  It is set, whenever the timer for the first help button expires, and the mode is
        //cleared as soon as the cursor moves into no region or a region with no helptext.
        private bool gfPersistantFastHelpMode;

        private int gsFastHelpDelay = 600; // In timer ticks
        private bool gfShowFastHelp = true;

        MouseRegion SystemBaseRegion = new(nameof(SystemBaseRegion))
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
            Cursor = Cursor.None,
            MovementCallback = null,
            ButtonCallback = null,
            UserData = new[] { 0, 0, 0, 0 },
            FastHelpTimer = 0,
            FastHelpText = string.Empty,
            FastHelpRect = -1,
            HelpDoneCallback = null,// MouseCallbackReasons.NO_CALLBACK, 
        };

        public bool gfRefreshUpdate = false;

        public List<MouseRegion> Regions { get; set; } = new(100);
        public Texture gpMouseCursor { get; set; }
        public Image<Rgba32> gpMouseCursorOriginal { get; set; }
        public bool IsInitialized { get; }

        public MouseSubSystem(
            ILogger<MouseSubSystem> logger,
            GameContext gameContext,
            IClockManager clockManager,
            ButtonSubSystem buttonManager,
            CursorSubSystem cursorSubSystem)
        {
            this.logger = logger;

            DefaultMoveCallback = this.BtnGenericMouseMoveButtonCallback;
            this.logger.LogDebug(LoggingEventId.MouseSystem, "Mouse Region System");
            this.clock = clockManager;
            this.buttons = buttonManager;
            this.cursors = cursorSubSystem;
            this.gameContext = gameContext;

            if (this.RegionList is not null)
            {
                this.ClearRegionList();
            }

            this.CurrentCoord = new(0, 0);
            this.CurrentButtons = 0;
            this.MouseAction = MouseDos.NO_ACTION;

            this.PreviousRegion = null;
            this.MouseSystemInitialized = true;
            this.UseMouseHandlerHook = false;

            this.WasMouseGrabbed = false;
            this.GrabbedRegion = null;

            // Setup the system's background region
            this.SystemBaseRegion.IdNumber = MSYS_ID.SYSTEM;
            this.SystemBaseRegion.PriorityLevel = MSYS_PRIORITY.SYSTEM;
            this.SystemBaseRegion.uiFlags = MouseRegionFlags.BASE_REGION_FLAGS;
            this.SystemBaseRegion.Bounds = new(-32767, -32767, 32767, 32767);

            this.SystemBaseRegion.MousePos = new(0, 0);
            this.SystemBaseRegion.RelativeMousePos = new(0, 0);

            this.SystemBaseRegion.ButtonState = 0;
            this.SystemBaseRegion.Cursor = 0;
            this.SystemBaseRegion.UserData[0] = 0;
            this.SystemBaseRegion.UserData[1] = 0;
            this.SystemBaseRegion.UserData[2] = 0;
            this.SystemBaseRegion.UserData[3] = 0;
            this.SystemBaseRegion.MovementCallback = null;
            this.SystemBaseRegion.ButtonCallback = null;

            this.SystemBaseRegion.FastHelpTimer = 0;
            this.SystemBaseRegion.FastHelpText = string.Empty;

            this.SystemBaseRegion.FastHelpRect = -1;

            this.AddRegionToList(ref this.SystemBaseRegion);

            this.UseMouseHandlerHook = true;
        }

        //Generic Button Movement Callback to reset the mouse button if the mouse is no longer
        //in the button region.
        public void BtnGenericMouseMoveButtonCallback(ref GUI_BUTTON btn, MouseCallbackReasons reasonValue)
        {
            MouseCallbackReasons reason = reasonValue;

            //If the button isn't the anchored button, then we don't want to modify the button state.
            if (btn != ButtonSubSystem.gpAnchoredButton)
            {
                return;
            }

            if (reason.HasFlag(MouseCallbackReasons.LOST_MOUSE))
            {
                if (!ButtonSubSystem.gfAnchoredState)
                {
                    btn.uiFlags &= ~ButtonFlags.BUTTON_CLICKED_ON;
                    if (btn.ubSoundSchemeID != 0)
                    {
                        this.buttons.PlayButtonSound(btn.IdNum, ButtonSounds.BUTTON_SOUND_CLICKED_OFF);
                    }
                }
            }
            else if (reason.HasFlag(MouseCallbackReasons.GAIN_MOUSE))
            {
                btn.uiFlags |= ButtonFlags.BUTTON_CLICKED_ON;
                if (btn.ubSoundSchemeID != 0)
                {
                    this.buttons.PlayButtonSound(btn.IdNum, ButtonSounds.BUTTON_SOUND_CLICKED_ON);
                }
            }
        }

        public void MouseHook(MouseEvents mouseEvent, Point coord, bool leftButtonDown, bool rightButtonDown)
        {
            // If the mouse system isn't initialized, get out o' here
            if (!this.MouseSystemInitialized)
            {
                return;
            }

            // If we're not using the handler stuff, ignore this call
            if (!this.UseMouseHandlerHook)
            {
                return;
            }

            coord.Y = 480 - coord.Y;

            if (coord != this.CurrentCoord)
            {
                // Console.WriteLine($"Mouse: {coord} L: {leftButtonDown} R: {rightButtonDown}");
            }


            this.MouseAction = MouseDos.NO_ACTION;
            switch (mouseEvent)
            {
                case MouseEvents.LEFT_BUTTON_DOWN:
                case MouseEvents.LEFT_BUTTON_UP:
                case MouseEvents.RIGHT_BUTTON_DOWN:
                case MouseEvents.RIGHT_BUTTON_UP:
                    //MSYS_Action|=MouseDos.BUTTONS;
                    if (mouseEvent == MouseEvents.LEFT_BUTTON_DOWN)
                    {
                        this.MouseAction |= MouseDos.LBUTTON_DWN;
                    }
                    else if (mouseEvent == MouseEvents.LEFT_BUTTON_UP)
                    {
                        this.MouseAction |= MouseDos.LBUTTON_UP;
                        //Kris:
                        //Used only if applicable.  This is used for that special button that is locked with the
                        //mouse press -- just like windows.  When you release the button, the previous state
                        //of the button is restored if you released the mouse outside of it's boundaries.  If
                        //you release inside of the button, the action is selected -- but later in the code.
                        //NOTE:  It has to be here, because the mouse can be released anywhere regardless of
                        //regions, buttons, etc.

                        this.buttons.ReleaseAnchorMode(coord);
                    }
                    else if (mouseEvent == MouseEvents.RIGHT_BUTTON_DOWN)
                    {
                        this.MouseAction |= MouseDos.RBUTTON_DWN;
                    }
                    else if (mouseEvent == MouseEvents.RIGHT_BUTTON_UP)
                    {
                        this.MouseAction |= MouseDos.RBUTTON_UP;
                    }

                    if (leftButtonDown)
                    {
                        this.CurrentButtons |= ButtonMasks.MSYS_LEFT_BUTTON;
                    }
                    else
                    {
                        this.CurrentButtons &= ~ButtonMasks.MSYS_LEFT_BUTTON;
                    }

                    if (rightButtonDown)
                    {
                        this.CurrentButtons |= ButtonMasks.MSYS_RIGHT_BUTTON;
                    }
                    else
                    {
                        this.CurrentButtons &= ~ButtonMasks.MSYS_RIGHT_BUTTON;
                    }

                    if (coord != this.CurrentCoord)
                    {
                        this.MouseAction |= MouseDos.MOVE;
                        this.CurrentCoord = coord;
                    }

                    this.UpdateMouseRegion();
                    break;

                // ATE: Checks here for mouse button repeats.....
                // Call mouse region with new reason
                case MouseEvents.LEFT_BUTTON_REPEAT:
                case MouseEvents.RIGHT_BUTTON_REPEAT:

                    if (mouseEvent == MouseEvents.LEFT_BUTTON_REPEAT)
                    {
                        this.MouseAction |= MouseDos.LBUTTON_REPEAT;
                    }
                    else if (mouseEvent == MouseEvents.RIGHT_BUTTON_REPEAT)
                    {
                        this.MouseAction |= MouseDos.RBUTTON_REPEAT;
                    }

                    if (coord != this.CurrentCoord)
                    {
                        this.MouseAction |= MouseDos.MOVE;
                        this.CurrentCoord = coord;
                    }

                    this.UpdateMouseRegion();
                    break;

                case MouseEvents.MousePosition:
                    if (coord != this.CurrentCoord || this.gfRefreshUpdate)
                    {
                        this.MouseAction |= MouseDos.MOVE;
                        this.CurrentCoord = coord;

                        this.gfRefreshUpdate = false;

                        this.UpdateMouseRegion();
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
        private void UpdateMouseRegion()
        {
            int ButtonReason;

            // Check previous region!
            if (this.WasMouseGrabbed)
            {
                this.CurrentRegion = this.GrabbedRegion;
            }

            var foundRegions = this.Regions.Where(r =>
                r.IsEnabled
                && r.Bounds.Contains(this.CurrentCoord));

            foundRegions = foundRegions
                .OrderByDescending(r => r.PriorityLevel)
                .ThenByDescending(r => r.IdNumber);

            if (!foundRegions.Any())
            {
                return;
            }

            this.CurrentRegion = foundRegions.First();

            if (this.CurrentRegion != this.PreviousRegion)
            {
                Console.WriteLine($"In MouseRegion: {this.CurrentRegion}");
            }

            if (this.PreviousRegion is not null)
            {
                this.PreviousRegion.HasMouse = false;

                if (this.PreviousRegion != this.CurrentRegion)
                {
                    //Remove the help text for the previous region if one is currently being displayed.
                    if (this.PreviousRegion.FastHelpText is not null)
                    {
                        //ExecuteMouseHelpEndCallBack( MSYS_PrevRegion );

                        this.PreviousRegion.uiFlags &= ~MouseRegionFlags.GOT_BACKGROUND;
                        this.PreviousRegion.uiFlags &= ~MouseRegionFlags.FASTHELP_RESET;
                    }

                    this.CurrentRegion.FastHelpTimer = this.gsFastHelpDelay;

                    // Force a callbacks to happen on previous region to indicate that
                    // the mouse has left the old region
                    if (this.PreviousRegion.HasMoveCallback
                        && this.PreviousRegion.IsEnabled)
                    {
                        this.PreviousRegion.MovementCallback?.Invoke(ref this.PreviousRegion, MouseCallbackReasons.LOST_MOUSE);
                    }
                }
            }

            if (this.CurrentRegion != this.PreviousRegion)
            {
                //Kris -- October 27, 1997
                //Implemented gain mouse region

                if (this.CurrentRegion.HasMoveCallback)
                {
                    if (this.CurrentRegion.FastHelpText is not null
                        && !this.CurrentRegion.uiFlags.HasFlag(MouseRegionFlags.FASTHELP_RESET))
                    {
                        //ExecuteMouseHelpEndCallBack( MSYS_CurrRegion );
                        this.CurrentRegion.FastHelpTimer = this.gsFastHelpDelay;
                        this.CurrentRegion.uiFlags &= ~MouseRegionFlags.GOT_BACKGROUND;
                        this.CurrentRegion.uiFlags |= MouseRegionFlags.FASTHELP_RESET;
                    }

                    if (this.CurrentRegion.IsEnabled)
                    {
                        this.CurrentRegion.MovementCallback?.Invoke(ref this.CurrentRegion, MouseCallbackReasons.GAIN_MOUSE);
                    }
                }

                // if the cursor is set and is not set to no cursor
                if (this.CurrentRegion.IsEnabled
                    && this.CurrentRegion.uiFlags.HasFlag(MouseRegionFlags.SET_CURSOR)
                    && this.CurrentRegion.Cursor != 0)
                {
                    this.SetCurrentCursor(this.CurrentRegion.Cursor);
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
                        this.SetCurrentCursor(regionWithCursor.Cursor);
                    }
                }
            }

            // OK, if we do not have a button down, any button is game!
            if (!this.gfClickedModeOn || (this.gfClickedModeOn && this.gusClickedIDNumber == this.CurrentRegion.IdNumber))
            {
                this.CurrentRegion.HasMouse = true;

                this.CurrentRegion.MousePos = this.CurrentCoord;

                this.CurrentRegion.RelativeMousePos = new(
                    this.CurrentCoord.X - this.CurrentRegion.Bounds.X,
                    this.CurrentCoord.Y - this.CurrentRegion.Bounds.Y);

                this.CurrentRegion.ButtonState = this.CurrentButtons;

                if (this.CurrentRegion.IsEnabled
                    && this.CurrentRegion.HasMoveCallback
                    && this.MouseAction.HasFlag(MouseDos.MOVE))
                {
                    IVideoManager.DebugRenderer.DrawRectangle(this.CurrentRegion.Bounds, Color.Yellow);
                    this.CurrentRegion.MovementCallback?.Invoke(ref this.CurrentRegion, MouseCallbackReasons.MOVE);
                }

                //ExecuteMouseHelpEndCallBack( MSYS_CurrRegion );
                //MSYS_CurrRegion.FastHelpTimer = gsFastHelpDelay;

                this.MouseAction &= ~MouseDos.MOVE;

                if (this.CurrentRegion.HasButtonCallback
                    && (this.MouseAction & MouseDos.BUTTONS) != 0)
                {
                    if (this.CurrentRegion.IsEnabled)
                    {
                        ButtonReason = (int)MouseCallbackReasons.NONE;
                        if (this.MouseAction.HasFlag(MouseDos.LBUTTON_DWN))
                        {
                            ButtonReason |= (int)MouseCallbackReasons.LBUTTON_DWN;
                            this.gfClickedModeOn = true;
                            // Set global ID
                            this.gusClickedIDNumber = this.CurrentRegion.IdNumber;
                        }

                        if (this.MouseAction.HasFlag(MouseDos.LBUTTON_UP))
                        {
                            ButtonReason |= (int)MouseCallbackReasons.LBUTTON_UP;
                            this.gfClickedModeOn = false;
                        }

                        if (this.MouseAction.HasFlag(MouseDos.RBUTTON_DWN))
                        {
                            ButtonReason |= (int)MouseCallbackReasons.RBUTTON_DWN;
                            this.gfClickedModeOn = true;
                            // Set global ID
                            this.gusClickedIDNumber = this.CurrentRegion.IdNumber;
                        }

                        if (this.MouseAction.HasFlag(MouseDos.RBUTTON_UP))
                        {
                            ButtonReason |= (int)MouseCallbackReasons.RBUTTON_UP;
                            this.gfClickedModeOn = false;
                        }

                        // ATE: Added repeat resons....
                        if (this.MouseAction.HasFlag(MouseDos.LBUTTON_REPEAT))
                        {
                            ButtonReason |= (int)MouseCallbackReasons.LBUTTON_REPEAT;
                        }

                        if (this.MouseAction.HasFlag(MouseDos.RBUTTON_REPEAT))
                        {
                            ButtonReason |= (int)MouseCallbackReasons.RBUTTON_REPEAT;
                        }

                        if (ButtonReason != (int)MouseCallbackReasons.NONE)
                        {

                            if (this.CurrentRegion.uiFlags.HasFlag(MouseRegionFlags.FASTHELP))
                            {
                                // Button was clicked so remove any FastHelp text
                                this.CurrentRegion.uiFlags &= ~MouseRegionFlags.FASTHELP;

                                this.CurrentRegion.uiFlags &= ~MouseRegionFlags.GOT_BACKGROUND;

                                this.CurrentRegion.FastHelpTimer = this.gsFastHelpDelay;
                                this.CurrentRegion.uiFlags &= ~MouseRegionFlags.FASTHELP_RESET;
                            }

                            //Kris: Nov 31, 1999 -- Added support for double click events.
                            //This is where double clicks are checked and passed down.
                            if (ButtonReason == (int)MouseCallbackReasons.LBUTTON_DWN)
                            {
                                long uiCurrTime = this.clock.GetClock();
                                if (this.gpRegionLastLButtonDown == this.CurrentRegion &&
                                        this.gpRegionLastLButtonUp == this.CurrentRegion &&
                                        uiCurrTime <= this.guiRegionLastLButtonDownTime + MSYS_DOUBLECLICK_DELAY)
                                { //Sequential left click on same button within the maximum time allowed for a double click
                                  //Double click check succeeded, set flag and reset double click globals.
                                    ButtonReason |= (int)MouseCallbackReasons.LBUTTON_DOUBLECLICK;
                                    this.gpRegionLastLButtonDown = null;
                                    this.gpRegionLastLButtonUp = null;
                                    this.guiRegionLastLButtonDownTime = 0;
                                }
                                else
                                { //First click, record time and region pointer (to check if 2nd click detected later)
                                    this.gpRegionLastLButtonDown = this.CurrentRegion;
                                    this.guiRegionLastLButtonDownTime = this.clock.GetClock();
                                }
                            }
                            else if (ButtonReason == (int)MouseCallbackReasons.LBUTTON_UP)
                            {
                                long uiCurrTime = this.clock.GetClock();
                                if (this.gpRegionLastLButtonDown == this.CurrentRegion
                                    && uiCurrTime <= this.guiRegionLastLButtonDownTime + MSYS_DOUBLECLICK_DELAY)
                                {
                                    //Double click is Left down, then left up, then left down.  We have just detected the left up here (step 2).
                                    this.gpRegionLastLButtonUp = this.CurrentRegion;
                                }
                                else
                                {
                                    //User released mouse outside of current button, so kill any chance of a double click happening.
                                    this.gpRegionLastLButtonDown = null;
                                    this.gpRegionLastLButtonUp = null;
                                    this.guiRegionLastLButtonDownTime = 0;
                                }
                            }

                            // TODO: Cast to MouseCallbackReasons shouldn't be here, move to two sep callbacks.
                            this.CurrentRegion.ButtonCallback?.Invoke(ref this.CurrentRegion, (MouseCallbackReasons)ButtonReason);
                        }
                    }
                }

                this.MouseAction &= ~MouseDos.BUTTONS;
            }
            else if (this.CurrentRegion.IsEnabled)
            {
                // OK here, if we have release a button, UNSET LOCK wherever you are....
                // Just don't give this button the message....
                if (this.MouseAction.HasFlag(MouseDos.RBUTTON_UP))
                {
                    this.gfClickedModeOn = false;
                }

                if (this.MouseAction.HasFlag(MouseDos.LBUTTON_UP))
                {
                    this.gfClickedModeOn = false;
                }

                // OK, you still want move messages however....
                this.CurrentRegion.HasMouse = true;
                this.CurrentRegion.MousePos = this.CurrentCoord;
                this.CurrentRegion.RelativeMousePos = new(
                    this.CurrentCoord.X - this.CurrentRegion.Bounds.X,
                    this.CurrentCoord.Y - this.CurrentRegion.Bounds.Y);

                if (this.CurrentRegion.HasMoveCallback && this.MouseAction.HasFlag(MouseDos.MOVE))
                {
                    this.CurrentRegion.MovementCallback?.Invoke(ref this.CurrentRegion, MouseCallbackReasons.MOVE);
                }

                this.MouseAction &= ~MouseDos.MOVE;
            }

            this.PreviousRegion = this.CurrentRegion;
        }

        public void SetRegionFastHelpText(MouseRegion region, string fastHelpText)
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

            if (this.gameContext.Services.GetRequiredService<IScreenManager>().CurrentScreenName != ScreenName.MAP_SCREEN)
            {
                region.uiFlags &= (~MouseRegionFlags.GOT_BACKGROUND);
                region.uiFlags &= (~MouseRegionFlags.FASTHELP_RESET);
            }
        }

        public void Draw(SpriteRenderer sr, GraphicsDevice gd, CommandList cl)
        {
            this.cursors.Draw(sr, gd, cl);
        }

        public void Draw(
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
        private void SetCurrentCursor(Cursor cursor)
        {
            this.cursors.SetCurrentCursorFromDatabase(cursor);
        }

        public void SetRegionUserData(MouseRegion region, int index, int userdata)
            => this.SetRegionUserData(ref region, index, userdata);

        public void SetRegionUserData(ref MouseRegion region, int index, int userdata)
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
            this.Regions.Clear();
        }

        //=================================================================================================
        //	MSYS_RemoveRegion
        //
        //	Removes a region from the list, disables it, then calls the callback functions for
        //	de-initialization.
        //
        public void MSYS_RemoveRegion(ref MouseRegion region)
        {
            region.FastHelpText = null;

            this.MSYS_DeleteRegionFromList(ref region);

            //if the previous region is the one that we are deleting, reset the previous region
            if (this.PreviousRegion == region)
            {
                this.PreviousRegion = null;
            }

            //if the current region is the one that we are deleting, then clear it.
            if (this.CurrentRegion == region)
            {
                this.CurrentRegion = null;
            }

            //dirty our update flag
            this.gfRefreshUpdate = true;

            // Check if this is a locked region, and unlock if so
            if (this.gfClickedModeOn)
            {
                // Set global ID
                if (this.gusClickedIDNumber == region.IdNumber)
                {
                    this.gfClickedModeOn = false;
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
        private void AddRegionToList(ref MouseRegion region)
        {
            // Set an ID number!
            region.IdNumber = this.GetNewId();

            this.Regions.Add(region);
        }

        public int GetRegionUserData(ref MouseRegion reg, int index) => reg.UserData[index];

        //======================================================================================================
        //	GetNewID
        //
        //	Returns a unique ID number for region nodes. If no new ID numbers can be found, the MAX value
        //	is returned.
        //
        private int GetNewId() => this.Regions.Count;

        //=================================================================================================
        //	MSYS_DefineRegion
        //
        //	Inits a MOUSE_REGION structure for use with the mouse system
        //

        public void DefineRegion(
            MouseRegion region,
            Rectangle bounds,
            MSYS_PRIORITY priority,
            Cursor crsr,
            MouseCallback? movecallback,
            MouseCallback? buttoncallback)
            => this.DefineRegion(
                ref region,
                bounds,
                priority,
                crsr,
                movecallback,
                buttoncallback);

        public void DefineRegion(
            ref MouseRegion region,
            Rectangle bounds,
            MSYS_PRIORITY priority,
            Cursor crsr,
            MouseCallback? movecallback,
            MouseCallback? buttoncallback)
        {
            region.IdNumber = MSYS_ID.BASE;

            region.PriorityLevel = priority switch
            {
                MSYS_PRIORITY.AUTO => MSYS_PRIORITY.BASE,
                MSYS_PRIORITY.LOWEST when priority <= MSYS_PRIORITY.LOWEST => MSYS_PRIORITY.LOWEST,
                MSYS_PRIORITY.HIGHEST when priority >= MSYS_PRIORITY.HIGHEST => MSYS_PRIORITY.HIGHEST,
                MSYS_PRIORITY.LOW =>  MSYS_PRIORITY.LOW,
                MSYS_PRIORITY.BASE => MSYS_PRIORITY.BASE,
                MSYS_PRIORITY.HIGH => MSYS_PRIORITY.HIGH,
                _ => priority,
            };

            region.uiFlags = MouseRegionFlags.NO_FLAGS;

            region.MovementCallback = movecallback;
            region.ButtonCallback = buttoncallback;

            region.Cursor = crsr;
            if (crsr != Cursor.MSYS_NO_CURSOR)
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
            this.AddRegionToList(ref region);

            // Dirty our update flag
            this.gfRefreshUpdate = true;
        }

        //======================================================================================================
        //	MSYS_DeleteRegionFromList
        //
        //	Removes a region from the current list.
        //
        private void MSYS_DeleteRegionFromList(ref MouseRegion region)
        {
            if (this.Regions.Contains(region))
            {
                this.Regions.Remove(region);
            }
        }

        public void Dispose()
        {
        }

        public ValueTask<bool> Initialize()
        {
            // this.video = this.gameContext.Services.GetRequiredService<IVideoManager>();

            return ValueTask.FromResult(true);
        }
    }

    public class MouseRegion
    {
        public MouseRegion(string name) => this.Name = name;
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
        public Cursor Cursor;                          // Cursor to use when mouse in this region (see flags)
        public MouseCallback? MovementCallback;        // Pointer to callback function if movement occured in this region
        public MouseCallback? ButtonCallback;      // Pointer to callback function if button action occured in this region
        public int[] UserData = new int[4];        // User Data, can be set to anything!

        //Fast help vars.
        public int FastHelpTimer;        // Countdown timer for FastHelp text
        public string? FastHelpText;       // Text string for the FastHelp (describes buttons if left there a while)
        public int FastHelpRect;
        public MOUSE_HELPTEXT_DONE_CALLBACK? HelpDoneCallback;

        public bool IsEnabled { get; set; }
        public bool HasMoveCallback => this.MovementCallback is not null;
        public bool HasButtonCallback => this.ButtonCallback is not null;

        public bool HasMouse { get; internal set; }

        public override string ToString()
        {
            return $"{this.Name}: {this.IdNumber}: {this.PriorityLevel}: {this.Bounds}";
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
        SET_CURSOR = 0x00000002,
        REGION_EXISTS = 0x00000010,
        SYSTEM_INIT = 0x00000020,
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
    public enum MouseCallbackReasons
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

    public delegate void MouseCallback(ref MouseRegion region, MouseCallbackReasons callbackReason);
    public delegate void MOUSE_HELPTEXT_DONE_CALLBACK();
}
