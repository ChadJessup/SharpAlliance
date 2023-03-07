using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpAlliance.Core.SubSystems;
using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core;

public class CreatureSpreading
{
    public static bool MineClearOfMonsters(MINE ubMineIndex)
    {
        Debug.Assert((ubMineIndex >= 0) && (ubMineIndex < MINE.MAX_NUMBER_OF_MINES));

        if (!gMineStatus[ubMineIndex].fPrevInvadedByMonsters)
        {
            switch (ubMineIndex)
            {
                case MINE.GRUMM:
                    if (CreaturesInUndergroundSector(SEC.H3, 1) > 0)
                    {
                        return false;
                    }

                    if (CreaturesInUndergroundSector(SEC.I3, 1) > 0)
                    {
                        return false;
                    }

                    if (CreaturesInUndergroundSector(SEC.I3, 2) > 0)
                    {
                        return false;
                    }

                    if (CreaturesInUndergroundSector(SEC.H3, 2) > 0)
                    {
                        return false;
                    }

                    if (CreaturesInUndergroundSector(SEC.H4, 2) > 0)
                    {
                        return false;
                    }

                    break;
                case MINE.CAMBRIA:
                    if (CreaturesInUndergroundSector(SEC.H8, 1) > 0)
                    {
                        return false;
                    }

                    if (CreaturesInUndergroundSector(SEC.H9, 1) > 0)
                    {
                        return false;
                    }

                    break;
                case MINE.ALMA:
                    if (CreaturesInUndergroundSector(SEC.I14, 1) > 0)
                    {
                        return false;
                    }

                    if (CreaturesInUndergroundSector(SEC.J14, 1) > 0)
                    {
                        return false;
                    }

                    break;
                case MINE.DRASSEN:
                    if (CreaturesInUndergroundSector(SEC.D13, 1) > 0)
                    {
                        return false;
                    }

                    if (CreaturesInUndergroundSector(SEC.E13, 1) > 0)
                    {
                        return false;
                    }

                    break;
                case MINE.CHITZENA:
                case MINE.SAN_MONA:
                    // these are never attacked
                    break;

                default:
# if JA2BETAVERSION
                    ScreenMsg(FONT_RED, MSG_ERROR, L"Attempting to check if mine is clear but mine index is invalid (%d).", ubMineIndex);
#endif
                    break;
            }
        }
        else
        { //mine was previously invaded by creatures.  Don't allow mine production until queen is dead.
            if (giLairID != -1)
            {
                return false;
            }
        }

        return true;
    }

    public static int CreaturesInUndergroundSector(SEC ubSectorID, int ubSectorZ)
    {
        UNDERGROUND_SECTORINFO? pSector;
        int ubSectorX;
        MAP_ROW ubSectorY;

        ubSectorX = SECTORINFO.SECTORX(ubSectorID);
        ubSectorY = SECTORINFO.SECTORY(ubSectorID);
        pSector = QueenCommand.FindUnderGroundSector(ubSectorX, ubSectorY, ubSectorZ);
        if (pSector is not null)
        {
            return pSector.ubNumCreatures;
        }

        return 0;
    }
}

public enum CREATURE_BATTLE
{
    CODE_NONE,
    CODE_TACTICALLYADD,
    CODE_TACTICALLYADD_WITHFOV,
    CODE_PREBATTLEINTERFACE,
    CODE_AUTORESOLVE,
};
