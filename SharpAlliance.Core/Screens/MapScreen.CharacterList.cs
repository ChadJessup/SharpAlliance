using System;
using SharpAlliance.Core.Managers;
using SharpAlliance.Core.Managers.VideoSurfaces;
using SharpAlliance.Core.SubSystems;
using SixLabors.ImageSharp;

using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core.Screens;

/// <summary>
/// The team (character) list down the left side of the map screen: LoadCharacters, DisplayCharacterList and the
/// Draw* helpers in mapscreen.c.
/// TODO(port): sorting, vehicles and their passengers, the plotted-path / contract / assignment menus, the
/// flashing of contracts that are about to end, squad colouring, and the character info panel and inventory.
/// </summary>
public partial class MapScreen
{
    private const FontStyle MAP_SCREEN_FONT = FontStyle.BLOCKFONT2;

    private const int PLAYER_INFO_X = 0;
    private const int PLAYER_INFO_Y = 107;
    private const int Y_START = 146;
    private const int Y_OFFSET = 2;
    private const int FIRST_VEHICLE = 18;
    private const int NAME_X = 11;
    private const int NAME_WIDTH = 62 - NAME_X;
    private const int ASSIGN_X = 67;
    private const int ASSIGN_WIDTH = 118 - ASSIGN_X;
    private const int LOC_X = 147;
    private const int LOC_WIDTH = 179 - LOC_X;
    private const int DEST_ETA_X = 184;
    private const int DEST_ETA_WIDTH = 217 - DEST_ETA_X;
    private const int TIME_REMAINING_X = 222;
    private const int TIME_REMAINING_WIDTH = 250 - TIME_REMAINING_X;
    private const int MAP_TIME_UNDER_THIS_DISPLAY_AS_HOURS = 3 * 24 * 60;
    private const FontColor FONT_MAP_DKYELLOW = (FontColor)170;
    private const FontColor CHAR_TEXT_FONT_COLOR = (FontColor)5;

    private static readonly string[] pAssignmentStrings =
    [
        "Squad 1", "Squad 2", "Squad 3", "Squad 4", "Squad 5", "Squad 6", "Squad 7", "Squad 8", "Squad 9", "Squad 10",
        "Squad 11", "Squad 12", "Squad 13", "Squad 14", "Squad 15", "Squad 16", "Squad 17", "Squad 18", "Squad 19", "Squad 20",
        "On Duty", "Doctor", "Patient", "Vehicle", "In Trans", "Repair", "Practice", "Militia", "Trainer", "Student",
        "Dead", "Incap.", "POW", "Hospital", "Empty",
    ];

    private static readonly string[] pShortVehicleStrings = ["Eldor.", "Hummer", "Truck", "Jeep", "Tank", "Heli"];

    private readonly MOUSE_REGION[] teamListRegions = new MOUSE_REGION[MAX_CHARACTER_COUNT];
    private int highlightedCharacter = -1;
    private bool fTeamListRegionsCreated;

    private int RowHeight => FontSubSystem.GetFontHeight(MAP_SCREEN_FONT) + Y_OFFSET;

    /// <summary>Fills the list with the player's active mercs (LoadCharacters / AddCharacter).</summary>
    private void LoadCharacters()
    {
        for (int i = 0; i < gCharactersList.Length; i++)
        {
            gCharactersList[i] = default;
        }

        int count = 0;
        for (int cnt = 0; cnt <= gTacticalStatus.Team[OUR_TEAM].bLastID && count < MAX_CHARACTER_COUNT; cnt++)
        {
            var pSoldier = MercPtrs[cnt];
            if (pSoldier is null || !pSoldier.bActive)
            {
                continue;
            }

            // TODO(port): vehicles are placed from FIRST_VEHICLE on
            gCharactersList[count].usSolID = pSoldier.ubID;
            gCharactersList[count].fValid = true;
            count++;
        }

        if (count == 0)
        {
            bSelectedInfoChar = -1;
        }
        else if (bSelectedInfoChar < 0 || !gCharactersList[bSelectedInfoChar].fValid)
        {
            bSelectedInfoChar = 0;
        }
    }

