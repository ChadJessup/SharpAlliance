﻿using System;
using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core.SubSystems;

public class NPC
{
    public static void TriggerNPCRecord(NPCID ubTriggerNPC, int ubTriggerNPCRec)
    {
        // Check if we have a quote to trigger...
        NPCQuoteInfo? pQuotePtr;
        bool fDisplayDialogue = true;

        if (EnsureQuoteFileLoaded(ubTriggerNPC) == false)
        {
            // error!!!
            return;
        }
        pQuotePtr = (gpNPCQuoteInfoArray[ubTriggerNPC][ubTriggerNPCRec]);
        if (pQuotePtr.ubQuoteNum == IRRELEVANT)
        {
            fDisplayDialogue = false;
        }

        if (NPCConsiderQuote(ubTriggerNPC, 0, TRIGGER_NPC, ubTriggerNPCRec, 0, gpNPCQuoteInfoArray[ubTriggerNPC]))
        {
            NPCTriggerNPC(ubTriggerNPC, ubTriggerNPCRec, TRIGGER_NPC, fDisplayDialogue);
        }
        else
        {
            // don't do anything
            // DebugMsg(TOPIC_JA2, DBG_LEVEL_3, String("WARNING: trigger of %d, record %d cannot proceed, possible error", ubTriggerNPC, ubTriggerNPCRec));
        }
    }

    internal static void TriggerNPCRecordImmediately(NPCID ubNPCNumber, int usTriggerEvent)
    {
        throw new NotImplementedException();
    }

    public bool ReloadAllQuoteFiles()
    {
        return true;
    }
}

public enum Approaches
{
    APPROACH_FRIENDLY = 1,
    APPROACH_DIRECT,
    APPROACH_THREATEN,
    APPROACH_RECRUIT,
    APPROACH_REPEAT,

    APPROACH_GIVINGITEM,
    NPC_INITIATING_CONV,
    NPC_INITIAL_QUOTE,
    NPC_WHOAREYOU,
    TRIGGER_NPC,

    APPROACH_GIVEFIRSTAID,
    APPROACH_SPECIAL_INITIAL_QUOTE,
    APPROACH_ENEMY_NPC_QUOTE,
    APPROACH_DECLARATION_OF_HOSTILITY,
    APPROACH_EPC_IN_WRONG_SECTOR,

    APPROACH_EPC_WHO_IS_RECRUITED,
    APPROACH_INITIAL_QUOTE,
    APPROACH_CLOSING_SHOP,
    APPROACH_SECTOR_NOT_SAFE,
    APPROACH_DONE_SLAPPED,  // 20

    APPROACH_DONE_PUNCH_0,
    APPROACH_DONE_PUNCH_1,
    APPROACH_DONE_PUNCH_2,
    APPROACH_DONE_OPEN_STRUCTURE,
    APPROACH_DONE_GET_ITEM,                 // 25

    APPROACH_DONE_GIVING_ITEM,
    APPROACH_DONE_TRAVERSAL,
    APPROACH_BUYSELL,
    APPROACH_ONE_OF_FOUR_STANDARD,
    APPROACH_FRIENDLY_DIRECT_OR_RECRUIT,	// 30
}
