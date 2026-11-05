using System;
using SharpAlliance.Core.Managers;
using SharpAlliance.Core.Managers.VideoSurfaces;
using SharpAlliance.Core.SubSystems;
using SixLabors.ImageSharp;

using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core.Screens;

/// <summary>
/// The character info panel in the top left corner of the map screen: face, name, assignment, health, the
/// attributes and skills, contract time, salary, medical deposit and morale (RenderCharacterInfoBackground,
/// DrawCharacterInfo, DrawCharStats, DrawCharHealth and RenderAttributeStringsForUpperLeftHandCorner in mapscreen.c).
/// TODO(port): vehicles and the robot, the ETA lines (in transit / travelling), training and repair detail, the
/// colouring of recently changed stats, the hand item and the icons for the upper left corner.
/// </summary>
public partial class MapScreen
{
    private const FontStyle CHAR_FONT = FontStyle.BLOCKFONT2;
    private const FontColor CHAR_TITLE_FONT_COLOR = (FontColor)6;

    private const int TOWN_INFO_X = 0;
    private const int TOWN_INFO_Y = 1;
    private const int SOLDIER_PIC_X = 9;
    private const int SOLDIER_PIC_Y = 20;

    // char stat positions
    private const int STR_X = 112, STR_Y = 42;
    private const int DEX_X = STR_X, DEX_Y = 32;
    private const int AGL_X = STR_X, AGL_Y = 22;
    private const int LDR_X = STR_X, LDR_Y = 52;
    private const int WIS_X = STR_X, WIS_Y = 62;
    private const int LVL_X = 159, LVL_Y = AGL_Y;
    private const int MRK_X = LVL_X, MRK_Y = DEX_Y;
    private const int EXP_X = LVL_X, EXP_Y = STR_Y;
    private const int MEC_X = LVL_X, MEC_Y = LDR_Y;
    private const int MED_X = LVL_X, MED_Y = WIS_Y;
    private const int STAT_WID = 15;

    private const int PIC_NAME_X = 8;
    private const int PIC_NAME_Y = 66 + 3;
    private const int PIC_NAME_WID = 60 - PIC_NAME_X;
    private const int PIC_NAME_HEI = 75 - PIC_NAME_Y;
    private const int CHAR_NAME_X = 14;
    private const int CHAR_NAME_Y = 2 + 3;
    private const int CHAR_NAME_WID = 164 - CHAR_NAME_X;
    private const int CHAR_NAME_HEI = 11 - CHAR_NAME_Y;
    private const int CHAR_TIME_REMAINING_X = 207;
    private const int CHAR_TIME_REMAINING_Y = 65;
    private const int CHAR_TIME_REMAINING_WID = 258 - CHAR_TIME_REMAINING_X;
    private const int CHAR_SALARY_Y = 79;
    private const int CHAR_SALARY_WID = CHAR_TIME_REMAINING_WID - 8;
    private const int CHAR_MEDICAL_Y = 93;
    private const int CHAR_ASSIGN_X = 182;
    private const int CHAR_ASSIGN1_Y = 18;
    private const int CHAR_ASSIGN2_Y = 31;
    private const int CHAR_ASSIGN_WID = 257 - 178;
    private const int CHAR_ASSIGN_HEI = 39 - 29;
    private const int CHAR_HP_X = 133;
    private const int CHAR_HP_Y = 77 + 3;
    private const int CHAR_HP_WID = 175 - CHAR_HP_X;
    private const int CHAR_HP_HEI = 90 - CHAR_HP_Y;
    private const int CHAR_MORALE_X = 133;
    private const int CHAR_MORALE_Y = 91 + 3;
    private const int CHAR_MORALE_WID = 175 - CHAR_MORALE_X;
    private const int CHAR_MORALE_HEI = 101 - CHAR_MORALE_Y;

