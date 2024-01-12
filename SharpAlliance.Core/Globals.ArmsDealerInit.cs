using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpAlliance.Core;

namespace SharpAlliance.Core;

public partial class Globals
{
    // To reduce memory fragmentation from frequent MemRealloc(), we allocate memory for more than one special slot each
    // time we run out of space.  Odds are that if we need one, we'll need another soon.
    public const int SPECIAL_ITEMS_ALLOCED_AT_ONCE = 3;
    // Once allocated, the special item slots remain allocated for the duration of the game, or until the dealer dies.
    // This is a little bit wasteful, but saves an awful lot of hassles, and avoid unnecessary memory fragmentation

    public const int MIN_REPAIR_TIME_IN_MINUTES = 15;     // minutes
    public const int MIN_REPAIR_COST = 10;// dollars

    // price classes
    public const int PRICE_CLASS_JUNK = 0;
    public const int PRICE_CLASS_CHEAP = 1;
    public const int PRICE_CLASS_EXPENSIVE = 2;


    public static int gubLastSpecialItemAddedAtElement = 255;


    public static ARMS_DEALER gbSelectedArmsDealerID;

    // THESE GET SAVED/RESTORED/RESET
    public static Dictionary<ARMS_DEALER, ARMS_DEALER_STATUS> gArmsDealerStatus = new();
    public static Dictionary<ARMS_DEALER, Dictionary<Items, DEALER_ITEM_HEADER>> gArmsDealersInventory = [];

    //
    // Setup the inventory arrays for each of the arms dealers
    //
    //	The arrays are composed of pairs of numbers
    //		The first is the item index
    //		The second is the amount of the items the dealer will try to keep in his inventory


    //
    // Tony ( Weapons only )
    //

