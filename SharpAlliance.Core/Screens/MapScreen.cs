using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using SharpAlliance.Core.Interfaces;
using SharpAlliance.Core.Managers;

using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core.Screens;

public class MapScreen : IScreen
{
    private readonly IVideoManager video;
    private readonly MapScreenInterfaceMap mapScreenInterface;
    private readonly MessageSubSystem messages;
    public static List<MERC_LEAVE_ITEM?> gpLeaveListHead = [];
    private static Path? gpHelicopterPreviousMercPath = null;
    private static Path?[] gpCharacterPreviousMercPath = new Path?[MAX_CHARACTER_COUNT];
    private static bool[] fSelectedListOfMercsForMapScreen = new bool[MAX_CHARACTER_COUNT];
    private static MOUSE_REGION? gMapScreenMaskRegion;
    private static bool fCheckCursorWasSet;
    private static bool gfInChangeArrivalSectorMode;
    private static bool gfInConfirmMapMoveMode;

    public MapScreen(
        MapScreenInterfaceMap mapScreenInterfaceMap,
        IVideoManager videoManager,
        MessageSubSystem messageSubSystem)
    {
        this.video = videoManager;
        this.mapScreenInterface = mapScreenInterfaceMap;
        this.messages = messageSubSystem;
    }

    public bool IsInitialized { get; set; }
    public ScreenState State { get; set; }
    public static IEnumerable<GUI_BUTTON> buttonList { get; private set; }

    public ValueTask Activate()
    {
        return ValueTask.CompletedTask;
    }

    public ValueTask<ScreenName> Handle()
    {
        return ValueTask.FromResult(ScreenName.MAP_SCREEN);
    }

    public ValueTask<bool> Initialize()
    {
        this.mapScreenInterface.SetUpBadSectorsList();

        // setup message box system
        this.messages.InitGlobalMessageList();

        // init palettes for big map
        this.mapScreenInterface.InitializePalettesForMap();

        // set up mapscreen fast help text
        this.mapScreenInterface.SetUpMapScreenFastHelpText();

        // set up leave list arrays for dismissed mercs
        MapScreenInterfaceMap.InitLeaveList();

        this.video.GetVideoObject("INTERFACE\\group_confirm.sti", out var idx1);
        this.mapScreenInterface.guiUpdatePanel = idx1;

        this.video.GetVideoObject("INTERFACE\\group_confirm_tactical.sti", out var idx2);
        this.mapScreenInterface.guiUpdatePanelTactical = idx2;

        return ValueTask.FromResult(true);
    }

    public void HandlePreloadOfMapGraphics()
    {
    }

    public void Dispose()
    {
    }

    public static void RenderMapRegionBackground()
    {
        // renders to save buffer when dirty flag set

        if (Globals.fMapPanelDirty == false)
        {
            Globals.gfMapPanelWasRedrawn = false;

            // not dirty, leave
            return;
        }

        // don't bother if showing sector inventory instead of the map!!!
        if (!Globals.fShowMapInventoryPool)
        {
            // draw map
            MapScreenInterfaceMap.DrawMap();
        }

        // blit in border
        MapScreenInterfaceMap.RenderMapBorder();

        if (Globals.ghAttributeBox != -1)
        {
            PopUpBox.ForceUpDateOfBox(Globals.ghAttributeBox);
        }

        if (Globals.ghTownMineBox != -1)
        {
            // force update of town mine info boxes
            PopUpBox.ForceUpDateOfBox(Globals.ghTownMineBox);
        }

        MapScreen.MapscreenMarkButtonsDirty();

        RenderDirty.RestoreExternBackgroundRect(261, 0, 640 - 261, 359);

        // don't bother if showing sector inventory instead of the map!!!
        if (!Globals.fShowMapInventoryPool)
        {
            // if Skyrider can and wants to talk to us
            if (MapScreenHelicopter.IsHelicopterPilotAvailable())
            {
                // see if Skyrider has anything new to tell us
                MapScreenHelicopter.CheckAndHandleSkyriderMonologues();
            }
        }

        // reset dirty flag
        Globals.fMapPanelDirty = false;

        Globals.gfMapPanelWasRedrawn = true;

        return;
    }

