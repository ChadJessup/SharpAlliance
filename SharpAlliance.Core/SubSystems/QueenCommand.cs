using System;
using System.Diagnostics;

namespace SharpAlliance.Core.SubSystems;

public class QueenCommand
{
    public static int NumEnemiesInAnySector(int sSectorX, int sSectorY, int sSectorZ)
    {
        int ubNumEnemies = 0;

        Debug.Assert(sSectorX >= 1 && sSectorX <= 16);
        Debug.Assert(sSectorY >= 1 && sSectorY <= 16);
        Debug.Assert(sSectorZ >= 0 && sSectorZ <= 3);

        if (sSectorZ)
        {
            UNDERGROUND_SECTORINFO? pSector;
            pSector = FindUnderGroundSector(sSectorX, sSectorY, (int)sSectorZ);
            if (pSector)
            {
                ubNumEnemies = (int)(pSector.ubNumAdmins + pSector.ubNumTroops + pSector.ubNumElites);
            }
        }
        else
        {
            SECTORINFO? pSector;
            GROUP? pGroup;

            //Count stationary enemies
            pSector = Globals.SectorInfo[SECTOR(sSectorX, sSectorY)];
            ubNumEnemies = (int)(pSector.ubNumAdmins + pSector.ubNumTroops + pSector.ubNumElites);

            //Count mobile enemies
            pGroup = gpGroupList;
            while (pGroup)
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

    public static int NumEnemiesInSector(int sSectorX, int sSectorY)
    {
        SECTORINFO? pSector;
        GROUP? pGroup;
        int ubNumTroops;
        Assert(sSectorX >= 1 && sSectorX <= 16);
        Assert(sSectorY >= 1 && sSectorY <= 16);
        pSector = &SectorInfo[SECTOR(sSectorX, sSectorY)];
        ubNumTroops = (int)(pSector.ubNumAdmins + pSector.ubNumTroops + pSector.ubNumElites);

        pGroup = gpGroupList;
        while (pGroup)
        {
            if (!pGroup.fPlayer && !pGroup.fVehicle && pGroup.ubSectorX == sSectorX && pGroup.ubSectorY == sSectorY)
            {
                ubNumTroops += pGroup.ubGroupSize;
            }
            pGroup = pGroup.next;
        }
        return ubNumTroops;
    }

    public static int NumStationaryEnemiesInSector(int sSectorX, int sSectorY)
    {
        SECTORINFO? pSector;
        Debug.Assert(sSectorX >= 1 && sSectorX <= 16);
        Debug.Assert(sSectorY >= 1 && sSectorY <= 16);
        pSector = Globals.SectorInfo[SECTOR(sSectorX, sSectorY)];

        if (pSector.ubGarrisonID == NO_GARRISON)
        { //If no garrison, no stationary.
            return (0);
        }

        // don't count roadblocks as stationary garrison, we want to see how many enemies are in them, not question marks
        if (gGarrisonGroup[pSector.ubGarrisonID].ubComposition == ROADBLOCK)
        {
            // pretend they're not stationary
            return (0);
        }

        return (int)(pSector.ubNumAdmins + pSector.ubNumTroops + pSector.ubNumElites);
    }
}
