using System;
using System.Collections.Generic;
using SharpAlliance.Core.Managers;
using SharpAlliance.Core.SubSystems;

namespace SharpAlliance.Core;


public class WeaponTypes
{
    public static List<ARMOURTYPE> Armour = new()
    {
	//	Class					      Protection	Degradation%			Description
	//  -------------       ----------  ------------      ----------------
	    new(ARMOURCLASS.VEST,               10,                 25          ), /* Flak jacket     */ 
	    new(ARMOURCLASS.VEST,               13,                 20          ), /* Flak jacket w X */ 
	    new(ARMOURCLASS.VEST,               16,                 15          ), /* Flak jacket w Y */ 
	    new(ARMOURCLASS.VEST,               15,                 20          ), /* Kevlar jacket   */ 
	    new(ARMOURCLASS.VEST,               19,                 15          ), /* Kevlar jack w X */ 
	    new(ARMOURCLASS.VEST,               24,                 10          ), /* Kevlar jack w Y */ 
	    new(ARMOURCLASS.VEST,               30,                 15          ), /* Spectra jacket  */ 
	    new(ARMOURCLASS.VEST,               36,                 10          ), /* Spectra jack w X*/
	    new(ARMOURCLASS.VEST,               42,                  5          ), /* Spectra jack w Y*/
	    new(ARMOURCLASS.LEGGINGS,           15,                 20          ), /* Kevlar leggings */	
	    new(ARMOURCLASS.LEGGINGS,           19,                 15          ), /* Kevlar legs w X */	
	    new(ARMOURCLASS.LEGGINGS,           24,                 10          ), /* Kevlar legs w Y */	
	    new(ARMOURCLASS.LEGGINGS,           30,                 15          ), /* Spectra leggings*/	
	    new(ARMOURCLASS.LEGGINGS,           36,                 10          ), /* Spectra legs w X*/	
	    new(ARMOURCLASS.LEGGINGS,           42,                  5          ), /* Spectra legs w Y*/	
	    new(ARMOURCLASS.HELMET,             10,                  5          ), /* Steel helmet    */	
	    new(ARMOURCLASS.HELMET,             15,                 20          ), /* Kevlar helmet   */	
	    new(ARMOURCLASS.HELMET,             19,                 15          ), /* Kevlar helm w X */	
	    new(ARMOURCLASS.HELMET,             24,                 10          ), /* Kevlar helm w Y */	
	    new(ARMOURCLASS.HELMET,             30,                 15          ), /* Spectra helmet  */	
	    new(ARMOURCLASS.HELMET,             36,                 10          ), /* Spectra helm w X*/	
	    new(ARMOURCLASS.HELMET,             42,                  5          ), /* Spectra helm w Y*/	
	    new(ARMOURCLASS.PLATE,              15,                 200         ), /* Ceramic plates  */ 
	    new(ARMOURCLASS.MONST,               3,                  0          ), /* Infant creature hide */
	    new(ARMOURCLASS.MONST,               5,                  0          ), /* Young male creature hide  */
	    new(ARMOURCLASS.MONST,               6,                  0          ), /* Male creature hide  */
	    new(ARMOURCLASS.MONST,              20,                  0          ), /* Queen creature hide  */
	    new(ARMOURCLASS.VEST,                2,                 25          ), /* Leather jacket    */ 
	    new(ARMOURCLASS.VEST,               12,                 30          ), /* Leather jacket w kevlar */ 
	    new(ARMOURCLASS.VEST,               16,                 25          ), /* Leather jacket w kevlar & compound 18 */ 
	    new(ARMOURCLASS.VEST,               19,                 20          ), /* Leather jacket w kevlar & queen blood */ 
	    new(ARMOURCLASS.MONST,               7,                  0          ), /* Young female creature hide */
	    new(ARMOURCLASS.MONST,               8,                  0          ), /* Old female creature hide  */
	    new(ARMOURCLASS.VEST,                1,                 25          ), /* T-shirt */ 
	    new(ARMOURCLASS.VEST,               22,                 20          ), /* Kevlar 2 jacket   */ 
	    new(ARMOURCLASS.VEST,               27,                 15          ), /* Kevlar 2 jack w X */ 
	    new(ARMOURCLASS.VEST,               32,                 10          ), /* Kevlar 2 jack w Y */ 
    };

