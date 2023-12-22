using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpAlliance.Core.SubSystems;

namespace SharpAlliance.Core;

public partial class Globals
{
    public const AnimationSurfaceTypes EMPTY_CACHE_ENTRY = (AnimationSurfaceTypes)65000;

    public const int MAX_CACHE_SIZE = 20;
    public const int MIN_CACHE_SIZE = 2;
    public static int guiCacheSize = MIN_CACHE_SIZE;
}
