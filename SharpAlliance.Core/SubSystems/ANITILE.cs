using System;

using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core.SubSystems;

public class ANITILE
{
    public object uiUserData { get; set; }
    public int sCachedTileID { get; internal set; } // Index into cached tile ID
    public ANITILE? pNext;
    public ANITILEFLAGS uiFlags;                         // flags struct
    public uint uiTimeLastUpdate;            // Stuff for animated tiles
    public LEVELNODE pLevelNode = new();
    public ANI ubLevelID;
    public int sCurrentFrame;
    public int sStartFrame;
    public int sDelay;
    public int usTileType;
    public int usNumFrames;
    public int usMissAnimationPlayed;
    public int ubAttackerMissed;
    public int sRelativeX;
    public MAP_ROW sRelativeY;
    public int sRelativeZ;
    public int sGridNo;
    public TileIndexes usTileIndex;
    public int usCachedTileSubIndex;        // sub Index 
    public int ubOwner;
    public int ubKeyFrame1;
    public ANI_KEYFRAME uiKeyFrame1Code;
    public int ubKeyFrame2;
    public ANI_KEYFRAME uiKeyFrame2Code;
    public int ubUserData2;
    public WorldDirections uiUserData3;
    public int bFrameCountAfterStart;
}

public struct ANITILE_PARAMS
{
    public ANITILEFLAGS uiFlags;                         // flags struct
    public ANI ubLevelID;                        // Level ID for rendering layer
    public int sStartFrame;                  // Start frame
    public int sDelay;                               // Delay time
    public TileTypeDefines usTileType;                      // Tile databse type ( optional )
    public TileIndexes usTileIndex;                 // Tile database index ( optional )
    public int sX;                                       // World X ( optional )
    public MAP_ROW sY;                                       // World Y ( optional )
    public int sZ;                                       // World Z ( optional )
    public int sGridNo;                          // World GridNo
    public LEVELNODE? pGivenLevelNode;         // Levelnode for existing tile ( optional )
    public string zCachedFile;                  // Filename for cached tile name ( optional )
    public int ubOwner;                          // UBID for the owner
    public int ubKeyFrame1;                  // Key frame 1
    public ANI_KEYFRAME uiKeyFrame1Code;         // Key frame code
    public int ubKeyFrame2;                  // Key frame 2
    public ANI_KEYFRAME uiKeyFrame2Code;         // Key frame code
    public object uiUserData;
    public int ubUserData2;
    public WorldDirections uiUserData3;
}

[Flags]
public enum ANITILEFLAGS
{
    DOOR = 0x00000001,
    BACKWARD = 0x00000020,
    FORWARD = 0x00000040,
    PAUSED = 0x00000200,
    EXISTINGTILE = 0x00000400,
    USEABSOLUTEPOS = 0x00004000,
    CACHEDTILE = 0x00008000,
    LOOPING = 0x00020000,
    NOZBLITTER = 0x00040000,
    REVERSE_LOOPING = 0x00080000,
    ALWAYS_TRANSLUCENT = 0x00100000,
    USEBEST_TRANSLUCENT = 0x00200000,
    OPTIMIZEFORSLOWMOVING = 0x00400000,
    ANIMATE_Z = 0x00800000,
    USE_DIRECTION_FOR_START_FRAME = 0x01000000,
    PAUSE_AFTER_LOOP = 0x02000000,
    ERASEITEMFROMSAVEBUFFFER = 0x04000000,
    OPTIMIZEFORSMOKEEFFECT = 0x08000000,
    SMOKE_EFFECT = 0x10000000,
    EXPLOSION = 0x20000000,
    RELEASE_ATTACKER_WHEN_DONE = 0x40000000,
    USE_4DIRECTION_FOR_START_FRAME = 0x02000000,
}

public enum ANI
{
    LAND_LEVEL = 1,
    SHADOW_LEVEL = 2,
    OBJECT_LEVEL = 3,
    STRUCT_LEVEL = 4,
    ROOF_LEVEL = 5,
    ONROOF_LEVEL = 6,
    TOPMOST_LEVEL = 7,
}
