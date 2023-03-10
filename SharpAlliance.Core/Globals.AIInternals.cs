using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpAlliance.Core;

public partial class Globals
{
    public const int NOWATER = 0;
    public const int WATEROK = 1;
    public const int IGNORE_PATH = 0;
    public const int ENSURE_PATH = 1;
    public const int ENSURE_PATH_COST = 2;
    public const int DONTFORCE = 0;
    public const int FORCE = 1;
    public const int MAX_ROAMING_RANGE = WORLD_COLS;
}
