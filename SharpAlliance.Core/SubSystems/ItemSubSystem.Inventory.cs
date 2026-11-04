using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpAlliance.Core.Managers;
using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core.SubSystems;

// Ported from Items.c (second half; the first half lives in ItemSubSystem.cs).
public partial class ItemSubSystem
{
    // constants local to Items.c
    private const int RELOAD_NONE = 0;
    private const int RELOAD_PLACE = 1;
    private const int RELOAD_SWAP = 2;
    private const int RELOAD_TOPOFF = 3;
    private const int RELOAD_AUTOPLACE_OLD = 4;
    private const int ANY_MAGSIZE = 255;
    private const int MAX_AMMO_TYPES_PER_GUN = 3;
    private const int XRAY_RANGE = 40;
    private const int INVALID_KEY_NUMBER = 255;

    // ---- dependencies that are not ported yet --------------------------------------------------
    // Sound is not hooked up: the original calls are kept as comments, matching the rest of the port.
    private static bool EnoughPointsStub(SOLDIERTYPE pSoldier, int sAPCost, int sBPCost, bool fFromUI) => throw new NotImplementedException("EnoughPoints (Points.c)");
    private static int GetAPsToReloadGunWithAmmo(OBJECTTYPE pGun, OBJECTTYPE pAmmo) => throw new NotImplementedException("GetAPsToReloadGunWithAmmo (Points.c)");
    private static bool IsValidSecondHandShotForReloadingPurposes(SOLDIERTYPE pSoldier) => throw new NotImplementedException("IsValidSecondHandShotForReloadingPurposes (Points.c)");
    private static bool ShouldSoldierDisplayHatchOnItem(NPCID ubProfile, InventorySlot sSlotNumber) => throw new NotImplementedException("ShouldSoldierDisplayHatchOnItem (Interface Items.c)");
    private static bool IsAutoResolveActive() => false;   // Auto Resolve.c is not ported
    private static void DeleteItemDescriptionBox() { }     // Interface Items.c
    private static void UpdateRobotControllerGivenController(SOLDIERTYPE pSoldier) { }   // Robot.c is not ported
    private static int GetTimeOfDayAmbientLightLevel() => throw new NotImplementedException("GetTimeOfDayAmbientLightLevel (Environment.c)");

    public static int ItemSlotLimit(Items usItem, InventorySlot bSlot)
    {
        int ubSlotLimit;

        if ( bSlot < InventorySlot.BIGPOCK1POS )
        {
            return( 1 );
        }
        else
        {
            ubSlotLimit = Item[usItem].ubPerPocket;
            if (bSlot >= InventorySlot.SMALLPOCK1POS && ubSlotLimit > 1)
            {
                ubSlotLimit /= 2;
            }
            return( ubSlotLimit );
        }
    }

    public static int MoneySlotLimit(InventorySlot bSlot)
    {
        if ( bSlot >= InventorySlot.SMALLPOCK1POS )
        {
            return( MAX_MONEY_PER_SLOT / 2 );
        }
        else
        {
            return( MAX_MONEY_PER_SLOT );
        }
    }

    public static InventorySlot FindThrowableGrenade(SOLDIERTYPE? pSoldier)
    {
        InventorySlot bLoop;
        bool fCheckForFlares = false;

        // JA2Gold: give some priority to looking for flares when at night
        // this is AI only so we can put in some customization for night
        if (GetTimeOfDayAmbientLightLevel() == Environment.NORMAL_LIGHTLEVEL_NIGHT)
        {
            if (pSoldier.bLife > (pSoldier.bLifeMax / 2))
            {
                fCheckForFlares = true;
            }
        }
        if (fCheckForFlares)
        {
            // Do a priority check for flares first
            for (bLoop = 0; bLoop < NUM_INV_SLOTS; bLoop++)
            {
                if (pSoldier.inv[ bLoop ].usItem == Items.BREAK_LIGHT)
                {
                    return( bLoop );
                }
            }
        }

        for (bLoop = 0; bLoop < NUM_INV_SLOTS; bLoop++)
        {
            if ( (Item[ pSoldier.inv[ bLoop ].usItem ].usItemClass.HasFlag(IC.GRENADE)) && !GLGrenadeInSlot( pSoldier, bLoop ) )
            {
                return( bLoop );
            }
        }
        return( NO_SLOT );
    }

    public static bool ItemHasAttachments(OBJECTTYPE? pObj)
    {
        if ((pObj.usAttachItem[0] == Items.NONE) && (pObj.usAttachItem[1] == Items.NONE) && (pObj.usAttachItem[2] == Items.NONE) && (pObj.usAttachItem[3] == Items.NONE))
        {
            return( false );
        }
        return( true );
    }

    public static bool ValidAttachmentClass(Items usAttachment, Items usItem)
    {
        int iLoop = 0;
        while (true)
        {
            // see comment for AttachmentInfo array for why we skip IC_NONE
            if ( AttachmentInfo[ iLoop ].uiItemClass != IC.NONE )
            {
                if ( AttachmentInfo[ iLoop ].usItem == usAttachment )
                {
                    if ( AttachmentInfo[ iLoop ].uiItemClass == Item[ usItem ].usItemClass )
                    {
                        return( true );
                    }
                }
            }
            iLoop++;
            if (AttachmentInfo[iLoop].usItem == Items.NONE)
            {
                // end of the array
                break;
            }
        }
        return( false );
    }

    public static int GetAttachmentInfoIndex(Items usItem)
    {
        int iLoop = 0;

        while (true)
        {
            if ( AttachmentInfo[ iLoop ].usItem == usItem )
            {
                return( (int) iLoop );
            }
            iLoop++;
            if (AttachmentInfo[iLoop].usItem == Items.NONE)
            {
                // end of the array
                break;
            }
        }
        return( -1 );
    }

    public static bool ValidAttachment(Items usAttachment, Items usItem)
    {
        int iLoop = 0;

        // look for the section of the array pertaining to this attachment...
        while (true)
        {
            if (Attachment[iLoop][0] == usAttachment)
            {
                break;
            }
            iLoop++;
            if (Attachment[iLoop][0] == Items.NONE)
            {
                // the proposed item cannot be attached to anything!
                return( false );
            }
        }
        // now look through this section for the item in question
        while (true)
        {
            if (Attachment[iLoop][1] == usItem)
            {
                break;
            }
            iLoop++;
            if (Attachment[iLoop][0] != usAttachment)
            {
                // the proposed item cannot be attached to the item in question
                return( false );
            }
        }
        return( true );
    }

    public static bool ValidItemAttachment(OBJECTTYPE? pObj, Items usAttachment, bool fAttemptingAttachment)
    {
        bool        fSameItem = false, fSimilarItems = false;
        Items usSimilarItem = Items.NONE;

        if ( !ValidAttachment( usAttachment, pObj.usItem ) )
        {
            // check for an underslung grenade launcher attached to the gun
            if ( (FindAttachment( pObj, Items.UNDER_GLAUNCHER ) != -1) && ValidLaunchable( usAttachment, Items.UNDER_GLAUNCHER ) )
            {
                return ( true );
                /*
                if ( fAttemptingAttachment )
                {
                    // if there is no other grenade attached already, then we can attach it
                    if (FindAttachmentByClass( pObj, IC.GRENADE) != -1)
                    {
                        return( false );
                    }
                    // keep going, it can be attached to the grenade launcher
                }
                else
                {
                    // logically, can be added
                    return( true );
                }
                */
            }
            else
            {
                if ( fAttemptingAttachment && ValidAttachmentClass( usAttachment, pObj.usItem ) )
                {
                    // well, maybe the player thought he could
                    string zTemp = "";

                    zTemp = wprintf( Message[(int)STRMessage.CANT_ATTACH], ItemNames[ usAttachment ], ItemNames[ pObj.usItem ] );
                    Messages.ScreenMsg( FontColor.FONT_MCOLOR_LTYELLOW, MSG.UI_FEEDBACK, zTemp );
                }

                return( false );
            }
        }
        // special conditions go here
        // can't have two of the same attachment on an item
        /*
        if (FindAttachment( pObj, usAttachment ) != -1)
        {
            fSameItem = true;
        }
        */

        // special code for items which won't attach if X is present
        switch( usAttachment )
        {
            case Items.BIPOD:
                if ( FindAttachment( pObj, Items.UNDER_GLAUNCHER) != -1 )
                {
                    fSimilarItems = true;
                    usSimilarItem = Items.UNDER_GLAUNCHER;
                }
                break;
            case Items.UNDER_GLAUNCHER:
                if ( FindAttachment( pObj, Items.BIPOD ) != -1 )
                {
                    fSimilarItems = true;
                    usSimilarItem = Items.BIPOD;
                }
                break;
        /*
            case Items.LASERSCOPE:
                if (FindAttachment( pObj, Items.SNIPERSCOPE ) != -1)
                {
                    return( false );
                }
                break;
            case Items.SNIPERSCOPE:
                if (FindAttachment( pObj, Items.LASERSCOPE ) != -1)
                {
                    return( false );
                }
                break;
                */
            case Items.DETONATOR:
                if( FindAttachment( pObj, Items.REMDETONATOR ) != -1 )
                {
                    fSameItem = true;
                }
                break;
            case Items.REMDETONATOR:
                if( FindAttachment( pObj, Items.DETONATOR ) != -1 )
                {
                    fSameItem = true;
                }
                break;
        }

        if (fAttemptingAttachment)
        {
            if (fSameItem)
            {
                Messages.ScreenMsg( FontColor.FONT_MCOLOR_LTYELLOW, MSG.UI_FEEDBACK, Message[(int)STRMessage.ATTACHMENT_ALREADY] );
                return( false );
            }
            else if (fSimilarItems)
            {
                Messages.ScreenMsg( FontColor.FONT_MCOLOR_LTYELLOW, MSG.UI_FEEDBACK, Message[(int)STRMessage.CANT_USE_TWO_ITEMS], ItemNames[ usSimilarItem ], ItemNames[ usAttachment ] );
                return( false );
            }
        }

        return( true );
    }

    public static bool ValidAmmoType(Items usItem, Items usAmmoType)
    {
        if (Item[usItem].usItemClass == IC.GUN && Item[usAmmoType].usItemClass == IC.AMMO)
        {
            if (WeaponTypes.Weapon[usItem].ubCalibre == WeaponTypes.Magazine[Item[usAmmoType].ubClassIndex].ubCalibre)
            {
                return( true );
            }
        }
        return( false );
    }

    public static bool CompatibleFaceItem(Items usItem1, Items usItem2)
    {
        int iLoop = 0;

        // look for the section of the array pertaining to this attachment...
        while (true)
        {
            if (CompatibleFaceItems[iLoop][0] == usItem1)
            {
                break;
            }
            iLoop++;
            if (CompatibleFaceItems[iLoop][0] == Items.NONE)
            {
                // the proposed item cannot fit with anything!
                return( false );
            }
        }
        // now look through this section for the item in question
        while (true)
        {
            if (CompatibleFaceItems[iLoop][1] == usItem2)
            {
                break;
            }
            iLoop++;
            if (CompatibleFaceItems[iLoop][0] != usItem1)
            {
                // the proposed item cannot be attached to the item in question
                return( false );
            }
        }
        return( true );
    }

    public static bool TwoHandedItem(Items usItem)
    {
        if (Item[usItem].fFlags.HasFlag(ItemAttributes.ITEM_TWO_HANDED))
        {
            return( true );
        }
        return false;
    }

    public static bool ValidItemLaunchable(OBJECTTYPE? pObj, Items usAttachment)
    {
        if ( !ValidLaunchable( usAttachment, pObj.usItem ) )
        {
            return( false );
        }
        // if we can find another of the same class as the attachment, it's not possible
        if ( FindAttachmentByClass( pObj, Item[ usAttachment ].usItemClass ) != -1 )
        {
            return( false );
        }
        return( true );
    }

    public static Items GetLauncherFromLaunchable(Items usLaunchable)
    {
        // the C table lists the launchers for each launchable item; the first one wins
        if (Launchable.TryGetValue(usLaunchable, out List<Items>? launchers) && launchers.Count > 0)
        {
            return launchers[0];
        }

        return Items.NONE;
    }

    public static bool EvaluateValidMerge(Items usMerge, Items usItem, out Items pusResult, out MergeType pubType)
    {
        // NB "usMerge" is the object being merged with (e.g. compound 18)
        // "usItem" is the item being merged "onto" (e.g. kevlar vest)
        int iLoop = 0;

        if (usMerge == usItem && Item[ usItem ].usItemClass == IC.AMMO)
        {
            pusResult = usItem;
            pubType = MergeType.COMBINE_POINTS;
            return( true );
        }

        pusResult = Items.NONE;
        pubType = MergeType.DESTRUCTION;

        // look for the section of the array pertaining to this Merge...
        while (true)
        {
            if (Merge[iLoop].usItem1 == usMerge)
            {
                break;
            }
            iLoop++;
            if (Merge[iLoop].usItem1 == Items.NONE)
            {
                // the proposed item cannot be merged with anything!
                return( false );
            }
        }
        // now look through this section for the item in question
        while (true)
        {
            if (Merge[iLoop].usItem2 == usItem)
            {
                break;
            }
            iLoop++;
            if (Merge[iLoop].usItem1 != usMerge)
            {
                // the proposed item cannot be merged with the item in question
                return( false );
            }
        }
        pusResult = Merge[iLoop].usResult;
        pubType = Merge[iLoop].ubType;
        return( true );
    }

    public static bool ValidMerge(Items usMerge, Items usItem)
    {
        return( EvaluateValidMerge( usMerge, usItem, out _, out _ ) );
    }

    public static void CopyObj(OBJECTTYPE? pSourceObj, OBJECTTYPE? pTargetObj)
    {
        pTargetObj.CopyFrom( pSourceObj );
    }