    private void CreateMouseRegionsForTeamList()
    {
        if (fTeamListRegionsCreated)
        {
            return;
        }

        fTeamListRegionsCreated = true;
        for (int i = 0; i < MAX_CHARACTER_COUNT; i++)
        {
            int row = i;
            teamListRegions[i] = new MOUSE_REGION($"Team List {i}");
            MouseSubSystem.MSYS_DefineRegion(
                teamListRegions[i],
                new Rectangle(NAME_X, Y_START + row * (RowHeight), TIME_REMAINING_X + TIME_REMAINING_WIDTH - NAME_X, RowHeight),
                MSYS_PRIORITY.HIGH - 2,
                CURSOR.NORMAL,
                TeamListMoveCallback,
                TeamListButtonCallback);
            teamListRegions[i].UserData[0] = row;
        }
    }

    private void RemoveMouseRegionsForTeamList()
    {
        if (!fTeamListRegionsCreated)
        {
            return;
        }

        fTeamListRegionsCreated = false;
        foreach (var region in teamListRegions)
        {
            MouseSubSystem.MSYS_RemoveRegion(region);
        }

        highlightedCharacter = -1;
    }

    private static int highlightLine = -1;

    private static void TeamListMoveCallback(ref MOUSE_REGION region, MSYS_CALLBACK_REASON reason)
    {
        int row = (int)region.UserData[0];

        if (reason.HasFlag(MSYS_CALLBACK_REASON.GAIN_MOUSE) && gCharactersList[row].fValid)
        {
            highlightLine = row;
        }
        else if (reason.HasFlag(MSYS_CALLBACK_REASON.LOST_MOUSE) && highlightLine == row)
        {
            highlightLine = -1;
        }
    }

