using SharpAlliance.Core;
using SharpAlliance.Core.SubSystems;
using System;
using System.Collections.Generic;

namespace SharpAlliance.Core;

public class PROFILE
{
    public const int X_SIZE = 5;
    public const int Y_SIZE = 5;
    public const int Z_SIZE = 4;

    private readonly int[,] profile = new int[X_SIZE, Y_SIZE];

    public int this[int x, int y]
    {
        get => this.profile[x, y];
        set => this.profile[x, y] = value;
    }
}

public enum WorldDefines
{
    CELL_X_SIZE = 10,
    CELL_Y_SIZE = 10,

    WORLD_BASE_HEIGHT = 0,
    WORLD_CLIFF_HEIGHT = 80,
}

[Flags]
public enum LEVELNODEFLAGS : uint
{
    SOLDIER = 0x00000001,
    UNUSED2 = 0x00000002,
    MERCPLACEHOLDER = 0x00000004,
    SHOW_THROUGH = 0x00000008,
    NOZBLITTER = 0x00000010,
    CACHEDANITILE = 0x00000020,
    ROTTINGCORPSE = 0x00000040,
    BUDDYSHADOW = 0x00000080,
    HIDDEN = 0x00000100,
    USERELPOS = 0x00000200,
    DISPLAY_AP = 0x00000400,
    ANIMATION = 0x00000800,
    USEABSOLUTEPOS = 0x00001000,
    REVEAL = 0x00002000,
    REVEALTREES = 0x00004000,
    USEBESTTRANSTYPE = 0x00008000,
    USEZ = 0x00010000,
    DYNAMICZ = 0x00020000,
    UPDATESAVEBUFFERONCE = 0x00040000,
    ERASEZ = 0x00080000,
    WIREFRAME = 0x00100000,
    ITEM = 0x00200000,
    IGNOREHEIGHT = 0x00400000,
    DYNAMIC = 0x02000000,
    LASTDYNAMIC = 0x04000000,
    PHYSICSOBJECT = 0x08000000,
    NOWRITEZ = 0x10000000,
    MULTITILESOLDIER = 0x20000000,
    EXITGRID = 0x40000000,
    CAVE = 0x80000000,
}

[Flags]
public enum MAPELEMENT_EXT
{
    EXT_SMOKE = 0x01,
    TEARGAS = 0x02,
    MUSTARDGAS = 0x04,
    DOOR_STATUS_PRESENT = 0x08,
    RECALCULATE_MOVEMENT = 0x10,
    NOBURN_STRUCT = 0x20,
    ROOFCODE_VISITED = 0x40,
    CREATUREGAS = 0x80,
}

// This was a C struct with lots of unions, but in C# you can't reference a struct inside itself.
public class LEVELNODE
{
    public LEVELNODE? pNext;
    public LEVELNODEFLAGS uiFlags;                         // flags struct

    public int ubSumLights;                  // LIGHTING INFO
    public int ubMaxLights;                  // MAX LIGHTING INFO

    LEVELNODE? pPrevNode;                    // FOR LAND, GOING BACKWARDS POINTER
    public STRUCTURE? pStructureData;      // STRUCTURE DATA
    public int iPhysicsObjectID;     // ID FOR PHYSICS ITEM
    public int uiAPCost;                     // FOR AP DISPLAY
    public int iExitGridInfo;

    public int usIndex;                         // TILE DATABASE INDEX
    public int sCurrentFrame;                // Stuff for animated tiles for a given tile location ( doors, etc )

    public SOLDIERTYPE? pSoldier;                          // POINTER TO SOLDIER

    // Some levelnodes can specify relative X and Y values!
    public int sRelativeX;                           // Relative position values
    public int sRelativeY;                           // Relative position values

    // Some can contains index values into dead corpses
    public int iCorpseID;                            // Index into corpse ID

    public uint uiAnimHitLocationFlags;  // Animation profile flags for soldier placeholders ( prone merc hit location values )

    // Some can contains index values into animated tile data
    public ANITILE? pAniTile;

    // Can be an item pool as well...
    public ITEM_POOL? pItemPool;                   // ITEM POOLS

    //
    public int sRelativeZ;                           // Relative position values
    public int ubShadeLevel;                     // LIGHTING INFO
    public int ubNaturalShadeLevel;      // LIGHTING INFO
    public int ubFakeShadeLevel;				// LIGHTING INFO
};

public class ITEM_POOL
{
    public ITEM_POOL? pNext;
    public ITEM_POOL? pPrev;

    public int iItemIndex;
    byte bVisible;
    byte bFlashColor;
    uint uiTimerID;
    short sGridNo;
    byte ubLevel;
    short usFlags;
    byte bRenderZHeightAboveLevel;
    LEVELNODE? pLevelNode;
}

