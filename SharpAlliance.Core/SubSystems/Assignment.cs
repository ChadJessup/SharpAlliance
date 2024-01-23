using System;
using System.Diagnostics;
using SharpAlliance.Core.Screens;
using SharpAlliance.Core.SubSystems.LaptopSubSystem;
using SixLabors.ImageSharp;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static SharpAlliance.Core.Globals;
using static SharpAlliance.Core.InputManager;

namespace SharpAlliance.Core.SubSystems;

public class Assignments
{
    private static bool fShowVehicleMenu;
    private static bool fShowRepairMenu;
    // PopUp Box Handles
    private static int ghAssignmentBox = -1;
    private static int ghEpcBox = -1;
    private static int ghSquadBox = -1;
    private static int ghVehicleBox = -1;
    private static int ghRepairBox = -1;
    private static int ghTrainingBox = -1;
    private static int ghAttributeBox = -1;
    private static int ghRemoveMercAssignBox = -1;
    private static int ghContractBox = -1;
    private static int ghMoveBox = -1;


    // mouse region for vehicle menu
    private static MOUSE_REGION[] gVehicleMenuRegion = new MOUSE_REGION[20];
    // assignment menu mouse regions

    private static MOUSE_REGION[] gAssignmentMenuRegion = new MOUSE_REGION[(int)ASSIGN_MENU.MAX_ASSIGN_STRING_COUNT];
    private static MOUSE_REGION[] gTrainingMenuRegion = new MOUSE_REGION[(int)TRAIN_MENU.MAX_TRAIN_STRING_COUNT];
    private static MOUSE_REGION[] gAttributeMenuRegion = new MOUSE_REGION[(int)ATTRIBUTE_MENU.MAX_ATTRIBUTE_STRING_COUNT];
    private static MOUSE_REGION[] gSquadMenuRegion = new MOUSE_REGION[(int)SQUAD_MENU.MAX_SQUAD_MENU_STRING_COUNT];
    private static MOUSE_REGION[] gContractMenuRegion = new MOUSE_REGION[(int)CONTRACT_MENU.MAX_CONTRACT_MENU_STRING_COUNT];
    private static MOUSE_REGION[] gRemoveMercAssignRegion = new MOUSE_REGION[(int)REMOVE.MAX_REMOVE_MERC_COUNT];
    private static MOUSE_REGION[] gEpcMenuRegion = new MOUSE_REGION[(int)EPC_MENU.MAX_EPC_MENU_STRING_COUNT];
    private static MOUSE_REGION[] gRepairMenuRegion = new MOUSE_REGION[20];


