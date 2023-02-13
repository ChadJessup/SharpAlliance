using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpAlliance.Core.SubSystems;

public class Points
{
}

public enum AP
{
    MINIMUM = 10,      // no merc can have less for his turn
    MAXIMUM = 25,      // no merc can have more for his turn
    MONSTER_MAXIMUM = 40,      // no monster can have more for his turn
    VEHICLE_MAXIMUM = 50,      // no merc can have more for his turn
    INCREASE = 10,      // optional across-the-board AP boost
    MAX_AP_CARRIED = 5,      // APs carried from turn-to-turn
    // monster AP bonuses, expressed in 10ths (12 = 120% normal) 
    YOUNG_MONST_FACTOR = 15,
    ADULT_MONST_FACTOR = 12,
    MONST_FRENZY_FACTOR = 13,
    // AP penalty for a phobia situation (again, in 10ths)
    CLAUSTROPHOBE = 9,
    AFRAID_OF_INSECTS = 8,
    EXCHANGE_PLACES = 5,
    // Action Point values
    REVERSE_MODIFIER = 1,
    STEALTH_MODIFIER = 2,
    STEAL_ITEM = 10,         // APs to steal item....
    TAKE_BLOOD = 10,
    TALK = 6,
    MOVEMENT_FLAT = 3,               // div by 2 for run, +2, for crawl, -1 for swat
    MOVEMENT_GRASS = 4,
    MOVEMENT_BUSH = 5,
    MOVEMENT_RUBBLE = 6,
    MOVEMENT_SHORE = 7,   // shallow wade
    MOVEMENT_LAKE = 9,   // deep wade -> slowest
    MOVEMENT_OCEAN = 8,   // swimming is faster than deep wade
    CHANGE_FACING = 1,   // turning to face any other direction
    CHANGE_TARGET = 1,   // aiming at a new target
    CATCH_ITEM = 5,              // turn to catch item
    TOSS_ITEM = 8,           // toss item from inv
    REFUEL_VEHICLE = 10,
    /*
    MOVE_ITEM_FREE       0       // same place, pocket->pocket
    MOVE_ITEM_FAST       2       // hand, holster, ground only
    MOVE_ITEM_AVG        4       // everything else!
    MOVE_ITEM_SLOW       6       // vests, protective gear
    */
    MOVE_ITEM_FAST = 4,     // hand, holster, ground only
    MOVE_ITEM_SLOW = 6, // vests, protective gear
    RADIO = 5,
    CROUCH = 2,
    PRONE = 2,
    LOOK_STANDING = 1,
    LOOK_CROUCHED = 2,
    LOOK_PRONE = 2,
    READY_KNIFE = 0,
    READY_PISTOL = 1,
    READY_RIFLE = 2,
    READY_SAW = 0,
    // JA2Gold: reduced dual AP cost from 3 to 1
    //READY_DUAL           3
    READY_DUAL = 1,
    MIN_AIM_ATTACK = 0,       // minimum permitted extra aiming
    MAX_AIM_ATTACK = 4,       // maximum permitted extra aiming
    BURST = 5,
    DROP_BOMB = 3,
    RELOAD_GUN = 5,// loading new clip/magazine
    START_FIRST_AID = 5,// get the stuff out of medic kit
    PER_HP_FIRST_AID = 1,// for each point healed
    STOP_FIRST_AID = 3,// put everything away again
    START_REPAIR = 5,    // get the stuff out of repair kit
    GET_HIT = 2,      // struck by bullet, knife, explosion
    GET_WOUNDED_DIVISOR = 4,  // 1 AP lost for every 'divisor' dmg
    FALL_DOWN = 4,  // falling down (explosion, exhaustion)
    GET_THROWN = 2,  // get thrown back (by explosion)
    GET_UP = 5,  // getting up again
    ROLL_OVER = 2,  // flipping from back to stomach
    OPEN_DOOR = 3,  // whether successful, or not (locked)
    PICKLOCK = 10,     // should really be several turns
    EXAMINE_DOOR = 5,    // time to examine door
    BOOT_DOOR = 8,         // time to boot door
    USE_CROWBAR = 10,      // time to crowbar door
    UNLOCK_DOOR = 6,       // time to unlock door
    LOCK_DOOR = 6,             // time to lock door
    EXPLODE_DOOR = 10,       // time to set explode charge on door
    UNTRAP_DOOR = 10,        // time to untrap door
    USEWIRECUTTERS = 10,     // Time to use wirecutters
    CLIMBROOF = 10,          // APs to climb roof
    CLIMBOFFROOF = 6,        // APs to climb off roof
    JUMPFENCE = 6,           // time to jump over a fence
    OPEN_SAFE = 8,       // time to use combination
    USE_REMOTE = 2,
    PULL_TRIGGER = 2,       // operate nearby panic trigger
    FORCE_LID_OPEN = 10,
    SEARCH_CONTAINER = 5,       // boxes, crates, safe, etc.
    READ_NOTE = 10,   // reading a note's contents in inv.
    SNAKE_BATTLE = 10,   // when first attacked
    KILL_SNAKE = 7,   // when snake battle's been won
    USE_SURV_CAM = 5,
    PICKUP_ITEM = 3,
    GIVE_ITEM = 1,
    BURY_MINE = 10,
    DISARM_MINE = 10,
    DRINK = 5,
    CAMOFLAGE = 10,
    TAKE_PHOTOGRAPH = 5,
    MERGE = 8,
    OTHER_COST = 99,
    START_RUN_COST = 1,
    ATTACH_CAN = 5,
    JUMP_OVER = 6,
}