    public static DEALER_POSSIBLE_INV[] gTonyInventory =
    {
	//Rare guns/ammo that Tony will buy although he won't ever sell them
	new(Items.  ROCKET_RIFLE,                   0 ),
    new(Items.  AUTO_ROCKET_RIFLE,      0 ),
    new(Items.AUTOMAG_III,                  0 ),
//	new(Items.FLAMETHROWER,					0 ),


	//Weapons
	new(Items.GLOCK_17,                         1 ),		/* Glock 17        */	
	new(Items.GLOCK_18,                         1 ),		/* Glock 18        */	
	new(Items.BERETTA_92F,                  1 ),		/* Beretta 92F     */	
	new(Items.BERETTA_93R,                  1 ),		/* Beretta 93R     */	
	new(Items.SW38,                                 1 ),		/* .38 S&W Special */	
	new(Items.BARRACUDA,                        1 ),		/* .357 Barracuda  */	
	new(Items.DESERTEAGLE,                  1 ),		/* .357 DesertEagle*/ 
	new(Items.M1911,                                1 ),		/* .45 M1911			 */ 
	new(Items.MP5K,                                 1 ),		/* H&K MP5K      	 */	
	new(Items.MAC10,                                1 ),		/* .45 MAC-10	     */

	new(Items.THOMPSON,                         1 ),		/* Thompson M1A1   */	
	new(Items.COMMANDO,                         1 ),		/* Colt Commando   */	
	new(Items.MP53,                                 1 ),		/* H&K MP53		 		 */	
	new(Items.AKSU74,                               1 ),		/* AKSU-74         */ 
	new(Items.TYPE85,                               1 ),		/* Type-85         */ 
	new(Items.SKS,                                  1 ),		/* SKS             */ 
	new(Items.DRAGUNOV,                         1 ),		/* Dragunov        */ 
	new(Items.M24,                                  1 ),		/* M24             */ 
	new(Items.AUG,                                  1 ),		/* Steyr AUG       */

	new(Items.G41,                                  1 ),		/* H&K G41         */ 
	new(Items.MINI14,                               1 ),		/* Ruger Mini-14   */ 
	new(Items.C7,                                       1 ),		/* C-7             */ 
	new(Items.FAMAS,                                1 ),		/* FA-MAS          */ 
	new(Items.AK74,                                 1 ),		/* AK-74           */ 
	new(Items.AKM,                                  1 ),		/* AKM             */ 
	new(Items.M14,                                  1 ),		/* M-14            */ 
	new(Items.G3A3,                                 1 ),		/* H&K G3A3        */ 
	new(Items.FNFAL,                                1 ),		/* FN-FAL          */

	new(Items.MINIMI,                               1 ),
    new(Items.RPK74,                                1 ),
    new(Items.HK21E,                                1 ),

    new(Items.M870,                                 1 ),		/* Remington M870  */	
	new(Items.SPAS15,                               1 ),		/* SPAS-15         */ 

	new(Items.GLAUNCHER,                        1 ),		/* grenade launcher*/
	new(Items.UNDER_GLAUNCHER,          1 ),		/* underslung g.l. */
	new(Items.ROCKET_LAUNCHER,          1 ),		/* rocket Launcher */
	new(Items.MORTAR,                               1 ),

	// SAP guns
	new(Items.G11,                                  1 ),
    new(Items.CAWS,                                 1 ),
    new(Items.P90,                                  1 ),

    new(Items.DART_GUN,                         1 ),


	//Ammo
	new(Items.CLIP9_15,                         8 ),
    new(Items.CLIP9_30,                         6 ),
    new(Items.CLIP9_15_AP,                  3 ),		/* CLIP9_15_AP */			
	new(Items.CLIP9_30_AP,                3 ),		/* CLIP9_30_AP */	
	new(Items.CLIP9_15_HP,                3 ),		/* CLIP9_15_HP */	
	new(Items.CLIP9_30_HP,                3 ),		/* CLIP9_30_HP */	

	new(Items.CLIP38_6,                         10),		/* CLIP38_6 */
	new(Items.CLIP38_6_AP,                5 ),		/* CLIP38_6_AP */
	new(Items.CLIP38_6_HP,                5 ),		/* CLIP38_6_HP */

	new(Items.CLIP45_7,                         6 ),		/* CLIP45_7 */				// 70

	new(Items.CLIP45_30,                      8 ),		/* CLIP45_30 */
	new(Items.CLIP45_7_AP,                  3 ),		/* CLIP45_7_AP */		
	new(Items.CLIP45_30_AP,                 3 ),		/* CLIP45_30_AP */	
	new(Items.CLIP45_7_HP,                  3 ),		/* CLIP45_7_HP */		
	new(Items.CLIP45_30_HP,                 3 ),		/* CLIP45_30_HP */	

	new(Items.CLIP357_6,                      6 ),		/* CLIP357_6 */			
	new(Items.CLIP357_9,                      5 ),		/* CLIP357_9 */			
	new(Items.CLIP357_6_AP,               3 ),		/* CLIP357_6_AP */	
	new(Items.CLIP357_9_AP,                 3 ),		/* CLIP357_9_AP */	
	new(Items.CLIP357_6_HP,                 3 ),		/* CLIP357_6_HP */			//80 
	new(Items.CLIP357_9_HP,                 3 ),		/* CLIP357_9_HP */	

	new(Items.CLIP545_30_AP,                6 ),		/* CLIP545_30_AP */	
	new(Items.CLIP545_30_HP,                3 ),		/* CLIP545_30_HP */	

	new(Items.CLIP556_30_AP,                6 ),		/* CLIP556_30_AP */	
	new(Items.CLIP556_30_HP,                3 ),		/* CLIP556_30_HP */	

	new(Items.CLIP762W_10_AP,               6 ),		/* CLIP762W_10_AP */
	new(Items.CLIP762W_30_AP,               5 ),		/* CLIP762W_30_AP */
	new(Items.CLIP762W_10_HP,               3 ),		/* CLIP762W_10_HP */
	new(Items.CLIP762W_30_HP,               3 ),		/* CLIP762W_30_HP */

	new(Items.CLIP762N_5_AP,                8 ),		/* CLIP762N_5_AP */			//90
	new(Items.CLIP762N_20_AP,               5 ),		/* CLIP762N_20_AP */
	new(Items.CLIP762N_5_HP,                3 ),		/* CLIP762N_5_HP */	
	new(Items.CLIP762N_20_HP,               3 ),		/* CLIP762N_20_HP */	

	new(Items.CLIP47_50_SAP,                5 ),		/* CLIP47_50_SAP */		

	new(Items.CLIP57_50_AP,                 6 ),		/* CLIP57_50_AP */		
	new(Items.CLIP57_50_HP,                 3 ),		/* CLIP57_50_HP */		

	new(Items.CLIP12G_7,                        9 ),		/* CLIP12G_7 */				
	new(Items.CLIP12G_7_BUCKSHOT,   9 ),		/* CLIP12G_7_BUCKSHOT */

	new(Items.CLIPCAWS_10_SAP,          5 ),		/* CLIPCAWS_10_SAP */			
	new(Items.CLIPCAWS_10_FLECH,        3 ),		/* CLIPCAWS_10_FLECH */			//100

	new(Items.CLIPROCKET_AP,                3 ),
    new(Items.CLIPROCKET_HE,                1 ),
    new(Items.CLIPROCKET_HEAT,          1 ),

    new(Items.CLIPDART_SLEEP,               5   ),

//	new(Items.CLIPFLAME,						5	),

	// "launchables" (New! From McCain!) - these are basically ammo
	new(Items.GL_HE_GRENADE,                2 ),
    new(Items.GL_TEARGAS_GRENADE,       2 ),
    new(Items.GL_STUN_GRENADE,          2 ),
    new(Items.GL_SMOKE_GRENADE,         2 ),
    new(Items.  MORTAR_SHELL,                   1 ),

	// knives
	new(Items.  COMBAT_KNIFE,                   3 ),
    new(Items.  THROWING_KNIFE,             6 ),
    new(Items.  BRASS_KNUCKLES,             1 ),
    new(Items.  MACHETE,                            1 ),

	// attachments
	new(Items.SILENCER,                         3 ),
    new(Items.SNIPERSCOPE,                  3 ),
    new(Items.LASERSCOPE,                       1 ),
    new(Items.BIPOD,                                3 ),
    new(Items.DUCKBILL,                         2 ),

/*
	// grenades
	new(Items.STUN_GRENADE,					5 ),
	new(Items.TEARGAS_GRENADE,			5 ),
	new(Items.MUSTARD_GRENADE,			5 ),
	new(Items.MINI_GRENADE,					5 ),
	new(Items.HAND_GRENADE,					5 ),
	new(Items.SMOKE_GRENADE,				5 ),
*/

	new(LAST_DEALER_ITEM, NO_DEALER_ITEM ),		//Last One
};


