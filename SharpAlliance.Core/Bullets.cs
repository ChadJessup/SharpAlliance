using System;
using SharpAlliance.Core.SubSystems;

namespace SharpAlliance.Core;

public class Bullets
{

}

public struct BULLET
{
    public int iBullet;
    public int ubFirerID;
    public int ubTargetID;
    public int bStartCubesAboveLevelZ;
    public int bEndCubesAboveLevelZ;
    public int sGridNo;
    public int sUnused;
    public int usLastStructureHit;
    public int qCurrX;
    public int qCurrY;
    public int qCurrZ;
    public int qIncrX;
    public int qIncrY;
    public int qIncrZ;
    public double ddHorizAngle;
    public int iCurrTileX;
    public int iCurrTileY;
    public int bLOSIndexX;
    public int bLOSIndexY;
    public int iCurrCubesZ;
    public int iLoop;
    public bool fCheckForRoof;
    public bool fAllocated;
    public bool fToDelete;
    public bool fLocated;
    public bool fReal;
    public bool fAimed;
    public int uiLastUpdate;
    public int ubTilesPerUpdate;
    public int usClockTicksPerUpdate;
    public SOLDIERTYPE? pFirer;
    public int sTargetGridNo;
    public int sHitBy;
    public int iImpact;
    public int iImpactReduction;
    public int iRange;
    public int iDistanceLimit;
    public int usFlags;
    public ANITILE? pAniTile;
    public ANITILE? pShadowAniTile;
    public int ubItemStatus;
}

[Flags]
public enum BULLET_FLAG
{
    CREATURE_SPIT = 0x0001,
    KNIFE = 0x0002,
    MISSILE = 0x0004,
    SMALL_MISSILE = 0x0008,
    BULLET_STOPPED = 0x0010,
    TANK_CANNON = 0x0020,
    BUCKSHOT = 0x0040,
    FLAME = 0x0080,
}
