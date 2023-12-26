using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpAlliance.Core.SubSystems;
using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core;

public partial class Globals
{
    public static TEAM_PANEL_SLOTS_TYPE[] gTeamPanel = new TEAM_PANEL_SLOTS_TYPE[NUM_TEAM_SLOTS];
    public const int NEW_ITEM_CYCLE_COUNT = 19;
    public const int NEW_ITEM_CYCLES = 4;
    public const int NUM_TEAM_SLOTS = 6;
    public const int PASSING_ITEM_DISTANCE_OKLIFE = 3;
    public const int PASSING_ITEM_DISTANCE_NOTOKLIFE = 2;
    public const int SHOW_LOCATOR_NORMAL = 1;
    public const int SHOW_LOCATOR_FAST = 2;

    // Globals for various mouse regions
    public static MOUSE_REGION gSM_SELMERCPanelRegion;
    public static MOUSE_REGION gSM_SELMERCBarsRegion;
    public static MOUSE_REGION gSM_SELMERCMoneyRegion;
    public static MOUSE_REGION gSM_SELMERCEnemyIndicatorRegion;
    public static MOUSE_REGION gTEAM_PanelRegion;
    public static MOUSE_REGION[] gTEAM_FaceRegions = new MOUSE_REGION[6];
    public static MOUSE_REGION[] gTEAM_BarsRegions = new MOUSE_REGION[6];
    public static MOUSE_REGION[] gTEAM_LeftBarsRegions = new MOUSE_REGION[6];
    public static MOUSE_REGION[] gTEAM_FirstHandInv = new MOUSE_REGION[6];
    public static MOUSE_REGION[] gTEAM_SecondHandInv = new MOUSE_REGION[6];
    public static MOUSE_REGION[] gTEAM_EnemyIndicator = new MOUSE_REGION[6];

    public static bool[,] gfTEAM_HandInvDispText = new bool[6, (int)InventorySlot.NUM_INV_SLOTS];
    public static bool[] gfSM_HandInvDispText = new bool[(int)InventorySlot.NUM_INV_SLOTS];
}

public struct TEAM_PANEL_SLOTS_TYPE
{
    public int ubID;
    public bool fOccupied;
}


public class InterfacePanel
{
    public static void RemoveAllPlayersFromSlot()
    {
        int cnt;

        for (cnt = 0; cnt < NUM_TEAM_SLOTS; cnt++)
        {
            RemovePlayerFromInterfaceTeamSlot(cnt);
        }
    }

    public static bool RemovePlayerFromInterfaceTeamSlot(int ubPanelSlot)
    {
        if (ubPanelSlot >= NUM_TEAM_SLOTS)
        {
            return false;
        }

        if (gTeamPanel[ubPanelSlot].fOccupied)
        {
            if (!MercPtrs[gTeamPanel[ubPanelSlot].ubID].uiStatusFlags.HasFlag(SOLDIER.DEAD))
            {
                // Set Id to close
                MercPtrs[gTeamPanel[ubPanelSlot].ubID].fUICloseMerc = true;
            }

            // Set face to inactive...
            Faces.SetAutoFaceInActive(MercPtrs[gTeamPanel[ubPanelSlot].ubID].iFaceIndex);

            gTeamPanel[ubPanelSlot].fOccupied = false;
            gTeamPanel[ubPanelSlot].ubID = NOBODY;

            MouseSubSystem.MSYS_SetRegionUserData(gTEAM_FirstHandInv[ubPanelSlot], 0, NOBODY);
            MouseSubSystem.MSYS_SetRegionUserData(gTEAM_FaceRegions[ubPanelSlot], 0, NOBODY);

            // DIRTY INTERFACE
            fInterfacePanelDirty = DIRTYLEVEL2;

            return true;
        }
        else
        {
            return false;
        }
    }

    public static void ShowRadioLocator(int ubID, SHOW_LOCATOR ubLocatorSpeed)
    {
        RESETTIMECOUNTER(ref MercPtrs[ubID].FlashSelCounter, FLASH_SELECTOR_DELAY);

        //LocateSoldier( ubID, FALSE );	// IC - this is already being done outside of this function :)
        MercPtrs[ubID].fFlashLocator = true;
        //gbPanelSelectedGuy = ubID;	IC - had to move this outside to make this function versatile
        MercPtrs[ubID].sLocatorFrame = 0;

        if (ubLocatorSpeed == SHOW_LOCATOR.NORMAL)
        {
            // If we are an AI guy, and we have the baton, make lower...
            // ( MercPtrs[ ubID ].uiStatusFlags.HasFlag(SOLDIER.UNDERAICONTROL && MercPtrs[ ubID ].bTeam != gbPlayerNum ))
            //
            //ercPtrs[ ubID ].ubNumLocateCycles = 3;
            //
            //se
            //
            MercPtrs[ubID].ubNumLocateCycles = 5;
            //
        }
        else
        {
            MercPtrs[ubID].ubNumLocateCycles = 3;
        }
    }
}

public enum SHOW_LOCATOR
{
    NORMAL = 1,
    FAST = 2,
}