    public static void SwapObjs(OBJECTTYPE? pObj1, OBJECTTYPE? pObj2)
    {
        OBJECTTYPE Temp = pObj1.Clone();

        pObj1.CopyFrom( pObj2 );
        pObj2.CopyFrom( Temp );
        // (the original also swapped the shopkeeper's "moving item" here; that interface is not ported yet)
    }

    public static void RemoveObjs(OBJECTTYPE? pObj, int ubNumberToRemove)
    {
        // remove a certain number of objects from an OBJECTTYPE, starting at index 0
        int ubLoop;

        if (ubNumberToRemove == 0)
        {
            return;
        }
        if (ubNumberToRemove >= pObj.ubNumberOfObjects)
        {
            // delete!
            DeleteObj( pObj );
        }
        else
        {
            for (ubLoop = 0; ubLoop < ubNumberToRemove; ubLoop++)
            {
                RemoveObjFrom( pObj, 0 );
            }
            pObj.ubWeight = CalculateObjectWeight( pObj );
        }
    }

    public static void GetObjFrom(OBJECTTYPE? pObj, int ubGetIndex, OBJECTTYPE? pDest)
    {
        if (pDest == null|| ubGetIndex >= pObj.ubNumberOfObjects)
        {
            return;
        }
        if (pObj.ubNumberOfObjects == 1)
        {
            pDest.CopyFrom( pObj );
            DeleteObj( pObj );
        }
        else
        {
            pDest.usItem = pObj.usItem;
            pDest.bStatus[0] = pObj.bStatus[ubGetIndex];
            pDest.ubNumberOfObjects = 1;
            pDest.ubWeight = CalculateObjectWeight( pDest );
            RemoveObjFrom( pObj, ubGetIndex );
            pObj.ubWeight = CalculateObjectWeight( pObj );
        }
    }

    public static void SwapWithinObj(OBJECTTYPE? pObj, int ubIndex1, int ubIndex2)
    {
        int bTemp;

        if (pObj.ubNumberOfObjects >= ubIndex1 || pObj.ubNumberOfObjects >= ubIndex1)
        {
            return;
        }

        bTemp = pObj.bStatus[ubIndex1];
        pObj.bStatus[ubIndex1] = pObj.bStatus[ubIndex2];
        pObj.bStatus[ubIndex2] = bTemp;
    }

    public static void DamageObj(OBJECTTYPE? pObj, int bAmount)
    {
        if (bAmount >= pObj.bStatus[0])
        {
            pObj.bStatus[0] = 1;
        }
        else
        {
            pObj.bStatus[0] -= bAmount;
        }
    }

    public static void StackObjs(OBJECTTYPE? pSourceObj, OBJECTTYPE? pTargetObj, int ubNumberToCopy)
    {
        int     ubLoop;

        // copy over N status values
        for (ubLoop = 0; ubLoop < ubNumberToCopy; ubLoop++)
        {
            pTargetObj.bStatus[ubLoop + pTargetObj.ubNumberOfObjects] = pSourceObj.bStatus[ubLoop ];
        }

        // now in the source object, move the rest down N places
        for (ubLoop = ubNumberToCopy; ubLoop < pSourceObj.ubNumberOfObjects; ubLoop++)
        {
            pSourceObj.bStatus[ubLoop - ubNumberToCopy] = pSourceObj.bStatus[ubLoop];
        }

        pTargetObj.ubNumberOfObjects += ubNumberToCopy;
        RemoveObjs( pSourceObj, ubNumberToCopy );
        pSourceObj.ubWeight = CalculateObjectWeight( pSourceObj );
        pTargetObj.ubWeight = CalculateObjectWeight( pTargetObj );
    }

    public static void CleanUpStack(OBJECTTYPE? pObj, OBJECTTYPE? pCursorObj)
    {
        int bLoop, bLoop2;
        int bMaxPoints, bPointsToMove;

        if ( !(Item[ pObj.usItem ].usItemClass.HasFlag(IC.AMMO) || Item[ pObj.usItem ].usItemClass.HasFlag(IC.KIT) || Item[ pObj.usItem ].usItemClass.HasFlag(IC.MEDKIT) ) )
        {
            return;
        }

        if ( Item[ pObj.usItem ].usItemClass.HasFlag(IC.AMMO) )
        {
            bMaxPoints = WeaponTypes.Magazine[ Item[ pObj.usItem ].ubClassIndex ].ubMagSize;
        }
        else
        {
            bMaxPoints = 100;
        }

        if ( pCursorObj != null && pCursorObj.usItem == pObj.usItem )
        {
            for ( bLoop = (int) pCursorObj.ubNumberOfObjects - 1; bLoop >= 0; bLoop-- )
            {
                if ( pCursorObj.bStatus[ bLoop ] > 0 )
                {
                    // take the points here and distribute over the lower #d items
                    for ( bLoop2 = pObj.ubNumberOfObjects - 1; bLoop2 >= 0; bLoop2-- )
                    {
                        if ( pObj.bStatus[ bLoop2 ] < bMaxPoints )
                        {
                            bPointsToMove = bMaxPoints - pObj.bStatus[ bLoop2 ];
                            bPointsToMove = Math.Min( bPointsToMove, pCursorObj.bStatus[ bLoop ] );

                            pObj.bStatus[ bLoop2 ] += bPointsToMove;

                            pCursorObj.bStatus[ bLoop ] -= bPointsToMove;
                            if ( pCursorObj.bStatus[ bLoop ] == 0 )
                            {
                                // done!
                                pCursorObj.ubNumberOfObjects--;
                                break;
                            }
                        }
                    }
                }
            }
        }

        for ( bLoop = (int) pObj.ubNumberOfObjects - 1; bLoop >= 0; bLoop-- )
        {
            if ( pObj.bStatus[ bLoop ] > 0 )
            {
                // take the points here and distribute over the lower #d items
                for ( bLoop2 = bLoop - 1; bLoop2 >= 0; bLoop2-- )
                {
                    if ( pObj.bStatus[ bLoop2 ] < bMaxPoints )
                    {
                        bPointsToMove = bMaxPoints - pObj.bStatus[ bLoop2 ];
                        bPointsToMove = Math.Min( bPointsToMove, pObj.bStatus[ bLoop ] );

                        pObj.bStatus[ bLoop2 ] += bPointsToMove;

                        pObj.bStatus[ bLoop ] -= bPointsToMove;
                        if ( pObj.bStatus[ bLoop ] == 0 )
                        {
                            // done!
                            pObj.ubNumberOfObjects--;
                            break;
                        }
                    }
                }
            }
        }

    }

    public static bool PlaceObjectAtObjectIndex(OBJECTTYPE? pSourceObj, OBJECTTYPE? pTargetObj, int ubIndex)
    {
        int bTemp;

        if (pSourceObj.usItem != pTargetObj.usItem)
        {
            return( true );
        }
        if (ubIndex < pTargetObj.ubNumberOfObjects)
        {
            // swap
            bTemp = pSourceObj.bStatus[0];
            pSourceObj.bStatus[0] = pTargetObj.bStatus[ubIndex];
            pTargetObj.bStatus[ubIndex] = bTemp;
            return( true );
        }
        else
        {
            // add to end
            StackObjs( pSourceObj, pTargetObj, 1 );
            return( false );
        }
    }

