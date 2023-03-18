using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpAlliance.Core.SubSystems;

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

    public const int MAX_TOSS_SEARCH_DIST = 1;// must throw within this of opponent;
    public const int NPC_TOSS_SAFETY_MARGIN = 4;       // all friends must be this far away;
    public static bool ACTING_ON_SCHEDULE(SOLDIERTYPE p) => (p.fAIFlags.HasFlag(AIDEFINES.AI_CHECK_SCHEDULE));
    public const int AI_AP_CLIMBROOF = 15;    // the AI should try to have this many APs before climbing a roof, if possible;
    public const int TEMPORARILY = 0;
    public const bool FOREVER = true;
    public const int DONTADDTURNCOST = 0;
    public const int ADDTURNCOST = 1;
    public static bool PTR_CIV_OR_MILITIA(SOLDIERTYPE pSoldier) => (PTR_CIVILIAN(pSoldier) || (pSoldier.bTeam == MILITIA_TEAM));
    public static int REALTIME_AI_DELAY = (10000 + Globals.Random.Next(1000));
    public static int REALTIME_CIV_AI_DELAY = (1000 * (gTacticalStatus.Team[MILITIA_TEAM].bMenInSector + gTacticalStatus.Team[CIV_TEAM].bMenInSector) + 5000 + 2000 * Globals.Random.Next(3));
    public static int REALTIME_CREATURE_AI_DELAY = (10000 + 1000 * Globals.Random.Next(3));
    public const int ENEMYDIFFICULTY = 8;   // this is being used in this module;
    public const int MAXGAMEOPTIONS = 14;
    public const int PERCENT_TO_IGNORE_THREAT = 50;      // any less, use threat value;
    public const int ACTION_TIMEOUT_CYCLES = 50;    // # failed cycles through AI;
    public const int MAX_THREAT_RANGE = 400;// 30 tiles worth;
    public const int MIN_PERCENT_BETTER = 5;// 5% improvement in cover is good;
    public const int TOSSES_PER_10TURNS = 18;      // max # of grenades tossable in 10 turns;
    public const int SHELLS_PER_10TURNS = 13;      // max # of shells   firable  in 10 turns;
    public const int SEE_THRU_COVER_THRESHOLD = 5;      // min chance to get through;
}

public enum NOSHOOT
{ 
    WAITABIT = -1,
    WATER = -2,
    MYSELF = -3,
    HURT = -4,
    NOAMMO = -5,
    NOLOAD = -6,
    NOWEAPON = -7,
}
