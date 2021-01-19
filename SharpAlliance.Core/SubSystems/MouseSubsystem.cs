using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using SharpAlliance.Core.Managers;
using SharpAlliance.Platform;
using SharpAlliance.Platform.Interfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Veldrid;
using Rectangle = SixLabors.ImageSharp.Rectangle;

namespace SharpAlliance.Core.SubSystems
{
    public class MouseSubSystem : IDisposable
    {
        private readonly ILogger<MouseSubSystem> logger;
        private readonly IClockManager clock;
        private readonly ButtonSubSystem buttons;
        private readonly CursorSubSystem cursors;
        private const int MSYS_DOUBLECLICK_DELAY = 400;

        //Records and stores the last place the user clicked.  These values are compared to the current
        //click to determine if a double click event has been detected.
        private MouseRegion? gpRegionLastLButtonDown = null;
        private MouseRegion? gpRegionLastLButtonUp = null;
        private long guiRegionLastLButtonDownTime = 0;

        private bool MSYS_ScanForID = false;
        private int MSYS_CurrentID = MSYS_ID.SYSTEM;

        private int MSYS_CurrentMX = 0;
        private int MSYS_CurrentMY = 0;
        private ButtonMasks MSYS_CurrentButtons;
        private MouseDos MSYS_Action = 0;

        private bool MSYS_SystemInitialized = false;
        private bool MSYS_UseMouseHandlerHook = false;

        private bool MSYS_Mouse_Grabbed = false;
        private MouseRegion? MSYS_GrabRegion = null;

        private int gusClickedIDNumber;
        private bool gfClickedModeOn = false;

        private MouseRegion? MSYS_RegList = null;

        private MouseRegion? MSYS_PrevRegion = null;
        private MouseRegion? MSYS_CurrRegion = null;

        //When set, the fast help text will be instantaneous, if consecutive regions with help text are
        //hilighted.  It is set, whenever the timer for the first help button expires, and the mode is
        //cleared as soon as the cursor moves into no region or a region with no helptext.
        private bool gfPersistantFastHelpMode;

        private int gsFastHelpDelay = 600; // In timer ticks
        private bool gfShowFastHelp = true;

        MouseRegion MSYS_SystemBaseRegion = new()
        {
            IDNumber = MSYS_ID.SYSTEM,
            PriorityLevel = MSYS_PRIORITY.SYSTEM,
            uiFlags = MouseRegionFlags.BASE_REGION_FLAGS,
            RegionTopLeftX = -32767,
            RegionTopLeftY = -32767,
            RegionBottomRightX = 32767,
            RegionBottomRightY = 32767,
            MouseXPos = 0,
            MouseYPos = 0,
            RelativeXPos = 0,
            RelativeYPos = 0,
            ButtonState = 0,
            Cursor = 0,
            MovementCallback = null,
            ButtonCallback = null,
            UserData = new[] { 0, 0, 0, 0 },
            FastHelpTimer = 0,
            FastHelpText = 0,
            FastHelpRect = -1,
            HelpDoneCallback = null,// MouseCallbackReasons.NO_CALLBACK, 
            next = null,
            prev = null,
        };

        public bool gfRefreshUpdate = false;

        public Image<Rgba32> gpMouseCursor { get; set; }
        public Image<Rgba32> gpMouseCursorOriginal { get; set; }