    public void Draw(IVideoManager videoManager)
    {
        throw new System.NotImplementedException();
    }

    public ValueTask Deactivate()
    {
        throw new System.NotImplementedException();
    }

    public static void MapscreenMarkButtonsDirty()
    {
        // redraw buttons
        ButtonSubSystem.MarkButtonsDirty(buttonList);

        // if border buttons are created
        if (!Globals.fShowMapInventoryPool)
        {
            // if the attribute assignment menu is showing
            if (Globals.fShowAttributeMenu)
            {
                // don't redraw the town button, it would wipe out a chunk of the attribute menu
                ButtonSubSystem.UnMarkButtonDirty(Globals.giMapBorderButtons[(int)MAP_BORDER.TOWN_BTN]);
            }
        }
    }

    internal static void ShutDownLeaveList()
    {
        gpLeaveListHead.Clear();
    }

    internal static void ChangeSelectedMapSector(int sMapX, MAP_ROW sMapY, int bMapZ)
    {
        // ignore while map inventory pool is showing, or else items can be replicated, since sector inventory always applies
        // only to the currently selected sector!!!
        if (fShowMapInventoryPool)
        {
            return;
        }

        if (gfPreBattleInterfaceActive)
        {
            return;
        }

        if (!MapScreenInterfaceMap.IsTheCursorAllowedToHighLightThisSector(sMapX, sMapY))
        {
            return;
        }

        // disallow going underground while plotting (surface) movement
        if ((bMapZ != 0) && ((MapScreenInterfaceMap.bSelectedDestChar != -1) || MapScreenHelicopter.fPlotForHelicopter))
        {
            return;
        }


        MapScreenInterfaceMap.sSelMapX = sMapX;
        MapScreenInterfaceMap.sSelMapY = sMapY;
        MapScreenInterfaceMap.iCurrentMapSectorZ = bMapZ;

        // if going underground while in airspace mode
        if ((bMapZ > 0) && (MapScreenInterfaceBorder.fShowAircraftFlag == true))
        {
            // turn off airspace mode
            MapScreenInterfaceBorder.ToggleAirspaceMode();
        }

        fMapPanelDirty = true;
        fMapScreenBottomDirty = true;

        // also need this, to update the text coloring of mercs in this sector
        fTeamPanelDirty = true;
    }

    internal static void AbortMovementPlottingMode()
    {
        // invalid if we're not plotting movement
        Debug.Assert((MapScreenInterfaceMap.bSelectedDestChar != -1) || (MapScreenHelicopter.fPlotForHelicopter == true));

        // make everybody go back to where they were going before this plotting session started
        MapScreen.RestorePreviousPaths();

        // don't need the previous paths any more
        MapScreen.ClearPreviousPaths();

        // clear the character's temporary path (this is the route being constantly updated on the map)
        if (pTempCharacterPath is not null)
        {
            // make sure we're at the beginning
            pTempCharacterPath = StrategicPathing.MoveToBeginningOfPathList(ref pTempCharacterPath);
            pTempCharacterPath = StrategicPathing.ClearStrategicPathList(ref pTempCharacterPath, 0);
        }

        // clear the helicopter's temporary path (this is the route being constantly updated on the map)
        if (pTempHelicopterPath is not null)
        {
            // make sure we're at the beginning
            pTempHelicopterPath = StrategicPathing.MoveToBeginningOfPathList(ref pTempHelicopterPath);
            pTempHelicopterPath = StrategicPathing.ClearStrategicPathList(ref pTempHelicopterPath, 0);
        }


        MapScreen.EndConfirmMapMoveMode();

        // cancel destination line highlight
        MapScreenInterface.giDestHighLine = -1;

        // cancel movement mode
        MapScreenInterfaceMap.bSelectedDestChar = -1;
        MapScreenHelicopter.fPlotForHelicopter = false;

        // tell player the route was UNCHANGED
        Messages.MapScreenMessage(FontColor.FONT_MCOLOR_LTYELLOW, MSG.MAP_UI_POSITION_MIDDLE, pMapPlotStrings[2]);


        // reset cursors
        MapScreen.ChangeMapScreenMaskCursor(CURSOR.NORMAL);
        MapScreen.SetUpCursorForStrategicMap();

        // restore glow region
        MapScreenInterface.RestoreBackgroundForDestinationGlowRegionList();

        // we might be on the map, redraw to remove old path stuff
        fMapPanelDirty = true;
        fTeamPanelDirty = true;

        gfRenderPBInterface = true;
    }

