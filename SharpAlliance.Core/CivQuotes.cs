using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpAlliance.Core;
using SharpAlliance.Core.Managers;
using SharpAlliance.Core.Screens;
using SharpAlliance.Core.SubSystems;
using SixLabors.ImageSharp;
using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core;

public partial class Globals
{
    public const int DIALOGUE_DEFAULT_WIDTH = 200;
    public const int EXTREAMLY_LOW_TOWN_LOYALTY = 20;
    public const int HIGH_TOWN_LOYALTY = 80;

    public static bool gfSurrendered = false;

    public static Dictionary<CIV_QUOTE, CIV_QUOTEStruct> gCivQuotes = new();// CIV_QUOTEStruct[(int)CIV_QUOTE.NUM_CIV_QUOTES];

    public static string sprintf(string format, params object?[] args) => string.Format(format, args);

    public static int[] gubNumEntries =
    {
        15,
        15,
        15,
        15,
        15,
        15,
        15,
        15,
        15,
        15,

        15,
        15,
        15,
        15,
        15,
        15,
        15,
        15,
        15,
        15,

        5,
        5,
        15,
        15,
        15,
        15,
        15,
        15,
        15,
        15,

        15,
        15,
        2,
        15,
        15,
        10,
        10,
        5,
        3,
        10,

        3,
        3,
        3,
        3,
        3,
        3,
        3,
        3,
        3,
        3
    };

    public static QUOTE_SYSTEM_STRUCT gCivQuoteData;

    public static int[] gzCivQuote = new int[320];
    public static int gusCivQuoteBoxWidth;
    public static int gusCivQuoteBoxHeight;
}

public struct QUOTE_SYSTEM_STRUCT
{
    public bool bActive;
    public MOUSE_REGION MouseRegion;
    public int iVideoOverlay;
    public int iDialogueBox;
    public uint uiTimeOfCreation;
    public int uiDelayTime;
    public SOLDIERTYPE pCiv;
}


public class CivQuotes
{

    void CopyNumEntriesIntoQuoteStruct()
    {
        CIV_QUOTE cnt;

        for (cnt = 0; cnt < CIV_QUOTE.NUM_CIV_QUOTES; cnt++)
        {
            //gCivQuotes[cnt].ubNumEntries = gubNumEntries[(int)cnt];
        }

    }


    public static bool GetCivQuoteText(CIV_QUOTE ubCivQuoteID, int ubEntryID, out string zQuote)
    {
        zQuote = string.Empty;
        string zFileName;// [164];

        // Build filename....
        if (ubCivQuoteID == CIV_QUOTE.HINT)
        {
            if (gbWorldSectorZ > 0)
            {
                //sprintf( zFileName, "NPCData\\miners.edt" );
                zFileName = sprintf("NPCDATA\\CIV%02d.edt", CIV_QUOTE.MINERS_NOT_FOR_PLAYER);
            }
            else
            {
                zFileName = sprintf("NPCData\\%c%d.edt", 'A' + ((int)gWorldSectorY - 1), gWorldSectorX);
            }
        }
        else
        {
            zFileName = sprintf("NPCDATA\\CIV%02d.edt", ubCivQuoteID);
        }

//        CHECKF(FileExists(zFileName));

        // Get data...
//        LoadEncryptedDataFromFile(zFileName, zQuote, ubEntryID * 320, 320);

        if (zQuote == string.Empty)
        {
            return (false);
        }

        return (true);
    }

    public static void SurrenderMessageBoxCallBack(MessageBoxReturnCode ubExitValue)
    {
        int cnt = 0;

        if (ubExitValue == MessageBoxReturnCode.MSG_BOX_RETURN_YES)
        {
            // CJC Dec 1 2002: fix multiple captures
//            BeginCaptureSquence();

            // Do capture....
            cnt = gTacticalStatus.Team[gbPlayerNum].bFirstID;

            //for (pTeamSoldier = MercPtrs[cnt]; cnt <= gTacticalStatus.Team[gbPlayerNum].bLastID; cnt++, pTeamSoldier++)
            foreach (var pTeamSoldier in MercPtrs)
            {
                // Are we active and in sector.....
                if (pTeamSoldier.bActive && pTeamSoldier.bInSector)
                {
                    if (pTeamSoldier.bLife != 0)
                    {
//                        EnemyCapturesPlayerSoldier(pTeamSoldier);
//
//                        RemoveSoldierFromTacticalSector(pTeamSoldier, true);
                    }
                }
            }

//            EndCaptureSequence();
//
//            gfSurrendered = true;
//            SetCustomizableTimerCallbackAndDelay(3000, CaptureTimerCallback, false);

            AIMain.ActionDone(gCivQuoteData.pCiv);
        }
        else
        {
            AIMain.ActionDone(gCivQuoteData.pCiv);
        }
    }

