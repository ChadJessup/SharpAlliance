using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using SharpAlliance.Core.Interfaces;
using SharpAlliance.Core.Managers;
using SharpAlliance.Platform.Interfaces;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core.SubSystems;

public class NPC
{
    private static IVideoManager video;
    private static IFileManager files;

    public NPC(IVideoManager videoManager, IFileManager fileManager)
    {
        video = videoManager;
        files = fileManager;
    }
    public static bool TriggerNPCWithIHateYouQuote(NPCID ubTriggerNPC)
    {
        // Check if we have a quote to trigger...
        List<NPCQuoteInfo> pNPCQuoteInfoArray;
        NPCQuoteInfo pQuotePtr;
        bool fDisplayDialogue = true;
        int ubLoop;

        if (!EnsureQuoteFileLoaded(ubTriggerNPC))
        {
            // error!!!
            return (false);
        }

        pNPCQuoteInfoArray = gpNPCQuoteInfoArray[ubTriggerNPC];

        for (ubLoop = 0; ubLoop < NUM_NPC_QUOTE_RECORDS; ubLoop++)
        {
            pQuotePtr = (pNPCQuoteInfoArray[ubLoop]);
//            if (NPCConsiderQuote(ubTriggerNPC, 0, APPROACH_DECLARATION_OF_HOSTILITY, ubLoop, 0, pNPCQuoteInfoArray))
//            {
//                // trigger this quote!
//                // reset approach required value so that we can trigger it
//                //pQuotePtr->ubApproachRequired = TRIGGER_NPC;
//                NPCTriggerNPC(ubTriggerNPC, ubLoop, APPROACH_DECLARATION_OF_HOSTILITY, true);
//                gMercProfiles[ubTriggerNPC].ubMiscFlags |= PROFILE_MISC_FLAG_SAID_HOSTILE_QUOTE;
//                return (true);
//            }
        }
        return (false);

    }

    public static void NPCReachedDestination(SOLDIERTYPE? pNPC, bool fAlreadyThere)
    {
        // perform action or whatever after reaching our destination
        NPCID ubNPC;
        NPCQuoteInfo pQuotePtr;
        List<NPCQuoteInfo> pNPCQuoteInfoArray;
        int ubLoop;
        NPC_ACTION ubQuoteRecord;

        if (pNPC.ubQuoteRecord == 0)
        {
            ubQuoteRecord = 0;
        }
        else
        {
            ubQuoteRecord = (pNPC.ubQuoteRecord - 1);
        }

        // Clear values!
        pNPC.ubQuoteRecord = 0;
        if (pNPC.bTeam == gbPlayerNum)
        {
            // the "under ai control" flag was set temporarily; better turn it off now
            pNPC.uiStatusFlags &= (~SOLDIER.PCUNDERAICONTROL);
            // make damn sure the AI_HANDLE_EVERY_FRAME flag is turned off
            pNPC.fAIFlags &= (AIDEFINES.AI_HANDLE_EVERY_FRAME);
        }

        ubNPC = pNPC.ubProfile;
        if (EnsureQuoteFileLoaded(ubNPC) == false)
        {
            // error!!!
            return;
        }

        pNPCQuoteInfoArray = gpNPCQuoteInfoArray[ubNPC];
        pQuotePtr = (pNPCQuoteInfoArray[(int)ubQuoteRecord]);
        // either we are supposed to consider a new quote record
        // (indicated by a negative gridno in the has-item field)
        // or an action to perform once we reached this gridno

        if (pNPC.sGridNo == pQuotePtr.usGoToGridno)
        {
            // check for an after-move action
            if (pQuotePtr.sActionData > 0)
            {
//                NPCDoAction(ubNPC, pQuotePtr.sActionData, ubQuoteRecord);
            }
        }

        for (ubLoop = 0; ubLoop < NUM_NPC_QUOTE_RECORDS; ubLoop++)
        {
            pQuotePtr = (pNPCQuoteInfoArray[ubLoop]);
            if (pNPC.sGridNo == -(pQuotePtr.sRequiredGridno))
            {
//                if (NPCConsiderQuote(ubNPC, 0, TRIGGER_NPC, ubLoop, 0, pNPCQuoteInfoArray))
//                {
//                    if (fAlreadyThere)
//                    {
//                        TriggerNPCRecord(ubNPC, ubLoop);
//                    }
//                    else
//                    {
//                        // trigger this quote
//                        TriggerNPCRecordImmediately(ubNPC, ubLoop);
//                    }
//                    return;
//                }
            }
        }
    }

