using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;

namespace SharpAlliance.Core.SubSystems;

public class CampaignInit
{
    UNDERGROUND_SECTORINFO? gpUndergroundSectorInfoTail = null;


    UNDERGROUND_SECTORINFO NewUndergroundNode(byte ubSectorX, MAP_ROW ubSectorY, byte ubSectorZ)
    {
        UNDERGROUND_SECTORINFO curr;
        curr = new UNDERGROUND_SECTORINFO();

        curr.ubSectorX = ubSectorX;
        curr.ubSectorY = ubSectorY;
        curr.ubSectorZ = ubSectorZ;

        if (gpUndergroundSectorInfoTail is not null)
        {
            gpUndergroundSectorInfoTail.next = curr;
            gpUndergroundSectorInfoTail = gpUndergroundSectorInfoTail.next;
        }
        else
        {
            gpUndergroundSectorInfoHead = curr;
            gpUndergroundSectorInfoTail = gpUndergroundSectorInfoHead;
        }

        return curr;
    }

    // setup which know facilities are in which cities
    void InitKnowFacilitiesFlags()
    {
        // Cambria hospital
        SECTORINFO pSector = SectorInfo[SEC.G8];
        pSector.uiFacilitiesFlags |= SFCF.HOSPITAL;
        pSector = SectorInfo[SEC.F8];
        pSector.uiFacilitiesFlags |= SFCF.HOSPITAL;
        pSector = SectorInfo[SEC.G9];
        pSector.uiFacilitiesFlags |= SFCF.HOSPITAL;
        pSector = SectorInfo[SEC.F9];
        pSector.uiFacilitiesFlags |= SFCF.HOSPITAL;

        // Drassen airport
        pSector = SectorInfo[SEC.B13];
        pSector.uiFacilitiesFlags |= SFCF.AIRPORT;
        pSector = SectorInfo[SEC.C13];
        pSector.uiFacilitiesFlags |= SFCF.AIRPORT;
        pSector = SectorInfo[SEC.D13];
        pSector.uiFacilitiesFlags |= SFCF.AIRPORT;

        // Meduna airport & military complex
        pSector = SectorInfo[SEC.N3];
        pSector.uiFacilitiesFlags |= SFCF.AIRPORT;
        pSector = SectorInfo[SEC.N4];
        pSector.uiFacilitiesFlags |= SFCF.AIRPORT;
        pSector = SectorInfo[SEC.N5];
        pSector.uiFacilitiesFlags |= SFCF.AIRPORT;
        pSector = SectorInfo[SEC.O3];
        pSector.uiFacilitiesFlags |= SFCF.AIRPORT;
        pSector = SectorInfo[SEC.O4];
        pSector.uiFacilitiesFlags |= SFCF.AIRPORT;

        return;
    }


    void InitMiningLocations()
    {
        //Set up mining sites

        SECTORINFO pSector = SectorInfo[SEC.D4];
        pSector.uiFlags |= SF.MINING_SITE;
        //	pSector.ubIncomeValue = 33;		

        pSector = SectorInfo[SEC.D13];
        pSector.uiFlags |= SF.MINING_SITE;
        //	pSector.ubIncomeValue = 41;

        pSector = SectorInfo[SEC.B2];
        pSector.uiFlags |= SF.MINING_SITE;
        //	pSector.ubIncomeValue = 20;

        pSector = SectorInfo[SEC.H8];
        pSector.uiFlags |= SF.MINING_SITE;
        //	pSector.ubIncomeValue = 64;

        pSector = SectorInfo[SEC.I14];
        pSector.uiFlags |= SF.MINING_SITE;
        //	pSector.ubIncomeValue = 80;

        //Grumm
        pSector = SectorInfo[SEC.H3];
        pSector.uiFlags |= SF.MINING_SITE;
        //	pSector.ubIncomeValue = 100;
    }

