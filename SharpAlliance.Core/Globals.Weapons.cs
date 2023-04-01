using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpAlliance.Core;

public partial class Globals
{
    public const int MAXCHANCETOHIT = 99;
    public const int BAD_DODGE_POSITION_PENALTY = 20;
    public const int GUN_BARREL_RANGE_BONUS = 100;
    // Special deaths can only occur within a limited distance to the target
    public const int MAX_DISTANCE_FOR_MESSY_DEATH = 7;
    // If you do a lot of damage with a close-range shot, instant kill
    public const int MIN_DAMAGE_FOR_INSTANT_KILL = 55;
    // If you happen to kill someone with a close-range shot doing a lot of damage to the head, head explosion
    public const int MIN_DAMAGE_FOR_HEAD_EXPLOSION = 45;
    // If you happen to kill someone with a close-range shot doing a lot of damage to the chest, chest explosion
    // This value is lower than head because of the damage bonus for shooting the head
    public const int MIN_DAMAGE_FOR_BLOWN_AWAY = 30;
    // If you happen to hit someone in the legs for enough damage, REGARDLESS of distance, person falls down
    // Leg damage is halved for these purposes
    public const int MIN_DAMAGE_FOR_AUTO_FALL_OVER = 20;
    // short range at which being prone provides to hit penalty when shooting standing people
    public const int MIN_PRONE_RANGE = 50;
    // can't miss at this range?
    public const int POINT_BLANK_RANGE = 16;
    public const int BODY_IMPACT_ABSORPTION = 20;
    public const int BUCKSHOT_SHOTS = 9;
    public const int MIN_MORTAR_RANGE = 150;// minimum range of a mortar

}
