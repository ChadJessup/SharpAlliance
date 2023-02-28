using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpAlliance.Core
{
    [Flags]
    public enum ItemAttributes
    {
        // replaces candamage
        ITEM_DAMAGEABLE = 0x0001,
        // replaces canrepair
        ITEM_REPAIRABLE = 0x0002,
        // replaces waterdamage
        ITEM_WATER_DAMAGES = 0x0004,
        // replaces metal
        ITEM_METAL = 0x0008,
        // replaces sinkable
        ITEM_SINKS = 0x0010,
        // replaces seemeter
        ITEM_SHOW_STATUS = 0x0020,
        // for attachers/merges, hidden
        ITEM_HIDDEN_ADDON = 0x0040,
        // needs two hands
        ITEM_TWO_HANDED = 0x0080,
        // can't be found for sale
        ITEM_NOT_BUYABLE = 0x0100,
        // item is an attachment for something
        ITEM_ATTACHMENT = 0x0200,
        // item only belongs in the "big gun list"
        ITEM_BIGGUNLIST = 0x0400,
        // item should not be placed via the editor
        ITEM_NOT_EDITOR = 0x0800,
        // item defaults to undroppable
        ITEM_DEFAULT_UNDROPPABLE = 0x1000,
        // item is terrible for throwing
        ITEM_UNAERODYNAMIC = 0x2000,
        // item is electronic for repair (etc) purposes
        ITEM_ELECTRONIC = 0x4000,
        // item is a PERMANENT attachment
        ITEM_INSEPARABLE = 0x8000,

        // item flag combinations

        IF_STANDARD_GUN = ITEM_DAMAGEABLE | ITEM_WATER_DAMAGES | ITEM_REPAIRABLE | ITEM_SHOW_STATUS | ITEM_METAL | ITEM_SINKS,
        IF_TWOHANDED_GUN = IF_STANDARD_GUN | ITEM_TWO_HANDED,
        IF_STANDARD_BLADE = ITEM_DAMAGEABLE | ITEM_WATER_DAMAGES | ITEM_REPAIRABLE | ITEM_SHOW_STATUS | ITEM_METAL | ITEM_SINKS,
        IF_STANDARD_ARMOUR = ITEM_DAMAGEABLE | ITEM_REPAIRABLE | ITEM_SHOW_STATUS | ITEM_SINKS,
        IF_STANDARD_KIT = ITEM_DAMAGEABLE | ITEM_SHOW_STATUS | ITEM_SINKS,
        IF_STANDARD_CLIP = ITEM_SINKS | ITEM_METAL,
    }
    //EXPLOSIVE_GUN( x ) ( x == ROCKET_LAUNCHER || x == TANK_CANNON )

    [Flags]
    public enum IC
    {
        NONE = 0x00000001,
        GUN = 0x00000002,
        BLADE = 0x00000004,
        THROWING_KNIFE = 0x00000008,
        LAUNCHER = 0x00000010,
        TENTACLES = 0x00000020,
        THROWN = 0x00000040,
        PUNCH = 0x00000080,
        GRENADE = 0x00000100,
        BOMB = 0x00000200,
        AMMO = 0x00000400,
        ARMOUR = 0x00000800,
        MEDKIT = 0x00001000,
        KIT = 0x00002000,
        APPLIABLE = 0x00004000,
        FACE = 0x00008000,
        KEY = 0x00010000,
        MISC = 0x10000000,
        MONEY = 0x20000000,
        // PARENT TYPES
        WEAPON = GUN | BLADE | THROWING_KNIFE | LAUNCHER | TENTACLES,
        EXPLOSV = GRENADE | BOMB,
        BOBBY_GUN = GUN | LAUNCHER,
        BOBBY_MISC = GRENADE | BOMB | MISC | MEDKIT | KIT | BLADE | THROWING_KNIFE | PUNCH | FACE,
    }

    public enum ParentType
    {

    }

    public class INVTYPE
    {
        public IC usItemClass;
        public int ubClassIndex;
        public int ubCursor;
        public int bSoundType;
        public int ubGraphicType;
        public int ubGraphicNum;
        public int ubWeight; //2 units per kilogram; roughly 1 unit per pound
        public int ubPerPocket;
        public int usPrice;
        public int ubCoolness;
        public int bReliability;
        public int bRepairEase;
        public ItemAttributes fFlags;
    }

    public enum DetonatorType
    {
        BOMB_TIMED = 1,
        BOMB_REMOTE,
        BOMB_PRESSURE,
        BOMB_SWITCH
    }

    public class OBJECTTYPE
    {
        int usItem;
        int ubNumberOfObjects;
        int bGunStatus;            // status % of gun
        int ubGunAmmoType;    // ammo type, as per weapons.h
        int ubGunShotsLeft;   // duh, amount of ammo left
        int usGunAmmoItem;   // the item # for the item table
        int bGunAmmoStatus; // only for "attached ammo" - grenades, mortar shells
        int[] ubGunUnused = new int[Globals.MAX_OBJECTS_PER_SLOT - 6];
        int[] ubShotsLeft = new int[Globals.MAX_OBJECTS_PER_SLOT];
        int[] bStatus = new int[Globals.MAX_OBJECTS_PER_SLOT];
        int bMoneyStatus;
        int uiMoneyAmount;
        int[] ubMoneyUnused = new int[Globals.MAX_OBJECTS_PER_SLOT - 5];
        // this is used by placed bombs, switches, and the action item
        int bBombStatus;           // % status
        int bDetonatorType;        // timed, remote, or pressure-activated
        int usBombItem;              // the usItem of the bomb.
        int bDelay;                // >=0 values used only
        int bFrequency;        // >=0 values used only
        int ubBombOwner; // side which placed the bomb
        int bActionValue;// this is used by the ACTION_ITEM fake item
        int ubTolerance; // tolerance value for panic triggers
        int ubLocationID; // location value for remote non-bomb (special!) triggers
        int[] bKeyStatus = new int[6];
        int ubKeyID;
        int[] ubKeyUnused = new int[1];
        int ubOwnerProfile;
        int ubOwnerCivGroup;
        int[] ubOwnershipUnused = new int[6];
        // attached objects
        int[] usAttachItem = new int[Globals.MAX_ATTACHMENTS];
        int[] bAttachStatus = new int[Globals.MAX_ATTACHMENTS];

        int fFlags;
        int ubMission;
        int bTrap;        // 1-10 exp_lvl to detect
        int ubImprintID;  // ID of merc that item is imprinted on
        int ubWeight;
        int fUsed;                // flags for whether the item is used or not
    }

    internal class ItemIndexes
    {
        public const int FIRST_WEAPON = 1;
        public const int FIRST_AMMO = 71;
        public const int FIRST_EXPLOSIVE = 131;
        public const int FIRST_ARMOUR = 161;
        public const int FIRST_MISC = 201;
        public const int FIRST_KEY = 271;
    }

    public enum Items
    {
        NONE = 0,

        // weapons
        GLOCK_17 = ItemIndexes.FIRST_WEAPON,
        GLOCK_18,
        BERETTA_92F,
        BERETTA_93R,
        SW38,
        BARRACUDA,
        DESERTEAGLE,
        M1911,
        MP5K,
        MAC10,

        THOMPSON,
        COMMANDO,
        MP53,
        AKSU74,
        P90,
        TYPE85,
        SKS,
        DRAGUNOV,
        M24,
        AUG,

        G41,
        MINI14,
        C7,
        FAMAS,
        AK74,
        AKM,
        M14,
        FNFAL,
        G3A3,
        G11,

        M870,
        SPAS15,
        CAWS,
        MINIMI,
        RPK74,
        HK21E,
        COMBAT_KNIFE,
        THROWING_KNIFE,
        ROCK,
        GLAUNCHER,

        MORTAR,
        ROCK2,
        CREATURE_YOUNG_MALE_CLAWS,
        CREATURE_OLD_MALE_CLAWS,
        CREATURE_YOUNG_FEMALE_CLAWS,
        CREATURE_OLD_FEMALE_CLAWS,
        CREATURE_QUEEN_TENTACLES,
        CREATURE_QUEEN_SPIT,
        BRASS_KNUCKLES,
        UNDER_GLAUNCHER,

        ROCKET_LAUNCHER,
        BLOODCAT_CLAW_ATTACK,
        BLOODCAT_BITE,
        MACHETE,
        ROCKET_RIFLE,
        AUTOMAG_III,
        CREATURE_INFANT_SPIT,
        CREATURE_YOUNG_MALE_SPIT,
        CREATURE_OLD_MALE_SPIT,
        TANK_CANNON,

        DART_GUN,
        BLOODY_THROWING_KNIFE,
        FLAMETHROWER,
        CROWBAR,
        AUTO_ROCKET_RIFLE,

        MAX_WEAPONS = ItemIndexes.FIRST_AMMO - 1,

        CLIP9_15 = ItemIndexes.FIRST_AMMO,
        CLIP9_30,
        CLIP9_15_AP,
        CLIP9_30_AP,
        CLIP9_15_HP,
        CLIP9_30_HP,
        CLIP38_6,
        CLIP38_6_AP,
        CLIP38_6_HP,
        CLIP45_7,

        CLIP45_30,
        CLIP45_7_AP,
        CLIP45_30_AP,
        CLIP45_7_HP,
        CLIP45_30_HP,
        CLIP357_6,
        CLIP357_9,
        CLIP357_6_AP,
        CLIP357_9_AP,
        CLIP357_6_HP,

        CLIP357_9_HP,
        CLIP545_30_AP,
        CLIP545_30_HP,
        CLIP556_30_AP,
        CLIP556_30_HP,
        CLIP762W_10_AP,
        CLIP762W_30_AP,
        CLIP762W_10_HP,
        CLIP762W_30_HP,
        CLIP762N_5_AP,

        CLIP762N_20_AP,
        CLIP762N_5_HP,
        CLIP762N_20_HP,
        CLIP47_50_SAP,
        CLIP57_50_AP,
        CLIP57_50_HP,
        CLIP12G_7,
        CLIP12G_7_BUCKSHOT,
        CLIPCAWS_10_SAP,
        CLIPCAWS_10_FLECH,

        CLIPROCKET_AP,
        CLIPROCKET_HE,
        CLIPROCKET_HEAT,
        CLIPDART_SLEEP,

        CLIPFLAME,

        // explosives
        STUN_GRENADE = ItemIndexes.FIRST_EXPLOSIVE,
        TEARGAS_GRENADE,
        MUSTARD_GRENADE,
        MINI_GRENADE,
        HAND_GRENADE,
        RDX,
        TNT,
        HMX,
        C1,
        MORTAR_SHELL,

        MINE,
        C4,
        TRIP_FLARE,
        TRIP_KLAXON,
        SHAPED_CHARGE,
        BREAK_LIGHT,
        GL_HE_GRENADE,
        GL_TEARGAS_GRENADE,
        GL_STUN_GRENADE,
        GL_SMOKE_GRENADE,

        SMOKE_GRENADE,
        TANK_SHELL,
        STRUCTURE_IGNITE,
        CREATURE_COCKTAIL,
        STRUCTURE_EXPLOSION,
        GREAT_BIG_EXPLOSION,
        BIG_TEAR_GAS,
        SMALL_CREATURE_GAS,
        LARGE_CREATURE_GAS,
        VERY_SMALL_CREATURE_GAS,

        // armor
        FLAK_JACKET,                        //= FIRST_ARMOUR, ( We're out of space! )
        FLAK_JACKET_18,
        FLAK_JACKET_Y,
        KEVLAR_VEST,
        KEVLAR_VEST_18,
        KEVLAR_VEST_Y,
        SPECTRA_VEST,
        SPECTRA_VEST_18,
        SPECTRA_VEST_Y,
        KEVLAR_LEGGINGS,

        KEVLAR_LEGGINGS_18,
        KEVLAR_LEGGINGS_Y,
        SPECTRA_LEGGINGS,
        SPECTRA_LEGGINGS_18,
        SPECTRA_LEGGINGS_Y,
        STEEL_HELMET,
        KEVLAR_HELMET,
        KEVLAR_HELMET_18,
        KEVLAR_HELMET_Y,
        SPECTRA_HELMET,

        SPECTRA_HELMET_18,
        SPECTRA_HELMET_Y,
        CERAMPLATES,
        CREATURE_INFANT_HIDE,
        CREATURE_YOUNG_MALE_HIDE,
        CREATURE_OLD_MALE_HIDE,
        CREATURE_QUEEN_HIDE,
        LEATHER_JACKET,
        LEATHER_JACKET_W_KEVLAR,
        LEATHER_JACKET_W_KEVLAR_18,

        LEATHER_JACKET_W_KEVLAR_Y,
        CREATURE_YOUNG_FEMALE_HIDE,
        CREATURE_OLD_FEMALE_HIDE,
        TSHIRT,
        TSHIRT_DEIDRANNA,
        KEVLAR2_VEST,
        KEVLAR2_VEST_18,
        KEVLAR2_VEST_Y,

        // kits
        FIRSTAIDKIT = ItemIndexes.FIRST_MISC,
        MEDICKIT,
        TOOLKIT,
        LOCKSMITHKIT,
        CAMOUFLAGEKIT,
        BOOBYTRAPKIT,
        // miscellaneous
        SILENCER,
        SNIPERSCOPE,
        BIPOD,
        EXTENDEDEAR,

        NIGHTGOGGLES,
        SUNGOGGLES,
        GASMASK,
        CANTEEN,
        METALDETECTOR,
        COMPOUND18,
        JAR_QUEEN_CREATURE_BLOOD,
        JAR_ELIXIR,
        MONEY,
        JAR,

        JAR_CREATURE_BLOOD,
        ADRENALINE_BOOSTER,
        DETONATOR,
        REMDETONATOR,
        VIDEOTAPE,
        DEED,
        LETTER,
        TERRORIST_INFO,
        CHALICE,
        BLOODCAT_CLAWS,

        BLOODCAT_TEETH,
        BLOODCAT_PELT,
        SWITCH,
        // fake items
        ACTION_ITEM,
        REGEN_BOOSTER,
        SYRINGE_3,
        SYRINGE_4,
        SYRINGE_5,
        JAR_HUMAN_BLOOD,
        OWNERSHIP,

        // additional items
        LASERSCOPE,
        REMOTEBOMBTRIGGER,
        WIRECUTTERS,
        DUCKBILL,
        ALCOHOL,
        UVGOGGLES,
        DISCARDED_LAW,
        HEAD_1,
        HEAD_2,
        HEAD_3,
        HEAD_4,
        HEAD_5,
        HEAD_6,
        HEAD_7,
        WINE,
        BEER,
        PORNOS,
        VIDEO_CAMERA,
        ROBOT_REMOTE_CONTROL,
        CREATURE_PART_CLAWS,
        CREATURE_PART_FLESH,
        CREATURE_PART_ORGAN,
        REMOTETRIGGER,
        GOLDWATCH,
        GOLFCLUBS,
        WALKMAN,
        PORTABLETV,
        MONEY_FOR_PLAYERS_ACCOUNT,
        CIGARS,

        KEY_1 = ItemIndexes.FIRST_KEY,
        KEY_2,
        KEY_3,
        KEY_4,
        KEY_5,
        KEY_6,
        KEY_7,
        KEY_8,
        KEY_9,
        KEY_10,

        KEY_11,
        KEY_12,
        KEY_13,
        KEY_14,
        KEY_15,
        KEY_16,
        KEY_17,
        KEY_18,
        KEY_19,
        KEY_20,

        KEY_21,
        KEY_22,
        KEY_23,
        KEY_24,
        KEY_25,
        KEY_26,
        KEY_27,
        KEY_28,
        KEY_29,
        KEY_30,

        KEY_31,
        KEY_32,     // 302
        SILVER_PLATTER,
        DUCT_TAPE,
        ALUMINUM_ROD,
        SPRING,
        SPRING_AND_BOLT_UPGRADE,
        STEEL_ROD,
        QUICK_GLUE,
        GUN_BARREL_EXTENDER,

        STRING,
        TIN_CAN,
        STRING_TIED_TO_TIN_CAN,
        MARBLES,
        LAME_BOY,
        COPPER_WIRE,
        DISPLAY_UNIT,
        FUMBLE_PAK,
        XRAY_BULB,
        CHEWING_GUM, // 320

        FLASH_DEVICE,
        BATTERIES,
        ELASTIC,
        XRAY_DEVICE,
        SILVER,
        GOLD,
        GAS_CAN,
        UNUSED_26,
        UNUSED_27,
        UNUSED_28,

        UNUSED_29,
        UNUSED_30,
        UNUSED_31,
        UNUSED_32,
        UNUSED_33,
        UNUSED_34,
        UNUSED_35,
        UNUSED_36,
        UNUSED_37,
        UNUSED_38, // 340

        UNUSED_39,
        UNUSED_40,
        UNUSED_41,
        UNUSED_42,
        UNUSED_43,
        UNUSED_44,
        UNUSED_45,
        UNUSED_46,
        UNUSED_47,
        UNUSED_48, // 350

        MAXITEMS
    }
}