    public static void DetermineWhichAssignmentMenusCanBeShown()
    {
        bool fCharacterNoLongerValid = false;
        SOLDIERTYPE? pSoldier = null;

        if ((guiTacticalInterfaceFlags.HasFlag(INTERFACE.MAPSCREEN)))
        {
            if (MapScreenInterface.fShowMapScreenMovementList == true)
            {
                if (MapScreenInterfaceMap.bSelectedDestChar == -1)
                {
                    fCharacterNoLongerValid = true;
                    Assignments.HandleShowingOfMovementBox();
                }
                else
                {
                    MapScreenInterface.fShowMapScreenMovementList = false;
                    fCharacterNoLongerValid = true;
                }
            }
            /*
                    else if( fShowUpdateBox )
                    {
                        //handle showing of the merc update box
                        HandleShowingOfUpBox( );
                    }
            */
            else if (MapScreenInterfaceMap.bSelectedAssignChar == -1)
            {
                fCharacterNoLongerValid = true;
            }

            // update the assignment positions
            Assignments.UpdateMapScreenAssignmentPositions();
        }

        // determine which assign menu needs to be shown
        if (((MapScreenInterface.fShowAssignmentMenu == false)) || (fCharacterNoLongerValid == true))
        {
            // reset show assignment menus
            MapScreenInterface.fShowAssignmentMenu = false;
            fShowVehicleMenu = false;
            Assignments.fShowRepairMenu = false;

            // destroy mask, if needed
            Assignments.CreateDestroyScreenMaskForAssignmentAndContractMenus();


            // destroy menu if needed
            CreateDestroyMouseRegionForVehicleMenu();
            CreateDestroyMouseRegionsForAssignmentMenu();
            CreateDestroyMouseRegionsForTrainingMenu();
            CreateDestroyMouseRegionsForAttributeMenu();
            CreateDestroyMouseRegionsForSquadMenu(true);
            CreateDestroyMouseRegionForRepairMenu();

            // hide all boxes being shown
            if (PopUpBox.IsBoxShown(ghEpcBox))
            {
                PopUpBox.HideBox(ghEpcBox);
                fTeamPanelDirty = true;
                gfRenderPBInterface = true;
            }
            if (PopUpBox.IsBoxShown(ghAssignmentBox))
            {
                PopUpBox.HideBox(Assignments.ghAssignmentBox);
                fTeamPanelDirty = true;
                gfRenderPBInterface = true;
            }
            if (PopUpBox.IsBoxShown(ghTrainingBox))
            {
                PopUpBox.HideBox(ghTrainingBox);
                fTeamPanelDirty = true;
                gfRenderPBInterface = true;
            }
            if (PopUpBox.IsBoxShown(ghRepairBox))
            {
                PopUpBox.HideBox(ghRepairBox);
                fTeamPanelDirty = true;
                gfRenderPBInterface = true;
            }
            if (PopUpBox.IsBoxShown(ghAttributeBox))
            {
                PopUpBox.HideBox(ghAttributeBox);
                fTeamPanelDirty = true;
                gfRenderPBInterface = true;
            }
            if (PopUpBox.IsBoxShown(ghVehicleBox))
            {
                PopUpBox.HideBox(ghVehicleBox);
                fTeamPanelDirty = true;
                gfRenderPBInterface = true;
            }

            // do we really want ot hide this box?
            if (MapScreenInterface.fShowContractMenu == false)
            {
                if (PopUpBox.IsBoxShown(ghRemoveMercAssignBox))
                {
                    PopUpBox.HideBox(ghRemoveMercAssignBox);
                    fTeamPanelDirty = true;
                    gfRenderPBInterface = true;
                }
            }
            //HideBox( ghSquadBox );


            //SetRenderFlags(RENDER_FLAG_FULL);

            // no menus, leave
            return;
        }

        // update the assignment positions
        UpdateMapScreenAssignmentPositions();

        // create mask, if needed
        CreateDestroyScreenMaskForAssignmentAndContractMenus();


        // created assignment menu if needed
        CreateDestroyMouseRegionsForAssignmentMenu();
        CreateDestroyMouseRegionsForTrainingMenu();
        CreateDestroyMouseRegionsForAttributeMenu();
        CreateDestroyMouseRegionsForSquadMenu(true);
        CreateDestroyMouseRegionForRepairMenu();


        if (((Menptr[gCharactersList[bSelectedInfoChar].usSolID].bLife == 0)
            || (Menptr[gCharactersList[bSelectedInfoChar].usSolID].bAssignment == Assignment.ASSIGNMENT_POW))
            && ((guiTacticalInterfaceFlags.HasFlag(INTERFACE.MAPSCREEN))))
        {
            // show basic assignment menu
            PopUpBox.ShowBox(ghRemoveMercAssignBox);
        }
        else
        {
            pSoldier = Assignments.GetSelectedAssignSoldier(false);

            if (pSoldier.ubWhatKindOfMercAmI == MERC_TYPE.EPC)
            {
                PopUpBox.ShowBox(ghEpcBox);
            }
            else
            {
                // show basic assignment menu

                PopUpBox.ShowBox(ghAssignmentBox);
            }
        }

        // TRAINING menu
        if (MapScreenInterface.fShowTrainingMenu == true)
        {
            HandleShadingOfLinesForTrainingMenu();
            PopUpBox.ShowBox(ghTrainingBox);
        }
        else
        {
            if (PopUpBox.IsBoxShown(ghTrainingBox))
            {
                PopUpBox.HideBox(ghTrainingBox);
                fTeamPanelDirty = true;
                fMapPanelDirty = true;
                gfRenderPBInterface = true;
                //	SetRenderFlags(RENDER_FLAG_FULL);
            }
        }

        // REPAIR menu
        if (fShowRepairMenu == true)
        {
            HandleShadingOfLinesForRepairMenu();
            PopUpBox.ShowBox(ghRepairBox);
        }
        else
        {
            // hide box
            if (PopUpBox.IsBoxShown(ghRepairBox))
            {
                PopUpBox.HideBox(ghRepairBox);
                fTeamPanelDirty = true;
                fMapPanelDirty = true;
                gfRenderPBInterface = true;
                //	SetRenderFlags(RENDER_FLAG_FULL);
            }
        }

        // ATTRIBUTE menu
        if (fShowAttributeMenu == true)
        {
            HandleShadingOfLinesForAttributeMenus();
            PopUpBox.ShowBox(ghAttributeBox);
        }
        else
        {
            if (PopUpBox.IsBoxShown(ghAttributeBox))
            {
                PopUpBox.HideBox(ghAttributeBox);
                fTeamPanelDirty = true;
                fMapPanelDirty = true;
                gfRenderPBInterface = true;
                //	SetRenderFlags(RENDER_FLAG_FULL);
            }

        }

        // VEHICLE menu
        if (fShowVehicleMenu == true)
        {
            PopUpBox.ShowBox(ghVehicleBox);
        }
        else
        {
            if (PopUpBox.IsBoxShown(ghVehicleBox))
            {
                PopUpBox.HideBox(ghVehicleBox);
                fTeamPanelDirty = true;
                fMapPanelDirty = true;
                gfRenderPBInterface = true;
                //	SetRenderFlags(RENDER_FLAG_FULL);
            }
        }

        CreateDestroyMouseRegionForVehicleMenu();

        return;
    }