    private static void SetUpCursorForStrategicMap()
    {
        if (MapScreen.gfInChangeArrivalSectorMode == false)
        {
            // check if character is in destination plotting mode
            if (MapScreenHelicopter.fPlotForHelicopter == false)
            {
                if (MapScreenInterfaceMap.bSelectedDestChar == -1)
                {
                    // no plot mode, reset cursor to normal
                    ChangeMapScreenMaskCursor(CURSOR.NORMAL);
                }
                else    // yes - by character
                {
                    // set cursor based on foot or vehicle
                    if ((Menptr[gCharactersList[MapScreenInterfaceMap.bSelectedDestChar].usSolID].bAssignment != Assignment.VEHICLE) && !(Menptr[gCharactersList[MapScreenInterfaceMap.bSelectedDestChar].usSolID].uiStatusFlags.HasFlag(SOLDIER.VEHICLE)))
                    {
                        ChangeMapScreenMaskCursor(CURSOR.STRATEGIC_FOOT);
                    }
                    else
                    {
                        ChangeMapScreenMaskCursor(CURSOR.STRATEGIC_VEHICLE);
                    }
                }
            }
            else    // yes - by helicopter
            {
                // set cursor to chopper
                ChangeMapScreenMaskCursor(CURSOR.CHOPPER);
            }
        }
        else
        {
            // set cursor to bullseye
            ChangeMapScreenMaskCursor(CURSOR.STRATEGIC_BULLSEYE);
        }
    }

    private static void ChangeMapScreenMaskCursor(CURSOR usCursor)
    {
        MouseSubSystem.MSYS_SetCurrentCursor(usCursor);
        MouseSubSystem.MSYS_ChangeRegionCursor(MapScreen.gMapScreenMaskRegion, usCursor);

        if (usCursor == CURSOR.CHECKMARK)
        {
            fCheckCursorWasSet = true;
        }
        else
        {
            fCheckCursorWasSet = false;
        }

        if (usCursor == CURSOR.NORMAL)
        {
            if (!InterfaceItems.InItemStackPopup())
            {
                // cancel mouse restriction
                CursorSubSystem.FreeMouseCursor();
            }
        }
        else
        {
            // restrict mouse cursor to the map area
            MouseSubSystem.RestrictMouseCursor(MapScreenInterfaceMap.MapScreenRect);
        }
    }

    private static void EndConfirmMapMoveMode()
    {
        CancelMapUIMessage();

        gfInConfirmMapMoveMode = false;
    }

    private static void CancelMapUIMessage()
    {
        // and kill the message overlay
        Interface.EndUIMessage();

        fMapPanelDirty = true;
    }

    private static void ClearPreviousPaths()
    {
        for (int iCounter = 0; iCounter < MAX_CHARACTER_COUNT; iCounter++)
        {
            if (MapScreen.fSelectedListOfMercsForMapScreen[iCounter] == true)
            {
                gpCharacterPreviousMercPath[iCounter] = StrategicPathing.ClearStrategicPathList(ref gpCharacterPreviousMercPath[iCounter], 0);
            }
        }
        gpHelicopterPreviousMercPath = StrategicPathing.ClearStrategicPathList(ref gpHelicopterPreviousMercPath, 0);

    }

