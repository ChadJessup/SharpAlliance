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
    public static bool gfDisplayPotentialRetreatPaths = false;
    public static int gusRetreatButtonLeft, gusRetreatButtonTop, gusRetreatButtonRight, gusRetreatButtonBottom;

    public static List<GROUP> gpBattleGroup = new();
}
