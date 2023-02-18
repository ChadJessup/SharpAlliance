using System.Collections.Generic;
using SharpAlliance.Core.Screens;

namespace SharpAlliance.Core;

public partial class Globals
{
    public static Dictionary<TOWNS, TOWN_LOYALTY> gTownLoyalty = new();
    public static bool[] gfTownUsesLoyalty = new bool[(int)TOWNS.NUM_TOWNS]
    {
        false,		// not a town - blank sector index
    	true,		// OMERTA
    	true,		// DRASSEN
    	true,		// ALMA
    	true,		// GRUMM
    	false,		// TIXA
    	true,		// CAMBRIA
    	false,		// SAN_MONA
    	false,		// ESTONI
    	false,		// ORTA
    	true,		// BALIME
    	true,		// MEDUNA
    	true,		// CHITZENA
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

