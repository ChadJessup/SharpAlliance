using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using SharpAlliance.Core.SubSystems;

namespace SharpAlliance.Core;

public partial class Globals
{
    public const int MAX_APS_SUPPRESSED = 8;

    public const int REFINE_AIM_1 = 0;
    public const int REFINE_AIM_MID1 = 1;
    public const int REFINE_AIM_2 = 2;
    public const int REFINE_AIM_MID2 = 3;
    public const int REFINE_AIM_3 = 4;
    public const int REFINE_AIM_MID3 = 5;
    public const int REFINE_AIM_4 = 6;
    public const int REFINE_AIM_MID4 = 7;
    public const int REFINE_AIM_5 = 8;
    public const int REFINE_AIM_BURST = 10;

    public const int AIM_SHOT_RANDOM = 0;
    public const int AIM_SHOT_HEAD = 1;
    public const int AIM_SHOT_TORSO = 2;
    public const int AIM_SHOT_LEGS = 3;
    public const int AIM_SHOT_GLAND = 4;

    public const int MIN_AMB_LEVEL_FOR_MERC_LIGHTS = 9;

    //TACTICAL OVERHEAD STUFF
    public const int NO_SOLDIER = TOTAL_SOLDIERS;// SAME AS NOBODY
    public const int NOBODY = NO_SOLDIER;

    // VIEWRANGE DEFINES
    public const int NORMAL_VIEW_RANGE = 13;
    public const int MIN_RANGE_FOR_BLOWNAWAY = 40;

    // TIMER DELAYS
    public const int DAMAGE_DISPLAY_DELAY = 250;
    public const int FADE_DELAY = 150;
    public const int FLASH_SELECTOR_DELAY = 4000;
    public const int BLINK_SELECTOR_DELAY = 250;

    public const int DONTLOOK = 0;
    public const int LOOK = 1;


    public const int NOLOCATE = 0;
    public const int LOCATE = 1;

    public const int DONTSETLOCATOR = 0;
    public const int SETLOCATOR = 1;
    public const int SETANDREMOVEPREVIOUSLOCATOR = 2;
    public const int SETLOCATORFAST = 3;

    public const int NOCENTERING = 0;

    public const int NOUPDATE = 0;
    public const int UPDATE = 1;

    // DEFINES FOR WEAPON HIT EVENT SPECIAL PARAM
    public const int FIRE_WEAPON_NO_SPECIAL = 0;
    public const int FIRE_WEAPON_BURST_SPECIAL = 1;
    public const int FIRE_WEAPON_HEAD_EXPLODE_SPECIAL = 2;
    public const int FIRE_WEAPON_CHEST_EXPLODE_SPECIAL = 3;
    public const int FIRE_WEAPON_LEG_FALLDOWN_SPECIAL = 4;
    public const int FIRE_WEAPON_HIT_BY_KNIFE_SPECIAL = 5;
    public const int FIRE_WEAPON_SLEEP_DART_SPECIAL = 6;
    public const int FIRE_WEAPON_BLINDED_BY_SPIT_SPECIAL = 7;
    public const int FIRE_WEAPON_TOSSED_OBJECT_SPECIAL = 8;

    public const int NO_INTERRUPTS = 0;
    public const int ALLOW_INTERRUPTS = 1;

    [Flags]
    public enum SIGHT
    {
        LOOK = 0x1,
        // SIGHT_SEND      0x2   // no longer needed using LOCAL OPPLISTs
        RADIO = 0x4,
        INTERRUPT = 0x8,
        ALL = 0xF,
    }


    // CHANGE THIS VALUE TO AFFECT TOTAL SIGHT RANGE
    public const int STRAIGHT_RANGE = 13;

    // CHANGE THESE VALUES TO ADJUST VARIOUS FOV ANGLES
    public const int STRAIGHT_RATIO = 1;
    public const double ANGLE_RATIO = 0.857;
    public const double SIDE_RATIO = 0.571;

    // CJC: Changed SBEHIND_RATIO (side-behind ratio) to be 0 to make stealth attacks easier
    // Changed on September 21, 1998
    //public const int SBEHIND_RATIO		0.142
    public const int SBEHIND_RATIO = 0;
    public const int BEHIND_RATIO = 0;

    // looking distance defines
    public static int BEHIND => (BEHIND_RATIO * STRAIGHT_RANGE);
    public static int SBEHIND => (SBEHIND_RATIO * STRAIGHT_RANGE);
    public static int SIDE => (int)(SIDE_RATIO * STRAIGHT_RANGE);
    public static int ANGLE => (int)(ANGLE_RATIO * STRAIGHT_RANGE);
    public static int STRAIGHT = (STRAIGHT_RATIO * STRAIGHT_RANGE);
    public static bool PTR_OURTEAM(SOLDIERTYPE pSoldier) => (pSoldier.bTeam == gbPlayerNum);

