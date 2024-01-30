using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SharpAlliance.Core.Interfaces;
using SharpAlliance.Core.Managers;
using SharpAlliance.Platform;
using Point = SixLabors.ImageSharp.Point;
using Rectangle = SixLabors.ImageSharp.Rectangle;

namespace SharpAlliance.Core;

public class InputManager : IInputManager
{
    private readonly ILogger<InputManager> logger;
    private static IVideoManager video;
    public MouseSubSystem Mouse { get; init; }
    public ButtonSubSystem buttonSystem { get; init; }
    private readonly GameContext context2;
    //private video video;
    private int[] gsKeyTranslationTable = new int[1024];
    private bool gfApplicationActive;
    private bool[] gfKeyState = new bool[256];            // true = Pressed, false = Not Pressed
    private bool fCursorWasClipped = false;
    private Rectangle gCursorClipRect;
    private int gfShiftState;                    // true = Pressed, false = Not Pressed
    private int gfAltState;                      // true = Pressed, false = Not Pressed
    private int gfCtrlState;                     // true = Pressed, false = Not Pressed

    private const int WH_MOUSE = 7;
    private const int WH_KEYBOARD = 2;

    // These data structure are used to track the mouse while polling

    private bool gfTrackDblClick;
    private int guiDoubleClkDelay;       // Current delay in milliseconds for a delay
    private int guiSingleClickTimer;
    private int guiRecordedWParam;
    private int guiRecordedLParam;
    private int gusRecordedKeyState;
    private bool gfRecordedLeftButtonUp;

    private long guiLeftButtonRepeatTimer;
    private long guiRightButtonRepeatTimer;

    private bool gfTrackMousePos;            // true = queue mouse movement events, false = don't
    public bool gfLeftButtonState { get; set; }      // true = Pressed, false = Not Pressed
    public bool gfRightButtonState { get; set; }     // true = Pressed, false = Not Pressed
    public Point gusMousePos { get; set; }                    // position of the mouse on screen

    private const int DBL_CLK_TIME = 300;     // Increased by Alex, Jun-10-97, 200 felt too short
    private const int BUTTON_REPEAT_TIMEOUT = 250;
    private const long BUTTON_REPEAT_TIME = 50;


    // The queue structures are used to track input events using queued events

    private Queue<IInputSnapshot> gEventQueue = new(256);

    // ATE: Added to signal if we have had input this frame - cleared by the SGP main loop
    private bool gfSGPInputReceived = false;

    // This is the WIN95 hook specific data and defines used to handle the keyboard and
    // mouse hook

    //HHOOK ghKeyboardHook;
    //HHOOK ghMouseHook;

    // If the following pointer is non null then input characters are redirected to
    // the related string

    private bool gfCurrentStringInputState;
    private IntPtr hInstance;
    //private HookProcedureHandle ghKeyboardHook;
    private object ghMouseHook;
    private System.Numerics.Vector2 lastMousePos;

    public bool IsInitialized { get; private set; }

    //StringInput* gpCurrentStringDescriptor;

    public InputManager(
        ILogger<InputManager> logger,
        IVideoManager videoManager,
        MouseSubSystem mouseSubSystem,
        ButtonSubSystem buttonSubsystem)
    {
        this.logger = logger;
        video = videoManager;
        this.Mouse = mouseSubSystem;
        this.buttonSystem = buttonSubsystem;
    }

    public void GetCursorPosition(out Point mousePos)
    {
        var pos = InputTracker.MousePosition;
        mousePos = new Point((int)pos.X, (int)pos.Y);
    }

    public void GetMousePos(out Point Point)
    {
        GetCursorPos(out Point MousePos);

        Point = new()
        {
            X = MousePos.X,
            Y = MousePos.Y,
        };

        return;
    }

    public async ValueTask<bool> Initialize()
    {
        if (this.IsInitialized)
        {
            return true;
        }

        //video = (this.context.Services.GetRequiredService<IVideoManager>() as video)!;

        // By default, we will not queue mousemove events
        this.gfTrackMousePos = false;
        // Initialize other variables
        this.gfShiftState = 0;
        this.gfAltState = 0;
        this.gfCtrlState = 0;
        // Initialize variables pertaining to DOUBLE CLIK stuff
        this.gfTrackDblClick = true;
        this.guiDoubleClkDelay = 300;
        this.guiSingleClickTimer = 0;
        this.gfRecordedLeftButtonUp = false;
        // Initialize variables pertaining to the button states
        this.gfLeftButtonState = false;
        this.gfRightButtonState = false;
        // Initialize variables pertaining to the repeat mechanism
        this.guiLeftButtonRepeatTimer = 0;
        this.guiRightButtonRepeatTimer = 0;
        // Set the mouse to the center of the screen
        this.gusMousePos = new(320, 240);

        // Initialize the string input mechanism
        this.gfCurrentStringInputState = false;

        ////gpCurrentStringDescriptor = null;

        this.IsInitialized = await this.buttonSystem.Initialize(this.Mouse, this);

        return this.IsInitialized;
    }