public enum BP
{
    // special Breath Point related constants
    RATIO_RED_PTS_TO_NORMAL = 100,
    RUN_ENERGYCOSTFACTOR = 3,// Richard thinks running is 3rd most strenous over time... tough, Mark didn't.  CJC increased it again
    WALK_ENERGYCOSTFACTOR = 1,// walking subtracts flat terrain breath value
    SWAT_ENERGYCOSTFACTOR = 2,// Richard thinks swatmove is 2nd most strenous over time... tough, Mark didn't
    CRAWL_ENERGYCOSTFACTOR = 4,// Richard thinks crawling is the MOST strenuous over time	
    RADIO = 0,// no breath cost
    USE_DETONATOR = 0, // no breath cost
    REVERSE_MODIFIER = 0,  // no change, a bit more challenging
    STEALTH_MODIFIER = -20,   // slow & cautious, not too strenuous
    MINING_MODIFIER = -30, // pretty relaxing, overall
                           // end-of-turn Breath Point gain/usage rates
    PER_AP_NO_EFFORT = -200,   // gain breath!
    PER_AP_MIN_EFFORT = -100,    // gain breath!
    PER_AP_LT_EFFORT = -50,     // gain breath!
    PER_AP_MOD_EFFORT = 25,
    PER_AP_HVY_EFFORT = 50,
    PER_AP_MAX_EFFORT = 100,
    // Breath Point values
    MOVEMENT_FLAT = 5,
    MOVEMENT_GRASS = 10,
    MOVEMENT_BUSH = 20,
    MOVEMENT_RUBBLE = 35,
    MOVEMENT_SHORE = 50,      // shallow wade
    MOVEMENT_LAKE = 75,      // deep wade
    MOVEMENT_OCEAN = 100,     // swimming
    CHANGE_FACING = 10,    // turning to face another direction
    CROUCH = 10,
    PRONE = 10,
    CLIMBROOF = 500,     // BPs to climb roof
    CLIMBOFFROOF = 250,  // BPs to climb off roof
    JUMPFENCE = 200,     // BPs to jump fence
    /*
    MOVE_ITEM_FREE       0       // same place, pocket->pocket
    MOVE_ITEM_FAST       0       // hand, holster, ground only
    MOVE_ITEM_AVG        0       // everything else!
    MOVE_ITEM_SLOW       20      // vests, protective gear
    */
    MOVE_ITEM_FAST = 0,// hand, holster, ground only
    MOVE_ITEM_SLOW = 20,// vests, protective gear
    READY_KNIFE = 0,// raise/lower knife
    READY_PISTOL = 10,// raise/lower pistol
    READY_RIFLE = 20,// raise/lower rifle
    READY_SAW = 0,// raise/lower saw
    STEAL_ITEM = 50,// BPs steal item
    PER_AP_AIMING = 5,// breath cost while aiming
    RELOAD_GUN = 20,// loading new clip/magazine
    THROW_ITEM = 50,// throw grenades, fire-bombs, etc.
    START_FIRST_AID = 0,// get the stuff out of medic kit
    PER_HP_FIRST_AID = -25,// gain breath for each point healed
    STOP_FIRST_AID = 0,// put everything away again
    GET_HIT = 200,// struck by bullet, knife, explosion
    GET_WOUNDED = 50,// per pt of GUNFIRE/EXPLOSION impact
    FALL_DOWN = 250,// falling down (explosion, exhaustion)
    GET_UP = 50,// getting up again
    ROLL_OVER = 20,// flipping from back to stomach
    OPEN_DOOR = 30,// whether successful, or not (locked)
    PICKLOCK = -250,// gain breath, not very tiring...
    EXAMINE_DOOR = -250,// gain breath, not very tiring...
    BOOT_DOOR = 200,     // BP to boot door
    USE_CROWBAR = 350,     // BP to crowbar door
    UNLOCK_DOOR = 50,    // BP to unlock door
    EXPLODE_DOOR = -250,     // BP to set explode charge on door
    UNTRAP_DOOR = 150,       // BP to untrap
    LOCK_DOOR = 50,          // BP to untrap
    USEWIRECUTTERS = 200,    // BP to use wirecutters
    PULL_TRIGGER = 0, // for operating panic triggers
    FORCE_LID_OPEN = 50, // per point of strength required
    SEARCH_CONTAINER = 0,// get some breath back (was -50)
    OPEN_SAFE = -50,
    READ_NOTE = -250,    // reading a note's contents in inv.
    SNAKE_BATTLE = 500,   // when first attacked
    KILL_SNAKE = 350,  // when snake battle's been won
    USE_SURV_CAM = -100,
    BURY_MINE = 250,   // involves digging & filling again
    DISARM_MINE = 0,  // 1/2 digging, 1/2 light effort
    FIRE_HANDGUN = 25, // preatty easy, little recoil
    FIRE_RIFLE = 50,// heavier, nasty recoil
    FIRE_SHOTGUN = 100, // quite tiring, must be pumped up
    STAB_KNIFE = 200,
    TAKE_PHOTOGRAPH = 0,
    MERGE = 50,
    FALLFROMROOF = 1000,
    JUMP_OVER = 250,
}
