using System;

namespace SharpAlliance.Core.SubSystems;

public class StructureInternals
{
}

[Flags]
public enum STRUCTUREFLAGS : uint
{
    // NOT used in DB structures!
    BASE_TILE = 0x00000001,
    OPEN = 0x00000002,
    OPENABLE = 0x00000004,
    // synonyms for OPENABLE
    CLOSEABLE = 0x00000004,
    SEARCHABLE = 0x00000004,
    HIDDEN = 0x00000008,
    MOBILE = 0x00000010,
    // PASSABLE is set for each structure instance where
    // the tile flag TILE_PASSABLE is set
    PASSABLE = 0x00000020,
    EXPLOSIVE = 0x00000040,
    TRANSPARENT = 0x00000080,
    GENERIC = 0x00000100,
    TREE = 0x00000200,
    FENCE = 0x00000400,
    WIREFENCE = 0x00000800,
    HASITEMONTOP = 0x00001000,            // ATE: HASITEM: struct has item on top of it
    SPECIAL = 0x00002000,
    LIGHTSOURCE = 0x00004000,
    VEHICLE = 0x00008000,
    WALL = 0x00010000,
    WALLNWINDOW = 0x00020000,
    SLIDINGDOOR = 0x00040000,
    DOOR = 0x00080000,

    // a "multi" structure (as opposed to multitiled) is composed of multiple graphics & structures
    MULTI = 0x00100000,
    CAVEWALL = 0x00200000,
    DDOOR_LEFT = 0x00400000,
    DDOOR_RIGHT = 0x00800000,
    NORMAL_ROOF = 0x01000000,
    SLANTED_ROOF = 0x02000000,
    TALL_ROOF = 0x04000000,
    SWITCH = 0x08000000,
    ON_LEFT_WALL = 0x10000000,
    ON_RIGHT_WALL = 0x20000000,
    CORPSE = 0x40000000,
    PERSON = 0x80000000,

    // COMBINATION FLAGS
    ANYFENCE = 0x00000C00,
    ANYDOOR = 0x00CC0000,
    OBSTACLE = 0x00008F00,
    WALLSTUFF = 0x00CF0000,
    BLOCKSMOVES = 0x00208F00,
    TYPE_DEFINED = 0x8FEF8F00,
    ROOF = 0x07000000,
}