    //
    // Devin		( Explosives )
    //
    public static DEALER_POSSIBLE_INV[] gDevinInventory =
    {
    new(Items.  STUN_GRENADE,                           3 ),
    new(Items.  TEARGAS_GRENADE,                    3 ),
    new(Items.  MUSTARD_GRENADE,                    2 ),
    new(Items.  MINI_GRENADE,                           3 ),
    new(Items.  HAND_GRENADE,                           2 ),
    new(Items.SMOKE_GRENADE,                        3 ),

    new(Items.  GL_HE_GRENADE,                      2 ),
    new(Items.  GL_TEARGAS_GRENADE,             2 ),
    new(Items.  GL_STUN_GRENADE,                    2 ),
    new(Items.  GL_SMOKE_GRENADE,                   2 ),
    new(Items.  MORTAR_SHELL,                           1 ),

    new(Items.  CLIPROCKET_AP,                      1 ),
    new(Items.CLIPROCKET_HE,                        1 ),
    new(Items.CLIPROCKET_HEAT,                  1 ),

    new(Items.DETONATOR,                                10),
    new(Items.REMDETONATOR,                         5 ),
    new(Items.REMOTEBOMBTRIGGER,                5 ),

    new(Items.  MINE,                                           6 ),
    new(Items.  RDX,                                            5 ),
    new(Items.  TNT,                                            5 ),
    new(Items.  C1,                                             4 ),
    new(Items.  HMX,                                            3 ),
    new(Items.  C4,                                             2 ),

    new(Items.  SHAPED_CHARGE,                      5 ),

//	{	TRIP_FLARE,								2 ),
//	{	TRIP_KLAXON,							2 ),

	new(Items.GLAUNCHER,                                1 ),		/* grenade launcher*/
	new(Items.UNDER_GLAUNCHER,                  1 ),		/* underslung g.l. */
	new(Items.ROCKET_LAUNCHER,                  1 ),		/* rocket Launcher */
	new(Items.MORTAR,                                       1 ),

    new(Items.  METALDETECTOR,                      2 ),
    new(Items.  WIRECUTTERS,                            1 ),
    new(Items.  DUCT_TAPE,                              1 ),

    new(LAST_DEALER_ITEM, NO_DEALER_ITEM ),		//Last One
};


