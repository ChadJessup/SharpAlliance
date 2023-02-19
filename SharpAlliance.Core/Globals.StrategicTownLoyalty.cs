using System.Collections.Generic;
using SharpAlliance.Core.Screens;

namespace SharpAlliance.Core;

public partial class Globals
{
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

