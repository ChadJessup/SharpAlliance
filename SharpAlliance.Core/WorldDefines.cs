using SharpAlliance.Core.SubSystems;
using System;

namespace SharpAlliance.Core;

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

// This was a C struct with lots of unions, but in C# you can't reference a struct inside itself.
public class LEVELNODE
{
    public LEVELNODE? pNext;
    public int uiFlags;                         // flags struct

    int ubSumLights;                  // LIGHTING INFO
    int ubMaxLights;                  // MAX LIGHTING INFO

    LEVELNODE? pPrevNode;                    // FOR LAND, GOING BACKWARDS POINTER
    STRUCTURE? pStructureData;      // STRUCTURE DATA
    public int iPhysicsObjectID;     // ID FOR PHYSICS ITEM
    public int uiAPCost;                     // FOR AP DISPLAY
    public int iExitGridInfo;

    uint usIndex;                         // TILE DATABASE INDEX
    int sCurrentFrame;                // Stuff for animated tiles for a given tile location ( doors, etc )

    SOLDIERTYPE? pSoldier;                          // POINTER TO SOLDIER

    // Some levelnodes can specify relative X and Y values!
    int sRelativeX;                           // Relative position values
    int sRelativeY;                           // Relative position values

    // Some can contains index values into dead corpses
    int iCorpseID;                            // Index into corpse ID

    uint uiAnimHitLocationFlags;  // Animation profile flags for soldier placeholders ( prone merc hit location values )

    // Some can contains index values into animated tile data
    TAG_anitile? pAniTile;

    // Can be an item pool as well...
    ITEM_POOL? pItemPool;                   // ITEM POOLS

    //
    int sRelativeZ;                           // Relative position values
    byte ubShadeLevel;                     // LIGHTING INFO
    byte ubNaturalShadeLevel;      // LIGHTING INFO
    byte ubFakeShadeLevel;				// LIGHTING INFO
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
    INT16 sGridNo;
    UINT16 usStructureID;
    DB_STRUCTURE_REF? pDBStructureRef;

    UINT8 ubHitPoints;
    UINT8 ubLockStrength;

    INT16 sBaseGridNo;

    INT16 sCubeOffset;// height of bottom of object in profile "cubes"
    UINT32 fFlags; // need to have something to indicate base tile/not
    PROFILE? pShape;
    UINT8 ubWallOrientation;
    UINT8 ubVehicleHitLocation;
    UINT8 ubStructureHeight; // if 0, then unset; otherwise stores height of structure when last calculated
    UINT8 ubUnused;
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
