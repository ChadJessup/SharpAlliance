using System.Collections.Generic;
using SharpAlliance.Core.SubSystems;
using static SharpAlliance.Core.SubSystems.StrategicAI;

namespace SharpAlliance.Core;

public partial class Globals
{
    public const int NUM_ENEMY_RANKGS = (int)ENEMY_RANK.NUM_ENEMY_RANKS;
    public const int SAI_VERSION = 29;

    //The maximum size for any team strategically speaking.  For example, we can't have more than 20 enemies, militia, or creatures at a time.
    public const int MAX_STRATEGIC_TEAM_SIZE = 20;

    //Modifies the number of troops the queen has at the beginning of the game on top
    //of all of the garrison and patrol groups.  Additionally, there are a total of 
    //16 sectors that are LEVEL 1, 2, or 3 garrison groups.  The lower the level, the more
    //troops stay in that sector, and the rest will also be used as a secondary pool when
    //the primary pool runs dry.  So basically, this number is only part of the equation.
    public const int EASY_QUEENS_POOL_OF_TROOPS = 150;
    public const int NORMAL_QUEENS_POOL_OF_TROOPS = 200;
    public const int HARD_QUEENS_POOL_OF_TROOPS = 400;

    //Modifies the starting values as well as the desired values for all of the garrisons.
    public const int EASY_INITIAL_GARRISON_PERCENTAGES = 70;
    public const int NORMAL_INITIAL_GARRISON_PERCENTAGES = 100;
    public const int HARD_INITIAL_GARRISON_PERCENTAGES = 125;

    public const int EASY_MIN_ENEMY_GROUP_SIZE = 3;
    public const int NORMAL_MIN_ENEMY_GROUP_SIZE = 4;
    public const int HARD_MIN_ENEMY_GROUP_SIZE = 6;

    //Sets the starting alert chances.  Everytime an enemy arrives in a new sector, or the player,
    //this is the chance the enemy will detect the player in adjacent sectors.  This chance is associated
    //with each side checked.  Stationary groups do this check periodically.
    public const int EASY_ENEMY_STARTING_ALERT_LEVEL = 5;
    public const int NORMAL_ENEMY_STARTING_ALERT_LEVEL = 20;
    public const int HARD_ENEMY_STARTING_ALERT_LEVEL = 60;

    //When an enemy spots and chases a player group, the alertness value decrements by this value.  The
    //higher the value, the less of a chance the enemy will spot and attack subsequent groups.  This 
    //minimizes the aggressiveness of the enemy.  Ranges from 1-100 (but recommend 20-60).
    public const int EASY_ENEMY_STARTING_ALERT_DECAY = 75;
    public const int NORMAL_ENEMY_STARTING_ALERT_DECAY = 50;
    public const int HARD_ENEMY_STARTING_ALERT_DECAY = 25;
    //The base time that the queen can think about reinforcements for refilling lost patrol groups, 
    //town garrisons, etc. She only is allowed one action per 'turn'.
    public const int EASY_TIME_EVALUATE_IN_MINUTES = 480;
    public const int NORMAL_TIME_EVALUATE_IN_MINUTES = 360;
    public const int HARD_TIME_EVALUATE_IN_MINUTES = 180;
    //The variance added on.
    public const int EASY_TIME_EVALUATE_VARIANCE = 240;
    public const int NORMAL_TIME_EVALUATE_VARIANCE = 180;
    public const int HARD_TIME_EVALUATE_VARIANCE = 120;

    //When a player takes control of a sector, don't allow any enemy reinforcements to enter the sector for a 
    //limited amount of time.  This essentially dumbs down the AI, making it less aggressive.
    public const int EASY_GRACE_PERIOD_IN_HOURS = 144;      // 6 days
    public const int NORMAL_GRACE_PERIOD_IN_HOURS = 96;// 4 days
    public const int HARD_GRACE_PERIOD_IN_HOURS = 48;// 2 days

