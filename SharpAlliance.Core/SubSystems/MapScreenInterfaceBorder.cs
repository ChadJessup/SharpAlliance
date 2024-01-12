using System;

namespace SharpAlliance.Core;

public class MapScreenInterfaceBorder
{
    public static bool fShowTownFlag { get; private set; }
    public static bool fShowMineFlag { get; private set; }
    public static bool fShowTeamFlag { get; private set; }
    public static bool fShowMilitia { get; private set; }
    public static bool fShowAircraftFlag { get; private set; }
    public static bool fShowItemsFlag { get; private set; }

    public static void InitMapScreenFlags()
    {
        fShowTownFlag = true;
        fShowMineFlag = false;

        fShowTeamFlag = true;
        fShowMilitia = false;

        fShowAircraftFlag = false;
        fShowItemsFlag = false;
    }
}
