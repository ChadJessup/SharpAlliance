using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpAlliance.Core.SubSystems;
using static System.Runtime.InteropServices.JavaScript.JSType;
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
                    ScreenMsg(FONT_RED, MSG.ERROR, "Attempting to check if mine is clear but mine index is invalid (%d).", ubMineIndex);
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

    //Returns true if valid and creature quest over, false if creature quest active or not yet started
    public static bool GetWarpOutOfMineCodes(out int psSectorX, out MAP_ROW psSectorY, out int pbSectorZ, out int psInsertionGridNo)
    {
        int iSwitchValue;
        psSectorX = 0;
        psSectorY = 0;
        pbSectorZ = 0;
        psInsertionGridNo = 0;

        if (!gfWorldLoaded)
        {
            return false;
        }

        if (gbWorldSectorZ == 0)
        {
            return false;
        }

        iSwitchValue = giLairID;

        if (iSwitchValue == -1)
        {
            iSwitchValue = giDestroyedLairID;
        }

        if (iSwitchValue == 0)
        {
            return false;
        }

        //Now make sure the mercs are in the previously infested mine
        switch (iSwitchValue)
        {
            case 1: //Drassen
                if (gWorldSectorX == 13 && gWorldSectorY == (MAP_ROW)6 && gbWorldSectorZ == 3 ||
                        gWorldSectorX == 13 && gWorldSectorY == (MAP_ROW)7 && gbWorldSectorZ == 3 ||
                        gWorldSectorX == 13 && gWorldSectorY == (MAP_ROW)7 && gbWorldSectorZ == 2 ||
                        gWorldSectorX == 13 && gWorldSectorY == (MAP_ROW)6 && gbWorldSectorZ == 2 ||
                        gWorldSectorX == 13 && gWorldSectorY == (MAP_ROW)5 && gbWorldSectorZ == 2 ||
                        gWorldSectorX == 13 && gWorldSectorY == (MAP_ROW)5 && gbWorldSectorZ == 1 ||
                        gWorldSectorX == 13 && gWorldSectorY == (MAP_ROW)4 && gbWorldSectorZ == 1)
                {
                    psSectorX = 13;
                    psSectorY = (MAP_ROW)4;
                    pbSectorZ = 0;
                    psInsertionGridNo = 20700;
                    return true;
                }
                break;
            case 3: //Cambria
                if (gWorldSectorX == 8     && gWorldSectorY == (MAP_ROW)9 && gbWorldSectorZ == 3 ||
                        gWorldSectorX == 8 && gWorldSectorY == (MAP_ROW)8 && gbWorldSectorZ == 3 ||
                        gWorldSectorX == 8 && gWorldSectorY == (MAP_ROW)8 && gbWorldSectorZ == 2 ||
                        gWorldSectorX == 9 && gWorldSectorY == (MAP_ROW)8 && gbWorldSectorZ == 2 ||
                        gWorldSectorX == 9 && gWorldSectorY == (MAP_ROW)8 && gbWorldSectorZ == 1 ||
                        gWorldSectorX == 8 && gWorldSectorY == (MAP_ROW)8 && gbWorldSectorZ == 1)
                {
                    psSectorX = 8;
                    psSectorY = (MAP_ROW)8;
                    pbSectorZ = 0;
                    psInsertionGridNo = 13002;
                    return true;
                }
                break;
            case 2: //Alma
                if (gWorldSectorX == 13     && gWorldSectorY == (MAP_ROW)11 && gbWorldSectorZ == 3 ||
                        gWorldSectorX == 13 && gWorldSectorY == (MAP_ROW)10 && gbWorldSectorZ == 3 ||
                        gWorldSectorX == 13 && gWorldSectorY == (MAP_ROW)10 && gbWorldSectorZ == 2 ||
                        gWorldSectorX == 14 && gWorldSectorY == (MAP_ROW)10 && gbWorldSectorZ == 2 ||
                        gWorldSectorX == 14 && gWorldSectorY == (MAP_ROW)10 && gbWorldSectorZ == 1 ||
                        gWorldSectorX == 14 && gWorldSectorY == (MAP_ROW)9 && gbWorldSectorZ == 1)
                {
                    psSectorX = 14;
                    psSectorY = (MAP_ROW)9;
                    pbSectorZ = 0;
                    psInsertionGridNo = 9085;
                    return true;
                }
                break;
            case 4: //Grumm
                if (gWorldSectorX == 4 && gWorldSectorY == MAP_ROW.G && gbWorldSectorZ == 3 ||
                        gWorldSectorX == 4 && gWorldSectorY == MAP_ROW.H && gbWorldSectorZ == 3 ||
                        gWorldSectorX == 3 && gWorldSectorY == MAP_ROW.H && gbWorldSectorZ == 2 ||
                        gWorldSectorX == 3 && gWorldSectorY == MAP_ROW.H && gbWorldSectorZ == 2 ||
                        gWorldSectorX == 3 && gWorldSectorY == MAP_ROW.I && gbWorldSectorZ == 2 ||
                        gWorldSectorX == 3 && gWorldSectorY == MAP_ROW.I && gbWorldSectorZ == 1 ||
                        gWorldSectorX == 3 && gWorldSectorY == MAP_ROW.H && gbWorldSectorZ == 1)
                {
                    psSectorX = 3;
                    psSectorY = MAP_ROW.H;
                    pbSectorZ = 0;
                    psInsertionGridNo = 9822;
                    return true;
                }
                break;
        }
        return false;
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
