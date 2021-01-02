using System;

namespace SharpAlliance.Core
{
    [Flags]
    public enum KeyboardEvents
    {
        Unknown = 0x0000,
        KEY_DOWN = 0x0001,
        KEY_UP = 0x0002,
        KEY_REPEAT = 0x0004,
    }
}
