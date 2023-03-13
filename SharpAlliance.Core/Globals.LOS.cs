using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpAlliance.Core;

public partial class Globals
{
    public const int HEIGHT_UNITS = 256;
    public const int HEIGHT_UNITS_PER_INDEX = (HEIGHT_UNITS / PROFILE_Z_SIZE);
    public const int MAX_STRUCTURE_HEIGHT = 50;
    // 5.12 == HEIGHT_UNITS / MAX_STRUCTURE_HEIGHT
    public static int CONVERT_PIXELS_TO_HEIGHTUNITS(int n) => ((n) * HEIGHT_UNITS / MAX_STRUCTURE_HEIGHT);
    public static int CONVERT_PIXELS_TO_INDEX(int n) => ((n) * HEIGHT_UNITS / MAX_STRUCTURE_HEIGHT / HEIGHT_UNITS_PER_INDEX);
    public static int CONVERT_HEIGHTUNITS_TO_INDEX(int n) => ((n) / HEIGHT_UNITS_PER_INDEX);
    public static int CONVERT_HEIGHTUNITS_TO_DISTANCE(int n) => ((n) / (HEIGHT_UNITS / CELL_X_SIZE));
    public static int CONVERT_HEIGHTUNITS_TO_PIXELS(int n) => ((n) * MAX_STRUCTURE_HEIGHT / HEIGHT_UNITS);
    public static int CONVERT_WITHINTILE_TO_INDEX(int n) => ((n) >> 1);
    public static int CONVERT_INDEX_TO_WITHINTILE(int n) => ((n) << 1);
    public static int CONVERT_INDEX_TO_PIXELS(int n) => ((n) * MAX_STRUCTURE_HEIGHT * HEIGHT_UNITS_PER_INDEX / HEIGHT_UNITS);
    public const int TREE_SIGHT_REDUCTION = 6;
    public const int NORMAL_TREES = 10;
}
