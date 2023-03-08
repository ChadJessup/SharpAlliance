using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpAlliance.Core.SubSystems;

namespace SharpAlliance.Core;

public partial class Globals
{
    // the delay for a group about to arrive
    public const int ABOUT_TO_ARRIVE_DELAY = 5;

    // is the bottom of the map panel dirty?
    public static  bool gfUsePersistantPBI;
    public static int[] SquadMovementGroups;
    public static int[] gubVehicleMovementGroups;
    public static bool gfDelayAutoResolveStart = false;
    public static bool gfRandomizingPatrolGroup = false;

    //Doesn't require text localization.  This is for debug strings only.
    public static Traversability[] gszTerrain =
    {
        Traversability.TOWN, Traversability.ROAD, Traversability.PLAINS, Traversability.SAND, Traversability.SPARSE, Traversability.DENSE, Traversability.SWAMP,
        Traversability.WATER, Traversability.HILLS, Traversability.GROUNDBARRIER, Traversability.NS_RIVER, Traversability.EW_RIVER, Traversability.EDGEOFWORLD
    };

    public static bool gfUndergroundTacticalTraversal = false;

    // remembers which player group is the Continue/Stop prompt about?  No need to save as long as you can't save while prompt ON
    public static GROUP? gpGroupPrompting = null;
    public static GROUP? gpInitPrebattleGroup = null;

    public static int[] uniqueIDMask = { 0, 0, 0, 0, 0, 0, 0, 0 };


    // waiting for input from user
    public static bool gfWaitingForInput = false;
}
