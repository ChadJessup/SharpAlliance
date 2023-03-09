using System;
using System.Collections.Generic;

namespace SharpAlliance.Core.SubSystems;

public class CampaignTypes
{
}

public class SECTORINFO
{
    //information pertaining to this sector
    public SF uiFlags;                     //various special conditions
    public sbyte ubInvestigativeState;     //When the sector is attacked by the player, the state increases by 1 permanently.
                                    //This value determines how quickly it is investigated by the enemy.
    public Garrisons ubGarrisonID;                     //IF the sector has an ID for this (non 255), then the queen values this sector and it
                                            //indexes the garrison group.
    public byte ubPendingReinforcements;   //when the enemy owns this sector, this value will keep track of HIGH priority reinforcements -- not regular.
    public bool fMilitiaTrainingPaid;
    public sbyte ubMilitiaTrainingPercentDone;
    public sbyte ubMilitiaTrainingHundredths;
    //enemy military presence
    public bool[] fPlayer = new bool[4];             //whether the player THINKS the sector is unde his control or not. array is for sublevels
                                              //enemy only info
    public int ubNumTroops;              //the actual number of troops here.
    public int ubNumElites;              //the actual number of elites here.
    public int ubNumAdmins;              //the actual number of admins here.
    public int ubNumCreatures;           //only set when immediately before ground attack made!
    public int ubTroopsInBattle, ubElitesInBattle, ubAdminsInBattle, ubCreaturesInBattle;

    public int bLastKnownEnemies; // -1 means never been there, no idea, otherwise it's what we'd observed most recently
                            // while this is being maintained (partially, surely buggy), nothing uses it anymore. ARM

    public uint ubDayOfLastCreatureAttack;
    public uint uiFacilitiesFlags;   // the flags for various facilities

    public Dictionary<StrategicMove, Traversability> ubTraversability = new();//determines the traversability ratings to adjacent sectors.
                                            //The last index represents the traversability if travelling
                                            //throught the sector without entering it.
    public byte bNameId;
    public byte bUSUSED;
    public int bBloodCats { get; set; }
    public int bBloodCatPlacements;
    public int UNUSEDbSAMCondition;

    public sbyte ubTravelRating;   //Represents how travelled a sector is.  Typically, the higher the travel rating,
                            //the more people go near it.  A travel rating of 0 means there are never people
                            //around.  This value is used for determining how often items would "vanish" from
                            //a sector (nice theory, except it isn't being used that way.  Stealing is only in towns.  ARM)
    public sbyte[] ubNumberOfCivsAtLevel = new sbyte[(int)MilitiaExperience.MAX_MILITIA_LEVELS]; // town militia per experience class, 0/1/2 is GREEN/REGULAR/ELITE
    public ushort usUNUSEDMilitiaLevels;               // unused (ARM)
    public sbyte ubUNUSEDNumberOfJoeBlowCivilians;     // unused (ARM)
    public uint uiTimeCurrentSectorWasLastLoaded;        //Specifies the last time the player was in the sector
    public sbyte ubUNUSEDNumberOfEnemiesThoughtToBeHere;       // using bLastKnownEnemies instead
    public uint uiTimeLastPlayerLiberated; //in game seconds (used to prevent the queen from attacking for awhile)

    public bool fSurfaceWasEverPlayerControlled;

    sbyte bFiller1;
    sbyte bFiller2;
    sbyte bFiller3;

    public int uiNumberOfWorldItemsInTempFileThatCanBeSeenByPlayer;

    byte[] bPadding = new byte[41];

    public static SEC SECTOR(int x, MAP_ROW y) => (SEC)(((int)y - 1) * 16 + x - 1);
    public static int SECTORX(SEC SectorID) => (((int)SectorID % 16) + 1);
    public static MAP_ROW SECTORY(SEC SectorID) => (MAP_ROW)(((int)SectorID / 16) + 1);
}

public enum GroupTypes//group types
{
    NOGROUP,
    MOBILE,
    DEFENCE
};

public enum StrategicValue//strategic values for each sector
{
    NO,
    LOW,
    FAIR,
    AVG,
    GOOD,
    HI,
    GREAT
};