    private static readonly string[] pShortAttributeStrings = ["Agi", "Dex", "Str", "Ldr", "Wis", "Lvl", "Mrk", "Exp", "Mec", "Med"];
    private static readonly string[] pUpperLeftMapScreenStrings = ["Assignment", "Contract", "Health", "Morale", "Cond.", "Fuel"];
    private static readonly string[] pMoralStrings = ["Great", "Good", "Stable", "Poor", "Panic", "Bad"];

    private static int CharFontHeight => FontSubSystem.GetFontHeight(CHAR_FONT);

    /// <summary>Draws the info panel for the selected character (RenderCharacterInfoBackground).</summary>
    private void DisplayCharacterInfoPanel()
    {
        // the upper left hand corner character info panel
        var panel = video.GetVideoObject(Utils.FilenameForBPP("INTERFACE\\charinfo.sti"));
        video.BltVideoObject(SurfaceType.FRAME_BUFFER, panel, 0, TOWN_INFO_X, TOWN_INFO_Y, VO_BLT.SRCTRANSPARENCY);

        if (bSelectedInfoChar >= 0 && bSelectedInfoChar < MAX_CHARACTER_COUNT && gCharactersList[bSelectedInfoChar].fValid)
        {
            DrawCharacterInfo(bSelectedInfoChar);
        }

        RenderAttributeStringsForUpperLeftHandCorner();
    }

    private void DrawCharacterInfo(int sCharNumber)
    {
        var pSoldier = MercPtrs[gCharactersList[sCharNumber].usSolID];
        if (pSoldier.ubProfile == NO_PROFILE)
        {
            return;
        }

        var profile = gMercProfiles[pSoldier.ubProfile];

        // the face
        DrawFace(pSoldier);

        FontSubSystem.SetFont(CHAR_FONT);
        FontSubSystem.SetFontForeground(CHAR_TEXT_FONT_COLOR);
        FontSubSystem.SetFontBackground(FontColor.FONT_BLACK);

        // Nickname (beneath Picture)
        DrawCentered(profile.zNickname, PIC_NAME_X, PIC_NAME_Y, PIC_NAME_WID, PIC_NAME_HEI);

        // Full name (Top Box)
        DrawCentered(profile.zName, CHAR_NAME_X, CHAR_NAME_Y, CHAR_NAME_WID, CHAR_NAME_HEI);

        // Assignment
        DrawCentered(GetMapscreenMercAssignmentString(pSoldier), CHAR_ASSIGN_X, CHAR_ASSIGN1_Y, CHAR_ASSIGN_WID, CHAR_ASSIGN_HEI);

        // second assignment line
        DrawCentered(GetSecondAssignmentLine(pSoldier), CHAR_ASSIGN_X, CHAR_ASSIGN2_Y, CHAR_ASSIGN_WID, CHAR_ASSIGN_HEI);

        DrawCharHealth(pSoldier);
        DrawCharBars(pSoldier);
        RenderIconsForUpperLeftCornerPiece(pSoldier);
        DrawCharStats(pSoldier);

        // remaining contract length
        string sString;
        FontColor contractColor = CHAR_TEXT_FONT_COLOR;
        if (pSoldier.bLife <= 0)
        {
            sString = "N/A";
        }
        else if (pSoldier.ubWhatKindOfMercAmI == MERC_TYPE.AIM_MERC || pSoldier.ubProfile == NPCID.SLAY)
        {
            int iTimeRemaining = pSoldier.iEndofContractTime - (int)GameClock.GetWorldTotalMin();

            // if the merc is in transit, the time left can't be more than the contract length
            if (pSoldier.bAssignment == Assignment.IN_TRANSIT && iTimeRemaining > pSoldier.iTotalContractLength * NUM_MIN_IN_DAY)
            {
                iTimeRemaining = pSoldier.iTotalContractLength * NUM_MIN_IN_DAY;
            }

            if (iTimeRemaining >= 24 * 60)
            {
                // more than a day, display in green
                contractColor = FontColor.FONT_LTGREEN;
                sString = $"{iTimeRemaining / (60 * 24.0):0.0}d/{pSoldier.iTotalContractLength}d";
            }
            else
            {
                // less than a day, display hours left in red
                contractColor = FontColor.FONT_RED;
                int hours = iTimeRemaining > 5 ? (iTimeRemaining + 59) / 60 : 0;
                sString = $"{hours}h/{pSoldier.iTotalContractLength}d";
            }
        }
        else if (pSoldier.ubWhatKindOfMercAmI == MERC_TYPE.MERC)
        {
            int iBeenHiredFor = (int)(GameClock.GetWorldTotalMin() / NUM_MIN_IN_DAY) - pSoldier.iStartContractTime;
            sString = $"{profile.iMercMercContractLength}d/{iBeenHiredFor}d";
        }
        else
        {
            sString = "N/A";
        }

        FontSubSystem.SetFontForeground(contractColor);
        DrawCentered(sString, CHAR_TIME_REMAINING_X, CHAR_TIME_REMAINING_Y, CHAR_TIME_REMAINING_WID, CharFontHeight);
        FontSubSystem.SetFontForeground(CHAR_TEXT_FONT_COLOR);

        // salary
        int iDailyCost;
        if (pSoldier.ubWhatKindOfMercAmI == MERC_TYPE.AIM_MERC && pSoldier.bTypeOfLastContract == 1)
        {
            iDailyCost = profile.uiWeeklySalary / 7;
        }
        else if (pSoldier.ubWhatKindOfMercAmI == MERC_TYPE.AIM_MERC && pSoldier.bTypeOfLastContract == 2)
        {
            iDailyCost = profile.uiBiWeeklySalary / 14;
        }
        else
        {
            iDailyCost = profile.sSalary;
        }

        DrawRight(FormatDollars(iDailyCost), CHAR_TIME_REMAINING_X, CHAR_SALARY_Y, CHAR_SALARY_WID, CharFontHeight);

        // medical deposit
        if (profile.sMedicalDepositAmount > 0)
        {
            DrawRight(FormatDollars(profile.sMedicalDepositAmount), CHAR_TIME_REMAINING_X, CHAR_MEDICAL_Y, CHAR_SALARY_WID, CharFontHeight);
        }

        // morale
        string morale;
        if (pSoldier.bAssignment == Assignment.ASSIGNMENT_POW)
        {
            // POW - morale unknown
            morale = pPOWStrings[1];
        }
        else if (pSoldier.bLife != 0)
        {
            morale = GetMoraleString(pSoldier);
        }
        else
        {
            morale = string.Empty;
        }

        DrawCentered(morale, CHAR_MORALE_X, CHAR_MORALE_Y, CHAR_MORALE_WID, CHAR_MORALE_HEI);
    }

