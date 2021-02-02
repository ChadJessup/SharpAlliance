using System;
using System.Collections.Generic;

namespace SharpAlliance.Core.SubSystems
{
    public class ItemSubSystem
    {
        public static class IntentoryConstants
        {
            public const int MAX_OBJECTS_PER_SLOT = 8;
            public const int MAX_ATTACHMENTS = 4;
            public const int MAX_MONEY_PER_SLOT = 20000;
        }

        public Dictionary<Items, Items> ReplacementGuns = new()
        {
            { Items.BARRACUDA, Items.DESERTEAGLE },
            { Items.M1911, Items.GLOCK_17 },
            { Items.GLOCK_18, Items.BERETTA_93R },
            { Items.BERETTA_92F, Items.GLOCK_17 },
            { Items.TYPE85, Items.BERETTA_93R },
            { Items.THOMPSON, Items.MP5K },
            { Items.MP53, Items.MP5K },
            { Items.SPAS15, Items.M870 },
            { Items.AKSU74, Items.MAC10 },
            { Items.SKS, Items.MINI14 },
            { Items.AKM, Items.G41 },
            { Items.G3A3, Items.G41 },
            { Items.AK74, Items.G41 },
            { Items.DRAGUNOV, Items.M24 },
            { Items.FAMAS, Items.M14 },
            { Items.AUG, Items.C7 },
            { Items.RPK74, Items.MINIMI },
            { Items.HK21E, Items.MINIMI },
            { 0, 0 }
        };


        public Dictionary<Items, Items> ReplacementAmmo = new()
        {
            { Items.CLIP545_30_AP, Items.CLIP556_30_AP },
            { Items.CLIP545_30_HP, Items.CLIP556_30_HP },
            { Items.CLIP762W_10_AP, Items.CLIP762N_5_AP },
            { Items.CLIP762W_30_AP, Items.CLIP762N_20_AP },
            { Items.CLIP762W_10_HP, Items.CLIP762N_5_HP },
            { Items.CLIP762W_30_HP, Items.CLIP762N_20_HP },
            { 0, 0 },
        };

        private Dictionary<Items, INVTYPE> items = new();

        // also used for ammo
        public bool ExtendedGunListGun(Items usGun)
        {
            return this[usGun].fFlags.HasFlag(ItemAttributes.ITEM_BIGGUNLIST);
        }

        public Items StandardGunListReplacement(Items usGun)
        {
            Items ubLoop;

            if (this.ExtendedGunListGun(usGun))
            {
                ubLoop = 0;
                while (this.ReplacementGuns[ubLoop] != 0)
                {
                    if (this.ReplacementGuns[ubLoop] == usGun)
                    {
                        return this.ReplacementGuns[ubLoop];
                    }

                    ubLoop++;
                }

                // ERROR!
                //AssertMsg(0, String("Extended gun with no replacement %d, CC:0", usGun));
                return Items.NONE;
            }
            else
            {
                return Items.NONE;
            }
        }

        public Items StandardGunListAmmoReplacement(Items usAmmo)
        {
            Items ubLoop;

            if (this.ExtendedGunListGun(usAmmo))
            {
                ubLoop = 0;
                while (this.ReplacementAmmo[ubLoop] != 0)
                {
                    if (this.ReplacementAmmo[ubLoop] == usAmmo)
                    {
                        return this.ReplacementAmmo[ubLoop];
                    }

                    ubLoop++;
                }
                // ERROR!
                //AssertMsg(0, String("Extended gun with no replacement %d, CC:0", usAmmo));

                return Items.NONE;
            }
            else
            {
                return Items.NONE;
            }
        }

        /// <summary>
        /// Returns the <seealso cref="INVTYPE"/> associated with <seealso cref="Items"/> passed in.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public INVTYPE this[Items item]
        {
            get { return this.items[item]; }
            set { this.items[item] = value; }
        }

        public Items FindReplacementMagazineIfNecessary(Items usOldGun, Items usOldAmmo, Items usNewGun)
        {
            Items usNewAmmo = Items.NONE;
            int oldGunIdx = (int)usOldGun;

            if ((WeaponTypes.Magazines[this[usOldAmmo].ubClassIndex].ubCalibre == WeaponTypes.Weapon[oldGunIdx].ubCalibre)
                && (WeaponTypes.Magazines[this[usOldAmmo].ubClassIndex].ubMagSize == WeaponTypes.Weapon[oldGunIdx].ubMagSize))
            {
                // must replace this!
                usNewAmmo = this.FindReplacementMagazine(WeaponTypes.Weapon[(int)usNewGun].ubCalibre, WeaponTypes.Weapon[(int)usNewGun].ubMagSize, WeaponTypes.Magazines[this.items[usOldAmmo].ubClassIndex].ubAmmoType);
            }

            return usNewAmmo;
        }

        public Items FindReplacementMagazine(CaliberType ubCalibre, int ubMagSize, AmmoType ubAmmoType)
        {
            int ubLoop;
            Items usDefault;

            ubLoop = 0;
            usDefault = Items.NONE;

            while (WeaponTypes.Magazines[ubLoop].ubCalibre != CaliberType.NOAMMO)
            {
                if (WeaponTypes.Magazines[ubLoop].ubCalibre == ubCalibre
                    && WeaponTypes.Magazines[ubLoop].ubMagSize == ubMagSize)
                {
                    if (WeaponTypes.Magazines[ubLoop].ubAmmoType == ubAmmoType)
                    {
                        return this.MagazineClassIndexToItemType(ubLoop);
                    }
                    else if (usDefault == Items.NONE)
                    {
                        // store this one to use if all else fails
                        usDefault = this.MagazineClassIndexToItemType(ubLoop);
                    }
                }

                ubLoop++;
            }

            return usDefault;
        }

        public Items MagazineClassIndexToItemType(int usMagIndex)
        {
            Items usLoop;

            // Note: if any ammo items in the item table are separated from the main group,
            // this function will have to be rewritten to scan the item table for an item
            // with item class ammo, which has class index usMagIndex
            for (usLoop = (Items)ItemIndexes.FIRST_AMMO; usLoop < Items.MAXITEMS; usLoop++)
            {
                if (this[usLoop].ubClassIndex == usMagIndex)
                {
                    return usLoop;
                }
            }

            return Items.NONE;
        }
    }
}