    public static Dictionary<Items, WEAPONTYPE> Weapon = new()
    {
        //Description Ammo Bullet Ready 4xSng Burst	Burst Deadl Accu Clip Range Attack Impact Fire
        //										   Spd  Imp	Time	 ROF	 ROF		penal	iness	racy	Size					Vol   Vol			Sounds
        { Items.NONE, new(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 10, 0, 0, 0, 0, 0, 0, 0, 0) }, // nada!  must have min range of 10
            { Items.GLOCK_17, PISTOL( /* Glock 17			*/	CaliberType.AMMO9,      24, 21,     0,      14,     0,           0,      8,     0,  15, 120,        60,     5,          SoundDefine.S_GLOCK17,      SoundDefine.NO_WEAPON_SOUND      ) }, // wt 6  // Austria
	        { Items.GLOCK_18, M_PISTOL( /* Glock 18		*/	CaliberType.AMMO9,      24, 21,     0,      14,     5,        15,        9,     0,  15, 120,        60,     5,          SoundDefine.S_GLOCK18,      SoundDefine.S_BURSTTYPE1             ) }, // wt 6  // Austria
	        { Items.BERETTA_92F, PISTOL( /* Beretta 92F     */	CaliberType.AMMO9,      23, 22,     0,      16,     0,           0,      9,     0,  15, 120,        60,     5,          SoundDefine.S_BERETTA92,    SoundDefine.NO_WEAPON_SOUND      ) }, // wt 11 // Italy
	        { Items.BERETTA_93R, M_PISTOL( /* Beretta 93R   */	CaliberType.AMMO9,      23, 22,     0,      13,     5,        15,        9,     0,  15, 120,        60,     5,          SoundDefine.S_BERETTA93,    SoundDefine.S_BURSTTYPE1) }, // wt 11 // Italy
	        { Items.SW38, PISTOL(	/* .38 S&W Special */	CaliberType.AMMO38,     23, 22,     0,      11,     0,           0,      6,     0,   6, 130,        63,     5,          SoundDefine.S_SWSPECIAL,    SoundDefine.NO_WEAPON_SOUND) }, // wt 11 // Britain
	        { Items.BARRACUDA, PISTOL(	/* .357 Barracuda  */	CaliberType.AMMO357,    23, 24,     0,      11,     0,           0,      7,     0,   6, 135,        66,     6,          SoundDefine.S_BARRACUDA,    SoundDefine.NO_WEAPON_SOUND) }, // wt 10 // Belgium
	        { Items.DESERTEAGLE, PISTOL(	/* .357 DesertEagle*/	CaliberType.AMMO357,    24, 24,     0,      11,     0,           0,      7,     0,   9, 135,        66,     6,          SoundDefine.S_DESERTEAGLE,SoundDefine.NO_WEAPON_SOUND) }, // wt 17 // US
	        { Items.M1911, PISTOL(	/* .45 M1911       */	CaliberType.AMMO45,     24, 23,     0,      13,     0,           0,      9,     0,   7, 125,        69,     6,          SoundDefine.S_M1911,            SoundDefine.NO_WEAPON_SOUND      ) }, // wt 12 // US
	        { Items.MP5K, SMG(/* H&K MP5K      	 */	CaliberType.AMMO9,      23, 23,     1,      15,     5,         8,       17,     0,  30, 200,        75,     7,          SoundDefine.S_MP5K,             SoundDefine.S_BURSTTYPE1             ) }, // wt 21 // Germany; ROF 900 ?
	        { Items.MAC10, SMG(/* .45 MAC-10	     */ CaliberType.AMMO45,     23, 27,     2,      13,     5,         8,       20,     0,  30, 200,        75,     7,          SoundDefine.S_MAC10,            SoundDefine.S_BURSTTYPE1) }, // wt 28 // US; ROF 1090
	        { Items.THOMPSON, SMG(/* Thompson M1A1   */	CaliberType.AMMO45,     23, 24,     2,      10,     4,         8,       14,     0,  30, 200,        75,     7,          SoundDefine.S_THOMPSON,     SoundDefine.S_BURSTTYPE1) }, // wt 48 // US; ROF 700
	        { Items.COMMANDO, SMG(/* Colt Commando   */	CaliberType.AMMO556,    20, 29,     2,      15,     4,         8,       23,     0,  30, 200,        75,     7,          SoundDefine.S_COMMANDO,     SoundDefine.S_BURSTTYPE1) }, // wt 26 // US; ROF 
	        { Items.MP53, SMG(/* H&K MP53		 		 */ CaliberType.AMMO556,    22, 25,     2,      12,     3,         8,       15,     0,  30, 200,        75,     7,      SoundDefine.S_MP53,             SoundDefine.S_BURSTTYPE1             ) }, // wt 31 // Germany // eff range assumed; ROF 700 ?
	        { Items.AKSU74, SMG(/* AKSU-74         */ CaliberType.AMMO545,  21, 26,     2,      17,     4,         8,       21,     0,  30, 200,        75,     7,              SoundDefine.S_AKSU74,           SoundDefine.S_BURSTTYPE1             ) }, // wt 39 // USSR; ROF 800
	        { Items.P90, SMG(/* 5.7mm FN P90    */	CaliberType.AMMO57,     21, 30,     2,      15,     5,         8,       42,     0,  50, 225,        75,     7,          SoundDefine.S_P90,              SoundDefine.S_BURSTTYPE1             ) }, // wt 28 // Belgium; ROF 800-1000
	        { Items.TYPE85, SMG(/* Type-85         */	CaliberType.AMMO762W,   23, 23,     1,      10,     4,        11,       12,     0,  30, 200,        75,     7,          SoundDefine.S_TYPE85,           SoundDefine.S_BURSTTYPE1) }, // wt 19 // China; ROF 780
	        { Items.SKS, RIFLE(/* SKS             */ CaliberType.AMMO762W, 22, 31,     2,      13,     0,                      24,     0,  10, 300,        80,     8,              SoundDefine.S_SKS,              SoundDefine.S_BURSTTYPE1) }, // wt 39 // USSR
	        { Items.DRAGUNOV, SN_RIFLE(/* Dragunov      */	CaliberType.AMMO762W,   21, 36,     5,      11,     0,                      32,     0,  10, 750,        80,     8,          SoundDefine.S_DRAGUNOV,     SoundDefine.S_BURSTTYPE1) }, // wt 43 // USSR
	        { Items.M24, SN_RIFLE(/* M24           */	CaliberType.AMMO762N,   21, 36,     5,       8,     0,                      32,     0,   5, 800,        80,     8,          SoundDefine.S_M24,              SoundDefine.S_BURSTTYPE1) }, // wt 66 // US
	        { Items.AUG, ASRIFLE(/* Steyr AUG       */	CaliberType.AMMO556,    20, 30,     2,      13,     3,           8,     38,     0,  30, 500,        77,     8,              SoundDefine.S_AUG,              SoundDefine.S_BURSTTYPE1) }, // wt 36 // Austria; ROF 650
	        { Items.G41, ASRIFLE(/* H&K G41         */	CaliberType.AMMO556,    20, 29,     2,      13,     4,           8,     27,     0,  30, 300,        77,     8,              SoundDefine.S_G41,              SoundDefine.S_BURSTTYPE1) }, // wt 41 // Germany; ROF 850
	        { Items.MINI14, RIFLE(	/* Ruger Mini-14	 */	CaliberType.AMMO556,    20, 30,     2,      13,     0,                      20,     0,  30, 250,        77,     8,          SoundDefine.S_RUGERMINI,    SoundDefine.S_BURSTTYPE1) }, // wt 29 // US; ROF 750
	        { Items.C7, ASRIFLE(/* C-7             */	CaliberType.AMMO556,    20, 30,     2,      15,     5,           8,     41,     0,  30, 400,        77,     8,              SoundDefine.S_C7,                   SoundDefine.S_BURSTTYPE1) }, // wt 36 // Canada; ROF 600-940
	        { Items.FAMAS, ASRIFLE(/* FA-MAS          */	CaliberType.AMMO556,    20, 30,     2,      17,   5,             8,     32,     0,  30, 250,        77,     8,              SoundDefine.S_FAMAS,            SoundDefine.S_BURSTTYPE1             ) }, // wt 36 // France; ROF 900-1000
	        { Items.AK74, ASRIFLE(/* AK-74           */ CaliberType.AMMO545,  20, 28,     2,      17,     3,           8,     30,     0,  30, 350,        77,     8,                  SoundDefine.S_AK74,             SoundDefine.S_BURSTTYPE1             ) }, // wt 36 // USSR; ROF 650
	        { Items.AKM, ASRIFLE(/* AKM             */	CaliberType.AMMO762W,   22, 29,     2,      17,     3,          11,     25,     0,  30, 250,        77,     8,              SoundDefine.S_AKM,              SoundDefine.S_BURSTTYPE1             ) }, // wt 43 // USSR; ROF 600
	        { Items.M14, ASRIFLE(/* M-14            */ CaliberType.AMMO762N, 20, 33,     2,      13,     4,          11,     33,     0,  20, 330,        80,     8,                  SoundDefine.S_M14,              SoundDefine.S_BURSTTYPE1             ) }, // wt 29 // US; ROF 750
	        { Items.FNFAL, ASRIFLE(/* FN-FAL          */ CaliberType.AMMO762N, 20, 32,     2,      17,     3,          11,     41,     0,  20, 425,        80,     8,                  SoundDefine.S_FNFAL,            SoundDefine.S_BURSTTYPE1             ) }, // wt 43 // Belgium; ROF 
	        { Items.G3A3, ASRIFLE(/* H&K G3A3        */ CaliberType.AMMO762N, 21, 31,     2,      13,     3,          11,     26,     0,  20, 300,        80,     8,                  SoundDefine.S_G3A3,             SoundDefine.S_BURSTTYPE1) }, // wt 44 // Germany; ROF 500-600
	        { Items.G11, ASRIFLE(/* H&K G11         */ CaliberType.AMMO47,       20, 27,     2,      13,     3,           0,     40,     0,  50, 300,        80,     8,          SoundDefine.S_G11,              SoundDefine.S_BURSTTYPE1             ) }, // wt 38 // Germany; ROF 600
	        { Items.M870, SHOTGUN(/* Remington M870  */	CaliberType.AMMO12G,    24, 32,     2,       7,     0,           0,     14,     0,   7, 135,        80,     8,          SoundDefine.S_M870,             SoundDefine.S_BURSTTYPE1) }, // wt 36 // US; damage for solid slug
	        { Items.SPAS15, SHOTGUN(/* SPAS-15				 */ CaliberType.AMMO12G,    24, 32,     2,      10,     0,           0,     18,     0,   7, 135,        80,     8,          SoundDefine.S_SPAS,             SoundDefine.S_BURSTTYPE1) }, // wt 38 // Italy; semi-auto; damage for solid slug
	        { Items.CAWS, SHOTGUN(/* CAWS            */ CaliberType.AMMOCAWS, 24, 40,     2,      10,     3,          11,     44,     0,  10, 135,        80,     8,          SoundDefine.S_CAWS,             SoundDefine.S_BURSTTYPE1             ) }, // wt 41 // US; fires 8 flechettes at once in very close fixed pattern
	        { Items.MINIMI, LMG(    /* FN Minimi       */ CaliberType.AMMO556,  20, 28,     3,      13,     6,           5,     48,     0,  30, 500,        82,     8,          SoundDefine.S_FNMINI,           SoundDefine.S_BURSTTYPE1) }, // wt 68 // Belgium; ROF 750-1000
	        { Items.RPK74, LMG(		/* RPK-74          */ CaliberType.AMMO545,  21, 30,     2,      13,     5,           5,     49,     0,  30, 500,        82,     8,          SoundDefine.S_RPK74,            SoundDefine.S_BURSTTYPE1) }, // wt 48 // USSR; ROF 800?
	        { Items.HK21E, LMG(    /* H&K 21E         */ CaliberType.AMMO762N, 21, 32,     3,      13,     5,           7,     52,     0,  20, 500,        82,   8,            SoundDefine.S_21E,              SoundDefine.S_BURSTTYPE1) }, // wt 93 // Germany; ROF 800
                     // NB blade distances will be = strength + dexterity /2
	        { Items.COMBAT_KNIFE, BLADE(  /* Combat knife    */								18,                 12,                                 5,                       40,        2,                      SoundDefine.NO_WEAPON_SOUND) },
            { Items.THROWING_KNIFE, THROWINGBLADE(  /* Throwing knife  */				15,                 12,                               4,                        150,        2,                      SoundDefine.S_THROWKNIFE) },
            { Items.ROCK, ROCK() },//rock
	        { Items.GLAUNCHER, LAUNCHER( /* grenade launcher*/					30,             3,       5,                                 80,     0,          500,        20,     10,         SoundDefine.S_GLAUNCHER) },
            { Items.MORTAR, LAUNCHER( /* mortar */									30,             0,       5,                                 100,    0,          550,        20,     10,         SoundDefine.S_MORTAR_SHOT) },
            { Items.ROCK2, ROCK() },// another rock
	        { Items.CREATURE_YOUNG_MALE_CLAWS, BLADE( /* yng male claws */									14,                 10,                                  1,                      10,        2,                      SoundDefine.NO_WEAPON_SOUND ) },
            { Items.CREATURE_YOUNG_FEMALE_CLAWS, BLADE( /* yng fem claws */									18,                 10,                                  1,                      10,        2,                      SoundDefine.NO_WEAPON_SOUND ) },
            { Items.CREATURE_OLD_MALE_CLAWS, BLADE( /* old male claws */									20,                 10,                                  1,                      10,        2,                      SoundDefine.NO_WEAPON_SOUND ) },
            { Items.CREATURE_OLD_FEMALE_CLAWS, BLADE( /* old fem claws */									24,                 10,                                  1,                      10,        2,                      SoundDefine.NO_WEAPON_SOUND) },
            { Items.CREATURE_QUEEN_TENTACLES, BLADE( /* queen's tentacles */							20,                 10,                                  1,                      70,        2,                      SoundDefine.NO_WEAPON_SOUND) },
            { Items.CREATURE_QUEEN_SPIT, MONSTSPIT( /* queen's spit */								20,                 10,                                  1,             50, 300,        10,      5,         SoundDefine.ACR_SPIT) },
            { Items.BRASS_KNUCKLES, PUNCHWEAPON( /* brass knuckles */						12,                 15,                                  1,                                     0,                      0) },
            { Items.UNDER_GLAUNCHER, LAUNCHER( /* underslung GL */						30,             3,       7,                                 80,     0,          450,        20,     10,         SoundDefine.S_UNDER_GLAUNCHER) },
            { Items.ROCKET_LAUNCHER, LAW(	/* rocket laucher */							30,             0,       5,                                 80,     0,          500,        80,     10,         SoundDefine.S_ROCKET_LAUNCHER) },
            { Items.BLOODCAT_CLAWS, BLADE( /* bloodcat claws */									12,                 14,                                  1,                      10,        2,          SoundDefine.NO_WEAPON_SOUND) },
            { Items.BLOODCAT_BITE, BLADE( /* bloodcat bite */									24,                 10,                                  1,                      10,        2,          SoundDefine.NO_WEAPON_SOUND) },
            { Items.MACHETE, BLADE( /* machete */												24,                 9,                                   6,                      40,        2,                      SoundDefine.NO_WEAPON_SOUND) },
            { Items.ROCKET_RIFLE, RIFLE(/* rocket rifle */		CaliberType.AMMOROCKET, 20, 38,     2,      10,     0,                      62,     0,   5, 600,        80,     10,         SoundDefine.S_SMALL_ROCKET_LAUNCHER,    SoundDefine.NO_WEAPON_SOUND) },
            { Items.AUTOMAG_III, PISTOL(	/* automag III     */	CaliberType.AMMO762N,   24, 29,     1,       9,     0,           0,     13,     0,   5, 220,        72,      6,         SoundDefine.S_AUTOMAG,  SoundDefine.NO_WEAPON_SOUND) },
            { Items.CREATURE_INFANT_SPIT, MONSTSPIT( /* infant spit */								12,     13,                                              1,              5, 200,        10,      5,         SoundDefine.ACR_SPIT) },
            { Items.CREATURE_YOUNG_MALE_SPIT, MONSTSPIT( /* yng male spit */							16,     10,                                              1,             10, 200,        10,      5,         SoundDefine.ACR_SPIT) },
            { Items.CREATURE_OLD_MALE_SPIT, MONSTSPIT( /* old male spit */							20,     10,                                              1,             20, 200,        10,      5,         SoundDefine.ACR_SPIT) },
            { Items.TANK_CANNON, CANNON( /* tank cannon*/								30,             0,       8,                                 80,     0,          800,        90,     10,         SoundDefine.S_TANK_CANNON) },
            { Items.DART_GUN, PISTOL(	/* DART GUN		    */	CaliberType.AMMODART,   25,  2,     1,      13,     0,           0,     10,     0,   1, 200,        0,      0,          SoundDefine.NO_WEAPON_SOUND, SoundDefine.NO_WEAPON_SOUND) },
            { Items.BLOODY_THROWING_KNIFE, THROWINGBLADE(  /* Bloody Thrw KN */				15,                 12,                               3,                        150,        2,                      SoundDefine.S_THROWKNIFE) },
            { Items.FLAMETHROWER, SHOTGUN(/* Flamethrower */ CaliberType.AMMOFLAME,       24, 60,     2,      10,     0,           0,     53,     0,   5, 130,        40,     8,          SoundDefine.S_CAWS,             SoundDefine.S_BURSTTYPE1) },
            { Items.CROWBAR, PUNCHWEAPON( /* crowbar */									25,                 10,                                  4,                                     0,                      0) },
            { Items.AUTO_ROCKET_RIFLE, ASRIFLE(/* auto rckt rifle */CaliberType.AMMOROCKET,20, 38,     2,      12,   5,            10,     97,     0,   5, 600,        80,     10,         SoundDefine.S_SMALL_ROCKET_LAUNCHER,    SoundDefine.S_BURSTTYPE1) },
    };