    private static readonly string[] pAttributeMenuStrings =
        ["Strength", "Dexterity", "Agility", "Health", "Marksmanship", "Medical", "Mechanical", "Leadership", "Explosives", "Cancel"];

    private static readonly string[] pRepairStrings = ["Items", "SAM Site", "Cancel", "Robot"];

    /// <summary>What goes under the assignment: the stat being trained, the town, what is repaired, the ETA or the location.</summary>
    private static string GetSecondAssignmentLine(SOLDIERTYPE pSoldier)
    {
        switch (pSoldier.bAssignment)
        {
            case Assignment.TRAIN_SELF:
            case Assignment.TRAIN_TEAMMATE:
            case Assignment.TRAIN_BY_OTHER:
                return pSoldier.bTrainStat >= 0 && pSoldier.bTrainStat < pAttributeMenuStrings.Length
                    ? pAttributeMenuStrings[pSoldier.bTrainStat]
                    : string.Empty;

            case Assignment.TRAIN_TOWN:
                return pTownNames.TryGetValue(StrategicMap.GetTownIdForSector(pSoldier.sSectorX, pSoldier.sSectorY), out string? town) ? town : string.Empty;

            case Assignment.REPAIR:
                // TODO(port): the vehicle being repaired
                return pSoldier.fFixingRobot ? pRepairStrings[3] : pRepairStrings[0];

            case Assignment.IN_TRANSIT:
                // show ETA
                return ConvertMinTimeToETADayHourMinString(pSoldier.uiTimeSoldierWillArrive);

            default:
                // TODO(port): the ETA of a group that is travelling
                return GetMapscreenMercLocationString(pSoldier);
        }
    }

