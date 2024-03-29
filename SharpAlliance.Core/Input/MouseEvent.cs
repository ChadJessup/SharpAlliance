﻿namespace SharpAlliance.Core;

public struct MouseEvent
{
    public MouseButton MouseButton { get; }
    public bool Down { get; }

    public MouseEvent(MouseButton button, bool down)
    {
        this.MouseButton = button;
        this.Down = down;
    }
}

public enum SDL_MouseButton : byte
{
    Left = 1,
    Middle = 2,
    Right = 3,
    X1 = 4,
    X2 = 5,
}


public enum MouseButton
{
    //
    // Summary:
    //     The left mouse button.
    Left = 0,
    //
    // Summary:
    //     The middle mouse button.
    Middle = 1,
    //
    // Summary:
    //     The right mouse button.
    Right = 2,
    //
    // Summary:
    //     The first extra mouse button.
    Button1 = 3,
    //
    // Summary:
    //     The second extra mouse button.
    Button2 = 4,
    //
    // Summary:
    //     The third extra mouse button.
    Button3 = 5,
    //
    // Summary:
    //     The fourth extra mouse button.
    Button4 = 6,
    //
    // Summary:
    //     The fifth extra mouse button.
    Button5 = 7,
    //
    // Summary:
    //     The sixth extra mouse button.
    Button6 = 8,
    //
    // Summary:
    //     The seventh extra mouse button.
    Button7 = 9,
    //
    // Summary:
    //     The eigth extra mouse button.
    Button8 = 10,
    //
    // Summary:
    //     The ninth extra mouse button.
    Button9 = 11,
    //
    // Summary:
    //     Indicates the last available mouse button.
    LastButton = 12
}