    private static WEAPONTYPE ROCK()
        => new(WeaponClass.NOGUNCLASS, GUN.NOT_GUN, CaliberType.NOAMMO, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, SoundDefine.NO_WEAPON_SOUND, SoundDefine.NO_WEAPON_SOUND, SoundDefine.NO_WEAPON_SOUND, SoundDefine.NO_WEAPON_SOUND);

    private static WEAPONTYPE CANNON(int update, int rt, int rof,
        int deadl, int acc, int range, int av, int hv, SoundDefine sd)
    => new(WeaponClass.RIFLECLASS, GUN.NOT_GUN, CaliberType.NOAMMO, rt, rof, 0, 0, update, 80,
            deadl, acc, 1, range, 200, av, hv, sd, SoundDefine.NO_WEAPON_SOUND, SoundDefine.NO_WEAPON_SOUND, SoundDefine.NO_WEAPON_SOUND);

    private static WEAPONTYPE LAW(int update, int rt, int rof,
        int deadl, int acc, int range, int av, int hv, SoundDefine sd)
    => new(WeaponClass.RIFLECLASS, GUN.NOT_GUN, CaliberType.NOAMMO, rt, rof, 0, 0, update, 80,
        deadl, acc, 1, range, 200, av, hv, sd, SoundDefine.NO_WEAPON_SOUND, SoundDefine.NO_WEAPON_SOUND, SoundDefine.NO_WEAPON_SOUND);