    //Defines how many days must pass before the queen is willing to refill a defeated patrol group.
    public const int EASY_PATROL_GRACE_PERIOD_IN_DAYS = 16;
    public const int NORMAL_PATROL_GRACE_PERIOD_IN_DAYS = 12;
    public const int HARD_PATROL_GRACE_PERIOD_IN_DAYS = 8;

    //Certain conditions can cause the queen to go into a "full alert" mode.  This means that temporarily, the queen's 
    //forces will automatically succeed adjacent checks until x number of enemy initiated battles occur.  The same variable
    //is what is used to determine the free checks.
    public const int EASY_NUM_AWARE_BATTLES = 1;
    public const int NORMAL_NUM_AWARE_BATTLES = 2;
    public const int HARD_NUM_AWARE_BATTLES = 3;

    public static bool gfAutoAIAware = false;

    //Saved vars
    public static int[] gbPadding2 = { 0, 0, 0 };   //NOT USED
    public static bool gfExtraElites = false;  //Set when queen compositions are augmented with bonus elites.
    public static int giGarrisonArraySize = 0;
    public static int giPatrolArraySize = 0;
    public static int giForcePercentage = 0;    //Modifies the starting group sizes relative by percentage
    public static int giArmyAlertness = 0;  //The chance the group will spot an adjacent player/militia
    public static int giArmyAlertnessDecay = 0; //How much the spotting chance decreases when spot check succeeds
    public static int gubNumAwareBattles = 0;   //When non-zero, this means the queen is very aware and searching for players.  Every time
                                                //there is an enemy initiated battle, this counter decrements until zero.  Until that point,
                                                //all adjacent sector checks automatically succeed.
    public static bool gfQueenAIAwake = false; //This flag turns on/off the strategic decisions.  If it's off, no reinforcements 
                                               //or assaults will happen.  
                                               //@@@Alex, this flag is ONLY set by the first meanwhile scene which calls an action.  If this
                                               //action isn't called, the AI will never turn on.  It is completely dependant on this action.  It can
                                               //be toggled at will in the AIViewer for testing purposes.
    public static int giReinforcementPool = 0;  //How many troops the queen has in reserve in noman's land.  These guys are spawned as needed in P3.
    public static int giReinforcementPoints = 0;    //the entire army's capacity to provide reinforcements.
    public static int giRequestPoints = 0;  //the entire army's need for reinforcements.
    public static int gubSAIVersion = SAI_VERSION;  //Used for adding new features to be saved.
    public static int gubQueenPriorityPhase = 0;    //Defines how far into defence the queen is -- abstractly related to defcon index ranging from 0-10.  
                                                    //10 is the most defensive
                                                    //Used for authorizing the use of the first battle meanwhile scene AFTER the battle is complete.  This is the case used when
                                                    //the player attacks a town, and is set once militia are sent to investigate.

    //After the first battle meanwhile scene is finished, this flag is set, and the queen orders patrol groups to immediately fortify all towns.
    public static bool gfMassFortificationOrdered = false;

    public static int gubMinEnemyGroupSize = 0;
    public static int gubHoursGracePeriod = 0;
    public static int gusPlayerBattleVictories = 0;
    public static bool gfUseAlternateQueenPosition = false;

    public const int NUM_ARMY_COMPOSITIONS = (int)Garrisons.NUM_ARMY_COMPOSITIONS;

    //padding for generic globals
    public const int SAI_PADDING_BYTES = 97;
    public static int[] gbPadding = new int[SAI_PADDING_BYTES];
    //patrol group info plus padding
    public const int SAVED_PATROL_GROUPS = 50;
    public static List<PATROL_GROUP> gPatrolGroup = new();
    //army composition info plus padding
    public const int SAVED_ARMY_COMPOSITIONS = 60;
    public static Dictionary<Garrisons, ARMY_COMPOSITION> gArmyComp = new();

    //garrison info plus padding
    public const int SAVED_GARRISON_GROUPS = 100;

    public static int gubNumGroupsArrivedSimultaneously;