//NOTE: These use the 0-255 SectorInfo[] numbering system, and CAN'T be used as indexes into the StrategicMap[] array
//Use SECTOR_INFO_TO_STRATEGIC_INDEX() macro to convert...
public enum SEC
{
    A1, A2, A3, A4, A5, A6, A7, A8, A9, A10, A11, A12, A13, A14, A15, A16,
    B1, B2, B3, B4, B5, B6, B7, B8, B9, B10, B11, B12, B13, B14, B15, B16,
    C1, C2, C3, C4, C5, C6, C7, C8, C9, C10, C11, C12, C13, C14, C15, C16,
    D1, D2, D3, D4, D5, D6, D7, D8, D9, D10, D11, D12, D13, D14, D15, D16,
    E1, E2, E3, E4, E5, E6, E7, E8, E9, E10, E11, E12, E13, E14, E15, E16,
    F1, F2, F3, F4, F5, F6, F7, F8, F9, F10, F11, F12, F13, F14, F15, F16,
    G1, G2, G3, G4, G5, G6, G7, G8, G9, G10, G11, G12, G13, G14, G15, G16,
    H1, H2, H3, H4, H5, H6, H7, H8, H9, H10, H11, H12, H13, H14, H15, H16,
    I1, I2, I3, I4, I5, I6, I7, I8, I9, I10, I11, I12, I13, I14, I15, I16,
    J1, J2, J3, J4, J5, J6, J7, J8, J9, J10, J11, J12, J13, J14, J15, J16,
    K1, K2, K3, K4, K5, K6, K7, K8, K9, K10, K11, K12, K13, K14, K15, K16,
    L1, L2, L3, L4, L5, L6, L7, L8, L9, L10, L11, L12, L13, L14, L15, L16,
    M1, M2, M3, M4, M5, M6, M7, M8, M9, M10, M11, M12, M13, M14, M15, M16,
    N1, N2, N3, N4, N5, N6, N7, N8, N9, N10, N11, N12, N13, N14, N15, N16,
    O1, O2, O3, O4, O5, O6, O7, O8, O9, O10, O11, O12, O13, O14, O15, O16,
    P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15, P16
}

public enum SF : uint
{
    USE_MAP_SETTINGS = 0x00000001,
    ENEMY_AMBUSH_LOCATION = 0x00000002,

    //Special case flag used when players encounter enemies in a sector, then retreat.  The number of enemies
    //will display on mapscreen until time is compressed.  When time is compressed, the flag is cleared, and
    //a question mark is displayed to reflect that the player no longer knows.
    PLAYER_KNOWS_ENEMIES_ARE_HERE = 0x00000004,
    SAM_SITE = 0x00000008,
    MINING_SITE = 0x00000010,
    ALREADY_VISITED = 0x00000020,
    USE_ALTERNATE_MAP = 0x00000040,
    PENDING_ALTERNATE_MAP = 0x00000080,
    ALREADY_LOADED = 0x00000100,
    HAS_ENTERED_TACTICAL = 0x00000200,
    SKYRIDER_NOTICED_ENEMIES_HERE = 0x00000400,
    HAVE_USED_GUIDE_QUOTE = 0x00000800,
    SMOKE_EFFECTS_TEMP_FILE_EXISTS = 0x00100000,     //Temp File starts with sm_
    LIGHTING_EFFECTS_TEMP_FILE_EXISTS = 0x00200000,      //Temp File starts with l_
    REVEALED_STATUS_TEMP_FILE_EXISTS = 0x01000000,       //Temp File starts with v_
    DOOR_STATUS_TEMP_FILE_EXISTS = 0x02000000,       //Temp File starts with ds_
    ENEMY_PRESERVED_TEMP_FILE_EXISTS = 0x04000000,       //Temp File starts with e_
    CIV_PRESERVED_TEMP_FILE_EXISTS = 0x08000000,     //Temp File starts with c_
    ITEM_TEMP_FILE_EXISTS = 0x10000000,      //Temp File starts with i_
    ROTTING_CORPSE_TEMP_FILE_EXISTS = 0x20000000,        //Temp File starts with r_
    MAP_MODIFICATIONS_TEMP_FILE_EXISTS = 0x40000000,     //Temp File starts with m_
    DOOR_TABLE_TEMP_FILES_EXISTS = 0x80000000,		//Temp File starts with d_

}

// Find a better place for this, it just makes all the NO_XXX easily usable.
public enum NO
{
    GARRISON = 255,
}