    private static WEAPONTYPE PUNCHWEAPON(int impact, int rof, int deadl, int av, SoundDefine sd)
    => new(WeaponClass.KNIFECLASS, GUN.NOT_GUN, 0, 0, rof, 0, 0, 0, impact,
            deadl, 0, 0, 10, 200, av, 0, sd, SoundDefine.NO_WEAPON_SOUND, SoundDefine.NO_WEAPON_SOUND, SoundDefine.NO_WEAPON_SOUND);

    private static WEAPONTYPE MONSTSPIT(int impact, int rof,
        int deadl, int clip, int range, int av, int hv, SoundDefine sd)
        => new(WeaponClass.MONSTERCLASS, GUN.NOT_GUN, CaliberType.AMMOMONST, AP.READY_KNIFE, rof, 0, 0, 250, impact,
            deadl, 0, clip, range, 200, av, hv, sd, SoundDefine.NO_WEAPON_SOUND, SoundDefine.NO_WEAPON_SOUND, SoundDefine.NO_WEAPON_SOUND);

    private static WEAPONTYPE LAUNCHER(int update, int rt, int rof,
        int deadl, int acc, int range, int av, int hv, SoundDefine sd)
        => new(WeaponClass.RIFLECLASS, GUN.NOT_GUN, CaliberType.NOAMMO, rt, rof, 0, 0, update, 1,
            deadl, acc, 0, range, 200, av, hv, sd, SoundDefine.NO_WEAPON_SOUND, SoundDefine.NO_WEAPON_SOUND, SoundDefine.NO_WEAPON_SOUND);

