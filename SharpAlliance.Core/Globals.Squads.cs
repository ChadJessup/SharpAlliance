using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpAlliance.Core.SubSystems;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SharpAlliance.Core;

public partial class Globals
{
    public static SquadEnum iCurrentTacticalSquad = SquadEnum.FIRST_SQUAD;

    public static bool gfPausedTacticalRenderFlags;
    public static int gfPausedTacticalRenderInterfaceFlags;

    public const SquadEnum NUMBER_OF_SQUADS = SquadEnum.NUMBER_OF_SQUADS;
    public const int NUMBER_OF_SOLDIERS_PER_SQUAD = 6;
    public const SquadEnum NO_CURRENT_SQUAD = SquadEnum.NUMBER_OF_SQUADS;

    public static Dictionary<SquadEnum, Dictionary<int, SOLDIERTYPE?>> Squad = new();// [NUMBER_OF_SQUADS][NUMBER_OF_SOLDIERS_PER_SQUAD];

    // list of dead guys for squads...in id values . -1 means no one home 
    public static Dictionary<SquadEnum, Dictionary<int, NPCID>> sDeadMercs = new();// [SquadEnum.NUMBER_OF_SQUADS][NUMBER_OF_SOLDIERS_PER_SQUAD];

    // the movement group ids
    public static Dictionary<SquadEnum, int> SquadMovementGroups = new();// [SquadEnum.NUMBER_OF_SQUADS];

}
