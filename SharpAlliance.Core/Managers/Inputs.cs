using System;

using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core.Managers;

[Flags]
public enum Inputs
{
    KEY_DOWN = 0x0001,
    KEY_UP = 0x0002,
    KEY_REPEAT = 0x0004,
    LEFT_BUTTON_DOWN = 0x0008,
    LEFT_BUTTON_UP = 0x0010,
    LEFT_BUTTON_DBL_CLK = 0x0020,
    LEFT_BUTTON_REPEAT = 0x0040,
    RIGHT_BUTTON_DOWN = 0x0080,
    RIGHT_BUTTON_UP = 0x0100,
    RIGHT_BUTTON_REPEAT = 0x0200,
    MOUSE_POS = 0x0400,
    MOUSE_WHEEL = 0x0800,
}