        public MouseSubSystem(
            ILogger<MouseSubSystem> logger,
            IClockManager clockManager,
            ButtonSubSystem buttonManager,
            CursorSubSystem cursorSubSystem)
        {
            this.logger = logger;

            this.logger.LogDebug(LoggingEventId.MouseSystem, "Mouse Region System");
            this.clock = clockManager;
            this.buttons = buttonManager;
            this.cursors = cursorSubSystem;

            if (this.MSYS_RegList is not null)
            {
                this.MSYS_TrashRegList();
            }

            this.MSYS_CurrentID = MSYS_ID.SYSTEM;
            this.MSYS_ScanForID = false;

            this.MSYS_CurrentMX = 0;
            this.MSYS_CurrentMY = 0;
            this.MSYS_CurrentButtons = 0;
            this.MSYS_Action = MouseDos.ACTION;

            this.MSYS_PrevRegion = null;
            this.MSYS_SystemInitialized = true;
            this.MSYS_UseMouseHandlerHook = false;

            this.MSYS_Mouse_Grabbed = false;
            this.MSYS_GrabRegion = null;

            // Setup the system's background region
            this.MSYS_SystemBaseRegion.IDNumber = MSYS_ID.SYSTEM;
            this.MSYS_SystemBaseRegion.PriorityLevel = MSYS_PRIORITY.SYSTEM;
            this.MSYS_SystemBaseRegion.uiFlags = MouseRegionFlags.BASE_REGION_FLAGS;
            this.MSYS_SystemBaseRegion.RegionTopLeftX = -32767;
            this.MSYS_SystemBaseRegion.RegionTopLeftY = -32767;
            this.MSYS_SystemBaseRegion.RegionBottomRightX = 32767;
            this.MSYS_SystemBaseRegion.RegionBottomRightY = 32767;
            this.MSYS_SystemBaseRegion.MouseXPos = 0;
            this.MSYS_SystemBaseRegion.MouseYPos = 0;
            this.MSYS_SystemBaseRegion.RelativeXPos = 0;
            this.MSYS_SystemBaseRegion.RelativeYPos = 0;
            this.MSYS_SystemBaseRegion.ButtonState = 0;
            this.MSYS_SystemBaseRegion.Cursor = 0;
            this.MSYS_SystemBaseRegion.UserData[0] = 0;
            this.MSYS_SystemBaseRegion.UserData[1] = 0;
            this.MSYS_SystemBaseRegion.UserData[2] = 0;
            this.MSYS_SystemBaseRegion.UserData[3] = 0;
            this.MSYS_SystemBaseRegion.MovementCallback = null;
            this.MSYS_SystemBaseRegion.ButtonCallback = null;

            this.MSYS_SystemBaseRegion.FastHelpTimer = 0;
            this.MSYS_SystemBaseRegion.FastHelpText = 0;
            this.MSYS_SystemBaseRegion.FastHelpRect = -1;

            this.MSYS_SystemBaseRegion.next = null;
            this.MSYS_SystemBaseRegion.prev = null;

            this.MSYS_AddRegionToList(ref this.MSYS_SystemBaseRegion);

            this.MSYS_UseMouseHandlerHook = true;
        }

        public void MouseHook(MouseEvents mouseEvent, int Xcoord, int Ycoord, bool leftButtonDown, bool rightButtonDown)
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

            this.MSYS_Action = MouseDos.ACTION;
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

                        this.buttons.ReleaseAnchorMode();
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
                        this.MSYS_CurrentButtons &= (~ButtonMasks.MSYS_LEFT_BUTTON);
                    }

                    if (rightButtonDown)
                    {
                        this.MSYS_CurrentButtons |= ButtonMasks.MSYS_RIGHT_BUTTON;
                    }
                    else
                    {
                        this.MSYS_CurrentButtons &= (~ButtonMasks.MSYS_RIGHT_BUTTON);
                    }

