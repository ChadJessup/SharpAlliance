using System.Diagnostics;
using System.Numerics;
using SDL2;
using SharpAlliance.Core.Managers;
using Point = SixLabors.ImageSharp.Point;
using Rectangle = SixLabors.ImageSharp.Rectangle;

namespace SharpAlliance.Core
{
    public delegate void SDLEventHandler(ref SDL.SDL_Event ev);

    public unsafe class Sdl2Window
    {

        private readonly List<SDL.SDL_Event> _events = [];
        private IntPtr _window;
        internal uint WindowID { get; private set; }
        private bool _exists;

        private SimpleInputSnapshot _publicSnapshot = new SimpleInputSnapshot();
        private SimpleInputSnapshot _privateSnapshot = new SimpleInputSnapshot();
        private SimpleInputSnapshot _privateBackbuffer = new SimpleInputSnapshot();

        // Threaded Sdl2Window flags
        private readonly bool _threadedProcessing;

        private bool _shouldClose;
        public bool LimitPollRate { get; set; }
        public float PollIntervalInMs { get; set; }

        // Current input states
        private int _currentMouseX;
        private int _currentMouseY;
        private bool[] _currentMouseButtonStates = new bool[13];
        private Vector2 _currentMouseDelta;

        // Cached Sdl2Window state (for threaded processing)
        private BufferedValue<Point> _cachedPosition = new BufferedValue<Point>();
        private BufferedValue<Size> _cachedSize = new BufferedValue<Size>();
        private string _cachedWindowTitle;
        private bool _newWindowTitleReceived;
        private bool _firstMouseEvent = true;
        private Func<bool> _closeRequestedHandler;

        public Sdl2Window(string title, int x, int y, int width, int height, SDL.SDL_WindowFlags flags, bool threadedProcessing)
        {
            SDL.SDL_SetHint("SDL.SDL_MOUSE_FOCUS_CLICKTHROUGH", "1");
            _threadedProcessing = threadedProcessing;
            if (threadedProcessing)
            {
                using (ManualResetEvent mre = new ManualResetEvent(false))
                {
                    WindowParams wp = new WindowParams()
                    {
                        Title = title,
                        X = x,
                        Y = y,
                        Width = width,
                        Height = height,
                        WindowFlags = flags,
                        ResetEvent = mre
                    };

                    Task.Factory.StartNew(WindowOwnerRoutine, wp, TaskCreationOptions.LongRunning);
                    mre.WaitOne();
                }
            }
            else
            {
                _window = SDL.SDL_CreateWindow(title, x, y, width, height, flags);
                WindowID = SDL.SDL_GetWindowID(_window);
                Sdl2WindowRegistry.RegisterWindow(this);
                PostWindowCreated(flags);
            }
        }

        public Sdl2Window(IntPtr windowHandle, bool threadedProcessing)
        {
            _threadedProcessing = threadedProcessing;
            if (threadedProcessing)
            {
                using (ManualResetEvent mre = new ManualResetEvent(false))
                {
                    WindowParams wp = new WindowParams()
                    {
                        WindowHandle = windowHandle,
                        WindowFlags = 0,
                        ResetEvent = mre
                    };

                    Task.Factory.StartNew(WindowOwnerRoutine, wp, TaskCreationOptions.LongRunning);
                    mre.WaitOne();
                }
            }
            else
            {
                _window = SDL.SDL_CreateWindowFrom(windowHandle);
                WindowID = SDL.SDL_GetWindowID(_window);
                Sdl2WindowRegistry.RegisterWindow(this);
                PostWindowCreated(0);
            }
        }

        public int X { get => _cachedPosition.Value.X; set => SetWindowPosition(value, Y); }
        public int Y { get => _cachedPosition.Value.Y; set => SetWindowPosition(X, value); }

        public int Width { get => GetWindowSize().Width; set => SetWindowSize(value, Height); }
        public int Height { get => GetWindowSize().Height; set => SetWindowSize(Width, value); }

        public IntPtr Handle => GetUnderlyingWindowHandle();

        public string Title { get => _cachedWindowTitle; set => SetWindowTitle(value); }

        private void SetWindowTitle(string value)
        {
            _cachedWindowTitle = value;
            _newWindowTitleReceived = true;
        }

