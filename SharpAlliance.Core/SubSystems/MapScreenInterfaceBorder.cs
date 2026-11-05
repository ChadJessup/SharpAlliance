using System;
using System.Diagnostics;
using SharpAlliance.Core.Screens;

namespace SharpAlliance.Core;

public class MapScreenInterfaceBorder
{
    public static bool fShowTownFlag { get; internal set; }
    public static bool fShowMineFlag { get; internal set; }
    public static bool fShowTeamFlag { get; internal set; }
    public static bool fShowMilitia { get; internal set; }
    public static bool fShowAircraftFlag { get; set; }
    public static bool fShowItemsFlag { get; internal set; }

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

    internal static void MapBorderButtonOn(MAP_BORDER ubBorderButtonIndex)
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

    internal static void MapBorderButtonOff(MAP_BORDER ubBorderButtonIndex)
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

    // turning on one overlay turns off the ones that would be drawn over it
    internal static void ToggleShowTownsMode()
    {
        if (fShowTownFlag)
        {
            fShowTownFlag = false;
            MapBorderButtonOff(MAP_BORDER.TOWN_BTN);
        }
        else
        {
            fShowTownFlag = true;
            MapBorderButtonOn(MAP_BORDER.TOWN_BTN);
            TurnOff(MAP_BORDER.MINE_BTN);
            TurnOff(MAP_BORDER.AIRSPACE_BTN);
            TurnOff(MAP_BORDER.ITEM_BTN);
        }

        fMapPanelDirty = true;
    }

    internal static void ToggleShowMinesMode()
    {
        if (fShowMineFlag)
        {
            fShowMineFlag = false;
            MapBorderButtonOff(MAP_BORDER.MINE_BTN);
        }
        else
        {
            fShowMineFlag = true;
            MapBorderButtonOn(MAP_BORDER.MINE_BTN);
            TurnOff(MAP_BORDER.TOWN_BTN);
            TurnOff(MAP_BORDER.AIRSPACE_BTN);
            TurnOff(MAP_BORDER.ITEM_BTN);
        }

        fMapPanelDirty = true;
    }

    internal static void ToggleShowMilitiaMode()
    {
        if (fShowMilitia)
        {
            fShowMilitia = false;
            MapBorderButtonOff(MAP_BORDER.MILITIA_BTN);
        }
        else
        {
            fShowMilitia = true;
            MapBorderButtonOn(MAP_BORDER.MILITIA_BTN);
            TurnOff(MAP_BORDER.TEAMS_BTN);
            TurnOff(MAP_BORDER.ITEM_BTN);

            // TODO(port): the message that explains the player has no militia yet (DoesPlayerHaveAnyMilitia)
        }

        fMapPanelDirty = true;
    }

    internal static void ToggleShowTeamsMode()
    {
        if (fShowTeamFlag)
        {
            // turn show teams OFF
            fShowTeamFlag = false;
            MapBorderButtonOff(MAP_BORDER.TEAMS_BTN);
            fMapPanelDirty = true;
            fTeamPanelDirty = true;
            fCharacterInfoPanelDirty = true;
        }
        else if (!fShowTeamFlag)
        {
            // turn show teams ON
            fShowTeamFlag = true;
            MapBorderButtonOn(MAP_BORDER.TEAMS_BTN);
            TurnOff(MAP_BORDER.MILITIA_BTN);
            TurnOff(MAP_BORDER.ITEM_BTN);
            fMapPanelDirty = true;
            fTeamPanelDirty = true;
            fCharacterInfoPanelDirty = true;
        }
    }

    internal static void ToggleItemsFilter()
    {
        if (fShowItemsFlag)
        {
            // turn items OFF
            fShowItemsFlag = false;
            MapBorderButtonOff(MAP_BORDER.ITEM_BTN);
        }
        else
        {
            // turn items ON, and everything it would hide behind off
            fShowItemsFlag = true;
            MapBorderButtonOn(MAP_BORDER.ITEM_BTN);
            TurnOff(MAP_BORDER.TOWN_BTN);
            TurnOff(MAP_BORDER.MINE_BTN);
            TurnOff(MAP_BORDER.TEAMS_BTN);
            TurnOff(MAP_BORDER.MILITIA_BTN);
            TurnOff(MAP_BORDER.AIRSPACE_BTN);

            if ((MapScreenInterfaceMap.bSelectedDestChar != -1) || MapScreenHelicopter.fPlotForHelicopter)
            {
                MapScreen.AbortMovementPlottingMode();
            }

            // if showing underground, switch to the surface
            if (MapScreenInterfaceMap.iCurrentMapSectorZ != 0)
            {
                MapScreenInterface.JumpToLevel(0);
            }
        }

        fMapPanelDirty = true;
        fTeamPanelDirty = true;
        fCharacterInfoPanelDirty = true;
    }

    private static void TurnOff(MAP_BORDER button)
    {
        bool wasOn = button switch
        {
            MAP_BORDER.TOWN_BTN => fShowTownFlag,
            MAP_BORDER.MINE_BTN => fShowMineFlag,
            MAP_BORDER.TEAMS_BTN => fShowTeamFlag,
            MAP_BORDER.AIRSPACE_BTN => fShowAircraftFlag,
            MAP_BORDER.ITEM_BTN => fShowItemsFlag,
            MAP_BORDER.MILITIA_BTN => fShowMilitia,
            _ => false,
        };

        if (!wasOn)
        {
            return;
        }

        switch (button)
        {
            case MAP_BORDER.TOWN_BTN: fShowTownFlag = false; break;
            case MAP_BORDER.MINE_BTN: fShowMineFlag = false; break;
            case MAP_BORDER.TEAMS_BTN: fShowTeamFlag = false; break;
            case MAP_BORDER.AIRSPACE_BTN: fShowAircraftFlag = false; break;
            case MAP_BORDER.ITEM_BTN: fShowItemsFlag = false; break;
            case MAP_BORDER.MILITIA_BTN: fShowMilitia = false; break;
        }

        MapBorderButtonOff(button);
    }

    /// <summary>Makes the pressed look of the buttons match the flags.</summary>
    internal static void InitializeMapBorderButtonStates()
    {
        void Set(bool on, MAP_BORDER button)
        {
            if (on)
            {
                MapBorderButtonOn(button);
            }
            else
            {
                MapBorderButtonOff(button);
            }
        }

        Set(fShowItemsFlag, MAP_BORDER.ITEM_BTN);
        Set(fShowTownFlag, MAP_BORDER.TOWN_BTN);
        Set(fShowMineFlag, MAP_BORDER.MINE_BTN);
        Set(fShowTeamFlag, MAP_BORDER.TEAMS_BTN);
        Set(fShowAircraftFlag, MAP_BORDER.AIRSPACE_BTN);
        Set(fShowMilitia, MAP_BORDER.MILITIA_BTN);
    }
}
