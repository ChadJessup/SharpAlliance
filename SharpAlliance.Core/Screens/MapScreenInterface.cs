using System;
using SixLabors.ImageSharp;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SharpAlliance.Core.Screens;

public class MapScreenInterface
{
    // as deep as the map goes
    public const int MAX_DEPTH_OF_MAP = 3;
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
            return;


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
                    new Rectangle((pMapScreenFastHelpLocationList[9].iX), 
                    (pMapScreenFastHelpLocationList[9].iY), 
                    (pMapScreenFastHelpLocationList[9].iX + pMapScreenFastHelpWidthList[9]), 
                    (pMapScreenFastHelpLocationList[9].iY + iHeightOfInitFastHelpText)),
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

    private static bool IsMapScreenHelpTextUp()
    {
        return (fShowMapScreenHelpText);
    }
}

// The character data structure
public struct MapScreenCharacterSt
{
    public int usSolID;// soldier ID in MenPtrs 
    public bool fValid;// is the current soldier a valid soldier

}
