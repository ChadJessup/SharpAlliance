using System;

namespace SharpAlliance.Platform
{
    [Flags]
    public enum MouseEvents
    {
        Unknown = 0x0000,
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

    public struct InputAtom
    {
        public InputAtom(InputAtom other)
        {
            this.uiTimeStamp = other.uiTimeStamp;
            this.usKeyState = other.usKeyState;
            this.MouseEvents = other.MouseEvents;
            this.usParam = other.usParam;
            this.uiParam = other.uiParam;
        }

        public int uiTimeStamp;
        public int usKeyState;
        public MouseEvents MouseEvents;
        public int usParam;
        public int uiParam;
    }
}
