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
        this.Key = key;
        this.Down = down;
        this.Modifiers = modifiers;
        this.Repeat = repeat;
    }

    public override string ToString() => $"{this.Key} {(this.Down ? "Down" : "Up")} [{this.Modifiers}] (repeat={this.Repeat})";
}
