using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpAlliance.Core
{

    public class WeaponTypes
    {
        public static List<ARMOURTYPE> Armour = new()
        {

        };

        public static List<WEAPONTYPE> Weapon = new()
        {

        };

        public static List<MAGTYPE> Magazines = new()
        {
            // calibre,			 mag size,			ammo type
            new(CaliberType.AMMO9, 15, AmmoType.AMMO_REGULAR),
            new(CaliberType.AMMO9, 30, AmmoType.AMMO_REGULAR),
            new(CaliberType.AMMO9, 15, AmmoType.AMMO_AP),
            new(CaliberType.AMMO9, 30, AmmoType.AMMO_AP),
            new(CaliberType.AMMO9, 15, AmmoType.AMMO_HP),
            new(CaliberType.AMMO9, 30, AmmoType.AMMO_HP),
            new(CaliberType.AMMO38, 6, AmmoType.AMMO_REGULAR),
            new(CaliberType.AMMO38, 6, AmmoType.AMMO_AP),
            new(CaliberType.AMMO38, 6, AmmoType.AMMO_HP),
            new(CaliberType.AMMO45, 7, AmmoType.AMMO_REGULAR),
            new(CaliberType.AMMO45, 30, AmmoType.AMMO_REGULAR),
            new(CaliberType.AMMO45, 7, AmmoType.AMMO_AP),
            new(CaliberType.AMMO45, 30, AmmoType.AMMO_AP),
            new(CaliberType.AMMO45, 7, AmmoType.AMMO_HP),
            new(CaliberType.AMMO45, 30, AmmoType.AMMO_HP),
            new(CaliberType.AMMO357, 6, AmmoType.AMMO_REGULAR),
            new(CaliberType.AMMO357, 9, AmmoType.AMMO_REGULAR),
            new(CaliberType.AMMO357, 6, AmmoType.AMMO_AP),
            new(CaliberType.AMMO357, 9, AmmoType.AMMO_AP),
            new(CaliberType.AMMO357, 6, AmmoType.AMMO_HP),
            new(CaliberType.AMMO357, 9, AmmoType.AMMO_HP),
            new(CaliberType.AMMO545, 30, AmmoType.AMMO_AP),
            new(CaliberType.AMMO545, 30, AmmoType.AMMO_HP),
            new(CaliberType.AMMO556, 30, AmmoType.AMMO_AP),
            new(CaliberType.AMMO556, 30, AmmoType.AMMO_HP),
            new(CaliberType.AMMO762W, 10, AmmoType.AMMO_AP),
            new(CaliberType.AMMO762W, 30, AmmoType.AMMO_AP),
            new(CaliberType.AMMO762W, 10, AmmoType.AMMO_HP),
            new(CaliberType.AMMO762W, 30, AmmoType.AMMO_HP),
            new(CaliberType.AMMO762N, 5, AmmoType.AMMO_AP),
            new(CaliberType.AMMO762N, 20, AmmoType.AMMO_AP),
            new(CaliberType.AMMO762N, 5, AmmoType.AMMO_HP),
            new(CaliberType.AMMO762N, 20, AmmoType.AMMO_HP),
            new(CaliberType.AMMO47, 50, AmmoType.AMMO_SUPER_AP),
            new(CaliberType.AMMO57, 50, AmmoType.AMMO_AP),
            new(CaliberType.AMMO57, 50, AmmoType.AMMO_HP),
            new(CaliberType.AMMO12G, 7, AmmoType.AMMO_BUCKSHOT),
            new(CaliberType.AMMO12G, 7, AmmoType.AMMO_REGULAR),
            new(CaliberType.AMMOCAWS, 10, AmmoType.AMMO_BUCKSHOT),
            new(CaliberType.AMMOCAWS, 10, AmmoType.AMMO_SUPER_AP),
            new(CaliberType.AMMOROCKET, 5, AmmoType.AMMO_SUPER_AP),
            new(CaliberType.AMMOROCKET, 5, AmmoType.AMMO_HE),
            new(CaliberType.AMMOROCKET, 5, AmmoType.AMMO_HEAT),
            new(CaliberType.AMMODART, 1, AmmoType.AMMO_SLEEP_DART),
            new(CaliberType.AMMOFLAME, 5, AmmoType.AMMO_BUCKSHOT),
            new(CaliberType.NOAMMO, 0, 0),
        };
    }

    public record MAGTYPE(CaliberType ubCalibre, int ubMagSize, AmmoType ubAmmoType);
    public record ARMOURTYPE(ArmorClass ubArmourClass, int ubProtection, int ubDegradePercent);


    // ARMOUR CLASSES
    public enum ArmorClass
    {
        ARMOURCLASS_HELMET,
        ARMOURCLASS_VEST,
        ARMOURCLASS_LEGGINGS,
        ARMOURCLASS_PLATE,
        ARMOURCLASS_MONST,
        ARMOURCLASS_VEHICLE
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

    public enum GunType
    {
        NOT_GUN = 0,
        GUN_PISTOL,
        GUN_M_PISTOL,
        GUN_SMG,
        GUN_RIFLE,
        GUN_SN_RIFLE,
        GUN_AS_RIFLE,
        GUN_LMG,
        GUN_SHOTGUN
    };

    public struct WEAPONTYPE
    {
        public WeaponClass ubWeaponClass;               // handgun/shotgun/rifle/knife
        public GunType ubWeaponType;                    // exact type (for display purposes)
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
        public int sSound;
        public int sBurstSound;
        public int sReloadSound;
        public int sLocknLoadSound;
    }

    public enum AmmoType
    {
        AMMO_REGULAR = 0,
        AMMO_HP,
        AMMO_AP,
        AMMO_SUPER_AP,
        AMMO_BUCKSHOT,
        AMMO_FLECHETTE,
        AMMO_GRENADE,
        AMMO_MONSTER,
        AMMO_KNIFE,
        AMMO_HE,
        AMMO_HEAT,
        AMMO_SLEEP_DART,
        AMMO_FLAME,
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
}
