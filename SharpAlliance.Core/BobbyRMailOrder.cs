using System;
using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core;

public class BobbyRMailOrder
{
}

public struct BobbyROrderLocationStruct
{
    public string psCityLoc;
    public int usOverNightExpress;
    public int us2DaysService;
    public int usStandardService;
}

//drop down menu
public enum BR_DROP_DOWN
{
    NO_ACTION,
    CREATE,
    DESTROY,
    DISPLAY,
};