        public WindowState WindowState
        {
            get
            {
                SDL.SDL_WindowFlags flags = (SDL.SDL_WindowFlags)SDL.SDL_GetWindowFlags(_window);
                if (((flags & SDL.SDL_WindowFlags.SDL_WINDOW_FULLSCREEN_DESKTOP) == SDL.SDL_WindowFlags.SDL_WINDOW_FULLSCREEN_DESKTOP)
                    || ((flags & (SDL.SDL_WindowFlags.SDL_WINDOW_BORDERLESS | SDL.SDL_WindowFlags.SDL_WINDOW_FULLSCREEN)) == (SDL.SDL_WindowFlags.SDL_WINDOW_BORDERLESS | SDL.SDL_WindowFlags.SDL_WINDOW_FULLSCREEN)))
                {
                    return WindowState.BorderlessFullScreen;
                }
                else if ((flags & SDL.SDL_WindowFlags.SDL_WINDOW_MINIMIZED) == SDL.SDL_WindowFlags.SDL_WINDOW_MINIMIZED)
                {
                    return WindowState.Minimized;
                }
                else if ((flags & SDL.SDL_WindowFlags.SDL_WINDOW_FULLSCREEN) == SDL.SDL_WindowFlags.SDL_WINDOW_FULLSCREEN)
                {
                    return WindowState.FullScreen;
                }
                else if ((flags & SDL.SDL_WindowFlags.SDL_WINDOW_MAXIMIZED) == SDL.SDL_WindowFlags.SDL_WINDOW_MAXIMIZED)
                {
                    return WindowState.Maximized;
                }
                else if ((flags & SDL.SDL_WindowFlags.SDL_WINDOW_HIDDEN) == SDL.SDL_WindowFlags.SDL_WINDOW_HIDDEN)
                {
                    return WindowState.Hidden;
                }

                return WindowState.Normal;
            }
            set
            {
                switch (value)
                {
                    case WindowState.Normal:
                        SDL.SDL_SetWindowFullscreen(_window, (uint)SDL.SDL_WindowFlags.SDL_WINDOW_FULLSCREEN);
                        break;
                    case WindowState.FullScreen:
                        SDL.SDL_SetWindowFullscreen(_window, (uint)SDL.SDL_WindowFlags.SDL_WINDOW_FULLSCREEN);
                        break;
                    case WindowState.Maximized:
                        SDL.SDL_MaximizeWindow(_window);
                        break;
                    case WindowState.Minimized:
                        SDL.SDL_MinimizeWindow(_window);
                        break;
                    case WindowState.BorderlessFullScreen:
                        SDL.SDL_SetWindowFullscreen(_window, (uint)SDL.SDL_WindowFlags.SDL_WINDOW_FULLSCREEN_DESKTOP);
                        break;
                    case WindowState.Hidden:
                        SDL.SDL_HideWindow(_window);
                        break;
                    default:
                        throw new InvalidOperationException("Illegal WindowState value: " + value);
                }
            }
        }

        public bool Exists => _exists;

        public bool Visible
        {
            get => ((SDL.SDL_WindowFlags)SDL.SDL_GetWindowFlags(_window) & SDL.SDL_WindowFlags.SDL_WINDOW_SHOWN) != 0;
            set
            {
                if (value)
                {
                    SDL.SDL_ShowWindow(_window);
                }
                else
                {
                    SDL.SDL_HideWindow(_window);
                }
            }
        }

        public Vector2 ScaleFactor => Vector2.One;

        public Rectangle Bounds => new Rectangle(_cachedPosition, GetWindowSize());

        public bool CursorVisible
        {
            get
            {
                return SDL.SDL_ShowCursor(SDL.SDL_QUERY) == 1;
            }
            set
            {
                int toggle = value ? SDL.SDL_ENABLE : SDL.SDL_DISABLE;
                SDL.SDL_ShowCursor(toggle);
            }
        }

        public float Opacity
        {
            get
            {
                float opacity = float.NaN;
                if (SDL.SDL_GetWindowOpacity(_window, out opacity) == 0)
                {
                    return opacity;
                }
                return float.NaN;
            }
            set
            {
                SDL.SDL_SetWindowOpacity(_window, value);
            }
        }

        public bool Focused => ((SDL.SDL_WindowFlags)SDL.SDL_GetWindowFlags(_window) & SDL.SDL_WindowFlags.SDL_WINDOW_INPUT_FOCUS) != 0;

        public bool Resizable
        {
            get => ((SDL.SDL_WindowFlags)SDL.SDL_GetWindowFlags(_window) & SDL.SDL_WindowFlags.SDL_WINDOW_RESIZABLE) != 0;
            set => SDL.SDL_SetWindowResizable(_window, value ? SDL.SDL_bool.SDL_TRUE : SDL.SDL_bool.SDL_FALSE);
        }

        public bool BorderVisible
        {
            get => ((SDL.SDL_WindowFlags)SDL.SDL_GetWindowFlags(_window) & SDL.SDL_WindowFlags.SDL_WINDOW_BORDERLESS) == 0;
            set => SDL.SDL_SetWindowBordered(_window, value ? SDL.SDL_bool.SDL_TRUE : SDL.SDL_bool.SDL_FALSE);
        }

        public IntPtr SdlWindowHandle => _window;

        public event Action Resized;
        public event Action Closing;
        public event Action Closed;
        public event Action FocusLost;
        public event Action FocusGained;
        public event Action Shown;
        public event Action Hidden;
        public event Action MouseEntered;
        public event Action MouseLeft;
        public event Action Exposed;
        public event Action<Point> Moved;
        public event Action<MouseWheelEventArgs> MouseWheel;
        public event Action<MouseMoveEventArgs> MouseMove;
        //        public event Action<MouseEvent> MouseDown;
        //        public event Action<MouseEvent> MouseUp;
        //        public event Action<KeyEvent> KeyDown;
        //        public event Action<KeyEvent> KeyUp;
        //        public event Action<DragDropEvent> DragDrop;

        public Point ClientToScreen(Point p)
        {
            Point position = _cachedPosition;
            return new Point(p.X + position.X, p.Y + position.Y);
        }

        public void SetMousePosition(Vector2 position) => SetMousePosition((int)position.X, (int)position.Y);
        public void SetMousePosition(int x, int y)
        {
            if (_exists)
            {
                SDL.SDL_WarpMouseInWindow(_window, x, y);
                _currentMouseX = x;
                _currentMouseY = y;
            }
        }

        public Vector2 MouseDelta => _currentMouseDelta;

        public void SetCloseRequestedHandler(Func<bool> handler)
        {
            _closeRequestedHandler = handler;
        }

        public void Close()
        {
            if (_threadedProcessing)
            {
                _shouldClose = true;
            }
            else
            {
                CloseCore();
            }
        }