    private static string ConvertMinTimeToETADayHourMinString(int uiTimeInMin)
    {
        int uiDay = uiTimeInMin / NUM_MIN_IN_DAY;
        int uiHour = (uiTimeInMin - uiDay * NUM_MIN_IN_DAY) / 60;
        int uiMin = uiTimeInMin - (uiDay * NUM_MIN_IN_DAY + uiHour * 60);

        return $"ETA: {uiHour:00}:{uiMin:00}";
    }

    private static string FormatDollars(int amount) => "$" + amount.ToString("N0");

    private static string GetMoraleString(SOLDIERTYPE pSoldier)
    {
        int bMorale = pSoldier.bMorale;

        if (pSoldier.uiStatusFlags.HasFlag(SOLDIER.DEAD))
        {
            return pMoralStrings[5];
        }

        return bMorale switch
        {
            > 80 => pMoralStrings[0],
            > 65 => pMoralStrings[1],
            > 35 => pMoralStrings[2],
            > 20 => pMoralStrings[3],
            _ => pMoralStrings[4],
        };
    }

    /// <summary>The merc's still picture. TODO(port): the animated face with blinking and talking (DrawFace).</summary>
    private void DrawFace(SOLDIERTYPE pSoldier)
    {
        try
        {
            var face = video.GetVideoObject(Utils.FilenameForBPP(wprintf("FACES\\%02d.sti", (int)pSoldier.ubProfile)));
            video.BltVideoObject(SurfaceType.FRAME_BUFFER, face, 0, SOLDIER_PIC_X, SOLDIER_PIC_Y, VO_BLT.SRCTRANSPARENCY);
        }
        catch (Exception)
        {
            // no picture for this profile
        }
    }

    private void DrawCharHealth(SOLDIERTYPE pSoldier)
    {
        if (pSoldier.bAssignment == Assignment.ASSIGNMENT_POW)
        {
            // POW - health unknown
            FontSubSystem.SetFontForeground(CHAR_TEXT_FONT_COLOR);
            DrawCentered(pPOWStrings[1], CHAR_HP_X, CHAR_HP_Y, CHAR_HP_WID, CHAR_HP_HEI);
            return;
        }

        // find starting X coordinate by centering all 3 substrings together, then print them separately (different colors)
        FontSubSystem.FindFontCenterCoordinates(CHAR_HP_X, CHAR_HP_Y, CHAR_HP_WID, CHAR_HP_HEI, $"{pSoldier.bLife}/{pSoldier.bLifeMax}", CHAR_FONT, out int usX, out _);

        int uiHealthPercent = pSoldier.bLifeMax > 0 ? (pSoldier.bLife * 100) / pSoldier.bLifeMax : 0;

        FontSubSystem.SetFontForeground(uiHealthPercent == 0 ? FontColor.FONT_METALGRAY
            : uiHealthPercent < 25 ? FontColor.FONT_RED
            : uiHealthPercent < 50 ? FontColor.FONT_YELLOW
            : CHAR_TEXT_FONT_COLOR);

        string life = pSoldier.bLife.ToString();
        mprintf(usX, CHAR_HP_Y, life);
        usX += FontSubSystem.StringPixLength(life, CHAR_FONT);

        FontSubSystem.SetFontForeground(CHAR_TEXT_FONT_COLOR);
        mprintf(usX, CHAR_HP_Y, "/");
        usX += FontSubSystem.StringPixLength("/", CHAR_FONT);

        // TODO(port): green / red max life for a recent change (uiChangeHealthTime)
        mprintf(usX, CHAR_HP_Y, pSoldier.bLifeMax.ToString());
    }

