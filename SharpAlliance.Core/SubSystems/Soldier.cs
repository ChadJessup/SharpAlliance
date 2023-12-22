using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core.SubSystems;

public static class Soldier
{

    // Checks if our guy can be controllable .... checks bInSector, team, on duty, etc...
    public static bool OK_CONTROLLABLE_MERC(SOLDIERTYPE p)
        => p.bLife >= Globals.OKLIFE
            && p.bActive
            && p.bInSector
            && p.bTeam == Globals.gbPlayerNum
            && p.bAssignment < Assignments.ON_DUTY;

}