    //
    // Franz	(Expensive pawn shop )
    //
    public static DEALER_POSSIBLE_INV[] gFranzInventory =
    {
    new(Items.NIGHTGOGGLES,                         3 ),
    new(Items.LASERSCOPE,                               3 ),
    new(Items.METALDETECTOR,                        2 ),
    new(Items.EXTENDEDEAR,                          2 ),
    new(Items.DART_GUN,                                   1 ),
    new(Items.KEVLAR_VEST,                          1   ),
    new(Items.KEVLAR_LEGGINGS,                  1 ),
    new(Items.KEVLAR_HELMET,                        1   ),
    new(Items.KEVLAR2_VEST,                         1 ),
    new(Items.SPECTRA_VEST,                           1 ),
    new(Items.SPECTRA_LEGGINGS,                   1 ),
    new(Items.SPECTRA_HELMET,                     1 ),
    new(Items.CERAMIC_PLATES,                       1 ),
    new(Items.CAMOUFLAGEKIT,                      1 ),
    new(Items.VIDEO_CAMERA,                         1 ),		// for robot quest
	new(Items.LAME_BOY,                                 1 ),
    new(Items.FUMBLE_PAK,                               1 ),
    new(Items.GOLDWATCH,                                1 ),
    new(Items.GOLFCLUBS,                                1 ),
    new(LAST_DEALER_ITEM, NO_DEALER_ITEM ),		//Last One
};


    //
    // Keith		( Cheap Pawn Shop )
    //
    public static DEALER_POSSIBLE_INV[] gKeithInventory =
    {
        new(Items.FIRSTAIDKIT, 5 ),

        // WARNING: Keith must not carry any guns, it would conflict with his story/quest

        new(Items.COMBAT_KNIFE, 2),
        new(Items.THROWING_KNIFE, 3),
        new(Items.BRASS_KNUCKLES, 1),
        new(Items.MACHETE, 1),
        new(Items.SUNGOGGLES,               3 ),
        new(Items.FLAK_JACKET,          2   ),
        new(Items.STEEL_HELMET,         3 ),
        new(Items.LEATHER_JACKET,       1 ),
        new(Items.CANTEEN,                  5 ),
        new(Items.CROWBAR,                  1 ),
        new(Items.JAR,                          6 ),
        new(Items.  TOOLKIT,                    1 ),
        new(Items.  GASMASK,                    1 ),
        new(Items.SILVER_PLATTER,       1 ),
        new(Items.WALKMAN,                  1 ),
        new(Items.PORTABLETV,               1 ),
        new(LAST_DEALER_ITEM, NO_DEALER_ITEM ),		//Last One
    };


    //
    // Sam		( Hardware )
    //
    public static DEALER_POSSIBLE_INV[] gSamInventory =
    {
    new(Items.FIRSTAIDKIT,          3 ),
    new(Items.LOCKSMITHKIT,         4 ),
    new(Items.TOOLKIT,                  3 ),
    new(Items.CANTEEN,                  5 ),
    new(Items.CROWBAR,                  3 ),
    new(Items.WIRECUTTERS,          3 ),
    new(Items.DUCKBILL,                 3 ),
    new(Items.JAR,                          12),
    new(Items.  BREAK_LIGHT,            12),		// flares
	new(Items.  METALDETECTOR,      1 ),
    new(Items.VIDEO_CAMERA,         1 ),
    new(Items.QUICK_GLUE,               3 ),
    new(Items.COPPER_WIRE,          5 ),
    new(Items.BATTERIES,                10 ),
    new(Items.CLIP9_15,                 5 ),
    new(Items.CLIP9_30,                 5 ),
    new(Items.CLIP38_6,                 5 ),
    new(Items.CLIP45_7,                 5 ),
    new(Items.CLIP45_30,                5 ),
    new(Items.CLIP357_6,                5 ),
    new(Items.CLIP357_9,                5 ),
    new(Items.CLIP12G_7,                    9 ),
    new(Items.CLIP12G_7_BUCKSHOT,  9 ),
    new(LAST_DEALER_ITEM, NO_DEALER_ITEM ),		//Last One
};


