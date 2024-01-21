using System;
using SharpAlliance.Core.Screens;
using static SharpAlliance.Core.Globals;

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

    private static void CreateDestroyMouseRegionForVehicleMenu()
    {
        throw new NotImplementedException();
    }

    private static void CreateDestroyMouseRegionsForAssignmentMenu()
    {
        throw new NotImplementedException();
    }

    private static void CreateDestroyMouseRegionsForTrainingMenu()
    {
        throw new NotImplementedException();
    }

    private static void CreateDestroyMouseRegionsForAttributeMenu()
    {
        throw new NotImplementedException();
    }

    private static void CreateDestroyMouseRegionsForSquadMenu(bool v)
    {
        throw new NotImplementedException();
    }

    private static void CreateDestroyMouseRegionForRepairMenu()
    {
        throw new NotImplementedException();
    }

    private static SOLDIERTYPE? GetSelectedAssignSoldier(bool v)
    {
        throw new NotImplementedException();
    }

    internal static void CreateDestroyAssignmentPopUpBoxes()
    {
        throw new NotImplementedException();
    }

    private static void CreateDestroyScreenMaskForAssignmentAndContractMenus()
    {
        throw new NotImplementedException();
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