    private static void HandleShadingOfLinesForTrainingMenu()
    {
        throw new NotImplementedException();
    }

    private static void HandleShadingOfLinesForRepairMenu()
    {
        throw new NotImplementedException();
    }

    private static void HandleShadingOfLinesForAttributeMenus()
    {
        throw new NotImplementedException();
    }

    private static void HandleShowingOfMovementBox()
    {
        throw new NotImplementedException();
    }

    private static bool fCreatedCreateDestroyMouseRegionForVehicleMenu = false;
    private static void CreateDestroyMouseRegionForVehicleMenu()
    {

        int  uiMenuLine = 0;
        int iVehicleId = 0;
        int iFontHeight = 0;
        int iBoxXPosition = 0;
        int iBoxYPosition = 0;
        Point pPosition, pPoint;
        int iBoxWidth = 0;
        Rectangle pDimensions;
        SOLDIERTYPE? pSoldier = null;


        if (fShowVehicleMenu)
        {
            PopUpBox.GetBoxPosition(ghAssignmentBox, out pPoint);

            // get dimensions..mostly for width
            PopUpBox.GetBoxSize(ghAssignmentBox, out pDimensions);

            // vehicle position
            MapScreenInterface.VehiclePosition.X = pPoint.X + pDimensions.Width;

            PopUpBox.SetBoxPosition(ghVehicleBox, MapScreenInterface.VehiclePosition);
        }


        if ((fShowVehicleMenu) && (!fCreatedCreateDestroyMouseRegionForVehicleMenu))
        {
            // grab height of font
            iFontHeight = PopUpBox.GetLineSpace(ghVehicleBox) + FontSubSystem.GetFontHeight(PopUpBox.GetBoxFont(ghVehicleBox));

            // get x.y position of box
            PopUpBox.GetBoxPosition(ghVehicleBox, out pPosition);

            // grab box x and y position
            iBoxXPosition = pPosition.X;
            iBoxYPosition = pPosition.Y;

            // get dimensions..mostly for width
            PopUpBox.GetBoxSize(ghVehicleBox, out pDimensions);
            PopUpBox.SetBoxSecondaryShade(ghVehicleBox, FontColor.FONT_YELLOW);

            // get width
            iBoxWidth = pDimensions.Width;

            PopUpBox.SetCurrentBox(ghVehicleBox);

            pSoldier = GetSelectedAssignSoldier(false);

            // run through list of vehicles in sector
            for (iVehicleId = 0; iVehicleId < ubNumberOfVehicles; iVehicleId++)
            {
                gVehicleMenuRegion[uiMenuLine] = new($"{gVehicleMenuRegion}-{uiMenuLine}");

                if (pVehicleList[iVehicleId].fValid)
                {
                    if (Vehicles.IsThisVehicleAccessibleToSoldier(pSoldier, iVehicleId))
                    {
                        // add mouse region for each accessible vehicle
                        MouseSubSystem.MSYS_DefineRegion(
                            gVehicleMenuRegion[uiMenuLine],
                            new Rectangle((iBoxXPosition), (iBoxYPosition + PopUpBox.GetTopMarginSize(ghAssignmentBox) + (iFontHeight) * uiMenuLine), (iBoxXPosition + iBoxWidth), (iBoxYPosition + PopUpBox.GetTopMarginSize(ghAssignmentBox) + (iFontHeight) * (uiMenuLine + 1))),
                            MSYS_PRIORITY.HIGHEST - 4,
                            MSYS_NO_CURSOR,
                            VehicleMenuMvtCallback,
                            VehicleMenuBtnCallback);

                        MouseSubSystem.MSYS_SetRegionUserData(gVehicleMenuRegion[uiMenuLine], 0, uiMenuLine);
                        // store vehicle ID in the SECOND user data
                        MouseSubSystem.MSYS_SetRegionUserData(gVehicleMenuRegion[uiMenuLine], 1, iVehicleId);

                        uiMenuLine++;
                    }
                }
            }


            // cancel line
            MouseSubSystem.MSYS_DefineRegion(
                gVehicleMenuRegion[uiMenuLine],
                new Rectangle((iBoxXPosition), (iBoxYPosition + PopUpBox.GetTopMarginSize(ghAssignmentBox) + (iFontHeight) * uiMenuLine), (iBoxXPosition + iBoxWidth), (iBoxYPosition + PopUpBox.GetTopMarginSize(ghAssignmentBox) + (iFontHeight) * (uiMenuLine + 1))),
                MSYS_PRIORITY.HIGHEST - 4,
                MSYS_NO_CURSOR,
                VehicleMenuMvtCallback,
                VehicleMenuBtnCallback);

            MouseSubSystem.MSYS_SetRegionUserData(gVehicleMenuRegion[uiMenuLine], 0, VEHICLE_MENU.CANCEL);

            // created
            fCreatedCreateDestroyMouseRegionForVehicleMenu = true;

            // pause game 
            GameClock.PauseGame();

            // unhighlight all strings in box
            PopUpBox.UnHighLightBox(ghVehicleBox);

            fCreatedCreateDestroyMouseRegionForVehicleMenu = true;

            HandleShadingOfLinesForVehicleMenu();
        }
        else if (((fShowVehicleMenu == false) || (MapScreenInterface.fShowAssignmentMenu == false))
            && (fCreatedCreateDestroyMouseRegionForVehicleMenu))
        {
            fCreatedCreateDestroyMouseRegionForVehicleMenu = false;

            // remove these regions
            for (uiMenuLine = 0; uiMenuLine < PopUpBox.GetNumberOfLinesOfTextInBox(ghVehicleBox); uiMenuLine++)
            {
                MouseSubSystem.MSYS_RemoveRegion(gVehicleMenuRegion[uiMenuLine]);
            }

            fShowVehicleMenu = false;

            RenderWorld.SetRenderFlags(RenderingFlags.FULL);

            PopUpBox.HideBox(ghVehicleBox);

            if (MapScreenInterface.fShowAssignmentMenu)
            {
                // remove highlight on the parent menu
                PopUpBox.UnHighLightBox(ghAssignmentBox);
            }
        }
    }

