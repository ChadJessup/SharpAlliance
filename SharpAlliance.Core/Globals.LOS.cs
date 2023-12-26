using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpAlliance.Core.SubSystems;

namespace SharpAlliance.Core;

public partial class Globals
{
    public const int HEIGHT_UNITS = 256;
    public const int HEIGHT_UNITS_PER_INDEX = HEIGHT_UNITS / PROFILE_Z_SIZE;
    public const int MAX_STRUCTURE_HEIGHT = 50;
    // 5.12 == HEIGHT_UNITS / MAX_STRUCTURE_HEIGHT
    public static int CONVERT_PIXELS_TO_HEIGHTUNITS(int n) => n * HEIGHT_UNITS / MAX_STRUCTURE_HEIGHT;
    public static int CONVERT_PIXELS_TO_INDEX(int n) => n * HEIGHT_UNITS / MAX_STRUCTURE_HEIGHT / HEIGHT_UNITS_PER_INDEX;
    public static int CONVERT_HEIGHTUNITS_TO_INDEX(int n) => n / HEIGHT_UNITS_PER_INDEX;
    public static int CONVERT_HEIGHTUNITS_TO_DISTANCE(int n) => n / (HEIGHT_UNITS / CELL_X_SIZE);
    public static int CONVERT_HEIGHTUNITS_TO_PIXELS(int n) => n * MAX_STRUCTURE_HEIGHT / HEIGHT_UNITS;
    public static int CONVERT_WITHINTILE_TO_INDEX(int n) => (n) >> 1;
    public static int CONVERT_INDEX_TO_WITHINTILE(int n) => (n) << 1;
    public static int CONVERT_INDEX_TO_PIXELS(int n) => n * MAX_STRUCTURE_HEIGHT * HEIGHT_UNITS_PER_INDEX / HEIGHT_UNITS;
    public const int TREE_SIGHT_REDUCTION = 6;
    public const int NORMAL_TREES = 10;

    public const float STANDING_HEIGHT = 191.0f;
    public const float STANDING_LOS_POS = 175.0f;
    public const float STANDING_FIRING_POS = 175.0f;
    public const float STANDING_HEAD_TARGET_POS = 175.0f;
    public const float STANDING_HEAD_BOTTOM_POS = 159.0f;
    public const float STANDING_TORSO_TARGET_POS = 127.0f;
    public const float STANDING_TORSO_BOTTOM_POS = 95.0f;
    public const float STANDING_LEGS_TARGET_POS = 47.0f;
    public const float STANDING_TARGET_POS = STANDING_HEAD_TARGET_POS;
    public const float CROUCHED_HEIGHT = 130.0f;
    public const float CROUCHED_LOS_POS = 111.0f;
    public const float CROUCHED_FIRING_POS = 111.0f;
    public const float CROUCHED_HEAD_TARGET_POS = 111.0f;
    public const float CROUCHED_HEAD_BOTTOM_POS = 95.0f;
    public const float CROUCHED_TORSO_TARGET_POS = 71.0f;
    public const float CROUCHED_TORSO_BOTTOM_POS = 47.0f;
    public const float CROUCHED_LEGS_TARGET_POS = 31.0f;
    public const float CROUCHED_TARGET_POS = CROUCHED_HEAD_TARGET_POS;
    public const float PRONE_HEIGHT = 63.0f;
    public const float PRONE_LOS_POS = 31.0f;
    public const float PRONE_FIRING_POS = 31.0f;
    public const float PRONE_TORSO_TARGET_POS = 31.0f;
    public const float PRONE_HEAD_TARGET_POS = 31.0f;
    public const float PRONE_LEGS_TARGET_POS = 31.0f;
    public const float PRONE_TARGET_POS = PRONE_HEAD_TARGET_POS;
    public const float WALL_HEIGHT_UNITS = HEIGHT_UNITS;
    public const float WINDOW_BOTTOM_HEIGHT_UNITS = 87;
    public const float WINDOW_TOP_HEIGHT_UNITS = 220;
    public const float CLOSE_TO_FIRER = 25;
    public const float VERY_CLOSE_TO_FIRER = 21;

    public static Dictionary<AnimationHeights, int> gubTreeSightReduction = new()
    {
        { (AnimationHeights)0, 0 },
        { AnimationHeights.ANIM_PRONE, 8 }, // prone
        { AnimationHeights.ANIM_CROUCH, 7 }, // crouched
        { AnimationHeights.ANIM_STAND , 6 } // standing
    };

    public const int MAX_DIST_FOR_LESS_THAN_MAX_CHANCE_TO_HIT_STRUCTURE = 25;

}