        private bool CloseCore()
        {
            if (_closeRequestedHandler?.Invoke() ?? false)
            {
                _shouldClose = false;
                return false;
            }

            Sdl2WindowRegistry.RemoveWindow(this);
            Closing?.Invoke();
            SDL.SDL_DestroyWindow(_window);
            _exists = false;
            Closed?.Invoke();

            return true;
        }

        private void WindowOwnerRoutine(object? state)
        {
            WindowParams wp = (WindowParams)state;
            _window = wp.Create();
            WindowID = SDL.SDL_GetWindowID(_window);
            Sdl2WindowRegistry.RegisterWindow(this);
            PostWindowCreated(wp.WindowFlags);
            wp.ResetEvent.Set();

            double previousPollTimeMs = 0;
            Stopwatch sw = new Stopwatch();
            sw.Start();

            while (_exists)
            {
                if (_shouldClose && CloseCore())
                {
                    return;
                }

                double currentTick = sw.ElapsedTicks;
                double currentTimeMs = sw.ElapsedTicks * (1000.0 / Stopwatch.Frequency);
                if (LimitPollRate && currentTimeMs - previousPollTimeMs < PollIntervalInMs)
                {
                    Thread.Sleep(0);
                }
                else
                {
                    previousPollTimeMs = currentTimeMs;
                    ProcessEvents(null);
                }
            }
        }

        private void PostWindowCreated(SDL.SDL_WindowFlags flags)
        {
            RefreshCachedPosition();
            RefreshCachedSize();
            if ((flags & SDL.SDL_WindowFlags.SDL_WINDOW_SHOWN) == SDL.SDL_WindowFlags.SDL_WINDOW_SHOWN)
            {
                SDL.SDL_ShowWindow(_window);
            }

            _exists = true;
        }

        // Called by Sdl2EventProcessor when an event for this window is encountered.
        internal void AddEvent(SDL.SDL_Event ev)
        {
            _events.Add(ev);
        }

        public InputSnapshot PumpEvents()
        {
            _currentMouseDelta = new Vector2();
            if (_threadedProcessing)
            {
                SimpleInputSnapshot snapshot = Interlocked.Exchange(ref _privateSnapshot, _privateBackbuffer);
                snapshot.CopyTo(_publicSnapshot);
                snapshot.Clear();
            }
            else
            {
                ProcessEvents(null);
                _privateSnapshot.CopyTo(_publicSnapshot);
                _privateSnapshot.Clear();
            }

            return _publicSnapshot;
        }

        private void ProcessEvents(SDLEventHandler eventHandler)
        {
            CheckNewWindowTitle();

                        Sdl2Events.ProcessEvents();
            for (int i = 0; i < _events.Count; i++)
            {
                SDL.SDL_Event ev = _events[i];
                if (eventHandler == null)
                {
                    HandleEvent(&ev);
                }
                else
                {
                    eventHandler(ref ev);
                }
            }
            _events.Clear();
        }

        public void PumpEvents(SDLEventHandler eventHandler)
        {
            ProcessEvents(eventHandler);
        }

        private unsafe void HandleEvent(SDL.SDL_Event* ev)
        {
            switch (ev->type)
            {
                //                case SDL.SDL_EventType.Quit:
                //                    Close();
                //                    break;
                //                case SDL.SDL_EventType.Terminating:
                //                    Close();
                //                    break;
                //                case SDL.SDL_EventType.WindowEvent:
                //                    SDL.SDL_WindowEvent windowEvent = Unsafe.Read<SDL.SDL_WindowEvent>(ev);
                //                    HandleWindowEvent(windowEvent);
                //                    break;
                //                case SDL.SDL_EventType.KeyDown:
                //                case SDL.SDL_EventType.KeyUp:
                //                    SDL.SDL_KeyboardEvent keyboardEvent = Unsafe.Read<SDL.SDL_KeyboardEvent>(ev);
                //                    HandleKeyboardEvent(keyboardEvent);
                //                    break;
                //                case SDL.SDL_EventType.TextEditing:
                //                    break;
                //                case SDL.SDL_EventType.TextInput:
                //                    SDL.SDL_TextInputEvent textInputEvent = Unsafe.Read<SDL.SDL_TextInputEvent>(ev);
                //                    HandleTextInputEvent(textInputEvent);
                //                    break;
                //                case SDL.SDL_EventType.KeyMapChanged:
                //                    break;
                //                case SDL.SDL_EventType.MouseMotion:
                //                    SDL.SDL_MouseMotionEvent mouseMotionEvent = Unsafe.Read<SDL.SDL_MouseMotionEvent>(ev);
                //                    HandleMouseMotionEvent(mouseMotionEvent);
                //                    break;
                //                case SDL.SDL_EventType.MouseButtonDown:
                //                case SDL.SDL_EventType.MouseButtonUp:
                //                    SDL.SDL_MouseButtonEvent mouseButtonEvent = Unsafe.Read<SDL.SDL_MouseButtonEvent>(ev);
                //                    HandleMouseButtonEvent(mouseButtonEvent);
                //                    break;
                //                case SDL.SDL_EventType.MouseWheel:
                //                    SDL.SDL_MouseWheelEvent mouseWheelEvent = Unsafe.Read<SDL.SDL_MouseWheelEvent>(ev);
                //                    HandleMouseWheelEvent(mouseWheelEvent);
                //                    break;
                //                case SDL.SDL_EventType.DropFile:
                //                case SDL.SDL_EventType.DropBegin:
                //                case SDL.SDL_EventType.DropTest:
                //                    SDL.SDL_DropEvent dropEvent = Unsafe.Read<SDL.SDL_DropEvent>(ev);
                //                    HandleDropEvent(dropEvent);
                //                    break;
                //                default:
                //                    // Ignore
                //                    break;
            }
        }