    //
    // Jake			( Junk )
    //
    public static DEALER_POSSIBLE_INV[] gJakeInventory =
    {
    new(Items.FIRSTAIDKIT,          4 ),
    new(Items.MEDICKIT,                 3 ),

    new(Items.SW38,                         1 ),
    new(Items.CLIP38_6,                 5 ),

    new(Items.JAR,                          3 ),
    new(Items.CANTEEN,                  2 ),
    new(Items.BEER,                         6 ),

    new(Items.CROWBAR,                  1 ),
    new(Items.WIRECUTTERS,          1 ),

    new(Items.COMBAT_KNIFE,         1 ),
    new(Items.THROWING_KNIFE,       1 ),
    new(Items.  BRASS_KNUCKLES,     1 ),
    new(Items.MACHETE,                  1 ),

    new(Items.  BREAK_LIGHT,            5 ),		// flares

	new(Items.  BIPOD,                      1 ),

    new(Items.TSHIRT,                       6 ),
    new(Items.CIGARS,                       3 ),
    new(Items.PORNOS,                       1 ),

    new(Items.LOCKSMITHKIT,         1 ),
    new(LAST_DEALER_ITEM, NO_DEALER_ITEM ),		//Last One
};


    //
    // Howard		( Pharmaceuticals )
    //
    public static DEALER_POSSIBLE_INV[] gHowardInventory =
    {
    new(Items.FIRSTAIDKIT,              10),
    new(Items.MEDICKIT,                     5 ),
    new(Items.ADRENALINE_BOOSTER,   5 ),
    new(Items.REGEN_BOOSTER,            5 ),

    new(Items.ALCOHOL,                      3 ),
    new(Items.  COMBAT_KNIFE,               2 ),

    new(Items.  CLIPDART_SLEEP,         5 ),

    new(Items.  CHEWING_GUM,                3 ),

    new(LAST_DEALER_ITEM, NO_DEALER_ITEM ),		//Last One
};


    //
    // Gabby			( Creature parts and Blood )
    //
    public static DEALER_POSSIBLE_INV[] gGabbyInventory =
    {
    new(Items.JAR,                                          12 ),
    new(Items.JAR_ELIXIR,                               3 ),
	// buys these, but can't supply them (player is the only source)
	new(Items.  JAR_CREATURE_BLOOD,             0 ),
    new(Items.JAR_QUEEN_CREATURE_BLOOD, 0 ),
    new(Items.BLOODCAT_CLAWS,                       0 ),
    new(Items.BLOODCAT_TEETH,                       0 ),
    new(Items.BLOODCAT_PELT,                        0 ),
    new(Items.CREATURE_PART_CLAWS,          0 ),
    new(Items.CREATURE_PART_FLESH,          0 ),
    new(Items.CREATURE_PART_ORGAN,          0 ),

    new(LAST_DEALER_ITEM, NO_DEALER_ITEM ),		//Last One
};


    //
    // Frank  ( Alcohol )
    //
    public static DEALER_POSSIBLE_INV[] gFrankInventory =
    {
    new(Items.BEER,                         12 ),
    new(Items.WINE,                         6 ),
    new(Items.ALCOHOL,                  9 ),

    new(LAST_DEALER_ITEM, NO_DEALER_ITEM ),		//Last One
};


    //
    // Elgin  ( Alcohol )
    //
    public static DEALER_POSSIBLE_INV[] gElginInventory =
    {
    new(Items.BEER,                         12 ),
    new(Items.WINE,                         6 ),
    new(Items.ALCOHOL,                  9 ),

    new(LAST_DEALER_ITEM, NO_DEALER_ITEM ),		//Last One
};


    //
    // Manny  ( Alcohol )
    //
    public static DEALER_POSSIBLE_INV[] gMannyInventory =
    {
    new(Items.BEER,                         12 ),
    new(Items.WINE,                         6 ),
    new(Items.ALCOHOL,                  9 ),

    new(LAST_DEALER_ITEM, NO_DEALER_ITEM ),		//Last One
};


    //
    // Herve Santos		( Alcohol )
    //
    public static DEALER_POSSIBLE_INV[] gHerveInventory =
    {
    new(Items.BEER,                         12 ),
    new(Items.WINE,                         6 ),
    new(Items.ALCOHOL,                  9 ),

    new(LAST_DEALER_ITEM, NO_DEALER_ITEM ),		//Last One
};


    //
    // Peter Santos ( Alcohol )
    //
    public static DEALER_POSSIBLE_INV[] gPeterInventory =
    {
    new(Items.BEER,                         12 ),
    new(Items.WINE,                         6 ),
    new(Items.ALCOHOL,                  9 ),

    new(LAST_DEALER_ITEM, NO_DEALER_ITEM ),		//Last One
};


