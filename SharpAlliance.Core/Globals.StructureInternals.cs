using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpAlliance.Core.Managers;
using SharpAlliance.Core.SubSystems;

namespace SharpAlliance.Core;

public partial class Globals
{
    // "J2SD" = Jagged 2 Structure Data
    public const string STRUCTURE_FILE_ID = "J2SD";
    public const string STRUCTURE_SCRIPT_FILE_EXTENSION = "JSS";
    public const string STRUCTURE_FILE_EXTENSION = "JSD";
    public const int STRUCTURE_FILE_ID_LEN = 4;

    // A few words about the overall structure scheme:
    //
    // Large structures are split into multiple sections,
    // one for each tile.
    // 
    // Each section is treated as a separate object,
    // except that it does NOT record information about
    // hit points, but instead stores a pointer to the
    // base object (section).
    //
    // Each section has a line of sight profile.  These
    // profiles are split into 5 in each horizontal direction
    // and 4 vertically, forming 100 "cubes".  In real
    // world terms, each section represents a volume
    // with a height of 8 feet (and width and length
    // of what?)
    //
    // It is important to note that the vertical
    // position of each section is measured in individual
    // cubes (rather than, as it were, groups of 4 vertical
    // cubes)

    public const int PROFILE_X_SIZE = 5;
    public const int PROFILE_Y_SIZE = 5;
    public const int PROFILE_Z_SIZE = 4;

    public const int NO_PARTNER_STRUCTURE = 0;

    /*
    * NB:  STRUCTURE_SPECIAL
    *
    * Means different things depending on the context.
    * 
    * WALLNWINDOW SPECIAL - opaque to sight
    * MULTI SPECIAL - second level (damaged) MULTI structure, should only be deleted if 
    *    starting with the deletion of a MULTI SPECIAL structure
    */

    public const int INVALID_STRUCTURE_ID = Globals.TOTAL_SOLDIERS + 100;
    public static int[] AtHeight = { 0x01, 0x02, 0x04, 0x08 };
    public static STRUCTURE_FILE_REF gpStructureFileRefs;

    public static SoundDefine[] guiMaterialHitSound =
    {
        (SoundDefine)(-1),
        SoundDefine.S_WOOD_IMPACT1,
        SoundDefine.S_WOOD_IMPACT2,
        SoundDefine.S_WOOD_IMPACT3,
        SoundDefine.S_VEG_IMPACT1,
        (SoundDefine)(-1),
        SoundDefine.S_PORCELAIN_IMPACT1,
        (SoundDefine)(-1),
        (SoundDefine)(-1),
        (SoundDefine)(-1),
        (SoundDefine)(-1),
        SoundDefine.S_STONE_IMPACT1,
        SoundDefine.S_STONE_IMPACT1,
        SoundDefine.S_STONE_IMPACT1,
        SoundDefine.S_STONE_IMPACT1,
        SoundDefine.S_RUBBER_IMPACT1,
        (SoundDefine)(-1),
        (SoundDefine)(-1),
        (SoundDefine)(-1),
        (SoundDefine)(-1),
        (SoundDefine)(-1),
        SoundDefine.S_METAL_IMPACT1,
        SoundDefine.S_METAL_IMPACT2,
        SoundDefine.S_METAL_IMPACT3,
        SoundDefine.S_STONE_IMPACT1,
        SoundDefine.S_METAL_IMPACT3,
    };

