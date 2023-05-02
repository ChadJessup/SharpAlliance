using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpAlliance.Core.SubSystems;
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

    public static void WindowHit(int sGridNo, int usStructureID, bool fBlowWindowSouth, bool fLargeForce)
    {
        STRUCTURE? pWallAndWindow;
        DB_STRUCTURE pWallAndWindowInDB;
        int sShatterGridNo;
        TileIndexes usTileIndex;
        ANITILE? pNode;

        // ATE: Make large force always for now ( feel thing )
        fLargeForce = true;

        // we have to do two things here: swap the window structure 
        // (right now just using the partner stuff in a chain from
        // intact to cracked to shattered) and display the 
        // animation if we've reached shattered

        // find the wall structure, and go one length along the chain	 
        pWallAndWindow = WorldStructures.FindStructureByID(sGridNo, usStructureID);
        if (pWallAndWindow == null)
        {
            return;
        }

        pWallAndWindow = StructureInternals.SwapStructureForPartner(sGridNo, pWallAndWindow);
        if (pWallAndWindow == null)
        {
            return;
        }

        // record window smash
        SaveLoadMap.AddWindowHitToMapTempFile(sGridNo);

        pWallAndWindowInDB = pWallAndWindow.pDBStructureRef.pDBStructure;

        if (fLargeForce)
        {
            // Force to destruction animation!
            if (pWallAndWindowInDB.bPartnerDelta != NO_PARTNER_STRUCTURE)
            {
                pWallAndWindow = StructureInternals.SwapStructureForPartner(sGridNo, pWallAndWindow);
                if (pWallAndWindow is not null)
                {
                    // record 2nd window smash
                    SaveLoadMap.AddWindowHitToMapTempFile(sGridNo);

                    pWallAndWindowInDB = pWallAndWindow.pDBStructureRef.pDBStructure;
                }
            }
        }

        RenderWorld.SetRenderFlags(RenderingFlags.FULL);

        if (pWallAndWindowInDB.ubArmour == MATERIAL.THICKER_METAL_WITH_SCREEN_WINDOWS)
        {
            // don't play any sort of animation or sound
            return;
        }

        if (pWallAndWindowInDB.bPartnerDelta != NO_PARTNER_STRUCTURE)
        { // just cracked; don't display the animation
            OppList.MakeNoise(NOBODY, sGridNo, 0, gpWorldLevelData[sGridNo].ubTerrainID, WINDOW_CRACK_VOLUME, NOISE.BULLET_IMPACT);
            return;
        }

        OppList.MakeNoise(NOBODY, sGridNo, 0, gpWorldLevelData[sGridNo].ubTerrainID, WINDOW_SMASH_VOLUME, NOISE.BULLET_IMPACT);
        if (pWallAndWindowInDB.ubWallOrientation == WallOrientation.INSIDE_TOP_RIGHT || pWallAndWindowInDB.ubWallOrientation == WallOrientation.OUTSIDE_TOP_RIGHT)
        {
            /*
                sShatterGridNo = sGridNo + 1;
                // check for wrapping around edge of map
                if (sShatterGridNo % WORLD_COLS == 0)
                {
                    // in which case we don't play the animation!
                    return;
                }*/
            if (fBlowWindowSouth)
            {
                usTileIndex = TileIndexes.WINDOWSHATTER1;
                sShatterGridNo = sGridNo + 1;
            }
            else
            {
                usTileIndex = TileIndexes.WINDOWSHATTER11;
                sShatterGridNo = sGridNo;
            }

        }
        else
        {
            /*
                sShatterGridNo = sGridNo + WORLD_COLS;
                // check for wrapping around edge of map
                if (sShatterGridNo % WORLD_ROWS == 0)
                {
                    // in which case we don't play the animation!
                    return;
                }*/
            if (fBlowWindowSouth)
            {
                usTileIndex = TileIndexes.WINDOWSHATTER6;
                sShatterGridNo = sGridNo + WORLD_COLS;
            }
            else
            {
                usTileIndex = TileIndexes.WINDOWSHATTER16;
                sShatterGridNo = sGridNo;
            }
        }

        ANITILE_PARAMS AniParams = new()
        {
            sGridNo = sShatterGridNo,
            ubLevelID = ANI.STRUCT_LEVEL,
            usTileType = TileTypeDefines.WINDOWSHATTER,
            usTileIndex = usTileIndex,
            sDelay = 50,
            sStartFrame = 0,
            uiFlags = ANITILEFLAGS.FORWARD,
        };

        pNode = TileAnimations.CreateAnimationTile(ref AniParams);

        // PlayJA2Sample(GLASS_SHATTER1 + Random(2), RATE_11025, MIDVOLUME, 1, SoundDir(sGridNo));
    }

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
    public const int MIN_MORTAR_RANGE = 150;		// minimum range of a mortar
}

public enum AIM
{
    BONUS_SAME_TARGET = 10,      // chance-to-hit bonus (in %)
    BONUS_PER_AP = 10,      // chance-to-hit bonus (in %) for aim
    BONUS_CROUCHING = 10,
    BONUS_PRONE = 20,
    BONUS_TWO_HANDED_PISTOL = 5,
    BONUS_FIRING_DOWN = 15,
    BONUS_TARGET_HATED = 20,
    BONUS_PSYCHO = 15,

    PENALTY_ONE_HANDED_PISTOL = 5,
    PENALTY_DUAL_PISTOLS = 20,
    PENALTY_SMG = 5,
    PENALTY_GASSED = 50,
    PENALTY_GETTINGAID = 20,
    PENALTY_PER_SHOCK = 5,     // 5% penalty per point of shock
    PENALTY_TARGET_BUDDY = 20,
    PENALTY_BURSTING = 10,
    PENALTY_GENTLEMAN = 15,
    PENALTY_TARGET_CROUCHED = 20,
    PENALTY_TARGET_PRONE = 40,
    PENALTY_BLIND = 80,
    PENALTY_FIRING_UP = 25,
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