    //Mobile groups are handled separately from sectors, because they are on the move.  
    void GeneratePatrolGroups()
    {
        GROUP pGroup;
        int ubNumTroops;
        ubNumTroops = (3 + gGameOptions.ubDifficultyLevel + Random(3));
        pGroup = StrategicMovement.CreateNewEnemyGroupDepartingFromSector(SEC.C7, 0, ubNumTroops, 0);
        pGroup.ubTransportationMask = CAR;
        AddWaypointToPGroup(pGroup, 8, 3); //C8
        AddWaypointToPGroup(pGroup, 7, 3); //C7

        ubNumTroops = (3 + gGameOptions.ubDifficultyLevel + Random(3));
        pGroup = CreateNewEnemyGroupDepartingFromSector(SEC.D9, 0, ubNumTroops, 0);
        AddWaypointToPGroup(pGroup, 9, 5); //E9
        AddWaypointToPGroup(pGroup, 9, 4); //D9
        pGroup.ubTransportationMask = TRUCK;

        ubNumTroops = (3 + gGameOptions.ubDifficultyLevel + Random(3));
        pGroup = CreateNewEnemyGroupDepartingFromSector(SEC.B9, 0, ubNumTroops, 0);
        AddWaypointToPGroup(pGroup, 12, 2); //B12
        AddWaypointToPGroup(pGroup, 9, 2); //B9
        pGroup.ubTransportationMask = FOOT;

        ubNumTroops = (3 + gGameOptions.ubDifficultyLevel + Random(3));
        pGroup = CreateNewEnemyGroupDepartingFromSector(SEC.A14, 0, ubNumTroops, 0);
        pGroup.ubMoveType = ENDTOEND_FORWARDS;
        AddWaypointToPGroup(pGroup, 13, 1); //A13
        AddWaypointToPGroup(pGroup, 15, 1); //A15
        AddWaypointToPGroup(pGroup, 15, 5); //E15
        AddWaypointToPGroup(pGroup, 13, 5); //E13
        AddWaypointToPGroup(pGroup, 12, 5); //E12
        AddWaypointToPGroup(pGroup, 12, 3); //C12
        pGroup.ubTransportationMask = TRACKED;

        ubNumTroops = (5 + gGameOptions.ubDifficultyLevel * 2 + Random(4));
        pGroup = CreateNewEnemyGroupDepartingFromSector(SEC.N6, 0, ubNumTroops, 0);
        AddWaypointToPGroup(pGroup, 9, 14); //N9
        AddWaypointToPGroup(pGroup, 6, 14); //N6
        pGroup.ubTransportationMask = CAR;

        ubNumTroops = (5 + gGameOptions.ubDifficultyLevel * 2 + Random(4));
        pGroup = CreateNewEnemyGroupDepartingFromSector(SEC.N10, 0, ubNumTroops, 0);
        AddWaypointToPGroup(pGroup, 10, 11); //K10
        AddWaypointToPGroup(pGroup, 10, 14); //N10
        pGroup.ubTransportationMask = CAR;
    }

    void TrashUndergroundSectorInfo()
    {
        UNDERGROUND_SECTORINFO* curr;
        while (gpUndergroundSectorInfoHead)
        {
            curr = gpUndergroundSectorInfoHead;
            gpUndergroundSectorInfoHead = gpUndergroundSectorInfoHead.next;
            MemFree(curr);
        }
        gpUndergroundSectorInfoHead = null;
        gpUndergroundSectorInfoTail = null;
    }

