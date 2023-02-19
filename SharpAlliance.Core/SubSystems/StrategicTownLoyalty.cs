using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpAlliance.Core.Screens;

namespace SharpAlliance.Core.SubSystems;

public class StrategicTownLoyalty
{
    public static int GetNumberOfWholeTownsUnderControl()
    {
        int  iNumber = 0;
        TOWNS bTownId = 0;

        // run through the list of towns..if the entire town is under player control, then increment the number of towns under player control

        // make sure that each town is one for which loyalty matters
        for (bTownId = Globals.FIRST_TOWN; bTownId < TOWNS.NUM_TOWNS; bTownId++)
        {
            if (IsTownUnderCompleteControlByPlayer(bTownId) && Globals.gfTownUsesLoyalty[bTownId])
            {
                iNumber++;
            }
        }

        return (iNumber);
    }
}