    public static void ReplaceLocationInNPCDataFromProfileID(NPCID ubNPC, int sOldGridNo, int sNewGridNo)
    {
        if (EnsureQuoteFileLoaded(ubNPC) == false)
        {
            // error!!!
            return;
        }

        var pNPCQuoteInfoArray = gpNPCQuoteInfoArray[ubNPC];

//        ReplaceLocationInNPCData(pNPCQuoteInfoArray, sOldGridNo, sNewGridNo);
    }

    public static void TriggerNPCRecord(NPCID ubTriggerNPC, int ubTriggerNPCRec)
    {
        // Check if we have a quote to trigger...
        NPCQuoteInfo pQuotePtr;
        bool fDisplayDialogue = true;

        if (EnsureQuoteFileLoaded(ubTriggerNPC) == false)
        {
            // error!!!
            return;
        }
        pQuotePtr = (gpNPCQuoteInfoArray[ubTriggerNPC][ubTriggerNPCRec]);
//        if (pQuotePtr.ubQuoteNum == IRRELEVANT)
//        {
//            fDisplayDialogue = false;
//        }
//
//        if (NPCConsiderQuote(ubTriggerNPC, 0, TRIGGER_NPC, ubTriggerNPCRec, 0, gpNPCQuoteInfoArray[ubTriggerNPC]))
//        {
//            NPCTriggerNPC(ubTriggerNPC, ubTriggerNPCRec, TRIGGER_NPC, fDisplayDialogue);
//        }
//        else
//        {
//            // don't do anything
//            // DebugMsg(TOPIC_JA2, DBG_LEVEL_3, String("WARNING: trigger of %d, record %d cannot proceed, possible error", ubTriggerNPC, ubTriggerNPCRec));
//        }
    }

    public static bool EnsureQuoteFileLoaded(NPCID ubNPC)
    {
        bool fLoadFile = false;

        if (ubNPC == NPCID.ROBOT)
        {
            return (false);
        }

        if (gpNPCQuoteInfoArray[ubNPC] == null)
        {
            fLoadFile = true;
        }

        if (ubNPC >= FIRST_RPC && ubNPC < FIRST_NPC)
        {
            if (gMercProfiles[ubNPC].ubMiscFlags.HasFlag(PROFILE_MISC_FLAG.RECRUITED))
            {
                // recruited
                if (gpBackupNPCQuoteInfoArray[ubNPC] == null)
                {
                    // no backup stored of current script, so need to backup
                    fLoadFile = true;
                    // set pointer to back up script!
//                    BackupOriginalQuoteFile(ubNPC);
                }
                // else have backup, are recruited, nothing special
            }
            else
            {
                // not recruited
                if (gpBackupNPCQuoteInfoArray[ubNPC] != null)
                {
                    // backup stored, restore backup
//                    RevertToOriginalQuoteFile(ubNPC);
                }
                // else are no backup, nothing special
            }
        }

        if (fLoadFile)
        {
            gpNPCQuoteInfoArray[ubNPC] = new() { LoadQuoteFile(ubNPC) };
            if (gpNPCQuoteInfoArray[ubNPC] == null)
            {
                // error message at this point!
                return (false);
            }
        }

        return (true);
    }