    public static void ShutDownQuoteBox(bool fForce)
    {
        if (!gCivQuoteData.bActive)
        {
            return;
        }

        // Check for min time....
        if ((GetJA2Clock() - gCivQuoteData.uiTimeOfCreation) > 300 || fForce)
        {
            RenderDirty.RemoveVideoOverlay(gCivQuoteData.iVideoOverlay);

            // Remove mouse region...
            MouseSubSystem.MSYS_RemoveRegion((gCivQuoteData.MouseRegion));

            MercTextBox.RemoveMercPopupBoxFromIndex(gCivQuoteData.iDialogueBox);
            gCivQuoteData.iDialogueBox = -1;

            gCivQuoteData.bActive = false;

            // do we need to do anything at the end of the civ quote?
            if (gCivQuoteData.pCiv is not null && gCivQuoteData.pCiv.bAction == AI_ACTION.OFFER_SURRENDER)
            {
                Rectangle? _ = null;
                MessageBoxSubSystem.DoMessageBox(
                    MessageBoxStyle.MSG_BOX_BASIC_STYLE,
                    "",//Message[STR_SURRENDER],
                    ScreenName.GAME_SCREEN,
                    MSG_BOX_FLAG.YESNO,
                    SurrenderMessageBoxCallBack,
                    ref _);
            }
        }
    }

    bool ShutDownQuoteBoxIfActive()
    {
        if (gCivQuoteData.bActive)
        {
            ShutDownQuoteBox(true);

            return (true);
        }

        return (false);
    }

    public static CIV_TYPE GetCivType(SOLDIERTYPE pCiv)
    {
        if (pCiv.ubProfile != NO_PROFILE)
        {
            return (CIV_TYPE.NA);
        }

        // ATE: Check if this person is married.....
        // 1 ) check sector....
        if (gWorldSectorX == 10 && gWorldSectorY == (MAP_ROW)6 && gbWorldSectorZ == 0)
        {
            // 2 ) the only female....
            if (pCiv.ubCivilianGroup == 0 && pCiv.bTeam != gbPlayerNum && pCiv.ubBodyType == SoldierBodyTypes.REGFEMALE)
            {
                // She's a ho!
                return (CIV_TYPE.MARRIED_PC);
            }
        }

        // OK, look for enemy type - MUST be on enemy team, merc bodytype
        if (pCiv.bTeam == ENEMY_TEAM && IS_MERC_BODY_TYPE(pCiv))
        {
            return (CIV_TYPE.ENEMY);
        }

        if (pCiv.bTeam != CIV_TEAM && pCiv.bTeam != MILITIA_TEAM)
        {
            return (CIV_TYPE.NA);
        }

        switch (pCiv.ubBodyType)
        {
            case SoldierBodyTypes.REGMALE:
            case SoldierBodyTypes.BIGMALE:
            case SoldierBodyTypes.STOCKYMALE:
            case SoldierBodyTypes.REGFEMALE:
            case SoldierBodyTypes.FATCIV:
            case SoldierBodyTypes.MANCIV:
            case SoldierBodyTypes.MINICIV:
            case SoldierBodyTypes.DRESSCIV:
            case SoldierBodyTypes.CRIPPLECIV:
                return (CIV_TYPE.ADULT);
            case SoldierBodyTypes.ADULTFEMALEMONSTER:
            case SoldierBodyTypes.AM_MONSTER:
            case SoldierBodyTypes.YAF_MONSTER:
            case SoldierBodyTypes.YAM_MONSTER:
            case SoldierBodyTypes.LARVAE_MONSTER:
            case SoldierBodyTypes.INFANT_MONSTER:
            case SoldierBodyTypes.QUEENMONSTER:

                return (CIV_TYPE.NA);

            case SoldierBodyTypes.HATKIDCIV:
            case SoldierBodyTypes.KIDCIV:

                return (CIV_TYPE.KID);

            default:

                return (CIV_TYPE.NA);
        }

        return (CIV_TYPE.NA);
    }