    private static void RestorePreviousPaths()
    {
        SOLDIERTYPE? pSoldier = null;
        Path? ppMovePath = null;
        int ubGroupId = 0;
        bool fPathChanged = false;


        // invalid if we're not plotting movement
        Debug.Assert((MapScreenInterfaceMap.bSelectedDestChar != -1) || (MapScreenHelicopter.fPlotForHelicopter == true));


        if (MapScreenHelicopter.fPlotForHelicopter == true)
        {
            ppMovePath = (pVehicleList[iHelicopterVehicleId].pMercPath);
            ubGroupId = pVehicleList[iHelicopterVehicleId].ubMovementGroup;

            // if the helicopter had a previous path
            if (gpHelicopterPreviousMercPath != null)
            {
                gpHelicopterPreviousMercPath = StrategicPathing.MoveToBeginningOfPathList(MapScreen.gpHelicopterPreviousMercPath);

                // clear current path
                ppMovePath = StrategicPathing.ClearStrategicPathList(ref ppMovePath, ubGroupId);
                // replace it with the previous one
                ppMovePath = StrategicPathing.CopyPaths(gpHelicopterPreviousMercPath, ppMovePath);
                // will need to rebuild waypoints
                fPathChanged = true;
            }
            else    // no previous path
            {
                // if he currently has a path
                if (ppMovePath is not null)
                {
                    // wipe it out!
                    ppMovePath = StrategicPathing.MoveToBeginningOfPathList(ref ppMovePath);
                    ppMovePath = StrategicPathing.ClearStrategicPathList(ref ppMovePath, ubGroupId);
                    // will need to rebuild waypoints
                    fPathChanged = true;
                }
            }

            if (fPathChanged)
            {
                // rebuild waypoints
                StrategicPathing.RebuildWayPointsForGroupPath(ref ppMovePath, ubGroupId);

                // copy his path to all selected characters
                CopyPathToAllSelectedCharacters(ref ppMovePath);
            }
        }
        else    // character(s) plotting
        {
            for (int iCounter = 0; iCounter < MAX_CHARACTER_COUNT; iCounter++)
            {
                // if selected
                if (fSelectedListOfMercsForMapScreen[iCounter] == true)
                {
                    pSoldier = MercPtrs[gCharactersList[iCounter].usSolID];

                    if (pSoldier.uiStatusFlags.HasFlag(SOLDIER.VEHICLE))
                    {
                        ppMovePath = (pVehicleList[pSoldier.bVehicleID].pMercPath);
                        ubGroupId = pVehicleList[pSoldier.bVehicleID].ubMovementGroup;
                    }
                    else if (pSoldier.bAssignment == Assignment.VEHICLE)
                    {
                        ppMovePath = (pVehicleList[pSoldier.iVehicleId].pMercPath);
                        ubGroupId = pVehicleList[pSoldier.iVehicleId].ubMovementGroup;
                    }
                    else if (pSoldier.bAssignment < Assignment.ON_DUTY)
                    {
                        ppMovePath = (pSoldier.pMercPath);
                        ubGroupId = pSoldier.ubGroupID;
                    }
                    else
                    {
                        // invalid pSoldier - that guy can't possibly be moving, he's on a non-vehicle assignment!
                        Debug.Assert(false);
                        continue;
                    }


                    fPathChanged = false;

                    // if we have the previous path stored for the dest char
                    if (gpCharacterPreviousMercPath[iCounter] is not null)
                    {
                        gpCharacterPreviousMercPath[iCounter] = StrategicPathing.MoveToBeginningOfPathList(gpCharacterPreviousMercPath[iCounter]);

                        // clear current path
                        ppMovePath = StrategicPathing.ClearStrategicPathList(ref ppMovePath, ubGroupId);
                        // replace it with the previous one
                        ppMovePath = StrategicPathing.CopyPaths(gpCharacterPreviousMercPath[iCounter], ppMovePath);
                        // will need to rebuild waypoints
                        fPathChanged = true;
                    }
                    else    // no previous path stored
                    {
                        // if he has one now, wipe it out
                        if (ppMovePath is not null)
                        {
                            // wipe it out!
                            ppMovePath = StrategicPathing.MoveToBeginningOfPathList(ref ppMovePath);
                            ppMovePath = StrategicPathing.ClearStrategicPathList(ref ppMovePath, ubGroupId);
                            // will need to rebuild waypoints
                            fPathChanged = true;
                        }
                    }


                    if (fPathChanged)
                    {
                        // rebuild waypoints
                        StrategicPathing.RebuildWayPointsForGroupPath(ref ppMovePath, ubGroupId);
                    }
                }
            }
        }
    }

