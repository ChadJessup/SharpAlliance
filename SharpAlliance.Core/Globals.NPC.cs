using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpAlliance.Core.SubSystems;

namespace SharpAlliance.Core;

public partial class Globals
{
    public const int NUM_NPC_QUOTE_RECORDS = 50;
    public static Dictionary<NPCID, List<NPCQuoteInfo>> gpNPCQuoteInfoArray = new();// [NUM_PROFILES] = { NULL };
    public static Dictionary<NPCID, List<NPCQuoteInfo>> gpBackupNPCQuoteInfoArray = new();// [NUM_PROFILES] = { NULL };
    public static Dictionary<NPCID, List<NPCQuoteInfo>> gpCivQuoteInfoArray = new();// [NUM_CIVQUOTE_SECTORS] = { NULL };
}