                    if ((Xcoord != this.MSYS_CurrentMX) || (Ycoord != this.MSYS_CurrentMY))
                    {
                        this.MSYS_Action |= MouseDos.MOVE;
                        this.MSYS_CurrentMX = Xcoord;
                        this.MSYS_CurrentMY = Ycoord;
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

                    if ((Xcoord != this.MSYS_CurrentMX) || (Ycoord != this.MSYS_CurrentMY))
                    {
                        this.MSYS_Action |= MouseDos.MOVE;
                        this.MSYS_CurrentMX = Xcoord;
                        this.MSYS_CurrentMY = Ycoord;
                    }

                    this.MSYS_UpdateMouseRegion();
                    break;

                case MouseEvents.MousePosition:
                    if ((Xcoord != this.MSYS_CurrentMX) || (Ycoord != this.MSYS_CurrentMY) || this.gfRefreshUpdate)
                    {
                        this.MSYS_Action |= MouseDos.MOVE;
                        this.MSYS_CurrentMX = Xcoord;
                        this.MSYS_CurrentMY = Ycoord;

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
            bool found = false;
            int ButtonReason;
            MouseRegion pTempRegion;
            bool fFound = false;

            // Check previous region!
            if (this.MSYS_Mouse_Grabbed)
            {
                this.MSYS_CurrRegion = this.MSYS_GrabRegion;
                found = true;
            }
            if (!found)
            {
                this.MSYS_CurrRegion = this.MSYS_RegList;
            }

            while (!found && this.MSYS_CurrRegion is not null)
            {
                if (this.MSYS_CurrRegion.uiFlags.HasFlag(MouseRegionFlags.REGION_ENABLED | MouseRegionFlags.ALLOW_DISABLED_FASTHELP)
                    && (this.MSYS_CurrRegion.RegionTopLeftX <= this.MSYS_CurrentMX)       // Check boundaries
                    && (this.MSYS_CurrRegion.RegionTopLeftY <= this.MSYS_CurrentMY)
                    && (this.MSYS_CurrRegion.RegionBottomRightX >= this.MSYS_CurrentMX)
                    && (this.MSYS_CurrRegion.RegionBottomRightY >= this.MSYS_CurrentMY))
                {
                    // We got the right region. We don't need to check for priorities 'cause
                    // the whole list is sorted the right way!
                    found = true;
                }
                else
                {
                    this.MSYS_CurrRegion = this.MSYS_CurrRegion.next;
                }
            }

            if (this.MSYS_PrevRegion is not null)
            {
                this.MSYS_PrevRegion.uiFlags &= (~MouseRegionFlags.MOUSE_IN_AREA);

                if (this.MSYS_PrevRegion != this.MSYS_CurrRegion)
                {
                    //Remove the help text for the previous region if one is currently being displayed.
                    if (this.MSYS_PrevRegion.FastHelpText is not null)
                    {
                        //ExecuteMouseHelpEndCallBack( MSYS_PrevRegion );

                        //# ifdef _JA2_RENDER_DIRTY
                        //                        if (MSYS_PrevRegion.uiFlags & MSYS_GOT_BACKGROUND)
                        //                        {
                        //                            FreeBackgroundRectPending(MSYS_PrevRegion.FastHelpRect);
                        //                        }
                        //#endif
                        this.MSYS_PrevRegion.uiFlags &= (~MouseRegionFlags.GOT_BACKGROUND);
                        this.MSYS_PrevRegion.uiFlags &= (~MouseRegionFlags.FASTHELP_RESET);

                        //if( region.uiFlags & MSYS_REGION_ENABLED )
                        //	region.uiFlags |= BUTTON_DIRTY;
                        //# ifndef JA2			
                        //                        VideoRemoveToolTip();
                        //#endif
                    }

                    this.MSYS_CurrRegion.FastHelpTimer = this.gsFastHelpDelay;

                    // Force a callbacks to happen on previous region to indicate that
                    // the mouse has left the old region
                    if (this.MSYS_PrevRegion.uiFlags.HasFlag(MouseRegionFlags.MOVE_CALLBACK)
                        && this.MSYS_PrevRegion.uiFlags.HasFlag(MouseRegionFlags.REGION_ENABLED))
                    {
                        this.MSYS_PrevRegion.MovementCallback(this.MSYS_PrevRegion, MouseCallbackReasons.LOST_MOUSE);
                    }
                }
            }

            // If a region was found in the list, update it's data
            if (found)
            {
                if (this.MSYS_CurrRegion != this.MSYS_PrevRegion)
                {
                    //Kris -- October 27, 1997
                    //Implemented gain mouse region

                    if (this.MSYS_CurrRegion.uiFlags.HasFlag(MouseRegionFlags.MOVE_CALLBACK))
                    {
                        if (this.MSYS_CurrRegion.FastHelpText is not null
                            && !(this.MSYS_CurrRegion.uiFlags.HasFlag(MouseRegionFlags.FASTHELP_RESET)))
                        {
                            //ExecuteMouseHelpEndCallBack( MSYS_CurrRegion );
                            this.MSYS_CurrRegion.FastHelpTimer = this.gsFastHelpDelay;
                            //# ifdef _JA2_RENDER_DIRTY
                            //                            if (MSYS_CurrRegion.uiFlags & MSYS_GOT_BACKGROUND)
                            //                                FreeBackgroundRectPending(MSYS_CurrRegion.FastHelpRect);
                            //#endif
                            this.MSYS_CurrRegion.uiFlags &= (~MouseRegionFlags.GOT_BACKGROUND);
                            this.MSYS_CurrRegion.uiFlags |= MouseRegionFlags.FASTHELP_RESET;

                            //if( b.uiFlags & BUTTON_ENABLED )
                            //	b.uiFlags |= BUTTON_DIRTY;
                        }
                        if (this.MSYS_CurrRegion.uiFlags.HasFlag(MouseRegionFlags.REGION_ENABLED))
                        {
                            this.MSYS_CurrRegion.MovementCallback(this.MSYS_CurrRegion, MouseCallbackReasons.GAIN_MOUSE);
                        }
                    }

                    // if the cursor is set and is not set to no cursor
                    if (this.MSYS_CurrRegion.uiFlags.HasFlag(MouseRegionFlags.REGION_ENABLED) &&
                                  this.MSYS_CurrRegion.uiFlags.HasFlag(MouseRegionFlags.SET_CURSOR) &&
                                  this.MSYS_CurrRegion.Cursor != 0)
                    {
                        this.MSYS_SetCurrentCursor(this.MSYS_CurrRegion.Cursor);
                    }
                    else
                    {
                        // Addition Oct 10/1997 Carter, patch for mouse cursor
                        // start at region and find another region encompassing
                        pTempRegion = this.MSYS_CurrRegion.next;
                        while ((pTempRegion != null) && (!fFound))
                        {
                            if ((pTempRegion.uiFlags.HasFlag(MouseRegionFlags.REGION_ENABLED)) &&
                                   (pTempRegion.RegionTopLeftX <= this.MSYS_CurrentMX) &&
                                   (pTempRegion.RegionTopLeftY <= this.MSYS_CurrentMY) &&
                                   (pTempRegion.RegionBottomRightX >= this.MSYS_CurrentMX) &&
                                   (pTempRegion.RegionBottomRightY >= this.MSYS_CurrentMY) && (pTempRegion.uiFlags.HasFlag(MouseRegionFlags.SET_CURSOR)))
                            {
                                fFound = true;
                                if (pTempRegion.Cursor != 0)//MSYS_NO_CURSOR)
                                {
                                    this.MSYS_SetCurrentCursor(pTempRegion.Cursor);
                                }
                            }
                            pTempRegion = pTempRegion.next;
                        }
                    }
                }

                // OK, if we do not have a button down, any button is game!
                if (!this.gfClickedModeOn || (this.gfClickedModeOn && this.gusClickedIDNumber == this.MSYS_CurrRegion.IDNumber))
                {
                    this.MSYS_CurrRegion.uiFlags |= MouseRegionFlags.MOUSE_IN_AREA;

                    this.MSYS_CurrRegion.MouseXPos = this.MSYS_CurrentMX;
                    this.MSYS_CurrRegion.MouseYPos = this.MSYS_CurrentMY;
                    this.MSYS_CurrRegion.RelativeXPos = this.MSYS_CurrentMX - this.MSYS_CurrRegion.RegionTopLeftX;
                    this.MSYS_CurrRegion.RelativeYPos = this.MSYS_CurrentMY - this.MSYS_CurrRegion.RegionTopLeftY;

                    this.MSYS_CurrRegion.ButtonState = this.MSYS_CurrentButtons;

                    if (this.MSYS_CurrRegion.uiFlags.HasFlag(MouseRegionFlags.REGION_ENABLED)
                        && this.MSYS_CurrRegion.uiFlags.HasFlag(MouseRegionFlags.MOVE_CALLBACK)
                        && this.MSYS_Action.HasFlag(MouseDos.MOVE))
                    {
                        this.MSYS_CurrRegion.ButtonCallback(this.MSYS_CurrRegion, MouseCallbackReasons.MOVE);
                    }

                    //ExecuteMouseHelpEndCallBack( MSYS_CurrRegion );
                    //MSYS_CurrRegion.FastHelpTimer = gsFastHelpDelay;

                    this.MSYS_Action &= (~MouseDos.MOVE);

                    if ((this.MSYS_CurrRegion.uiFlags.HasFlag(MouseRegionFlags.BUTTON_CALLBACK)) && (this.MSYS_Action.HasFlag(MouseDos.BUTTONS)))
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
                                    this.MSYS_CurrRegion.uiFlags &= (~MouseRegionFlags.FASTHELP);
                                    //# ifdef _JA2_RENDER_DIRTY
                                    //                                    if (MSYS_CurrRegion.uiFlags & MSYS_GOT_BACKGROUND)
                                    //                                        FreeBackgroundRectPending(MSYS_CurrRegion.FastHelpRect);
                                    //#endif

                                    this.MSYS_CurrRegion.uiFlags &= (~MouseRegionFlags.GOT_BACKGROUND);

                                    //ExecuteMouseHelpEndCallBack( MSYS_CurrRegion );
                                    this.MSYS_CurrRegion.FastHelpTimer = this.gsFastHelpDelay;
                                    this.MSYS_CurrRegion.uiFlags &= (~MouseRegionFlags.FASTHELP_RESET);

                                    //if( b.uiFlags & BUTTON_ENABLED )
                                    //	b.uiFlags |= BUTTON_DIRTY;
                                    // VideoRemoveToolTip();
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
                                    if (this.gpRegionLastLButtonDown == this.MSYS_CurrRegion &&
                                            uiCurrTime <= this.guiRegionLastLButtonDownTime + MSYS_DOUBLECLICK_DELAY)
                                    { //Double click is Left down, then left up, then left down.  We have just detected the left up here (step 2).
                                        this.gpRegionLastLButtonUp = this.MSYS_CurrRegion;
                                    }
                                    else
                                    { //User released mouse outside of current button, so kill any chance of a double click happening.
                                        this.gpRegionLastLButtonDown = null;
                                        this.gpRegionLastLButtonUp = null;
                                        this.guiRegionLastLButtonDownTime = 0;
                                    }
                                }

                                // TODO: Cast to MouseCallbackReasons shouldn't be here, move to two sep callbacks.
                                this.MSYS_CurrRegion.ButtonCallback(this.MSYS_CurrRegion, (MouseCallbackReasons)ButtonReason);
                            }
                        }
                    }

