using System.Numerics;

namespace SharpAlliance.Core;

public struct MouseMoveEventArgs
{
    public MouseState State { get; }
    public Vector2 MousePosition { get; }
    public MouseMoveEventArgs(MouseState mouseState, Vector2 mousePosition)
    {
        this.State = mouseState;
        this.MousePosition = mousePosition;
    }
}
