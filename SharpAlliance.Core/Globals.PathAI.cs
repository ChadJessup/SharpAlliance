using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SharpAlliance.Core;

public partial class Globals
{
    public static int gubGlobalPathFlags = 0;
    public static int gubBuildingInfoToSet;

    public const int ABSMAX_SKIPLIST_LEVEL = 5;
    public const int ABSMAX_TRAIL_TREE = (16384);
    public const int ABSMAX_PATHQ = (512);

    public const int MAX_SKIPLIST_LEVEL = 5;
    public const int MAX_TRAIL_TREE = (4096);
    public const int MAX_PATHQ = (512);

    public static int iMaxSkipListLevel = MAX_SKIPLIST_LEVEL;
    public static int iMaxTrailTree = MAX_TRAIL_TREE;
    public static int iMaxPathQ = MAX_PATHQ;

    public static bool gfGeneratingMapEdgepoints;
}