                    this.MSYS_Action &= (~MouseDos.BUTTONS);
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
                    this.MSYS_CurrRegion.MouseXPos = this.MSYS_CurrentMX;
                    this.MSYS_CurrRegion.MouseYPos = this.MSYS_CurrentMY;
                    this.MSYS_CurrRegion.RelativeXPos = this.MSYS_CurrentMX - this.MSYS_CurrRegion.RegionTopLeftX;
                    this.MSYS_CurrRegion.RelativeYPos = this.MSYS_CurrentMY - this.MSYS_CurrRegion.RegionTopLeftY;

                    if ((this.MSYS_CurrRegion.uiFlags.HasFlag(MouseRegionFlags.MOVE_CALLBACK)) && (this.MSYS_Action.HasFlag(MouseDos.MOVE)))
                    {
                        this.MSYS_CurrRegion.MovementCallback(this.MSYS_CurrRegion, MouseCallbackReasons.MOVE);
                    }

                    this.MSYS_Action &= (~MouseDos.MOVE);
                }
                this.MSYS_PrevRegion = this.MSYS_CurrRegion;
            }
            else
            {
                this.MSYS_PrevRegion = null;
            }
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
        private void MSYS_SetCurrentCursor(int Cursor)
        {
            this.cursors.SetCurrentCursorFromDatabase(Cursor);
        }

        //======================================================================================================
        //	MSYS_TrashRegList
        //
        //	Deletes the entire region list.
        //
        private void MSYS_TrashRegList()
        {
            while (this.MSYS_RegList is not null)
            {
                if (this.MSYS_RegList.uiFlags.HasFlag(MouseRegionFlags.REGION_EXISTS))
                {
                    this.MSYS_RemoveRegion(ref this.MSYS_RegList);
                }
                else
                {
                    this.MSYS_RegList = this.MSYS_RegList.next;
                }
            }
        }

        //=================================================================================================
        //	MSYS_RemoveRegion
        //
        //	Removes a region from the list, disables it, then calls the callback functions for
        //	de-initialization.
        //
        void MSYS_RemoveRegion(ref MouseRegion? region)
        {
            if (region is null)
            {
#if MOUSESYSTEM_DEBUGGING
                if (gfIgnoreShutdownAssertions)
#endif
                return;
                //AssertMsg(0, "Attempting to remove a null region.");
            }

            if (!(region.uiFlags.HasFlag(MouseRegionFlags.REGION_EXISTS)))
            {
                // AssertMsg(0, "Attempting to remove an already removed region.");
            }

            //# ifdef _JA2_RENDER_DIRTY
            //            if (region.uiFlags & MSYS_HAS_BACKRECT)
            //            {
            //                FreeBackgroundRectPending(region.FastHelpRect);
            //                region.uiFlags &= (~MSYS_HAS_BACKRECT);


            //            }
            //#endif

            // Get rid of the FastHelp text (if applicable)
            if (region.FastHelpText.HasValue)
            {
                //MemFree(region.FastHelpText);
            }

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
            MouseRegion curr;
            bool done;

            // If region seems to already be in list, delete it so we can
            // re-insert the region.
            if (region.next is not null || region.prev is not null)
            { // if it wasn't actually there, then call does nothing!
                this.MSYS_DeleteRegionFromList(ref region);
            }

            // Set an ID number!
            region.IDNumber = this.MSYS_GetNewID();

            region.next = null;
            region.prev = null;

            if (this.MSYS_RegList is null)
            { // Null list, so add it straight up.
                this.MSYS_RegList = region;
            }
            else
            {
                // Walk down list until we find place to insert (or at end of list)
                curr = this.MSYS_RegList!;
                done = false;
                while ((curr.next != null) && !done)
                {
                    if (curr.PriorityLevel <= region.PriorityLevel)
                    {
                        done = true;
                    }
                    else
                    {
                        curr = curr.next;
                    }
                }

                if (curr.PriorityLevel > region.PriorityLevel)
                {
                    // Add after curr node
                    region.next = curr.next;
                    curr.next = region;
                    region.prev = curr;
                    if (region.next != null)
                    {
                        region.next.prev = region;
                    }
                }
                else
                {
                    // Add before curr node
                    region.next = curr;
                    region.prev = curr.prev;

                    curr.prev = region;
                    if (region.prev != null)
                    {
                        region.prev.next = region;
                    }

                    if (this.MSYS_RegList == curr)   // Make sure if adding at start, to adjust the list pointer
                    {
                        this.MSYS_RegList = region;
                    }
                }
            }
        }


        //======================================================================================================
        //	MSYS_GetNewID
        //
        //	Returns a unique ID number for region nodes. If no new ID numbers can be found, the MAX value
        //	is returned.
        //
        private int MSYS_GetNewID()
        {
            int retID;
            int Current;
            bool found;
            bool done;
            MouseRegion? node;

            retID = this.MSYS_CurrentID;
            this.MSYS_CurrentID++;

            // Crapy scan for an unused ID
            if ((this.MSYS_CurrentID >= MSYS_ID.MAX) || this.MSYS_ScanForID)
            {
                this.MSYS_ScanForID = true;
                Current = MSYS_ID.BASE;
                done = found = false;
                while (!done)
                {
                    found = false;
                    node = this.MSYS_RegList;
                    while (node != null && !found)
                    {
                        if (node.IDNumber == Current)
                        {
                            found = true;
                        }
                    }

                    if (found && Current < MSYS_ID.MAX) // Current ID is in use, and their are more to scan
                    {
                        Current++;
                    }
                    else
                    {
                        done = true;                        // Got an ID to use.
                        if (found)
                        {
                            Current = MSYS_ID.MAX;      // Ooops, ran out of IDs, use MAX value!
                        }
                    }
                }
            
                this.MSYS_CurrentID = Current;
            }

            return (retID);
        }


        //======================================================================================================
        //	MSYS_RegionInList
        //
        //	Scan region list for presence of a node with the same region ID number
        //
        private bool MSYS_RegionInList(ref MouseRegion region)
        {
            MouseRegion? Current;
            bool found;

            found = false;
            Current = this.MSYS_RegList;
            while (Current is not null && !found)
            {
                if (Current.IDNumber == region.IDNumber)
                {
                    found = true;
                }

                Current = Current.next;
            }

            return found;
        }

        //======================================================================================================
        //	MSYS_DeleteRegionFromList
        //
        //	Removes a region from the current list.
        //
        private void MSYS_DeleteRegionFromList(ref MouseRegion region)
        {
            // If no list present, there's nothin' to do.
            if (this.MSYS_RegList is null)
            {
                return;
            }

            // Check if region in list
            if (!this.MSYS_RegionInList(ref region))
            {
                return;
            }

            // Remove a node from the list
            if (this.MSYS_RegList == region)
            { // First node on list, adjust main pointer.
                this.MSYS_RegList = region.next;
                if (this.MSYS_RegList != null)
                {
                    this.MSYS_RegList.prev = null;
                }

                region.next = region.prev = null;
            }
            else
            {
                if (region.prev is not null)
                {
                    region.prev.next = region.next;
                }

                // If not last node in list, adjust following node's .prev entry.
                if (region.next is not null)
                {
                    region.next.prev = region.prev;
                }

                region.prev = region.next = null;
            }

            // Did we delete a grabbed region?
            if (this.MSYS_Mouse_Grabbed)
            {
                if (this.MSYS_GrabRegion == region)
                {
                    this.MSYS_Mouse_Grabbed = false;
                    this.MSYS_GrabRegion = null;
                }
            }

            // Is only the system background region remaining?
            if (this.MSYS_RegList == this.MSYS_SystemBaseRegion)
            {
                // Yup, so let's reset the ID values!
                this.MSYS_CurrentID = MSYS_ID.BASE;
                this.MSYS_ScanForID = false;
            }
            else if (this.MSYS_RegList is null)
            {
                // Ack, we actually emptied the list, so let's reset for re-init possibilities
                this.MSYS_CurrentID = MSYS_ID.SYSTEM;
                this.MSYS_ScanForID = false;
            }
        }

        public void Dispose()
        {
        }
    }

    public class MouseRegion
    {
        public int IDNumber;                        // Region's ID number, set by mouse system
        public MSYS_PRIORITY PriorityLevel;         // Region's Priority, set by system and/or caller
        public MouseRegionFlags uiFlags;                     // Region's state flags
        public int RegionTopLeftX;           // Screen area affected by this region (absolute coordinates)
        public int RegionTopLeftY;
        public int RegionBottomRightX;
        public int RegionBottomRightY;
        public int MouseXPos;                    // Mouse's Coordinates in absolute screen coordinates
        public int MouseYPos;
        public int RelativeXPos;             // Mouse's Coordinates relative to the Top-Left corner of the region
        public int RelativeYPos;
        public ButtonMasks ButtonState;             // Current state of the mouse buttons
        public int Cursor;                          // Cursor to use when mouse in this region (see flags)
        public MouseCallback? MovementCallback;        // Pointer to callback function if movement occured in this region
        public MouseCallback? ButtonCallback;      // Pointer to callback function if button action occured in this region
        public int[] UserData = new int[4];        // User Data, can be set to anything!

        //Fast help vars.
        public int FastHelpTimer;        // Countdown timer for FastHelp text
        public int? FastHelpText;       // Text string for the FastHelp (describes buttons if left there a while)
        public int FastHelpRect;
        public MOUSE_HELPTEXT_DONE_CALLBACK? HelpDoneCallback;

        public MouseRegion? next;                           // List maintenance, do NOT touch these entries
        public MouseRegion? prev;
    }

    [Flags]
    public enum ButtonMasks
    {
        // Mouse system button masks
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
        ACTION = 0,
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
        public Image<Rgba32> _pSurface;
        public Image<Rgba32>  pSurface;
    }

    public delegate void MouseCallback(MouseRegion region, MouseCallbackReasons callbackReason);
    public delegate void MOUSE_HELPTEXT_DONE_CALLBACK();
}
