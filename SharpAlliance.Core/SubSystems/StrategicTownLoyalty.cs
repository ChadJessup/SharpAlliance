﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpAlliance.Core.Screens;

using static SharpAlliance.Core.Globals;

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

        return iNumber;
    }

    // is the ENTIRE town under player control?
    public static bool IsTownUnderCompleteControlByPlayer(TOWNS bTownId)
    {
//        if (GetTownSectorSize(bTownId) == GetTownSectorsUnderControl(bTownId))
//        {
//            return (true);
//        }

        return false;
    }

    // is the ENTIRE town under enemy control?
    bool IsTownUnderCompleteControlByEnemy(TOWNS bTownId)
    {
        if (GetTownSectorsUnderControl(bTownId) == 0)
        {
            return true;
        }

        return false;
    }

    // return number of sectors under player control for this town
    public static int GetTownSectorsUnderControl(TOWNS bTownId)
    {
        int ubSectorsControlled = 0;
        int iCounterA = 0;
        MAP_ROW iCounterB = 0;
        int usSector = 0;

        for (iCounterA = 0; iCounterA < (Globals.MAP_WORLD_X - 1); iCounterA++)
        {
            for (iCounterB = 0; (int)iCounterB < (Globals.MAP_WORLD_Y - 1); iCounterB++)
            {
                usSector = StrategicMap.CALCULATE_STRATEGIC_INDEX(iCounterA, iCounterB);

                if ((Globals.strategicMap[usSector].bNameId == bTownId) &&
                        (Globals.strategicMap[usSector].fEnemyControlled == false) &&
                        (QueenCommand.NumEnemiesInSector(iCounterA, iCounterB) == 0))
                {
                    ubSectorsControlled++;
                }
            }
        }

        return ubSectorsControlled;
    }

    public static void InitTownLoyalty()
    {
        // set up town loyalty table
        for (TOWNS ubTown = FIRST_TOWN; ubTown < TOWNS.NUM_TOWNS; ubTown++)
        {
            gTownLoyalty[ubTown] = new()
            {
                ubRating = 0,
                sChange = 0,
                fStarted = false,
                //		gTownLoyalty[ ubTown ].ubRebelSentiment = gubTownRebelSentiment[ ubTown ];
                fLiberatedAlready = false
            };
        }

        return;
    }
}