        private void CheckNewWindowTitle()
        {
            if (WindowState != WindowState.Minimized && _newWindowTitleReceived)
            {
                _newWindowTitleReceived = false;
                SDL.SDL_SetWindowTitle(_window, _cachedWindowTitle);
            }
        }

        private void HandleTextInputEvent(SDL.SDL_TextInputEvent textInputEvent)
        {
            //uint byteCount = 0;
            //// Loop until the null terminator is found or the max size is reached.
            //while (byteCount < SDL_TextInputEvent.MaxTextSize && textInputEvent.text[byteCount++] != 0)
            //{ }

            //if (byteCount > 1)
            //{
            //    // We don't want the null terminator.
            //    byteCount -= 1;
            //    int charCount = Encoding.UTF8.GetCharCount(textInputEvent.text, (int)byteCount);
            //    char* charsPtr = stackalloc char[charCount];
            //    Encoding.UTF8.GetChars(textInputEvent.text, (int)byteCount, charsPtr, charCount);
            //    for (int i = 0; i < charCount; i++)
            //    {
            //        _privateSnapshot.KeyCharPressesList.Add(charsPtr[i]);
            //    }
            //}
        }

        private void HandleMouseWheelEvent(SDL.SDL_MouseWheelEvent mouseWheelEvent)
        {
            _privateSnapshot.WheelDelta += mouseWheelEvent.y;
            MouseWheel?.Invoke(new MouseWheelEventArgs(GetCurrentMouseState(), (float)mouseWheelEvent.y));
        }

        //        private void HandleDropEvent(SDL.SDL_DropEvent dropEvent)
        //        {
        //            string file = Utilities.GetString(dropEvent.file);
        //            SDL.SDL_free(dropEvent.file);
        //
        //            if (dropEvent.type == SDL.SDL_EventType.DropFile)
        //            {
        //                DragDrop?.Invoke(new DragDropEvent(file));
        //            }
        //        }

        //        private void HandleMouseButtonEvent(SDL.SDL_MouseButtonEvent mouseButtonEvent)
        //        {
        //            MouseButton button = MapMouseButton(mouseButtonEvent.button);
        //            bool down = mouseButtonEvent.state == 1;
        //            _currentMouseButtonStates[(int)button] = down;
        //            _privateSnapshot.MouseDown[(int)button] = down;
        //            MouseEvent mouseEvent = new MouseEvent(button, down);
        //            _privateSnapshot.MouseEventsList.Add(mouseEvent);
        //            if (down)
        //            {
        //                MouseDown?.Invoke(mouseEvent);
        //            }
        //            else
        //            {
        //                MouseUp?.Invoke(mouseEvent);
        //            }
        //        }

        //        private MouseButton MapMouseButton(SDL_MouseButton button)
        //        {
        //            switch (button)
        //            {
        //                case SDL.SDL_MouseButton.Left:
        //                    return MouseButton.Left;
        //                case SDL.SDL_MouseButton.Middle:
        //                    return MouseButton.Middle;
        //                case SDL.SDL_MouseButton.Right:
        //                    return MouseButton.Right;
        //                case SDL.SDL_MouseButtonEvent.SDL_MouseButton.X1:
        //                    return MouseButton.Button1;
        //                case SDL.SDL_MouseButton.X2:
        //                    return MouseButton.Button2;
        //                default:
        //                    return MouseButton.Left;
        //            }
        //        }

        private void HandleMouseMotionEvent(SDL.SDL_MouseMotionEvent mouseMotionEvent)
        {
            Vector2 mousePos = new Vector2(mouseMotionEvent.x, mouseMotionEvent.y);
            Vector2 delta = new Vector2(mouseMotionEvent.xrel, mouseMotionEvent.yrel);
            _currentMouseX = (int)mousePos.X;
            _currentMouseY = (int)mousePos.Y;
            _privateSnapshot.MousePosition = mousePos;

            if (!_firstMouseEvent)
            {
                _currentMouseDelta += delta;
                MouseMove?.Invoke(new MouseMoveEventArgs(GetCurrentMouseState(), mousePos));
            }

            _firstMouseEvent = false;
        }

        private void HandleKeyboardEvent(SDL.SDL_KeyboardEvent keyboardEvent)
        {
            //            SimpleInputSnapshot snapshot = _privateSnapshot;
            //            KeyEvent keyEvent = new KeyEvent(MapKey(keyboardEvent.keysym), keyboardEvent.state == 1, MapModifierKeys(keyboardEvent.keysym.mod), keyboardEvent.repeat == 1);
            //            snapshot.KeyEventsList.Add(keyEvent);
            //            if (keyboardEvent.state == 1)
            //            {
            //                KeyDown?.Invoke(keyEvent);
            //            }
            //            else
            //            {
            //                KeyUp?.Invoke(keyEvent);
            //            }
        }