    // opplist value constants
    public const int HEARD_3_TURNS_AGO = -4;
    public const int HEARD_2_TURNS_AGO = -3;
    public const int HEARD_LAST_TURN = -2;
    public const int HEARD_THIS_TURN = -1;
    public const int NOT_HEARD_OR_SEEN = 0;
    public const int SEEN_CURRENTLY = 1;
    public const int SEEN_THIS_TURN = 2;
    public const int SEEN_LAST_TURN = 3;
    public const int SEEN_2_TURNS_AGO = 4;
    public const int SEEN_3_TURNS_AGO = 5;

    public const int OLDEST_SEEN_VALUE = SEEN_3_TURNS_AGO;
    public const int OLDEST_HEARD_VALUE = HEARD_3_TURNS_AGO;

    public const int UNDER_FIRE = 2;
    public const int UNDER_FIRE_LAST_TURN = 1;


    public const int MAX_DISTANCE_FOR_PROXIMITY_SIGHT = 15;

    // DEFINES FOR BODY TYPE SUBSTITUTIONS
    public const int SUB_ANIM_BIGGUYSHOOT2 = 0x00000001;
    public const int SUB_ANIM_BIGGUYTHREATENSTANCE = 0x00000002;

    // DEFINE TEAMS
    public const TEAM OUR_TEAM = 0;
    public const TEAM ENEMY_TEAM = TEAM.ENEMY_TEAM;
    public const TEAM CREATURE_TEAM = TEAM.CREATURE_TEAM;
    public const TEAM MILITIA_TEAM = TEAM.MILITIA_TEAM;
    public const TEAM CIV_TEAM = TEAM.CIV_TEAM;
    public const TEAM LAST_TEAM = TEAM.CIV_TEAM;
    public const int PLAYER_PLAN = 5;

    public const int CIV_GROUP_NEUTRAL = 0;
    public const int CIV_GROUP_WILL_EVENTUALLY_BECOME_HOSTILE = 1;
    public const int CIV_GROUP_WILL_BECOME_HOSTILE = 2;
    public const int CIV_GROUP_HOSTILE = 3;

    // MACROS
    // This will set an animation ID
    public static string SET_PALETTEREP_ID(string a, string b)
    {
        a = b;
        return a;
    }

    // strcmp returns 0 if true!		
    public static bool COMPARE_PALETTEREP_ID(string a, string b) => ((a.Equals(b)) ? false : true);
}

// ORDERS
public enum Orders
{
    STATIONARY = 0,     // moves max 1 sq., no matter what's going on
    ONGUARD,                    // moves max 2 sqs. until alerted by something
    CLOSEPATROL,            // patrols within 5 spaces until alerted
    FARPATROL,              // patrols within 15 spaces
    POINTPATROL,            // patrols using patrolGrids
    ONCALL,                     // helps buddies anywhere within the sector
    SEEKENEMY,              // not tied down to any one particular spot
    RNDPTPATROL,            // patrols randomly using patrolGrids
    MAXORDERS
};

// ATTITUDES
public enum Attitudes
{
    DEFENSIVE = 0,
    BRAVESOLO,
    BRAVEAID,
    CUNNINGSOLO,
    CUNNINGAID,
    AGGRESSIVE,
    MAXATTITUDES,
    ATTACKSLAYONLY // special hyperaggressive vs Slay only value for Carmen the bounty hunter
};

// alert status types
public enum STATUS
{
    GREEN = 0,   // everything's OK, no suspicion
    YELLOW,      // he or his friend heard something
    RED,             // has definite evidence of opponent
    BLACK,           // currently sees an active opponent
    NUM_STATUS_STATES
};

public enum MORALE
{
    HOPELESS = 0,
    WORRIED,
    NORMAL,
    CONFIDENT,
    FEARLESS,
    NUM_MORALE_STATES
};

// boxing state
public enum BoxingStates
{
    NOT_BOXING = 0,
    BOXING_WAITING_FOR_PLAYER,
    PRE_BOXING,
    BOXING,
    DISQUALIFIED,
    WON_ROUND,
    LOST_ROUND
}

//
//-----------------------------------------------

// PALETTE SUBSITUTION TYPES
public struct PaletteSubRangeType
{
    int ubStart;
    int ubEnd;
}


public struct PaletteReplacementType
{
    int ubType;
    PaletteRepID ID;
    int ubPaletteSize;
    int r;
    int g;
    int b;
}