    private void DrawCharStats(SOLDIERTYPE pSoldier)
    {
        FontSubSystem.SetFont(CHAR_FONT);
        FontSubSystem.SetFontForeground(CHAR_TEXT_FONT_COLOR);
        FontSubSystem.SetFontBackground(FontColor.FONT_BLACK);

        // TODO(port): green / red numbers for stats that changed recently (uiChange*Time / usValueGoneUp)
        DrawRight(pSoldier.bStrength.ToString(), STR_X, STR_Y, STAT_WID, CharFontHeight);
        DrawRight(pSoldier.bDexterity.ToString(), DEX_X, DEX_Y, STAT_WID, CharFontHeight);
        DrawRight(pSoldier.bAgility.ToString(), AGL_X, AGL_Y, STAT_WID, CharFontHeight);
        DrawRight(pSoldier.bWisdom.ToString(), WIS_X, WIS_Y, STAT_WID, CharFontHeight);
        DrawRight(pSoldier.bLeadership.ToString(), LDR_X, LDR_Y, STAT_WID, CharFontHeight);
        DrawRight(pSoldier.bExpLevel.ToString(), LVL_X, LVL_Y, STAT_WID, CharFontHeight);
        DrawRight(pSoldier.bMarksmanship.ToString(), MRK_X, MRK_Y, STAT_WID, CharFontHeight);
        DrawRight(pSoldier.bExplosive.ToString(), EXP_X, EXP_Y, STAT_WID, CharFontHeight);
        DrawRight(pSoldier.bMechanical.ToString(), MEC_X, MEC_Y, STAT_WID, CharFontHeight);
        DrawRight(pSoldier.bMedical.ToString(), MED_X, MED_Y, STAT_WID, CharFontHeight);
    }

    /// <summary>The titles of the panel: "Assignment", "Health", "Morale" and the attribute names.</summary>
    private void RenderAttributeStringsForUpperLeftHandCorner()
    {
        SOLDIERTYPE? pSoldier = null;
        if (bSelectedInfoChar >= 0 && bSelectedInfoChar < MAX_CHARACTER_COUNT && gCharactersList[bSelectedInfoChar].fValid)
        {
            pSoldier = MercPtrs[gCharactersList[bSelectedInfoChar].usSolID];
        }

        FontSubSystem.SetFont(CHAR_FONT);
        FontSubSystem.SetFontForeground(CHAR_TITLE_FONT_COLOR);
        FontSubSystem.SetFontBackground(FontColor.FONT_BLACK);

        // assignment strings
        mprintf(220 - FontSubSystem.StringPixLength(pUpperLeftMapScreenStrings[0], CHAR_FONT) / 2, 6, pUpperLeftMapScreenStrings[0]);

        // TODO(port): vehicles and the robot show "Cond." instead of the attributes
        if (pSoldier is null || !pSoldier.uiStatusFlags.HasFlag(SOLDIER.VEHICLE))
        {
            // health
            mprintf(87, 80, pUpperLeftMapScreenStrings[2]);

            for (int iCounter = 0; iCounter < 5; iCounter++)
            {
                mprintf(88, 22 + iCounter * 10, pShortAttributeStrings[iCounter]);
                mprintf(133, 22 + iCounter * 10, pShortAttributeStrings[iCounter + 5]);
            }

            // morale
            mprintf(87, 94, pUpperLeftMapScreenStrings[3]);
        }
        else
        {
            mprintf(96, 80, pUpperLeftMapScreenStrings[4]);
        }
    }

    private static void DrawCentered(string text, int x, int y, int width, int height)
    {
        if (text.Length == 0)
        {
            return;
        }

        FontSubSystem.FindFontCenterCoordinates(x, y, width, height, text, CHAR_FONT, out int usX, out int usY);
        mprintf(usX, usY, text);
    }

    private static void DrawRight(string text, int x, int y, int width, int height)
    {
        FontSubSystem.FindFontRightCoordinates(x, y, width, height, text, CHAR_FONT, out int usX, out _);
        mprintf(usX, y, text);
    }
}