    private static WEAPONTYPE THROWINGBLADE(int impact, int rof,
        int deadl, int range, int av, SoundDefine sd)
        => new(WeaponClass.KNIFECLASS, GUN.NOT_GUN, 0, AP.READY_KNIFE, rof, 0, 0, 0, impact,
            deadl, 0, 0, range, 200, av, 0, sd, SoundDefine.NO_WEAPON_SOUND, SoundDefine.NO_WEAPON_SOUND, SoundDefine.NO_WEAPON_SOUND);

    private static WEAPONTYPE BLADE(int impact, int rof,
        int deadl, int range, int av, SoundDefine sd)
        => new(WeaponClass.KNIFECLASS, GUN.NOT_GUN, 0, AP.READY_KNIFE, rof, 0, 0, 0, impact,
            deadl, 0, 0, range, 200, av, 0, sd, SoundDefine.NO_WEAPON_SOUND, SoundDefine.NO_WEAPON_SOUND, SoundDefine.NO_WEAPON_SOUND);

    private static WEAPONTYPE LMG(CaliberType ammo, int update, int impact, int rt, int rof, int burstrof, int burstpenal,
        int deadl, int acc, int clip, int range, int av, int hv, SoundDefine sd, SoundDefine bsd)
        => new(WeaponClass.MGCLASS, GUN.LMG, ammo, rt, rof, burstrof, burstpenal, update, impact,
            deadl, acc, clip, range, 200, av, hv, sd, bsd, SoundDefine.S_RELOAD_LMG, SoundDefine.S_LNL_LMG);