// town militia experience categories
public enum MilitiaExperience
{
    GREEN_MILITIA = 0,
    REGULAR_MILITIA,
    ELITE_MILITIA,
    MAX_MILITIA_LEVELS
}


// facilities flags
public enum SFCF
{
    HOSPITAL = 0x00000001,
    INDUSTRY = 0x00000002,
    PRISON = 0x00000004,
    MILITARY = 0x00000008,
    AIRPORT = 0x00000010,
    GUN_RANGE = 0x00000020,
}

public class UNDERGROUND_SECTORINFO
{
    public uint uiFlags;
    public byte ubSectorX, ubSectorZ;
    public MAP_ROW ubSectorY;
    public int ubNumElites, ubNumTroops, ubNumAdmins, ubNumCreatures;
    public int fVisited;
    public sbyte ubTravelRating;    //Represents how travelled a sector is.  Typically, the higher the travel rating,
                                    //the more people go near it.  A travel rating of 0 means there are never people
                                    //around.  This value is used for determining how often items would "vanish" from
                                    //a sector.
    public uint uiTimeCurrentSectorWasLastLoaded;        //Specifies the last time the player was in the sector
    public UNDERGROUND_SECTORINFO? next;
	public byte ubAdjacentSectors;    //mask containing which sectors are adjacent
    public byte ubCreatureHabitat;    //determines how creatures live in this sector (see creature spreading.c)
    public byte ubElitesInBattle, ubTroopsInBattle, ubAdminsInBattle, ubCreaturesInBattle;

    public uint uiNumberOfWorldItemsInTempFileThatCanBeSeenByPlayer;
    public sbyte[] bPadding = new sbyte[36];
    //no padding left!
}


// coordinates of shooting range sector
public enum GUN_RANGE
{
    RANGE_X = 13,
    RANGE_Y = MAP_ROW.H,
    RANGE_Z = 0,
}

//Vehicle types
[Flags]
public enum VehicleTypes
{
    FOOT = 0x01,    //anywhere
    CAR = 0x02,     //roads
    TRUCK = 0x04,   //roads, plains, sparse
    TRACKED = 0x08, //roads, plains, sand, sparse
    AIR = 0x10,     //can traverse all terrains at 100%
}

//Traversability ratings
public enum Traversability
{
    TOWN,               //instant
    ROAD,               //everything travels at 100%
    PLAINS,             //foot 90%, truck 75%, tracked 100%
    SAND,               //foot 70%, tracked 60%
    SPARSE,             //foot 70%, truck 50%, tracked 60%
    DENSE,              //foot 50% 
    SWAMP,              //foot 20%
    WATER,              //foot 15%
    HILLS,              //foot 50%, truck 50%, tracked 50%
    GROUNDBARRIER,      //only air (super dense forest, ocean, etc.)
    NS_RIVER,           //river from north to south
    EW_RIVER,           //river from east to west
    EDGEOFWORLD,        //nobody can traverse.
                        //NEW (not used for border values -- traversal calculations)
    TROPICS,
    FARMLAND,
    PLAINS_ROAD,
    SPARSE_ROAD,
    FARMLAND_ROAD,
    TROPICS_ROAD,
    DENSE_ROAD,
    COASTAL,
    HILLS_ROAD,
    COASTAL_ROAD,
    SAND_ROAD,
    SWAMP_ROAD,
    //only used for text purposes and not assigned to areas (SAM sites are hard coded throughout the code)
    SPARSE_SAM_SITE, //D15 near Drassen
    SAND_SAM_SITE,   //I8 near Tixa
    TROPICS_SAM_SITE, //D2 near Chitzena
    MEDUNA_SAM_SITE, //N4 in Meduna
    CAMBRIA_HOSPITAL_SITE,
    DRASSEN_AIRPORT_SITE,
    MEDUNA_AIRPORT_SITE,
    SAM_SITE,
    REBEL_HIDEOUT,
    TIXA_DUNGEON,
    CREATURE_LAIR,
    ORTA_BASEMENT,
    TUNNEL,
    SHELTER,
    ABANDONED_MINE,

    NUM_TRAVTERRAIN_TYPES
}

public enum TRAVELRATING
{
    NONE = 0,
    LOW = 25,
    NORMAL = 50,
    HIGH = 75,
    EXTREME = 100,
}