    //This refers to the number of force points that are *saved* for the AI to use.  This is basically an array of each
    //group.  When the queen wants to send forces to attack a town that is defended, the initial number of forces that 
    //she would send would be considered too weak.  So, instead, she will send that force to the sector's adjacent sector,
    //and stage, while 
    public static Dictionary<Garrisons, int> gubGarrisonReinforcementsDenied = new();
    public static int[] gubPatrolReinforcementsDenied = null;

    //Unsaved vars
    public static bool gfDisplayStrategicAILogs = false;

    //If you change the MAX_STRATEGIC_TEAM_SIZE, then all the garrison sizes (start, desired) will have to be changed accordingly.

    public static Dictionary<Garrisons, ARMY_COMPOSITION> gOrigArmyComp = new()
    {	//COMPOSITION					PRIORITY	ELITE%	TROOP%	ADMIN 	DESIRED#	START#		PADDING
    	//																							START%
    	{ Garrisons.QUEEN_DEFENCE,       new(Garrisons.QUEEN_DEFENCE,              100,            100,        0,          0,          32,             32)},
        { Garrisons.MEDUNA_DEFENCE,      new(Garrisons.MEDUNA_DEFENCE,             95,             55,         45,         0,          16,             20)},
        { Garrisons.MEDUNA_SAMSITE,      new(Garrisons.MEDUNA_SAMSITE,             96,             65,         35,         0,          20,             20)},
        { Garrisons.LEVEL1_DEFENCE,      new(Garrisons.LEVEL1_DEFENCE,             40,             20,         80,         0,          12,             20)},
        { Garrisons.LEVEL2_DEFENCE,      new(Garrisons.LEVEL2_DEFENCE,             30,             10,         90,         0,          10,             20)},
        { Garrisons.LEVEL3_DEFENCE,      new(Garrisons.LEVEL3_DEFENCE,             20,             5,          95,         0,          8,              20)},
        { Garrisons.ORTA_DEFENCE,        new(Garrisons.ORTA_DEFENCE,                   90,             50,         50,         0,          18,             19)},
        { Garrisons.EAST_GRUMM_DEFENCE,  new(Garrisons.EAST_GRUMM_DEFENCE,     80,             20,         80,         0,          15,             15)},
        { Garrisons.WEST_GRUMM_DEFENCE,  new(Garrisons.WEST_GRUMM_DEFENCE,     70,             0,          100,        40,         15,             15)},
        { Garrisons.GRUMM_MINE,          new(Garrisons.GRUMM_MINE,                     85,             25,         75,         45,         15,             15)},
        { Garrisons.OMERTA_WELCOME_WAGON,new(Garrisons.OMERTA_WELCOME_WAGON,   0,              0,          100,        0,          0,              3)},
        { Garrisons.BALIME_DEFENCE,      new(Garrisons.BALIME_DEFENCE,             60,             45,         55,         20,         10,             4)},
        { Garrisons.TIXA_PRISON,         new(Garrisons.TIXA_PRISON,                    80,             10,         90,         15,         15,             15)},
        { Garrisons.TIXA_SAMSITE,        new(Garrisons.TIXA_SAMSITE,                   85,             10,         90,         0,          12,             12)},
        { Garrisons.ALMA_DEFENCE,        new(Garrisons.ALMA_DEFENCE,                   74,             15,         85,         0,          11,             20)},
        { Garrisons.ALMA_MINE,           new(Garrisons.ALMA_MINE,                      80,             20,         80,         45,         15,             20)},
        { Garrisons.CAMBRIA_DEFENCE,     new(Garrisons.CAMBRIA_DEFENCE,            50,             0,          100,        30,         10,             6)},
        { Garrisons.CAMBRIA_MINE,        new(Garrisons.CAMBRIA_MINE,                   60,             15,         90,         40,         11,             6)},
        { Garrisons.CHITZENA_DEFENCE,    new(Garrisons.CHITZENA_DEFENCE,           30,             0,          100,        75,         12,             10)},
        { Garrisons.CHITZENA_MINE,       new(Garrisons.CHITZENA_MINE,              40,             0,          100,        75,         10,             10)},
        { Garrisons.CHITZENA_SAMSITE,    new(Garrisons.CHITZENA_SAMSITE,           75,             10,         90,         0,          9,              9)},
        { Garrisons.DRASSEN_AIRPORT,     new(Garrisons.DRASSEN_AIRPORT,            30,             0,          100,        85,         12,             10)},
        { Garrisons.DRASSEN_DEFENCE,     new(Garrisons.DRASSEN_DEFENCE,            20,             0,          100,        80,         10,             8)},
        { Garrisons.DRASSEN_MINE,        new(Garrisons.DRASSEN_MINE,                   35,             0,          100,        75,         11,             9)},
        { Garrisons.DRASSEN_SAMSITE,     new(Garrisons.DRASSEN_SAMSITE,            50,             0,          100,        0,          10,             10)},
        { Garrisons.ROADBLOCK,           new(Garrisons.ROADBLOCK,                      20,             2,          98,         0,          8,              0)},
        { Garrisons.SANMONA_SMALL,       new(Garrisons.SANMONA_SMALL,              0,              0,          0,          0,          0,              0) },
    };