        //        private Key MapKey(SDL.SDL_Keysym keysym)
        //        {
        //            switch (keysym.scancode)
        //            {
        //                case SDL.SDL_Scancode.SDL_SCANCODE_A:
        //                    return Key.A;
        //                case SDL.SDL_Scancode.SDL_SCANCODE_B:
        //                    return Key.B;
        //                case SDL.SDL_Scancode.SDL_SCANCODE_C:
        //                    return Key.C;
        //                case SDL.SDL_Scancode.SDL_SCANCODE_D:
        //                    return Key.D;
        //                case SDL.SDL_Scancode.SDL_SCANCODE_E:
        //                    return Key.E;
        //                case SDL.SDL_Scancode.SDL_SCANCODE_F:
        //                    return Key.F;
        //                case SDL.SDL_Scancode.SDL_SCANCODE_G:
        //                    return Key.G;
        //                case SDL.SDL_Scancode.SDL_SCANCODE_H:
        //                    return Key.H;
        //                case SDL.SDL_Scancode.SDL_SCANCODE_I:
        //                    return Key.I;
        //                case SDL.SDL_Scancode.SDL_SCANCODE_J:
        //                    return Key.J;
        //                case SDL.SDL_Scancode.SDL_SCANCODE_K:
        //                    return Key.K;
        //                case SDL.SDL_Scancode.SDL_SCANCODE_L:
        //                    return Key.L;
        //                case SDL.SDL_Scancode.SDL_SCANCODE_M:
        //                    return Key.M;
        //                case SDL.SDL_Scancode.SDL_SCANCODE_N:
        //                    return Key.N;
        //                case SDL.SDL_Scancode.SDL_SCANCODE_O:
        //                    return Key.O;
        //                case SDL.SDL_Scancode.SDL_SCANCODE_P:
        //                    return Key.P;
        //                case SDL.SDL_Scancode.SDL_SCANCODE_Q:
        //                    return Key.Q;
        //                case SDL.SDL_Scancode.SDL_SCANCODE_R:
        //                    return Key.R;
        //                case SDL.SDL_Scancode.SDL_SCANCODE_S:
        //                    return Key.S;
        //                case SDL.SDL_Scancode.SDL_SCANCODE_T:
        //                    return Key.T;
        //                case SDL.SDL_Scancode.SDL_SCANCODE_U:
        //                    return Key.U;
        //                case SDL.SDL_Scancode.SDL_SCANCODE_V:
        //                    return Key.V;
        //                case SDL.SDL_Scancode.SDL_SCANCODE_W:
        //                    return Key.W;
        //                case SDL.SDL_Scancode.SDL_SCANCODE_X:
        //                    return Key.X;
        //                case SDL.SDL_Scancode.SDL_SCANCODE_Y:
        //                    return Key.Y;
        //                case SDL.SDL_Scancode.SDL_SCANCODE_Z:
        //                    return Key.Z;
        //                case SDL.SDL_Scancode.SDL_SCANCODE_1:
        //                    return Key.Number1;
        //                case SDL.SDL_Scancode.SDL_SCANCODE_2:
        //                    return Key.Number2;
        //                case SDL.SDL_Scancode.SDL_SCANCODE_3:
        //                    return Key.Number3;
        //                case SDL.SDL_Scancode.SDL_SCANCODE_4:
        //                    return Key.Number4;
        //                case SDL.SDL_Scancode.SDL_SCANCODE_5:
        //                    return Key.Number5;
        //                case SDL.SDL_Scancode.SDL_SCANCODE_6:
        //                    return Key.Number6;
        //                case SDL.SDL_Scancode.SDL_SCANCODE_7:
        //                    return Key.Number7;
        //                case SDL.SDL_Scancode.SDL_SCANCODE_8:
        //                    return Key.Number8;
        //                case SDL.SDL_Scancode.SDL_SCANCODE_9:
        //                    return Key.Number9;
        //                case SDL.SDL_Scancode.SDL_SCANCODE_0:
        //                    return Key.Number0;
        //                case SDL.SDL_Scancode.SDL_SCANCODE_RETURN:
        //                    return Key.Enter;
        //                case SDL.SDL_Scancode.SDL_SCANCODE_ESCAPE:
        //                    return Key.Escape;
        //                case SDL.SDL_Scancode.SDL_SCANCODE_BACKSPACE:
        //                    return Key.BackSpace;
        //                case SDL.SDL_Scancode.SDL_SCANCODE_TAB:
        //                    return Key.Tab;
        //                case SDL.SDL_Scancode.SDL_SCANCODE_SPACE:
        //                    return Key.Space;
        //                case SDL.SDL_Scancode.SDL_SCANCODE_MINUS:
        //                    return Key.Minus;
        //                case SDL.SDL_Scancode.SDL_SCANCODE_EQUALS:
        //                    return Key.Plus;
        //                case SDL.SDL_Scancode.SDL_SCANCODE_LEFTBRACKET:
        //                    return Key.BracketLeft;
        //                case SDL.SDL_Scancode.SDL_SCANCODE_RIGHTBRACKET:
        //                    return Key.BracketRight;
        //                case SDL.SDL_Scancode.SDL_SCANCODE_BACKSLASH:
        //                    return Key.BackSlash;
        //                case SDL.SDL_Scancode.SDL_SCANCODE_SEMICOLON:
        //                    return Key.Semicolon;
        //                case SDL.SDL_Scancode.SDL_SCANCODE_APOSTROPHE:
        //                    return Key.Quote;
        //                case SDL.SDL_Scancode.SDL_SCANCODE_GRAVE:
        //                    return Key.Grave;
        //                case SDL.SDL_Scancode.SDL_SCANCODE_COMMA:
        //                    return Key.Comma;
        //                case SDL.SDL_Scancode.SDL_SCANCODE_PERIOD:
        //                    return Key.Period;
        //                case SDL.SDL_Scancode.SDL_SCANCODE_SLASH:
        //                    return Key.Slash;
        //                case SDL.SDL_Scancode.SDL_SCANCODE_CAPSLOCK:
        //                    return Key.CapsLock;
        //                case SDL.SDL_Scancode.SDL_SCANCODE_F1:
        //                    return Key.F1;
        //                case SDL.SDL_Scancode.SDL_SCANCODE_F2:
        //                    return Key.F2;
        //                case SDL.SDL_Scancode.SDL_SCANCODE_F3:
        //                    return Key.F3;
        //                case SDL.SDL_Scancode.SDL_SCANCODE_F4:
        //                    return Key.F4;
        //                case SDL.SDL_Scancode.SDL_SCANCODE_F5:
        //                    return Key.F5;
        //                case SDL.SDL_Scancode.SDL_SCANCODE_F6:
        //                    return Key.F6;
        //                case SDL.SDL_Scancode.SDL_SCANCODE_F7:
        //                    return Key.F7;
        //                case SDL.SDL_Scancode.SDL_SCANCODE_F8:
        //                    return Key.F8;
        //                case SDL.SDL_Scancode.SDL_SCANCODE_F9:
        //                    return Key.F9;
        //                case SDL.SDL_Scancode.SDL_SCANCODE_F10:
        //                    return Key.F10;
        //                case SDL.SDL_Scancode.SDL_SCANCODE_F11:
        //                    return Key.F11;
        //                case SDL.SDL_Scancode.SDL_SCANCODE_F12:
        //                    return Key.F12;
        //                case SDL.SDL_Scancode.SDL_SCANCODE_PRINTSCREEN:
        //                    return Key.PrintScreen;
        //                case SDL.SDL_Scancode.SDL_SCANCODE_SCROLLLOCK:
        //                    return Key.ScrollLock;
        //                case SDL.SDL_Scancode.SDL_SCANCODE_PAUSE:
        //                    return Key.Pause;
        //                case SDL.SDL_Scancode.SDL_SCANCODE_INSERT:
        //                    return Key.Insert;
        //                case SDL.SDL_Scancode.SDL_SCANCODE_HOME:
        //                    return Key.Home;
        //                case SDL.SDL_Scancode.SDL_SCANCODE_PAGEUP:
        //                    return Key.PageUp;
        //                case SDL.SDL_Scancode.SDL_SCANCODE_DELETE:
        //                    return Key.Delete;
        //                case SDL.SDL_Scancode.SDL_SCANCODE_END:
        //                    return Key.End;
        //                case SDL.SDL_Scancode.SDL_SCANCODE_PAGEDOWN:
        //                    return Key.PageDown;
        //                case SDL.SDL_Scancode.SDL_SCANCODE_RIGHT:
        //                    return Key.Right;
        //                case SDL.SDL_Scancode.SDL_SCANCODE_LEFT:
        //                    return Key.Left;
        //                case SDL.SDL_Scancode.SDL_SCANCODE_DOWN:
        //                    return Key.Down;
        //                case SDL.SDL_Scancode.SDL_SCANCODE_UP:
        //                    return Key.Up;
        //                case SDL.SDL_Scancode.SDL_SCANCODE_NUMLOCKCLEAR:
        //                    return Key.NumLock;
        //                case SDL.SDL_Scancode.SDL_SCANCODE_KP_DIVIDE:
        //                    return Key.KeypadDivide;
        //                case SDL.SDL_Scancode.SDL_SCANCODE_KP_MULTIPLY:
        //                    return Key.KeypadMultiply;
        //                case SDL.SDL_Scancode.SDL_SCANCODE_KP_MINUS:
        //                    return Key.KeypadMinus;
        //                case SDL.SDL_Scancode.SDL_SCANCODE_KP_PLUS:
        //                    return Key.KeypadPlus;
        //                case SDL.SDL_Scancode.SDL_SCANCODE_KP_ENTER:
        //                    return Key.KeypadEnter;
        //                case SDL.SDL_Scancode.SDL_SCANCODE_KP_1:
        //                    return Key.Keypad1;
        //                case SDL.SDL_Scancode.SDL_SCANCODE_KP_2:
        //                    return Key.Keypad2;
        //                case SDL.SDL_Scancode.SDL_SCANCODE_KP_3:
        //                    return Key.Keypad3;
        //                case SDL.SDL_Scancode.SDL_SCANCODE_KP_4:
        //                    return Key.Keypad4;
        //                case SDL.SDL_Scancode.SDL_SCANCODE_KP_5:
        //                    return Key.Keypad5;
        //                case SDL.SDL_Scancode.SDL_SCANCODE_KP_6:
        //                    return Key.Keypad6;
        //                case SDL.SDL_Scancode.SDL_SCANCODE_KP_7:
        //                    return Key.Keypad7;
        //                case SDL.SDL_Scancode.SDL_SCANCODE_KP_8:
        //                    return Key.Keypad8;
        //                case SDL.SDL_Scancode.SDL_SCANCODE_KP_9:
        //                    return Key.Keypad9;
        //                case SDL.SDL_Scancode.SDL_SCANCODE_KP_0:
        //                    return Key.Keypad0;
        //                case SDL.SDL_Scancode.SDL_SCANCODE_KP_PERIOD:
        //                    return Key.KeypadPeriod;
        //                case SDL.SDL_Scancode.SDL_SCANCODE_NONUSBACKSLASH:
        //                    return Key.NonUSBackSlash;
        //                case SDL.SDL_Scancode.SDL_SCANCODE_KP_EQUALS:
        //                    return Key.KeypadPlus;
        //                case SDL.SDL_Scancode.SDL_SCANCODE_F13:
        //                    return Key.F13;
        //                case SDL.SDL_Scancode.SDL_SCANCODE_F14:
        //                    return Key.F14;
        //                case SDL.SDL_Scancode.SDL_SCANCODE_F15:
        //                    return Key.F15;
        //                case SDL.SDL_Scancode.SDL_SCANCODE_F16:
        //                    return Key.F16;
        //                case SDL.SDL_Scancode.SDL_SCANCODE_F17:
        //                    return Key.F17;
        //                case SDL.SDL_Scancode.SDL_SCANCODE_F18:
        //                    return Key.F18;
        //                case SDL.SDL_Scancode.SDL_SCANCODE_F19:
        //                    return Key.F19;
        //                case SDL.SDL_Scancode.SDL_SCANCODE_F20:
        //                    return Key.F20;
        //                case SDL.SDL_Scancode.SDL_SCANCODE_F21:
        //                    return Key.F21;
        //                case SDL.SDL_Scancode.SDL_SCANCODE_F22:
        //                    return Key.F22;
        //                case SDL.SDL_Scancode.SDL_SCANCODE_F23:
        //                    return Key.F23;
        //                case SDL.SDL_Scancode.SDL_SCANCODE_F24:
        //                    return Key.F24;
        //                case SDL.SDL_Scancode.SDL_SCANCODE_MENU:
        //                    return Key.Menu;
        //                case SDL.SDL_Scancode.SDL_SCANCODE_LCTRL:
        //                    return Key.ControlLeft;
        //                case SDL.SDL_Scancode.SDL_SCANCODE_LSHIFT:
        //                    return Key.ShiftLeft;
        //                case SDL.SDL_Scancode.SDL_SCANCODE_LALT:
        //                    return Key.AltLeft;
        //                case SDL.SDL_Scancode.SDL_SCANCODE_RCTRL:
        //                    return Key.ControlRight;
        //                case SDL.SDL_Scancode.SDL_SCANCODE_RSHIFT:
        //                    return Key.ShiftRight;
        //                case SDL.SDL_Scancode.SDL_SCANCODE_RALT:
        //                    return Key.AltRight;
        //                case SDL.SDL_Scancode.SDL_SCANCODE_LGUI:
        //                    return Key.LWin;
        //                case SDL.SDL_Scancode.SDL_SCANCODE_RGUI:
        //                    return Key.RWin;
        //                default:
        //                    return Key.Unknown;
        //            }
        //        }

