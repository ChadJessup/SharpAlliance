using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;

namespace SharpAlliance.Core;

public class SimpleInputSnapshot : IInputSnapshot
{
    public List<KeyEvent> KeyEventsList { get; private set; } = new List<KeyEvent>();
    public List<MouseEvent> MouseEventsList { get; private set; } = new List<MouseEvent>();
    public List<char> KeyCharPressesList { get; private set; } = new List<char>();

    public IReadOnlyList<KeyEvent> KeyEvents => this.KeyEventsList;

    public IReadOnlyList<MouseEvent> MouseEvents => this.MouseEventsList;

    public IReadOnlyList<char> KeyCharPresses => this.KeyCharPressesList;

    public Vector2 MousePosition { get; set; }

    private bool[] _mouseDown = new bool[13];
    public bool[] MouseDown => this._mouseDown;
    public float WheelDelta { get; set; }

    public bool IsMouseDown(MouseButton button)
    {
        return this._mouseDown[(int)button];
    }

    public void Clear()
    {
        this.KeyEventsList.Clear();
        this.MouseEventsList.Clear();
        this.KeyCharPressesList.Clear();
        this.WheelDelta = 0f;
    }

    public void CopyTo(SimpleInputSnapshot other)
    {
        Debug.Assert(this != other);

        other.MouseEventsList.Clear();
        foreach (var me in this.MouseEventsList)
        { other.MouseEventsList.Add(me); }
        
        other.KeyEventsList.Clear();
        foreach (var ke in this.KeyEventsList)
        { other.KeyEventsList.Add(ke); }
        
        other.KeyCharPressesList.Clear();
        foreach (var kcp in this.KeyCharPressesList)
        { other.KeyCharPressesList.Add(kcp); }

        other.MousePosition = this.MousePosition;
        other.WheelDelta = this.WheelDelta;
        this._mouseDown.CopyTo(other._mouseDown, 0);
    }
}