    public void ProcessEvents()
    {
        try
        {
            var enqueue = false;
            IInputSnapshot snapshot = video.Window.PumpEvents();

            InputTracker.UpdateFrameInput(snapshot);

            var tmpLeft = snapshot.IsMouseDown(MouseButton.Left);
            var tmpRight = snapshot.IsMouseDown(MouseButton.Right);

            if (tmpLeft != this.gfLeftButtonState)
            {
                this.gfLeftButtonState = tmpLeft;
                enqueue = true;
            }

            if (tmpRight != this.gfRightButtonState)
            {
                this.gfRightButtonState = tmpRight;
                enqueue = true;
            }

            if (this.lastMousePos != snapshot.MousePosition)
            {
                this.lastMousePos = snapshot.MousePosition;
                enqueue = true;
            }

            if (snapshot.KeyEvents.Any() || snapshot.KeyCharPresses.Any())
            {
                enqueue = true;
            }

            if (enqueue)
            {
                this.gEventQueue.Enqueue(snapshot);
            }
        }
        catch
        {
            // threaded input can throw in sdl2 library.
            // TODO: bug it and work on getting it fixed.
            this.logger.LogDebug(LoggingEventId.INPUT, "Exception from PumpEvents");
        }
    }

    public bool DequeSpecificEvent(out IInputSnapshot inputSnapshot)
    {
        if (this.gEventQueue.Any())
        {
            inputSnapshot = this.gEventQueue.Peek();
            if (inputSnapshot is not null)
            {
                var copyInputAtom = inputSnapshot;
                if (copyInputAtom.MouseEvents.Count >= 1)
                {
                    if (copyInputAtom.MouseEvents.Any())
                    {
                        return this.DequeueEvent(out inputSnapshot);
                    }
                }

                return false;
            }
        }

        inputSnapshot = default!;
        return false;
    }

    public MouseEvents ConvertToMouseEvents(ref IInputSnapshot inputSnapshot)
    {
        var input = inputSnapshot.MouseEvents.First();

        MouseEvents mouseEvent = MouseEvents.Unknown;
        if (!input.Down && input.MouseButton == MouseButton.Left)
        {

        }

        if (input.MouseButton == MouseButton.Left)
        {
            mouseEvent |= input.Down
                ? MouseEvents.LEFT_BUTTON_DOWN
                : MouseEvents.LEFT_BUTTON_UP;
        }
        else if (input.MouseButton == MouseButton.Right)
        {
            mouseEvent |= input.Down
                ? MouseEvents.RIGHT_BUTTON_DOWN
                : MouseEvents.RIGHT_BUTTON_UP;
        }

        return mouseEvent;
    }

    public bool DequeueEvent(out IInputSnapshot inputSnapshot)
    {
        this.HandleSingleClicksAndButtonRepeats();

        // Is there an event to dequeue
        if (this.gEventQueue.Any())
        {
            // We have an event, so we dequeue it
            inputSnapshot = this.gEventQueue.Dequeue();

            // dequeued an event, return true
            return true;
        }
        else
        {
            inputSnapshot = default!;
            return false;
        }
    }


    public void KeyboardChangeEvent(KeyEvent keyEvent)
    {
    }

    public void MouseChangeEvent(MouseEvent mouseEvent)
    {
    }

    private void HandleSingleClicksAndButtonRepeats()
    {
        long uiTimer = ClockManager.GetTickCount();

        // Is there a LEFT mouse button repeat
        if (this.gfLeftButtonState)
        {
            if ((this.guiLeftButtonRepeatTimer > 0) && (this.guiLeftButtonRepeatTimer <= uiTimer))
            {

                GetCursorPos(out Point MousePos);

                this.QueueEvent(MouseEvents.LEFT_BUTTON_REPEAT, null, MousePos);
                this.guiLeftButtonRepeatTimer = uiTimer + BUTTON_REPEAT_TIME;
            }
        }
        else
        {
            this.guiLeftButtonRepeatTimer = 0;
        }


        // Is there a RIGHT mouse button repeat
        if (this.gfRightButtonState)
        {
            if ((this.guiRightButtonRepeatTimer > 0) && (this.guiRightButtonRepeatTimer <= uiTimer))
            {

                GetCursorPos(out Point MousePos);
                this.QueueEvent(MouseEvents.RIGHT_BUTTON_REPEAT, 0, MousePos);
                this.guiRightButtonRepeatTimer = uiTimer + BUTTON_REPEAT_TIME;
            }
        }
        else
        {
            this.guiRightButtonRepeatTimer = 0;
        }

    }

    private void QueueEvent(MouseEvents mouseEvents, object? parameterOne, object? parameterTwo)
    {

    }

    public void Dispose()
    {
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct LPPOINT
    {
        public long x;
        public long y;
    }

    [return: MarshalAs(UnmanagedType.Bool)]
    [DllImport("user32.dll", ExactSpelling = true)]
    public static extern bool GetCursorPos(out Point lpPoint);

    public void DequeueAllKeyBoardEvents()
    {
    }

    public bool HandleTextInput(IInputSnapshot @event)
    {
        throw new NotImplementedException();
    }
}