    private static WEAPONTYPE SHOTGUN(CaliberType ammo, int update, int impact, int rt, int rof, int burstrof, int burstpenal,
        int deadl, int acc, int clip, int range, int av, int hv, SoundDefine sd, SoundDefine bsd)
    => new(WeaponClass.SHOTGUNCLASS, GUN.SHOTGUN, ammo, rt, rof, burstrof, burstpenal, update, impact,
            deadl, acc, clip, range, 200, av, hv, sd, bsd, SoundDefine.S_RELOAD_SHOTGUN, SoundDefine.S_LNL_SHOTGUN);

    private static WEAPONTYPE ASRIFLE(CaliberType ammo, int update, int impact, int rt, int rof, int burstrof, int burstpenal,
        int deadl, int acc, int clip, int range, int av, int hv, SoundDefine sd, SoundDefine bsd)
    => new(WeaponClass.RIFLECLASS, GUN.AS_RIFLE, ammo, rt, rof, burstrof, burstpenal, update, impact,
            deadl, acc, clip, range, 200, av, hv, sd, bsd, SoundDefine.S_RELOAD_RIFLE, SoundDefine.S_LNL_RIFLE);

    private static WEAPONTYPE RIFLE(CaliberType ammo, int update, int impact, int rt, int rof, int burstrof,
        int deadl, int acc, int clip, int range, int av, int hv, SoundDefine sd, SoundDefine bsd)
    => new(WeaponClass.RIFLECLASS, GUN.RIFLE, ammo, rt, rof, burstrof, 0, update, impact,
        deadl, acc, clip, range, 200, av, hv, sd, bsd, SoundDefine.S_RELOAD_RIFLE, SoundDefine.S_LNL_RIFLE);

    private static WEAPONTYPE SN_RIFLE(CaliberType ammo, int update, int impact, int rt, int rof, int burstrof,
        int deadl, int acc, int clip, int range, int av, int hv, SoundDefine sd, SoundDefine bsd)
    => new(WeaponClass.RIFLECLASS, GUN.SN_RIFLE, ammo, rt, rof, burstrof, 0, update, impact,
            deadl, acc, clip, range, 200, av, hv, sd, bsd, SoundDefine.S_RELOAD_RIFLE, SoundDefine.S_LNL_RIFLE);

