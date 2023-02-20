using System;
using System.Diagnostics;

namespace SharpAlliance.Core.SubSystems;

public class QueenCommand
{
    public static int NumEnemiesInAnySector(int sSectorX, MAP_ROW sSectorY, int sSectorZ)
    {
        int ubNumEnemies = 0;

        Debug.Assert(sSectorX >= 1 && sSectorX <= 16);
        Debug.Assert(sSectorY >= (MAP_ROW)1 && sSectorY <= (MAP_ROW)16);
        Debug.Assert(sSectorZ >= 0 && sSectorZ <= 3);

        if (sSectorZ > 0)
        {
            UNDERGROUND_SECTORINFO? pSector;
            pSector = FindUnderGroundSector(sSectorX, sSectorY, (int)sSectorZ);
            if (pSector is not null)
            {
                ubNumEnemies = (int)(pSector.ubNumAdmins + pSector.ubNumTroops + pSector.ubNumElites);
            }
        }
        else
        {
            SECTORINFO? pSector;
            GROUP? pGroup;

            //Count stationary enemies
            pSector = Globals.SectorInfo[SECTORINFO.SECTOR(sSectorX, sSectorY)];
            ubNumEnemies = (int)(pSector.ubNumAdmins + pSector.ubNumTroops + pSector.ubNumElites);

            //Count mobile enemies
            pGroup = Globals.gpGroupList;
            while (pGroup is not null)
            {
                if (!pGroup.fPlayer && !pGroup.fVehicle && pGroup.ubSectorX == sSectorX && pGroup.ubSectorY == sSectorY)
                {
                    ubNumEnemies += pGroup.ubGroupSize;
                }
                pGroup = pGroup.next;
            }
        }

        return ubNumEnemies;
    }

    public static int NumEnemiesInSector(int sSectorX, MAP_ROW sSectorY)
    {
        SECTORINFO? pSector;
        GROUP? pGroup;
        int ubNumTroops;
        Debug.Assert(sSectorX >= 1 && sSectorX <= 16);
        Debug.Assert(sSectorY >= (MAP_ROW)1 && sSectorY <= (MAP_ROW)16);
        pSector = SectorInfo[SECTORINFO.SECTOR(sSectorX, sSectorY)];
        ubNumTroops = (int)(pSector.ubNumAdmins + pSector.ubNumTroops + pSector.ubNumElites);

        pGroup = Globals.gpGroupList;
        while (pGroup is not null)
        {
            if (!pGroup.fPlayer
                && !pGroup.fVehicle
                && pGroup.ubSectorX == sSectorX
                && pGroup.ubSectorY == sSectorY)
            {
                ubNumTroops += pGroup.ubGroupSize;
            }
            pGroup = pGroup.next;
        }
        return ubNumTroops;
    }

    public static int NumStationaryEnemiesInSector(int sSectorX, MAP_ROW sSectorY)
    {
        SECTORINFO? pSector;
        Debug.Assert(sSectorX >= 1 && sSectorX <= 16);
        Debug.Assert(sSectorY >= (MAP_ROW)1 && sSectorY <= (MAP_ROW)16);
        pSector = Globals.SectorInfo[SECTORINFO.SECTOR(sSectorX, sSectorY)];

        if (pSector.ubGarrisonID == NO_GARRISON)
        { //If no garrison, no stationary.
            return (0);
        }

        // don't count roadblocks as stationary garrison, we want to see how many enemies are in them, not question marks
        if (Globals.gGarrisonGroup[pSector.ubGarrisonID].ubComposition == ROADBLOCK)
        {
            // pretend they're not stationary
            return (0);
        }

        return (int)(pSector.ubNumAdmins + pSector.ubNumTroops + pSector.ubNumElites);
    }
}