public class STRUCTURE
{
    public STRUCTURE? pPrev;
    public STRUCTURE? pNext;
    public int sGridNo;
    public int usStructureID;
    public DB_STRUCTURE_REF pDBStructureRef { get; set; } = new();

    public byte ubHitPoints;
    byte ubLockStrength;

    public int sBaseGridNo { get; set; }

    public int sCubeOffset;// height of bottom of object in profile "cubes"
    public STRUCTUREFLAGS fFlags; // need to have something to indicate base tile/not
    public PROFILE? pShape;
    public WallOrientation ubWallOrientation;
    public byte ubVehicleHitLocation;
    byte ubStructureHeight; // if 0, then unset; otherwise stores height of structure when last calculated
    byte ubUnused;
}

public class DB_STRUCTURE
{
    public byte ubArmour;
    public byte ubDensity;
    public byte ubHitPoints;
    public byte ubNumberOfTiles;
    public STRUCTUREFLAGS fFlags;
    ushort usStructureNumber;
    public WallOrientation ubWallOrientation;
    public sbyte bDestructionPartner; // >0 = debris number (bDP - 1), <0 = partner graphic 
    public sbyte bPartnerDelta; // opened/closed version, etc... 0 for unused
    public sbyte bZTileOffsetX;
    public sbyte bZTileOffsetY;
    public byte bUnused;
} // 16 bytes

public class DB_STRUCTURE_REF
{
    public DB_STRUCTURE pDBStructure;
    public List<DB_STRUCTURE_TILE> ppTile; // dynamic array
} // 8 bytes

public class DB_STRUCTURE_TILE
{
    public short sPosRelToBase;  // "single-axis"
    public sbyte bXPosRelToBase;
    public sbyte bYPosRelToBase;
    public PROFILE Shape;                  // 25 bytes
    public TILE fFlags { get; set; }
    public byte ubVehicleHitLocation;
    byte bUnused;
} // 32 bytes

public class MAP_ELEMENT
{
    public LEVELNODE? pLandHead;                           //0
    public LEVELNODE? pLandStart;                      //1
    public LEVELNODE? pObjectHead;                     //2
    public LEVELNODE? pStructHead;                     //3
    public LEVELNODE? pShadowHead;                     //4
    public LEVELNODE? pMercHead;                           //5
    public LEVELNODE? pRoofHead;                           //6
    public LEVELNODE? pOnRoofHead { get; set; } = new();   //7
    public LEVELNODE pTopmostHead { get; set; } = new();  //8
    public LEVELNODE[] pLevelNodes = new LEVELNODE[9];

    public STRUCTURE? pStructureHead;
    public STRUCTURE? pStructureTail;

    public MAPELEMENTFLAGS uiFlags;
    public MAPELEMENT_EXT[] ubExtFlags = new MAPELEMENT_EXT[2];
    public ushort[] sSumRealLights = new ushort[1];
    public byte sHeight;
    public byte ubAdjacentSoldierCnt;
    public TerrainTypeDefines ubTerrainID;

    public int ubReservedSoldierID;
    public int ubBloodInfo;
    public int ubSmellInfo;
}

public enum MAPELEMENTFLAGS
{
    // THE FIRST FEW ( 4 ) bits are flags which are saved in the world
    REDUNDENT = 0x0001,
    REEVALUATE_REDUNDENCY = 0x0002,
    ENEMY_MINE_PRESENT = 0x0004,
    PLAYER_MINE_PRESENT = 0x0008,
    STRUCTUREFLAGS_DAMAGED = 0x0010,
    REEVALUATEBLOOD = 0x0020,
    INTERACTIVETILE = 0x0040,
    RAISE_LAND_START = 0x0080,
    REVEALED = 0x0100,
    RAISE_LAND_END = 0x0200,
    REDRAW = 0x0400,
    REVEALED_ROOF = 0x0800,
    MOVEMENT_RESERVED = 0x1000,
    RECALCULATE_WIREFRAMES = 0x2000,
    ITEMPOOL_PRESENT = 0x4000,
    REACHABLE = 0x8000,
    EXT_SMOKE = 0x01,
    EXT_TEARGAS = 0x02,
    EXT_MUSTARDGAS = 0x04,
    EXT_DOOR_STATUS_PRESENT = 0x08,
    EXT_RECALCULATE_MOVEMENT = 0x10,
    EXT_NOBURN_STRUCT = 0x20,
    EXT_ROOFCODE_VISITED = 0x40,
    EXT_CREATUREGAS = 0x80,
}
