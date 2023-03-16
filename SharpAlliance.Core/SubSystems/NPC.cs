using System;
using System.Runtime.InteropServices;
using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core.SubSystems;

public class NPC
{
    public static void ReplaceLocationInNPCDataFromProfileID(NPCID ubNPC, int sOldGridNo, int sNewGridNo)
    {
        if (EnsureQuoteFileLoaded(ubNPC) == false)
        {
            // error!!!
            return;
        }

        var pNPCQuoteInfoArray = gpNPCQuoteInfoArray[ubNPC];

        ReplaceLocationInNPCData(pNPCQuoteInfoArray, sOldGridNo, sNewGridNo);
    }

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

public enum APPROACH
{
    FRIENDLY = 1,
    DIRECT,
    THREATEN,
    RECRUIT,
    REPEAT,

    GIVINGITEM,
    NPC_INITIATING_CONV,
    NPC_INITIAL_QUOTE,
    NPC_WHOAREYOU,
    TRIGGER_NPC,

    GIVEFIRSTAID,
    SPECIAL_INITIAL_QUOTE,
    ENEMY_NPC_QUOTE,
    DECLARATION_OF_HOSTILITY,
    EPC_IN_WRONG_SECTOR,

    EPC_WHO_IS_RECRUITED,
    INITIAL_QUOTE,
    CLOSING_SHOP,
    SECTOR_NOT_SAFE,
    DONE_SLAPPED,  // 20

    DONE_PUNCH_0,
    DONE_PUNCH_1,
    DONE_PUNCH_2,
    DONE_OPEN_STRUCTURE,
    DONE_GET_ITEM,                 // 25

    DONE_GIVING_ITEM,
    DONE_TRAVERSAL,
    BUYSELL,
    ONE_OF_FOUR_STANDARD,
    FRIENDLY_DIRECT_OR_RECRUIT,	// 30
}

public enum QUOTE//StandardQuoteIDs
{
    INTRO = 0,
    SUBS_INTRO,
    FRIENDLY_DEFAULT1,
    FRIENDLY_DEFAULT2,
    GIVEITEM_NO,
    DIRECT_DEFAULT,
    THREATEN_DEFAULT,
    RECRUIT_NO,
    BYE,
    GETLOST,
}

[StructLayout(LayoutKind.Explicit, Size = 32)]
public struct NPCQuoteInfo
{
    [FieldOffset(00)] public ushort fFlags;
    [FieldOffset(02)] public short sRequiredItem;            // item NPC must have to say quote
    [FieldOffset(04)] public short sRequiredGridno;		// location for NPC req'd to say quote
    [FieldOffset(06)] public ushort usFactMustBeTrue;        // ...before saying quote
    [FieldOffset(08)] public ushort usFactMustBeFalse;   // ...before saying quote
    [FieldOffset(10)] public byte ubQuest;                      // quest must be current to say quote
    [FieldOffset(11)] public byte ubFirstDay;                   // first day quote can be said
    [FieldOffset(12)] public byte ubLastDay;                    // last day quote can be said
    [FieldOffset(13)] public byte ubApproachRequired;   // must use this approach to generate quote
    [FieldOffset(14)] public byte ubOpinionRequired;    // opinion needed for this quote     13 bytes
    [FieldOffset(15)] public byte ubQuoteNum;                   // this is the quote to say
    [FieldOffset(16)] public byte ubNumQuotes;              // total # of quotes to say          15 bytes
    [FieldOffset(17)] public byte ubStartQuest;
    [FieldOffset(18)] public byte ubEndQuest;
    [FieldOffset(19)] public byte ubTriggerNPC;
    [FieldOffset(20)] public byte ubTriggerNPCRec;
    [FieldOffset(21)] public byte ubFiller;             //                                       20 bytes
    [FieldOffset(22)] public ushort usSetFactTrue;
    [FieldOffset(24)] public ushort usGiftItem;          // item NPC gives to merc after saying quote
    [FieldOffset(26)] public ushort usGoToGridno;
    [FieldOffset(28)] public short sActionData;      // special action value

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    [FieldOffset(30)] public byte[] ubUnused;
} // 32 bytes