        //        private ModifierKeys MapModifierKeys(SDL.SDL_Keymod mod)
        //        {
        //            ModifierKeys mods = ModifierKeys.None;
        //            if ((mod & (SDL.SDL_Keymod.LeftShift | SDL.SDL_Keymod.RightShift)) != 0)
        //            {
        //                mods |= ModifierKeys.Shift;
        //            }
        //            if ((mod & (SDL.SDL_Keymod.LeftAlt | SDL.SDL_Keymod.RightAlt)) != 0)
        //            {
        //                mods |= ModifierKeys.Alt;
        //            }
        //            if ((mod & (SDL.SDL_Keymod.LeftControl | SDL.SDL_Keymod.RightControl)) != 0)
        //            {
        //                mods |= ModifierKeys.Control;
        //            }
        //            if ((mod & (SDL.SDL_Keymod.LeftGui | SDL.SDL_Keymod.RightGui)) != 0)
        //            {
        //                mods |= ModifierKeys.Gui;
        //            }
        //
        //            return mods;
        //        }

        private void HandleWindowEvent(SDL.SDL_WindowEvent windowEvent)
        {
            //            switch (windowEvent.@event)
            //            {
            //                case SDL.SDL_WindowEventID.Resized:
            //                case SDL.SDL_WindowEventID.SizeChanged:
            //                case SDL.SDL_WindowEventID.Minimized:
            //                case SDL.SDL_WindowEventID.Maximized:
            //                case SDL.SDL_WindowEventID.Restored:
            //                    HandleResizedMessage();
            //                    break;
            //                case SDL.SDL_WindowEventID.FocusGained:
            //                    FocusGained?.Invoke();
            //                    break;
            //                case SDL.SDL_WindowEventID.FocusLost:
            //                    FocusLost?.Invoke();
            //                    break;
            //                case SDL.SDL_WindowEventID.Close:
            //                    Close();
            //                    break;
            //                case SDL.SDL_WindowEventID.Shown:
            //                    Shown?.Invoke();
            //                    break;
            //                case SDL.SDL_WindowEventID.Hidden:
            //                    Hidden?.Invoke();
            //                    break;
            //                case SDL.SDL_WindowEventID.Enter:
            //                    MouseEntered?.Invoke();
            //                    break;
            //                case SDL.SDL_WindowEventID.Leave:
            //                    MouseLeft?.Invoke();
            //                    break;
            //                case SDL.SDL_WindowEventID.Exposed:
            //                    Exposed?.Invoke();
            //                    break;
            //                case SDL.SDL_WindowEventID.Moved:
            //                    _cachedPosition.Value = new Point(windowEvent.data1, windowEvent.data2);
            //                    Moved?.Invoke(new Point(windowEvent.data1, windowEvent.data2));
            //                    break;
            //                default:
            //                    Debug.WriteLine("Unhandled SDL WindowEvent: " + windowEvent.@event);
            //                    break;
            //            }
        }