    private static void VehicleMenuBtnCallback(ref MOUSE_REGION pRegion, MSYS_CALLBACK_REASON iReason)
    {
        // btn callback handler for assignment region
        int iVehicleID;
        SOLDIERTYPE? pSoldier;


        VEHICLE_MENU iValue = (VEHICLE_MENU)MouseSubSystem.MSYS_GetRegionUserData(ref pRegion, 0);

        if (iReason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP))
        {
            if (iValue == VEHICLE_MENU.CANCEL)
            {
                fShowVehicleMenu = false;
                PopUpBox.UnHighLightBox(ghAssignmentBox);
                fTeamPanelDirty = true;
                fMapScreenBottomDirty = true;
                fCharacterInfoPanelDirty = true;
                return;
            }

            pSoldier = GetSelectedAssignSoldier(false);
            iVehicleID = (int)MouseSubSystem.MSYS_GetRegionUserData(ref pRegion, 1);

            // inaccessible vehicles shouldn't be listed in the menu!
            Debug.Assert(Vehicles.IsThisVehicleAccessibleToSoldier(pSoldier, iVehicleID));

            if (Vehicles.IsEnoughSpaceInVehicle(iVehicleID))
            {
                Vehicles.PutSoldierInVehicle(pSoldier, iVehicleID);
            }
            else
            {
               // ScreenMsg(FONT_MCOLOR_LTYELLOW, MSG_UI_FEEDBACK, gzLateLocalizedString[18], zVehicleName[pVehicleList[iVehicleID].ubVehicleType]);
            }

            MapScreenInterface.fShowAssignmentMenu = false;

            // update mapscreen
            fTeamPanelDirty = true;
            fCharacterInfoPanelDirty = true;
            fMapScreenBottomDirty = true;

            MapScreenInterface.giAssignHighLine = -1;

            SetAssignmentForList(Assignment.VEHICLE, iVehicleID);
        }
    }

    private static void SetAssignmentForList(Assignment vEHICLE, int iVehicleID)
    {
        throw new NotImplementedException();
    }

    private static void VehicleMenuMvtCallback(ref MOUSE_REGION pRegion, MSYS_CALLBACK_REASON iReason)
    {
        // mvt callback handler for assignment region
        VEHICLE_MENU iValue = (VEHICLE_MENU)MouseSubSystem.MSYS_GetRegionUserData(ref pRegion, 0);

        if (iReason.HasFlag(MSYS_CALLBACK_REASON.GAIN_MOUSE))
        {
            if (iValue != VEHICLE_MENU.CANCEL)
            {
                // no shaded(disabled) lines actually appear in vehicle menus
                if (PopUpBox.GetBoxShadeFlag(ghVehicleBox, (int)iValue) == false)
                {
                    // highlight vehicle line
                    PopUpBox.HighLightBoxLine(ghVehicleBox, (int)iValue);
                }
            }
            else
            {
                // highlight cancel line
                PopUpBox.HighLightBoxLine(ghVehicleBox, PopUpBox.GetNumberOfLinesOfTextInBox(ghVehicleBox) - 1);
            }
        }
        else if (iReason.HasFlag(MSYS_CALLBACK_REASON.LOST_MOUSE))
        {
            // unhighlight all strings in box
            PopUpBox.UnHighLightBox(ghVehicleBox);

            HandleShadingOfLinesForVehicleMenu();
        }
    }

    private static void HandleShadingOfLinesForVehicleMenu()
    {
        SOLDIERTYPE? pSoldier = null;
        int iVehicleId;
        int uiMenuLine = 0;


        if ((fShowVehicleMenu == false) || (ghVehicleBox == -1))
        {
            return;
        }

        pSoldier = GetSelectedAssignSoldier(false);

        // run through list of vehicles
        for (iVehicleId = 0; iVehicleId < ubNumberOfVehicles; iVehicleId++)
        {
            if (pVehicleList[iVehicleId].fValid == true)
            {
                // inaccessible vehicles aren't listed at all!
                if (Vehicles.IsThisVehicleAccessibleToSoldier(pSoldier, iVehicleId))
                {
                    if (Vehicles.IsEnoughSpaceInVehicle(iVehicleId))
                    {
                        // legal vehicle, leave it green
                        PopUpBox.UnShadeStringInBox(ghVehicleBox, uiMenuLine);
                        PopUpBox.UnSecondaryShadeStringInBox(ghVehicleBox, uiMenuLine);
                    }
                    else
                    {
                        // unjoinable vehicle - yellow
                        PopUpBox.SecondaryShadeStringInBox(ghVehicleBox, uiMenuLine);
                    }

                    uiMenuLine++;
                }
            }
        }
    }

    private static bool fShowRemoveMenu = false;
    private static bool fCreatedCreateDestroyMouseRegionsForAssignmentMenu = false;
    private static void CreateDestroyMouseRegionsForAssignmentMenu()
    {
        int iCounter = 0;
        int iFontHeight = 0;
        int iBoxXPosition = 0;
        int iBoxYPosition = 0;
        SOLDIERTYPE? pSoldier = null;
        Point pPosition;
        int iBoxWidth = 0;
        Rectangle pDimensions;


        // will create/destroy mouse regions for the map screen assignment main menu
        // check if we can only remove character from team..not assign
        if ((MapScreenInterfaceMap.bSelectedAssignChar != -1) || (fShowRemoveMenu == true))
        {
            if (fShowRemoveMenu == true)
            {
                // dead guy handle menu stuff
                fShowRemoveMenu = MapScreenInterface.fShowAssignmentMenu | MapScreenInterface.fShowContractMenu;

                CreateDestroyMouseRegionsForRemoveMenu();

                return;
            }
            if ((Menptr[gCharactersList[MapScreenInterfaceMap.bSelectedAssignChar].usSolID].bLife == 0) || (Menptr[gCharactersList[MapScreenInterfaceMap.bSelectedAssignChar].usSolID].bAssignment == Assignment.ASSIGNMENT_POW))
            {
                // dead guy handle menu stuff
                fShowRemoveMenu = MapScreenInterface.fShowAssignmentMenu | MapScreenInterface.fShowContractMenu;

                CreateDestroyMouseRegionsForRemoveMenu();

                return;
            }
        }


        if ((MapScreenInterface.fShowAssignmentMenu == true) && (fCreatedCreateDestroyScreenMaskForAssignmentAndContractMenus == false))
        {

            gfIgnoreScrolling = false;

            if ((MapScreenInterface.fShowAssignmentMenu) && (guiCurrentScreen == ScreenName.MAP_SCREEN))
            {
                PopUpBox.SetBoxPosition(ghAssignmentBox, MapScreenInterface.AssignmentPosition);
            }

            pSoldier = GetSelectedAssignSoldier(false);

            if (pSoldier.ubWhatKindOfMercAmI == MERC_TYPE.EPC)
            {
                // grab height of font
                iFontHeight = PopUpBox.GetLineSpace(ghEpcBox) + FontSubSystem.GetFontHeight(PopUpBox.GetBoxFont(ghEpcBox));

                // get x.y position of box
                PopUpBox.GetBoxPosition(ghEpcBox, out pPosition);

                // grab box x and y position
                iBoxXPosition = pPosition.X;
                iBoxYPosition = pPosition.Y;

                // get dimensions..mostly for width
                PopUpBox.GetBoxSize(ghEpcBox, out pDimensions);

                // get width
                iBoxWidth = pDimensions.Width;

                PopUpBox.SetCurrentBox(ghEpcBox);
            }
            else
            {
                // grab height of font
                iFontHeight = PopUpBox.GetLineSpace(ghAssignmentBox) + FontSubSystem.GetFontHeight(PopUpBox.GetBoxFont(ghAssignmentBox));

                // get x.y position of box
                PopUpBox.GetBoxPosition(ghAssignmentBox, out pPosition);

                // grab box x and y position
                iBoxXPosition = pPosition.X;
                iBoxYPosition = pPosition.Y;

                // get dimensions..mostly for width
                PopUpBox.GetBoxSize(ghAssignmentBox, out pDimensions);

                // get width
                iBoxWidth = pDimensions.Width;

                PopUpBox.SetCurrentBox(ghAssignmentBox);
            }


            // define regions
            for (iCounter = 0; iCounter < PopUpBox.GetNumberOfLinesOfTextInBox(ghAssignmentBox); iCounter++)
            {
                // add mouse region for each line of text..and set user data
                MouseSubSystem.MSYS_DefineRegion(
                    gAssignmentMenuRegion[iCounter],
                    new Rectangle((iBoxXPosition), (iBoxYPosition + PopUpBox.GetTopMarginSize(ghAssignmentBox) + (iFontHeight) * iCounter), (iBoxXPosition + iBoxWidth), (iBoxYPosition + PopUpBox.GetTopMarginSize(ghAssignmentBox) + (iFontHeight) * (iCounter + 1))),
                    MSYS_PRIORITY.HIGHEST - 4,
                    MSYS_NO_CURSOR,
                    AssignmentMenuMvtCallBack,
                    AssignmentMenuBtnCallback);

                MouseSubSystem.MSYS_SetRegionUserData(gAssignmentMenuRegion[iCounter], 0, iCounter);
            }

            // created
            fCreatedCreateDestroyScreenMaskForAssignmentAndContractMenus = true;

            // unhighlight all strings in box
            PopUpBox.UnHighLightBox(ghAssignmentBox);
            CheckAndUpdateTacticalAssignmentPopUpPositions();

            PositionCursorForTacticalAssignmentBox();
        }
        else if ((MapScreenInterface.fShowAssignmentMenu == false) && (fCreatedCreateDestroyScreenMaskForAssignmentAndContractMenus == true))
        {
            // destroy 
            for (iCounter = 0; iCounter < PopUpBox.GetNumberOfLinesOfTextInBox(ghAssignmentBox); iCounter++)
            {
                MouseSubSystem.MSYS_RemoveRegion(gAssignmentMenuRegion[iCounter]);
            }

            fShownAssignmentMenu = false;

            // not created
            fCreatedCreateDestroyScreenMaskForAssignmentAndContractMenus = false;
            RenderWorld.SetRenderFlags(RenderingFlags.FULL);

        }
    }

    private static bool fCreatedCreateDestroyMouseRegionsForRemoveMenu = false;

    private static void CreateDestroyMouseRegionsForRemoveMenu()
    {
        int iCounter = 0;
        int iFontHeight = 0;
        int iBoxXPosition = 0;
        int iBoxYPosition = 0;
        Point pPosition;
        int iBoxWidth = 0;
        Rectangle pDimensions;

        // will create/destroy mouse regions for the map screen attribute  menu
        if (((MapScreenInterface.fShowAssignmentMenu == true) || (MapScreenInterface.fShowContractMenu == true))
            && (fCreatedCreateDestroyMouseRegionsForRemoveMenu == false))
        {

            if (MapScreenInterface.fShowContractMenu)
            {
                PopUpBox.SetBoxPosition(ghContractBox, MapScreenInterface.ContractPosition);
            }
            else
            {
                PopUpBox.SetBoxPosition(ghAssignmentBox, MapScreenInterface.AssignmentPosition);
            }

            if (MapScreenInterface.fShowContractMenu)
            {
                // set box position to contract box position
                PopUpBox.SetBoxPosition(ghRemoveMercAssignBox, MapScreenInterface.ContractPosition);
            }
            else
            {
                // set box position to contract box position
                PopUpBox.SetBoxPosition(ghRemoveMercAssignBox, MapScreenInterface.AssignmentPosition);
            }

            CheckAndUpdateTacticalAssignmentPopUpPositions();

            // grab height of font
            iFontHeight = PopUpBox.GetLineSpace(ghRemoveMercAssignBox) + FontSubSystem.GetFontHeight(PopUpBox.GetBoxFont(ghRemoveMercAssignBox));

            // get x.y position of box
            PopUpBox.GetBoxPosition(ghRemoveMercAssignBox, out pPosition);

            // grab box x and y position
            iBoxXPosition = pPosition.X;
            iBoxYPosition = pPosition.Y;

            // get dimensions..mostly for width
            PopUpBox.GetBoxSize(ghRemoveMercAssignBox, out pDimensions);

            // get width
            iBoxWidth = pDimensions.Width;

            PopUpBox.SetCurrentBox(ghRemoveMercAssignBox);

            // define regions
            for (iCounter = 0; iCounter < PopUpBox.GetNumberOfLinesOfTextInBox(ghRemoveMercAssignBox); iCounter++)
            {
                // add mouse region for each line of text..and set user data
                MouseSubSystem.MSYS_DefineRegion(
                    gRemoveMercAssignRegion[iCounter], 
                    new Rectangle((iBoxXPosition), (iBoxYPosition + PopUpBox.GetTopMarginSize(ghAttributeBox) + (iFontHeight) * iCounter), (iBoxXPosition + iBoxWidth), (iBoxYPosition + PopUpBox.GetTopMarginSize(ghAttributeBox) + (iFontHeight) * (iCounter + 1))),
                    MSYS_PRIORITY.HIGHEST - 2,
                    MSYS_NO_CURSOR,
                    RemoveMercMenuMvtCallBack,
                    RemoveMercMenuBtnCallback);

                // set user defines
                MouseSubSystem.MSYS_SetRegionUserData(gRemoveMercAssignRegion[iCounter], 0, iCounter);
            }

            // created
            fCreatedCreateDestroyMouseRegionsForRemoveMenu = true;

            // unhighlight all strings in box
            PopUpBox.UnHighLightBox(ghRemoveMercAssignBox);

        }
        else if ((MapScreenInterface.fShowAssignmentMenu == false) && (fCreatedCreateDestroyMouseRegionsForRemoveMenu == true) && (MapScreenInterface.fShowContractMenu == false))
        {
            // destroy 
            for (iCounter = 0; iCounter < PopUpBox.GetNumberOfLinesOfTextInBox(ghRemoveMercAssignBox); iCounter++)
            {
                MouseSubSystem.MSYS_RemoveRegion(gRemoveMercAssignRegion[iCounter]);
            }

            fShownContractMenu = false;

            // stop showing  menu
            if (fShowRemoveMenu == false)
            {
                fShowAttributeMenu = false;
                fMapPanelDirty = true;
                gfRenderPBInterface = true;

            }


            PopUpBox.RestorePopUpBoxes();

            fMapPanelDirty = true;
            fCharacterInfoPanelDirty = true;
            fTeamPanelDirty = true;
            fMapScreenBottomDirty = true;

            // turn off the GLOBAL fShowRemoveMenu flag!!!
            fShowRemoveMenu = false;
            // and the assignment menu itself!!!
            MapScreenInterface.fShowAssignmentMenu = false;

            // not created 
            fCreatedCreateDestroyMouseRegionsForRemoveMenu = false;
        }
    }

    private static void RemoveMercMenuMvtCallBack(ref MOUSE_REGION pRegion, MSYS_CALLBACK_REASON iReason)
    {
        throw new NotImplementedException();
    }

    private static void RemoveMercMenuBtnCallback(ref MOUSE_REGION pRegion, MSYS_CALLBACK_REASON iReason)
    {
        throw new NotImplementedException();
    }

    private static void CheckAndUpdateTacticalAssignmentPopUpPositions()
    {
        throw new NotImplementedException();
    }

    private static void PositionCursorForTacticalAssignmentBox()
    {
        throw new NotImplementedException();
    }

    private static void AssignmentMenuBtnCallback(ref MOUSE_REGION pRegion, MSYS_CALLBACK_REASON iReason)
    {
        throw new NotImplementedException();
    }

    private static void AssignmentMenuMvtCallBack(ref MOUSE_REGION pRegion, MSYS_CALLBACK_REASON iReason)
    {
        throw new NotImplementedException();
    }

    private static void CreateDestroyMouseRegionsForTrainingMenu()
    {
        
    }

    private static void CreateDestroyMouseRegionsForAttributeMenu()
    {
        
    }

    private static void CreateDestroyMouseRegionsForSquadMenu(bool v)
    {
    }

    private static void CreateDestroyMouseRegionForRepairMenu()
    {
    }

    private static SOLDIERTYPE? GetSelectedAssignSoldier(bool v)
    {
        throw new NotImplementedException();
    }

    internal static void CreateDestroyAssignmentPopUpBoxes()
    {
    }

    private static MOUSE_REGION? gAssignmentScreenMaskRegion;
    private static bool fFirstClickInAssignmentScreenMask;
    private static bool fGlowContractRegion;
    private static bool fShownAssignmentMenu;
    private static bool fCreatedCreateDestroyScreenMaskForAssignmentAndContractMenus = false;
    private static bool fShownContractMenu;

    private static void CreateDestroyScreenMaskForAssignmentAndContractMenus()
    {
        // will create a screen mask to catch mouse input to disable assignment menus

        // not created, create
        if ((fCreatedCreateDestroyScreenMaskForAssignmentAndContractMenus == false)
            && ((MapScreenInterface.fShowAssignmentMenu) || (MapScreenInterface.fShowContractMenu) || (MapScreenInterfaceTownMineInfo.fShowTownInfo)))
        {
            MouseSubSystem.MSYS_DefineRegion(
                gAssignmentScreenMaskRegion,
                0, 0, 640, 480,
                MSYS_PRIORITY.HIGHEST - 4,
                MSYS_NO_CURSOR,
                MSYS_NO_CALLBACK,
                AssignmentScreenMaskBtnCallback);

            // created
            fCreatedCreateDestroyScreenMaskForAssignmentAndContractMenus = true;

            if (!(guiTacticalInterfaceFlags.HasFlag(INTERFACE.MAPSCREEN)))
            {
                MouseSubSystem.MSYS_ChangeRegionCursor(gAssignmentScreenMaskRegion, 0);
            }

        }
        else if ((fCreatedCreateDestroyScreenMaskForAssignmentAndContractMenus) && (MapScreenInterface.fShowAssignmentMenu) && (MapScreenInterface.fShowContractMenu == false) && (MapScreenInterfaceTownMineInfo.fShowTownInfo == false))
        {
            // created, get rid of it
            MouseSubSystem.MSYS_RemoveRegion(gAssignmentScreenMaskRegion);

            // not created
            fCreatedCreateDestroyScreenMaskForAssignmentAndContractMenus = false;
        }
    }

    private static void AssignmentScreenMaskBtnCallback(ref MOUSE_REGION pRegion, MSYS_CALLBACK_REASON iReason)
    {
        // btn callback handler for assignment screen mask region

        if ((iReason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP)) || (iReason.HasFlag(MSYS_CALLBACK_REASON.RBUTTON_UP)))
        {
            if (fFirstClickInAssignmentScreenMask == true)
            {
                fFirstClickInAssignmentScreenMask = false;
                return;
            }

            // button event, stop showing menus
            MapScreenInterface.fShowAssignmentMenu = false;

            fShowVehicleMenu = false;

            MapScreenInterface.fShowContractMenu = false;

            // stop showing town mine info
            MapScreenInterfaceTownMineInfo.fShowTownInfo = false;

            // reset contract character and contract highlight line
            MapScreenInterface.giContractHighLine = -1;
            MapScreenInterfaceMap.bSelectedContractChar = -1;
            Assignments.fGlowContractRegion = false;


            // update mapscreen
            fTeamPanelDirty = true;
            fCharacterInfoPanelDirty = true;
            fMapScreenBottomDirty = true;

            gfRenderPBInterface = true;
            RenderWorld.SetRenderFlags(RenderingFlags.FULL);
        }
    }

    private static void UpdateMapScreenAssignmentPositions()
    {
        throw new NotImplementedException();
    }
}

