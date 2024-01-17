using System;
using System.Diagnostics;
using SharpAlliance.Core.Screens;

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
            MapBorderButtonOff(MAP_BORDER.AIRSPACE_BTN);

            if (MapScreenHelicopter.fPlotForHelicopter == true)
            {
                MapScreen.AbortMovementPlottingMode();
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

    private static void TurnOnAirSpaceMode()
    {
        // if mode already on, leave, else set and redraw

        if (fShowAircraftFlag == false)
        {
            fShowAircraftFlag = true;
            MapBorderButtonOn(MAP_BORDER.AIRSPACE_BTN);


            // Turn off towns & mines (mostly because town/mine names overlap SAM site names)
            if (fShowTownFlag == true)
            {
                fShowTownFlag = false;
                MapBorderButtonOff(MAP_BORDER.TOWN_BTN);
            }

            if (fShowMineFlag == true)
            {
                fShowMineFlag = false;
                MapBorderButtonOff(MAP_BORDER.MINE_BTN);
            }

            /*
                    // Turn off teams and militia
                    if( fShowTeamFlag == true )
                    {
                        fShowTeamFlag = false;
                        MapBorderButtonOff( MAP_BORDER_TEAMS_BTN );
                    }

                    if( fShowMilitia == true )
                    {
                        fShowMilitia = false;
                        MapBorderButtonOff( MAP_BORDER_MILITIA_BTN );
                    }
            */

            // Turn off items
            if (fShowItemsFlag == true)
            {
                fShowItemsFlag = false;
                MapBorderButtonOff(MAP_BORDER.ITEM_BTN);
            }

            if (MapScreenInterfaceMap.bSelectedDestChar != -1)
            {
                MapScreen.AbortMovementPlottingMode();
            }


            // if showing underground		
            if (MapScreenInterfaceMap.iCurrentMapSectorZ != 0)
            {
                // switch to the surface
                MapScreenInterface.JumpToLevel(0);
            }

            // dirty regions
            fMapPanelDirty = true;
            fTeamPanelDirty = true;
            fCharacterInfoPanelDirty = true;
        }
    }

    private static void MapBorderButtonOn(MAP_BORDER ubBorderButtonIndex)
    {
        if (fShowMapInventoryPool)
        {
            return;
        }

        // if button doesn't exist, return
        if (giMapBorderButtons[ubBorderButtonIndex] == null)
        {
            return;
        }

        giMapBorderButtons[ubBorderButtonIndex].uiFlags |= ButtonFlags.BUTTON_CLICKED_ON;
    }

    private static void MapBorderButtonOff(MAP_BORDER ubBorderButtonIndex)
    {
        if (fShowMapInventoryPool)
        {
            return;
        }

        // if button doesn't exist, return
        if (giMapBorderButtons[ubBorderButtonIndex] == null)
        {
            return;
        }

        giMapBorderButtons[ubBorderButtonIndex].uiFlags &= ~ButtonFlags.BUTTON_CLICKED_ON;
    }
}
