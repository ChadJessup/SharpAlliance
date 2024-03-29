﻿using SDL2;

namespace SharpAlliance.Core.Managers;

public class WindowParams
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
        if (this.WindowHandle != IntPtr.Zero)
        {
            return SDL.SDL_CreateWindowFrom(this.WindowHandle);
        }
        else
        {
            return SDL.SDL_CreateWindow(this.Title, this.X, this.Y, this.Width, this.Height, this.WindowFlags);
        }
    }
}