public enum Assignment
{
    SQUAD_1 = 0,
    SQUAD_2,
    SQUAD_3,
    SQUAD_4,
    SQUAD_5,
    SQUAD_6,
    SQUAD_7,
    SQUAD_8,
    SQUAD_9,
    SQUAD_10,
    SQUAD_11,
    SQUAD_12,
    SQUAD_13,
    SQUAD_14,
    SQUAD_15,
    SQUAD_16,
    SQUAD_17,
    SQUAD_18,
    SQUAD_19,
    SQUAD_20,
    ON_DUTY,
    DOCTOR,
    PATIENT,
    VEHICLE,
    IN_TRANSIT,
    REPAIR,
    TRAIN_SELF,
    TRAIN_TOWN,
    TRAIN_TEAMMATE,
    TRAIN_BY_OTHER,
    ASSIGNMENT_DEAD,
    ASSIGNMENT_UNCONCIOUS,          // unused
    ASSIGNMENT_POW,
    ASSIGNMENT_HOSPITAL,
    ASSIGNMENT_EMPTY,
    NO_ASSIGNMENT = 127, //used when no pSoldier.ubDesiredSquad

    SLEEPING = 172, // chad: added because original code intermixed Assignments and AnimationStates.
};


public enum VEHICLE_MENU
{
    VEHICLE1 = 0,
    VEHICLE2,
    VEHICLE3,
    CANCEL,
};

