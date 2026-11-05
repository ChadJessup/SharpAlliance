using System;
using System.Collections.Generic;
using SharpAlliance.Core.Managers;
using SharpAlliance.Core.Managers.VideoSurfaces;
using SharpAlliance.Core.SubSystems;
using SixLabors.ImageSharp;

using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core.Screens;

/// <summary>
/// The bottom panel of the map screen (Map Screen Interface Bottom.c): the buttons to leave for the laptop,
/// tactical and options, the time compression buttons and pause region, the message log with its scroll
/// buttons, the sector name, the balance and the compression mode text.
/// TODO(port): the message scroll bar slider, daily mine income, the radar picture of the selected sector,
/// the keyboard shortcuts and the flashing "paused" text.
/// </summary>
public partial class MapScreen
{
    private const FontColor MAP_BOTTOM_FONT_COLOR = (FontColor)(32 * 4 - 9);

    private static readonly string[] sTimeStrings = ["Paused", "Normal", "5 min", "30 min", "60 min", "6 hrs"];

    private static readonly string[] pMapScreenBottomFastHelp =
    [
        "|Laptop",
        "Tactical (|E|s|c)",
        "|Options",
        "Time Compress (|+)",
        "Time Compress (|-)",
        "Previous Message (|U|p)\nPrevious Page (|P|g|U|p)",
        "Next Message (|D|o|w|n)\nNext Page (|P|g|D|n)",
        "Start/Stop Time (|S|p|a|c|e)",
    ];

    private readonly List<GUI_BUTTON> bottomButtons = [];
    private readonly List<ButtonPic> bottomButtonImages = [];
    private readonly MOUSE_REGION pauseRegion = new("Map Pause");
    private bool fBottomCreated;
    private ScreenName? screenToLeaveFor;

    private GUI_BUTTON CreateBottomButton(string file, int off, int on, Point at, MSYS_PRIORITY priority, GUI_CALLBACK click, string help, int grayed = -1)
    {
        var image = ButtonSubSystem.LoadButtonImage(file, grayed, off, -1, on, -1);
        bottomButtonImages.Add(image);

        var button = ButtonSubSystem.QuickCreateButton(
            image,
            at,
            ButtonFlags.BUTTON_TOGGLE,
            priority,
            MouseSubSystem.BtnGenericMouseMoveButtonCallback,
            click);

        ButtonSubSystem.SetButtonFastHelpText(button, help);
        ButtonSubSystem.SetButtonCursor(button, CURSOR.NORMAL);
        bottomButtons.Add(button);
        return button;
    }

    private void CreateBottomInterface()
    {
        if (fBottomCreated)
        {
            return;
        }

        fBottomCreated = true;

        CreateBottomButton("INTERFACE\\map_border_buttons.sti", 6, 15, new(456, 410), MSYS_PRIORITY.HIGHEST - 1, BtnLaptopCallback, pMapScreenBottomFastHelp[0]);
        CreateBottomButton("INTERFACE\\map_border_buttons.sti", 7, 16, new(496, 410), MSYS_PRIORITY.HIGHEST - 1, BtnTacticalCallback, pMapScreenBottomFastHelp[1]);
        CreateBottomButton("INTERFACE\\map_border_buttons.sti", 18, 19, new(458, 372), MSYS_PRIORITY.HIGHEST - 1, BtnOptionsCallback, pMapScreenBottomFastHelp[2]);

        // time compression buttons
        CreateBottomButton("INTERFACE\\map_screen_bottom_arrows.sti", 1, 3, new(528, 456), MSYS_PRIORITY.HIGHEST - 2, BtnTimeCompressMoreCallback, pMapScreenBottomFastHelp[3], grayed: 10);
        CreateBottomButton("INTERFACE\\map_screen_bottom_arrows.sti", 0, 2, new(466, 456), MSYS_PRIORITY.HIGHEST - 2, BtnTimeCompressLessCallback, pMapScreenBottomFastHelp[4], grayed: 9);

        // message scroll buttons
        CreateBottomButton("INTERFACE\\map_screen_bottom_arrows.sti", 4, 6, new(331, 371), MSYS_PRIORITY.HIGHEST - 1, BtnMessageUpCallback, pMapScreenBottomFastHelp[5], grayed: 11);
        CreateBottomButton("INTERFACE\\map_screen_bottom_arrows.sti", 5, 7, new(331, 452), MSYS_PRIORITY.HIGHEST - 1, BtnMessageDownCallback, pMapScreenBottomFastHelp[6], grayed: 12);

        // start / stop time by clicking the compression text
        MouseSubSystem.MSYS_DefineRegion(
            pauseRegion,
            new Rectangle(487, 456, 522 - 487, 467 - 456),
            MSYS_PRIORITY.HIGH,
            CURSOR.NORMAL,
            MSYS_NO_CALLBACK,
            CompressModeClickCallback);

        buttonList.AddRange(bottomButtons);
    }

