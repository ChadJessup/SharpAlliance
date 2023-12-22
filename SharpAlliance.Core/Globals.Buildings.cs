using System.Collections.Generic;

namespace SharpAlliance.Core;

public partial class Globals
{
    // for what it's worth, 2 bytes, we use roof climb spots as 1-based
    // so the 0th entry is always 0 and can be compared with (and not equal)
    // NOWHERE or any other location
    public const int MAX_CLIMBSPOTS_PER_BUILDING = 21;

    // similarly for buildings, only we really want 0 to be invalid index
    public const int NO_BUILDING = 0;
    public const int MAX_BUILDINGS = 31;

    public const int ROOF_LOCATION_CHANCE = 8;

    public static List<int> gubBuildingInfo = new();
    public static List<BUILDING> gBuildings = new();
    public static int gubNumberOfBuildings;
}