    public static NPCQuoteInfo LoadQuoteFile(NPCID ubNPC)
    {
        string zFileName;
        Stream hFile;
        NPCQuoteInfo pFileData = new();
        int uiBytesRead;
        int uiFileSize;

        if (ubNPC == NPCID.PETER || ubNPC == NPCID.ALBERTO || ubNPC == NPCID.CARLO)
        {
            // use a copy of Herve's data file instead!
            zFileName = sprintf("NPCData\\%03d.npc", NPCID.HERVE);
        }
        else if (ubNPC < FIRST_RPC || (ubNPC < FIRST_NPC && gMercProfiles[ubNPC].ubMiscFlags.HasFlag(PROFILE_MISC_FLAG.RECRUITED)))
        {
            zFileName = sprintf("NPCData\\000.npc", ubNPC);
        }
        else
        {
            zFileName = sprintf("NPCData\\%03d.npc", ubNPC);
        }

        // ATE: Put some stuff i here to use a different NPC file if we are in a meanwhile.....
        if (Meanwhile.AreInMeanwhile())
        {
            // If we are the queen....
            if (ubNPC == NPCID.QUEEN)
            {
                sprintf(zFileName, "NPCData\\%03d.npc", gubAlternateNPCFileNumsForQueenMeanwhiles[Meanwhile.GetMeanwhileID()]);
            }

            // If we are elliot....
            if (ubNPC == NPCID.ELLIOT)
            {
                sprintf(zFileName, "NPCData\\%03d.npc", gubAlternateNPCFileNumsForElliotMeanwhiles[Meanwhile.GetMeanwhileID()]);
            }

        }

        CHECKN(files.FileExists(zFileName));

//        hFile = files.FileOpen(zFileName, FILE_ACCESS_READ, false);
//        CHECKN(hFile);

//        uiFileSize = sizeof(NPCQuoteInfo) * NUM_NPC_QUOTE_RECORDS;
//        pFileData = MemAlloc(uiFileSize);
//        if (pFileData)
//        {
//            if (!files.FileRead(hFile, ref pFileData, uiFileSize, out uiBytesRead) || uiBytesRead != uiFileSize)
//            {
//                MemFree(pFileData);
//                pFileData = null;
//            }
//        }
//
//        FileClose(hFile);

        return (pFileData);
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

public enum QUOTEID//StandardQuoteIDs
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

//[StructLayout(LayoutKind.Explicit, Size = 32)]
public class NPCQuoteInfo
{
    /*[FieldOffset(00)]*/ public ushort fFlags;
    /*[FieldOffset(02)]*/ public short sRequiredItem;            // item NPC must have to say quote
    /*[FieldOffset(04)]*/ public short sRequiredGridno;		// location for NPC req'd to say quote
    /*[FieldOffset(06)]*/ public ushort usFactMustBeTrue;        // ...before saying quote
    /*[FieldOffset(08)]*/ public ushort usFactMustBeFalse;   // ...before saying quote
    /*[FieldOffset(10)]*/ public byte ubQuest;                      // quest must be current to say quote
    /*[FieldOffset(11)]*/ public byte ubFirstDay;                   // first day quote can be said
    /*[FieldOffset(12)]*/ public byte ubLastDay;                    // last day quote can be said
    /*[FieldOffset(13)]*/ public byte ubApproachRequired;   // must use this approach to generate quote
    /*[FieldOffset(14)]*/ public byte ubOpinionRequired;    // opinion needed for this quote     13 bytes
    /*[FieldOffset(15)]*/ public byte ubQuoteNum;                   // this is the quote to say
    /*[FieldOffset(16)]*/ public byte ubNumQuotes;              // total # of quotes to say          15 bytes
    /*[FieldOffset(17)]*/ public byte ubStartQuest;
    /*[FieldOffset(18)]*/ public byte ubEndQuest;
    /*[FieldOffset(19)]*/ public byte ubTriggerNPC;
    /*[FieldOffset(20)]*/ public byte ubTriggerNPCRec;
    /*[FieldOffset(21)]*/ public byte ubFiller;             //                                       20 bytes
    /*[FieldOffset(22)]*/ public ushort usSetFactTrue;
    /*[FieldOffset(24)]*/ public ushort usGiftItem;          // item NPC gives to merc after saying quote
    /*[FieldOffset(26)]*/ public ushort usGoToGridno;
    /*[FieldOffset(28)]*/ public short sActionData;      // special action value

//    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    /*[FieldOffset(30)]*/ public byte[] ubUnused;
} // 32 bytes