    //Defines the sectors that can be occupied by enemies, creatures, etc.  It also
    //contains the network of cave connections critical for strategic creature spreading, as we can't
    //know how the levels connect without loading the maps.  This is completely hardcoded, and any 
    //changes to the maps, require changes accordingly.
    void BuildUndergroundSectorInfoList()
    {
        UNDERGROUND_SECTORINFO curr;
        SECTORINFO pSector;

        TrashUndergroundSectorInfo();

        //********************
        //* BASEMENT LEVEL 1 *
        //********************

        //Miguel's basement.  Nothing here.
        curr = NewUndergroundNode(10, 1, 1);

        //Chitzena mine.  Nothing here.
        curr = NewUndergroundNode(2, 2, 1);

        //San mona mine.  Nothing here.
        curr = NewUndergroundNode(4, 4, 1);
        curr = NewUndergroundNode(5, 4, 1);

        //J9
        curr = NewUndergroundNode(9, 10, 1);
        switch (gGameOptions.ubDifficultyLevel)
        {
            case DifficultyLevel.Easy:
                curr.ubNumTroops = 8;
                break;
            case DifficultyLevel.Medium:
                curr.ubNumTroops = 11;
                break;
            case DifficultyLevel.Hard:
                curr.ubNumTroops = 15;
                break;
        }
        //J9 feeding zone
        curr = NewUndergroundNode(9, 10, 2);
        curr.ubNumCreatures = (2 + (int)gGameOptions.ubDifficultyLevel * 2 + Random(2));

        //K4
        curr = NewUndergroundNode(4, 11, 1);
        curr.ubNumTroops = (6 + (int)gGameOptions.ubDifficultyLevel * 2 + Random(3));
        curr.ubNumElites = (4 + gGameOptions.ubDifficultyLevel + Random(2));

        //O3
        curr = NewUndergroundNode(3, 15, 1);
        curr.ubNumTroops = (6 + (int)gGameOptions.ubDifficultyLevel * 2 + Random(3));
        curr.ubNumElites = (4 + gGameOptions.ubDifficultyLevel + Random(2));
        curr.ubAdjacentSectors |= SOUTH_ADJACENT_SECTOR;

        //P3
        curr = NewUndergroundNode(3, 16, 1);
        switch (gGameOptions.ubDifficultyLevel)
        {
            case DifficultyLevel.Easy:
                curr.ubNumElites = (8 + Random(3));
                break;
            case DifficultyLevel.Medium:
                curr.ubNumElites = (10 + Random(5));
                break;
            case DifficultyLevel.Hard:
                curr.ubNumElites = (14 + Random(6));
                break;
        }
        curr.ubAdjacentSectors |= NORTH_ADJACENT_SECTOR;

        //Do all of the mandatory underground mine sectors

        //Drassen's mine
        //D13_B1
        curr = NewUndergroundNode(13, 4, 1);
        curr.ubAdjacentSectors |= SOUTH_ADJACENT_SECTOR;
        //E13_B1
        curr = NewUndergroundNode(13, 5, 1);
        curr.ubAdjacentSectors |= NORTH_ADJACENT_SECTOR;
        //E13_B2
        curr = NewUndergroundNode(13, 5, 2);
        curr.ubAdjacentSectors |= SOUTH_ADJACENT_SECTOR;
        //F13_B2
        curr = NewUndergroundNode(13, 6, 2);
        curr.ubAdjacentSectors |= NORTH_ADJACENT_SECTOR | SOUTH_ADJACENT_SECTOR;
        //G13_B2
        curr = NewUndergroundNode(13, 7, 2);
        curr.ubAdjacentSectors |= NORTH_ADJACENT_SECTOR;
        //G13_B3
        curr = NewUndergroundNode(13, 7, 3);
        curr.ubAdjacentSectors |= NORTH_ADJACENT_SECTOR;
        //F13_B3
        curr = NewUndergroundNode(13, 6, 3);
        curr.ubAdjacentSectors |= SOUTH_ADJACENT_SECTOR;

        //Cambria's mine
        //H8_B1
        curr = NewUndergroundNode(8, 8, 1);
        curr.ubAdjacentSectors |= EAST_ADJACENT_SECTOR;
        //H9_B1
        curr = NewUndergroundNode(9, MAP_ROW.H, 1);
        curr.ubAdjacentSectors |= WEST_ADJACENT_SECTOR;
        //H9_B2
        curr = NewUndergroundNode(9, 8, 2);
        curr.ubAdjacentSectors |= WEST_ADJACENT_SECTOR;
        //H8_B2
        curr = NewUndergroundNode(8, 8, 2);
        curr.ubAdjacentSectors |= EAST_ADJACENT_SECTOR;
        //H8_B3
        curr = NewUndergroundNode(8, 8, 3);
        curr.ubAdjacentSectors |= SOUTH_ADJACENT_SECTOR;
        //I8_B3
        curr = NewUndergroundNode(8, 9, 3);
        curr.ubAdjacentSectors |= NORTH_ADJACENT_SECTOR | SOUTH_ADJACENT_SECTOR;
        //J8_B3
        curr = NewUndergroundNode(8, 10, 3);
        curr.ubAdjacentSectors |= NORTH_ADJACENT_SECTOR;

        //Alma's mine
        //I14_B1
        curr = NewUndergroundNode(14, 9, 1);
        curr.ubAdjacentSectors |= SOUTH_ADJACENT_SECTOR;
        //J14_B1
        curr = NewUndergroundNode(14, 10, 1);
        curr.ubAdjacentSectors |= NORTH_ADJACENT_SECTOR;
        //J14_B2
        curr = NewUndergroundNode(14, 10, 2);
        curr.ubAdjacentSectors |= WEST_ADJACENT_SECTOR;
        //J13_B2
        curr = NewUndergroundNode(13, 10, 2);
        curr.ubAdjacentSectors |= EAST_ADJACENT_SECTOR;
        //J13_B3
        curr = NewUndergroundNode(13, 10, 3);
        curr.ubAdjacentSectors |= SOUTH_ADJACENT_SECTOR;
        //K13_B3
        curr = NewUndergroundNode(13, 11, 3);
        curr.ubAdjacentSectors |= NORTH_ADJACENT_SECTOR;

        //Grumm's mine
        //H3_B1
        curr = NewUndergroundNode(3, 8, 1);
        curr.ubAdjacentSectors |= SOUTH_ADJACENT_SECTOR;
        //I3_B1
        curr = NewUndergroundNode(3, 9, 1);
        curr.ubAdjacentSectors |= NORTH_ADJACENT_SECTOR;
        //I3_B2
        curr = NewUndergroundNode(3, 9, 2);
        curr.ubAdjacentSectors |= NORTH_ADJACENT_SECTOR;
        //H3_B2
        curr = NewUndergroundNode(3, 8, 2);
        curr.ubAdjacentSectors |= SOUTH_ADJACENT_SECTOR | EAST_ADJACENT_SECTOR;
        //H4_B2
        curr = NewUndergroundNode(4, 8, 2);
        curr.ubAdjacentSectors |= WEST_ADJACENT_SECTOR;
        curr.uiFlags |= SF.PENDING_ALTERNATE_MAP;
        //H4_B3
        curr = NewUndergroundNode(4, 8, 3);
        curr.ubAdjacentSectors |= NORTH_ADJACENT_SECTOR;
        //G4_B3
        curr = NewUndergroundNode(4, 7, 3);
        curr.ubAdjacentSectors |= SOUTH_ADJACENT_SECTOR;
    }

    //This is the function that is called only once, when the player begins a new game.  This will calculate
    //starting numbers of the queen's army in various parts of the map, which will vary from campaign to campaign.
    //This is also highly effected by the game's difficulty setting.
    void InitNewCampaign()
    {
        //First clear all the sector information of all enemy existance.  Conveniently, the
        //ubGroupType is also cleared, which is perceived to be an empty group.
        SectorInfo = new(258);
        InitStrategicMovementCosts();
        RemoveAllGroups();

        InitMiningLocations();
        InitKnowFacilitiesFlags();

        BuildUndergroundSectorInfoList();

        // allow overhead view of omerta A9 on game onset
        SetSectorFlag(9, 1, 0, SF.ALREADY_VISITED);

        //Generates the initial forces in a new campaign.  The idea is to randomize numbers and sectors
        //so that no two games are the same.
        InitStrategicAI();

        InitStrategicStatus();

    }

}
