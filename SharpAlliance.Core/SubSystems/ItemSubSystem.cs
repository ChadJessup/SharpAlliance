using System.Collections.Generic;
using System.Diagnostics;

namespace SharpAlliance.Core.SubSystems;

public class ItemSubSystem
{
    public static Dictionary<Items, Items> ReplacementGuns = new()
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


    public static Dictionary<Items, Items> ReplacementAmmo = new()
    {
        { Items.CLIP545_30_AP, Items.CLIP556_30_AP },
        { Items.CLIP545_30_HP, Items.CLIP556_30_HP },
        { Items.CLIP762W_10_AP, Items.CLIP762N_5_AP },
        { Items.CLIP762W_30_AP, Items.CLIP762N_20_AP },
        { Items.CLIP762W_10_HP, Items.CLIP762N_5_HP },
        { Items.CLIP762W_30_HP, Items.CLIP762N_20_HP },
        { 0, 0 },
    };

    // also used for ammo
    public static bool ExtendedGunListGun(Items usGun)
    {
        return Globals.Item[usGun].fFlags.HasFlag(ItemAttributes.ITEM_BIGGUNLIST);
    }

    public static bool RemoveAttachment(OBJECTTYPE? pObj, int bAttachPos, OBJECTTYPE? pNewObj)
    {
        Items bGrenade;

        if (pObj is null)
        {
            return false;
        }

        if (bAttachPos < 0 || bAttachPos >= Globals.MAX_ATTACHMENTS)
        {
            return (false);
        }
        if (pObj.usAttachItem[bAttachPos] == Globals.NOTHING)
        {
            return (false);
        }

        if (Globals.Item[pObj.usAttachItem[bAttachPos]].fFlags.HasFlag(ItemAttributes.ITEM_INSEPARABLE))
        {
            return (false);
        }

        // if pNewObj is passed in null, then we just delete the attachment
        if (pNewObj != null)
        {
            CreateItem(pObj.usAttachItem[bAttachPos], (int)pObj.bAttachStatus[bAttachPos], pNewObj);
        }

        pObj.usAttachItem[bAttachPos] = Globals.NOTHING;
        pObj.bAttachStatus[bAttachPos] = 0;

        if (pNewObj is not null && pNewObj.usItem == Items.UNDER_GLAUNCHER)
        {
            // look for any grenade; if it exists, we must make it an 
            // attachment of the grenade launcher
            bGrenade = FindAttachmentByClass(pObj, IC.GRENADE);
            if (bGrenade != Globals.ITEM_NOT_FOUND)
            {
                pNewObj.usAttachItem[0] = pObj.usAttachItem[bGrenade];
                pNewObj.bAttachStatus[0] = pObj.bAttachStatus[bGrenade];
                pObj.usAttachItem[bGrenade] = Globals.NOTHING;
                pObj.bAttachStatus[bGrenade] = 0;
                pNewObj.ubWeight = CalculateObjectWeight(pNewObj);
            }
        }

        RenumberAttachments(pObj);

        pObj.ubWeight = CalculateObjectWeight(pObj);
        return (true);
    }

    public static bool CreateItem(Items usItem, int bStatus, OBJECTTYPE? pObj)
    {
        bool fRet;

        pObj = new();

        if (usItem >= Items.MAXITEMS)
        {
            return (false);
        }

        if (Globals.Item[usItem].usItemClass == IC.GUN)
        {
            fRet = CreateGun(usItem, bStatus, pObj);
        }
        else if (Globals.Item[usItem].usItemClass == IC.AMMO)
        {
            fRet = CreateMagazine(usItem, pObj);
        }
        else
        {
            pObj.usItem = usItem;
            pObj.ubNumberOfObjects = 1;
            if (usItem == Items.MONEY)
            {
                // special case... always set status to 100 when creating
                // and use status value to determine amount!
                pObj.bStatus[0] = 100;
                pObj.uiMoneyAmount = bStatus * 50;
            }
            else
            {
                pObj.bStatus[0] = bStatus;
            }
            pObj.ubWeight = CalculateObjectWeight(pObj);
            fRet = true;
        }

        if (fRet)
        {
            if (Globals.Item[usItem].fFlags.HasFlag(ItemAttributes.ITEM_DEFAULT_UNDROPPABLE))
            {
                pObj.fFlags |= OBJECT.UNDROPPABLE;
            }
        }
        return (fRet);
    }

    public static bool CreateGun(Items usItem, int bStatus, OBJECTTYPE? pObj)
    {
        int usAmmo;


        Debug.Assert(pObj != null);
        if (pObj == null)
        {
            return (false);
        }


        pObj = new()
        {
            usItem = usItem,
            ubNumberOfObjects = 1,
            bGunStatus = bStatus,
            ubImprintID = SoldierControl.NO_PROFILE,
            ubWeight = CalculateObjectWeight(pObj),
        };

        if (Weapon[usItem].ubWeaponClass == WeaponClass.MONSTERCLASS)
        {
            pObj.ubGunShotsLeft = Weapon[usItem].ubMagSize;
            pObj.ubGunAmmoType = AMMO.MONSTER;
        }
        else if (EXPLOSIVE_GUN(usItem))
        {
            if (usItem == ROCKET_LAUNCHER)
            {
                pObj.ubGunShotsLeft = 1;
            }
            else
            {
                // cannon
                pObj.ubGunShotsLeft = 0;
            }
            pObj.bGunAmmoStatus = 100;
            pObj.ubGunAmmoType = 0;
        }
        else
        {
            usAmmo = DefaultMagazine(usItem);
            Debug.Assert(usAmmo != 0);
            if (usAmmo == 0)
            {
                // item's calibre & mag size not found in magazine list!
                return (false);
            }
            else
            {
                pObj.usGunAmmoItem = usAmmo;
                pObj.ubGunAmmoType = Magazine[Globals.Item[usAmmo].ubClassIndex].ubAmmoType;
                pObj.bGunAmmoStatus = 100;
                pObj.ubGunShotsLeft = Magazine[Globals.Item[usAmmo].ubClassIndex].ubMagSize;
                /*
                if (usItem == CAWS)
                {
                    pObj->usAttachItem[0] = DUCKBILL;
                    pObj->bAttachStatus[0] = 100;
                }
                */
            }
        }

        // succesful
        return (true);
    }