        private void HandleResizedMessage()
        {
            RefreshCachedSize();
            Resized?.Invoke();
        }

        private void RefreshCachedSize()
        {
            int w, h;
            SDL.SDL_GetWindowSize(_window, out w, out h);
            _cachedSize.Value = new Size(w, h);
        }

        private void RefreshCachedPosition()
        {
            int x, y;
            SDL.SDL_GetWindowPosition(_window, out x, out y);
            _cachedPosition.Value = new Point(x, y);
        }

        private MouseState GetCurrentMouseState()
        {
            return new MouseState(
                _currentMouseX, _currentMouseY,
                _currentMouseButtonStates[0], _currentMouseButtonStates[1],
                _currentMouseButtonStates[2], _currentMouseButtonStates[3],
                _currentMouseButtonStates[4], _currentMouseButtonStates[5],
                _currentMouseButtonStates[6], _currentMouseButtonStates[7],
                _currentMouseButtonStates[8], _currentMouseButtonStates[9],
                _currentMouseButtonStates[10], _currentMouseButtonStates[11],
                _currentMouseButtonStates[12]);
        }

        public Point ScreenToClient(Point p)
        {
            Point position = _cachedPosition;
            return new Point(p.X - position.X, p.Y - position.Y);
        }

        private void SetWindowPosition(int x, int y)
        {
            SDL.SDL_SetWindowPosition(_window, x, y);
            _cachedPosition.Value = new Point(x, y);
        }

