using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core;

public class Weapons
{
    public static EXPLOSIVETYPE[] Explosive =
{
	//	Type							Yield		Yield2		Radius		Volume		Volatility	Animation			Description
	//										-----		-------		------		------		----------	--------- 		------------------
	new(EXPLOSV.STUN,               1,          70,             4,              0,              0,          EXPLOSION_TYPES.STUN_BLAST		/* stun grenade       */),
    new(EXPLOSV.TEARGAS,        0,          20,             4,              0,              0,              EXPLOSION_TYPES.TARGAS_EXP		/* tear gas grenade   */),
    new(EXPLOSV.MUSTGAS,        15,         40,             4,              0,              0,                  EXPLOSION_TYPES.MUSTARD_EXP		/* mustard gas grenade*/),
    new(EXPLOSV.NORMAL,         15,         7,              3,              15,             1,                  EXPLOSION_TYPES.BLAST_1				/* mini hand grenade  */),
    new(EXPLOSV.NORMAL,         25,         10,             4,              25,             1,                  EXPLOSION_TYPES.BLAST_1				/* reg hand grenade   */),
    new(EXPLOSV.NORMAL,         40,         12,             5,              20,             10,                 EXPLOSION_TYPES.BLAST_2				/* RDX                */),
    new(EXPLOSV.NORMAL,         50,         15,             5,              50,             2,                  EXPLOSION_TYPES.BLAST_2				/* TNT (="explosives")*/),
    new(EXPLOSV.NORMAL,         60,         15,             6,              60,             2,                  EXPLOSION_TYPES.BLAST_2				/* HMX (=RDX+TNT)     */),
    new(EXPLOSV.NORMAL,         55,         15,             6,              55,             0,                  EXPLOSION_TYPES.BLAST_2				/* C1  (=RDX+min oil) */),
    new(EXPLOSV.NORMAL,         50,         22,             6,              50,             2,                  EXPLOSION_TYPES.BLAST_2				/* mortar shell       */),
    new(EXPLOSV.NORMAL,         30,         30,             2,              30,             2,                  EXPLOSION_TYPES.BLAST_1				/* mine               */),
    new(EXPLOSV.NORMAL,         65,         30,             7,              65,             0,              EXPLOSION_TYPES.BLAST_1				/* C4  ("plastique")  */),
    new(EXPLOSV.FLARE,          0,          0,              10,             0,              0,              EXPLOSION_TYPES.BLAST_1				/* trip flare				  */),
    new(EXPLOSV.NOISE,          0,          0,              50,             50,             0,              EXPLOSION_TYPES.BLAST_1				/* trip klaxon        */),
    new(EXPLOSV.NORMAL,         20,         0,              1,              20,             0,              EXPLOSION_TYPES.BLAST_1				/* shaped charge      */),
    new(EXPLOSV.FLARE,          0,          0,              10,             0,              0,              EXPLOSION_TYPES.BLAST_1			/* break light        */),
    new(EXPLOSV.NORMAL,         25,         5,              4,              25,             1,              EXPLOSION_TYPES.BLAST_1			/* GL grenade					*/),
    new(EXPLOSV.TEARGAS,        0,          20,             3,              0,              0,              EXPLOSION_TYPES.TARGAS_EXP		/* GL tear gas grenade*/),
    new(EXPLOSV.STUN,               1,          50,             4,              0,              0,          EXPLOSION_TYPES.STUN_BLAST	  /* GL stun grenade		*/),
    new(EXPLOSV.SMOKE,          0,          0,              3,              0,              0,              EXPLOSION_TYPES.SMOKE_EXP		/* GL smoke grenade		*/),
    new(EXPLOSV.SMOKE,          0,          0,              4,              0,              0,              EXPLOSION_TYPES.SMOKE_EXP		/* smoke grenade			*/),
    new(EXPLOSV.NORMAL,         60,         20,             6,              60,             2,                  EXPLOSION_TYPES.BLAST_2			/* Tank Shell         */),
    new(EXPLOSV.NORMAL,         100,        0,              0,              0,              0,                  EXPLOSION_TYPES.BLAST_1			/* Fake structure igniter*/),
    new(EXPLOSV.NORMAL,         100,        0,              1,              0,              0,                  EXPLOSION_TYPES.BLAST_1			/* creature cocktail */),
    new(EXPLOSV.NORMAL,         50,         10,             5,              50,             2,                  EXPLOSION_TYPES.BLAST_2			/* fake struct explosion*/),
    new(EXPLOSV.NORMAL,         50,         10,             5,              50,             2,                  EXPLOSION_TYPES.BLAST_3			/* fake vehicle explosion*/),
    new(EXPLOSV.TEARGAS,        0,          40,             4,              0,              0,              EXPLOSION_TYPES.TARGAS_EXP		/* big tear gas */),
    new(EXPLOSV.CREATUREGAS,    5,          0,              1,              0,              0,                  EXPLOSION_TYPES.NO_BLAST		  /* small creature gas*/),
    new(EXPLOSV.CREATUREGAS,    8,          0,              3,              0,              0,                  EXPLOSION_TYPES.NO_BLAST  		/* big creature gas*/),
    new(EXPLOSV.CREATUREGAS,    0,          0,              0,              0,              0,                  EXPLOSION_TYPES.NO_BLAST  		/* vry sm creature gas*/),
};
}

public record EXPLOSIVETYPE(
    EXPLOSV ubType,                   // type of explosive
    int ubDamage,             // damage value
    int ubStunDamage,     // stun amount / 100
    int ubRadius,            // radius of effect
    int ubVolume,             // sound radius of explosion
    int ubVolatility,     // maximum chance of accidental explosion
    EXPLOSION_TYPES ubAnimationID  // Animation enum to use
);

public enum EXPLOSV
{
    NORMAL,
    STUN,
    TEARGAS,
    MUSTGAS,
    FLARE,
    NOISE,
    SMOKE,
    CREATUREGAS,
};
