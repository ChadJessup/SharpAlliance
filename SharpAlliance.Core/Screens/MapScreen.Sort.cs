using System;
using System.Collections.Generic;
using System.Linq;
using SharpAlliance.Core.SubSystems;
using SixLabors.ImageSharp;

using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core.Screens;

/// <summary>
/// The six gold buttons above the team list that name its columns and sort it (AddTeamPanelSortButtonsForMapScreen,
/// MapSortBtnCallback, ChangeCharacterListSortMethod and SortListOfMercsInTeamPanel in mapscreen.c).
/// TODO(port): sorting by destination needs the plotted paths, so mercs without a destination are all treated as
/// having none.
/// </summary>
public partial class MapScreen
{
    private const int MAX_SORT_METHODS = 6;

    private static readonly Point[] gMapSortButtons =
    [
        new(12, 125),
        new(68, 125),
        new(124, 125),
        new(148, 125),
        new(185, 125),
        new(223, 125),
    ];

    // the sleep image is out of order (last)
    private static readonly int[] sortImageIndex = [0, 1, 5, 2, 3, 4];

    private static readonly string[] wMapScreenSortButtonHelpText =
    [
        "Sort by Name (|F|1)",
        "Sort by Assignment (|F|2)",
        "Sort by Sleep Status (|F|3)",
        "Sort by Location (|F|4)",
        "Sort by Destination (|F|5)",
        "Sort by Departure Time (|F|6)",
    ];

    private static int giSortStateForMapScreenList;

    private readonly List<GUI_BUTTON> sortButtons = [];
    private readonly List<ButtonPic> sortButtonImages = [];

    private void AddTeamPanelSortButtonsForMapScreen()
    {
        if (sortButtons.Count > 0)
        {
            return;
        }

        MultilanguageGraphicUtils.GetMLGFilename(out string filename, MLG.GOLDPIECEBUTTONS);

        for (int iCounter = 0; iCounter < MAX_SORT_METHODS; iCounter++)
        {
            int method = iCounter;
            var image = ButtonSubSystem.LoadButtonImage(filename, -1, sortImageIndex[iCounter], -1, sortImageIndex[iCounter] + 6, -1);
            sortButtonImages.Add(image);

            var button = ButtonSubSystem.QuickCreateButton(
                image,
                gMapSortButtons[iCounter],
                ButtonFlags.BUTTON_TOGGLE,
                MSYS_PRIORITY.HIGHEST - 5,
                MouseSubSystem.BtnGenericMouseMoveButtonCallback,
                (ref GUI_BUTTON btn, MSYS_CALLBACK_REASON reason) => MapSortBtnCallback(ref btn, reason, method));

            ButtonSubSystem.SetButtonFastHelpText(button, wMapScreenSortButtonHelpText[iCounter]);

            sortButtons.Add(button);
            buttonList.Add(button);
        }
    }

    private void RemoveTeamPanelSortButtonsForMapScreen()
    {
        foreach (var button in sortButtons)
        {
            buttonList.Remove(button);
            ButtonSubSystem.RemoveButton(button);
        }

        foreach (var image in sortButtonImages)
        {
            ButtonSubSystem.UnloadButtonImage(image);
        }

        sortButtons.Clear();
        sortButtonImages.Clear();
    }

    private static void MapSortBtnCallback(ref GUI_BUTTON btn, MSYS_CALLBACK_REASON reason, int method)
    {
        if (reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_DWN))
        {
            btn.uiFlags |= ButtonFlags.BUTTON_CLICKED_ON;
        }
        else if (reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP) && btn.uiFlags.HasFlag(ButtonFlags.BUTTON_CLICKED_ON))
        {
            btn.uiFlags &= ~ButtonFlags.BUTTON_CLICKED_ON;
            ChangeCharacterListSortMethod(method);
        }
    }

    private static void ChangeCharacterListSortMethod(int iValue)
    {
        // TODO(port): not while the pre-battle interface is up, and abort movement plotting
        giSortStateForMapScreenList = iValue;
        SortListOfMercsInTeamPanel();
    }

    private static int GetContractExpiryTime(SOLDIERTYPE pSoldier)
        => (pSoldier.ubWhatKindOfMercAmI == MERC_TYPE.AIM_MERC || pSoldier.ubProfile == NPCID.SLAY)
            ? pSoldier.iEndofContractTime
            : 999999; // never - really high number

    private static int CalcLocationValueForChar(SOLDIERTYPE pSoldier)
    {
        // don't reveal location of POWs!
        if (pSoldier.bAssignment == Assignment.ASSIGNMENT_POW)
        {
            return 0;
        }

        // underground: add 1000 per sublevel
        return (int)SECTORINFO.SECTOR(pSoldier.sSectorX, pSoldier.sSectorY) + 1000 * pSoldier.bSectorZ;
    }

    /// <summary>Puts the soldiers in the list in the order the current sort button asks for (SortListOfMercsInTeamPanel).</summary>
    private static void SortListOfMercsInTeamPanel()
    {
        // remember who is selected, so the same merc stays selected after the list moves around
        int? selectedSoldier = bSelectedInfoChar >= 0 && bSelectedInfoChar < MAX_CHARACTER_COUNT && gCharactersList[bSelectedInfoChar].fValid
            ? gCharactersList[bSelectedInfoChar].usSolID
            : null;

        // only people sort; vehicles stay where they are (from FIRST_VEHICLE on)
        var people = new List<SOLDIERTYPE>();
        for (int i = 0; i < FIRST_VEHICLE && i < MAX_CHARACTER_COUNT && gCharactersList[i].fValid; i++)
        {
            people.Add(MercPtrs[gCharactersList[i].usSolID]);
        }

        IOrderedEnumerable<SOLDIERTYPE> sorted = giSortStateForMapScreenList switch
        {
            0 => people.OrderBy(s => s.name, StringComparer.OrdinalIgnoreCase),
            1 => people.OrderBy(s => (int)s.bAssignment).ThenBy(s => s.bAssignment == Assignment.VEHICLE ? s.iVehicleId : 0),
            2 => people.OrderByDescending(s => s.fMercAsleep),
            3 => people.OrderBy(CalcLocationValueForChar),
            4 => people.OrderBy(_ => 9999), // TODO(port): the last sector of the plotted path
            _ => people.OrderBy(GetContractExpiryTime),
        };

        int index = 0;
        foreach (SOLDIERTYPE soldier in sorted)
        {
            gCharactersList[index].usSolID = soldier.ubID;
            index++;
        }

        if (selectedSoldier is int id)
        {
            for (int i = 0; i < MAX_CHARACTER_COUNT; i++)
            {
                if (gCharactersList[i].fValid && gCharactersList[i].usSolID == id)
                {
                    bSelectedInfoChar = i;
                    break;
                }
            }
        }

        fTeamPanelDirty = true;
        fCharacterInfoPanelDirty = true;
    }
}