// the epc assignment menu
public enum EPC_MENU
{
    ON_DUTY = 0,
    PATIENT,
    VEHICLE,
    REMOVE,
    CANCEL,
    MAX_EPC_MENU_STRING_COUNT,
};

// assignment menu defines
public enum ASSIGN_MENU
{
    ON_DUTY = 0,
    DOCTOR,
    PATIENT,
    VEHICLE,
    REPAIR,
    TRAIN,
    CANCEL,
    MAX_ASSIGN_STRING_COUNT,
};


// training assignment menu defines
public enum TRAIN_MENU
{
    SELF,
    TOWN,
    TEAMMATES,
    TRAIN_BY_OTHER,
    CANCEL,
    MAX_TRAIN_STRING_COUNT,
};


// the remove merc from team pop up box strings
public enum REMOVE
{
    REMOVE_MERC = 0,
    REMOVE_MERC_CANCEL,
    MAX_REMOVE_MERC_COUNT,
};

// attribute menu defines (must match NUM_TRAINABLE_STATS defines, and pAttributeMenuStrings )
public enum ATTRIBUTE_MENU
{
    STR = 0,
    DEX,
    AGI,
    HEA,
    MARK,
    MED,
    MECH,
    LEAD,
    EXPLOS,
    CANCEL,
    MAX_ATTRIBUTE_STRING_COUNT,
};

// contract menu defines
public enum CONTRACT_MENU
{
    CURRENT_FUNDS = 0,
    SPACE,
    DAY,
    WEEK,
    TWO_WEEKS,
    TERMINATE,
    CANCEL,
    MAX_CONTRACT_MENU_STRING_COUNT,
};

// squad menu defines
public enum SQUAD_MENU
{
    SQUAD_MENU_1,
    SQUAD_MENU_2,
    SQUAD_MENU_3,
    SQUAD_MENU_4,
    SQUAD_MENU_5,
    SQUAD_MENU_6,
    SQUAD_MENU_7,
    SQUAD_MENU_8,
    SQUAD_MENU_9,
    SQUAD_MENU_10,
    SQUAD_MENU_11,
    SQUAD_MENU_12,
    SQUAD_MENU_13,
    SQUAD_MENU_14,
    SQUAD_MENU_15,
    SQUAD_MENU_16,
    SQUAD_MENU_17,
    SQUAD_MENU_18,
    SQUAD_MENU_19,
    SQUAD_MENU_20,
    SQUAD_MENU_CANCEL,
    MAX_SQUAD_MENU_STRING_COUNT,
};