    private static void RestorePreviousPathsint()
    {
        SOLDIERTYPE? pSoldier = null;
        Path? ppMovePath = null;
        int ubGroupId = 0;
        bool fPathChanged = false;

        // invalid if we're not plotting movement
        Debug.Assert((MapScreenInterfaceMap.bSelectedDestChar != -1) || (MapScreenHelicopter.fPlotForHelicopter == true));


        if (MapScreenHelicopter.fPlotForHelicopter == true)
        {
            ppMovePath = (pVehicleList[iHelicopterVehicleId].pMercPath);
            ubGroupId = pVehicleList[iHelicopterVehicleId].ubMovementGroup;

            // if the helicopter had a previous path
            if (gpHelicopterPreviousMercPath != null)
            {
                gpHelicopterPreviousMercPath = StrategicPathing.MoveToBeginningOfPathList(gpHelicopterPreviousMercPath);

                // clear current path
                ppMovePath = StrategicPathing.ClearStrategicPathList(ref ppMovePath, ubGroupId);
                // replace it with the previous one
                ppMovePath = StrategicPathing.CopyPaths(gpHelicopterPreviousMercPath, ppMovePath);
                // will need to rebuild waypoints
                fPathChanged = true;
            }
            else    // no previous path
            {
                // if he currently has a path
                if (ppMovePath is not null)
                {
                    // wipe it out!
                    ppMovePath = StrategicPathing.MoveToBeginningOfPathList(ref ppMovePath);
                    ppMovePath = StrategicPathing.ClearStrategicPathList(ref ppMovePath, ubGroupId);
                    // will need to rebuild waypoints
                    fPathChanged = true;
                }
            }

            if (fPathChanged)
            {
                // rebuild waypoints
                StrategicPathing.RebuildWayPointsForGroupPath(ref ppMovePath, ubGroupId);

                // copy his path to all selected characters
                MapScreen.CopyPathToAllSelectedCharacters(ref ppMovePath);
            }
        }
        else    // character(s) plotting
        {
            for (int iCounter = 0; iCounter < MAX_CHARACTER_COUNT; iCounter++)
            {
                // if selected
                if (fSelectedListOfMercsForMapScreen[iCounter] == true)
                {
                    pSoldier = MercPtrs[gCharactersList[iCounter].usSolID];

                    if (pSoldier.uiStatusFlags.HasFlag(SOLDIER.VEHICLE))
                    {
                        ppMovePath = (pVehicleList[pSoldier.bVehicleID].pMercPath);
                        ubGroupId = pVehicleList[pSoldier.bVehicleID].ubMovementGroup;
                    }
                    else if (pSoldier.bAssignment == Assignment.VEHICLE)
                    {
                        ppMovePath = (pVehicleList[pSoldier.iVehicleId].pMercPath);
                        ubGroupId = pVehicleList[pSoldier.iVehicleId].ubMovementGroup;
                    }
                    else if (pSoldier.bAssignment < Assignment.ON_DUTY)
                    {
                        ppMovePath = (pSoldier.pMercPath);
                        ubGroupId = pSoldier.ubGroupID;
                    }
                    else
                    {
                        // invalid pSoldier - that guy can't possibly be moving, he's on a non-vehicle assignment!
                        Debug.Assert(false);
                        continue;
                    }

                    fPathChanged = false;

                    // if we have the previous path stored for the dest char
                    if (gpCharacterPreviousMercPath[iCounter] is not null)
                    {
                        gpCharacterPreviousMercPath[iCounter] = StrategicPathing.MoveToBeginningOfPathList(MapScreen.gpCharacterPreviousMercPath[iCounter]);

                        // clear current path
                        ppMovePath = StrategicPathing.ClearStrategicPathList(ref ppMovePath, ubGroupId);
                        // replace it with the previous one
                        ppMovePath = StrategicPathing.CopyPaths(gpCharacterPreviousMercPath[iCounter], ppMovePath);
                        // will need to rebuild waypoints
                        fPathChanged = true;
                    }
                    else    // no previous path stored
                    {
                        // if he has one now, wipe it out
                        if (ppMovePath is not null)
                        {
                            // wipe it out!
                            ppMovePath = StrategicPathing.MoveToBeginningOfPathList(ref ppMovePath);
                            ppMovePath = StrategicPathing.ClearStrategicPathList(ref ppMovePath, ubGroupId);
                            // will need to rebuild waypoints
                            fPathChanged = true;
                        }
                    }


                    if (fPathChanged)
                    {
                        // rebuild waypoints
                        StrategicPathing.RebuildWayPointsForGroupPath(ref ppMovePath, ubGroupId);
                    }
                }
            }
        }
    }

