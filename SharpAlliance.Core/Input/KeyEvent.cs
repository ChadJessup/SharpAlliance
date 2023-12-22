namespace SharpAlliance.Core;

public struct KeyEvent
{
    public Key Key { get; }
    public bool Down { get; }
    public ModifierKeys Modifiers { get; }
    public bool Repeat { get; }

    public KeyEvent(Key key, bool down, ModifierKeys modifiers)
    : this(key, down, modifiers, false)
    {
    }

    public KeyEvent(Key key, bool down, ModifierKeys modifiers, bool repeat)
    {
        Key = key;
        Down = down;
        Modifiers = modifiers;
        Repeat = repeat;
    }

    public override string ToString() => $"{Key} {(Down ? "Down" : "Up")} [{Modifiers}] (repeat={Repeat})";
}
