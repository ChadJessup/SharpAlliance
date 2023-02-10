using System;

namespace SharpAlliance.Core.SubSystems;

public class Interface
{
    // this might need to be an int, we'll see.
    public InterfaceLevel gsInterfaceLevel { get; set; }
    public InterfacePanelDefines gsCurInterfacePanel { get; internal set; }
}

public enum InterfacePanelDefines
{
    SM_PANEL,
    TEAM_PANEL,
    NUM_UI_PANELS
}

// Interface level enums
public enum InterfaceLevel
{
    I_GROUND_LEVEL,
    I_ROOF_LEVEL,
    I_NUMLEVELS
};

// GLOBAL DEFINES FOR SOME UI FLAGS
[Flags]
public enum ARROWS
{
    HIDE_UP = 0x00000002,
    HIDE_DOWN = 0x00000004,
    SHOW_UP_BESIDE = 0x00000008,
    SHOW_DOWN_BESIDE = 0x00000020,
    SHOW_UP_ABOVE_Y = 0x00000040,
    SHOW_DOWN_BELOW_Y = 0x00000080,
    SHOW_DOWN_BELOW_G = 0x00000200,
    SHOW_DOWN_BELOW_YG = 0x00000400,
    SHOW_DOWN_BELOW_GG = 0x00000800,
    SHOW_UP_ABOVE_G = 0x00002000,
    SHOW_UP_ABOVE_YG = 0x00004000,
    SHOW_UP_ABOVE_GG = 0x00008000,
    SHOW_UP_ABOVE_YY = 0x00020000,
    SHOW_DOWN_BELOW_YY = 0x00040000,
    SHOW_UP_ABOVE_CLIMB = 0x00080000,
    SHOW_UP_ABOVE_CLIMB2 = 0x00400000,
    SHOW_UP_ABOVE_CLIMB3 = 0x00800000,
    SHOW_DOWN_CLIMB = 0x02000000,
}
