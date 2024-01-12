using System;
using System.Diagnostics;

using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core.SubSystems;

public class QueenCommand
{
    public static UNDERGROUND_SECTORINFO? FindUnderGroundSector(int sMapX, MAP_ROW sMapY, int bMapZ)
    {
        UNDERGROUND_SECTORINFO? pUnderground;
        pUnderground = gpUndergroundSectorInfoHead;

        //Loop through all the underground sectors looking for specified sector
        while (pUnderground is not null)
        {
            //If the sector is the right one
            if (pUnderground.ubSectorX == sMapX
                && pUnderground.ubSectorY == sMapY
                && pUnderground.ubSectorZ == bMapZ)
            {
                return pUnderground;
            }
            pUnderground = pUnderground.next;
        }

        return null;
    }

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
            SECTORINFO? pSector = null;
            GROUP? pGroup = null;

            //Count stationary enemies
            pSector = Globals.SectorInfo[SECTORINFO.SECTOR(sSectorX, sSectorY)];
            ubNumEnemies = (int)(pSector.ubNumAdmins + pSector.ubNumTroops + pSector.ubNumElites);

            //Count mobile enemies
//            pGroup = Globals.gpGroupList;
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
        SECTORINFO? pSector = null;
        GROUP? pGroup = null;
        int ubNumTroops;
        Debug.Assert(sSectorX >= 1 && sSectorX <= 16);
        Debug.Assert(sSectorY >= (MAP_ROW)1 && sSectorY <= (MAP_ROW)16);
        pSector = SectorInfo[SECTORINFO.SECTOR(sSectorX, sSectorY)];
        ubNumTroops = (int)(pSector.ubNumAdmins + pSector.ubNumTroops + pSector.ubNumElites);

//        pGroup = Globals.gpGroupList;
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
            return 0;
        }

        // don't count roadblocks as stationary garrison, we want to see how many enemies are in them, not question marks
        if (gGarrisonGroup[(int)pSector.ubGarrisonID].ubComposition == Garrisons.ROADBLOCK)
        {
            // pretend they're not stationary
            return 0;
        }

        return (int)(pSector.ubNumAdmins + pSector.ubNumTroops + pSector.ubNumElites);
    }
}
