using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpAlliance.Core;

public class SmokeEffects
{
    public static bool SaveSmokeEffectsToMapTempFile(int sMapX, MAP_ROW sMapY, int bMapZ)
    {
        return false;
    }
}

// Smoke effect types
public enum SmokeEffectType
{
    NO_SMOKE_EFFECT,
    NORMAL_SMOKE_EFFECT,
    TEARGAS_SMOKE_EFFECT,
    MUSTARDGAS_SMOKE_EFFECT,
    CREATURE_SMOKE_EFFECT,
};

[Flags]
public enum SMOKE_EFFECT
{
INDOORS         = 0x01,
ON_ROOF         = 0x02,
MARK_FOR_UPDATE = 0x04,
}

public struct SMOKEEFFECT
{
    public int sGridNo;          // gridno at which the tear gas cloud is centered
    public int ubDuration;        // the number of turns gas will remain effective
    public int ubRadius;          // the current radius of the cloud in map tiles
    public int bFlags;            // 0 - outdoors (fast spread), 1 - indoors (slow)
    public int bAge;             // the number of turns gas has been around
    public int bType;
    public int  usItem;
    public int ubOwner;
    public int ubPadding;
    public int  uiTimeOfLastUpdate;
    public bool fAllocated;
}