    public static bool CreateMagazine(Items usItem, OBJECTTYPE? pObj)
    {
        if (pObj == null)
        {
            return (false);
        }

        pObj = new()
        {
            usItem = usItem,
            ubNumberOfObjects = 1,
            ubShotsLeft[0] = Magazine[Globals.Item[usItem].ubClassIndex].ubMagSize,
            ubWeight = CalculateObjectWeight(pObj)
        };

        return (true);
    }

    public static int CalculateObjectWeight(OBJECTTYPE? pObject)
    {
        int cnt;
        int usWeight;
        INVTYPE? pItem;

        pItem = (Globals.Item[pObject.usItem]);

        // Start with base weight
        usWeight = pItem.ubWeight;

        if (pItem.ubPerPocket < 2)
        {
            // account for any attachments
            for (cnt = 0; cnt < Globals.MAX_ATTACHMENTS; cnt++)
            {
                if (pObject.usAttachItem[cnt] != Globals.NOTHING)
                {
                    usWeight += Globals.Item[pObject.usAttachItem[cnt]].ubWeight;
                }
            }

            // add in weight of ammo
            if (Globals.Item[pObject.usItem].usItemClass == IC.GUN && pObject.ubGunShotsLeft > 0)
            {
                usWeight += Globals.Item[pObject.usGunAmmoItem].ubWeight;
            }
        }

        // make sure it really fits into that UINT8, in case we ever add anything real heavy with attachments/ammo
        Debug.Assert(usWeight <= 255);

        return (usWeight);
    }

    public static Items FindAttachmentByClass(OBJECTTYPE? pObj, IC uiItemClass)
    {
        for (int bLoop = 0; bLoop < Globals.MAX_ATTACHMENTS; bLoop++)
        {
            if (Globals.Item[pObj.usAttachItem[bLoop]].usItemClass == uiItemClass)
            {
                return (pObj.usAttachItem[bLoop]);
            }
        }

        return Globals.ITEM_NOT_FOUND;
    }

    public static Items StandardGunListReplacement(Items usGun)
    {
        Items ubLoop;

        if (ExtendedGunListGun(usGun))
        {
            ubLoop = 0;
            while (ReplacementGuns[ubLoop] != 0)
            {
                if (ReplacementGuns[ubLoop] == usGun)
                {
                    return ReplacementGuns[ubLoop];
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

    public static Items StandardGunListAmmoReplacement(Items usAmmo)
    {
        Items ubLoop;

        if (ExtendedGunListGun(usAmmo))
        {
            ubLoop = 0;
            while (ReplacementAmmo[ubLoop] != 0)
            {
                if (ReplacementAmmo[ubLoop] == usAmmo)
                {
                    return ReplacementAmmo[ubLoop];
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
    public static Items FindReplacementMagazineIfNecessary(Items usOldGun, Items usOldAmmo, Items usNewGun)
    {
        Items usNewAmmo = Items.NONE;
        int oldGunIdx = (int)usOldGun;

        if ((WeaponTypes.Magazines[Globals.Item[usOldAmmo].ubClassIndex].ubCalibre == WeaponTypes.Weapon[oldGunIdx].ubCalibre)
            && (WeaponTypes.Magazines[Globals.Item[usOldAmmo].ubClassIndex].ubMagSize == WeaponTypes.Weapon[oldGunIdx].ubMagSize))
        {
            // must replace this!
            usNewAmmo = FindReplacementMagazine(WeaponTypes.Weapon[(int)usNewGun].ubCalibre, WeaponTypes.Weapon[(int)usNewGun].ubMagSize, WeaponTypes.Magazines[Globals.Item[usOldAmmo].ubClassIndex].ubAmmoType);
        }

        return usNewAmmo;
    }

    public static Items FindReplacementMagazine(CaliberType ubCalibre, int ubMagSize, AMMO ubAmmoType)
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
                    return MagazineClassIndexToItemType(ubLoop);
                }
                else if (usDefault == Items.NONE)
                {
                    // store this one to use if all else fails
                    usDefault = MagazineClassIndexToItemType(ubLoop);
                }
            }

            ubLoop++;
        }

        return usDefault;
    }

    public static Items MagazineClassIndexToItemType(int usMagIndex)
    {
        Items usLoop;

        // Note: if any ammo items in the item table are separated from the main group,
        // this function will have to be rewritten to scan the item table for an item
        // with item class ammo, which has class index usMagIndex
        for (usLoop = (Items)ItemIndexes.FIRST_AMMO; usLoop < Items.MAXITEMS; usLoop++)
        {
            if (Globals.Item[usLoop].ubClassIndex == usMagIndex)
            {
                return usLoop;
            }
        }

        return Items.NONE;
    }
}
