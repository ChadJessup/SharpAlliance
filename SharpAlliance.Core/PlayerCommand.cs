using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpAlliance.Core.SubSystems;

namespace SharpAlliance.Core;

public class PlayerCommand
{
    public static void ReplaceSoldierProfileInPlayerGroup(int ubGroupID, NPCID ubOldProfile, NPCID ubNewProfile)
    {
        GROUP? pGroup = StrategicMovement.GetGroup(ubGroupID);

        if (pGroup is null)
        {
            return;
        }

        foreach (var curr in pGroup.pPlayerList)
        {
            if (curr.ubProfileID == ubOldProfile)
            {
                // replace and return!
                curr.ubProfileID = ubNewProfile;
                return;
            }

            //curr = curr.next;
        }

    }
}