    public static bool ReloadGun(SOLDIERTYPE? pSoldier, OBJECTTYPE? pGun, OBJECTTYPE? pAmmo)
    {
        OBJECTTYPE OldAmmo = new();
        int             ubBulletsToMove;
        int bAPs = 0;
        SoundDefine usReloadSound;
        bool            fSameAmmoType;
        bool            fSameMagazineSize;
        bool            fReloadingWithStack;
        bool            fEmptyGun;
        int             bReloadType;
        Items usNewAmmoItem;

        if ( (gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.TURNBASED)) && (gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.INCOMBAT)) )
        {
            bAPs = GetAPsToReloadGunWithAmmo( pGun, pAmmo );
            if ( !EnoughPointsStub( pSoldier, bAPs, 0,true ) )
            {
                return( false );
            }

        }

        if ( Item[ pGun.usItem ].usItemClass == IC.LAUNCHER || pGun.usItem == Items.TANK_CANNON )
        {
            if ( AttachObject( pSoldier, pGun, pAmmo ) == false )
            {
                // abort
                return( false );
            }
        }
        else
        {
            fEmptyGun = (pGun.ubGunShotsLeft == 0);
            fReloadingWithStack = (pAmmo.ubNumberOfObjects > 1);
            fSameAmmoType = ( pGun.ubGunAmmoType == WeaponTypes.Magazine[Item[pAmmo.usItem].ubClassIndex].ubAmmoType );
            fSameMagazineSize = ( WeaponTypes.Magazine[ Item[ pAmmo.usItem ].ubClassIndex ].ubMagSize == WeaponTypes.Weapon[pGun.usItem].ubMagSize );

            if (fEmptyGun)
            {
                bReloadType = RELOAD_PLACE;
            }
            else
            {
                // record old ammo
                OldAmmo.Clear();
                OldAmmo.usItem = pGun.usGunAmmoItem;
                OldAmmo.ubNumberOfObjects = 1;
                OldAmmo.ubShotsLeft[0] = pGun.ubGunShotsLeft;

                if (fSameMagazineSize)
                {
                    if (fSameAmmoType)
                    {
                        if ( (gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.TURNBASED)) && (gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.INCOMBAT)) )
                        {
                            bReloadType = RELOAD_SWAP;
                        }
                        else
                        {
                            bReloadType = RELOAD_TOPOFF;
                        }
                    }
                    else
                    {
                        if (!fReloadingWithStack)
                        {
                            bReloadType = RELOAD_SWAP;
                        }
                        else
                        {
                            bReloadType = RELOAD_AUTOPLACE_OLD;
                        }
                    }
                }
                else  // diff sized magazines
                {
                    if (fSameAmmoType)
                    {
                        bReloadType = RELOAD_TOPOFF;
                    }
                    else
                    {
                        bReloadType = RELOAD_AUTOPLACE_OLD;
                    }
                }
            }

            if (fSameMagazineSize)
            {
                // record new ammo item for gun
                usNewAmmoItem = pAmmo.usItem;

                if (bReloadType == RELOAD_TOPOFF)
                {
                    ubBulletsToMove = Math.Min( pAmmo.ubShotsLeft[0], WeaponTypes.Weapon[pGun.usItem].ubMagSize - pGun.ubGunShotsLeft );
                }
                else
                {
                    ubBulletsToMove = pAmmo.ubShotsLeft[0];
                }

            }
            else if (WeaponTypes.Magazine[Item[pAmmo.usItem].ubClassIndex].ubMagSize > WeaponTypes.Weapon[pGun.usItem].ubMagSize)
            {
                usNewAmmoItem = pAmmo.usItem - 1;
                if (bReloadType == RELOAD_TOPOFF)
                {
                    ubBulletsToMove = Math.Min( pAmmo.ubShotsLeft[0], WeaponTypes.Weapon[pGun.usItem].ubMagSize - pGun.ubGunShotsLeft );
                }
                else
                {
                    ubBulletsToMove = Math.Min( pAmmo.ubShotsLeft[0], WeaponTypes.Weapon[pGun.usItem].ubMagSize );
                }
            }
            else // mag is smaller than weapon mag
            {
                usNewAmmoItem = pAmmo.usItem + 1;
                if (bReloadType == RELOAD_TOPOFF)
                {
                    ubBulletsToMove = Math.Min( pAmmo.ubShotsLeft[0], WeaponTypes.Weapon[pGun.usItem].ubMagSize - pGun.ubGunShotsLeft );
                }
                else
                {
                    ubBulletsToMove = Math.Min( pAmmo.ubShotsLeft[0], WeaponTypes.Weapon[pGun.usItem].ubMagSize );
                }
            }


            switch( bReloadType )
            {

                case RELOAD_PLACE:
                    pGun.ubGunShotsLeft = ubBulletsToMove;
                    pGun.ubGunAmmoType = WeaponTypes.Magazine[Item[pAmmo.usItem].ubClassIndex].ubAmmoType;
                    pGun.usGunAmmoItem = usNewAmmoItem;
                    break;

                case RELOAD_SWAP:
                    pGun.ubGunShotsLeft = ubBulletsToMove;
                    pGun.ubGunAmmoType = WeaponTypes.Magazine[Item[pAmmo.usItem].ubClassIndex].ubAmmoType;
                    pGun.usGunAmmoItem = usNewAmmoItem;
                    if (fReloadingWithStack)
                    {
                        // add to end of stack
                        StackObjs( OldAmmo, pAmmo, 1 );
                    }
                    else
                    {
                        // Copying the old ammo to the cursor in turnbased could screw up for the player
                        // (suppose his inventory is full!)

                        if ( (gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.TURNBASED)) && (gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.INCOMBAT)) && !EnoughPointsStub( pSoldier, (int) (bAPs + AP.PICKUP_ITEM), 0, false ) )
                        {
                            // try autoplace
                            if ( !AutoPlaceObject( pSoldier, OldAmmo, false ) )
                            {
                                // put it on the ground
                                HandleItems.AddItemToPool( pSoldier.sGridNo, OldAmmo, ItemVisibility.VISIBLE, pSoldier.bLevel, 0, -1 );
                            }
                            // delete the object now in the cursor
                            DeleteObj( pAmmo );
                        }
                        else
                        {
                            // copy the old ammo to the cursor
                            pAmmo.CopyFrom( OldAmmo );
                        }
                    }
                    break;
                case RELOAD_AUTOPLACE_OLD:
                    if ( !AutoPlaceObject( pSoldier, OldAmmo, true ) )
                    {
                        // error msg!
                        return( false );
                    }
                    // place first ammo in gun
                    pGun.ubGunShotsLeft = ubBulletsToMove;
                    pGun.ubGunAmmoType = WeaponTypes.Magazine[Item[pAmmo.usItem].ubClassIndex].ubAmmoType;
                    pGun.usGunAmmoItem = usNewAmmoItem;

                    break;

                case RELOAD_TOPOFF:
                    // ADD that many bullets to gun
                    pGun.ubGunShotsLeft += ubBulletsToMove;
                    break;

            }

            if ( ! ( bReloadType == RELOAD_SWAP && !fReloadingWithStack ) )
            {
                // remove # of bullets, delete 1 object if necessary

                pAmmo.ubShotsLeft[0] -= ubBulletsToMove;
                if (pAmmo.ubShotsLeft[0] == 0)
                {
                    RemoveObjs( pAmmo, 1 );
                }

            }
        }

        // OK, let's play a sound of reloading too...
        // If this guy is visible...
        if ( pSoldier.bVisible != -1 )
        {
            // Play some effects!
            usReloadSound   = WeaponTypes.Weapon[ pGun.usItem ].sReloadSound;

            if ( usReloadSound != 0 && !IsAutoResolveActive() )
            {
                // PlayJA2Sample( usReloadSound, RATE_11025, HIGHVOLUME, 1, SoundPans.MIDDLEPAN );
            }
        }

        if (pSoldier.bTeam == gbPlayerNum)
        {
            // spit out a message if this is one of our folks reloading
            Messages.ScreenMsg( FontColor.FONT_MCOLOR_LTYELLOW, MSG.INTERFACE, Message[(int)STRMessage.PLAYER_RELOADS], pSoldier.name );
        }

        Points.DeductPoints( pSoldier, bAPs, 0 );
        pGun.ubWeight = CalculateObjectWeight( pGun );

        if ( pGun.bGunAmmoStatus >= 0 )
        {
            // make sure gun ammo status is 100, if gun isn't jammed
            pGun.bGunAmmoStatus = 100;
        }

        return( true );
    }

    public static bool EmptyWeaponMagazine(OBJECTTYPE? pWeapon, OBJECTTYPE? pAmmo)
    {
        SoundDefine usReloadSound;

        CHECKF( pAmmo != null );

        if ( pWeapon.ubGunShotsLeft > 0 )
        {
            // start by erasing ammo item, just in case...
            DeleteObj( pAmmo );

            pAmmo.ubShotsLeft[0]            = pWeapon.ubGunShotsLeft;
            pAmmo.usItem                            = pWeapon.usGunAmmoItem;
            pAmmo.ubNumberOfObjects = 1;

            pWeapon.ubGunShotsLeft      = 0;
            pWeapon.ubGunAmmoType     = 0;
            //pWeapon->usGunAmmoItem        = 0; // leaving the ammo item the same for auto-reloading purposes

            // Play some effects!
            usReloadSound   = WeaponTypes.Weapon[ pWeapon.usItem ].sReloadSound;

            if ( usReloadSound != 0 )
            {
                // PlayJA2Sample( usReloadSound, RATE_11025, HIGHVOLUME, 1, SoundPans.MIDDLEPAN );
            }

            pWeapon.ubWeight = CalculateObjectWeight( pWeapon );

            return( true );
        }
        else
        {
            return( false );
        }
    }

    public static bool ReloadLauncher(OBJECTTYPE? pLauncher, OBJECTTYPE? pAmmo)
    {
        bool            fOldAmmo;
        OBJECTTYPE OldAmmo = new();

        if (pLauncher.ubGunShotsLeft == 0)
        {
            fOldAmmo = false;
        }
        else
        {
            if (pAmmo.ubNumberOfObjects > 1)
            {
                // can't do the swap out to the cursor
                return( false );
            }
            // otherwise temporarily store the launcher's old ammo
            OldAmmo.Clear();
            fOldAmmo = true;
            OldAmmo.usItem = pLauncher.usGunAmmoItem;
            OldAmmo.ubNumberOfObjects = 1;
            OldAmmo.bStatus[0] = pLauncher.bGunAmmoStatus;
        }

        // put the new ammo in the gun
        pLauncher.usGunAmmoItem = pAmmo.usItem;
        pLauncher.ubGunShotsLeft = 1;
        pLauncher.ubGunAmmoType = AMMO.GRENADE;
        pLauncher.bGunAmmoStatus = (sbyte)pAmmo.bStatus[0];


        if (fOldAmmo)
        {
            // copy the old ammo back to the item in the cursor
            pAmmo.CopyFrom( OldAmmo );
        }
        else
        {
            // reduce the number of objects in the cursor by 1
            RemoveObjs( pAmmo, 1 );
        }
        return( true );
    }

    public static InventorySlot FindAmmo(SOLDIERTYPE? pSoldier, CaliberType ubCalibre, int ubMagSize, InventorySlot bExcludeSlot)
    {
        InventorySlot bLoop;
        INVTYPE? pItem;

        for (bLoop = InventorySlot.HANDPOS; bLoop < NUM_INV_SLOTS; bLoop++)
        {
            if (bLoop == bExcludeSlot)
            {
                continue;
            }
            pItem = Item[pSoldier.inv[bLoop].usItem];
            if (pItem.usItemClass == IC.AMMO)
            {
                if (WeaponTypes.Magazine[pItem.ubClassIndex].ubCalibre == ubCalibre && (WeaponTypes.Magazine[pItem.ubClassIndex].ubMagSize == ubMagSize || ubMagSize == ANY_MAGSIZE))
                {
                    return( bLoop );
                }
            }
        }
        return( NO_SLOT );
    }

    public static InventorySlot FindAmmoToReload(SOLDIERTYPE? pSoldier, InventorySlot bWeaponIn, InventorySlot bExcludeSlot)
    {
        OBJECTTYPE? pObj;
        InventorySlot bSlot;

        if (pSoldier == null)
        {
            return( NO_SLOT );
        }
        pObj = pSoldier.inv[bWeaponIn];
        if ( Item[pObj.usItem].usItemClass == IC.GUN && pObj.usItem != Items.TANK_CANNON )
        {
            // look for same ammo as before
            bSlot = FindObjExcludingSlot( pSoldier, pObj.usGunAmmoItem, bExcludeSlot );
            if (bSlot != NO_SLOT)
            {
                // reload using this ammo!
                return( bSlot );
            }
            // look for any ammo that matches which is of the same calibre and magazine size
            bSlot = FindAmmo( pSoldier, WeaponTypes.Weapon[pObj.usItem].ubCalibre, WeaponTypes.Weapon[pObj.usItem].ubMagSize, bExcludeSlot );
            if (bSlot != NO_SLOT)
            {
                return( bSlot );
            }
            else
            {
                // look for any ammo that matches which is of the same calibre (different size okay)
                return( FindAmmo( pSoldier, WeaponTypes.Weapon[pObj.usItem].ubCalibre, ANY_MAGSIZE, bExcludeSlot ) );
            }
        }
        else
        {
            switch( pObj.usItem )
            {
                case Items.MORTAR:
                    return( FindObj( pSoldier, Items.MORTAR_SHELL ) );
                case Items.TANK_CANNON:
                    return( FindObj( pSoldier, Items.TANK_SHELL ) );
                case Items.GLAUNCHER:
                case Items.UNDER_GLAUNCHER:
                    return( FindObjInObjRange( pSoldier, Items.GL_HE_GRENADE, Items.GL_SMOKE_GRENADE ) );
                default:
                    return( NO_SLOT );
            }
        }
    }

    public static bool AutoReload(SOLDIERTYPE? pSoldier)
    {
        OBJECTTYPE? pObj;
        InventorySlot bSlot; int bAPCost;
        bool                fRet;

        CHECKF( pSoldier );
        pObj = pSoldier.inv[InventorySlot.HANDPOS];

        if (Item[pObj.usItem].usItemClass == IC.GUN || Item[pObj.usItem].usItemClass == IC.LAUNCHER)
        {
            bSlot = FindAmmoToReload( pSoldier, InventorySlot.HANDPOS, NO_SLOT );
            if (bSlot != NO_SLOT)
            {
                // reload using this ammo!
                fRet = ReloadGun( pSoldier, pObj, (pSoldier.inv[bSlot]) );
                // if we are valid for two-pistol shooting (reloading) and we have enough APs still
                // then do a reload of both guns!
                if ( (fRet == true) && IsValidSecondHandShotForReloadingPurposes( pSoldier ) )
                {
                    pObj = pSoldier.inv[InventorySlot.SECONDHANDPOS];
                    bSlot = FindAmmoToReload( pSoldier, InventorySlot.SECONDHANDPOS, NO_SLOT );
                    if (bSlot != NO_SLOT)
                    {
                        // ce would reload using this ammo!
                        bAPCost = GetAPsToReloadGunWithAmmo( pObj, pSoldier.inv[bSlot] );
                        if ( EnoughPointsStub( pSoldier, (int) bAPCost, 0, false ) )
                        {
                            // reload the 2nd gun too
                            fRet = ReloadGun( pSoldier, pObj, (pSoldier.inv[bSlot]) );
                        }
                        else
                        {
                            Messages.ScreenMsg( FontColor.FONT_MCOLOR_LTYELLOW, MSG.INTERFACE, Message[(int)STRMessage.RELOAD_ONLY_ONE_GUN], pSoldier.name );
                        }
                    }
                }

                Interface.DirtyMercPanelInterface( pSoldier, DIRTYLEVEL2 );
                return( fRet );
            }
        }

        // couldn't reload
        return( false );
    }

    public static int GetAttachmentComboMerge(OBJECTTYPE? pObj)
    {
        int     bIndex = 0;
        int bAttachLoop; int bAttachPos;

        while( AttachmentComboMerge[ bIndex ].usItem != Items.NONE )
        {
            if ( pObj.usItem == AttachmentComboMerge[ bIndex ].usItem )
            {
                // search for all the appropriate attachments
                for ( bAttachLoop = 0; bAttachLoop < 2; bAttachLoop++ )
                {
                    if ( AttachmentComboMerge[ bIndex ].usAttachment[ bAttachLoop ] == Items.NONE )
                    {
                        continue;
                    }

                    bAttachPos = FindAttachment( pObj, AttachmentComboMerge[ bIndex ].usAttachment[ bAttachLoop ] );
                    if ( bAttachPos == -1 )
                    {
                        // didn't find something required
                        return( -1 );
                    }
                }
                // found everything required!
                return( bIndex );
            }

            bIndex++;
        }

        return( -1 );
    }

    public static void PerformAttachmentComboMerge(OBJECTTYPE? pObj, int bAttachmentComboMerge)
    {
        int bAttachLoop; int bAttachPos;
        int uiStatusTotal = 0;
        int     bNumStatusContributors = 0;

        // This object has been validated as available for an attachment combo merge.
        // - find all attachments in list and destroy them
        // - status of new object should be average of items including attachments
        // - change object

        for ( bAttachLoop = 0; bAttachLoop < 2; bAttachLoop++ )
        {
            if ( AttachmentComboMerge[ bAttachmentComboMerge ].usAttachment[ bAttachLoop ] == Items.NONE )
            {
                continue;
            }

            bAttachPos = FindAttachment( pObj, AttachmentComboMerge[ bAttachmentComboMerge ].usAttachment[ bAttachLoop ] );
            Debug.Assert( bAttachPos != -1, "Attachment combo merge couldn't find a necessary attachment" );

            uiStatusTotal += pObj.bAttachStatus[ bAttachPos ];
            bNumStatusContributors++;

            pObj.usAttachItem[ bAttachPos ] = Items.NONE;
            pObj.bAttachStatus[ bAttachPos ] = 0;
        }

        uiStatusTotal += pObj.bStatus[ 0 ];
        bNumStatusContributors++;

        pObj.usItem = AttachmentComboMerge[ bAttachmentComboMerge ].usResult;
        pObj.bStatus[ 0 ] = (int) (uiStatusTotal / bNumStatusContributors );
    }

    public static bool AttachObject(SOLDIERTYPE? pSoldier, OBJECTTYPE? pTargetObj, OBJECTTYPE? pAttachment)
    {
        int bAttachPos; int bSecondAttachPos;//, bAbility, bSuccess;
        Items usResult;
        int     bLoop;
        MergeType ubType; int ubLimit;
        int     iCheckResult;
        int     bAttachInfoIndex = -1, bAttachComboMerge;
        bool    fValidLaunchable = false;

        fValidLaunchable = ValidLaunchable( pAttachment.usItem, pTargetObj.usItem );

        if ( fValidLaunchable || ValidItemAttachment( pTargetObj, pAttachment.usItem, true ) )
        {
            OBJECTTYPE  TempObj = new();

            // find an attachment position...
            // second half of this 'if' is for attaching GL grenades to a gun
            if ( fValidLaunchable || pAttachment.usItem >= Items.GL_HE_GRENADE && pAttachment.usItem <= Items.GL_SMOKE_GRENADE )
            {
                // try replacing if possible
                bAttachPos = FindAttachmentByClass( pTargetObj, Item[ pAttachment.usItem ].usItemClass );
                if ( bAttachPos != -1 )
                {
                    // we can only do a swap if there is only 1 grenade being attached
                    if ( pAttachment.ubNumberOfObjects > 1 )
                    {
                        return( false );
                    }
                }
                else
                {
                    bAttachPos = FindAttachment( pTargetObj, Items.NONE );
                }
            }
            else
            {
                // try replacing if possible
                bAttachPos = FindAttachment( pTargetObj, pAttachment.usItem );
                if ( bAttachPos == -1 )
                {
                    bAttachPos = FindAttachment( pTargetObj, Items.NONE );
                }
            }

            if (bAttachPos == -1)
            {
                return( false );
            }
            else
            {
                if ( pSoldier != null )
                {
                    bAttachInfoIndex = GetAttachmentInfoIndex( pAttachment.usItem );
                    // in-game (not behind the scenes) attachment
                    if ( bAttachInfoIndex != -1 && AttachmentInfo[ bAttachInfoIndex ].bAttachmentSkillCheck != SKILLCHECKS.NO_CHECK )
                    {
                        iCheckResult = SkillChecks.SkillCheck( pSoldier, AttachmentInfo[ bAttachInfoIndex ].bAttachmentSkillCheck, AttachmentInfo[ bAttachInfoIndex ].bAttachmentSkillCheckMod );
                        if (iCheckResult < 0)
                        {
                            // the attach failure damages both items
                            DamageObj( pTargetObj, (int) -iCheckResult );
                            DamageObj( pAttachment, (int) -iCheckResult );
                            // there should be a quote here!
                            SoldierControl.DoMercBattleSound( pSoldier, BATTLE_SOUND.CURSE1 );
                            if ( gfInItemDescBox )
                            {
                                DeleteItemDescriptionBox();
                            }
                            return( false );
                        }
                    }

                    if ( ValidItemAttachment( pTargetObj, pAttachment.usItem, true ) ) // not launchable
                    {
                        // attachment sounds
                        if ( (Item[ pTargetObj.usItem ].usItemClass & IC.WEAPON) != 0 )
                        {
                            // PlayJA2Sample( SoundDefine.ATTACH_TO_GUN, RATE_11025, SoundVolume( MIDVOLUME, pSoldier.sGridNo ), 1, SoundDir( pSoldier.sGridNo ) );
                        }
                        else if ( Item[ pTargetObj.usItem ].usItemClass.HasFlag(IC.ARMOUR) )
                        {
                            // PlayJA2Sample( SoundDefine.ATTACH_CERAMIC_PLATES, RATE_11025, SoundVolume( MIDVOLUME, pSoldier.sGridNo ), 1, SoundDir( pSoldier.sGridNo ) );
                        }
                        else if ( Item[ pTargetObj.usItem ].usItemClass.HasFlag(IC.BOMB) )
                        {
                            // PlayJA2Sample( SoundDefine.ATTACH_DETONATOR, RATE_11025, SoundVolume( MIDVOLUME, pSoldier.sGridNo ), 1, SoundDir( pSoldier.sGridNo ) );
                        }
                    }
                }

                if ( pTargetObj.usAttachItem[ bAttachPos ] != Items.NONE )
                {
                    CreateItem( pTargetObj.usAttachItem[bAttachPos], pTargetObj.bAttachStatus[bAttachPos], TempObj );
                }

                pTargetObj.usAttachItem[bAttachPos] = pAttachment.usItem;
                pTargetObj.bAttachStatus[bAttachPos] = pAttachment.bStatus[0];

                if (pAttachment.usItem == Items.UNDER_GLAUNCHER)
                {
                    // transfer any attachment (max 1) from the grenade launcher to the gun
                    if (pAttachment.usAttachItem[0] != Items.NONE)
                    {
                        bSecondAttachPos = FindAttachment( pTargetObj, Items.NONE );
                        if (bSecondAttachPos == -1)
                        {
                            // not enough room for all attachments - cancel!!
                            pTargetObj.usAttachItem[bAttachPos] = Items.NONE;
                            pTargetObj.bAttachStatus[bAttachPos] = 0;
                            return( false );
                        }
                        else
                        {
                            pTargetObj.usAttachItem[bSecondAttachPos] = pAttachment.usAttachItem[0];
                            pTargetObj.bAttachStatus[bSecondAttachPos] = pAttachment.bAttachStatus[0];
                            pAttachment.usAttachItem[0] = Items.NONE;
                            pAttachment.bAttachStatus[0] = 0;
                        }
                    }
                }

                if ( TempObj.usItem != Items.NONE )
                {
                    // overwrite/swap!
                    CopyObj( TempObj, pAttachment );
                }
                else
                {
                    RemoveObjs( pAttachment, 1 );
                }

                // Check for attachment merge combos here
                bAttachComboMerge = GetAttachmentComboMerge( pTargetObj );
                if ( bAttachComboMerge != -1 )
                {
                    PerformAttachmentComboMerge( pTargetObj, bAttachComboMerge );
                    if ( bAttachInfoIndex != -1 && AttachmentInfo[ bAttachInfoIndex ].bAttachmentSkillCheckMod < 20 )
                    {
                        Campaign.StatChange( pSoldier, Stat.MECHANAMT, (int) ( 20 - AttachmentInfo[ bAttachInfoIndex ].bAttachmentSkillCheckMod ), 0 );
                    }
                }

                pTargetObj.ubWeight = CalculateObjectWeight( pTargetObj );

                return( true );
            }
        }
        // check for merges
        else if (EvaluateValidMerge( pAttachment.usItem, pTargetObj.usItem, out usResult, out ubType ))
        {
            if ( ubType != MergeType.COMBINE_POINTS )
            {
                if ( !EnoughPointsStub( pSoldier, AP.MERGE, 0, true ) )
                {
                    return( false );
                }

                Points.DeductPoints( pSoldier, AP.MERGE, 0 );
            }

            switch( ubType )
            {
                case MergeType.COMBINE_POINTS:
                    // transfer points...
                    if ( Item[ pTargetObj.usItem ].usItemClass == IC.AMMO )
                    {
                        ubLimit = WeaponTypes.Magazine[ Item[ pTargetObj.usItem ].ubClassIndex ].ubMagSize;
                    }
                    else
                    {
                        ubLimit = 100;
                    }

                    // count down through # of attaching items and add to status of item in position 0
                    for (bLoop = pAttachment.ubNumberOfObjects - 1; bLoop >= 0; bLoop--)
                    {
                        if (pTargetObj.bStatus[0] + pAttachment.bStatus[bLoop] <= ubLimit)
                        {
                            // consume this one totally and continue
                            pTargetObj.bStatus[0] += pAttachment.bStatus[bLoop];
                            RemoveObjFrom( pAttachment, bLoop );
                            // reset loop limit
                            bLoop = pAttachment.ubNumberOfObjects; // add 1 to counteract the -1 from the loop
                        }
                        else
                        {
                            // add part of this one and then we're done
                            pAttachment.bStatus[bLoop] -= (ubLimit - pTargetObj.bStatus[0]);
                            pTargetObj.bStatus[0] = ubLimit;
                            break;
                        }
                    }
                    break;
                case MergeType.DESTRUCTION:
                    // the merge destroyed both items!
                    DeleteObj( pTargetObj );
                    DeleteObj( pAttachment );
                    SoldierControl.DoMercBattleSound( pSoldier, BATTLE_SOUND.CURSE1 );
                    break;
                case MergeType.ELECTRONIC_MERGE:
                    if ( pSoldier != null )
                    {
                        iCheckResult = SkillChecks.SkillCheck( pSoldier, SKILLCHECKS.ATTACHING_SPECIAL_ELECTRONIC_ITEM_CHECK, -30 );
                        if ( iCheckResult < 0 )
                        {
                            DamageObj( pTargetObj, (int) -iCheckResult );
                            DamageObj( pAttachment, (int) -iCheckResult );
                            SoldierControl.DoMercBattleSound( pSoldier, BATTLE_SOUND.CURSE1 );
                            return( false );
                        }
                        // grant experience!
                    }
                    // fall through
                    goto case MergeType.EXPLOSIVE;
                case MergeType.EXPLOSIVE:
                    if ( ubType == MergeType.EXPLOSIVE ) /// coulda fallen through
                    {
                        if ( pSoldier != null )
                        {
                            // requires a skill check, and gives experience
                            iCheckResult = SkillChecks.SkillCheck( pSoldier, SKILLCHECKS.ATTACHING_DETONATOR_CHECK, -30 );
                            if (iCheckResult < 0)
                            {
                                // could have a chance of detonation
                                // for now, damage both objects
                                DamageObj( pTargetObj, (int) -iCheckResult );
                                DamageObj( pAttachment, (int) -iCheckResult );
                                SoldierControl.DoMercBattleSound( pSoldier, BATTLE_SOUND.CURSE1 );
                                return( false );
                            }
                            Campaign.StatChange( pSoldier, Stat.EXPLODEAMT, 25, 0 );
                        }
                    }
                    // fall through
                    goto default;
                default:
                    // the merge will combine the two items
                    pTargetObj.usItem = usResult;
                    if ( ubType != MergeType.TREAT_ARMOUR )
                    {
                        pTargetObj.bStatus[0] = (pTargetObj.bStatus[0] + pAttachment.bStatus[0]) / 2;
                    }
                    DeleteObj( pAttachment );
                    pTargetObj.ubWeight = CalculateObjectWeight( pTargetObj );
                    if (pSoldier != null && pSoldier.bTeam == gbPlayerNum)
                    {
                        SoldierControl.DoMercBattleSound( pSoldier, BATTLE_SOUND.COOL1 );
                    }
                    break;
                }
                return( true );
        }
        return( false );
    }

    public static bool CanItemFitInPosition(SOLDIERTYPE? pSoldier, OBJECTTYPE? pObj, InventorySlot bPos, bool fDoingPlacement)
    {
        int                 ubSlotLimit;
        InventorySlot bNewPos;

        switch( bPos )
        {
            case InventorySlot.SECONDHANDPOS:
                if (Item[pSoldier.inv[InventorySlot.HANDPOS].usItem].fFlags.HasFlag(ItemAttributes.ITEM_TWO_HANDED))
                {
                    return( false );
                }
                break;
            case InventorySlot.HANDPOS:
                if (Item[ pObj.usItem ].fFlags.HasFlag(ItemAttributes.ITEM_TWO_HANDED))
                {
                    if (pSoldier.inv[InventorySlot.HANDPOS].usItem != Items.NONE && pSoldier.inv[InventorySlot.SECONDHANDPOS].usItem != Items.NONE)
                    {
                        // two items in hands; try moving the second one so we can swap
                        if (Item[pSoldier.inv[InventorySlot.SECONDHANDPOS].usItem].ubPerPocket == 0)
                        {
                            bNewPos = FindEmptySlotWithin( pSoldier, InventorySlot.BIGPOCK1POS, InventorySlot.BIGPOCK4POS );
                        }
                        else
                        {
                            bNewPos = FindEmptySlotWithin( pSoldier, InventorySlot.BIGPOCK1POS, InventorySlot.SMALLPOCK8POS );
                        }
                        if (bNewPos == NO_SLOT)
                        {
                            // nowhere to put second item
                            return( false );
                        }

                        if ( fDoingPlacement )
                        {
                            // otherwise move it.
                            CopyObj( (pSoldier.inv[InventorySlot.SECONDHANDPOS]), (pSoldier.inv[bNewPos]) );
                            DeleteObj( (pSoldier.inv[InventorySlot.SECONDHANDPOS]) );
                        }
                    }
                }
                break;
            case InventorySlot.VESTPOS:
            case InventorySlot.HELMETPOS:
            case InventorySlot.LEGPOS:
                if (Item[pObj.usItem].usItemClass != IC.ARMOUR)
                {
                    return( false );
                }
                switch (bPos)
                {
                    case InventorySlot.VESTPOS:
                        if (WeaponTypes.Armour[Item[pObj.usItem].ubClassIndex].ubArmourClass != ARMOURCLASS.VEST)
                        {
                            return( false );
                        }
                        break;
                    case InventorySlot.HELMETPOS:
                        if (WeaponTypes.Armour[Item[pObj.usItem].ubClassIndex].ubArmourClass != ARMOURCLASS.HELMET)
                        {
                            return( false );
                        }
                        break;
                    case InventorySlot.LEGPOS:
                        if (WeaponTypes.Armour[Item[pObj.usItem].ubClassIndex].ubArmourClass != ARMOURCLASS.LEGGINGS)
                        {
                            return( false );
                        }
                        break;
                    default:
                        break;
                }
                break;
            case InventorySlot.HEAD1POS:
            case InventorySlot.HEAD2POS:
                if (Item[pObj.usItem].usItemClass != IC.FACE)
                {
                    return( false );
                }
                break;
            default:
                break;
        }

        ubSlotLimit = ItemSlotLimit( pObj.usItem, bPos );
        if (ubSlotLimit == 0 && bPos >= InventorySlot.SMALLPOCK1POS )
        {
            // doesn't fit!
            return( false );
        }

        return( true );
    }

    public static bool DropObjIfThereIsRoom(SOLDIERTYPE? pSoldier, InventorySlot bPos, OBJECTTYPE? pObj)
    {
        // try autoplacing item in bSlot elsewhere, then do a placement
        bool fAutoPlacedOld;

        fAutoPlacedOld = AutoPlaceObject( pSoldier, (pSoldier.inv[bPos]), false );
        if ( fAutoPlacedOld )
        {
            return( PlaceObject( pSoldier, bPos, pObj ) );
        }
        else
        {
            return( false );
        }
    }

    public static bool PlaceObject(SOLDIERTYPE? pSoldier, InventorySlot bPos, OBJECTTYPE? pObj)
    {
        // returns object to have in hand after placement... same as original in the
        // case of error

        int                 ubSlotLimit, ubNumberToDrop, ubLoop;
        OBJECTTYPE? pInSlot;
        bool                fObjectWasRobotRemote = false;

        if ( pObj.usItem == Items.ROBOT_REMOTE_CONTROL )
        {
            fObjectWasRobotRemote = true;
        }

        if ( !CanItemFitInPosition( pSoldier, pObj, bPos, true ) )
        {
            return( false );
        }

        // If the position is either head slot, then the item must be IC_FACE (checked in
        // CanItemFitInPosition).
        if ( bPos == InventorySlot.HEAD1POS )
        {
            if ( !CompatibleFaceItem( pObj.usItem, pSoldier.inv[ InventorySlot.HEAD2POS ].usItem ) )
            {
                string zTemp = "";

                zTemp = wprintf( Message[(int)STRMessage.CANT_USE_TWO_ITEMS], ItemNames[ pObj.usItem ], ItemNames[ pSoldier.inv[ InventorySlot.HEAD2POS ].usItem ] );
                Messages.ScreenMsg( FontColor.FONT_MCOLOR_LTYELLOW, MSG.UI_FEEDBACK, zTemp );
                return( false );
            }
        }
        else if ( bPos == InventorySlot.HEAD2POS )
        {
            if ( !CompatibleFaceItem( pObj.usItem, pSoldier.inv[ InventorySlot.HEAD1POS ].usItem ) )
            {
                string zTemp = "";

                zTemp = wprintf( Message[(int)STRMessage.CANT_USE_TWO_ITEMS], ItemNames[ pObj.usItem ], ItemNames[ pSoldier.inv[ InventorySlot.HEAD1POS ].usItem ] );
                Messages.ScreenMsg( FontColor.FONT_MCOLOR_LTYELLOW, MSG.UI_FEEDBACK, zTemp );
                return( false );
            }
        }

        if ( Item[ pObj.usItem ].usItemClass == IC.KEY && pSoldier.uiStatusFlags.HasFlag(SOLDIER.PC) )
        {
            if ( Keys.KeyTable[ pObj.ubKeyID ].usDateFound == 0 )
            {
                Keys.KeyTable[ pObj.ubKeyID ].usDateFound = (int) GameClock.GetWorldDay();
                Keys.KeyTable[ pObj.ubKeyID ].usSectorFound = (int) SECTORINFO.SECTOR( pSoldier.sSectorX, pSoldier.sSectorY );
            }
        }

        ubSlotLimit = ItemSlotLimit( pObj.usItem, bPos );

        pInSlot = pSoldier.inv[bPos];

        if (pInSlot.ubNumberOfObjects == 0)
        {
            // placement in an empty slot
            ubNumberToDrop = pObj.ubNumberOfObjects;

            if (ubNumberToDrop > Math.Max( ubSlotLimit, 1 ) )
            {
                // drop as many as possible into pocket
                ubNumberToDrop = Math.Max( ubSlotLimit, 1 );
            }

            // could be wrong type of object for slot... need to check...
            // but assuming it isn't
            pInSlot.CopyFrom( pObj );
    /*
            //if we are in the shopkeeper interface
            if( guiTacticalInterfaceFlags.HasFlag(INTERFACE.SHOPKEEP_INTERFACE) )
            {
                memset( &gMoveingItem, 0, sizeof( INVENTORY_IN_SLOT ) );  // TODO(port): address-of, mem*, sizeof
                SetSkiCursor( CURSOR.NORMAL );
            }
    */

            if (ubNumberToDrop != pObj.ubNumberOfObjects)
            {
                // in the InSlot copy, zero out all the objects we didn't drop
                for (ubLoop = ubNumberToDrop; ubLoop < pObj.ubNumberOfObjects; ubLoop++)
                {
                    pInSlot.bStatus[ubLoop] = 0;
                }
            }
            pInSlot.ubNumberOfObjects = ubNumberToDrop;

            // remove a like number of objects from pObj
            RemoveObjs( pObj, ubNumberToDrop );
            if (pObj.ubNumberOfObjects == 0)
            {
                // dropped everything
                if (bPos == InventorySlot.HANDPOS && Item[pInSlot.usItem].fFlags.HasFlag(ItemAttributes.ITEM_TWO_HANDED))
                {
                    // We just performed a successful drop of a two-handed object into the
                    // main hand
                    if (pSoldier.inv[InventorySlot.SECONDHANDPOS].usItem != Items.NONE)
                    {
                        // swap what WAS in the second hand into the cursor
                        SwapObjs( pObj, (pSoldier.inv[InventorySlot.SECONDHANDPOS]) );
                    }
                }
            }
        }
        else
        {
            // replacement/reloading/merging/stacking
            // keys have an additional check for key ID being the same
            if ( (pObj.usItem == pInSlot.usItem) && ( Item[ pObj.usItem ].usItemClass != IC.KEY || pObj.ubKeyID == pInSlot.ubKeyID ) )
            {
                if (Item[ pObj.usItem ].usItemClass == IC.MONEY)
                {

                    int uiMoneyMax = MoneySlotLimit( bPos );

                    // always allow money to be combined!
                    // IGNORE STATUS!

                    if (pInSlot.uiMoneyAmount + pObj.uiMoneyAmount > uiMoneyMax)
                    {
                        // remove X dollars
                        pObj.uiMoneyAmount -= (uiMoneyMax - pInSlot.uiMoneyAmount);
                        // set in slot to maximum
                        pInSlot.uiMoneyAmount = uiMoneyMax;
                    }
                    else
                    {
                        pInSlot.uiMoneyAmount += pObj.uiMoneyAmount;
                        DeleteObj( pObj );
    /*
                        if( guiTacticalInterfaceFlags.HasFlag(INTERFACE.SHOPKEEP_INTERFACE) )
                        {
                            memset( &gMoveingItem, 0, sizeof( INVENTORY_IN_SLOT ) );  // TODO(port): address-of, mem*, sizeof
                            SetSkiCursor( CURSOR.NORMAL );
                        }
    */
                    }
                }
                else if ( ubSlotLimit == 1 || (ubSlotLimit == 0 && bPos >= InventorySlot.HANDPOS && bPos <= InventorySlot.BIGPOCK4POS ) )
                {
                    if (pObj.ubNumberOfObjects <= 1)
                    {
                        // swapping
                        SwapObjs( pObj, pInSlot );
                    }
                    else
                    {
                        return( DropObjIfThereIsRoom( pSoldier, bPos, pObj ) );
                    }
                }
                else if (ubSlotLimit == 0) // trying to drop into a small pocket
                {
                    return( DropObjIfThereIsRoom( pSoldier, bPos, pObj ) );
                }
                else
                {
                    // stacking
                    ubNumberToDrop = ubSlotLimit - pInSlot.ubNumberOfObjects;
                    if (ubNumberToDrop > pObj.ubNumberOfObjects)
                    {
                        ubNumberToDrop = pObj.ubNumberOfObjects;
                    }
                    StackObjs( pObj, pInSlot, ubNumberToDrop );
                }
            }
            else
            {
                // replacement, unless reloading...
                switch (Item[pInSlot.usItem].usItemClass)
                {
                    case IC.GUN:
                        if (Item[pObj.usItem].usItemClass == IC.AMMO)
                        {
                            if (WeaponTypes.Weapon[pInSlot.usItem].ubCalibre == WeaponTypes.Magazine[Item[pObj.usItem].ubClassIndex].ubCalibre)
                            {
                                // reload...
                                return( ReloadGun( pSoldier, pInSlot, pObj ) );
                            }
                            else
                            {
                                // invalid ammo
                                break;
                                //return( FALSE );
                            }
                        }
                        break;
                    case IC.LAUNCHER:
                    {
                        if ( ValidLaunchable( pObj.usItem, pInSlot.usItem ) )
                        {
                            // reload...
                            return( ReloadGun( pSoldier, pInSlot, pObj ) );
                        }
                    }
                    break;
                }

                if ( (Item[pObj.usItem].fFlags.HasFlag(ItemAttributes.ITEM_TWO_HANDED)) && (bPos == InventorySlot.HANDPOS) )
                {
                    if (pSoldier.inv[InventorySlot.SECONDHANDPOS].usItem != Items.NONE)
                    {
                        // both pockets have something in them, so we can't swap
                        return( false );
                    }
                    else
                    {
                        SwapObjs( pObj, pInSlot );
                    }
                }
                else if (pObj.ubNumberOfObjects <= Math.Max( ubSlotLimit, 1 ) )
                {
                    // swapping
                    SwapObjs( pObj, pInSlot );
                }
                else
                {
                    return( DropObjIfThereIsRoom( pSoldier, bPos, pObj ) );
                }

            }
        }

        // ATE: Put this in to see if we should update the robot, if we were given a controller...
        if ( pSoldier.bTeam == gbPlayerNum && fObjectWasRobotRemote )
        {
            UpdateRobotControllerGivenController( pSoldier );
        }

        return( true );
    }

    public static bool InternalAutoPlaceObject(SOLDIERTYPE? pSoldier, OBJECTTYPE? pObj, bool fNewItem, InventorySlot bExcludeSlot)
    {
        InventorySlot bSlot;
        INVTYPE? pItem;
        int         ubPerSlot;

        // statuses of extra objects would be 0 if the # exceeds the maximum
        Debug.Assert( pObj.ubNumberOfObjects <= MAX_OBJECTS_PER_SLOT);

        pItem = Item[pObj.usItem];
        ubPerSlot = pItem.ubPerPocket;

        // Overrides to the standard system: put guns in hand, armour on body (if slot empty)
        switch (pItem.usItemClass)
        {
            case IC.GUN:
            case IC.BLADE:
            case IC.LAUNCHER:
            case IC.BOMB:
            case IC.GRENADE:
                if (!(pItem.fFlags.HasFlag(ItemAttributes.ITEM_TWO_HANDED)))
                {
                    if (pSoldier.inv[InventorySlot.HANDPOS].usItem == Items.NONE)
                    {
                        // put the one-handed weapon in the guy's hand...
                        PlaceObject( pSoldier, InventorySlot.HANDPOS, pObj );
                        SetNewItem( pSoldier, InventorySlot.HANDPOS, fNewItem );
                        if ( pObj.ubNumberOfObjects == 0 )
                        {
                            return( true );
                        }
                    }
                    else if ( !(Item[pSoldier.inv[InventorySlot.HANDPOS].usItem].fFlags.HasFlag(ItemAttributes.ITEM_TWO_HANDED)) && pSoldier.inv[InventorySlot.SECONDHANDPOS].usItem == Items.NONE)
                    {
                        // put the one-handed weapon in the guy's 2nd hand...
                        PlaceObject( pSoldier, InventorySlot.SECONDHANDPOS, pObj );
                        SetNewItem( pSoldier, InventorySlot.SECONDHANDPOS, fNewItem );
                        if ( pObj.ubNumberOfObjects == 0 )
                        {
                            return( true );
                        }
                    }
                }
                // two-handed objects are best handled in the main loop for large objects,
                // which checks the hands first anyhow
                break;

            case IC.ARMOUR:
                switch (WeaponTypes.Armour[Item[pObj.usItem].ubClassIndex].ubArmourClass)
                {
                    case ARMOURCLASS.VEST:
                        if (pSoldier.inv[InventorySlot.VESTPOS].usItem == Items.NONE)
                        {
                            // put on the armour!
                            PlaceObject( pSoldier, InventorySlot.VESTPOS, pObj );
                            SetNewItem( pSoldier, InventorySlot.VESTPOS, fNewItem );
                            if ( pObj.ubNumberOfObjects == 0 )
                            {
                                return( true );
                            }
                        }
                        break;
                    case ARMOURCLASS.LEGGINGS:
                        if (pSoldier.inv[InventorySlot.LEGPOS].usItem == Items.NONE)
                        {
                            // put on the armour!
                            PlaceObject( pSoldier, InventorySlot.LEGPOS, pObj );
                            SetNewItem( pSoldier, InventorySlot.LEGPOS, fNewItem );
                            if ( pObj.ubNumberOfObjects == 0 )
                            {
                                return( true );
                            }
                        }
                        break;
                    case ARMOURCLASS.HELMET:
                        if (pSoldier.inv[InventorySlot.HELMETPOS].usItem == Items.NONE)
                        {
                            // put on the armour!
                            PlaceObject( pSoldier, InventorySlot.HELMETPOS, pObj );
                            SetNewItem( pSoldier, InventorySlot.HELMETPOS, fNewItem );
                            if ( pObj.ubNumberOfObjects == 0 )
                            {
                                return( true );
                            }
                        }
                        break;
                    default:
                        break;
                }
                // otherwise stuff it in a slot somewhere
                break;
            case IC.FACE:
                if ( (pSoldier.inv[InventorySlot.HEAD1POS].usItem == Items.NONE) && CompatibleFaceItem( pObj.usItem, pSoldier.inv[InventorySlot.HEAD2POS].usItem ) )
                {
                    PlaceObject( pSoldier, InventorySlot.HEAD1POS, pObj );
                    SetNewItem( pSoldier, InventorySlot.HEAD1POS, fNewItem );
                    if ( pObj.ubNumberOfObjects == 0 )
                    {
                        return( true );
                    }
                }
                else if ( (pSoldier.inv[InventorySlot.HEAD2POS].usItem == Items.NONE) && CompatibleFaceItem( pObj.usItem, pSoldier.inv[InventorySlot.HEAD1POS].usItem ) )
                {
                    PlaceObject( pSoldier, InventorySlot.HEAD2POS, pObj );
                    SetNewItem( pSoldier, InventorySlot.HEAD2POS, fNewItem );
                    if ( pObj.ubNumberOfObjects == 0 )
                    {
                        return( true );
                    }
                }
                break;
            default:
                break;
        }

        if (ubPerSlot == 0)
        {
            // Large object; look for an empty hand/large pocket and dump it in there
            // FindObjWithin with 0 will search for empty slots!
            bSlot = InventorySlot.HANDPOS;
            while (true)
            {
                bSlot = FindEmptySlotWithin( pSoldier, bSlot, InventorySlot.BIGPOCK4POS );
                if (bSlot == NO_SLOT)
                {
                    return( false );
                }
                if (bSlot == InventorySlot.SECONDHANDPOS)
                {
                    if (pSoldier.inv[InventorySlot.HANDPOS].usItem != Items.NONE)
                    {
                        bSlot++;
                        continue;
                    }
                }
                // this might fail if we're trying to place in HANDPOS,
                // and SECONDHANDPOS is full
                PlaceObject( pSoldier, bSlot, pObj );
                SetNewItem( pSoldier, bSlot, fNewItem );
                if (pObj.ubNumberOfObjects == 0)
                {
                    return( true );
                }
                bSlot++;
            }
        }
        else
        {
            // Small items; don't allow stack/dumping for keys right now as that
            // would require a bunch of functions for finding the same object by two values...
            if ( ubPerSlot > 1 || Item[ pObj.usItem ].usItemClass == IC.KEY || Item[ pObj.usItem ].usItemClass == IC.MONEY )
            {
                // First, look for slots with the same object, and dump into them.
                bSlot = InventorySlot.HANDPOS;
                while (true)
                {
                    bSlot = FindObjWithin( pSoldier, pObj.usItem, bSlot, InventorySlot.SMALLPOCK8POS );
                    if (bSlot == NO_SLOT)
                    {
                        break;
                    }
                    if ( bSlot != bExcludeSlot )
                    {
                        if ( ( (Item[ pObj.usItem ].usItemClass == IC.MONEY) && pSoldier.inv[ bSlot ].uiMoneyAmount < MoneySlotLimit( bSlot ) ) || (Item[ pObj.usItem ].usItemClass != IC.MONEY && pSoldier.inv[bSlot].ubNumberOfObjects < ItemSlotLimit( pObj.usItem, bSlot ) ) )
                        {
                            // NEW: If in SKI, don't auto-place anything into a stackable slot that's currently hatched out!  Such slots
                            // will disappear in their entirety if sold/moved, causing anything added through here to vanish also!
                            if( !( guiTacticalInterfaceFlags.HasFlag(INTERFACE.SHOPKEEP_INTERFACE) && ShouldSoldierDisplayHatchOnItem( pSoldier.ubProfile, bSlot ) ) )
                            {
                                PlaceObject( pSoldier, bSlot, pObj );
                                SetNewItem( pSoldier, bSlot, fNewItem );
                                if (pObj.ubNumberOfObjects == 0)
                                {
                                    return( true );
                                }
                            }
                        }
                    }
                    bSlot++;
                }
            }
            // Search for empty slots to dump into, starting with small pockets
            bSlot = InventorySlot.SMALLPOCK1POS;
            while (true)
            {
                bSlot = FindEmptySlotWithin( pSoldier, bSlot, InventorySlot.SMALLPOCK8POS );
                if (bSlot == NO_SLOT)
                {
                    break;
                }
                PlaceObject( pSoldier, bSlot, pObj );
                SetNewItem( pSoldier, bSlot, fNewItem );
                if (pObj.ubNumberOfObjects == 0)
                {
                    return( true );
                }
                bSlot++;
            }
            // now check hands/large pockets
            bSlot = InventorySlot.HANDPOS;
            while (true)
            {
                bSlot = FindEmptySlotWithin( pSoldier, bSlot, InventorySlot.BIGPOCK4POS );
                if (bSlot == NO_SLOT)
                {
                    break;
                }
                PlaceObject( pSoldier, bSlot, pObj );
                SetNewItem( pSoldier, bSlot, fNewItem );
                if (pObj.ubNumberOfObjects == 0)
                {
                    return( true );
                }
                bSlot++;
            }
        }
        return( false );
    }

    public static bool AutoPlaceObject(SOLDIERTYPE? pSoldier, OBJECTTYPE? pObj, bool fNewItem)
    {
        return( InternalAutoPlaceObject( pSoldier, pObj, fNewItem, NO_SLOT ) );
    }

    public static bool RemoveObjectFromSlot(SOLDIERTYPE? pSoldier, InventorySlot bPos, OBJECTTYPE? pObj)
    {
        CHECKF( pObj );
        if (pSoldier.inv[bPos].ubNumberOfObjects == 0)
        {
            return( false );
        }
        else
        {
            pObj.CopyFrom( pSoldier.inv[bPos] );
            DeleteObj( (pSoldier.inv[bPos]) );
            return( true );
        }
    }

    public static bool RemoveKeyFromSlot(SOLDIERTYPE? pSoldier, int bKeyRingPosition, OBJECTTYPE? pObj)
    {
        int ubItem = 0;

        CHECKF( pObj );

        if( ( pSoldier.pKeyRing[ bKeyRingPosition ].ubNumber == 0 ) || ( pSoldier.pKeyRing[ bKeyRingPosition ].ubKeyID == INVALID_KEY_NUMBER ) )
        {
            return( false );
        }
        else
        {
            //pObj.CopyFrom( pSoldier->inv[bPos] );

            // create an object
            ubItem = pSoldier.pKeyRing[ bKeyRingPosition ].ubKeyID;

            if( pSoldier.pKeyRing[ bKeyRingPosition ].ubNumber > 1 )
            {
                pSoldier.pKeyRing[ bKeyRingPosition ].ubNumber--;
            }
            else
            {

                pSoldier.pKeyRing[ bKeyRingPosition ].ubNumber = 0;
                pSoldier.pKeyRing[ bKeyRingPosition ].ubKeyID = INVALID_KEY_NUMBER;
            }

            return( CreateKeyObject( pObj, 1, ubItem ) );
        }

        return( false );
    }

    public static bool RemoveKeysFromSlot(SOLDIERTYPE? pSoldier, int bKeyRingPosition, int ubNumberOfKeys, OBJECTTYPE? pObj)
    {
        int ubItems = 0;

        CHECKF( pObj );


        if( ( pSoldier.pKeyRing[ bKeyRingPosition ].ubNumber == 0 ) || ( pSoldier.pKeyRing[ bKeyRingPosition ].ubKeyID == INVALID_KEY_NUMBER ) )
        {
            return( false );
        }
        else
        {
            //pObj.CopyFrom( pSoldier->inv[bPos] );

            if( pSoldier.pKeyRing[ bKeyRingPosition ].ubNumber < ubNumberOfKeys )
            {
                ubNumberOfKeys = pSoldier.pKeyRing[ bKeyRingPosition ].ubNumber;
            }


            ubItems = pSoldier.pKeyRing[ bKeyRingPosition ].ubKeyID;
            if( pSoldier.pKeyRing[ bKeyRingPosition ].ubNumber - ubNumberOfKeys > 0 )
            {
                pSoldier.pKeyRing[ bKeyRingPosition ].ubNumber--;
            }
            else
            {
                pSoldier.pKeyRing[ bKeyRingPosition ].ubNumber = 0;
                pSoldier.pKeyRing[ bKeyRingPosition ].ubKeyID = INVALID_KEY_NUMBER;
            }

            // create an object
            return( CreateKeyObject( pObj, ubNumberOfKeys, ubItems ) );
        }
    }

    public static int AddKeysToSlot(SOLDIERTYPE? pSoldier, int bKeyRingPosition, OBJECTTYPE? pObj)
    {
        int ubNumberNotAdded = 0;

        if ( pSoldier.uiStatusFlags.HasFlag(SOLDIER.PC) ) // redundant but what the hey
        {
            if ( Keys.KeyTable[ pObj.ubKeyID ].usDateFound == 0 )
            {
                Keys.KeyTable[ pObj.ubKeyID ].usDateFound = (int) GameClock.GetWorldDay();
                Keys.KeyTable[ pObj.ubKeyID ].usSectorFound = (int) SECTORINFO.SECTOR( pSoldier.sSectorX, pSoldier.sSectorY );
            }
        }

        // check if we are going to far
        if ( ( pSoldier.pKeyRing[ bKeyRingPosition ].ubNumber + pObj.ubNumberOfObjects ) > Item[ pObj.usItem ].ubPerPocket )
        {
            // only take what we can
            ubNumberNotAdded = pObj.ubNumberOfObjects - ( Item[ pObj.usItem ].ubPerPocket - pSoldier.pKeyRing[ bKeyRingPosition ].ubNumber );

            // set to max
            pSoldier.pKeyRing[ bKeyRingPosition ].ubNumber = Item[ pObj.usItem ].ubPerPocket;

            if( pSoldier.pKeyRing[ bKeyRingPosition ].ubNumber == 0 )
            {
                pSoldier.pKeyRing[ bKeyRingPosition ].ubKeyID = pObj.ubKeyID;
            }

            // return number used
            return( pObj.ubNumberOfObjects - ubNumberNotAdded );
        }
        else
        {
            // check
            if( pSoldier.pKeyRing[ bKeyRingPosition ].ubNumber == 0 )
            {
                pSoldier.pKeyRing[ bKeyRingPosition ].ubKeyID = pObj.ubKeyID;
            }

            pSoldier.pKeyRing[ bKeyRingPosition ].ubNumber += pObj.ubNumberOfObjects;
        }

        return( pObj.ubNumberOfObjects );
    }

    public static int SwapKeysToSlot(SOLDIERTYPE? pSoldier, int bKeyRingPosition, OBJECTTYPE? pObj)
    {
        // swap keys in keyring slot and keys in pocket
        int ubNumberNotAdded = 0;
        OBJECTTYPE TempObj = new();

        // create temp object to hold keys currently in key ring slot
        CreateKeyObject( TempObj, pSoldier.pKeyRing[ bKeyRingPosition ].ubNumber, pSoldier.pKeyRing[ bKeyRingPosition ].ubKeyID );

        pSoldier.pKeyRing[ bKeyRingPosition ].ubNumber = pObj.ubNumberOfObjects;
        pSoldier.pKeyRing[ bKeyRingPosition ].ubKeyID = pObj.ubKeyID;

        // swap params?
        CopyObj( TempObj, pObj );

        return( 1 );
    }

    public static bool CreateKeyObject(OBJECTTYPE? pObj, int ubNumberOfKeys, int ubKeyID)
    {
        bool fRet;

        fRet = CreateItems( (Items) (ItemIndexes.FIRST_KEY + Keys.LockTable[ ubKeyID ].usKeyItem), 100, ubNumberOfKeys, pObj );
        if (fRet)
        {
            pObj.ubKeyID = (byte)ubKeyID;
        }
        //fRet = CreateItems( (UINT16)(ubKeyIdValue + ItemIndexes.FIRST_KEY) , 100, ubNumberOfKeys, pObj )
        //return(  );
        return( fRet );
    }

    public static bool AllocateObject(out OBJECTTYPE pObj)
    {
        // create a key object
        pObj = new OBJECTTYPE();

        return( true );
    }

    public static bool DeleteKeyObject(OBJECTTYPE? pObj)
    {
        if( pObj == null )
        {
            return( false );
        }

        // nothing to free: the garbage collector owns the object

        return( true );
    }

    public static int TotalPoints(OBJECTTYPE? pObj)
    {
        int usPoints = 0;
        int     ubLoop;

        for (ubLoop = 0; ubLoop < pObj.ubNumberOfObjects; ubLoop++)
        {
            usPoints += pObj.bStatus[ubLoop];
        }
        return( usPoints );
    }

    public static Items RandomMagazine(Items usItem, int ubPercentStandard)
    {
        // Note: if any ammo items in the item table are separated from the main group,
        // this function will have to be rewritten to scan the item table for an item
        // with item class ammo, which has class index ubLoop

        WEAPONTYPE? pWeapon;
        int             usLoop;
        int[] usPossibleMagIndex = new int[ MAX_AMMO_TYPES_PER_GUN ];
        int             usPossibleMagCnt = 0;
        int                 ubMagChosen;

        if (!(Item[usItem].usItemClass.HasFlag(IC.GUN)))
        {
            return( 0 );
        }

        pWeapon = WeaponTypes.Weapon[usItem];

        // find & store all possible mag types that fit this gun
        usLoop = 0;
        while ( WeaponTypes.Magazine[ usLoop ].ubCalibre != CaliberType.NOAMMO )
        {
            if (WeaponTypes.Magazine[usLoop].ubCalibre == pWeapon.ubCalibre &&
                    WeaponTypes.Magazine[usLoop].ubMagSize == pWeapon.ubMagSize)
            {
                // store it! (make sure array is big enough)
                Debug.Assert(usPossibleMagCnt < MAX_AMMO_TYPES_PER_GUN);
                usPossibleMagIndex[usPossibleMagCnt++] = usLoop;
            }

            usLoop++;
        }

        // no matches?
        if (usPossibleMagCnt == 0)
        {
            return( 0 );
        }
        else
        // only one match?
        if (usPossibleMagCnt == 1)
        {
            // use that, no choice
            return(MagazineClassIndexToItemType(usPossibleMagIndex[ 0 ] ));
        }
        else    // multiple choices
        {
            // Pick one at random, using supplied probability to pick the default
            if (Globals.Random.Next(100) < ubPercentStandard)
            {
                ubMagChosen = 0;
            }
            else
            {
                // pick a non-standard type instead
                ubMagChosen = ( int ) (1 + Globals.Random.Next(( int ) ( usPossibleMagCnt - 1 )));
            }

            return( MagazineClassIndexToItemType(usPossibleMagIndex[ ubMagChosen ] ) );
        }
    }

    public static bool CreateItems(Items usItem, int bStatus, int ubNumber, OBJECTTYPE? pObj)
    {
        bool fOk;
        int     ubLoop;

        // can't create any more than this, the loop for setting the bStatus[] of others will overwrite memory!
        Debug.Assert( ubNumber <= MAX_OBJECTS_PER_SLOT );

        // ARM: to avoid whacking memory once Assertions are removed...  Items will be lost in this situation!
        if ( ubNumber > MAX_OBJECTS_PER_SLOT )
        {
            ubNumber = MAX_OBJECTS_PER_SLOT;
        }

        fOk = CreateItem( usItem, bStatus, pObj );
        if (fOk)
        {
            for (ubLoop = 1; ubLoop < ubNumber; ubLoop++)
            {
                // we reference status[0] here because the status value might actually be a
                // # of rounds of ammo, in which case the value won't be the bStatus value
                // passed in.
                pObj.bStatus[ubLoop] = pObj.bStatus[0];
            }
            pObj.ubNumberOfObjects = ubNumber;
            pObj.ubWeight *= ubNumber;
            return( true );
        }
        return( false );
    }

    public static bool CreateMoney(int uiMoney, OBJECTTYPE? pObj)
    {
        bool fOk;

        fOk = CreateItem( Items.MONEY, 100, pObj );
        if (fOk)
        {
            pObj.uiMoneyAmount = uiMoney;
        }
        return( fOk );
    }

    public static bool ArmBomb(OBJECTTYPE? pObj, int bSetting)
    {
        bool fRemote = false;
        bool fPressure = false;
        bool fTimed = false;
        bool    fSwitch = false;

        if (pObj.usItem == Items.ACTION_ITEM)
        {
            switch( pObj.bActionValue )
            {
                case ACTION_ITEM.SMALL_PIT:
                case ACTION_ITEM.LARGE_PIT:
                    fPressure = true;
                    break;
                default:
                    fRemote = true;
                    break;

            }
        }
        else if ( FindAttachment( pObj, Items.DETONATOR ) != -1 )
        {
            fTimed = true;
        }
        else if ( (FindAttachment( pObj, Items.REMDETONATOR ) != -1) || (pObj.usItem == Items.ACTION_ITEM) )
        {
            fRemote = true;
        }
        else if ( pObj.usItem == Items.MINE || pObj.usItem == Items.TRIP_FLARE || pObj.usItem == Items.TRIP_KLAXON || pObj.usItem == Items.ACTION_ITEM )
        {
            fPressure = true;
        }
        else if ( pObj.usItem == Items.SWITCH )
        {
            // this makes a remote detonator into a pressure-sensitive trigger
            if ( bSetting == PANIC_FREQUENCY )
            {
                // panic trigger is only activated by expending APs, not by
                // stepping on it... so don't define a detonator type
                fSwitch = true;
            }
            else
            {
                fPressure = true;
            }
        }
        else
        {
            // no sorta bomb at all!
            return( false );
        }

        if (fRemote)
        {
            pObj.bDetonatorType = DetonatorType.BOMB_REMOTE;
            pObj.bFrequency = bSetting;
        }
        else if (fPressure)
        {
            pObj.bDetonatorType = DetonatorType.BOMB_PRESSURE;
            pObj.bFrequency = 0;
        }
        else if (fTimed)
        {
            pObj.bDetonatorType = DetonatorType.BOMB_TIMED;
            // In realtime the player could choose to put down a bomb right before a turn expires, SO
            // add 1 to the setting in RT
            pObj.bDelay = (sbyte)bSetting;
            if ( !(gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.TURNBASED) && gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.INCOMBAT)) )
            {
                pObj.bDelay++;
            }

        }
        else if (fSwitch)
        {
            pObj.bDetonatorType = DetonatorType.BOMB_SWITCH;
            pObj.bFrequency = bSetting;
        }
        else
        {
            return( false );
        }

        pObj.fFlags |= OBJECT.ARMED_BOMB;
        pObj.usBombItem = pObj.usItem;
        return( true );
    }

    public static void SetNewItem(SOLDIERTYPE? pSoldier, InventorySlot ubInvPos, bool fNewItem)
    {
        if( fNewItem )
        {
            pSoldier.bNewItemCount[ ubInvPos ]                       = -1;
            pSoldier.bNewItemCycleCount[ ubInvPos ]          = NEW_ITEM_CYCLE_COUNT;
            pSoldier.fCheckForNewlyAddedItems             = true;
        }
    }

    public static bool PlaceObjectInSoldierProfile(NPCID ubProfile, OBJECTTYPE? pObject)
    {
        InventorySlot bLoop; int bLoop2;
        SOLDIERTYPE? pSoldier;
        Items usItem;
        int             bStatus;
        bool            fReturnVal = false;

        usItem  = pObject.usItem;
        bStatus = pObject.bStatus[0];
        pSoldier = SoldierProfileSubSystem.FindSoldierByProfileID( ubProfile, false );

        if ( Item[ usItem ].usItemClass == IC.MONEY && gMercProfiles[ ubProfile ].uiMoney > 0 )
        {
            gMercProfiles[ ubProfile ].uiMoney += pObject.uiMoneyAmount;
            SetMoneyInSoldierProfile( ubProfile, gMercProfiles[ ubProfile ].uiMoney );
            return( true );
        }

        for (bLoop = InventorySlot.BIGPOCK1POS; bLoop < InventorySlot.SMALLPOCK8POS; bLoop++)
        {
            if ( gMercProfiles[ ubProfile ].bInvNumber[(int)bLoop] == 0 && (pSoldier == null || pSoldier.inv[ bLoop ].usItem == Items.NONE ) )
            {

                // CJC: Deal with money by putting money into # stored in profile
                if ( Item[ usItem ].usItemClass == IC.MONEY )
                {
                    gMercProfiles[ ubProfile ].uiMoney += pObject.uiMoneyAmount;
                    // change any gold/silver to money
                    usItem = Items.MONEY;
                }
                else
                {
                    gMercProfiles[ ubProfile ].inv[bLoop] = usItem;
                    gMercProfiles[ ubProfile ].bInvStatus[(int)bLoop] = (byte)bStatus;
                    gMercProfiles[ ubProfile ].bInvNumber[(int)bLoop] = (byte)pObject.ubNumberOfObjects;
                }

                fReturnVal = true;
                break;
            }
        }

        //uiMoneyAmount
        if ( fReturnVal )
        {
            // ATE: Manage soldier pointer as well....
            //pSoldier = SoldierProfileSubSystem.FindSoldierByProfileID( ubProfile, FALSE );

            // Do we have a valid profile?
            if ( pSoldier != null )
            {
                // OK, place in soldier...
                if ( usItem == Items.MONEY )
                {
                    CreateMoney( gMercProfiles[ ubProfile ].uiMoney, (pSoldier.inv[ bLoop ] ) );
                }
                else
                {
                    if ( pSoldier.ubProfile == NPCID.MADLAB )
                    {
                        // remove attachments and drop them
                        OBJECTTYPE Attachment = new();

                        for ( bLoop2 = MAX_ATTACHMENTS - 1; bLoop2 >= 0; bLoop2-- )
                        {
                            // remove also checks for existence attachment
                            if ( RemoveAttachment( pObject, bLoop2, Attachment ) == true )
                            {
                                // drop it in Madlab's tile
                                HandleItems.AddItemToPool( pSoldier.sGridNo, Attachment, ItemVisibility.VISIBLE, 0, 0, 0 );
                            }
                        }
                    }

                    CreateItem( usItem, bStatus, (pSoldier.inv[ bLoop ] ) );
                }
            }
        }

        return( fReturnVal );
    }

    public static bool RemoveObjectFromSoldierProfile(NPCID ubProfile, Items usItem)
    {
        InventorySlot bLoop;
        SOLDIERTYPE? pSoldier;
        bool    fReturnVal = false;

        if ( usItem == Items.NONE )
        {
            return( true );
        }

        for (bLoop = 0; bLoop < NUM_INV_SLOTS; bLoop++)
        {
            if ( gMercProfiles[ ubProfile ].inv[bLoop] == usItem )
            {
                gMercProfiles[ ubProfile ].inv[bLoop] = Items.NONE;
                gMercProfiles[ ubProfile ].bInvStatus[(int)bLoop] = 0;
                gMercProfiles[ ubProfile ].bInvNumber[(int)bLoop] = 0;

                fReturnVal = true;
                break;
            }
        }

        // ATE: Manage soldier pointer as well....
        pSoldier = SoldierProfileSubSystem.FindSoldierByProfileID( ubProfile, false );

        // Do we have a valid profile?
        if ( pSoldier != null )
        {
            // Remove item...
            RemoveInvObject( pSoldier, usItem );
        }

        return( fReturnVal );
    }

    public static void SetMoneyInSoldierProfile(NPCID ubProfile, int uiMoney)
    {
        //INT8                      bSlot;
        OBJECTTYPE Object = new();
        //SOLDIERTYPE *     pSoldier;
        bool                    fRet;

        // remove all money from soldier
        do
        {
            fRet = RemoveObjectFromSoldierProfile( ubProfile, Items.MONEY );
        }
        while ( fRet == true );

        gMercProfiles[ ubProfile ].uiMoney = 0;

        if (uiMoney > 0)
        {
            // now add the amount specified
            CreateMoney( uiMoney, Object );
            PlaceObjectInSoldierProfile( ubProfile, Object );
        }
    }

    public static InventorySlot FindObjectInSoldierProfile(NPCID ubProfile, Items usItem)
    {
        InventorySlot bLoop;

        for (bLoop = 0; bLoop < NUM_INV_SLOTS; bLoop++)
        {
            if ( gMercProfiles[ ubProfile ].bInvNumber[(int)bLoop] > 0 )
            {
                if ( gMercProfiles[ ubProfile ].inv[bLoop] == usItem )
                {
                    return( bLoop );
                }
            }
        }
        return( NO_SLOT );
    }

    public static bool ObjectExistsInSoldierProfile(NPCID ubProfile, Items usItem)
    {
        InventorySlot bSlot;

        bSlot = FindObjectInSoldierProfile( ubProfile, usItem );
        return( bSlot != NO_SLOT );
    }

    public static void RemoveInvObject(SOLDIERTYPE? pSoldier, Items usItem)
    {
        InventorySlot bInvPos;

        // find object
        bInvPos = FindObj( pSoldier, usItem );
        if (bInvPos != NO_SLOT)
        {

            // Erase!
            pSoldier.inv[ bInvPos ].Clear();

            //Dirty!
            Interface.DirtyMercPanelInterface( pSoldier, DIRTYLEVEL2 );
        }

    }

    public static void CheckEquipmentForDamage(SOLDIERTYPE? pSoldier, int iDamage)
    {
        InventorySlot bSlot;
        bool            fBlowsUp;
        int             ubNumberOfObjects;

        if ( TANK( pSoldier ) )
        {
            return;
        }

        for (bSlot = 0; bSlot < NUM_INV_SLOTS; bSlot++)
        {
            ubNumberOfObjects = pSoldier.inv[bSlot].ubNumberOfObjects;
            fBlowsUp = DamageItem( (pSoldier.inv[bSlot]), iDamage, false );
            if (fBlowsUp)
            {
                // blow it up!
                if ( gTacticalStatus.ubAttackBusyCount > 0 )
                {
                    ExplosionControl.IgniteExplosion( pSoldier.ubAttackerID, IsometricUtils.CenterX( pSoldier.sGridNo ), IsometricUtils.CenterY( pSoldier.sGridNo ), 0, pSoldier.sGridNo, pSoldier.inv[ bSlot ].usItem, pSoldier.bLevel );
                }
                else
                {
                    ExplosionControl.IgniteExplosion( pSoldier.ubID, IsometricUtils.CenterX( pSoldier.sGridNo ), IsometricUtils.CenterY( pSoldier.sGridNo ), 0, pSoldier.sGridNo, pSoldier.inv[ bSlot ].usItem, pSoldier.bLevel );
                }

                // Remove item!
                DeleteObj( (pSoldier.inv[ bSlot ]) );

                Interface.DirtyMercPanelInterface( pSoldier, DIRTYLEVEL2 );
            }
            else if ( ubNumberOfObjects != pSoldier.inv[bSlot].ubNumberOfObjects )
            {
                Interface.DirtyMercPanelInterface( pSoldier, DIRTYLEVEL2 );
            }
        }
    }

    public static void CheckEquipmentForFragileItemDamage(SOLDIERTYPE? pSoldier, int iDamage)
    {
        // glass jars etc can be damaged by falling over
        InventorySlot bSlot;
        int             ubNumberOfObjects;
        bool            fPlayedGlassBreak = false;

        for (bSlot = 0; bSlot < NUM_INV_SLOTS; bSlot++)
        {
            switch( pSoldier.inv[bSlot].usItem )
            {
                case Items.JAR_CREATURE_BLOOD:
                case Items.JAR:
                case Items.JAR_HUMAN_BLOOD:
                case Items.JAR_ELIXIR:
                    ubNumberOfObjects = pSoldier.inv[bSlot].ubNumberOfObjects;
                    DamageItem( (pSoldier.inv[bSlot]), iDamage, false );
                    if ( !fPlayedGlassBreak && (ubNumberOfObjects != pSoldier.inv[bSlot].ubNumberOfObjects) )
                    {
                        // PlayJA2Sample( SoundDefine.GLASS_CRACK, RATE_11025, SoundVolume( MIDVOLUME, pSoldier.sGridNo ), 1, SoundDir( pSoldier.sGridNo ) );
                        fPlayedGlassBreak = true;
                        // only dirty once
                        Interface.DirtyMercPanelInterface( pSoldier, DIRTYLEVEL2 );
                    }
                    break;
                default:
                    break;
            }
        }
    }

    public static int IsMedicalKitItem(OBJECTTYPE? pObject)
    {
        // check item id against current medical kits
        switch( pObject.usItem )
        {
            case( Items.MEDICKIT ):
                // medical bag, return 1
                return ( 1 );
            break;
        }

        return( 0 );
    }

    public static void SwapHandItems(SOLDIERTYPE? pSoldier)
    {
        bool        fOk;

        if ( pSoldier == null ) { return; }
        if (pSoldier.inv[InventorySlot.HANDPOS].usItem == Items.NONE || pSoldier.inv[InventorySlot.SECONDHANDPOS].usItem == Items.NONE)
        {
            // whatever is in the second hand can be swapped to the main hand!
            SwapObjs( (pSoldier.inv[InventorySlot.HANDPOS]), (pSoldier.inv[InventorySlot.SECONDHANDPOS]) );
            Interface.DirtyMercPanelInterface( pSoldier, DIRTYLEVEL2 );
        }
        else
        {
            if (TwoHandedItem( pSoldier.inv[InventorySlot.SECONDHANDPOS].usItem ) )
            {
                // must move the item in the main hand elsewhere in the inventory
                fOk = InternalAutoPlaceObject( pSoldier, (pSoldier.inv[InventorySlot.HANDPOS]), false, InventorySlot.HANDPOS );
                if (!fOk)
                {
                    return;
                }
                // the main hand is now empty so a swap is going to work...
            }
            SwapObjs( (pSoldier.inv[InventorySlot.HANDPOS]), (pSoldier.inv[InventorySlot.SECONDHANDPOS]) );
            Interface.DirtyMercPanelInterface( pSoldier, DIRTYLEVEL2 );
        }
    }

    public static void SwapOutHandItem(SOLDIERTYPE? pSoldier)
    {
        bool            fOk;

        if ( pSoldier == null ) { return; }

        // puts away the item in the main hand
        if (pSoldier.inv[InventorySlot.HANDPOS].usItem != Items.NONE )
        {
            if (pSoldier.inv[InventorySlot.SECONDHANDPOS].usItem == Items.NONE)
            {
                // just swap the hand item to the second hand
                SwapObjs( (pSoldier.inv[InventorySlot.HANDPOS]), (pSoldier.inv[InventorySlot.SECONDHANDPOS]) );
                Interface.DirtyMercPanelInterface( pSoldier, DIRTYLEVEL2 );
                return;
            }
            else
            {
                // try placing it somewhere else in our inventory
                fOk = AutoPlaceObject( pSoldier, (pSoldier.inv[InventorySlot.HANDPOS]), false );
                if (fOk)
                {
                    Interface.DirtyMercPanelInterface( pSoldier, DIRTYLEVEL2 );
                }
                // otherwise there's no room for the item anywhere!
            }
        }
    }

    public static void WaterDamage(SOLDIERTYPE? pSoldier)
    {
        // damage guy's equipment and camouflage due to water
        InventorySlot bLoop; int bDamage, bDieSize;
        int uiRoll;

        if ( pSoldier.bOverTerrainType == TerrainTypeDefines.DEEP_WATER )
        {
            for ( bLoop = 0; bLoop < NUM_INV_SLOTS; bLoop++ )
            {
                // if there's an item here that can get water damaged...
                if (pSoldier.inv[ bLoop ].usItem != Items.NONE && Item[pSoldier.inv[ bLoop ].usItem].fFlags.HasFlag(ItemAttributes.ITEM_WATER_DAMAGES))
                {
                    // roll the 'ol 100-sided dice
                    uiRoll = PreRandom(100);

                    // 10% chance of getting damage!
                    if (uiRoll < 10)
                    {
                        // lose between 1 and 10 status points each time
                        bDamage = (int) (10 - uiRoll);

                        // but don't let anything drop lower than 1%
                        pSoldier.inv[bLoop].bStatus[0] -= bDamage;
                        if (pSoldier.inv[bLoop].bStatus[0] < 1)
                        {
                            pSoldier.inv[bLoop].bStatus[0] = 1;
                        }
                    }
                }
            }
        }
        if (pSoldier.bCamo > 0 && !HAS_SKILL_TRAIT( pSoldier, SkillTrait.CAMOUFLAGED ) )
        {
            // reduce camouflage by 2% per tile of deep water
            // and 1% for medium water
            if ( pSoldier.bOverTerrainType == TerrainTypeDefines.DEEP_WATER )
            {
                pSoldier.bCamo = Math.Max( 0, pSoldier.bCamo - 2 );
            }
            else
            {
                pSoldier.bCamo = Math.Max( 0, pSoldier.bCamo - 1 );
            }
            if (pSoldier.bCamo == 0)
            {
                // Reload palettes....
                if ( pSoldier.bInSector )
                {
                    SoldierControl.CreateSoldierPalettes( pSoldier );
                }
                Messages.ScreenMsg( FontColor.FONT_MCOLOR_LTYELLOW, MSG.INTERFACE, Message[(int)STRMessage.CAMMO_WASHED_OFF], pSoldier.name );
            }

        }
        if ( pSoldier.bTeam == gbPlayerNum && pSoldier.bMonsterSmell > 0 )
        {
            if ( pSoldier.bOverTerrainType == TerrainTypeDefines.DEEP_WATER )
            {
                bDieSize = 10;
            }
            else
            {
                bDieSize = 20;
            }
            if ( Globals.Random.Next( bDieSize ) == 0 )
            {
                pSoldier.bMonsterSmell--;
            }
        }

        Interface.DirtyMercPanelInterface( pSoldier, DIRTYLEVEL2 );
    }

    // note: pfGoodAPs: pointer-to-primitive -> ref (use out if only written)
    public static bool ApplyCammo(SOLDIERTYPE? pSoldier, OBJECTTYPE? pObj, ref bool pfGoodAPs)
    {
        int     bPointsToUse;
        int usTotalKitPoints;

      pfGoodAPs = true;

        if (pObj.usItem != Items.CAMOUFLAGEKIT)
        {
            return( false );
        }

        if (!EnoughPointsStub( pSoldier, AP.CAMOFLAGE, 0, true ) )
        {
        pfGoodAPs = false;
            return( true );
        }

        usTotalKitPoints = TotalPoints( pObj );
        if (usTotalKitPoints == 0)
        {
            // HUH???
            return( false );
        }

        if (pSoldier.bCamo == 100)
        {
            // nothing more to add
            return( false );
        }

        // points are used up at a rate of 50% kit = 100% cammo on guy
        // add 1 to round off
        bPointsToUse = (100 - pSoldier.bCamo + 1 ) / 2;
        bPointsToUse = Math.Min( bPointsToUse, usTotalKitPoints );
        pSoldier.bCamo = Math.Min( 100, pSoldier.bCamo + bPointsToUse * 2);

        DrugsAndAlcohol.UseKitPoints( pObj, bPointsToUse, pSoldier );

        Points.DeductPoints( pSoldier, AP.CAMOFLAGE, 0 );

        // Reload palettes....
        if ( pSoldier.bInSector )
        {
            SoldierControl.CreateSoldierPalettes( pSoldier );
        }

        return( true );
    }

    // note: pfGoodAPs: pointer-to-primitive -> ref (use out if only written)
    public static bool ApplyCanteen(SOLDIERTYPE? pSoldier, OBJECTTYPE? pObj, ref bool pfGoodAPs)
    {
        int     sPointsToUse;
        int usTotalKitPoints;

      pfGoodAPs = true;

        if (pObj.usItem != Items.CANTEEN)
        {
            return( false );
        }

        usTotalKitPoints = TotalPoints( pObj );
        if (usTotalKitPoints == 0)
        {
            // HUH???
            return( false );
        }

        if (!EnoughPointsStub( pSoldier, AP.DRINK, 0, true ) )
        {
        pfGoodAPs = false;
            return( true );
        }

      if ( pSoldier.bTeam == gbPlayerNum )
      {
        if ( gMercProfiles[ pSoldier.ubProfile ].bSex == Sexes.MALE )
        {
              // PlayJA2Sample( SoundDefine.DRINK_CANTEEN_MALE, RATE_11025, MIDVOLUME, 1, SoundPans.MIDDLEPAN );
        }
        else
        {
              // PlayJA2Sample( SoundDefine.DRINK_CANTEEN_FEMALE, RATE_11025, MIDVOLUME, 1, SoundPans.MIDDLEPAN );
        }
      }

        sPointsToUse = Math.Min( 20, usTotalKitPoints );

        // CJC Feb 9.  Canteens don't seem effective enough, so doubled return from them
        Points.DeductPoints( pSoldier, AP.DRINK, (int) (2 * sPointsToUse * -(100 - pSoldier.bBreath) ) );

        DrugsAndAlcohol.UseKitPoints( pObj, sPointsToUse, pSoldier );

        return( true );
    }

    // note: pfGoodAPs: pointer-to-primitive -> ref (use out if only written)
    public static bool ApplyElixir(SOLDIERTYPE? pSoldier, OBJECTTYPE? pObj, ref bool pfGoodAPs)
    {
        int     sPointsToUse;
        int usTotalKitPoints;

      pfGoodAPs = true;

        if (pObj.usItem != Items.JAR_ELIXIR )
        {
            return( false );
        }

        usTotalKitPoints = TotalPoints( pObj );
        if (usTotalKitPoints == 0)
        {
            // HUH???
            return( false );
        }

        if (!EnoughPointsStub( pSoldier, AP.CAMOFLAGE, 0, true ) )
        {
        pfGoodAPs = false;
            return( true );
        }

        Points.DeductPoints( pSoldier, AP.CAMOFLAGE, 0 );

        sPointsToUse = ( MAX_HUMAN_CREATURE_SMELL - pSoldier.bMonsterSmell ) * 2;
        sPointsToUse = Math.Min( sPointsToUse, usTotalKitPoints );

        DrugsAndAlcohol.UseKitPoints( pObj, sPointsToUse, pSoldier );

        pSoldier.bMonsterSmell += sPointsToUse / 2;

        return( true );
    }

    public static int ConvertProfileMoneyValueToObjectTypeMoneyValue(int ubStatus)
    {
        return( ubStatus * 50 );
    }

    public static int ConvertObjectTypeMoneyValueToProfileMoneyValue(int uiMoneyAmount)
    {
        return( (int)( uiMoneyAmount / 50 ) );
    }

    public static bool ItemIsCool(OBJECTTYPE? pObj)
    {
        if (pObj.bStatus[0] < 60)
        {
            return( false );
        }
        if ( (Item[ pObj.usItem ].usItemClass & IC.WEAPON) != 0 )
        {
            if ( WeaponTypes.Weapon[ pObj.usItem ].ubDeadliness >= 30 )
            {
                return( true );
            }
        }
        else if ( Item[ pObj.usItem ].usItemClass.HasFlag(IC.ARMOUR) )
        {
            if ( WeaponTypes.Armour[ Item[ pObj.usItem ].ubClassIndex ].ubProtection >= 20 )
            {
                return( true );
            }
        }

        return( false );
    }

    public static void ActivateXRayDevice(SOLDIERTYPE? pSoldier)
    {
        SOLDIERTYPE? pSoldier2;
        int             uiSlot;
        int bBatteries;

        // check for batteries
        bBatteries = FindAttachment( (pSoldier.inv[InventorySlot.HANDPOS]), Items.BATTERIES );
        if ( bBatteries == -1 )
        {
            // doesn't work without batteries!
            return;
        }

        // use up 8-12 percent of batteries
        pSoldier.inv[ InventorySlot.HANDPOS ].bAttachStatus[ bBatteries ] -= (int) (8 + Globals.Random.Next( 5 ));
        if ( pSoldier.inv[ InventorySlot.HANDPOS ].bAttachStatus[ bBatteries ] <= 0 )
        {
            // destroy batteries
            pSoldier.inv[ InventorySlot.HANDPOS ].usAttachItem[ bBatteries ] = Items.NONE;
            pSoldier.inv[ InventorySlot.HANDPOS ].bAttachStatus[ bBatteries ] = 0;
        }

        // first, scan through all mercs and turn off xrayed flag for anyone
        // previously xrayed by this guy
        for ( uiSlot = 0; uiSlot < guiNumMercSlots; uiSlot++ )
        {
            pSoldier2 = MercSlots[ uiSlot ];
            if ( pSoldier2 != null )
            {
                if ( (pSoldier2.ubMiscSoldierFlags.HasFlag(SOLDIER_MISC.XRAYED)) && (pSoldier2.ubXRayedBy == pSoldier.ubID) )
                {
                    pSoldier2.ubMiscSoldierFlags &= (~SOLDIER_MISC.XRAYED);
                    pSoldier2.ubXRayedBy = NOBODY;
                }
            }
        }
        // now turn on xray for anyone within range
        for ( uiSlot = 0; uiSlot < guiNumMercSlots; uiSlot++ )
        {
            pSoldier2 = MercSlots[ uiSlot ];
            if ( pSoldier2 != null )
            {
                if ( pSoldier2.bTeam != pSoldier.bTeam && IsometricUtils.PythSpacesAway( pSoldier.sGridNo, pSoldier2.sGridNo ) < XRAY_RANGE )
                {
                    pSoldier2.ubMiscSoldierFlags |= SOLDIER_MISC.XRAYED;
                    pSoldier2.ubXRayedBy = pSoldier.ubID;
                }
            }
        }
        pSoldier.uiXRayActivatedTime = (int) GameClock.GetWorldTotalSeconds();
    }

    public static void TurnOffXRayEffects(SOLDIERTYPE? pSoldier)
    {
        SOLDIERTYPE? pSoldier2;
        int             uiSlot;

        if ( pSoldier.uiXRayActivatedTime == 0 )
        {
            return;
        }

        // scan through all mercs and turn off xrayed flag for anyone
        // xrayed by this guy
        for ( uiSlot = 0; uiSlot < guiNumMercSlots; uiSlot++ )
        {
            pSoldier2 = MercSlots[ uiSlot ];
            if ( pSoldier2 != null )
            {
                if ( (pSoldier2.ubMiscSoldierFlags.HasFlag(SOLDIER_MISC.XRAYED)) && (pSoldier2.ubXRayedBy == pSoldier.ubID) )
                {
                    pSoldier2.ubMiscSoldierFlags &= (~SOLDIER_MISC.XRAYED);
                    pSoldier2.ubXRayedBy = NOBODY;
                }
            }
        }
        pSoldier.uiXRayActivatedTime = 0;
    }

    public static void DumpItemsList()
    {
        // open output file
        using var FDump = new System.IO.StreamWriter("ItemDump.txt");

        // print headings
        FDump.WriteLine("            LEVELNODEFLAGS.ITEM              COOLNESS  VALUE");
        FDump.WriteLine("============================  ========  =====");

        foreach (var (usItem, pItem) in Item)
        {
            if (pItem.ubCoolness > 0)
            {
                FDump.WriteLine($"{ItemNames[usItem],28}     {pItem.ubCoolness,2}     ${pItem.usPrice,4}");
            }
        }
    }
}