    private static WEAPONTYPE SMG(CaliberType ammo, int update, int impact, int rt, int rof, int burstrof, int burstpenal,
        int deadl, int acc, int clip, int range, int av, int hv, SoundDefine sd, SoundDefine bsd)
    => new(WeaponClass.SMGCLASS, GUN.SMG, ammo, rt, rof, burstrof, burstpenal, update, impact,
            deadl, acc, clip, range, 200, av, hv, sd, bsd, SoundDefine.S_RELOAD_SMG, SoundDefine.S_LNL_SMG);

    private static WEAPONTYPE M_PISTOL(CaliberType ammo, int update, int impact, int rt, int rof, int burstrof, int burstpenal,
        int deadl, int acc, int clip, int range, int av, int hv, SoundDefine sd, SoundDefine bsd)
    => new(WeaponClass.HANDGUNCLASS, GUN.M_PISTOL, ammo, rt, rof, burstrof, burstpenal, update, impact,
            deadl, acc, clip, range, 200, av, hv, sd, bsd, SoundDefine.S_RELOAD_PISTOL, SoundDefine.S_LNL_PISTOL);

    private static WEAPONTYPE PISTOL(CaliberType ammo, int update, int impact, int rt, int rof, int burstrof, int burstpenal,
         int deadl, int acc, int clip, int range, int av, int hv, SoundDefine sd, SoundDefine bsd)
    => new(WeaponClass.HANDGUNCLASS, GUN.PISTOL, ammo, rt, rof, burstrof, burstpenal, update, impact,
            deadl, acc, clip, range, 200, av, hv, sd, bsd, SoundDefine.S_RELOAD_PISTOL, SoundDefine.S_LNL_PISTOL);

    public static List<MAGTYPE> Magazine = new()
    {
        // calibre,			 mag size,			ammo type
        new(CaliberType.AMMO9, 15, AMMO.REGULAR),
        new(CaliberType.AMMO9, 30, AMMO.REGULAR),
        new(CaliberType.AMMO9, 15, AMMO.AP),
        new(CaliberType.AMMO9, 30, AMMO.AP),
        new(CaliberType.AMMO9, 15, AMMO.HP),
        new(CaliberType.AMMO9, 30, AMMO.HP),
        new(CaliberType.AMMO38, 6, AMMO.REGULAR),
        new(CaliberType.AMMO38, 6, AMMO.AP),
        new(CaliberType.AMMO38, 6, AMMO.HP),
        new(CaliberType.AMMO45, 7, AMMO.REGULAR),
        new(CaliberType.AMMO45, 30, AMMO.REGULAR),
        new(CaliberType.AMMO45, 7, AMMO.AP),
        new(CaliberType.AMMO45, 30, AMMO.AP),
        new(CaliberType.AMMO45, 7, AMMO.HP),
        new(CaliberType.AMMO45, 30, AMMO.HP),
        new(CaliberType.AMMO357, 6, AMMO.REGULAR),
        new(CaliberType.AMMO357, 9, AMMO.REGULAR),
        new(CaliberType.AMMO357, 6, AMMO.AP),
        new(CaliberType.AMMO357, 9, AMMO.AP),
        new(CaliberType.AMMO357, 6, AMMO.HP),
        new(CaliberType.AMMO357, 9, AMMO.HP),
        new(CaliberType.AMMO545, 30, AMMO.AP),
        new(CaliberType.AMMO545, 30, AMMO.HP),
        new(CaliberType.AMMO556, 30, AMMO.AP),
        new(CaliberType.AMMO556, 30, AMMO.HP),
        new(CaliberType.AMMO762W, 10, AMMO.AP),
        new(CaliberType.AMMO762W, 30, AMMO.AP),
        new(CaliberType.AMMO762W, 10, AMMO.HP),
        new(CaliberType.AMMO762W, 30, AMMO.HP),
        new(CaliberType.AMMO762N, 5, AMMO.AP),
        new(CaliberType.AMMO762N, 20, AMMO.AP),
        new(CaliberType.AMMO762N, 5, AMMO.HP),
        new(CaliberType.AMMO762N, 20, AMMO.HP),
        new(CaliberType.AMMO47, 50, AMMO.SUPER_AP),
        new(CaliberType.AMMO57, 50, AMMO.AP),
        new(CaliberType.AMMO57, 50, AMMO.HP),
        new(CaliberType.AMMO12G, 7, AMMO.BUCKSHOT),
        new(CaliberType.AMMO12G, 7, AMMO.REGULAR),
        new(CaliberType.AMMOCAWS, 10, AMMO.BUCKSHOT),
        new(CaliberType.AMMOCAWS, 10, AMMO.SUPER_AP),
        new(CaliberType.AMMOROCKET, 5, AMMO.SUPER_AP),
        new(CaliberType.AMMOROCKET, 5, AMMO.HE),
        new(CaliberType.AMMOROCKET, 5, AMMO.HEAT),
        new(CaliberType.AMMODART, 1, AMMO.SLEEP_DART),
        new(CaliberType.AMMOFLAME, 5, AMMO.BUCKSHOT),
        new(CaliberType.NOAMMO, 0, 0),
    };
}

