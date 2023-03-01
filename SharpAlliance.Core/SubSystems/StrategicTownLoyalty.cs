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
    public static bool IsTownUnderCompleteControlByPlayer(TOWNS bTownId)
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

public enum GLOBAL_LOYALTY
{
    // There are only for distance-adjusted global loyalty effects.  Others go into list above instead!
    BATTLE_WON,
    BATTLE_LOST,
    ENEMY_KILLED,
    NATIVE_KILLED,
    GAIN_TOWN_SECTOR,
    LOSE_TOWN_SECTOR,
    LIBERATE_WHOLE_TOWN,     // awarded only the first time it happens
    ABANDON_MILITIA,
    GAIN_MINE,
    LOSE_MINE,
    GAIN_SAM,
    LOSE_SAM,
    QUEEN_BATTLE_WON,
}