    //Patrol definitions
    //NOTE:	  A point containing 0 is actually the same as SEC.A1, but because nobody is using SEC.A1 in any 
    //				of the patrol groups, I am coding 0 to be ignored.
    //NOTE:		Must have at least two points.
    public static PATROL_GROUP[] gOrigPatrolGroup = new PATROL_GROUP[]
    { //SIZE	PRIORITY	POINT1		POINT2		POINT3		POINT4		MOD 		GROUPID	WEIGHT	PENDING		
    	//																												DAY100									GROUP ID
    	new(8,          40,             new SEC[] { SEC.B1,     SEC.C1,     SEC.C3,     SEC.A3 },     -1,         0,          0,          0),
        new(6,          35,             new SEC[] { SEC.B4,     SEC.B7,     SEC.C7,     0 },              -1,         0,          0,          0),
        new(6,          25,             new SEC[] { SEC.A8,     SEC.B8,     SEC.B9,     0 },              -1,         0,          0,          0),
        new(6,          30,             new SEC[] { SEC.B10,    SEC.B12,    0,              0 },              -1,         0,          0,          0),
        new(7,          45,             new SEC[] { SEC.A11,    SEC.A14,    SEC.D14,    0 },              -1,         0,          0,          0),
    	//5	
    	new(6,          50,             new SEC[] { SEC.C8,     SEC.C9,     SEC.D9,     0 },              -1,         0,          0,          0),
        new(12,         55,             new SEC[] { SEC.D3,     SEC.G3,     0,              0 },              -1,         0,          0,          0),
        new(10,         50,             new SEC[] { SEC.D6,     SEC.D7,     SEC.F7,     0 },              -1,         0,          0,          0),
        new(10,         55,             new SEC[] { SEC.E8,     SEC.E11,    SEC.F11,    0 },              -1,         0,          0,          0),
        new(10,         60,             new SEC[] { SEC.E12,    SEC.E15,    0,              0 },              -1,         0,          0,          0),
    	//10
    	new(12,         60,             new SEC[] { SEC.G4,     SEC.G7,     0,              0 },              -1,         0,          0,          0),
        new(12,         65,             new SEC[] { SEC.G10,    SEC.G12,    SEC.F12,    0 },              -1,         0,          0,          0),
        new(12,         65,             new SEC[] { SEC.G13,    SEC.G15,    0,              0 },              -1,         0,          0,          0),
        new(10,         65,             new SEC[] { SEC.H15,    SEC.J15,    0,              0},              -1,         0,          0,          0),
        new(14,         65,             new SEC[] { SEC.H12,    SEC.J12,    SEC.J13,    0 },              -1,         0,          0,          0),
    	//15
    	new(13,         70,             new SEC[] { SEC.H9,     SEC.I9,     SEC.I10,    SEC.J10 },    -1,         0,          0,          0),
        new(11,         70,             new SEC[] { SEC.K11,    SEC.K14,    SEC.J14,    0 },              -1,         0,          0,          0),
        new(12,         75,             new SEC[] { SEC.J2,     SEC.K2,     0,              0 },              -1,         0,          0,          0),
        new(12,         80,             new SEC[] { SEC.I3,     SEC.J3,     0,              0 },              -1,         0,          0,          0),
        new(12,         80,             new SEC[] { SEC.J6,     SEC.K6,     0,              0 },              -1,         0,          0,          0),
    	//20
    	new(13,         85,             new SEC[] { SEC.K7,     SEC.K10,    0,              0 },              -1,         0,          0,          0),
        new(12,         90,             new SEC[] { SEC.L10,    SEC.M10,    0,              0 },              -1,         0,          0,          0),
        new(12,         90,             new SEC[] { SEC.N9,     SEC.N10,    0,              0 },              -1,         0,          0,          0),
        new(12,         80,             new SEC[] { SEC.L7,     SEC.L8,     SEC.M8,     SEC.M9 },     -1,         0,          0,          0),
        new(14,         80,             new SEC[] { SEC.H4,     SEC.H5,     SEC.I5,     0 },              -1,         0,          0,          0),
    	//25
    	new(7,          40,             new SEC [] { SEC.D4,     SEC.E4,     SEC.E5,     0 },              -1,         0,          0,          0),
        new(7,          50,             new SEC [] { SEC.C10,    SEC.C11,    SEC.D11,    SEC.D12 },    -1,         0,          0,          0),
        new(8,          40,             new SEC [] { SEC.A15,    SEC.C15,    SEC.C16,    0 },              -1,         0,          0,          0),
        new(12,         30,             new SEC [] { SEC.L13,    SEC.M13,    SEC.M14,    SEC.L14 },    -1,         0,          0,          0),
    	//29
    };

