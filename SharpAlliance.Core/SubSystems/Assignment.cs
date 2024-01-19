using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core.SubSystems;

public class Assignments
{
    public static void DetermineWhichAssignmentMenusCanBeShown()
    {
        bool fCharacterNoLongerValid = false;
        SOLDIERTYPE? pSoldier = null;

        if ((guiTacticalInterfaceFlags.HasFlag(INTERFACE.MAPSCREEN)))
        {
            if (fShowMapScreenMovementList == true)
            {
                if (bSelectedDestChar == -1)
                {
                    fCharacterNoLongerValid = true;
                    HandleShowingOfMovementBox();
                }
                else
                {
                    fShowMapScreenMovementList = false;
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
            else if (bSelectedAssignChar == -1)
            {
                fCharacterNoLongerValid = true;
            }

            // update the assignment positions
            UpdateMapScreenAssignmentPositions();
        }

        // determine which assign menu needs to be shown
        if (((fShowAssignmentMenu == false)) || (fCharacterNoLongerValid == true))
        {
            // reset show assignment menus
            fShowAssignmentMenu = false;
            fShowVehicleMenu = false;
            fShowRepairMenu = false;

            // destroy mask, if needed
            CreateDestroyScreenMaskForAssignmentAndContractMenus();


            // destroy menu if needed
            CreateDestroyMouseRegionForVehicleMenu();
            CreateDestroyMouseRegionsForAssignmentMenu();
            CreateDestroyMouseRegionsForTrainingMenu();
            CreateDestroyMouseRegionsForAttributeMenu();
            CreateDestroyMouseRegionsForSquadMenu(true);
            CreateDestroyMouseRegionForRepairMenu();

            // hide all boxes being shown
            if (IsBoxShown(ghEpcBox))
            {
                HideBox(ghEpcBox);
                fTeamPanelDirty = true;
                gfRenderPBInterface = true;
            }
            if (IsBoxShown(ghAssignmentBox))
            {
                HideBox(ghAssignmentBox);
                fTeamPanelDirty = true;
                gfRenderPBInterface = true;
            }
            if (IsBoxShown(ghTrainingBox))
            {
                HideBox(ghTrainingBox);
                fTeamPanelDirty = true;
                gfRenderPBInterface = true;
            }
            if (IsBoxShown(ghRepairBox))
            {
                HideBox(ghRepairBox);
                fTeamPanelDirty = true;
                gfRenderPBInterface = true;
            }
            if (IsBoxShown(ghAttributeBox))
            {
                HideBox(ghAttributeBox);
                fTeamPanelDirty = true;
                gfRenderPBInterface = true;
            }
            if (IsBoxShown(ghVehicleBox))
            {
                HideBox(ghVehicleBox);
                fTeamPanelDirty = true;
                gfRenderPBInterface = true;
            }

            // do we really want ot hide this box?
            if (fShowContractMenu == false)
            {
                if (IsBoxShown(ghRemoveMercAssignBox))
                {
                    HideBox(ghRemoveMercAssignBox);
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


        if (((Menptr[gCharactersList[bSelectedInfoChar].usSolID].bLife == 0) || (Menptr[gCharactersList[bSelectedInfoChar].usSolID].bAssignment == ASSIGNMENT_POW)) && ((guiTacticalInterfaceFlags & INTERFACE_MAPSCREEN)))
        {
            // show basic assignment menu
            ShowBox(ghRemoveMercAssignBox);
        }
        else
        {
            pSoldier = GetSelectedAssignSoldier(false);

            if (pSoldier.ubWhatKindOfMercAmI == MERC_TYPE.EPC)
            {
                ShowBox(ghEpcBox);
            }
            else
            {
                // show basic assignment menu

                ShowBox(ghAssignmentBox);
            }
        }

        // TRAINING menu
        if (fShowTrainingMenu == true)
        {
            HandleShadingOfLinesForTrainingMenu();
            ShowBox(ghTrainingBox);
        }
        else
        {
            if (IsBoxShown(ghTrainingBox))
            {
                HideBox(ghTrainingBox);
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
            ShowBox(ghRepairBox);
        }
        else
        {
            // hide box
            if (IsBoxShown(ghRepairBox))
            {
                HideBox(ghRepairBox);
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
            ShowBox(ghAttributeBox);
        }
        else
        {
            if (IsBoxShown(ghAttributeBox))
            {
                HideBox(ghAttributeBox);
                fTeamPanelDirty = true;
                fMapPanelDirty = true;
                gfRenderPBInterface = true;
                //	SetRenderFlags(RENDER_FLAG_FULL);
            }

        }

        // VEHICLE menu
        if (fShowVehicleMenu == true)
        {
            ShowBox(ghVehicleBox);
        }
        else
        {
            if (IsBoxShown(ghVehicleBox))
            {
                HideBox(ghVehicleBox);
                fTeamPanelDirty = true;
                fMapPanelDirty = true;
                gfRenderPBInterface = true;
                //	SetRenderFlags(RENDER_FLAG_FULL);
            }
        }

        CreateDestroyMouseRegionForVehicleMenu();

        return;
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
