using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpAlliance.Core.SubSystems;

using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core.SubSystems;

public class EXITGRIDS
{
}

public struct EXITGRID //for exit grids (object level)
{ //if an item pool is also in same gridno, then this would be a separate levelnode
    //in the object level list
    public int usGridNo; //sweet spot for placing mercs in new sector.
    public int ubGotoSectorX;
    public int ubGotoSectorY;
    public int ubGotoSectorZ;
}