    public const int PATROL_GROUPS = 29;



    public static GARRISON_GROUP[] gOrigGarrisonGroup = new GARRISON_GROUP[]
    { //SECTOR	MILITARY								WEIGHT	UNUSED
    	//				COMPOSITION											GROUP ID
    	new(SEC.P3,     Garrisons.QUEEN_DEFENCE,                  0,          0),
        new(SEC.O3,     Garrisons.MEDUNA_DEFENCE,                 0,          0),
        new(SEC.O4,     Garrisons.MEDUNA_DEFENCE,                 0,          0),
        new(SEC.N3,     Garrisons.MEDUNA_DEFENCE,                 0,          0),
        new(SEC.N4,     Garrisons.MEDUNA_SAMSITE,                 0,          0),
    	//5
    	new(SEC.N5,     Garrisons.MEDUNA_DEFENCE,                 0,          0),
        new(SEC.M3,     Garrisons.LEVEL1_DEFENCE,                 0,          0),
        new(SEC.M4,     Garrisons.LEVEL1_DEFENCE,                 0,          0),
        new(SEC.M5,     Garrisons.LEVEL1_DEFENCE,                 0,          0),
        new(SEC.N6,     Garrisons.LEVEL1_DEFENCE,                 0,          0),
    	//10
    	new(SEC.M2,     Garrisons.LEVEL2_DEFENCE,                 0,          0),
        new(SEC.L3,     Garrisons.LEVEL2_DEFENCE,                 0,          0),
        new(SEC.L4,     Garrisons.LEVEL2_DEFENCE,                 0,          0),
        new(SEC.L5,     Garrisons.LEVEL2_DEFENCE,                 0,          0),
        new(SEC.M6,     Garrisons.LEVEL2_DEFENCE,                 0,          0),
    	//15
    	new(SEC.N7,     Garrisons.LEVEL1_DEFENCE,                 0,          0),
        new(SEC.L2,     Garrisons.LEVEL3_DEFENCE,                 0,          0),
        new(SEC.K3,     Garrisons.LEVEL3_DEFENCE,                 0,          0),
        new(SEC.K5,     Garrisons.LEVEL3_DEFENCE,                 0,          0),
        new(SEC.L6,     Garrisons.LEVEL3_DEFENCE,                 0,          0),
    	//20
    	new(SEC.M7,     Garrisons.LEVEL3_DEFENCE,                 0,          0),
        new(SEC.N8,     Garrisons.LEVEL3_DEFENCE,                 0,          0),
        new(SEC.K4,     Garrisons.ORTA_DEFENCE,                       0,          0),
        new(SEC.G1,     Garrisons.WEST_GRUMM_DEFENCE,         0,          0),
        new(SEC.G2,     Garrisons.EAST_GRUMM_DEFENCE,         0,          0),
    	//25
    	new(SEC.H1,     Garrisons.WEST_GRUMM_DEFENCE,         0,          0),
        new(SEC.H2,     Garrisons.EAST_GRUMM_DEFENCE,         0,          0),
        new(SEC.H3,     Garrisons.GRUMM_MINE,                         0,          0),
        new(SEC.A9,     Garrisons.OMERTA_WELCOME_WAGON,       0,          0),
        new(SEC.L11,    Garrisons.BALIME_DEFENCE,                 0,          0),
    	//30
    	new(SEC.L12,    Garrisons.BALIME_DEFENCE,                 0,          0),
        new(SEC.J9,     Garrisons.TIXA_PRISON,                        0,          0),
        new(SEC.I8,     Garrisons.TIXA_SAMSITE,                       0,          0),
        new(SEC.H13,    Garrisons.ALMA_DEFENCE,                       0,          0),
        new(SEC.H14,    Garrisons.ALMA_DEFENCE,                       0,          0),
    	//35
    	new(SEC.I13,    Garrisons.ALMA_DEFENCE,                       0,          0),
        new(SEC.I14,    Garrisons.ALMA_MINE,                          0,          0),
        new(SEC.F8,     Garrisons.CAMBRIA_DEFENCE,                0,          0),
        new(SEC.F9,     Garrisons.CAMBRIA_DEFENCE,                0,          0),
        new(SEC.G8,     Garrisons.CAMBRIA_DEFENCE,                0,          0),
    	//40
    	new(SEC.G9,     Garrisons.CAMBRIA_DEFENCE,                0,          0),
        new(SEC.H8,     Garrisons.CAMBRIA_MINE,                       0,          0),
        new(SEC.A2,     Garrisons.CHITZENA_DEFENCE,               0,          0),
        new(SEC.B2,     Garrisons.CHITZENA_MINE,                  0,          0),
        new(SEC.D2,     Garrisons.CHITZENA_SAMSITE,               0,          0),
    	//45
    	new(SEC.B13,    Garrisons.DRASSEN_AIRPORT,                0,          0),
        new(SEC.C13,    Garrisons.DRASSEN_DEFENCE,                0,          0),
        new(SEC.D13,    Garrisons.DRASSEN_MINE,                       0,          0),
        new(SEC.D15,    Garrisons.DRASSEN_SAMSITE,                0,          0),
        new(SEC.G12,    Garrisons.ROADBLOCK,                          0,          0),
    	//50
    	new(SEC.M10,    Garrisons.ROADBLOCK,                          0,          0),
        new(SEC.G6,     Garrisons.ROADBLOCK,                          0,          0),
        new(SEC.C9,     Garrisons.ROADBLOCK,                          0,          0),
        new(SEC.K10,    Garrisons.ROADBLOCK,                          0,          0),
        new(SEC.G7,     Garrisons.ROADBLOCK,                          0,          0),
    	//55
    	new(SEC.G3,     Garrisons.ROADBLOCK,                          0,          0),
        new(SEC.C5,     Garrisons.SANMONA_SMALL,                  0,          0),
    	//57
    };
}
