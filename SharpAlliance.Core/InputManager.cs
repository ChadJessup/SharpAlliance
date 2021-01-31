using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SharpAlliance.Core.Interfaces;
using SharpAlliance.Core.Managers;
using SharpAlliance.Core.SubSystems;
using SharpAlliance.Platform;
using SharpAlliance.Platform.Interfaces;
using Veldrid;
using Veldrid.Sdl2;

namespace SharpAlliance.Core
{
    public class InputManager : IInputManager
    {
        private readonly ILogger<InputManager> logger;
        private readonly MouseSubSystem mouseSystem;
        private readonly ButtonSubSystem buttonSystem;
        private readonly GameContext context;
        private VeldridVideoManager video;
        private int[] gsKeyTranslationTable = new int[1024];
        private bool gfApplicationActive;
        private bool[] gfKeyState = new bool[256];            // TRUE = Pressed, FALSE = Not Pressed
        private bool fCursorWasClipped = false;
        private Rectangle gCursorClipRect;
        private int gfShiftState;                    // TRUE = Pressed, FALSE = Not Pressed
        private int gfAltState;                      // TRUE = Pressed, FALSE = Not Pressed
        private int gfCtrlState;                     // TRUE = Pressed, FALSE = Not Pressed

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

        private int guiLeftButtonRepeatTimer;
        private int guiRightButtonRepeatTimer;

        private bool gfTrackMousePos;            // TRUE = queue mouse movement events, FALSE = don't
        public bool gfLeftButtonState { get; set; }      // TRUE = Pressed, FALSE = Not Pressed
        public bool gfRightButtonState { get; set; }     // TRUE = Pressed, FALSE = Not Pressed
        private int gusMouseXPos;                    // X position of the mouse on screen
        private int gusMouseYPos;                    // y position of the mouse on screen

        // The queue structures are used to track input events using queued events

        private InputAtom[] gEventQueue = new InputAtom[256];
        private int gusQueueCount;
        private int gusHeadIndex;
        private int gusTailIndex;

        // ATE: Added to signal if we have had input this frame - cleared by the SGP main loop
        private bool gfSGPInputReceived = false;

        // This is the WIN95 hook specific data and defines used to handle the keyboard and
        // mouse hook

        //HHOOK ghKeyboardHook;
        //HHOOK ghMouseHook;

        // If the following pointer is non NULL then input characters are redirected to
        // the related string

        private bool gfCurrentStringInputState;
        private IntPtr hInstance;
        //private HookProcedureHandle ghKeyboardHook;
        private object ghMouseHook;

        public bool IsInitialized { get; private set; }

        //StringInput* gpCurrentStringDescriptor;

        public InputManager(
            ILogger<InputManager> logger,
            MouseSubSystem mouseSubSystem,
            ButtonSubSystem buttonSubsystem,
            GameContext gameContext)
        {
            this.logger = logger;
            this.mouseSystem = mouseSubSystem;
            this.buttonSystem = buttonSubsystem;
            this.context = gameContext;
        }

        //private void KeyboardHandler(object sender, KeyPressEventArgs e)
        //{
        //    Console.WriteLine("KeyPress: \t{0}", e.KeyChar);
        //}


        public void GetCursorPosition(out Point mousePos)
        {
            var pos = InputTracker.MousePosition;
            mousePos = new Point((int)pos.X, (int)pos.Y);
        }

        public async ValueTask<bool> Initialize()
        {
            if (this.IsInitialized)
            {
                return true;
            }

            this.video = (this.context.Services.GetRequiredService<IVideoManager>() as VeldridVideoManager)!;

            // Initialize the Event Queue
            this.gusQueueCount = 0;
            this.gusHeadIndex = 0;
            this.gusTailIndex = 0;
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
            this.gusMouseXPos = 320;
            this.gusMouseYPos = 240;
            // Initialize the string input mechanism
            this.gfCurrentStringInputState = false;

            ////gpCurrentStringDescriptor = null;

            this.IsInitialized = await this.buttonSystem.Initialize(this.context);

            return this.IsInitialized;
        }

        public void ProcessEvents()
        {
            try
            {
                InputSnapshot? snapshot = null;
                snapshot = this.video.Window.PumpEvents();
                InputTracker.UpdateFrameInput(snapshot, this.video.Window);

                this.gfLeftButtonState = snapshot.IsMouseDown(MouseButton.Left);
                this.gfRightButtonState = snapshot.IsMouseDown(MouseButton.Right);
            }
            catch
            {
                // threaded input can throw in sdl2 library.
                // TODO: bug it and work on getting it fixed.
                this.logger.LogDebug(LoggingEventId.INPUT, "Exception from PumpEvents");
            }
        }

        public bool DequeSpecificEvent(out InputAtom? inputAtom, MouseEvents mouseEvents)
        {
            if (this.gusQueueCount > 0)
            {
                inputAtom = this.gEventQueue[this.gusHeadIndex];
                if (inputAtom is not null)
                {
                    InputAtom copyInputAtom = (InputAtom)inputAtom;

                    if (copyInputAtom.MouseEvents.HasFlag(mouseEvents))
                    {
                        return this.DequeueEvent(out inputAtom);
                    }
                }
            }

            inputAtom = null;
            return false;
        }

        public bool DequeueEvent(out InputAtom? inputAtom)
        {
            this.HandleSingleClicksAndButtonRepeats();

            // Is there an event to dequeue
            if (this.gusQueueCount > 0)
            {
                // We have an event, so we dequeue it
                //memcpy(Event, &(gEventQueue[gusHeadIndex]), sizeof(InputAtom));

                inputAtom = this.gEventQueue[this.gusHeadIndex];

                if (this.gusHeadIndex == 255)
                {
                    this.gusHeadIndex = 0;
                }
                else
                {
                    this.gusHeadIndex++;
                }

                // Decrement the number of items on the input queue
                this.gusQueueCount--;

                // dequeued an event, return TRUE
                return true;
            }
            else
            {
                // No events to dequeue, return FALSE
                inputAtom = null;
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
    }
}
