using System;

namespace SharpAlliance.Core;

public class MapScreenInterfaceBorder
{
    public static bool fShowTownFlag { get; private set; }
    public static bool fShowMineFlag { get; private set; }
    public static bool fShowTeamFlag { get; private set; }
    public static bool fShowMilitia { get; private set; }
    public static bool fShowAircraftFlag { get; set; }
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

    internal static void ToggleAirspaceMode()
    {
        if (fShowAircraftFlag == true)
        {
            // turn airspace OFF
            fShowAircraftFlag = false;
            MapBorderButtonOff(MAP_BORDER_AIRSPACE_BTN);

            if (MapScreenHelicopter.fPlotForHelicopter == true)
            {
                AbortMovementPlottingMode();
            }

            // dirty regions
            fMapPanelDirty = true;
            fTeamPanelDirty = true;
            fCharacterInfoPanelDirty = true;
        }
        else
        {   // turn airspace ON
            TurnOnAirSpaceMode();
        }
    }
}