    public static void RenderCivQuoteBoxOverlay(VIDEO_OVERLAY pBlitter)
    {
        if (gCivQuoteData.iVideoOverlay != -1)
        {
            MercTextBox.RenderMercPopUpBoxFromIndex(gCivQuoteData.iDialogueBox, pBlitter.sX, pBlitter.sY, pBlitter.uiDestBuff);

            VeldridVideoManager.InvalidateRegion(pBlitter.sX, pBlitter.sY, pBlitter.sX + gusCivQuoteBoxWidth, pBlitter.sY + gusCivQuoteBoxHeight);
        }
    }


    static bool fLButtonDown = false;
    void QuoteOverlayClickCallback(MOUSE_REGION pRegion, MSYS_CALLBACK_REASON iReason)
    {

        if (iReason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_DWN))
        {
            fLButtonDown = true;
        }

        if (iReason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP) && fLButtonDown)
        {
            // Shutdown quote box....
            ShutDownQuoteBox(false);
        }
        else if (iReason.HasFlag(MSYS_CALLBACK_REASON.LOST_MOUSE))
        {
            fLButtonDown = false;
        }
    }


    public static void BeginCivQuote(SOLDIERTYPE pCiv, CIV_QUOTE ubCivQuoteID, int ubEntryID, int sX, int sY)
    {
        VIDEO_OVERLAY_DESC VideoOverlayDesc = new();
        string zQuote;// [320];

        // OK, do we have another on?
        if (gCivQuoteData.bActive)
        {
            // Delete?
            ShutDownQuoteBox(true);
        }

        // get text
        if (!GetCivQuoteText(ubCivQuoteID, ubEntryID, out zQuote))
        {
            return;
        }

# if TAIWANESE
        wprintf(gzCivQuote, L"%s", zQuote);
#else
//        wprintf(gzCivQuote, "\"%s\"", zQuote);
#endif


        if (ubCivQuoteID == CIV_QUOTE.HINT)
        {
//            MapScreenMessage(FONT_MCOLOR_WHITE, MSG_DIALOG, "%s", gzCivQuote);
        }

        // Create video oeverlay....
//        memset(&VideoOverlayDesc, 0, sizeof(VIDEO_OVERLAY_DESC));

        // Prepare text box
//        SET_USE_WINFONTS(true);
//        SET_WINFONT(giSubTitleWinFont);
//        gCivQuoteData.iDialogueBox = PrepareMercPopupBox(gCivQuoteData.iDialogueBox, BASIC_MERC_POPUP_BACKGROUND, BASIC_MERC_POPUP_BORDER, gzCivQuote, DIALOGUE_DEFAULT_WIDTH, 0, 0, 0, gusCivQuoteBoxWidth, gusCivQuoteBoxHeight);
//        SET_USE_WINFONTS(false);

        // OK, find center for box......
        sX = sX - (gusCivQuoteBoxWidth / 2);
        sY = sY - (gusCivQuoteBoxHeight / 2);

        // OK, limit to screen......
        {
            if (sX < 0)
            {
                sX = 0;
            }

            // CHECK FOR LEFT/RIGHT
            if ((sX + gusCivQuoteBoxWidth) > 640)
            {
                sX = 640 - gusCivQuoteBoxWidth;
            }

            // Now check for top
            if (sY < gsVIEWPORT_WINDOW_START_Y)
            {
                sY = gsVIEWPORT_WINDOW_START_Y;
            }

            // Check for bottom
            if ((sY + gusCivQuoteBoxHeight) > 340)
            {
                sY = 340 - gusCivQuoteBoxHeight;
            }
        }

        VideoOverlayDesc.sLeft = sX;
        VideoOverlayDesc.sTop = sY;
        VideoOverlayDesc.sRight = VideoOverlayDesc.sLeft + gusCivQuoteBoxWidth;
        VideoOverlayDesc.sBottom = VideoOverlayDesc.sTop + gusCivQuoteBoxHeight;
        VideoOverlayDesc.sX = VideoOverlayDesc.sLeft;
        VideoOverlayDesc.sY = VideoOverlayDesc.sTop;
        VideoOverlayDesc.BltCallback = RenderCivQuoteBoxOverlay;

//        gCivQuoteData.iVideoOverlay = RegisterVideoOverlay(0, &VideoOverlayDesc);


        //Define main region
//        MSYS_DefineRegion((gCivQuoteData.MouseRegion), VideoOverlayDesc.sLeft, VideoOverlayDesc.sTop, VideoOverlayDesc.sRight, VideoOverlayDesc.sBottom, MSYS_PRIORITY_HIGHEST,
//                             CURSOR_NORMAL, MSYS_NO_CALLBACK, QuoteOverlayClickCallback);
        // Add region
//        MSYS_AddRegion((gCivQuoteData.MouseRegion));


        gCivQuoteData.bActive = true;

//        gCivQuoteData.uiTimeOfCreation = GetJA2Clock();

//        gCivQuoteData.uiDelayTime = FindDelayForString(gzCivQuote) + 500;

        gCivQuoteData.pCiv = pCiv;

    }

    public static CIV_QUOTE DetermineCivQuoteEntry(SOLDIERTYPE pCiv, ref int pubCivHintToUse, bool fCanUseHints)
    {
        CIV_TYPE ubCivType;
        int bTownId;
        bool bCivLowLoyalty = false;
        bool bCivHighLoyalty = false;
        CIV_TYPE bCivHint;
        int bMineId;
        bool bMiners = false;

        (pubCivHintToUse) = 0;

        ubCivType = GetCivType(pCiv);

        if (ubCivType == CIV_TYPE.ENEMY)
        {
            // Determine what type of quote to say...
            // Are are we going to attack?

//            if (pCiv.bAction == AI_ACTION_TOSS_PROJECTILE || pCiv.bAction == AI_ACTION_FIRE_GUN ||
//                                pCiv.bAction == AI_ACTION_FIRE_GUN || pCiv.bAction == AI_ACTION_KNIFE_MOVE)
//            {
//                return (CIV_QUOTE_ENEMY_THREAT);
//            }
//            else if (pCiv.bAction == AI_ACTION_OFFER_SURRENDER)
//            {
//                return (CIV_QUOTE_ENEMY_OFFER_SURRENDER);
//            }
//            // Hurt?
//            else if (pCiv.bLife < 30)
//            {
//                return (CIV_QUOTE_ENEMY_HURT);
//            }
//            // elite?
//            else if (pCiv.ubSoldierClass == SOLDIER_CLASS_ELITE)
//            {
//                return (CIV_QUOTE_ENEMY_ELITE);
//            }
//            else
//            {
//                return (CIV_QUOTE_ENEMY_ADMIN);
//            }
        }

        // Are we in a town sector?
        // get town id
        // bTownId = GetTownIdForSector(gWorldSectorX, gWorldSectorY);


        // If a married PC...
        if (ubCivType == CIV_TYPE.MARRIED_PC)
        {
            return (CIV_QUOTE.PC_MARRIED);
        }

        // CIV GROUPS FIRST!
        // Hicks.....
        if (pCiv.ubCivilianGroup == CIV_GROUP.HICKS_CIV_GROUP)
        {
            // Are they friendly?
            //if ( gTacticalStatus.fCivGroupHostile[ HICKS_CIV_GROUP ] < CIV_GROUP_WILL_BECOME_HOSTILE )
            if (pCiv.IsNeutral)
            {
                return (CIV_QUOTE.HICKS_FRIENDLY);
            }
            else
            {
                return (CIV_QUOTE.HICKS_ENEMIES);
            }
        }

        // Goons.....
        if (pCiv.ubCivilianGroup == CIV_GROUP.KINGPIN_CIV_GROUP)
        {
            // Are they friendly?
            //if ( gTacticalStatus.fCivGroupHostile[ KINGPIN_CIV_GROUP ] < CIV_GROUP_WILL_BECOME_HOSTILE )
            if (pCiv.IsNeutral)
            {
                return (CIV_QUOTE.GOONS_FRIENDLY);
            }
            else
            {
                return (CIV_QUOTE.GOONS_ENEMIES);
            }
        }

        // ATE: Cowering people take precedence....
        //  if ((pCiv.uiStatusFlags & SOLDIER.COWERING) || (pCiv.bTeam == CIV_TEAM && (gTacticalStatus.uiFlags & INCOMBAT)))
        {
            // if (ubCivType == CIV_TYPE_ADULT)
            {
                return (CIV_QUOTE.ADULTS_COWER);
            }
            //     else
            {
                return (CIV_QUOTE.KIDS_COWER);
            }
        }

        // Kid slaves...
        //   if (pCiv.ubCivilianGroup == FACTORY_KIDS_GROUP)
        //   {
        //       // Check fact.....
        //       if (CheckFact(FACT_DOREEN_HAD_CHANGE_OF_HEART, 0) || !CheckFact(FACT_DOREEN_ALIVE, 0))
        //       {
        //           return (CIV_QUOTE_KID_SLAVES_FREE);
        //       }
        //       else
        //       {
        //           return (CIV_QUOTE_KID_SLAVES);
        //       }
        //   }

        // BEGGERS
        //        if (pCiv.ubCivilianGroup == BEGGARS_CIV_GROUP)
        //        {
        //            // Check if we are in a town...
        //            if (bTownId != BLANK_SECTOR && gbWorldSectorZ == 0)
        //            {
        //                if (bTownId == SAN_MONA && ubCivType == CIV_TYPE_ADULT)
        //                {
        //                    return (CIV_QUOTE_SAN_MONA_BEGGERS);
        //                }
        //            }
        //
        //            // DO normal beggers...
        //            if (ubCivType == CIV_TYPE_ADULT)
        //            {
        //                return (CIV_QUOTE_ADULTS_BEGGING);
        //            }
        //            else
        //            {
        //                return (CIV_QUOTE_KIDS_BEGGING);
        //            }
        //        }

        // REBELS
        //        if (pCiv.ubCivilianGroup == REBEL_CIV_GROUP)
        //        {
        //            // DO normal beggers...
        //            if (ubCivType == CIV_TYPE_ADULT)
        //            {
        //                return (CIV_QUOTE_ADULTS_REBELS);
        //            }
        //            else
        //            {
        //                return (CIV_QUOTE_KIDS_REBELS);
        //            }
        //        }

        // Do miltitia...
        if (pCiv.bTeam == MILITIA_TEAM)
        {
            // Different types....
            //            if (pCiv.ubSoldierClass == SOLDIER_CLASS_GREEN_MILITIA)
            //            {
            //                return (CIV_QUOTE_GREEN_MILITIA);
            //            }
            //            if (pCiv.ubSoldierClass == SOLDIER_CLASS_REG_MILITIA)
            //            {
            //                return (CIV_QUOTE_MEDIUM_MILITIA);
            //            }
            //            if (pCiv.ubSoldierClass == SOLDIER_CLASS_ELITE_MILITIA)
            //            {
            //                return (CIV_QUOTE_ELITE_MILITIA);
            //            }
        }

        // If we are in medunna, and queen is dead, use these...
        //        if (bTownId == MEDUNA && CheckFact(FACT_QUEEN_DEAD, 0))
        //        {
        //            return (CIV_QUOTE_DEIDRANNA_DEAD);
        //        }

        // if in a town
        //        if ((bTownId != BLANK_SECTOR) && (gbWorldSectorZ == 0) && gfTownUsesLoyalty[bTownId])
        //        {
        //            // Check loyalty special quotes.....
        //            // EXTREMELY LOW TOWN LOYALTY...
        //            if (gTownLoyalty[bTownId].ubRating < EXTREAMLY_LOW_TOWN_LOYALTY)
        //            {
        //                bCivLowLoyalty = true;
        //            }
        //
        //            // HIGH TOWN LOYALTY...
        //            if (gTownLoyalty[bTownId].ubRating >= HIGH_TOWN_LOYALTY)
        //            {
        //                bCivHighLoyalty = true;
        //            }
        //        }


        // ATE: OK, check if we should look for a civ hint....
        if (fCanUseHints)
        {
            //            bCivHint = ConsiderCivilianQuotes(gWorldSectorX, gWorldSectorY, gbWorldSectorZ, false);
        }
        else
        {
            //            bCivHint = -1;
        }

        // ATE: check miners......
        //        if (pCiv.ubSoldierClass == SOLDIER_CLASS_MINER)
        {
            bMiners = true;

            // If not a civ hint available...
            //            if (bCivHint == -1)
            {
                // Check if they are under our control...

                // Should I go talk to miner?
                // Not done yet.

                // Are they working for us?
                //                bMineId = GetIdOfMineForSector(gWorldSectorX, gWorldSectorY, gbWorldSectorZ);
                //
                //                if (PlayerControlsMine(bMineId))
                //                {
                //                    return (CIV_QUOTE_MINERS_FOR_PLAYER);
                //                }
                //                else
                //                {
                //                    return (CIV_QUOTE_MINERS_NOT_FOR_PLAYER);
                //                }
                //            }
            }


            // Is one availible?
            // If we are to say low loyalty, do chance
            //        if (bCivHint != -1 && bCivLowLoyalty && !bMiners)
            //        {
            //            if (Random(100) < 25)
            //            {
            //                // Get rid of hint...
            //                bCivHint = -1;
            //            }
            //        }

            // Say hint if availible...
            //        if (bCivHint != -1)
            //        {
            //            if (ubCivType == CIV_TYPE_ADULT)
            //            {
            //                (*pubCivHintToUse) = bCivHint;
            //
            //                // Set quote as used...
            //                ConsiderCivilianQuotes(gWorldSectorX, gWorldSectorY, gbWorldSectorZ, true);
            //
            //                // retrun value....
            //                return (CIV_QUOTE_HINT);
            //            }
            //        }
            //
            //        if (bCivLowLoyalty)
            //        {
            //            if (ubCivType == CIV_TYPE_ADULT)
            //            {
            //                return (CIV_QUOTE_ADULTS_EXTREMLY_LOW_LOYALTY);
            //            }
            //            else
            //            {
            //                return (CIV_QUOTE_KIDS_EXTREMLY_LOW_LOYALTY);
            //            }
            //        }
            //
            //        if (bCivHighLoyalty)
            //        {
            //            if (ubCivType == CIV_TYPE_ADULT)
            //            {
            //                return (CIV_QUOTE_ADULTS_HIGH_LOYALTY);
            //            }
            //            else
            //            {
            //                return (CIV_QUOTE_KIDS_HIGH_LOYALTY);
            //            }
            //        }
            //
            //
            //        // All purpose quote here....
            //        if (ubCivType == CIV_TYPE_ADULT)
            //        {
            //            return (CIV_QUOTE_ADULTS_ALL_PURPOSE);
            //        }
            //        else
            //        {
            //            return (CIV_QUOTE_KIDS_ALL_PURPOSE);
            //        }
            //
        }
    }


    void HandleCivQuote()
    {
        if (gCivQuoteData.bActive)
        {
            // Check for min time....
            if ((GetJA2Clock() - gCivQuoteData.uiTimeOfCreation) > gCivQuoteData.uiDelayTime)
            {
                // Stop!
                ShutDownQuoteBox(true);
            }
        }
    }

    public static void StartCivQuote(SOLDIERTYPE pCiv)
    {
        CIV_QUOTE ubCivQuoteID;
        int sX = 0, sY = 0;
        int ubEntryID = 0;
        int sScreenX, sScreenY = 0;
        int ubCivHintToUse = 0;

        // ATE: Check for old quote.....
        // This could have been stored on last attempt...
        if (pCiv.bCurrentCivQuote == CIV_QUOTE.HINT)
        {
            // Determine which quote to say.....
            // CAN'T USE HINTS, since we just did one...
            pCiv.bCurrentCivQuote = (CIV_QUOTE)(-1);
            pCiv.bCurrentCivQuoteDelta = 0;
            ubCivQuoteID = DetermineCivQuoteEntry(pCiv, ref ubCivHintToUse, false);
        }
        else
        {
            // Determine which quote to say.....
            ubCivQuoteID = DetermineCivQuoteEntry(pCiv, ref ubCivHintToUse, true);
        }

        // Determine entry id
        // ATE: Try and get entry from soldier pointer....
        if (ubCivQuoteID != CIV_QUOTE.HINT)
        {
            if (pCiv.bCurrentCivQuote == CIV_QUOTE.UNSET)
            {
                // Pick random one
                //                pCiv.bCurrentCivQuote = (int)Random(gCivQuotes[ubCivQuoteID].ubNumEntries - 2);
                pCiv.bCurrentCivQuoteDelta = 0;
            }

           // ubEntryID = pCiv.bCurrentCivQuote + pCiv.bCurrentCivQuoteDelta;
        }
        else
        {
            ubEntryID = ubCivHintToUse;

            // ATE: set value for quote ID.....
            pCiv.bCurrentCivQuote = ubCivQuoteID;
            pCiv.bCurrentCivQuoteDelta = ubEntryID;

        }

        // Determine location...
        // Get location of civ on screen.....
        //        GetSoldierScreenPos(pCiv, &sScreenX, &sScreenY);
//sX = sScreenX;
//sY = sScreenY;

        // begin quote
        BeginCivQuote(pCiv, ubCivQuoteID, ubEntryID, sX, sY);

        // Increment use
        if (ubCivQuoteID != CIV_QUOTE.HINT)
        {
            pCiv.bCurrentCivQuoteDelta++;

            if (pCiv.bCurrentCivQuoteDelta == 2)
            {
                pCiv.bCurrentCivQuoteDelta = 0;
            }
        }
    }


    void InitCivQuoteSystem()
    {
//        memset(&gCivQuotes, 0, sizeof(gCivQuotes));
        CopyNumEntriesIntoQuoteStruct();

//        memset(&gCivQuoteData, 0, sizeof(gCivQuoteData));
        gCivQuoteData.bActive = false;
        gCivQuoteData.iVideoOverlay = -1;
        gCivQuoteData.iDialogueBox = -1;
    }


    bool SaveCivQuotesToSaveGameFile(Stream hFile)
    {
        int uiNumBytesWritten;

//        FileWrite(hFile, gCivQuotes, sizeof(gCivQuotes), out uiNumBytesWritten);
//        if (uiNumBytesWritten != sizeof(gCivQuotes))
//        {
//            return (false);
//        }

        return (true);
    }


    bool LoadCivQuotesFromLoadGameFile(Stream hFile)
    {
        int uiNumBytesRead;

//        FileRead(hFile, gCivQuotes, sizeof(gCivQuotes), out uiNumBytesRead);
//        if (uiNumBytesRead != sizeof(gCivQuotes))
//        {
//            return (false);
//        }

        CopyNumEntriesIntoQuoteStruct();

        return (true);
    }
}

