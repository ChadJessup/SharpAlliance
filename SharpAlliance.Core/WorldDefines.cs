using SharpAlliance.Core;
using SharpAlliance.Core.SubSystems;
using System;
using System.Collections.Generic;

namespace SharpAlliance.Core;

public class PROFILE
{
    const int PROFILE_X_SIZE = 5;
    const int PROFILE_Y_SIZE = 5;
    const int PROFILE_Z_SIZE = 4;

    private readonly int[,] profile = new int[PROFILE_X_SIZE, PROFILE_Y_SIZE];

    public int this[int x, int y]
    {
        get => this.profile[x, y];
        set => this.profile[x, y] = value;
    }
}

public enum WorldDefines
{
    WORLD_TILE_X = 40,
    WORLD_TILE_Y = 20,
    WORLD_COLS = 160,
    WORLD_ROWS = 160,
    WORLD_COORD_COLS = 1600,
    WORLD_COORD_ROWS = 1600,
    WORLD_MAX = 25600,
    CELL_X_SIZE = 10,
    CELL_Y_SIZE = 10,

    WORLD_BASE_HEIGHT = 0,
    WORLD_CLIFF_HEIGHT = 80,
}

[Flags]
public enum LEVELNODEFlags :uint
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

// This was a C struct with lots of unions, but in C# you can't reference a struct inside itself.
public class LEVELNODE
{
    public LEVELNODE? pNext;
    public LEVELNODEFlags uiFlags;                         // flags struct

    int ubSumLights;                  // LIGHTING INFO
    int ubMaxLights;                  // MAX LIGHTING INFO

    LEVELNODE? pPrevNode;                    // FOR LAND, GOING BACKWARDS POINTER
    public STRUCTURE? pStructureData;      // STRUCTURE DATA
    public int iPhysicsObjectID;     // ID FOR PHYSICS ITEM
    public int uiAPCost;                     // FOR AP DISPLAY
    public int iExitGridInfo;

    public ushort usIndex;                         // TILE DATABASE INDEX
    int sCurrentFrame;                // Stuff for animated tiles for a given tile location ( doors, etc )

    public SOLDIERTYPE? pSoldier;                          // POINTER TO SOLDIER

    // Some levelnodes can specify relative X and Y values!
    public int sRelativeX;                           // Relative position values
    public int sRelativeY;                           // Relative position values

    // Some can contains index values into dead corpses
    int iCorpseID;                            // Index into corpse ID

    uint uiAnimHitLocationFlags;  // Animation profile flags for soldier placeholders ( prone merc hit location values )

    // Some can contains index values into animated tile data
    TAG_anitile? pAniTile;

    // Can be an item pool as well...
    ITEM_POOL? pItemPool;                   // ITEM POOLS

    //
    int sRelativeZ;                           // Relative position values
    public int ubShadeLevel;                     // LIGHTING INFO
    public int ubNaturalShadeLevel;      // LIGHTING INFO
    public int ubFakeShadeLevel;				// LIGHTING INFO
};

public class ITEM_POOL
{
    ITEM_POOL? pNext;
    ITEM_POOL? pPrev;

    int iItemIndex;
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
    STRUCTURE? pPrev;
    STRUCTURE? pNext;
    short sGridNo;
    public ushort usStructureID;
    DB_STRUCTURE_REF? pDBStructureRef;

    byte ubHitPoints;
    byte ubLockStrength;

    short sBaseGridNo;

    short sCubeOffset;// height of bottom of object in profile "cubes"
    public uint fFlags; // need to have something to indicate base tile/not
    PROFILE? pShape;
    public WallOrientation ubWallOrientation;
    byte ubVehicleHitLocation;
    byte ubStructureHeight; // if 0, then unset; otherwise stores height of structure when last calculated
    byte ubUnused;
}


struct DB_STRUCTURE
{
    byte ubArmour;
    byte ubHitPoints;
    byte ubDensity;
    byte ubNumberOfTiles;
    uint fFlags;
    ushort usStructureNumber;
    byte ubWallOrientation;
    sbyte bDestructionPartner; // >0 = debris number (bDP - 1), <0 = partner graphic 
    sbyte bPartnerDelta; // opened/closed version, etc... 0 for unused
    sbyte bZTileOffsetX;
    sbyte bZTileOffsetY;
    byte bUnused;
} // 16 bytes

struct DB_STRUCTURE_REF
{
    DB_STRUCTURE pDBStructure;
    List<DB_STRUCTURE_TILE> ppTile; // dynamic array
} // 8 bytes

struct DB_STRUCTURE_TILE
{
    short sPosRelToBase;  // "single-axis"
    sbyte bXPosRelToBase;
    sbyte bYPosRelToBase;
    PROFILE Shape;                  // 25 bytes
    byte fFlags;
    byte ubVehicleHitLocation;
    byte bUnused;
} // 32 bytes

public class MAP_ELEMENT
{
    LEVELNODE? pLandHead;                           //0
    LEVELNODE? pLandStart;                      //1
    public LEVELNODE? pObjectHead;                     //2
    LEVELNODE? pStructHead;                     //3
    LEVELNODE? pShadowHead;                     //4
    LEVELNODE? pMercHead;                           //5
    LEVELNODE? pRoofHead;                           //6
    public LEVELNODE pOnRoofHead { get; set; } = new();   //7
    public LEVELNODE pTopmostHead { get; set; } = new();  //8
    LEVELNODE?[] pLevelNodes;                 // [9];

    STRUCTURE? pStructureHead;
    STRUCTURE? pStructureTail;

    public ushort uiFlags;
    byte[] ubExtFlags;// [2];
    ushort[] sSumRealLights = new ushort[1];// [1];
    public byte sHeight;
    byte ubAdjacentSoldierCnt;
    byte ubTerrainID;

    byte ubReservedSoldierID;
    byte ubBloodInfo;
    byte ubSmellInfo;
}
