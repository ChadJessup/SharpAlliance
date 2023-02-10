﻿using SharpAlliance.Core.SubSystems;
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
    ushort usStructureID;
    DB_STRUCTURE_REF? pDBStructureRef;

    byte ubHitPoints;
    byte ubLockStrength;

    short sBaseGridNo;

    short sCubeOffset;// height of bottom of object in profile "cubes"
    uint fFlags; // need to have something to indicate base tile/not
    PROFILE? pShape;
    byte ubWallOrientation;
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