public record MAGTYPE(CaliberType ubCalibre, int ubMagSize, AMMO ubAmmoType);
public record ARMOURTYPE(ARMOURCLASS ubArmourClass, int ubProtection, int ubDegradePercent);


// ARMOUR CLASSES
public enum ARMOURCLASS
{
    HELMET,
    VEST,
    LEGGINGS,
    PLATE,
    MONST,
    VEHICLE
};

public enum WeaponClass
{
    NOGUNCLASS,
    HANDGUNCLASS,
    SMGCLASS,
    RIFLECLASS,
    MGCLASS,
    SHOTGUNCLASS,
    KNIFECLASS,
    MONSTERCLASS,
    NUM_WEAPON_CLASSES
};

public enum GUN
{
    NOT_GUN = 0,
    PISTOL,
    M_PISTOL,
    SMG,
    RIFLE,
    SN_RIFLE,
    AS_RIFLE,
    LMG,
    SHOTGUN
};

public class WEAPONTYPE
{
    public WEAPONTYPE(
        WeaponClass ubWeaponClass,          // handgun/shotgun/rifle/knife
        GUN ubWeaponType,           // exact type (for display purposes)
        CaliberType ubCalibre,              // type of ammunition needed
        int ubReadyTime,            // APs to ready/unready weapon
        int ubShotsPer4Turns,       // maximum (mechanical) firing rate
        int ubShotsPerBurst,
        int ubBurstPenalty,         // % penalty per shot after first
        int ubBulletSpeed,          // bullet's travelling speed
        int ubImpact,               // weapon's max damage impact (size & speed)
        int ubDeadliness,           // comparative ratings of guns
        int bAccuracy,              // accuracy or penalty
        int ubMagSize,
        int usRange,
        int usReloadDelay,
        int ubAttackVolume,
        int ubHitVolume,
        SoundDefine sSound,
        SoundDefine sBurstSound,
        SoundDefine sReloadSound,
        SoundDefine sLocknLoadSound)
    {
        this.ubWeaponClass = ubWeaponClass;
        this.ubWeaponType = ubWeaponType;
        this.ubCalibre = ubCalibre;
        this.ubReadyTime = ubReadyTime;
        this.ubShotsPer4Turns = ubShotsPer4Turns;
        this.ubShotsPerBurst = ubShotsPerBurst;
        this.ubBurstPenalty = ubBurstPenalty;
        this.ubBulletSpeed = ubBulletSpeed;
        this.ubImpact = ubImpact;
        this.ubDeadliness = ubDeadliness;
        this.bAccuracy = bAccuracy;
        this.ubMagSize = ubMagSize;
        this.usRange = usRange;
        this.usReloadDelay = usReloadDelay;
        this.ubAttackVolume = ubAttackVolume;
        this.ubHitVolume = ubHitVolume;
        this.sSound = sSound;
        this.sBurstSound = sBurstSound;
        this.sReloadSound = sReloadSound;
        this.sLocknLoadSound = sLocknLoadSound;
    }

    public WeaponClass ubWeaponClass;               // handgun/shotgun/rifle/knife
    public GUN ubWeaponType;                    // exact type (for display purposes)
    public CaliberType ubCalibre;                   // type of ammunition needed
    public int ubReadyTime;                         // APs to ready/unready weapon
    public int ubShotsPer4Turns;                    // maximum (mechanical) firing rate
    public int ubShotsPerBurst;
    public int ubBurstPenalty;                      // % penalty per shot after first
    public int ubBulletSpeed;                       // bullet's travelling speed
    public int ubImpact;                            // weapon's max damage impact (size & speed)
    public int ubDeadliness;                        // comparative ratings of guns
    public int bAccuracy;                           // accuracy or penalty
    public int ubMagSize;
    public int usRange;
    public int usReloadDelay;
    public int ubAttackVolume;
    public int ubHitVolume;
    public SoundDefine sSound;
    public SoundDefine sBurstSound;
    public SoundDefine sReloadSound;
    public SoundDefine sLocknLoadSound;
}

public enum AMMO
{
    REGULAR = 0,
    HP,
    AP,
    SUPER_AP,
    BUCKSHOT,
    FLECHETTE,
    GRENADE,
    MONSTER,
    KNIFE,
    HE,
    HEAT,
    SLEEP_DART,
    FLAME,
}

public enum CaliberType
{
    NOAMMO = 0,
    AMMO38,
    AMMO9,
    AMMO45,
    AMMO357,
    AMMO12G,
    AMMOCAWS,
    AMMO545,
    AMMO556,
    AMMO762N,
    AMMO762W,
    AMMO47,
    AMMO57,
    AMMOMONST,
    AMMOROCKET,
    AMMODART,
    AMMOFLAME,
};
