using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpAlliance.Core.SubSystems;

namespace SharpAlliance.Core;

public partial class Globals
{
    public const int EDGE_OF_MAP_SEARCH = 5;
    public static int[,] gubAIPathCosts = new int[19, 19];
    public const int MINIMUM_REQUIRED_STATUS = 70;
    public static int SkipCoverCheck = 0;
    public static THREATTYPE[] Threat = new THREATTYPE[MAXMERCS];
}
