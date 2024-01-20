using System;
using SixLabors.ImageSharp;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SharpAlliance.Core.Screens;

public class MapScreenInterface
{
    // map screen font
    private const FontStyle MAP_SCREEN_FONT = FontStyle.BLOCKFONT2;

    // as deep as the map goes
    public const int MAX_DEPTH_OF_MAP = 3;
    // characterlist regions
    private const int Y_START = 146;
    private const int MAP_START_KEYRING_Y = 107;
    private static int Y_SIZE { get; } = FontSubSystem.GetFontHeight(MAP_SCREEN_FONT);

    private static MOUSE_REGION? gMapScreenHelpTextMask;
    private static int iHeightOfInitFastHelpText = 0;

    internal static int giDestHighLine;
    private static MapScreenCharacterSt[] gCharactersList = new MapScreenCharacterSt[MAX_CHARACTER_COUNT + 1];
    private static bool fShowMapScreenHelpText;



    internal static void DoMapMessageBoxWithRect(MessageBoxStyle mSG_BOX_BASIC_STYLE, string zString, ScreenName mAP_SCREEN, MSG_BOX_FLAG usFlags, MSGBOX_CALLBACK? returnCallback, Rectangle? pCenteringRect)
    {
        throw new NotImplementedException();
    }

    public static void JumpToLevel(int iLevel)
    {
        if (IsMapScreenHelpTextUp())
        {
            // stop mapscreen text
            StopMapScreenHelpText();
            return;
        }

        if (gfPreBattleInterfaceActive == true)
        {
            return;
        }

        // disable level-changes while in inventory pool (for keyboard equivalents!)
        if (fShowMapInventoryPool)
        {
            return;
        }

        if ((MapScreenInterfaceMap.bSelectedDestChar != -1) || (MapScreenHelicopter.fPlotForHelicopter == true))
        {
            MapScreen.AbortMovementPlottingMode();
        }

        if (iLevel < 0)
        {
            iLevel = 0;
        }

        if (iLevel > MAX_DEPTH_OF_MAP)
        {
            iLevel = MAX_DEPTH_OF_MAP;
        }

        // set current sector Z to level passed
        MapScreen.ChangeSelectedMapSector(MapScreenInterfaceMap.sSelMapX, MapScreenInterfaceMap.sSelMapY, iLevel);

    }

    private static void StopMapScreenHelpText()
    {
        fShowMapScreenHelpText = false;
        fTeamPanelDirty = true;
        fMapPanelDirty = true;
        fCharacterInfoPanelDirty = true;
        fMapScreenBottomDirty = true;

        SetUpShutDownMapScreenHelpTextScreenMask();
    }

    private static bool fCreated = false;
    private static bool fInterfaceFastHelpTextActive;

    private static void SetUpShutDownMapScreenHelpTextScreenMask()
    {

        // create or destroy the screen mask as needed
        if (((fShowMapScreenHelpText == true) || (fInterfaceFastHelpTextActive == true)) && (fCreated == false))
        {
            if (gTacticalStatus.fDidGameJustStart)
            {
                MouseSubSystem.MSYS_DefineRegion(
                    gMapScreenHelpTextMask,
                    new Rectangle((MapScreenInterfaceMap.pMapScreenFastHelpLocationList[9].X),
                    (MapScreenInterfaceMap.pMapScreenFastHelpLocationList[9].Y),
                    (MapScreenInterfaceMap.pMapScreenFastHelpLocationList[9].X + MapScreenInterfaceMap.pMapScreenFastHelpWidthList[9]),
                    (MapScreenInterfaceMap.pMapScreenFastHelpLocationList[9].Y + iHeightOfInitFastHelpText)),
                    MSYS_PRIORITY.HIGHEST,
                    MSYS_NO_CURSOR,
                    MSYS_NO_CALLBACK,
                    MapScreenHelpTextScreenMaskBtnCallback);
            }
            else
            {
                MouseSubSystem.MSYS_DefineRegion(
                    gMapScreenHelpTextMask,
                    new Rectangle(0, 0, 640, 480),
                    MSYS_PRIORITY.HIGHEST,
                    MSYS_NO_CURSOR,
                    MSYS_NO_CALLBACK,
                    MapScreenHelpTextScreenMaskBtnCallback);
            }

            fCreated = true;

        }
        else if ((fShowMapScreenHelpText == false) && (fInterfaceFastHelpTextActive == false) && (fCreated == true))
        {
            MouseSubSystem.MSYS_RemoveRegion(gMapScreenHelpTextMask);

            fCreated = false;
        }
    }

    private static void MapScreenHelpTextScreenMaskBtnCallback(ref MOUSE_REGION region, MSYS_CALLBACK_REASON iReason)
    {
        if (iReason.HasFlag(MSYS_CALLBACK_REASON.RBUTTON_UP))
        {
            // stop showing
            ShutDownUserDefineHelpTextRegions();
        }
        else if (iReason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP))
        {
            // stop showing
            ShutDownUserDefineHelpTextRegions();
        }

    }

    private static void ShutDownUserDefineHelpTextRegions()
    {
        // dirty the tactical panel
        fInterfacePanelDirty = DIRTYLEVEL2;
        RenderWorld.SetRenderFlags(RenderingFlags.FULL);

        //dirty the map panel
        StopMapScreenHelpText();

        //r eset tactical flag too
        StopShowingInterfaceFastHelpText();

    }

    public static void InitalizeVehicleAndCharacterList()
    {
        // will init the vehicle and character lists to zero
        Array.ForEach(gCharactersList, c => new MapScreenCharacterSt());

        return;
    }


    private static void StopShowingInterfaceFastHelpText()
    {
        fInterfaceFastHelpTextActive = false;
    }

    private static bool IsMapScreenHelpTextUp()
    {
        return (fShowMapScreenHelpText);
    }

    private static int iOldDestinationLine = -1;
    internal static bool fShowMapScreenMovementList;
    internal static bool fShowAssignmentMenu;
    // which menus are we showing
    public static bool fShowTrainingMenu = false;
    private static bool fShowAttributeMenu = false;
    private static bool fShowSquadMenu = false;
    public static bool fShowContractMenu = false;
    private static bool fShowRemoveMenu = false;

    internal static void RestoreBackgroundForDestinationGlowRegionList()
    {
        // will restore the background region of the destinationz list after a glow has ceased
        // ( a _LOST_MOUSE reason to the assignment region mvt callback handler )

        if (fDisableDueToBattleRoster)
        {
            return;
        }

        if (iOldDestinationLine != giDestHighLine)
        {
            // restore background
            RenderDirty.RestoreExternBackgroundRect(182, Y_START - 1, 217 + 1 - 182, (((MAX_CHARACTER_COUNT + 1) * (Y_SIZE + 2)) + 1));

            // ARM: not good enough! must reblit the whole panel to erase glow chunk restored by help text disappearing!!!
            fTeamPanelDirty = true;

            // set old to current
            iOldDestinationLine = giDestHighLine;
        }
    }
}

// The character data structure
public struct MapScreenCharacterSt
{
    public int usSolID;// soldier ID in MenPtrs 
    public bool fValid;// is the current soldier a valid soldier

}
