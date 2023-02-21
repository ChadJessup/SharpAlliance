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
        int iNumber = 0;
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

    // is the ENTIRE town under player control?
    bool IsTownUnderCompleteControlByPlayer(TOWNS bTownId)
    {
        if (GetTownSectorSize(bTownId) == GetTownSectorsUnderControl(bTownId))
        {
            return (true);
        }

        return (false);
    }

    // is the ENTIRE town under enemy control?
    bool IsTownUnderCompleteControlByEnemy(TOWNS bTownId)
    {
        if (GetTownSectorsUnderControl(bTownId) == 0)
        {
            return (true);
        }

        return (false);
    }

    // return number of sectors under player control for this town
    int GetTownSectorsUnderControl(TOWNS bTownId)
    {
        int ubSectorsControlled = 0;
        int iCounterA = 0;
        MAP_ROW iCounterB = 0;
        int usSector = 0;

        for (iCounterA = 0; iCounterA < (Globals.MAP_WORLD_X - 1); iCounterA++)
        {
            for (iCounterB = 0; (int)iCounterB < (Globals.MAP_WORLD_Y - 1); iCounterB++)
            {
                usSector = CALCULATE_STRATEGIC_INDEX(iCounterA, iCounterB);

                if ((Globals.StrategicMap[usSector].bNameId == bTownId) &&
                        (Globals.StrategicMap[usSector].fEnemyControlled == false) &&
                        (QueenCommand.NumEnemiesInSector(iCounterA, iCounterB) == 0))
                {
                    ubSectorsControlled++;
                }
            }
        }

        return (ubSectorsControlled);
    }
}