public struct CIV_QUOTEStruct
{
    public byte ubNumEntries;
    public byte ubUnusedCurrentEntry;
}

public enum CIV_QUOTE
{
    ADULTS_BEGGING,
    KIDS_BEGGING,
    ADULTS_RECENT_BUG_ATTACK,
    KIDS_RECENT_BUG_ATTACK,
    ADULTS_BUG_EXTERMINATED_X_TIME,
    KIDS_BUG_EXTERMINATED_X_TIME,
    ADULTS_EXTREMLY_LOW_LOYALTY,
    KIDS_EXTREMLY_LOW_LOYALTY,
    ADULTS_HIGH_LOYALTY,
    KIDS_HIGH_LOYALTY,
    ADULTS_ALL_PURPOSE,
    KIDS_ALL_PURPOSE,
    ADULTS_LIBREATED_FIRST_TIME,
    KIDS_LIBREATED_FIRST_TIME,
    ADULTS_TOWN_TAKEN_BACK,
    KIDS_TOWN_TAKEN_BACK,
    HICKS_FRIENDLY,
    HICKS_ENEMIES,
    GOONS_FRIENDLY,
    GOONS_ENEMIES,
    ADULTS_REBELS,
    KIDS_REBELS,
    GREEN_MILITIA,
    MEDIUM_MILITIA,
    ELITE_MILITIA,
    SAN_MONA_BEGGERS,
    ENEMY_HURT,
    ENEMY_ADMIN,
    ENEMY_THREAT,
    ENEMY_ELITE,

    ADULTS_COWER,
    KIDS_COWER,
    PC_MARRIED,
    KID_SLAVES,
    KID_SLAVES_FREE,
    MINERS_NOT_FOR_PLAYER,
    MINERS_FOR_PLAYER,
    ENEMY_OFFER_SURRENDER,
    HICKS_SEE_US_AT_NIGHT,
    DEIDRANNA_DEAD,

    CIV_QUOTE_40,
    CIV_QUOTE_41,
    CIV_QUOTE_42,
    CIV_QUOTE_43,
    CIV_QUOTE_44,
    CIV_QUOTE_45,
    CIV_QUOTE_46,
    CIV_QUOTE_47,
    CIV_QUOTE_48,
    CIV_QUOTE_49,

    NUM_CIV_QUOTES,

    HINT = 99,
    UNSET = -1,
};


public enum CIV_TYPE
{
    NA = 0,
    ADULT = 1,
    KID = 2,
    MARRIED_PC = 3,
    ENEMY = 4,
}