    private static void CopyPathToAllSelectedCharacters(ref Path? pPath)
    {
        SOLDIERTYPE? pSoldier = null;


        // run through list and copy paths for each selected character
        for (int iCounter = 0; iCounter < MAX_CHARACTER_COUNT; iCounter++)
        {
            if (fSelectedListOfMercsForMapScreen[iCounter] == true)
            {
                pSoldier = MercPtrs[gCharactersList[iCounter].usSolID];

                // skip itself!
                if (StrategicPathing.GetSoldierMercPathPtr(pSoldier) != pPath)
                {
                    if (pSoldier.uiStatusFlags.HasFlag(SOLDIER.VEHICLE))
                    {
                        pVehicleList[pSoldier.bVehicleID].pMercPath = StrategicPathing.CopyPaths(pPath, pVehicleList[pSoldier.bVehicleID].pMercPath);
                    }
                    else if (pSoldier.bAssignment == Assignment.VEHICLE)
                    {
                        pVehicleList[pSoldier.iVehicleId].pMercPath = StrategicPathing.CopyPaths(pPath, pVehicleList[pSoldier.iVehicleId].pMercPath);
                    }
                    else
                    {
                        pSoldier.pMercPath = StrategicPathing.CopyPaths(pPath, pSoldier.pMercPath);
                    }

                    // don't use CopyPathToCharactersSquadIfInOne(), it will whack the original pPath by replacing that merc's path!
                }
            }
        }
    }
}

public enum MAP_BORDER
{
    TOWN_BTN = 0,
    MINE_BTN,
    TEAMS_BTN,
    AIRSPACE_BTN,
    ITEM_BTN,
    MILITIA_BTN,
}


public enum TOWNS
{
    BLANK_SECTOR = 0,
    OMERTA,
    DRASSEN,
    ALMA,
    GRUMM,
    TIXA,
    CAMBRIA,
    SAN_MONA,
    ESTONI,
    ORTA,
    BALIME,
    MEDUNA,
    CHITZENA,
    NUM_TOWNS
}