        private Size GetWindowSize()
        {
            return _cachedSize;
        }

        private void SetWindowSize(int width, int height)
        {
            SDL.SDL_SetWindowSize(_window, width, height);
            _cachedSize.Value = new Size(width, height);
        }

        private IntPtr GetUnderlyingWindowHandle()
        {
            SDL.SDL_SysWMinfo wmInfo;
            SDL.SDL_GetVersion(out wmInfo.version);
            //            SDL.SDL_GetWMWindowInfo(_window, out wmInfo);
            //            switch (wmInfo.subsystem)
            //            {
            //                case SysWMType.Windows:
            //                    Win32WindowInfo win32Info = Unsafe.Read<Win32WindowInfo>(&wmInfo.info);
            //                    return win32Info.Sdl2Window;
            //                case SysWMType.X11:
            //                    X11WindowInfo x11Info = Unsafe.Read<X11WindowInfo>(&wmInfo.info);
            //                    return x11Info.Sdl2Window;
            //                case SysWMType.Wayland:
            //                    WaylandWindowInfo waylandInfo = Unsafe.Read<WaylandWindowInfo>(&wmInfo.info);
            //                    return waylandInfo.surface;
            //                case SysWMType.Cocoa:
            //                    CocoaWindowInfo cocoaInfo = Unsafe.Read<CocoaWindowInfo>(&wmInfo.info);
            //                    return cocoaInfo.Window;
            //                case SysWMType.Android:
            //                    AndroidWindowInfo androidInfo = Unsafe.Read<AndroidWindowInfo>(&wmInfo.info);
            //                    return androidInfo.window;
            //                default:
            return _window;
            //            }
        }

        public interface InputSnapshot
        {
            //            IReadOnlyList<KeyEvent> KeyEvents { get; }
            //            IReadOnlyList<MouseEvent> MouseEvents { get; }
            //            IReadOnlyList<char> KeyCharPresses { get; }
            //            bool IsMouseDown(MouseButton button);
            Vector2 MousePosition { get; }
            float WheelDelta { get; }
        }

        private class SimpleInputSnapshot : InputSnapshot
        {
            //            public List<KeyEvent> KeyEventsList { get; private set; } = new List<KeyEvent>();
            //            public List<MouseEvent> MouseEventsList { get; private set; } = new List<MouseEvent>();
            //            public List<char> KeyCharPressesList { get; private set; } = new List<char>();
            //
            //            public IReadOnlyList<KeyEvent> KeyEvents => KeyEventsList;
            //
            //            public IReadOnlyList<MouseEvent> MouseEvents => MouseEventsList;

            //            public IReadOnlyList<char> KeyCharPresses => KeyCharPressesList;

            public Vector2 MousePosition { get; set; }

            private bool[] _mouseDown = new bool[13];
            public bool[] MouseDown => _mouseDown;
            public float WheelDelta { get; set; }

            //            public bool IsMouseDown(MouseButton button)
            //            {
            //                return _mouseDown[(int)button];
            //            }

            internal void Clear()
            {
                //                KeyEventsList.Clear();
                //                MouseEventsList.Clear();
                //                KeyCharPressesList.Clear();
                WheelDelta = 0f;
            }

            public void CopyTo(SimpleInputSnapshot other)
            {
                Debug.Assert(this != other);

                //                other.MouseEventsList.Clear();
                //                foreach (var me in MouseEventsList)
                //                { other.MouseEventsList.Add(me); }
                //
                //                other.KeyEventsList.Clear();
                //                foreach (var ke in KeyEventsList)
                //                { other.KeyEventsList.Add(ke); }
                //
                //                other.KeyCharPressesList.Clear();
                //                foreach (var kcp in KeyCharPressesList)
                //                { other.KeyCharPressesList.Add(kcp); }

                other.MousePosition = MousePosition;
                other.WheelDelta = WheelDelta;
                _mouseDown.CopyTo(other._mouseDown, 0);
            }
        }

        private class WindowParams
        {
            public int X { get; set; }
            public int Y { get; set; }
            public int Width { get; set; }
            public int Height { get; set; }
            public string Title { get; set; }
            public SDL.SDL_WindowFlags WindowFlags { get; set; }

            public IntPtr WindowHandle { get; set; }

            public ManualResetEvent ResetEvent { get; set; }

            public nint Create()
            {
                if (WindowHandle != IntPtr.Zero)
                {
                    return SDL.SDL_CreateWindowFrom(WindowHandle);
                }
                else
                {
                    return SDL.SDL_CreateWindow(Title, X, Y, Width, Height, WindowFlags);
                }
            }
        }
    }
}