    private static void TeamListButtonCallback(ref MOUSE_REGION region, MSYS_CALLBACK_REASON reason)
    {
        int row = (int)region.UserData[0];

        if (reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP) && gCharactersList[row].fValid)
        {
            // TODO(port): shift/ctrl multi-select and the right-click assignment menu
            bSelectedInfoChar = row;

            // a click in the sleep column also puts the merc to sleep or wakes them
            if (region.MousePos.X >= SLEEP_X && region.MousePos.X < SLEEP_X + SLEEP_WIDTH)
            {
                ToggleSleep(row);
            }
        }
    }

    /// <summary>Draws the panel the list sits on and then the list itself (RenderTeamRegionBackground / DisplayCharacterList).</summary>
    private void DisplayCharacterList()
    {
        var panel = video.GetVideoObject(Utils.FilenameForBPP("INTERFACE\\newgoldpiece3.sti"));
        video.BltVideoObject(SurfaceType.FRAME_BUFFER, panel, 0, PLAYER_INFO_X, PLAYER_INFO_Y, VO_BLT.SRCTRANSPARENCY);

        FontSubSystem.SetFont(MAP_SCREEN_FONT);
        FontSubSystem.SetFontBackground(FontColor.FONT_BLACK);

        for (int sCount = 0; sCount < MAX_CHARACTER_COUNT; sCount++)
        {
            if (!gCharactersList[sCount].fValid)
            {
                continue;
            }

            var pSoldier = MercPtrs[gCharactersList[sCount].usSolID];

            FontColor color;
            if (sCount == highlightLine || sCount == bSelectedInfoChar)
            {
                color = FontColor.FONT_WHITE;
            }
            else if (pSoldier.bLife == 0)
            {
                color = FontColor.FONT_METALGRAY;
            }
            else if (pSoldier.sSectorX == MapScreenInterfaceMap.sSelMapX
                && pSoldier.sSectorY == MapScreenInterfaceMap.sSelMapY
                && pSoldier.bSectorZ == MapScreenInterfaceMap.iCurrentMapSectorZ)
            {
                // in the current sector: mobile ones are bright, ones on duty are darker
                color = (pSoldier.bAssignment < Assignment.ON_DUTY) || (pSoldier.bAssignment == Assignment.VEHICLE)
                    ? FontColor.FONT_YELLOW
                    : FONT_MAP_DKYELLOW;
            }
            else
            {
                // not in current sector
                color = CHAR_TEXT_FONT_COLOR;
            }

            FontSubSystem.SetFontForeground(color);

            DrawCell(pSoldier.name, sCount, NAME_X, NAME_WIDTH);
            DrawCell(GetMapscreenMercLocationString(pSoldier), sCount, LOC_X, LOC_WIDTH);
            DrawCell(GetMapscreenMercDestinationString(pSoldier), sCount, DEST_ETA_X, DEST_ETA_WIDTH);
            DrawCell(GetMapscreenMercAssignmentString(pSoldier), sCount, ASSIGN_X, ASSIGN_WIDTH);

            string departure = GetMapscreenMercDepartureString(pSoldier, ref color);
            FontSubSystem.SetFontForeground(sCount == highlightLine ? FontColor.FONT_WHITE : color);
            DrawCell(departure, sCount, TIME_REMAINING_X, TIME_REMAINING_WIDTH);
        }

        DisplayIconsForMercsAsleep();
    }

    /// <summary>Prints text centred in one column of a row of the list (DrawName, DrawLocation, ...).</summary>
    private void DrawCell(string text, int row, int x, int width)
    {
        if (text.Length == 0)
        {
            return;
        }

        int yTop = Y_START + row * RowHeight;
        if (row >= FIRST_VEHICLE)
        {
            yTop += 6;
        }

        FontSubSystem.FindFontCenterCoordinates(x + 1, yTop, width, RowHeight, text, MAP_SCREEN_FONT, out int usX, out int usY);
        mprintf(usX, usY, text);
    }

    private static string GetMapscreenMercAssignmentString(SOLDIERTYPE pSoldier)
    {
        if (pSoldier.bAssignment != Assignment.VEHICLE)
        {
            int index = (int)pSoldier.bAssignment;
            return index >= 0 && index < pAssignmentStrings.Length ? pAssignmentStrings[index] : string.Empty;
        }

        // TODO(port): pVehicleList[pSoldier.iVehicleId].ubVehicleType
        return pAssignmentStrings[(int)Assignment.VEHICLE];
    }

    private static string GetMapscreenMercLocationString(SOLDIERTYPE pSoldier)
    {
        if (pSoldier.bAssignment == Assignment.IN_TRANSIT)
        {
            // show blank
            return "--";
        }

        if (pSoldier.bAssignment == Assignment.ASSIGNMENT_POW)
        {
            // POW - location unknown
            return pPOWStrings[1];
        }

        string sector = ((char)('A' + (int)pSoldier.sSectorY - 1)).ToString() + pSoldier.sSectorX;
        if (pSoldier.bSectorZ > 0)
        {
            sector += "-" + pSoldier.bSectorZ;
        }

        // put brackets around it when he's between sectors
        return pSoldier.fBetweenSectors ? $"({sector})" : sector;
    }

    private static string GetMapscreenMercDestinationString(SOLDIERTYPE pSoldier)
    {
        // dead or POW: no destination
        if (pSoldier.bAssignment == Assignment.ASSIGNMENT_DEAD || pSoldier.bAssignment == Assignment.ASSIGNMENT_POW || pSoldier.bLife == 0)
        {
            return string.Empty;
        }

        if (pSoldier.bAssignment == Assignment.IN_TRANSIT)
        {
            // show the sector he'll be arriving in
            return ((char)('A' + (int)gsMercArriveSectorY - 1)).ToString() + gsMercArriveSectorX;
        }

        // TODO(port): the last sector of his plotted path (GetLastSectorIdInCharactersPath) and the next sector when between sectors
        return string.Empty;
    }

    private static string GetMapscreenMercDepartureString(SOLDIERTYPE pSoldier, ref FontColor color)
    {
        if ((pSoldier.ubWhatKindOfMercAmI != MERC_TYPE.AIM_MERC && pSoldier.ubProfile != NPCID.SLAY) || pSoldier.bLife == 0)
        {
            return "N/A";
        }

        int iMinsRemaining = pSoldier.iEndofContractTime - (int)GameClock.GetWorldTotalMin();

        // if the merc is in transit, the time left can't be more than the contract length
        if (pSoldier.bAssignment == Assignment.IN_TRANSIT && iMinsRemaining > pSoldier.iTotalContractLength * NUM_MIN_IN_DAY)
        {
            iMinsRemaining = pSoldier.iTotalContractLength * NUM_MIN_IN_DAY;
        }

        if (iMinsRemaining >= MAP_TIME_UNDER_THIS_DISPLAY_AS_HOURS)
        {
            color = FontColor.FONT_LTGREEN;
            return (iMinsRemaining / (24 * 60)) + "d";
        }

        // less than 3 days: red (TODO(port): flashing in the last 4 hours)
        color = FontColor.FONT_RED;
        int hours = iMinsRemaining > 5 ? (iMinsRemaining + 59) / 60 : 0;
        return hours + "h";
    }
}