    /*
    index  1-10, organics
    index 11-20, rocks and concretes
    index 21-30, metals

    index 1, dry timber
    index 2, furniture wood
    index 3, tree wood
    index 11, stone masonry
    index 12, non-reinforced concrete
    index 13, reinforced concrete
    index 14, rock
    index 21, light metal (furniture)
    index 22, heavy metal (doors etc)
    index 23, really heavy metal
    index 24, indestructable stone
    index 25, indestructable metal
    */
    public static Dictionary<MATERIAL, int> gubMaterialArmour = new()
    {
        // note: must increase; r.c. should block *AP* 7.62mm rounds
    	{ MATERIAL.NOTHING,  0 },		// nothing
    	{ MATERIAL.WOOD_WALL, 25 },		// dry timber; wood wall +1/2
    	{ MATERIAL.PLYWOOD_WALL, 20 },		// furniture wood (thin!) or plywood wall +1/2
    	{ MATERIAL.LIVE_WOOD, 30 },		// wood (live); 1.5x timber
    	{ MATERIAL.LIGHT_VEGETATION,  3 },		// light vegetation
    	{ MATERIAL.FURNITURE, 10 },		// upholstered furniture
    	{ MATERIAL.PORCELAIN, 47 },		// porcelain
    	{ MATERIAL.CACTUS, 10 },		// cactus, hay, bamboo
        { MATERIAL.NOTUSED1,  0 },
        { MATERIAL.NOTUSED2,  0 },
        { MATERIAL.NOTUSED3,  0 },
        { MATERIAL.STONE, 55 },		// stone masonry; 3x timber
    	{ MATERIAL.CONCRETE1, 63 },		// non-reinforced concrete; 4x timber???
        { MATERIAL.CONCRETE2, 70 },		// reinforced concrete; 6x timber
        { MATERIAL.ROCK, 85 },		// rock? - number invented
        { MATERIAL.RUBBER,  9 },		// rubber - tires
        { MATERIAL.SAND, 40 },		// sand
        { MATERIAL.CLOTH,  1 },	// cloth
        { MATERIAL.SANDBAG, 40 },		// sandbag
        { MATERIAL.NOTUSED5,  0 },
        { MATERIAL.NOTUSED6,  0 },
        { MATERIAL.LIGHT_METAL,  37 },		// light metal (furniture; NB thin!)
        { MATERIAL.THICKER_METAL,  57 },		// thicker metal (dumpster)
        { MATERIAL.HEAVY_METAL,  85 },		// heavy metal (vault doors) - block everything
        // note that vehicle armour will probably end up in here
        { MATERIAL.INDESTRUCTABLE_STONE, 127 },	// rock indestructable
        { MATERIAL.INDESTRUCTABLE_METAL, 127 },	// indestructable
        { MATERIAL.THICKER_METAL_WITH_SCREEN_WINDOWS, 57  },		// like 22 but with screen windows
    };

    public const int BASE_TILE = (int)STRUCTUREFLAGS.BASE_TILE;
}


// these values should be compared for less than rather than less
// than or equal to
public enum STRUCTURE_ON
{
    GROUND = 0,
    ROOF = Globals.PROFILE_Z_SIZE,
    GROUND_MAX = Globals.PROFILE_Z_SIZE,
    ROOF_MAX = Globals.PROFILE_Z_SIZE * 2,
}

// Material armour type enumeration
public enum MATERIAL : byte
{
    NOTHING,
    WOOD_WALL,
    PLYWOOD_WALL,
    LIVE_WOOD,
    LIGHT_VEGETATION,
    FURNITURE,
    PORCELAIN,
    CACTUS,
    NOTUSED1,
    NOTUSED2,
    NOTUSED3,
    STONE,
    CONCRETE1,
    CONCRETE2,
    ROCK,
    RUBBER,
    SAND,
    CLOTH,
    SANDBAG,
    NOTUSED5,
    NOTUSED6,
    LIGHT_METAL,
    THICKER_METAL,
    HEAVY_METAL,
    INDESTRUCTABLE_STONE,
    INDESTRUCTABLE_METAL,
    THICKER_METAL_WITH_SCREEN_WINDOWS,

    NUM_MATERIAL_TYPES
};

[Flags]
public enum STRUCTURE_FILE_CONTAINS : byte
{
    AUXIMAGEDATA = 0x01,
    STRUCTUREDATA = 0x02,
}
