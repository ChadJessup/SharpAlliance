using System;

using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core.SubSystems;

public class Environment
{
}

[Flags]
public enum WEATHER_FORECAST
{
    SUNNY = 0x00000001,
    OVERCAST = 0x00000002,
    PARTLYSUNNY = 0x00000004,
    DRIZZLE = 0x00000008,
    SHOWERS = 0x00000010,
    THUNDERSHOWERS = 0x00000020,
}
