using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpAlliance.Core.SubSystems;

namespace SharpAlliance.Core;

public partial class Globals
{
    public const int NUM_WORLD_DIRECTIONS = (int)WorldDirections.NUM_WORLD_DIRECTIONS;

    public static Dictionary<WorldDirections, WorldDirections> gOppositeDirection = new();
    public static Dictionary<WorldDirections, WorldDirections> gTwoCCDirection = new();
    public static Dictionary<WorldDirections, WorldDirections> gTwoCDirection = new();
    public static Dictionary<WorldDirections, WorldDirections> gOneCDirection = new();
    public static Dictionary<WorldDirections, WorldDirections> gOneCCDirection = new();
    public static Dictionary<WorldDirections, List<WorldDirections>> gPurpendicularDirection = new();// WorldDirections[NUM_WORLD_DIRECTIONS, NUM_WORLD_DIRECTIONS];
}
