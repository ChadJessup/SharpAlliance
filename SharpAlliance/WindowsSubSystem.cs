﻿using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SharpAlliance.Core;
using SharpAlliance.Core.Interfaces;
using SharpAlliance.Core.Managers;
using SharpAlliance.Core.SubSystems;
using SharpAlliance.Platform;
using SharpAlliance.Platform.Interfaces;
using SixLabors.ImageSharp;
using Vortice.Win32;
using static Vortice.Win32.Kernel32;
using static Vortice.Win32.User32;

namespace SharpAlliance
{
    public class WindowsSubSystem : IOSManager
    {
        private readonly GameContext context;
        private readonly ILogger<WindowsSubSystem> logger;
        private readonly IVideoManager video;
        private readonly IInputManager input;
        private readonly FontSubSystem fonts;
        public static readonly string WndClassName = "VorticeWindow";
        public readonly IntPtr HInstance = GetModuleHandle(null);

        private WNDPROC _wndProc;
        private bool _paused;
        private bool exitRequested;

        public WindowsSubSystem(
            ILogger<WindowsSubSystem> logger,
            GameContext context,
            IInputManager inputManager,
            IVideoManager videoManager,
            FontSubSystem fontSubSystem)
        {
            this.context = context;
            this.logger = logger;
            video = videoManager;
            this.input = inputManager;
            this.fonts = fontSubSystem;
            this.Initialize().AsTask().Wait();
        }

        public async ValueTask<bool> Initialize()
        {
            await video.Initialize();
            await this.input.Initialize();
            await this.fonts.Initialize();

            //this.PlatformConstruct();

            var validation = false;
#if DEBUG
            validation = true;
#endif

            if (this.MainWindow is not null)
            {
                SDL2VideoManager videoManager = this.context.Services.GetRequiredService<SDL2VideoManager>();
                this.MainWindow = videoManager.Window;

//                vorticeVideoManager.SetGraphicsDevice(new D3D12GraphicsDevice(validation, this.MainWindow));
            }

            return true;
        }

        public IWindow MainWindow { get; private set; }
        public bool IsInitialized { get; }

        public IWindow CreateWindow(string name = "Vortice")
        {
            SDL2VideoManager videoManager = this.context.Services.GetRequiredService<SDL2VideoManager>();
            return videoManager.Window;
        }

        private void PlatformConstruct()
        {
            this._wndProc = this.ProcessWindowMessage;
            var wndClassEx = new WNDCLASSEX
            {
                Size = Unsafe.SizeOf<WNDCLASSEX>(),
                Styles = WindowClassStyles.CS_HREDRAW | WindowClassStyles.CS_VREDRAW | WindowClassStyles.CS_OWNDC,
                WindowProc = this._wndProc,
                InstanceHandle = this.HInstance,
                CursorHandle = LoadCursor(IntPtr.Zero, SystemCursor.IDC_ARROW),
                BackgroundBrushHandle = IntPtr.Zero,
                IconHandle = IntPtr.Zero,
                ClassName = WndClassName,
            };

            var atom = RegisterClassEx(ref wndClassEx);

            if (atom == 0)
            {
                throw new InvalidOperationException($"Failed to register window class. Error: {Marshal.GetLastWin32Error()}");
            }

            // Defer actual window creation until we are on the right thread.
        }

        private IntPtr ProcessWindowMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
        {
            if (msg == (uint)WindowMessage.ActivateApp)
            {
                this._paused = IntPtrToInt32(wParam) == 0;
                if (IntPtrToInt32(wParam) != 0)
                {
                    this.OnActivated();
                }
                else
                {
                    this.OnDeactivated();
                }

                return DefWindowProc(hWnd, msg, wParam, lParam);
            }

            switch ((WindowMessage)msg)
            {
                case WindowMessage.Destroy:
                    PostQuitMessage(0);
                    break;
            }

            return DefWindowProc(hWnd, msg, wParam, lParam);
        }

        public ValueTask<bool> Pump(Action gameLoopCallback)
        {
            if (this.MainWindow is null)
            {
                this.MainWindow = this.CreateWindow();
            }

            if (!this._paused)
            {
                const uint PM_REMOVE = 1;
                if (PeekMessage(out var msg, IntPtr.Zero, 0, 0, PM_REMOVE))
                {
                    TranslateMessage(ref msg);
                    DispatchMessage(ref msg);

                    switch (msg.Value)
                    {
                        case WindowMessage.MouseMove:
                            this.input.MouseChangeEvent(this.ConvertToMouseEvent(MouseEvents.MousePosition, msg));
                            break;

                        case WindowMessage.Quit:
                            this.exitRequested = true;
                            return ValueTask.FromResult(false);
                    };
                }

                gameLoopCallback();
            }
            else
            {
                var ret = GetMessage(out var msg, IntPtr.Zero, 0, 0);
                if (ret == 0)
                {
                    this.exitRequested = true;
                    return ValueTask.FromResult(false);
                }
                else if (ret == -1)
                {
                    //Log.Error("[Win32] - Failed to get message");
                    this.exitRequested = true;
                    return ValueTask.FromResult(false);
                }
                else
                {
                    TranslateMessage(ref msg);
                    DispatchMessage(ref msg);
                }
            }

            return ValueTask.FromResult(true);
        }

        private MouseEvent ConvertToMouseEvent(MouseEvents eventType, Message msg)
        {
            static Point ConvertToPoint(IntPtr lParam)
            {
                return new Point
                {
                    X = GET_X_LPARAM(lParam),
                    Y = GET_Y_LPARAM(lParam),
                };
            }

            //var me = new MouseEvent
            //{
            //     MouseButton = 
            //    EventType = eventType,
            //    Position = ConvertToPoint(msg.LParam),
            //};
            //
            //Console.WriteLine($"{me.Position.X}:{me.Position.Y}");

            return new MouseEvent();
        }

        private void OnActivated()
        {
        }

        private void OnDeactivated()
        {
        }

        private static int IntPtrToInt32(IntPtr intPtr)
        {
            return (int)intPtr.ToInt64();
        }

        public void Dispose()
        {
        }

        public static int GET_X_LPARAM(IntPtr lp) => (int)(short)LOWORD(lp);
        public static int GET_Y_LPARAM(IntPtr lp) => (int)(short)HIWORD(lp);
        public static ushort LOWORD(IntPtr _dw) => (ushort)(((ulong)_dw) & 0xffff);
        public static ushort HIWORD(IntPtr _dw) => (ushort)((((ulong)_dw) >> 16) & 0xffff);
    }
}
