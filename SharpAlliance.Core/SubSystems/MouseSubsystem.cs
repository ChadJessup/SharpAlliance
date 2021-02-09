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

        private Point MSYS_CurrentM = new(0, 0);

        private ButtonMasks MSYS_CurrentButtons;
        private MouseDos MSYS_Action = 0;

        private bool MSYS_SystemInitialized = false;
        private bool MSYS_UseMouseHandlerHook = false;

        private bool MSYS_Mouse_Grabbed = false;
        private MouseRegion? MSYS_GrabRegion = null;

        private int gusClickedIDNumber;
        private bool gfClickedModeOn = false;

        private MouseRegion? MSYS_RegList = null;

        public MouseRegion? MSYS_PrevRegion = null;
        public MouseRegion? MSYS_CurrRegion = null;

        //When set, the fast help text will be instantaneous, if consecutive regions with help text are
        //hilighted.  It is set, whenever the timer for the first help button expires, and the mode is
        //cleared as soon as the cursor moves into no region or a region with no helptext.
        private bool gfPersistantFastHelpMode;

        private int gsFastHelpDelay = 600; // In timer ticks
        private bool gfShowFastHelp = true;

        MouseRegion MSYS_SystemBaseRegion = new(nameof(MSYS_SystemBaseRegion))
        {
            IDNumber = MSYS_ID.SYSTEM,
            PriorityLevel = MSYS_PRIORITY.SYSTEM,
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

            if (this.MSYS_RegList is not null)
            {
                this.MSYS_TrashRegList();
            }

            this.MSYS_CurrentM = new(0, 0);
            this.MSYS_CurrentButtons = 0;
            this.MSYS_Action = MouseDos.NO_ACTION;

            this.MSYS_PrevRegion = null;
            this.MSYS_SystemInitialized = true;
            this.MSYS_UseMouseHandlerHook = false;

            this.MSYS_Mouse_Grabbed = false;
            this.MSYS_GrabRegion = null;

            // Setup the system's background region
            this.MSYS_SystemBaseRegion.IDNumber = MSYS_ID.SYSTEM;
            this.MSYS_SystemBaseRegion.PriorityLevel = MSYS_PRIORITY.SYSTEM;
            this.MSYS_SystemBaseRegion.uiFlags = MouseRegionFlags.BASE_REGION_FLAGS;
            this.MSYS_SystemBaseRegion.Bounds = new(-32767, -32767, 32767, 32767);

            this.MSYS_SystemBaseRegion.MousePos = new(0, 0);
            this.MSYS_SystemBaseRegion.RelativeMousePos = new(0, 0);

            this.MSYS_SystemBaseRegion.ButtonState = 0;
            this.MSYS_SystemBaseRegion.Cursor = 0;
            this.MSYS_SystemBaseRegion.UserData[0] = 0;
            this.MSYS_SystemBaseRegion.UserData[1] = 0;
            this.MSYS_SystemBaseRegion.UserData[2] = 0;
            this.MSYS_SystemBaseRegion.UserData[3] = 0;
            this.MSYS_SystemBaseRegion.MovementCallback = null;
            this.MSYS_SystemBaseRegion.ButtonCallback = null;

            this.MSYS_SystemBaseRegion.FastHelpTimer = 0;
            this.MSYS_SystemBaseRegion.FastHelpText = string.Empty;
            ;
            this.MSYS_SystemBaseRegion.FastHelpRect = -1;

            this.MSYS_AddRegionToList(ref this.MSYS_SystemBaseRegion);

            this.MSYS_UseMouseHandlerHook = true;
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
                        this.buttons.PlayButtonSound(btn.IDNum, ButtonSounds.BUTTON_SOUND_CLICKED_OFF);
                    }
                }

                // this.video.InvalidateRegion(btn.Area.Bounds);
            }
            else if (reason.HasFlag(MouseCallbackReasons.GAIN_MOUSE))
            {
                btn.uiFlags |= ButtonFlags.BUTTON_CLICKED_ON;
                if (btn.ubSoundSchemeID != 0)
                {
                    this.buttons.PlayButtonSound(btn.IDNum, ButtonSounds.BUTTON_SOUND_CLICKED_ON);
                }

                // this.video.InvalidateRegion(btn.Area.Bounds);
            }
        }

        public void MouseHook(MouseEvents mouseEvent, Point coord, bool leftButtonDown, bool rightButtonDown)
        {
            // If the mouse system isn't initialized, get out o' here
            if (!this.MSYS_SystemInitialized)
            {
                return;
            }

            // If we're not using the handler stuff, ignore this call
            if (!this.MSYS_UseMouseHandlerHook)
            {
                return;
            }

            coord.Y = 480 - coord.Y;

            if (coord != this.MSYS_CurrentM)
            {
                // Console.WriteLine($"Mouse: {coord} L: {leftButtonDown} R: {rightButtonDown}");
            }


            this.MSYS_Action = MouseDos.NO_ACTION;
            switch (mouseEvent)
            {
                case MouseEvents.LEFT_BUTTON_DOWN:
                case MouseEvents.LEFT_BUTTON_UP:
                case MouseEvents.RIGHT_BUTTON_DOWN:
                case MouseEvents.RIGHT_BUTTON_UP:
                    //MSYS_Action|=MouseDos.BUTTONS;
                    if (mouseEvent == MouseEvents.LEFT_BUTTON_DOWN)
                    {
                        this.MSYS_Action |= MouseDos.LBUTTON_DWN;
                    }
                    else if (mouseEvent == MouseEvents.LEFT_BUTTON_UP)
                    {
                        this.MSYS_Action |= MouseDos.LBUTTON_UP;
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
                        this.MSYS_Action |= MouseDos.RBUTTON_DWN;
                    }
                    else if (mouseEvent == MouseEvents.RIGHT_BUTTON_UP)
                    {
                        this.MSYS_Action |= MouseDos.RBUTTON_UP;
                    }

                    if (leftButtonDown)
                    {
                        this.MSYS_CurrentButtons |= ButtonMasks.MSYS_LEFT_BUTTON;
                    }
                    else
                    {
                        this.MSYS_CurrentButtons &= ~ButtonMasks.MSYS_LEFT_BUTTON;
                    }

                    if (rightButtonDown)
                    {
                        this.MSYS_CurrentButtons |= ButtonMasks.MSYS_RIGHT_BUTTON;
                    }
                    else
                    {
                        this.MSYS_CurrentButtons &= ~ButtonMasks.MSYS_RIGHT_BUTTON;
                    }

                    if (coord != this.MSYS_CurrentM)
                    {
                        this.MSYS_Action |= MouseDos.MOVE;
                        this.MSYS_CurrentM = coord;
                    }

                    this.MSYS_UpdateMouseRegion();
                    break;

                // ATE: Checks here for mouse button repeats.....
                // Call mouse region with new reason
                case MouseEvents.LEFT_BUTTON_REPEAT:
                case MouseEvents.RIGHT_BUTTON_REPEAT:

                    if (mouseEvent == MouseEvents.LEFT_BUTTON_REPEAT)
                    {
                        this.MSYS_Action |= MouseDos.LBUTTON_REPEAT;
                    }
                    else if (mouseEvent == MouseEvents.RIGHT_BUTTON_REPEAT)
                    {
                        this.MSYS_Action |= MouseDos.RBUTTON_REPEAT;
                    }

                    if (coord != this.MSYS_CurrentM)
                    {
                        this.MSYS_Action |= MouseDos.MOVE;
                        this.MSYS_CurrentM = coord;
                    }

                    this.MSYS_UpdateMouseRegion();
                    break;

                case MouseEvents.MousePosition:
                    if (coord != this.MSYS_CurrentM || this.gfRefreshUpdate)
                    {
                        this.MSYS_Action |= MouseDos.MOVE;
                        this.MSYS_CurrentM = coord;

                        this.gfRefreshUpdate = false;

                        this.MSYS_UpdateMouseRegion();
                    }
                    break;

                default:
                    //DbgMessage(TOPIC_MOUSE_SYSTEM, DBG_LEVEL_0, "ERROR -- MSYS 2 SGP Mouse Hook got bad type");
                    break;
            }
        }


        //======================================================================================================
        //	MSYS_UpdateMouseRegion
        //
        //	Searches the list for the highest priority region and updates it's info. It also dispatches
        //	the callback functions
        //
        private void MSYS_UpdateMouseRegion()
        {
            int ButtonReason;

            // Check previous region!
            if (this.MSYS_Mouse_Grabbed)
            {
                this.MSYS_CurrRegion = this.MSYS_GrabRegion;
            }

            var foundRegions = this.Regions.Where(r =>
                r.uiFlags.HasFlag(MouseRegionFlags.REGION_ENABLED)
                && r.Bounds.Contains(this.MSYS_CurrentM));

            foundRegions = foundRegions
                .OrderByDescending(r => r.PriorityLevel)
                .ThenByDescending(r => r.IDNumber);

            if (foundRegions is null || !foundRegions.Any())
            {
                return;
            }

            this.MSYS_CurrRegion = foundRegions.First();

            Console.WriteLine($"In MouseRegion: {this.MSYS_CurrRegion}");

            if (this.MSYS_PrevRegion is not null)
            {
                this.MSYS_PrevRegion.uiFlags &= ~MouseRegionFlags.MOUSE_IN_AREA;

                if (this.MSYS_PrevRegion != this.MSYS_CurrRegion)
                {
                    //Remove the help text for the previous region if one is currently being displayed.
                    if (this.MSYS_PrevRegion.FastHelpText is not null)
                    {
                        //ExecuteMouseHelpEndCallBack( MSYS_PrevRegion );

                        this.MSYS_PrevRegion.uiFlags &= ~MouseRegionFlags.GOT_BACKGROUND;
                        this.MSYS_PrevRegion.uiFlags &= ~MouseRegionFlags.FASTHELP_RESET;
                    }

                    this.MSYS_CurrRegion.FastHelpTimer = this.gsFastHelpDelay;

                    // Force a callbacks to happen on previous region to indicate that
                    // the mouse has left the old region
                    if (this.MSYS_PrevRegion.uiFlags.HasFlag(MouseRegionFlags.MOVE_CALLBACK)
                        && this.MSYS_PrevRegion.uiFlags.HasFlag(MouseRegionFlags.REGION_ENABLED))
                    {
                        this.MSYS_PrevRegion.MovementCallback?.Invoke(ref this.MSYS_PrevRegion, MouseCallbackReasons.LOST_MOUSE);
                    }
                }
            }

            if (this.MSYS_CurrRegion != this.MSYS_PrevRegion)
            {
                //Kris -- October 27, 1997
                //Implemented gain mouse region

                if (this.MSYS_CurrRegion.uiFlags.HasFlag(MouseRegionFlags.MOVE_CALLBACK))
                {
                    if (this.MSYS_CurrRegion.FastHelpText is not null
                        && !this.MSYS_CurrRegion.uiFlags.HasFlag(MouseRegionFlags.FASTHELP_RESET))
                    {
                        //ExecuteMouseHelpEndCallBack( MSYS_CurrRegion );
                        this.MSYS_CurrRegion.FastHelpTimer = this.gsFastHelpDelay;
                        this.MSYS_CurrRegion.uiFlags &= ~MouseRegionFlags.GOT_BACKGROUND;
                        this.MSYS_CurrRegion.uiFlags |= MouseRegionFlags.FASTHELP_RESET;
                    }

                    if (this.MSYS_CurrRegion.uiFlags.HasFlag(MouseRegionFlags.REGION_ENABLED))
                    {
                        this.MSYS_CurrRegion.MovementCallback?.Invoke(ref this.MSYS_CurrRegion, MouseCallbackReasons.GAIN_MOUSE);
                    }
                }

                // if the cursor is set and is not set to no cursor
                if (this.MSYS_CurrRegion.uiFlags.HasFlag(MouseRegionFlags.REGION_ENABLED)
                    && this.MSYS_CurrRegion.uiFlags.HasFlag(MouseRegionFlags.SET_CURSOR)
                    && this.MSYS_CurrRegion.Cursor != 0)
                {
                    this.MSYS_SetCurrentCursor(this.MSYS_CurrRegion.Cursor);
                }
                else
                {
                    // Addition Oct 10/1997 Carter, patch for mouse cursor
                    // start at region and find another region encompassing
                    var regionWithCursor = foundRegions.FirstOrDefault(r =>
                        r.uiFlags.HasFlag(MouseRegionFlags.REGION_ENABLED)
                        && r.uiFlags.HasFlag(MouseRegionFlags.SET_CURSOR));

                    if (regionWithCursor is not null)
                    {
                        this.MSYS_SetCurrentCursor(regionWithCursor.Cursor);
                    }
                }
            }

            // OK, if we do not have a button down, any button is game!
            if (!this.gfClickedModeOn || (this.gfClickedModeOn && this.gusClickedIDNumber == this.MSYS_CurrRegion.IDNumber))
            {
                this.MSYS_CurrRegion.uiFlags |= MouseRegionFlags.MOUSE_IN_AREA;

                this.MSYS_CurrRegion.MousePos = this.MSYS_CurrentM;

                this.MSYS_CurrRegion.RelativeMousePos = new(
                    this.MSYS_CurrentM.X - this.MSYS_CurrRegion.Bounds.X,
                    this.MSYS_CurrentM.Y - this.MSYS_CurrRegion.Bounds.Y);

                this.MSYS_CurrRegion.ButtonState = this.MSYS_CurrentButtons;

                if (this.MSYS_CurrRegion.uiFlags.HasFlag(MouseRegionFlags.REGION_ENABLED)
                    && this.MSYS_CurrRegion.uiFlags.HasFlag(MouseRegionFlags.MOVE_CALLBACK)
                    && this.MSYS_Action.HasFlag(MouseDos.MOVE))
                {
                    IVideoManager.DebugRenderer.DrawRectangle(this.MSYS_CurrRegion.Bounds, Color.Yellow);
                    this.MSYS_CurrRegion.MovementCallback?.Invoke(ref this.MSYS_CurrRegion, MouseCallbackReasons.MOVE);
                }

                //ExecuteMouseHelpEndCallBack( MSYS_CurrRegion );
                //MSYS_CurrRegion.FastHelpTimer = gsFastHelpDelay;

                this.MSYS_Action &= ~MouseDos.MOVE;

                if (this.MSYS_CurrRegion.uiFlags.HasFlag(MouseRegionFlags.BUTTON_CALLBACK)
                    && (this.MSYS_Action & MouseDos.BUTTONS) != 0)
                {
                    if (this.MSYS_CurrRegion.uiFlags.HasFlag(MouseRegionFlags.REGION_ENABLED))
                    {
                        ButtonReason = (int)MouseCallbackReasons.NONE;
                        if (this.MSYS_Action.HasFlag(MouseDos.LBUTTON_DWN))
                        {
                            ButtonReason |= (int)MouseCallbackReasons.LBUTTON_DWN;
                            this.gfClickedModeOn = true;
                            // Set global ID
                            this.gusClickedIDNumber = this.MSYS_CurrRegion.IDNumber;
                        }

                        if (this.MSYS_Action.HasFlag(MouseDos.LBUTTON_UP))
                        {
                            ButtonReason |= (int)MouseCallbackReasons.LBUTTON_UP;
                            this.gfClickedModeOn = false;
                        }

                        if (this.MSYS_Action.HasFlag(MouseDos.RBUTTON_DWN))
                        {
                            ButtonReason |= (int)MouseCallbackReasons.RBUTTON_DWN;
                            this.gfClickedModeOn = true;
                            // Set global ID
                            this.gusClickedIDNumber = this.MSYS_CurrRegion.IDNumber;
                        }

                        if (this.MSYS_Action.HasFlag(MouseDos.RBUTTON_UP))
                        {
                            ButtonReason |= (int)MouseCallbackReasons.RBUTTON_UP;
                            this.gfClickedModeOn = false;
                        }

                        // ATE: Added repeat resons....
                        if (this.MSYS_Action.HasFlag(MouseDos.LBUTTON_REPEAT))
                        {
                            ButtonReason |= (int)MouseCallbackReasons.LBUTTON_REPEAT;
                        }

                        if (this.MSYS_Action.HasFlag(MouseDos.RBUTTON_REPEAT))
                        {
                            ButtonReason |= (int)MouseCallbackReasons.RBUTTON_REPEAT;
                        }

                        if (ButtonReason != (int)MouseCallbackReasons.NONE)
                        {

                            if (this.MSYS_CurrRegion.uiFlags.HasFlag(MouseRegionFlags.FASTHELP))
                            {
                                // Button was clicked so remove any FastHelp text
                                this.MSYS_CurrRegion.uiFlags &= ~MouseRegionFlags.FASTHELP;

                                this.MSYS_CurrRegion.uiFlags &= ~MouseRegionFlags.GOT_BACKGROUND;

                                this.MSYS_CurrRegion.FastHelpTimer = this.gsFastHelpDelay;
                                this.MSYS_CurrRegion.uiFlags &= ~MouseRegionFlags.FASTHELP_RESET;
                            }

                            //Kris: Nov 31, 1999 -- Added support for double click events.
                            //This is where double clicks are checked and passed down.
                            if (ButtonReason == (int)MouseCallbackReasons.LBUTTON_DWN)
                            {
                                long uiCurrTime = this.clock.GetClock();
                                if (this.gpRegionLastLButtonDown == this.MSYS_CurrRegion &&
                                        this.gpRegionLastLButtonUp == this.MSYS_CurrRegion &&
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
                                    this.gpRegionLastLButtonDown = this.MSYS_CurrRegion;
                                    this.guiRegionLastLButtonDownTime = this.clock.GetClock();
                                }
                            }
                            else if (ButtonReason == (int)MouseCallbackReasons.LBUTTON_UP)
                            {
                                long uiCurrTime = this.clock.GetClock();
                                if (this.gpRegionLastLButtonDown == this.MSYS_CurrRegion
                                    && uiCurrTime <= this.guiRegionLastLButtonDownTime + MSYS_DOUBLECLICK_DELAY)
                                {
                                    //Double click is Left down, then left up, then left down.  We have just detected the left up here (step 2).
                                    this.gpRegionLastLButtonUp = this.MSYS_CurrRegion;
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
                            this.MSYS_CurrRegion.ButtonCallback?.Invoke(ref this.MSYS_CurrRegion, (MouseCallbackReasons)ButtonReason);
                        }
                    }
                }

                this.MSYS_Action &= ~MouseDos.BUTTONS;
            }
            else if (this.MSYS_CurrRegion.uiFlags.HasFlag(MouseRegionFlags.REGION_ENABLED))
            {
                // OK here, if we have release a button, UNSET LOCK wherever you are....
                // Just don't give this button the message....
                if (this.MSYS_Action.HasFlag(MouseDos.RBUTTON_UP))
                {
                    this.gfClickedModeOn = false;
                }

                if (this.MSYS_Action.HasFlag(MouseDos.LBUTTON_UP))
                {
                    this.gfClickedModeOn = false;
                }

                // OK, you still want move messages however....
                this.MSYS_CurrRegion.uiFlags |= MouseRegionFlags.MOUSE_IN_AREA;
                this.MSYS_CurrRegion.MousePos = this.MSYS_CurrentM;
                this.MSYS_CurrRegion.RelativeMousePos = new(
                    this.MSYS_CurrentM.X - this.MSYS_CurrRegion.Bounds.X,
                    this.MSYS_CurrentM.Y - this.MSYS_CurrRegion.Bounds.Y);

                if (this.MSYS_CurrRegion.uiFlags.HasFlag(MouseRegionFlags.MOVE_CALLBACK) && this.MSYS_Action.HasFlag(MouseDos.MOVE))
                {
                    this.MSYS_CurrRegion.MovementCallback?.Invoke(ref this.MSYS_CurrRegion, MouseCallbackReasons.MOVE);
                }

                this.MSYS_Action &= ~MouseDos.MOVE;
            }

            this.MSYS_PrevRegion = this.MSYS_CurrRegion;
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
        private void MSYS_SetCurrentCursor(Cursor cursor)
        {
            this.cursors.SetCurrentCursorFromDatabase(cursor);
        }

        public void MSYS_SetRegionUserData(MouseRegion region, int index, int userdata)
            => this.MSYS_SetRegionUserData(ref region, index, userdata);

        public void MSYS_SetRegionUserData(ref MouseRegion region, int index, int userdata)
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
        private void MSYS_TrashRegList()
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
            if (this.MSYS_PrevRegion == region)
            {
                this.MSYS_PrevRegion = null;
            }

            //if the current region is the one that we are deleting, then clear it.
            if (this.MSYS_CurrRegion == region)
            {
                this.MSYS_CurrRegion = null;
            }

            //dirty our update flag
            this.gfRefreshUpdate = true;

            // Check if this is a locked region, and unlock if so
            if (this.gfClickedModeOn)
            {
                // Set global ID
                if (this.gusClickedIDNumber == region.IDNumber)
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
        private void MSYS_AddRegionToList(ref MouseRegion region)
        {
            // Set an ID number!
            region.IDNumber = this.MSYS_GetNewID();

            this.Regions.Add(region);
        }

        public int MSYS_GetRegionUserData(ref MouseRegion reg, int index) => reg.UserData[index];

        //======================================================================================================
        //	MSYS_GetNewID
        //
        //	Returns a unique ID number for region nodes. If no new ID numbers can be found, the MAX value
        //	is returned.
        //
        private int MSYS_GetNewID() => this.Regions.Count;

        //=================================================================================================
        //	MSYS_DefineRegion
        //
        //	Inits a MOUSE_REGION structure for use with the mouse system
        //

        public void MSYS_DefineRegion(
            MouseRegion region,
            Rectangle bounds,
            MSYS_PRIORITY priority,
            Cursor crsr,
            MouseCallback? movecallback,
            MouseCallback? buttoncallback)
            => this.MSYS_DefineRegion(
                ref region,
                bounds,
                priority,
                crsr,
                movecallback,
                buttoncallback);


        public void MSYS_DefineRegion(
            ref MouseRegion region,
            Rectangle bounds,
            MSYS_PRIORITY priority,
            Cursor crsr,
            MouseCallback? movecallback,
            MouseCallback? buttoncallback)
        {
            region.IDNumber = MSYS_ID.BASE;

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
            if (movecallback is not null)
            {
                region.uiFlags |= MouseRegionFlags.MOVE_CALLBACK;
            }

            region.ButtonCallback = buttoncallback;
            if (buttoncallback is not null)
            {
                region.uiFlags |= MouseRegionFlags.BUTTON_CALLBACK;
            }

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
            region.uiFlags |= MouseRegionFlags.REGION_ENABLED | MouseRegionFlags.REGION_EXISTS;
            this.MSYS_AddRegionToList(ref region);

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
        public int IDNumber;                        // Region's ID number, set by mouse system
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

        public override string ToString()
        {
            return $"{this.Name}: {this.IDNumber}: {this.PriorityLevel}: {this.Bounds}";
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
        MOUSE_IN_AREA = 0x00000001,
        SET_CURSOR = 0x00000002,
        MOVE_CALLBACK = 0x00000004,
        BUTTON_CALLBACK = 0x00000008,
        REGION_EXISTS = 0x00000010,
        SYSTEM_INIT = 0x00000020,
        REGION_ENABLED = 0x00000040,
        FASTHELP = 0x00000080,
        GOT_BACKGROUND = 0x00000100,
        HAS_BACKRECT = 0x00000200,
        FASTHELP_RESET = 0x00000400,
        ALLOW_DISABLED_FASTHELP = 0x00000800,

        BASE_REGION_FLAGS = REGION_ENABLED | SET_CURSOR,
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