    private void DestroyBottomInterface()
    {
        if (!fBottomCreated)
        {
            return;
        }

        fBottomCreated = false;

        foreach (var button in bottomButtons)
        {
            buttonList.Remove(button);
            ButtonSubSystem.RemoveButton(button);
        }

        foreach (var image in bottomButtonImages)
        {
            ButtonSubSystem.UnloadButtonImage(image);
        }

        bottomButtons.Clear();
        bottomButtonImages.Clear();
        MouseSubSystem.MSYS_RemoveRegion(pauseRegion);
    }

    private void BtnLaptopCallback(ref GUI_BUTTON btn, MSYS_CALLBACK_REASON reason)
        => ExitButton(ref btn, reason, ScreenName.LAPTOP_SCREEN);

    private void BtnOptionsCallback(ref GUI_BUTTON btn, MSYS_CALLBACK_REASON reason)
        => ExitButton(ref btn, reason, ScreenName.OPTIONS_SCREEN);

    private void BtnTacticalCallback(ref GUI_BUTTON btn, MSYS_CALLBACK_REASON reason)
    {
        // TODO(port): the tactical screen needs a loaded sector (UpdateMercInSector / SetCurrentWorldSector)
        ExitButton(ref btn, reason, null);
    }

    private void ExitButton(ref GUI_BUTTON btn, MSYS_CALLBACK_REASON reason, ScreenName? screen)
    {
        if (reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_DWN))
        {
            btn.uiFlags |= ButtonFlags.BUTTON_CLICKED_ON;
        }
        else if (reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP) && btn.uiFlags.HasFlag(ButtonFlags.BUTTON_CLICKED_ON))
        {
            btn.uiFlags &= ~ButtonFlags.BUTTON_CLICKED_ON;
            fMapScreenBottomDirty = true;

            if (screen is not null)
            {
                if (screen == ScreenName.OPTIONS_SCREEN)
                {
                    guiPreviousOptionScreen = ScreenName.MAP_SCREEN;
                }

                screenToLeaveFor = screen;
            }
        }
    }

    private static bool CommonTimeCompressionChecks()
    {
        // TODO(port): stop the map help text, and abort movement plotting
        return false;
    }

    private void CompressModeClickCallback(ref MOUSE_REGION region, MSYS_CALLBACK_REASON reason)
    {
        if (reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP) || reason.HasFlag(MSYS_CALLBACK_REASON.RBUTTON_UP))
        {
            if (CommonTimeCompressionChecks())
            {
                return;
            }

            RequestToggleTimeCompression();
        }
    }

    private void BtnTimeCompressMoreCallback(ref GUI_BUTTON btn, MSYS_CALLBACK_REASON reason)
        => TimeButton(ref btn, reason, RequestIncreaseInTimeCompression);

    private void BtnTimeCompressLessCallback(ref GUI_BUTTON btn, MSYS_CALLBACK_REASON reason)
        => TimeButton(ref btn, reason, RequestDecreaseInTimeCompression);

    private static void TimeButton(ref GUI_BUTTON btn, MSYS_CALLBACK_REASON reason, Action action)
    {
        if (reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_DWN))
        {
            if (!CommonTimeCompressionChecks())
            {
                btn.uiFlags |= ButtonFlags.BUTTON_CLICKED_ON;
            }
        }
        else if (reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP) && btn.uiFlags.HasFlag(ButtonFlags.BUTTON_CLICKED_ON))
        {
            btn.uiFlags &= ~ButtonFlags.BUTTON_CLICKED_ON;
            fMapScreenBottomDirty = true;
            action();
        }
    }

    private static void RequestToggleTimeCompression()
    {
        if (!GameClock.IsTimeBeingCompressed())
        {
            GameClock.StartTimeCompression();
        }
        else
        {
            GameClock.StopTimeCompression();
        }
    }

    private static void RequestIncreaseInTimeCompression()
    {
        if (GameClock.IsTimeBeingCompressed())
        {
            GameClock.IncreaseGameTimeCompressionRate();
        }
        else
        {
            // start over at 5x compression
            GameClock.SetGameTimeCompressionLevel(TIME_COMPRESS.TIME_COMPRESS_5MINS);
        }
    }

    private static void RequestDecreaseInTimeCompression()
    {
        if (GameClock.IsTimeBeingCompressed())
        {
            GameClock.DecreaseGameTimeCompressionRate();
        }
    }

    private void BtnMessageUpCallback(ref GUI_BUTTON btn, MSYS_CALLBACK_REASON reason)
        => ScrollButton(ref btn, reason, -1);

    private void BtnMessageDownCallback(ref GUI_BUTTON btn, MSYS_CALLBACK_REASON reason)
        => ScrollButton(ref btn, reason, 1);

    private static void ScrollButton(ref GUI_BUTTON btn, MSYS_CALLBACK_REASON reason, int direction)
    {
        if (reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_DWN))
        {
            btn.uiFlags |= ButtonFlags.BUTTON_CLICKED_ON;
        }
        else if (reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP) && btn.uiFlags.HasFlag(ButtonFlags.BUTTON_CLICKED_ON))
        {
            btn.uiFlags &= ~ButtonFlags.BUTTON_CLICKED_ON;

            int range = Messages.GetRangeOfMapScreenMessages();
            int maxFirst = Math.Max(0, range - MAX_MESSAGES_ON_MAP_BOTTOM);
            int next = Math.Clamp(gubFirstMapscreenMessageIndex + direction, 0, maxFirst);
            MapScreenInterfaceBottom.ChangeCurrentMapscreenMessageIndex(next);
        }
    }

    /// <summary>Everything the bottom panel shows each frame (RenderMapScreenInterfaceBottom).</summary>
    private void DisplayMapScreenBottom()
    {
        FontSubSystem.SetFont(FontStyle.COMPFONT);
        FontSubSystem.SetFontBackground(FontColor.FONT_BLACK);

        // balance and daily income titles
        FontSubSystem.SetFontForeground(MAP_BOTTOM_FONT_COLOR);
        DrawBottomCentered("Current Balance", 359, 387 - 14, 437 - 359, 10);
        DrawBottomCentered("Daily Income", 359, 433 - 14, 437 - 359, 10);

        // current balance
        FontSubSystem.SetFontForeground((FontColor)183);
        DrawBottomCentered(FormatDollars(LaptopSaveInfo.iCurrentBalance), 359, 387 + 2, 437 - 359, 10);

        // projected daily income from the mines
        DrawBottomCentered(FormatDollars(Finances.GetProjectedTotalDailyIncome()), 359, 433 + 2, 437 - 359, 10);

        // the sector being looked at
        StrategicMap.GetSectorIDString(MapScreenInterfaceMap.sSelMapX, MapScreenInterfaceMap.sSelMapY, MapScreenInterfaceMap.iCurrentMapSectorZ, out string sector, true);
        DrawBottomCentered(sector, 548, 426, 80, 16);

        DisplayCompressMode();
        DisplayMessageList();
    }

    private static void DrawBottomCentered(string text, int x, int y, int width, int height)
    {
        FontSubSystem.FindFontCenterCoordinates(x, y, width, height, text, FontStyle.COMPFONT, out int usX, out int usY);
        mprintf(usX, usY, text);
    }

    private static void DisplayCompressMode()
    {
        string text = GameClock.IsTimeBeingCompressed()
            ? sTimeStrings[Math.Clamp((int)giTimeCompressMode, 0, sTimeStrings.Length - 1)]
            : sTimeStrings[0];

        FontSubSystem.SetFont(FontStyle.COMPFONT);
        FontSubSystem.SetFontForeground(FontColor.FONT_LTGREEN);
        FontSubSystem.SetFontBackground(FontColor.FONT_BLACK);
        DrawBottomCentered(text, 489, 456, 522 - 489, 467 - 454);
    }

    /// <summary>The scrollable message log (DisplayStringsInMapScreenMessageList).</summary>
    private static void DisplayMessageList()
    {
        FontSubSystem.SetFont(MAP_SCREEN_MESSAGE_FONT);
        FontSubSystem.SetFontBackground(FontColor.FONT_BLACK);

        int index = gubCurrentMapMessageString;
        int y = 377;
        int spacing = FontSubSystem.GetFontHeight(MAP_SCREEN_MESSAGE_FONT);

        for (int lines = 0; lines < MAX_MESSAGES_ON_MAP_BOTTOM; lines++)
        {
            if (index == gubEndOfMapScreenMessageList || gMapScreenMessageList[index] is null)
            {
                break;
            }

            FontSubSystem.SetFontForeground(gMapScreenMessageList[index]!.usColor);
            mprintf(20, y, gMapScreenMessageList[index]!.pString16 ?? string.Empty);

            y += spacing;
            index = (index + 1) % 256;
        }
    }
}
