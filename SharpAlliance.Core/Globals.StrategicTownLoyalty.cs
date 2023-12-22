using System.Collections.Generic;
using SharpAlliance.Core.Screens;

namespace SharpAlliance.Core;

public partial class Globals
{

    // gain pts per real loyalty pt
    public const int GAIN_PTS_PER_LOYALTY_PT = 500;

    // --- LOYALTY BONUSES ---
    // Omerta
    public const int LOYALTY_BONUS_MIGUEL_READS_LETTER = (10 * GAIN_PTS_PER_LOYALTY_PT);    // multiplied by 4.5 due to Omerta's high seniment, so it's 45%
    // Drassen
    public const int LOYALTY_BONUS_CHILDREN_FREED_DOREEN_KILLED = (10 * GAIN_PTS_PER_LOYALTY_PT);   // +50% bonus for Drassen
    public const int LOYALTY_BONUS_CHILDREN_FREED_DOREEN_SPARED = (20 * GAIN_PTS_PER_LOYALTY_PT);   // +50% bonus for Drassen
    // Cambria
    public const int LOYALTY_BONUS_MARTHA_WHEN_JOEY_RESCUED = (15 * GAIN_PTS_PER_LOYALTY_PT);   // -25% for low Cambria sentiment
    public const int LOYALTY_BONUS_KEITH_WHEN_HILLBILLY_SOLVED = (15 * GAIN_PTS_PER_LOYALTY_PT);    // -25% for low Cambria sentiment
    // Chitzena
    public const int LOYALTY_BONUS_YANNI_WHEN_CHALICE_RETURNED_LOCAL = (20 * GAIN_PTS_PER_LOYALTY_PT);  // +75% higher in Chitzena
    public const int LOYALTY_BONUS_YANNI_WHEN_CHALICE_RETURNED_GLOBAL = (10 * GAIN_PTS_PER_LOYALTY_PT); // for ALL towns!
    // Alma
    public const int LOYALTY_BONUS_AUNTIE_WHEN_BLOODCATS_KILLED = (20 * GAIN_PTS_PER_LOYALTY_PT);   // Alma's increases reduced by half due to low rebel sentiment
    public const int LOYALTY_BONUS_MATT_WHEN_DYNAMO_FREED = (20 * GAIN_PTS_PER_LOYALTY_PT); // Alma's increases reduced by half due to low rebel sentiment
    public const int LOYALTY_BONUS_FOR_SERGEANT_KROTT = (20 * GAIN_PTS_PER_LOYALTY_PT); // Alma's increases reduced by half due to low rebel sentiment
    // Everywhere
    public const int LOYALTY_BONUS_TERRORISTS_DEALT_WITH = (5 * GAIN_PTS_PER_LOYALTY_PT);
    public const int LOYALTY_BONUS_KILL_QUEEN_MONSTER = (10 * GAIN_PTS_PER_LOYALTY_PT);
    // Anywhere
    // loyalty bonus for completing town training
    public const int LOYALTY_BONUS_FOR_TOWN_TRAINING = (2 * GAIN_PTS_PER_LOYALTY_PT);		// 2%

    // --- LOYALTY PENALTIES ---
    // Cambria
    public const int LOYALTY_PENALTY_MARTHA_HEART_ATTACK = (20 * GAIN_PTS_PER_LOYALTY_PT);
    public const int LOYALTY_PENALTY_JOEY_KILLED = (10 * GAIN_PTS_PER_LOYALTY_PT);
    // Balime
    public const int LOYALTY_PENALTY_ELDIN_KILLED = (20 * GAIN_PTS_PER_LOYALTY_PT);	// effect is double that!
    // Any mine
    public const int LOYALTY_PENALTY_HEAD_MINER_ATTACKED = (20 * GAIN_PTS_PER_LOYALTY_PT);  // exact impact depends on rebel sentiment in that town
                                                                                            // Loyalty penalty for being inactive, per day after the third
    public const int LOYALTY_PENALTY_INACTIVE = (10 * GAIN_PTS_PER_LOYALTY_PT);

    public const int RETREAT_TACTICAL_TRAVERSAL = 0;
    public const int RETREAT_PBI = 1;
    public const int RETREAT_AUTORESOLVE = 2;

    public const TOWNS FIRST_TOWN = TOWNS.OMERTA;

    public static Dictionary<TOWNS, TOWN_LOYALTY> gTownLoyalty = new();
    public static Dictionary<TOWNS, bool> gfTownUsesLoyalty = new()
    {
        { 0, false              },      // not a town - blank sector index
    	{ TOWNS.OMERTA, true    },      // OMERTA
    	{ TOWNS.DRASSEN, true   },      // DRASSEN
    	{ TOWNS.ALMA, true      },      // ALMA
    	{ TOWNS.GRUMM, true     },      // GRUMM
    	{ TOWNS.TIXA, false     },      // TIXA
    	{ TOWNS.CAMBRIA, true   },      // CAMBRIA
    	{ TOWNS.SAN_MONA, false },      // SAN_MONA
    	{ TOWNS.ESTONI, false   },      // ESTONI
    	{ TOWNS.ORTA, false     },      // ORTA
    	{ TOWNS.BALIME, true    },      // BALIME
    	{ TOWNS.MEDUNA, true    },      // MEDUNA
    	{ TOWNS.CHITZENA, true  },      // CHITZENA
    };
}

public class TOWN_LOYALTY
{
    public int ubRating;
    public int sChange;
    public bool fStarted;       // starting loyalty of each town is initialized only when player first enters that town
    public int UNUSEDubRebelSentiment;       // current rebel sentiment.  Events could change the starting value...
    public bool fLiberatedAlready;
    byte[] filler = new byte[19];					// reserved for expansion
}

