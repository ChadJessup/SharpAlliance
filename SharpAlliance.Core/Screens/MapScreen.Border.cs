using System;
using System.Collections.Generic;
using SharpAlliance.Core.SubSystems;
using SixLabors.ImageSharp;

using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core.Screens;

/// <summary>
/// The row of buttons on the map border that switch the map overlays: towns, mines, teams, airspace, items and
/// militia (CreateButtonsForMapBorder and the Btn*Callback functions in Map Screen Interface Border.c).
/// </summary>
public partial class MapScreen
{
    private static readonly string[] pMapScreenBorderButtonHelpText =
    [
        "Show To|wns",
        "Show |Mines",
        "Show |Teams & Enemies",
        "Show |Airspace",
        "Show |Items",
        "Show Militia & Enemies (|Z)",
    ];

    private readonly List<ButtonPic> borderButtonImages = [];
    private bool fBorderButtonsCreated;

    private void CreateBorderButton(MAP_BORDER which, int off, int on, int x, GUI_CALLBACK click)
    {
        var image = ButtonSubSystem.LoadButtonImage("INTERFACE\\map_border_buttons.sti", -1, off, -1, on, -1);
        borderButtonImages.Add(image);

        var button = ButtonSubSystem.QuickCreateButton(
            image,
            new Point(x, 323),
            ButtonFlags.BUTTON_NO_TOGGLE,
            MSYS_PRIORITY.HIGH,
            null,
            click);

        ButtonSubSystem.SetButtonFastHelpText(button, pMapScreenBorderButtonHelpText[(int)which]);
        ButtonSubSystem.SetButtonCursor(button, CURSOR.NORMAL);

        giMapBorderButtons[which] = button;
        buttonList.Add(button);
    }

    private void CreateButtonsForMapBorder()
    {
        if (fBorderButtonsCreated)
        {
            return;
        }

        fBorderButtonsCreated = true;

        CreateBorderButton(MAP_BORDER.TOWN_BTN, 5, 14, 299, BorderButton(MapScreenInterfaceBorder.ToggleShowTownsMode));
        CreateBorderButton(MAP_BORDER.MINE_BTN, 4, 13, 342, BorderButton(MapScreenInterfaceBorder.ToggleShowMinesMode));
        CreateBorderButton(MAP_BORDER.TEAMS_BTN, 3, 12, 385, BorderButton(MapScreenInterfaceBorder.ToggleShowTeamsMode));
        CreateBorderButton(MAP_BORDER.MILITIA_BTN, 8, 17, 428, BorderButton(MapScreenInterfaceBorder.ToggleShowMilitiaMode));
        CreateBorderButton(MAP_BORDER.AIRSPACE_BTN, 2, 11, 471, BorderButton(MapScreenInterfaceBorder.ToggleAirspaceMode));
        CreateBorderButton(MAP_BORDER.ITEM_BTN, 1, 10, 514, BorderButton(MapScreenInterfaceBorder.ToggleItemsFilter));

        // make the pressed look match the flags
        MapScreenInterfaceBorder.InitializeMapBorderButtonStates();
    }

    private static GUI_CALLBACK BorderButton(System.Action toggle)
        => (ref GUI_BUTTON btn, MSYS_CALLBACK_REASON reason) =>
        {
            if (reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_DWN))
            {
                // TODO(port): CommonBtnCallbackBtnDownChecks (stop the help text, cancel a UI message)
                toggle();
            }
        };

    private void DestroyBorderButtons()
    {
        if (!fBorderButtonsCreated)
        {
            return;
        }

        fBorderButtonsCreated = false;

        foreach (var button in giMapBorderButtons.Values)
        {
            buttonList.Remove(button);
            ButtonSubSystem.RemoveButton(button);
        }

        giMapBorderButtons.Clear();

        foreach (var image in borderButtonImages)
        {
            ButtonSubSystem.UnloadButtonImage(image);
        }

        borderButtonImages.Clear();
    }

    /// <summary>
    /// Mine details under each mine while the mines filter is on: name, whether it is shut down or running out, and
    /// the daily production if the player controls it (BlitMineText).
    /// </summary>
    private static void ShowMineText()
    {
        FontSubSystem.SetFont(FontStyle.BLOCKFONT2);
        FontSubSystem.SetFontForeground(FontColor.FONT_LTGREEN);
        FontSubSystem.SetFontBackground(FontColor.FONT_BLACK);

        int lineHeight = FontSubSystem.GetFontHeight(FontStyle.BLOCKFONT2);

        foreach (var (mine, location) in gMineLocation)
        {
            // when not zoomed, the position is the top left corner of the sector: centre on it, slightly below
            var (sx, sy) = ScreenXYFromMapXY(location.sSectorX, (int)location.sSectorY);
            sx += MAP_GRID_X / 2;
            sy += MAP_GRID_Y + 1;

            var lines = new List<string>();
            pTownNames.TryGetValue(StrategicMines.GetTownAssociatedWithMine(mine), out string? town);
            lines.Add($"{town} {pwMineStrings[0]}");

            if (gMineStatus.TryGetValue(mine, out var status))
            {
                if (status.fEmpty)
                {
                    lines.Add(pwMineStrings[5]);
                }
                else if (status.fShutDown)
                {
                    lines.Add(pwMineStrings[6]);
                }
                else if (status.fRunningOut)
                {
                    lines.Add(pwMineStrings[7]);
                }

                // only show production if the player controls it and it is actually producing
                if (StrategicMines.PlayerControlsMine(mine) && !status.fEmpty)
                {
                    lines.Add(FormatDollars(StrategicMines.PredictDailyIncomeFromAMine(mine)));
                }
            }

            for (int i = 0; i < lines.Count; i++)
            {
                // don't go past the left edge of the map
                int x = Math.Max(sx - FontSubSystem.StringPixLength(lines[i], FontStyle.BLOCKFONT2) / 2, MAP_VIEW_START_X + 23);
                mprintf(x, sy + i * lineHeight, lines[i]);
            }
        }
    }
}
