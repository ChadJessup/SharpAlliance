using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core.SubSystems;

public class LightingSystem
{
    // Lighting system general data
    private static int ubAmbientLightLevel = Shading.DEFAULT_SHADE_LEVEL;
    public static byte gubNumLightColors = 1;

    public ValueTask<bool> InitLightingSystem()
    {
        return ValueTask.FromResult(true);
    }

    public static int LightGetAmbient() => ubAmbientLightLevel;
}
