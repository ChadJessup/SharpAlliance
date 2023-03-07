using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core;

public class RenderFun
{
    public static bool InARoom(int sGridNo, out int? pubRoomNo)
    {
        pubRoomNo= null;

        if (gubWorldRoomInfo[sGridNo] != NO_ROOM)
        {
            pubRoomNo = gubWorldRoomInfo[sGridNo];

            return (true);
        }

        return (false);
    }

}
