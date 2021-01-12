// Copyright (c) Amer Koleci and contributors.
// Distributed under the MIT license. See the LICENSE file in the project root for more information.

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using SharpAlliance;
using Vortice.Win32;
using static Vortice.Win32.User32;

namespace Vortice
{
    public sealed class Window
    {
        private const int CW_USEDEFAULT = unchecked((int)0x80000000);

        public string Title { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        public IntPtr Handle { get; private set; }

        public Window(string title, int width, int height)
        {
            this.Title = title;
            this.Width = width;
            this.Height = height;
            this.PlatformConstruct();
        }

        private void PlatformConstruct()
        {
            int x = 0;
            int y = 0;
            WindowStyles style = 0;
            WindowExStyles styleEx = 0;
            const bool resizable = true;

            // Setup the screen settings depending on whether it is running in full screen or in windowed mode.
            //if (fullscreen)
            //{
            //style = User32.WindowStyles.WS_POPUP | User32.WindowStyles.WS_VISIBLE;
            //styleEx = User32.WindowStyles.WS_EX_APPWINDOW;

            //width = screenWidth;
            //height = screenHeight;
            //}
            //else
            {
                if (this.Width > 0 && this.Height > 0)
                {
                    int screenWidth = GetSystemMetrics(SystemMetrics.SM_CXSCREEN);
                    int screenHeight = GetSystemMetrics(SystemMetrics.SM_CYSCREEN);

                    // Place the window in the middle of the screen.WS_EX_APPWINDOW
                    x = (screenWidth - this.Width) / 2;
                    y = (screenHeight - this.Height) / 2;
                }

                if (resizable)
                {
                    style = WindowStyles.WS_OVERLAPPEDWINDOW;
                }
                else
                {
                    style = WindowStyles.WS_POPUP | WindowStyles.WS_BORDER | WindowStyles.WS_CAPTION | WindowStyles.WS_SYSMENU;
                }

                styleEx = WindowExStyles.WS_EX_APPWINDOW | WindowExStyles.WS_EX_WINDOWEDGE;
            }
            style |= WindowStyles.WS_CLIPCHILDREN | WindowStyles.WS_CLIPSIBLINGS;

            int windowWidth;
            int windowHeight;

            if (this.Width > 0 && this.Height > 0)
            {
                var rect = new RawRect(0, 0, this.Width, this.Height);

                // Adjust according to window styles
                AdjustWindowRectEx(
                    ref rect,
                    style,
                    false,
                    styleEx);

                windowWidth = rect.Right - rect.Left;
                windowHeight = rect.Bottom - rect.Top;
            }
            else
            {
                x = y = windowWidth = windowHeight = CW_USEDEFAULT;
            }

            this.Handle = CreateWindowEx(
                (int)styleEx,
                WindowsSubSystem.WndClassName,
                this.Title,
                (int)style,
                x,
                y,
                windowWidth,
                windowHeight,
                IntPtr.Zero,
                IntPtr.Zero,
                IntPtr.Zero,
                IntPtr.Zero);

            if (this.Handle == IntPtr.Zero)
            {
                var error = Marshal.GetLastWin32Error();
                var hresult = Marshal.GetHRForLastWin32Error();
                return;
            }

            ShowWindow(this.Handle, ShowWindowCommand.Normal);
            UpdateWindow(this.Handle);
            this.Width = windowWidth;
            this.Height = windowHeight;
        }

        public void Destroy()
        {
            IntPtr hwnd = this.Handle;
            if (hwnd != IntPtr.Zero)
            {
                IntPtr destroyHandle = hwnd;
                this.Handle = IntPtr.Zero;

                Debug.WriteLine($"[WIN32] - Destroying window: {destroyHandle}");
                DestroyWindow(destroyHandle);
            }
        }
    }
}