    //
    // Alberto Santos		( Alcohol )
    //
    public static DEALER_POSSIBLE_INV[] gAlbertoInventory =
    {
    new(Items.BEER,                         12 ),
    new(Items.WINE,                         6 ),
    new(Items.ALCOHOL,                  9 ),

    new(LAST_DEALER_ITEM, NO_DEALER_ITEM ),		//Last One
};


    //
    // Carlo Santos		( Alcohol )
    //
    public static DEALER_POSSIBLE_INV[] gCarloInventory =
    {
    new(Items.BEER,                         12 ),
    new(Items.WINE,                         6 ),
    new(Items.ALCOHOL,                  9 ),

    new(LAST_DEALER_ITEM, NO_DEALER_ITEM ),		//Last One
};


    //
    // Micky	( BUYS Animal / Creature parts )
    //

    public static DEALER_POSSIBLE_INV[] gMickyInventory =
    {
    	// ONLY BUYS THIS STUFF, DOESN'T SELL IT
    	new(Items.BLOODCAT_CLAWS,   0 ),
        new(Items.BLOODCAT_TEETH,   0 ),
        new(Items.BLOODCAT_PELT,        0 ),
        new(Items.CREATURE_PART_CLAWS,  0 ),
        new(Items.CREATURE_PART_FLESH,  0 ),
        new(Items.CREATURE_PART_ORGAN,  0 ),
        new(Items.JAR_QUEEN_CREATURE_BLOOD, 0 ),

        new(LAST_DEALER_ITEM, NO_DEALER_ITEM ),		//Last One
    };


    //
    // Arnie		( Weapons REPAIR )
    //
    public static DEALER_POSSIBLE_INV[] gArnieInventory =
    {
    	// NO INVENTORY
    
    	new(LAST_DEALER_ITEM, NO_DEALER_ITEM ),		//Last One
    };

    //
    // Perko			( REPAIR)
    //
    public static DEALER_POSSIBLE_INV[] gPerkoInventory =
    {
    	// NO INVENTORY
    
    	new(LAST_DEALER_ITEM, NO_DEALER_ITEM ),		//Last One
    };

    //
    // Fredo			( Electronics REPAIR)
    //
    public static DEALER_POSSIBLE_INV[] gFredoInventory =
    {
    	// NO INVENTORY
    
    	new(LAST_DEALER_ITEM, NO_DEALER_ITEM ),		//Last One
    };

    // This table controls the order items appear in inventory at BR's and dealers, and which kinds of items are sold used
    public static ITEM_SORT_ENTRY[] DealerItemSortInfo =
    {
//  item class					weapon class	sold used?
	new(IC.GUN,                       WeaponClass.HANDGUNCLASS, true  ),
    new(IC.GUN,                       WeaponClass.SHOTGUNCLASS, true  ),
    new(IC.GUN,                       WeaponClass.SMGCLASS,           true    ),
    new(IC.GUN,                       WeaponClass.RIFLECLASS,     true    ),
    new(IC.GUN,                       WeaponClass.MGCLASS,            false   ),
    new(IC.LAUNCHER,                  WeaponClass.NOGUNCLASS,     false   ),
    new(IC.AMMO,                      WeaponClass.NOGUNCLASS,     false   ),
    new(IC.GRENADE,                   WeaponClass.NOGUNCLASS,     false   ),
    new(IC.BOMB,                      WeaponClass.NOGUNCLASS,     false   ),
    new(IC.BLADE,                     WeaponClass.NOGUNCLASS,     false   ),
    new(IC.THROWING_KNIFE,            WeaponClass.NOGUNCLASS,     false   ),
    new(IC.PUNCH,                     WeaponClass.NOGUNCLASS,     false   ),
    new(IC.ARMOUR,                    WeaponClass.NOGUNCLASS,     true    ),
    new(IC.FACE,                      WeaponClass.NOGUNCLASS,     true    ),
    new(IC.MEDKIT,                    WeaponClass.NOGUNCLASS,     false   ),
    new(IC.KIT,                       WeaponClass.NOGUNCLASS,     false   ),
    new(IC.MISC,                      WeaponClass.NOGUNCLASS,     true    ),
    new(IC.THROWN,                    WeaponClass.NOGUNCLASS,     false   ),
    new(IC.KEY,                       WeaponClass.NOGUNCLASS,     false   ),

	// marks end of list
	new(IC.NONE,                      WeaponClass.NOGUNCLASS,     false),
};
}
