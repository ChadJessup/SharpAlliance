using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpAlliance.Core.SubSystems;

namespace SharpAlliance.Core;

public class SoldierInitList
{
}

public class SOLDIERINITNODE
{
    public int ubNodeID;
    public int ubSoldierID;
    public BASIC_SOLDIERCREATE_STRUCT? pBasicPlacement;
    public SOLDIERCREATE_STRUCT? pDetailedPlacement;
    public SOLDIERTYPE? pSoldier;
    public SOLDIERINITNODE? prev;
    public SOLDIERINITNODE? next;
}

public partial class Globals
{
    public const int SOLDIER_CREATE_AUTO_TEAM = -1;
    public const int MAX_INDIVIDUALS = 148;
    public SOLDIERINITNODE? gSoldierInitHead;
    public SOLDIERINITNODE? gSoldierInitTail;

    public static bool gfEstimatePath { get; internal set; }
}
