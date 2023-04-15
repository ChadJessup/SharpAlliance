using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static SharpAlliance.Core.Globals;
using static SharpAlliance.Core.EnglishText;
using SharpAlliance.Core.SubSystems;
using SharpAlliance.Core.Screens;
using System.IO;
using SharpAlliance.Core.Managers.Image;
using SharpAlliance.Core.Managers;
using SharpAlliance.Core.Managers.VideoSurfaces;
using SixLabors.ImageSharp;
using SharpAlliance.Core.Interfaces;

namespace SharpAlliance.Core;

public class InterfaceItems
{
    private readonly IVideoManager video;

    public InterfaceItems(IVideoManager videoManager) => this.video = videoManager;

    bool AttemptToAddSubstring(string zDest, string zTemp, ref int puiStringLength, int uiPixLimit)
    {
        int uiRequiredStringLength, uiTempStringLength;

        uiTempStringLength = FontSubSystem.StringPixLength(zTemp, ITEMDESC_FONT);
        uiRequiredStringLength = puiStringLength + uiTempStringLength;
        if (zDest[0] != 0)
        {
            uiRequiredStringLength += FontSubSystem.StringPixLength(COMMA_AND_SPACE, ITEMDESC_FONT);
        }
        if (uiRequiredStringLength < uiPixLimit)
        {
            if (zDest[0] != 0)
            {
                wcscat(zDest, COMMA_AND_SPACE);
            }
            wcscat(zDest, zTemp);
            puiStringLength = uiRequiredStringLength;
            return (true);
        }
        else
        {
            wcscat(zDest, DOTDOTDOT);
            return (false);
        }
    }

    void GenerateProsString(string zItemPros, OBJECTTYPE pObject, int uiPixLimit)
    {
        int uiStringLength = 0;
        string zTemp;
        Items usItem = pObject.usItem;
        int ubWeight;

        zItemPros = string.Empty;

        ubWeight = Item[usItem].ubWeight;
        if (Item[usItem].usItemClass == IC.GUN)
        {
            ubWeight += Item[pObject.usGunAmmoItem].ubWeight;
        }

        if (Item[usItem].ubWeight <= EXCEPTIONAL_WEIGHT)
        {
            zTemp = Message[(int)STRINGS.LIGHT];
            if (!AttemptToAddSubstring(zItemPros, zTemp, ref uiStringLength, uiPixLimit))
            {
                return;
            }
        }

        if (Item[usItem].ubPerPocket >= 1) // fits in a small pocket
        {
            zTemp = Message[(int)STRINGS.SMALL];
            if (!AttemptToAddSubstring(zItemPros, zTemp, ref uiStringLength, uiPixLimit))
            {
                return;
            }
        }

        //        if (GunRange(pObject) >= EXCEPTIONAL_RANGE)
        {
            zTemp = Message[(int)STRINGS.LONG_RANGE];
            if (!AttemptToAddSubstring(zItemPros, zTemp, ref uiStringLength, uiPixLimit))
            {
                return;
            }
        }

        //        if (Weapon[usItem].ubImpact >= EXCEPTIONAL_DAMAGE)
        {
            zTemp = Message[(int)STRINGS.HIGH_DAMAGE];
            if (!AttemptToAddSubstring(zItemPros, zTemp, ref uiStringLength, uiPixLimit))
            {
                return;
            }
        }

        //        if (BaseAPsToShootOrStab(DEFAULT_APS, DEFAULT_AIMSKILL, gpItemDescObject) <= EXCEPTIONAL_AP_COST)
        {
            zTemp = Message[(int)STRINGS.QUICK_FIRING];
            if (!AttemptToAddSubstring(zItemPros, zTemp, ref uiStringLength, uiPixLimit))
            {
                return;
            }
        }

        //        if (Weapon[usItem].ubShotsPerBurst >= EXCEPTIONAL_BURST_SIZE || usItem == Items.G11)
        {
            zTemp = Message[(int)STRINGS.FAST_BURST];
            if (!AttemptToAddSubstring(zItemPros, zTemp, ref uiStringLength, uiPixLimit))
            {
                return;
            }
        }

        //        if (Weapon[usItem].ubMagSize > EXCEPTIONAL_MAGAZINE)
        {
            zTemp = Message[(int)STRINGS.LARGE_AMMO_CAPACITY];
            if (!AttemptToAddSubstring(zItemPros, zTemp, ref uiStringLength, uiPixLimit))
            {
                return;
            }
        }

        if (Item[usItem].bReliability >= EXCEPTIONAL_RELIABILITY)
        {
            zTemp = Message[(int)STRINGS.RELIABLE];
            if (!AttemptToAddSubstring(zItemPros, zTemp, ref uiStringLength, uiPixLimit))
            {
                return;
            }
        }

        if (Item[usItem].bRepairEase >= EXCEPTIONAL_REPAIR_EASE)
        {
            zTemp = Message[(int)STRINGS.EASY_TO_REPAIR];
            if (!AttemptToAddSubstring(zItemPros, zTemp, ref uiStringLength, uiPixLimit))
            {
                return;
            }
        }

        if (zItemPros[0] == 0)
        {
            // empty string, so display "None"
            if (!AttemptToAddSubstring(zItemPros, Message[(int)STRINGS.NONE], ref uiStringLength, uiPixLimit))
            {
                return;
            }
        }
    }

    void GenerateConsString(string zItemCons, OBJECTTYPE pObject, int uiPixLimit)
    {
        int uiStringLength = 0;
        string zTemp;
        int ubWeight;
        Items usItem = pObject.usItem;

        zItemCons = string.Empty;

        // calculate the weight of the item plus ammunition but not including any attachments
        ubWeight = Item[usItem].ubWeight;
        if (Item[usItem].usItemClass == IC.GUN)
        {
            ubWeight += Item[pObject.usGunAmmoItem].ubWeight;
        }

        if (ubWeight >= BAD_WEIGHT)
        {
            zTemp = Message[(int)STRINGS.HEAVY];
            if (!AttemptToAddSubstring(zItemCons, zTemp, ref uiStringLength, uiPixLimit))
            {
                return;
            }
        }

        //        if (GunRange(pObject) <= BAD_RANGE)
        //        {
        //            zTemp = Message[(int)STRINGS.SHORT_RANGE];
        //            if (!AttemptToAddSubstring(zItemCons, zTemp, ref uiStringLength, uiPixLimit))
        //            {
        //                return;
        //            }
        //        }

        //        if (Weapon[usItem].ubImpact <= BAD_DAMAGE)
        {
            zTemp = Message[(int)STRINGS.LOW_DAMAGE];
            if (!AttemptToAddSubstring(zItemCons, zTemp, ref uiStringLength, uiPixLimit))
            {
                return;
            }
        }

        //        if (BaseAPsToShootOrStab(DEFAULT_APS, DEFAULT_AIMSKILL, gpItemDescObject) >= BAD_AP_COST)
        {
            zTemp = Message[(int)STRINGS.SLOW_FIRING];
            if (!AttemptToAddSubstring(zItemCons, zTemp, ref uiStringLength, uiPixLimit))
            {
                return;
            }
        }

        //        if (Weapon[usItem].ubShotsPerBurst == 0)
        {
            zTemp = Message[(int)STRINGS.NO_BURST];
            if (!AttemptToAddSubstring(zItemCons, zTemp, ref uiStringLength, uiPixLimit))
            {
                return;
            }
        }

        //        if (Weapon[usItem].ubMagSize < BAD_MAGAZINE)
        {
            zTemp = Message[(int)STRINGS.SMALL_AMMO_CAPACITY];
            if (!AttemptToAddSubstring(zItemCons, zTemp, ref uiStringLength, uiPixLimit))
            {
                return;
            }
        }

        if (Item[usItem].bReliability <= BAD_RELIABILITY)
        {
            zTemp = Message[(int)STRINGS.UNRELIABLE];
            if (!AttemptToAddSubstring(zItemCons, zTemp, ref uiStringLength, uiPixLimit))
            {
                return;
            }
        }

        if (Item[usItem].bRepairEase <= BAD_REPAIR_EASE)
        {
            zTemp = Message[(int)STRINGS.HARD_TO_REPAIR];
            if (!AttemptToAddSubstring(zItemCons, zTemp, ref uiStringLength, uiPixLimit))
            {
                return;
            }
        }


        if (zItemCons[0] == 0)
        {
            // empty string, so display "None"
            if (!AttemptToAddSubstring(zItemCons, Message[(int)STRINGS.NONE], ref uiStringLength, uiPixLimit))
            {
                return;
            }
        }
    }

    bool InitInvSlotInterface(Dictionary<InventorySlot, INV_REGION_DESC> pRegionDesc, INV_REGION_DESC pCamoRegion, MouseCallback INVMoveCallback, MouseCallback INVClickCallback, MouseCallback INVMoveCammoCallback, MouseCallback INVClickCammoCallback, bool fSetHighestPrioity)
    {
        InventorySlot cnt;

        string key;

        // Load all four body type images
        CHECKF(this.video.AddVideoObject("INTERFACE\\inventory_figure_large_male.sti", out key));
        guiBodyInvVO[SoldierBodyTypes.BIGMALE][0] = key;
        CHECKF(this.video.AddVideoObject("INTERFACE\\inventory_figure_large_male_H.sti", out key));
        guiBodyInvVO[SoldierBodyTypes.BIGMALE][1] = key;
        CHECKF(this.video.AddVideoObject("INTERFACE\\inventory_normal_male.sti", out key));
        guiBodyInvVO[SoldierBodyTypes.REGMALE][0] = key;
        CHECKF(this.video.AddVideoObject("INTERFACE\\inventory_normal_male_H.sti", out key));
        guiBodyInvVO[SoldierBodyTypes.REGMALE][1] = key;
        CHECKF(this.video.AddVideoObject("INTERFACE\\inventory_normal_male.sti", out key));
        guiBodyInvVO[SoldierBodyTypes.STOCKYMALE][0] = key;
        CHECKF(this.video.AddVideoObject("INTERFACE\\inventory_normal_male.sti", out key));
        guiBodyInvVO[SoldierBodyTypes.STOCKYMALE][1] = key;
        CHECKF(this.video.AddVideoObject("INTERFACE\\inventory_figure_female.sti", out key));
        guiBodyInvVO[SoldierBodyTypes.REGFEMALE][0] = key;
        CHECKF(this.video.AddVideoObject("INTERFACE\\inventory_figure_female_H.sti", out key));
        guiBodyInvVO[SoldierBodyTypes.REGFEMALE][1] = key;
        CHECKF(this.video.AddVideoObject("INTERFACE\\gold_key_button.sti", out guiGoldKeyVO));

        // Add cammo region 
        MouseSubSystem.MSYS_DefineRegion(
            gSMInvCamoRegion,
            new(
                pCamoRegion.sX,
                pCamoRegion.sY,
                (pCamoRegion.sX + CAMO_REGION_WIDTH),
                (pCamoRegion.sY + CAMO_REGION_HEIGHT)
            ),
            MSYS_PRIORITY.HIGH,
            CURSOR.MSYS_NO_CURSOR,
            INVMoveCammoCallback,
            INVClickCammoCallback);

        // Add region
        MouseSubSystem.MSYS_AddRegion(ref gSMInvCamoRegion);

        // Add regions for inventory slots
        for (cnt = 0; cnt < NUM_INV_SLOTS; cnt++)
        {
            // set inventory pocket coordinates from the table passed in
            gSMInvData[cnt].sX = pRegionDesc[cnt].sX;
            gSMInvData[cnt].sY = pRegionDesc[cnt].sY;

            MouseSubSystem.MSYS_DefineRegion(
                gSMInvRegion[cnt],
                new(
                    gSMInvData[cnt].sX,
                    gSMInvData[cnt].sY,
                    (gSMInvData[cnt].sX + gSMInvData[cnt].sWidth),
                    (gSMInvData[cnt].sY + gSMInvData[cnt].sHeight)
                ),
                (fSetHighestPrioity ? MSYS_PRIORITY.HIGHEST : MSYS_PRIORITY.HIGH),
                CURSOR.MSYS_NO_CURSOR,
                INVMoveCallback,
                INVClickCallback);

            // Add region
            var region = gSMInvRegion[cnt];
            MouseSubSystem.MSYS_AddRegion(ref region);
            MouseSubSystem.MSYS_SetRegionUserData(gSMInvRegion[cnt], 0, cnt);
        }

        gbCompatibleAmmo = new();

        return (true);
    }

    void InitKeyRingInterface(MouseCallback KeyRingClickCallback)
    {
        MouseSubSystem.MSYS_DefineRegion(
            gKeyRingPanel,
            new(KEYRING_X, KEYRING_Y, KEYRING_X + KEYRING_WIDTH, KEYRING_X + KEYRING_HEIGHT),
            MSYS_PRIORITY.HIGH,
            CURSOR.MSYS_NO_CURSOR,
            MSYS_NO_CALLBACK,
            KeyRingClickCallback);

        //        SetRegionFastHelpText((gKeyRingPanel), TacticalStr[KEYRING_HELP_TEXT]);
    }

    void InitMapKeyRingInterface(MouseCallback KeyRingClickCallback)
    {
        MouseSubSystem.MSYS_DefineRegion(
            gKeyRingPanel,
            new(MAP_KEYRING_X, MAP_KEYRING_Y, MAP_KEYRING_X + KEYRING_WIDTH, MAP_KEYRING_Y + KEYRING_HEIGHT),
            MSYS_PRIORITY.HIGH,
            CURSOR.MSYS_NO_CURSOR,
            MSYS_NO_CALLBACK,
            KeyRingClickCallback);

        //        MouseSubSystem.SetRegionFastHelpText((gKeyRingPanel), TacticalStr[KEYRING_HELP_TEXT]);
    }

    void EnableKeyRing(bool fEnable)
    {
        if (fEnable)
        {
            MouseSubSystem.MSYS_EnableRegion(gKeyRingPanel);
        }
        else
        {
            MouseSubSystem.MSYS_DisableRegion(gKeyRingPanel);
        }
    }


    void ShutdownKeyRingInterface()
    {
        MouseSubSystem.MSYS_RemoveRegion(gKeyRingPanel);
        return;
    }

    void DisableInvRegions(bool fDisable)
    {
        InventorySlot cnt;

        for (cnt = 0; cnt < NUM_INV_SLOTS; cnt++)
        {
            if (fDisable)
            {
                MouseSubSystem.MSYS_DisableRegion(gSMInvRegion[cnt]);
            }
            else
            {
                MouseSubSystem.MSYS_EnableRegion(gSMInvRegion[cnt]);
            }
        }

        if (fDisable)
        {
            MouseSubSystem.MSYS_DisableRegion(gSMInvCamoRegion);

            MouseSubSystem.MSYS_DisableRegion(gSM_SELMERCMoneyRegion);
            EnableKeyRing(false);
        }
        else
        {
            MouseSubSystem.MSYS_EnableRegion(gSMInvCamoRegion);

            MouseSubSystem.MSYS_EnableRegion(gSM_SELMERCMoneyRegion);
            EnableKeyRing(true);
        }

    }

    void ShutdownInvSlotInterface()
    {
        InventorySlot cnt;

        // Remove all body type panels
        this.video.DeleteVideoObjectFromIndex(guiBodyInvVO[SoldierBodyTypes.REGMALE][0]);
        this.video.DeleteVideoObjectFromIndex(guiBodyInvVO[SoldierBodyTypes.STOCKYMALE][0]);
        this.video.DeleteVideoObjectFromIndex(guiBodyInvVO[SoldierBodyTypes.BIGMALE][0]);
        this.video.DeleteVideoObjectFromIndex(guiBodyInvVO[SoldierBodyTypes.REGFEMALE][0]);
        this.video.DeleteVideoObjectFromIndex(guiBodyInvVO[SoldierBodyTypes.REGMALE][1]);
        this.video.DeleteVideoObjectFromIndex(guiBodyInvVO[SoldierBodyTypes.STOCKYMALE][1]);
        this.video.DeleteVideoObjectFromIndex(guiBodyInvVO[SoldierBodyTypes.BIGMALE][1]);
        this.video.DeleteVideoObjectFromIndex(guiBodyInvVO[SoldierBodyTypes.REGFEMALE][1]);

        this.video.DeleteVideoObjectFromIndex(guiGoldKeyVO);

        // Remove regions
        // Add regions for inventory slots
        for (cnt = 0; cnt < NUM_INV_SLOTS; cnt++)
        {
            // Remove region
            MouseSubSystem.MSYS_RemoveRegion(gSMInvRegion[cnt]);
        }

        // Remove cammo
        MouseSubSystem.MSYS_RemoveRegion(gSMInvCamoRegion);

    }

    void RenderInvBodyPanel(SOLDIERTYPE pSoldier, int sX, int sY)
    {
        // Blit body inv, based on body type
        int bSubImageIndex = gbCompatibleApplyItem;

        //        BltVideoObjectFromIndex(guiSAVEBUFFER, guiBodyInvVO[pSoldier.ubBodyType][bSubImageIndex], 0, sX, sY, VO_BLT.SRCTRANSPARENCY, null);
    }


    static string pStr;
    void HandleRenderInvSlots(SOLDIERTYPE pSoldier, int fDirtyLevel)
    {
        InventorySlot cnt;

        if (InItemDescriptionBox() || InItemStackPopup() || InKeyRingPopup())
        {

        }
        else
        {
            for (cnt = 0; cnt < NUM_INV_SLOTS; cnt++)
            {
                if (fDirtyLevel == DIRTYLEVEL2)
                {
                    GetHelpTextForItem(pStr, (pSoldier.inv[cnt]), pSoldier);

                    //                    SetRegionFastHelpText((gSMInvRegion[cnt]), pStr);
                }

                INVRenderINVPanelItem(pSoldier, cnt, fDirtyLevel);
            }

            //            if (KeyExistsInKeyRing(pSoldier, ANYKEY, null))
            //            {
            //                // blit gold key here?
            //                if (guiCurrentItemDescriptionScreen != ScreenName.MAP_SCREEN)
            //                {
            //                    BltVideoObjectFromIndex(guiSAVEBUFFER, guiGoldKeyVO, 0, 496, 446, VO_BLT.SRCTRANSPARENCY, null);
            //                    RestoreExternBackgroundRect(496, 446, 29, 23);
            //                }
            //                else
            //                {
            //                    BltVideoObjectFromIndex(guiSAVEBUFFER, guiGoldKeyVO, 0, 217, 271, VO_BLT.SRCTRANSPARENCY, null);
            //                    RestoreExternBackgroundRect(217, 271, 29, 23);
            //                }
            //            }
        }
    }


    void INVRenderINVPanelItem(SOLDIERTYPE pSoldier, InventorySlot sPocket, int fDirtyLevel)
    {
        int sX, sY;
        int sBarX, sBarY;
        OBJECTTYPE pObject;
        bool fOutline = false;
        int sOutlineColor = 0;
        int fRenderDirtyLevel;
        bool fHatchItOut = false;


        //Assign the screen
        guiCurrentItemDescriptionScreen = guiCurrentScreen;

        pObject = (pSoldier.inv[sPocket]);

        sX = gSMInvData[sPocket].sX;
        sY = gSMInvData[sPocket].sY;

        if (fDirtyLevel == DIRTYLEVEL2)
        {
            // CHECK FOR COMPATIBILITY WITH MAGAZINES

            /*	OLD VERSION OF GUN/AMMO MATCH HIGHLIGHTING
                    int	uiDestPitchBYTES;
                    int		*pDestBuf;
                    int	usLineColor;

                    if ( ( Item [ pSoldier.inv[ HANDPOS ].usItem ].usItemClass & IC_GUN )  && ( Item[ pObject.usItem ].usItemClass & IC_AMMO ) )
                    {
                        // CHECK
                        if (Weapon[pSoldier.inv[ HANDPOS ].usItem].ubCalibre == Magazine[Item[pObject.usItem].ubClassIndex].ubCalibre )
                        {
                            // IT's an OK calibre ammo, do something!
                            // Render Item with specific color
                            //fOutline = true;
                            //sOutlineColor = Get16BPPColor( FROMRGB( 96, 104, 128 ) );
                            //sOutlineColor = Get16BPPColor( FROMRGB( 20, 20, 120 ) );

                            // Draw rectangle!
                            pDestBuf = LockVideoSurface( guiSAVEBUFFER, &uiDestPitchBYTES );
                            SetClippingRegionAndImageWidth( uiDestPitchBYTES, 0, 0, 640, 480 );

                            //usLineColor = Get16BPPColor( FROMRGB( 255, 255, 0 ) );
                            usLineColor = Get16BPPColor( FROMRGB( 230, 215, 196 ) );
                            RectangleDraw( true, (sX+1), (sY+1), (sX + gSMInvData[ sPocket ].sWidth - 2 ),( sY + gSMInvData[ sPocket ].sHeight - 2 ), usLineColor, pDestBuf );

                            SetClippingRegionAndImageWidth( uiDestPitchBYTES, 0, 0, 640, 480 );

                            UnLockVideoSurface( guiSAVEBUFFER );
                        }
                    }
            */

            if (gbCompatibleAmmo[sPocket] > 0)
            {
                fOutline = true;
                //                sOutlineColor = Get16BPPColor(FROMRGB(255, 255, 255));
            }

            // IF it's the second hand and this hand cannot contain anything, remove the second hand position graphic
            //            if (sPocket == InventorySlot.SECONDHANDPOS && Item[pSoldier.inv[InventorySlot.HANDPOS].usItem].fFlags & ITEM_TWO_HANDED)
            {
                //			if( guiCurrentScreen != MAP_SCREEN )
                if (guiCurrentItemDescriptionScreen != ScreenName.MAP_SCREEN)
                {
                    //                    BltVideoObjectFromIndex(guiSAVEBUFFER, guiSecItemHiddenVO, 0, 217, 448, VO_BLT.SRCTRANSPARENCY, null);
                    //                    RestoreExternBackgroundRect(217, 448, 72, 28);
                }
                else
                {
                    //                    BltVideoObjectFromIndex(guiSAVEBUFFER, guiMapInvSecondHandBlockout, 0, 14, 218, VO_BLT.SRCTRANSPARENCY, null);
                    //                    RestoreExternBackgroundRect(14, 218, 102, 24);
                }
            }
        }

        // If we have a new item and we are in the right panel...
        if (pSoldier.bNewItemCount[sPocket] > 0 && gsCurInterfacePanel == InterfacePanelDefines.SM_PANEL && fInterfacePanelDirty != DIRTYLEVEL2)
        {
            fRenderDirtyLevel = DIRTYLEVEL0;
            //fRenderDirtyLevel = fDirtyLevel;
        }
        else
        {
            fRenderDirtyLevel = fDirtyLevel;
        }

        //Now render as normal
        //INVRenderItem( guiSAVEBUFFER, pObject, (int)(sX + gSMInvData[ sPocket ].sSubX), (int)(sY + gSMInvData[ sPocket ].sSubY), gSMInvData[ sPocket ].sWidth, gSMInvData[ sPocket ].sHeight, fDirtyLevel, &(gfSM_HandInvDispText[ sPocket ] ) );
        int? _ = null;
        INVRenderItem(guiSAVEBUFFER, pSoldier, pObject, sX, sY, gSMInvData[sPocket].sWidth, gSMInvData[sPocket].sHeight, fRenderDirtyLevel, _, 0, fOutline, sOutlineColor);

        if (gbInvalidPlacementSlot[sPocket] > 0)
        {
            if (sPocket != InventorySlot.SECONDHANDPOS)
            {
                // If we are in inv panel and our guy is not = cursor guy...
                if (!gfSMDisableForItems)
                {
                    fHatchItOut = true;
                }
            }
        }

        //if we are in the shop keeper interface
        if (guiTacticalInterfaceFlags.HasFlag(INTERFACE.SHOPKEEP_INTERFACE))
        {
            //            if (ShouldSoldierDisplayHatchOnItem(pSoldier.ubProfile, sPocket) && !gbInvalidPlacementSlot[sPocket])
            {
                fHatchItOut = true;
            }
        }

        if (fHatchItOut)
        {
            Surfaces uiWhichBuffer = (guiCurrentItemDescriptionScreen == ScreenName.MAP_SCREEN) ? guiSAVEBUFFER : guiRENDERBUFFER;
            //            DrawHatchOnInventory(uiWhichBuffer, sX, sY, (int)(gSMInvData[sPocket].sWidth - 1), (int)(gSMInvData[sPocket].sHeight - 1));
        }

        // if there's an item in there
        if (pObject.usItem != NOTHING)
        {
            // Add item status bar
            sBarX = sX - gSMInvData[sPocket].sBarDx;
            sBarY = sY + gSMInvData[sPocket].sBarDy;
            //            DrawItemUIBarEx(pObject, 0, sBarX, sBarY, ITEM_BAR_WIDTH, ITEM_BAR_HEIGHT, Get16BPPColor(STATUS_BAR), Get16BPPColor(STATUS_BAR_SHADOW), true, guiSAVEBUFFER);
        }

    }


    bool CompatibleAmmoForGun(OBJECTTYPE pTryObject, OBJECTTYPE pTestObject)
    {
        if ((Item[pTryObject.usItem].usItemClass.HasFlag(IC.AMMO)))
        {
            // CHECK
            //            if (Weapon[pTestObject.usItem].ubCalibre == Magazine[Item[pTryObject.usItem].ubClassIndex].ubCalibre)
            {
                return (true);
            }
        }
        return (false);
    }

    bool CompatibleGunForAmmo(OBJECTTYPE pTryObject, OBJECTTYPE pTestObject)
    {
        if ((Item[pTryObject.usItem].usItemClass.HasFlag(IC.GUN)))
        {
            // CHECK
            //            if (Weapon[pTryObject.usItem].ubCalibre == Magazine[Item[pTestObject.usItem].ubClassIndex].ubCalibre)
            {
                return (true);
            }
        }
        return (false);
    }

    bool CompatibleItemForApplyingOnMerc(OBJECTTYPE? pTestObject)
    {
        Items usItem = pTestObject.usItem;

        // ATE: If in mapscreen, return false always....
        if (guiTacticalInterfaceFlags.HasFlag(INTERFACE.MAPSCREEN))
        {
            return (false);
        }

        // ATE: Would be nice to have flag here to check for these types....
        if (usItem == Items.CAMOUFLAGEKIT
            || usItem == Items.ADRENALINE_BOOSTER
            || usItem == Items.REGEN_BOOSTER
            || usItem == Items.SYRINGE_3
            || usItem == Items.SYRINGE_4
            || usItem == Items.SYRINGE_5
            || usItem == Items.ALCOHOL
            || usItem == Items.WINE
            || usItem == Items.BEER
            || usItem == Items.CANTEEN
            || usItem == Items.JAR_ELIXIR)
        {
            return (true);
        }
        else
        {
            return (false);
        }
    }



    bool SoldierContainsAnyCompatibleStuff(SOLDIERTYPE pSoldier, OBJECTTYPE pTestObject)
    {
        InventorySlot cnt;
        OBJECTTYPE pObject;

        if ((Item[pTestObject.usItem].usItemClass.HasFlag(IC.GUN)))
        {
            for (cnt = 0; cnt < NUM_INV_SLOTS; cnt++)
            {
                pObject = (pSoldier.inv[cnt]);

                if (CompatibleAmmoForGun(pObject, pTestObject))
                {
                    return (true);
                }
            }
        }

        if ((Item[pTestObject.usItem].usItemClass.HasFlag(IC.AMMO)))
        {
            for (cnt = 0; cnt < NUM_INV_SLOTS; cnt++)
            {
                pObject = (pSoldier.inv[cnt]);

                if (CompatibleGunForAmmo(pObject, pTestObject))
                {
                    return (true);
                }
            }
        }

        // ATE: Put attachment checking here.....

        return (false);
    }


    void HandleAnyMercInSquadHasCompatibleStuff(SquadEnum ubSquad, OBJECTTYPE pObject, bool fReset)
    {
        int iCounter = 0;

        if (ubSquad == NUMBER_OF_SQUADS)
        {
            return;
        }

        for (iCounter = 0; iCounter < NUMBER_OF_SOLDIERS_PER_SQUAD; iCounter++)
        {
            if (Squad[iCurrentTacticalSquad][iCounter] != null)
            {
                if (!fReset)
                {
                    if (SoldierContainsAnyCompatibleStuff(Squad[iCurrentTacticalSquad][iCounter], pObject))
                    {
                        // Get face and set value....
                        gFacesData[Squad[iCurrentTacticalSquad][iCounter].iFaceIndex].fCompatibleItems = true;
                    }
                }
                else
                {
                    gFacesData[Squad[iCurrentTacticalSquad][iCounter].iFaceIndex].fCompatibleItems = false;
                }
            }
        }
    }

    bool HandleCompatibleAmmoUIForMapScreen(SOLDIERTYPE pSoldier, InventorySlot bInvPos, int fOn, bool fFromMerc)
    {
        bool fFound = false;
        InventorySlot cnt;
        OBJECTTYPE pObject;
        OBJECTTYPE? pTestObject = null;
        bool fFoundAttachment = false;

        if (fFromMerc == false)
        {
            //            pTestObject = (pInventoryPoolList[bInvPos].o);
        }
        else
        {
            if (bInvPos == NO_SLOT)
            {
                pTestObject = null;
            }
            else
            {
                pTestObject = (pSoldier.inv[bInvPos]);
            }
        }

        // ATE: If pTest object is null, test only for existence of syringes, etc...
        if (pTestObject == null)
        {
            for (cnt = 0; cnt < NUM_INV_SLOTS; cnt++)
            {
                pObject = (pSoldier.inv[cnt]);

                if (CompatibleItemForApplyingOnMerc(pObject))
                {
                    if (fOn != gbCompatibleAmmo[cnt])
                    {
                        fFound = true;
                    }

                    // IT's an OK calibere ammo, do something!
                    // Render Item with specific color
                    gbCompatibleAmmo[cnt] = fOn;

                }
            }


            if (gpItemPointer != null)
            {
                if (CompatibleItemForApplyingOnMerc(gpItemPointer))
                {
                    // OK, Light up portrait as well.....
                    if (fOn > 0)
                    {
                        gbCompatibleApplyItem = 1;
                    }
                    else
                    {
                        gbCompatibleApplyItem = 0;
                    }

                    fFound = true;
                }
            }

            if (fFound)
            {
                fInterfacePanelDirty = DIRTYLEVEL2;
                //HandleRenderInvSlots( pSoldier, DIRTYLEVEL2 );
            }

            return (fFound);
        }

        if ((!Item[pTestObject.usItem].fFlags.HasFlag(ItemAttributes.ITEM_HIDDEN_ADDON)))
        {
            // First test attachments, which almost any type of item can have....
            for (cnt = 0; cnt < NUM_INV_SLOTS; cnt++)
            {
                pObject = (pSoldier.inv[cnt]);

                if (Item[pObject.usItem].fFlags.HasFlag(ItemAttributes.ITEM_HIDDEN_ADDON))
                {
                    // don't consider for UI purposes
                    continue;
                }

                //                if (ValidAttachment(pObject.usItem, pTestObject.usItem)
                //                    || ValidAttachment(pTestObject.usItem, pObject.usItem)
                //                    || ValidLaunchable(pTestObject.usItem, pObject.usItem)
                //                    || ValidLaunchable(pObject.usItem, pTestObject.usItem))
                {
                    fFoundAttachment = true;

                    if (fOn != gbCompatibleAmmo[cnt])
                    {
                        fFound = true;
                    }

                    // IT's an OK calibere ammo, do something!
                    // Render Item with specific color
                    gbCompatibleAmmo[cnt] = fOn;
                }
            }
        }


        if ((Item[pTestObject.usItem].usItemClass.HasFlag(IC.GUN)))
        {
            for (cnt = 0; cnt < NUM_INV_SLOTS; cnt++)
            {
                pObject = (pSoldier.inv[cnt]);

                if (CompatibleAmmoForGun(pObject, pTestObject))
                {
                    if (fOn != gbCompatibleAmmo[cnt])
                    {
                        fFound = true;
                    }

                    // IT's an OK calibere ammo, do something!
                    // Render Item with specific color
                    gbCompatibleAmmo[cnt] = fOn;
                }
            }
        }
        else if ((Item[pTestObject.usItem].usItemClass.HasFlag(IC.AMMO)))
        {
            for (cnt = 0; cnt < NUM_INV_SLOTS; cnt++)
            {
                pObject = (pSoldier.inv[cnt]);

                if (CompatibleGunForAmmo(pObject, pTestObject))
                {
                    if (fOn != gbCompatibleAmmo[cnt])
                    {
                        fFound = true;
                    }

                    // IT's an OK calibere ammo, do something!
                    // Render Item with specific color
                    gbCompatibleAmmo[cnt] = fOn;

                }
            }
        }


        return (fFound);
    }

    bool HandleCompatibleAmmoUIForMapInventory(SOLDIERTYPE pSoldier, InventorySlot bInvPos, int iStartSlotNumber, bool fOn, bool fFromMerc)
    {
        // CJC: ATE, needs fixing here!

        bool fFound = false;
        int cnt;
        OBJECTTYPE pObject, pTestObject;
        bool fFoundAttachment = false;

        if (fFromMerc == false)
        {
            //            pTestObject = (pInventoryPoolList[iStartSlotNumber + bInvPos].o);
        }
        else
        {
            if (bInvPos == NO_SLOT)
            {
                pTestObject = null;
            }
            else
            {
                pTestObject = (pSoldier.inv[bInvPos]);
            }
        }

        // First test attachments, which almost any type of item can have....
        //        for (cnt = 0; cnt < MAP_INVENTORY_POOL_SLOT_COUNT; cnt++)
        //        {
        //            pObject = (pInventoryPoolList[iStartSlotNumber + cnt].o);
        //
        //            if (Item[pObject.usItem].fFlags & ITEM_HIDDEN_ADDON)
        //            {
        //                // don't consider for UI purposes
        //                continue;
        //            }
        //
        //            if (ValidAttachment(pObject.usItem, pTestObject.usItem) ||
        //                     ValidAttachment(pTestObject.usItem, pObject.usItem) ||
        //                     ValidLaunchable(pTestObject.usItem, pObject.usItem) ||
        //                     ValidLaunchable(pObject.usItem, pTestObject.usItem))
        //            {
        //                fFoundAttachment = true;
        //
        //                if (fOn != fMapInventoryItemCompatable[cnt])
        //                {
        //                    fFound = true;
        //                }
        //
        //                // IT's an OK calibere ammo, do something!
        //                // Render Item with specific color
        //                fMapInventoryItemCompatable[cnt] = fOn;
        //            }
        //        }


        //        if ((Item[pTestObject.usItem].usItemClass.HasFlag(IC.GUN)))
        //        {
        //            for (cnt = 0; cnt < MAP_INVENTORY_POOL_SLOT_COUNT; cnt++)
        //            {
        //                pObject = (pInventoryPoolList[iStartSlotNumber + cnt].o);
        //
        //                if (CompatibleAmmoForGun(pObject, pTestObject))
        //                {
        //                    if (fOn != fMapInventoryItemCompatable[cnt])
        //                    {
        //                        fFound = true;
        //                    }
        //
        //                    // IT's an OK calibere ammo, do something!
        //                    // Render Item with specific color
        //                    fMapInventoryItemCompatable[cnt] = fOn;
        //                }
        //            }
        //        }
        //        else if ((Item[pTestObject.usItem].usItemClass.HasFlag(IC.AMMO)))
        //        {
        //            for (cnt = 0; cnt < MAP_INVENTORY_POOL_SLOT_COUNT; cnt++)
        //            {
        //                pObject = (pInventoryPoolList[iStartSlotNumber + cnt].o);
        //
        //                if (CompatibleGunForAmmo(pObject, pTestObject))
        //                {
        //                    if (fOn != fMapInventoryItemCompatable[cnt])
        //                    {
        //                        fFound = true;
        //                    }
        //
        //                    // IT's an OK calibere ammo, do something!
        //                    // Render Item with specific color
        //                    fMapInventoryItemCompatable[cnt] = fOn;
        //
        //                }
        //            }
        //        }

        return (fFound);
    }


    bool InternalHandleCompatibleAmmoUI(SOLDIERTYPE pSoldier, OBJECTTYPE pTestObject, int fOn)
    {
        bool fFound = false;
        InventorySlot cnt;
        OBJECTTYPE pObject;
        bool fFoundAttachment = false;

        // ATE: If pTest object is null, test only for existence of syringes, etc...
        if (pTestObject == null)
        {
            for (cnt = 0; cnt < NUM_INV_SLOTS; cnt++)
            {
                pObject = (pSoldier.inv[cnt]);

                if (CompatibleItemForApplyingOnMerc(pObject))
                {
                    if (fOn != gbCompatibleAmmo[cnt])
                    {
                        fFound = true;
                    }

                    // IT's an OK calibere ammo, do something!
                    // Render Item with specific color
                    gbCompatibleAmmo[cnt] = fOn;

                }
            }


            if (gpItemPointer != null)
            {
                if (CompatibleItemForApplyingOnMerc(gpItemPointer))
                {
                    // OK, Light up portrait as well.....
                    if (fOn > 0)
                    {
                        gbCompatibleApplyItem = 1;
                    }
                    else
                    {
                        gbCompatibleApplyItem = 0;
                    }

                    fFound = true;
                }
            }

            if (fFound)
            {
                fInterfacePanelDirty = DIRTYLEVEL2;
                //HandleRenderInvSlots( pSoldier, DIRTYLEVEL2 );
            }

            return (fFound);
        }

        // First test attachments, which almost any type of item can have....
        for (cnt = 0; cnt < NUM_INV_SLOTS; cnt++)
        {
            pObject = (pSoldier.inv[cnt]);

            if (Item[pObject.usItem].fFlags.HasFlag(ItemAttributes.ITEM_HIDDEN_ADDON))
            {
                // don't consider for UI purposes
                continue;
            }

            //            if (ValidAttachment(pObject.usItem, pTestObject.usItem) ||
            //                     ValidAttachment(pTestObject.usItem, pObject.usItem) ||
            //                     ValidLaunchable(pTestObject.usItem, pObject.usItem) ||
            //                     ValidLaunchable(pObject.usItem, pTestObject.usItem))
            {
                fFoundAttachment = true;

                if (fOn != gbCompatibleAmmo[cnt])
                {
                    fFound = true;
                }

                // IT's an OK calibere ammo, do something!
                // Render Item with specific color
                gbCompatibleAmmo[cnt] = fOn;
            }
        }

        //if ( !fFoundAttachment )
        //{
        if ((Item[pTestObject.usItem].usItemClass.HasFlag(IC.GUN)))
        {
            for (cnt = 0; cnt < NUM_INV_SLOTS; cnt++)
            {
                pObject = (pSoldier.inv[cnt]);

                if (CompatibleAmmoForGun(pObject, pTestObject))
                {
                    if (fOn != gbCompatibleAmmo[cnt])
                    {
                        fFound = true;
                    }

                    // IT's an OK calibere ammo, do something!
                    // Render Item with specific color
                    gbCompatibleAmmo[cnt] = fOn;
                }
            }
        }

        else if ((Item[pTestObject.usItem].usItemClass.HasFlag(IC.AMMO)))
        {
            for (cnt = 0; cnt < NUM_INV_SLOTS; cnt++)
            {
                pObject = (pSoldier.inv[cnt]);

                if (CompatibleGunForAmmo(pObject, pTestObject))
                {
                    if (fOn != gbCompatibleAmmo[cnt])
                    {
                        fFound = true;
                    }

                    // IT's an OK calibere ammo, do something!
                    // Render Item with specific color
                    gbCompatibleAmmo[cnt] = fOn;

                }
            }
        }
        else if (CompatibleItemForApplyingOnMerc(pTestObject))
        {
            //If we are currently NOT in the Shopkeeper interface
            if (!(guiTacticalInterfaceFlags.HasFlag(INTERFACE.SHOPKEEP_INTERFACE)))
            {
                fFound = true;
                gbCompatibleApplyItem = fOn;
            }
        }
        //}


        if (!fFound)
        {
            for (cnt = 0; cnt < NUM_INV_SLOTS; cnt++)
            {
                if (gbCompatibleAmmo[cnt] > 0)
                {
                    fFound = true;
                    gbCompatibleAmmo[cnt] = 0;
                }

                if (gbCompatibleApplyItem > 0)
                {
                    fFound = true;
                    gbCompatibleApplyItem = 0;
                }
            }
        }

        if (fFound)
        {
            fInterfacePanelDirty = DIRTYLEVEL2;
            //HandleRenderInvSlots( pSoldier, DIRTYLEVEL2 );
        }

        return (fFound);

    }

    void ResetCompatibleItemArray()
    {
        InventorySlot cnt = 0;

        for (cnt = 0; cnt < NUM_INV_SLOTS; cnt++)
        {
            if (gbCompatibleAmmo[cnt] > 0)
            {
                gbCompatibleAmmo[cnt] = 0;
            }
        }
    }

    bool HandleCompatibleAmmoUI(SOLDIERTYPE pSoldier, InventorySlot bInvPos, int fOn)
    {
        InventorySlot cnt;
        OBJECTTYPE? pTestObject;
        bool fFound = false;

        //if we are in the shopkeeper interface
        if (guiTacticalInterfaceFlags.HasFlag(INTERFACE.SHOPKEEP_INTERFACE))
        {
            // if the inventory position is -1, this is a flag from the Shopkeeper interface screen
            //indicating that we are to use a different object to do the search
            if (bInvPos == InventorySlot.UNSET)
            {
                if (fOn > 0)
                {
                    if (gpHighLightedItemObject is not null)
                    {
                        pTestObject = gpHighLightedItemObject;
                        //					gubSkiDirtyLevel = SKI_DIRTY_LEVEL2;
                    }
                    else
                    {
                        return (false);
                    }
                }
                else
                {
                    gpHighLightedItemObject = null;

                    for (cnt = 0; cnt < NUM_INV_SLOTS; cnt++)
                    {
                        if (gbCompatibleAmmo[cnt] > 0)
                        {
                            fFound = true;
                            gbCompatibleAmmo[cnt] = 0;
                        }
                    }

                    //                    gubSkiDirtyLevel = SKI_DIRTY_LEVEL1;
                    return (true);
                }
            }
            else
            {
                if (fOn > 0)
                {
                    pTestObject = (pSoldier.inv[bInvPos]);
                    gpHighLightedItemObject = pTestObject;
                }
                else
                {
                    pTestObject = (pSoldier.inv[bInvPos]);
                    gpHighLightedItemObject = null;
                    //                    gubSkiDirtyLevel = SKI_DIRTY_LEVEL1;
                }
            }
        }
        else
        {
            //		if( fOn )

            if (bInvPos == NO_SLOT)
            {
                pTestObject = null;
            }
            else
            {
                pTestObject = (pSoldier.inv[bInvPos]);
            }

        }

        return (InternalHandleCompatibleAmmoUI(pSoldier, pTestObject, fOn));

    }

    void GetSlotInvXY(InventorySlot ubPos, out int psX, out int psY)
    {
        psX = gSMInvData[ubPos].sX;
        psY = gSMInvData[ubPos].sY;
    }

    void GetSlotInvHeightWidth(InventorySlot ubPos, out int psWidth, out int psHeight)
    {
        psWidth = gSMInvData[ubPos].sWidth;
        psHeight = gSMInvData[ubPos].sHeight;
    }

    void HandleNewlyAddedItems(SOLDIERTYPE pSoldier, out int fDirtyLevel)
    {
        fDirtyLevel = 0;
        InventorySlot cnt;
        int sX, sY;
        OBJECTTYPE pObject;


        // If item description up.... stop
        if (gfInItemDescBox)
        {
            return;
        }

        for (cnt = 0; cnt < NUM_INV_SLOTS; cnt++)
        {
            if (pSoldier.bNewItemCount[cnt] == -2)
            {
                // Stop
                fDirtyLevel = DIRTYLEVEL2;
                pSoldier.bNewItemCount[cnt] = 0;
            }

            if (pSoldier.bNewItemCount[cnt] > 0)
            {

                sX = gSMInvData[cnt].sX;
                sY = gSMInvData[cnt].sY;

                pObject = (pSoldier.inv[cnt]);

                if (pObject.usItem == NOTHING)
                {
                    //                    gbNewItem[cnt] = 0;
                    continue;
                }

                //                INVRenderItem(guiSAVEBUFFER, pSoldier, pObject, sX, sY, gSMInvData[cnt].sWidth, gSMInvData[cnt].sHeight, DIRTYLEVEL2, null, 0, true, us16BPPItemCyclePlacedItemColors[pSoldier.bNewItemCycleCount[cnt]]);

            }

        }
    }

    void CheckForAnyNewlyAddedItems(SOLDIERTYPE pSoldier)
    {
        InventorySlot cnt;

        // OK, l0ok for any new...
        for (cnt = 0; cnt < NUM_INV_SLOTS; cnt++)
        {
            if (pSoldier.bNewItemCount[cnt] == -1)
            {
                pSoldier.bNewItemCount[cnt] = NEW_ITEM_CYCLES - 1;
            }
        }

    }

    void DegradeNewlyAddedItems()
    {
        uint uiTime;
        InventorySlot cnt;
        int cnt2;
        SOLDIERTYPE pSoldier;

        // If time done
        uiTime = GetJA2Clock();

        if ((uiTime - guiNewlyPlacedItemTimer) > 100)
        {
            guiNewlyPlacedItemTimer = uiTime;

            for (cnt2 = 0; cnt2 < NUM_TEAM_SLOTS; cnt2++)
            {
                // GET SOLDIER
                if (gTeamPanel[cnt2].fOccupied)
                {
                    pSoldier = MercPtrs[gTeamPanel[cnt2].ubID];

                    for (cnt = 0; cnt < NUM_INV_SLOTS; cnt++)
                    {
                        if (pSoldier.bNewItemCount[cnt] > 0)
                        {
                            // Decrement all the time!
                            pSoldier.bNewItemCycleCount[cnt]--;

                            if (pSoldier.bNewItemCycleCount[cnt] == 0)
                            {
                                // OK, cycle down....
                                pSoldier.bNewItemCount[cnt]--;

                                if (pSoldier.bNewItemCount[cnt] == 0)
                                {
                                    // Stop...
                                    pSoldier.bNewItemCount[cnt] = -2;
                                }
                                else
                                {
                                    // Reset!
                                    pSoldier.bNewItemCycleCount[cnt] = NEW_ITEM_CYCLE_COUNT;
                                    continue;
                                }
                            }
                        }
                    }
                }
            }
        }
    }


    void InitItemInterface()
    {
        int cnt, cnt2;

        for (cnt = 0, cnt2 = 0; cnt2 < 20; cnt += 3, cnt2++)
        {
            //            us16BPPItemCyclePlacedItemColors[cnt2] = Get16BPPColor(FROMRGB(ubRGBItemCyclePlacedItemColors[cnt], ubRGBItemCyclePlacedItemColors[cnt + 1], ubRGBItemCyclePlacedItemColors[cnt + 2]));
        }

    }


    static string pStr2;

    void INVRenderItem(Surfaces uiBuffer, SOLDIERTYPE pSoldier, OBJECTTYPE pObject, int sX, int sY, int sWidth, int sHeight, int fDirtyLevel, int? pubHighlightCounter, int ubStatusIndex, bool fOutline, int sOutlineColor)
    {
        int uiStringLength;
        INVTYPE pItem;
        ETRLEObject pTrav;
        int usHeight, usWidth;
        int sCenX, sCenY, sNewY, sNewX;
        HVOBJECT? hVObject;
        bool fLineSplit = false;
        int sFontX2, sFontY2;
        int sFontX, sFontY;

        if (pObject.usItem == NOTHING)
        {
            return;
        }

        if (ubStatusIndex < RENDER_ITEM_ATTACHMENT1)
        {
            pItem = Item[pObject.usItem];
        }
        else
        {
            pItem = Item[pObject.usAttachItem[ubStatusIndex - RENDER_ITEM_ATTACHMENT1]];
        }

        if (fDirtyLevel == DIRTYLEVEL2)
        {
            // TAKE A LOOK AT THE VIDEO OBJECT SIZE ( ONE OF TWO SIZES ) AND CENTER!
            video.GetVideoObject(out hVObject, GetInterfaceGraphicForItem(pItem));
            pTrav = (hVObject.pETRLEObject[pItem.ubGraphicNum]);
            usHeight = (int)pTrav.usHeight;
            usWidth = (int)pTrav.usWidth;



            // CENTER IN SLOT!
            // CANCEL OFFSETS!
            sCenX = sX + (Math.Abs(sWidth - usWidth) / 2) - pTrav.sOffsetX;
            sCenY = sY + (Math.Abs(sHeight - usHeight) / 2) - pTrav.sOffsetY;

            // Shadow area
            //            BltVideoObjectOutlineShadowFromIndex(uiBuffer, GetInterfaceGraphicForItem(pItem), pItem.ubGraphicNum, sCenX - 2, sCenY + 2);

            //            BltVideoObjectOutlineFromIndex(uiBuffer, GetInterfaceGraphicForItem(pItem), pItem.ubGraphicNum, sCenX, sCenY, sOutlineColor, fOutline);


            if (uiBuffer == Surfaces.FRAME_BUFFER)
            {
                //                InvalidateRegion(sX, sY, (int)(sX + sWidth), (int)(sY + sHeight));
            }
            else
            {
                //                RestoreExternBackgroundRect(sX, sY, sWidth, sHeight);
            }

        }

        FontSubSystem.SetFont(ITEM_FONT);

        if (fDirtyLevel != DIRTYLEVEL0)
        {

            if (ubStatusIndex < RENDER_ITEM_ATTACHMENT1)
            {

                FontSubSystem.SetFontBackground(FontColor.FONT_MCOLOR_BLACK);
                FontSubSystem.SetFontForeground(FontColor.FONT_MCOLOR_DKGRAY);

                // FIRST DISPLAY FREE ROUNDS REMIANING
                if (pItem.usItemClass == IC.GUN && pObject.usItem != Items.ROCKET_LAUNCHER)
                {
                    sNewY = sY + sHeight - 10;
                    sNewX = sX + 1;

                    switch (pObject.ubGunAmmoType)
                    {
                        case AMMO.AP:
                        case AMMO.SUPER_AP:
                            FontSubSystem.SetFontForeground(ITEMDESC_FONTAPFORE);
                            break;
                        case AMMO.HP:
                            FontSubSystem.SetFontForeground(ITEMDESC_FONTHPFORE);
                            break;
                        case AMMO.BUCKSHOT:
                            FontSubSystem.SetFontForeground(ITEMDESC_FONTBSFORE);
                            break;
                        case AMMO.HE:
                            FontSubSystem.SetFontForeground(ITEMDESC_FONTHEFORE);
                            break;
                        case AMMO.HEAT:
                            FontSubSystem.SetFontForeground(ITEMDESC_FONTHEAPFORE);
                            break;
                        default:
                            FontSubSystem.SetFontForeground(FontColor.FONT_MCOLOR_DKGRAY);
                            break;
                    }


                    wprintf(pStr, "%d", pObject.ubGunShotsLeft);
                    if (uiBuffer == guiSAVEBUFFER)
                    {
                        //                        RestoreExternBackgroundRect(sNewX, sNewY, 20, 15);
                    }
                    
                    mprintf(sNewX, sNewY, pStr);
                    //                    gprintfinvalidate(sNewX, sNewY, pStr);

                    FontSubSystem.SetFontForeground(FontColor.FONT_MCOLOR_DKGRAY);

                    // Display 'JAMMED' if we are jammed
                    if (pObject.bGunAmmoStatus < 0)
                    {
                        FontSubSystem.SetFontForeground(FontColor.FONT_MCOLOR_RED);

                        if (sWidth >= (BIG_INV_SLOT_WIDTH - 10))
                        {
                            //                            wprintf(pStr, TacticalStr[JAMMED_ITEM_STR]);
                        }
                        else
                        {
                            //                            wprintf(pStr, TacticalStr[SHORT_JAMMED_GUN]);
                        }

                        //                        VarFindFontCenterCoordinates(sX, sY, sWidth, sHeight, ITEM_FONT, out sNewX, out sNewY, pStr);

                        mprintf(sNewX, sNewY, pStr);
                        //                        gprintfinvalidate(sNewX, sNewY, pStr);
                    }
                }
                else
                {
                    if (ubStatusIndex != RENDER_ITEM_NOSTATUS)
                    {
                        // Now display # of items
                        if (pObject.ubNumberOfObjects > 1)
                        {
                            FontSubSystem.SetFontForeground(FontColor.FONT_GRAY4);

                            sNewY = sY + sHeight - 10;
                            wprintf(pStr, "%d", pObject.ubNumberOfObjects);

                            // Get length of string
                            uiStringLength = FontSubSystem.StringPixLength(pStr, ITEM_FONT);

                            sNewX = sX + sWidth - uiStringLength - 4;

                            if (uiBuffer == guiSAVEBUFFER)
                            {
                                //                                RestoreExternBackgroundRect(sNewX, sNewY, 15, 15);
                            }
                            mprintf(sNewX, sNewY, pStr);
                            //                            gprintfinvalidate(sNewX, sNewY, pStr);
                        }

                    }
                }

                //                if (ItemHasAttachments(pObject))
                {
                    //                    if (FindAttachment(pObject, UNDER_GLAUNCHER) == NO_SLOT)
                    {
                        FontSubSystem.SetFontForeground(FontColor.FONT_GREEN);
                    }
                    //                    else
                    {
                        FontSubSystem.SetFontForeground(FontColor.FONT_YELLOW);
                    }

                    sNewY = sY;
                    wprintf(pStr, "*");

                    // Get length of string
                    uiStringLength = FontSubSystem.StringPixLength(pStr, ITEM_FONT);

                    sNewX = sX + sWidth - uiStringLength - 4;

                    if (uiBuffer == guiSAVEBUFFER)
                    {
                        //                        RestoreExternBackgroundRect(sNewX, sNewY, 15, 15);
                    }
                    mprintf(sNewX, sNewY, pStr);
                    //                    gprintfinvalidate(sNewX, sNewY, pStr);

                }

                if (pObject == (pSoldier.inv[InventorySlot.HANDPOS])
                    && (Item[pSoldier.inv[InventorySlot.HANDPOS].usItem].usItemClass == IC.GUN)
                    && pSoldier.bWeaponMode != WM.NORMAL)
                {
                    FontSubSystem.SetFontForeground(FontColor.FONT_DKRED);

                    sNewY = sY + 13; // rather arbitrary
                    if (pSoldier.bWeaponMode == WM.BURST)
                    {
                        wprintf(pStr, "*");
                    }
                    else
                    {
                        wprintf(pStr, "+");
                    }

                    // Get length of string
                    uiStringLength = FontSubSystem.StringPixLength(pStr, ITEM_FONT);

                    sNewX = sX + sWidth - uiStringLength - 4;

                    if (uiBuffer == guiSAVEBUFFER)
                    {
                        //                        RestoreExternBackgroundRect(sNewX, sNewY, 15, 15);
                    }
                    mprintf(sNewX, sNewY, pStr);
                    //                    gprintfinvalidate(sNewX, sNewY, pStr);
                }
            }
        }

        if (pubHighlightCounter != null)
        {
            FontSubSystem.SetFontBackground(FontColor.FONT_MCOLOR_BLACK);
            FontSubSystem.SetFontForeground(FontColor.FONT_MCOLOR_LTGRAY);

            // DO HIGHLIGHT
            if (pubHighlightCounter > 0)
            {
                // Set string
                if (ubStatusIndex < RENDER_ITEM_ATTACHMENT1)
                {
                    wprintf(pStr, "%s", ShortItemNames[pObject.usItem]);
                }
                else
                {
                    wprintf(pStr, "%s", ShortItemNames[pObject.usAttachItem[ubStatusIndex - RENDER_ITEM_ATTACHMENT1]]);
                }

                //                fLineSplit = WrapString(pStr, pStr2, WORD_WRAP_INV_WIDTH, ITEM_FONT);

                //                VarFindFontCenterCoordinates(sX, sY, sWidth, sHeight, ITEM_FONT, out sFontX, out sFontY, pStr);
                sFontY = sY + 1;
                //                gprintfinvalidate(sFontX, sFontY, pStr);

                if (fLineSplit)
                {
                    //                    VarFindFontCenterCoordinates(sX, sY, sWidth, sHeight, ITEM_FONT, out sFontX2, out sFontY2, pStr2);
                    sFontY2 = sY + 13;
                    //                    gprintfinvalidate(sFontX2, sFontY2, pStr2);
                }

            }

            if (pubHighlightCounter == 2)
            {
                //                mprintf(sFontX, sFontY, pStr);

                if (fLineSplit)
                {
                    //                    mprintf(sFontX2, sFontY2, pStr2);
                }
            }
            else if (pubHighlightCounter == 1)
            {
                pubHighlightCounter = 0;
                //                gprintfRestore(sFontX, sFontY, pStr);

                if (fLineSplit)
                {
                    //                    gprintfRestore(sFontX2, sFontY2, pStr2);
                }
            }
        }
    }


    bool InItemDescriptionBox()
    {
        return (gfInItemDescBox);
    }

    void CycleItemDescriptionItem()
    {
        Items usOldItem;

        // Delete old box...
        DeleteItemDescriptionBox();

        // Make new item....
        usOldItem = gpItemDescSoldier.inv[InventorySlot.HANDPOS].usItem;

        //        if (_KeyDown(SHIFT))
        {
            usOldItem--;

            if (usOldItem < 0)
            {
                usOldItem = Items.MAXITEMS - 1;
            }
        }
        //        else
        {
            usOldItem++;

            if (usOldItem > Items.MAXITEMS)
            {
                usOldItem = 0;
            }
        }

        //        CreateItem(usOldItem, 100, (gpItemDescSoldier.inv[InventorySlot.HANDPOS]));

        InternalInitItemDescriptionBox((gpItemDescSoldier.inv[InventorySlot.HANDPOS]), 214, (int)(INV_INTERFACE_START_Y + 1), gubItemDescStatusIndex, gpItemDescSoldier);
    }

    bool InitItemDescriptionBox(SOLDIERTYPE pSoldier, InventorySlot ubPosition, int sX, int sY, int ubStatusIndex)
    {
        OBJECTTYPE pObject;

        //DEF:
        //if we are in the shopkeeper screen, and we are to use the 
        if (guiCurrentScreen == ScreenName.SHOPKEEPER_SCREEN && ubPosition == (InventorySlot)255)
        {
            pObject = pShopKeeperItemDescObject;
        }

        //else use item from the hand position
        else
        {
            pObject = (pSoldier.inv[ubPosition]);
        }

        return (InternalInitItemDescriptionBox(pObject, sX, sY, ubStatusIndex, pSoldier));
    }

    bool InitKeyItemDescriptionBox(SOLDIERTYPE pSoldier, int ubPosition, int sX, int sY, int ubStatusIndex)
    {
        OBJECTTYPE pObject = new();

        //        AllocateObject(pObject);
        //        CreateKeyObject(pObject, pSoldier.pKeyRing[ubPosition].ubNumber, pSoldier.pKeyRing[ubPosition].ubKeyID);

        return (InternalInitItemDescriptionBox(pObject, sX, sY, ubStatusIndex, pSoldier));
    }

    bool InternalInitItemDescriptionBox(OBJECTTYPE pObject, int sX, int sY, int ubStatusIndex, SOLDIERTYPE pSoldier)
    {
        //        VOBJECT_DESC VObjectDesc;
        int[] ubString = new int[48];
        int cnt;
        int[] pStr = new int[10];
        int usX, usY;
        FontColor sForeColour;
        int sProsConsIndent;

        //Set the current screen
        guiCurrentItemDescriptionScreen = guiCurrentScreen;

        // Set X, Y
        gsInvDescX = sX;
        gsInvDescY = sY;

        gpItemDescObject = pObject;
        gubItemDescStatusIndex = ubStatusIndex;
        gpItemDescSoldier = pSoldier;
        fItemDescDelete = false;

        // Build a mouse region here that is over any others.....
        if (guiCurrentItemDescriptionScreen == ScreenName.MAP_SCREEN)
        {

            //return( false );

            MouseSubSystem.MSYS_DefineRegion(
                gInvDesc,
                new(gsInvDescX,
                    gsInvDescY,
                    (gsInvDescX + MAP_ITEMDESC_WIDTH),
                    (gsInvDescY + MAP_ITEMDESC_HEIGHT)),
                MSYS_PRIORITY.HIGHEST - 2,
                CURSOR.NORMAL,
                MSYS_NO_CALLBACK,
                ItemDescCallback);

            MouseSubSystem.MSYS_AddRegion(ref gInvDesc);

            //            giMapInvDescButtonImage = LoadButtonImage("INTERFACE\\itemdescdonebutton.sti", -1, 0, -1, 1, -1);

            // create button
            //            giMapInvDescButton = QuickCreateButton(giMapInvDescButtonImage, (int)(gsInvDescX + 204), (int)(gsInvDescY + 107),
            //                                        BUTTON_TOGGLE, MSYS_PRIORITY.HIGHEST,
            //                                        (GUI_CALLBACK)BtnGenericMouseMoveButtonCallback, (GUI_CALLBACK)ItemDescDoneButtonCallback);

            fShowDescriptionFlag = true;
        }
        else
        {
            MouseSubSystem.MSYS_DefineRegion(
                gInvDesc,
                new(gsInvDescX,
                gsInvDescY,
                (gsInvDescX + ITEMDESC_WIDTH),
                (gsInvDescY + ITEMDESC_HEIGHT)),
                MSYS_PRIORITY.HIGHEST,
                CURSOR.MSYS_NO_CURSOR,
                MSYS_NO_CALLBACK,
                ItemDescCallback);

            MouseSubSystem.MSYS_AddRegion(ref gInvDesc);


        }
        // Add region
        if ((Item[pObject.usItem].usItemClass.HasFlag(IC.GUN) && pObject.usItem != Items.ROCKET_LAUNCHER))
        {
            // Add button
            //    if( guiCurrentScreen != MAP_SCREEN )
            //if( guiCurrentItemDescriptionScreen != MAP_SCREEN )
            //            wprintf(pStr, "%d/%d", gpItemDescObject.ubGunShotsLeft, Weapon[gpItemDescObject.usItem].ubMagSize);
            //            FilenameForBPP("INTERFACE\\infobox.sti", ubString);
            sForeColour = ITEMDESC_AMMO_FORE;

            switch (pObject.ubGunAmmoType)
            {
                case AMMO.AP:
                case AMMO.SUPER_AP:
                    //sForeColour = ITEMDESC_FONTAPFORE;
                    //                    giItemDescAmmoButtonImages = LoadButtonImage(ubString, 8, 5, -1, 7, -1);
                    break;
                case AMMO.HP:
                    //sForeColour = ITEMDESC_FONTHPFORE;

                    //                    giItemDescAmmoButtonImages = LoadButtonImage(ubString, 12, 9, -1, 11, -1);
                    break;
                default:
                    //sForeColour = FONT_MCOLOR_WHITE;
                    //                    giItemDescAmmoButtonImages = LoadButtonImage(ubString, 4, 1, -1, 3, -1);
                    break;

            }

            if (guiCurrentItemDescriptionScreen == ScreenName.MAP_SCREEN)
            {
                // in mapscreen, move over a bit
                //                giItemDescAmmoButton = CreateIconAndTextButton(giItemDescAmmoButtonImages, pStr, FontStyle.TINYFONT1,
                //                                                                 sForeColour, FontColor.FONT_MCOLOR_BLACK,
                //                                                                 sForeColour, FontColor.FONT_MCOLOR_BLACK,
                //                                                                 TEXT_CJUSTIFIED,
                //                                                                 (int)(ITEMDESC_AMMO_X + 18), (int)(ITEMDESC_AMMO_Y - 5), BUTTON_TOGGLE, MSYS_PRIORITY.HIGHEST,
                //                                                                 DEFAULT_MOVE_CALLBACK, (GUI_CALLBACK)ItemDescAmmoCallback);

            }
            else
            {

                // not in mapscreen
                //                giItemDescAmmoButton = CreateIconAndTextButton(giItemDescAmmoButtonImages, pStr, FontStyle.TINYFONT1,
                //                                                                    sForeColour, FontColor.FONT_MCOLOR_BLACK,
                //                                                                    sForeColour, FontColor.FONT_MCOLOR_BLACK,
                //                                                                    TEXT_CJUSTIFIED,
                //                                                                    (int)(ITEMDESC_AMMO_X), (int)(ITEMDESC_AMMO_Y), BUTTON_TOGGLE, MSYS_PRIORITY.HIGHEST,
                //                                                                    DEFAULT_MOVE_CALLBACK, (GUI_CALLBACK)ItemDescAmmoCallback);

                //if we are being called from the 
            }
            //if we are being init from the shop keeper screen and this is a dealer item we are getting info from
            if (guiTacticalInterfaceFlags.HasFlag(INTERFACE.SHOPKEEP_INTERFACE) && pShopKeeperItemDescObject != null)
            {
                //disable the eject button
                //                SpecifyDisabledButtonStyle(giItemDescAmmoButton, DISABLED_STYLE_HATCHED);
                //
                //                DisableButton(giItemDescAmmoButton);
                //                SetButtonFastHelpText(giItemDescAmmoButton, "\0");
            }
            else
            {
                //                SetButtonFastHelpText(giItemDescAmmoButton, Message[STR_EJECT_AMMO]);
            }

            //            FontSubSystem.FindFontCenterCoordinates((int)ITEMDESC_AMMO_TEXT_X, (int)ITEMDESC_AMMO_TEXT_Y, ITEMDESC_AMMO_TEXT_WIDTH, FontSubSystem.GetFontHeight(FontStyle.TINYFONT1), pStr, FontStyle.TINYFONT1, out usX, out usY);

            //            SpecifyButtonTextOffsets(giItemDescAmmoButton, (int)usX, (int)usY, true);

            gfItemAmmoDown = false;

        }

        if (ITEM_PROS_AND_CONS(gpItemDescObject.usItem))
        {
            if (guiCurrentItemDescriptionScreen == ScreenName.MAP_SCREEN)
            {
                sProsConsIndent = 0;//Math.Max(FontSubSystem.StringPixLength(gzProsLabel, ITEMDESC_FONT), FontSubSystem.StringPixLength(gzConsLabel, ITEMDESC_FONT)) + 10;
                for (cnt = 0; cnt < 2; cnt++)
                {
                    // Add region for pros/cons help text 
                    MouseSubSystem.MSYS_DefineRegion(
                        gProsAndConsRegions[cnt],
                        new((ITEMDESC_PROS_START_X + sProsConsIndent),
                            (gsInvDescY + gMapItemDescProsConsRects[cnt].Top),
                            (gsInvDescX + gMapItemDescProsConsRects[cnt].Right),
                            (gsInvDescY + gMapItemDescProsConsRects[cnt].Bottom)),
                        MSYS_PRIORITY.HIGHEST,
                        CURSOR.MSYS_NO_CURSOR,
                        MSYS_NO_CALLBACK,
                        ItemDescCallback);

                    MouseSubSystem.MSYS_AddRegion(ref gProsAndConsRegions[cnt]);

                    if (cnt == 0)
                    {
                        //                        wcscpy(gzFullItemPros, gzProsLabel);
                        //                        wcscat(gzFullItemPros, " ");
                        // use temp variable to prevent an initial comma from being displayed
                        //                        GenerateProsString(gzFullItemTemp, gpItemDescObject, 1000);
                        //                        wcscat(gzFullItemPros, gzFullItemTemp);
                        //                        SetRegionFastHelpText((gProsAndConsRegions[cnt]), gzFullItemPros);
                    }
                    else
                    {
                        //                        wcscpy(gzFullItemCons, gzConsLabel);
                        //                        wcscat(gzFullItemCons, " ");
                        // use temp variable to prevent an initial comma from being displayed
                        //                        GenerateConsString(gzFullItemTemp, gpItemDescObject, 1000);
                        //                        wcscat(gzFullItemCons, gzFullItemTemp);
                        //                        SetRegionFastHelpText((gProsAndConsRegions[cnt]), gzFullItemCons);
                    }

                    //                    SetRegionHelpEndCallback((gProsAndConsRegions[cnt]), HelpTextDoneCallback);
                }

            }
            else
            {
                sProsConsIndent = 0;//Math.Max(FontSubSystem.StringPixLength(gzProsLabel, ITEMDESC_FONT), FontSubSystem.StringPixLength(gzConsLabel, ITEMDESC_FONT)) + 10;
                for (cnt = 0; cnt < 2; cnt++)
                {
                    // Add region for pros/cons help text 
                    MouseSubSystem.MSYS_DefineRegion(
                        gProsAndConsRegions[cnt],
                        new((ITEMDESC_PROS_START_X + sProsConsIndent),
                            (gsInvDescY + gItemDescProsConsRects[cnt].Top),
                            (gsInvDescX + gItemDescProsConsRects[cnt].Right),
                            (gsInvDescY + gItemDescProsConsRects[cnt].Bottom)),
                        MSYS_PRIORITY.HIGHEST, CURSOR.MSYS_NO_CURSOR, MSYS_NO_CALLBACK, ItemDescCallback);

                    MouseSubSystem.MSYS_AddRegion(ref gProsAndConsRegions[cnt]);

                    if (cnt == 0)
                    {
                        //                        wcscpy(gzFullItemPros, gzProsLabel);
                        //                        wcscat(gzFullItemPros, " ");
                        //                        // use temp variable to prevent an initial comma from being displayed
                        //                        GenerateProsString(gzFullItemTemp, gpItemDescObject, 1000);
                        //                        wcscat(gzFullItemPros, gzFullItemTemp);
                        //                        SetRegionFastHelpText((gProsAndConsRegions[cnt]), gzFullItemPros);
                    }
                    else
                    {
                        //                        wcscpy(gzFullItemCons, gzConsLabel);
                        //                        wcscat(gzFullItemCons, " ");
                        //                        // use temp variable to prevent an initial comma from being displayed
                        //                        GenerateConsString(gzFullItemTemp, gpItemDescObject, 1000);
                        //                        wcscat(gzFullItemCons, gzFullItemTemp);
                        //                        SetRegionFastHelpText((gProsAndConsRegions[cnt]), gzFullItemCons);
                    }
                    //                    SetRegionHelpEndCallback((gProsAndConsRegions[cnt]), HelpTextDoneCallback);
                }
            }
        }

        // Load graphic
        //        VObjectDesc.fCreateFlags = VOBJECT_CREATE_FROMFILE;
        //        strcpy(VObjectDesc.ImageFile, "INTERFACE\\infobox.sti");
        //        CHECKF(AddVideoObject(&VObjectDesc, guiItemDescBox));


        //        VObjectDesc.fCreateFlags = VOBJECT_CREATE_FROMFILE;
        //        strcpy(VObjectDesc.ImageFile, "INTERFACE\\iteminfoc.STI");
        //        CHECKF(AddVideoObject(&VObjectDesc, guiMapItemDescBox));

        //        VObjectDesc.fCreateFlags = VOBJECT_CREATE_FROMFILE;
        //        strcpy(VObjectDesc.ImageFile, "INTERFACE\\bullet.STI");
        //        CHECKF(AddVideoObject(&VObjectDesc, guiBullet));

        if (gpItemDescObject.usItem != Items.MONEY)
        {
            for (cnt = 0; cnt < MAX_ATTACHMENTS; cnt++)
            {
                // Build a mouse region here that is over any others.....
                //			if (guiTacticalInterfaceFlags & INTERFACE_MAPSCREEN )
                if (guiCurrentItemDescriptionScreen == ScreenName.MAP_SCREEN)
                {
                    MouseSubSystem.MSYS_DefineRegion(
                        gItemDescAttachmentRegions[cnt],
                        new((gsInvDescX + gMapItemDescAttachmentsXY[cnt].sX), (gsInvDescY + gMapItemDescAttachmentsXY[cnt].sY), (gsInvDescX + gMapItemDescAttachmentsXY[cnt].sX + gMapItemDescAttachmentsXY[cnt].sWidth), (gsInvDescY + gMapItemDescAttachmentsXY[cnt].sY + gMapItemDescAttachmentsXY[cnt].sHeight)),
                        MSYS_PRIORITY.HIGHEST,
                        CURSOR.MSYS_NO_CURSOR,
                        MSYS_NO_CALLBACK,
                        ItemDescAttachmentsCallback);
                }
                else
                {
                    MouseSubSystem.MSYS_DefineRegion(
                        gItemDescAttachmentRegions[cnt],
                        new((gsInvDescX + gItemDescAttachmentsXY[cnt].sX), (gsInvDescY + gItemDescAttachmentsXY[cnt].sY), (gsInvDescX + gItemDescAttachmentsXY[cnt].sX + gItemDescAttachmentsXY[cnt].sBarDx + gItemDescAttachmentsXY[cnt].sWidth), (gsInvDescY + gItemDescAttachmentsXY[cnt].sY + gItemDescAttachmentsXY[cnt].sHeight)),
                        MSYS_PRIORITY.HIGHEST,
                        CURSOR.MSYS_NO_CURSOR,
                        MSYS_NO_CALLBACK,
                        ItemDescAttachmentsCallback);
                }
                // Add region
                MouseSubSystem.MSYS_AddRegion(ref gItemDescAttachmentRegions[cnt]);
                MouseSubSystem.MSYS_SetRegionUserData(gItemDescAttachmentRegions[cnt], 0, cnt);

                if (gpItemDescObject.usAttachItem[cnt] != NOTHING)
                {
                    //                    SetRegionFastHelpText((gItemDescAttachmentRegions[cnt]), ItemNames[gpItemDescObject.usAttachItem[cnt]]);
                    //                    SetRegionHelpEndCallback((gItemDescAttachmentRegions[cnt]), HelpTextDoneCallback);
                }
                else
                {
                    //                    SetRegionFastHelpText((gItemDescAttachmentRegions[cnt]), Message[STR_ATTACHMENTS]);
                    //                    SetRegionHelpEndCallback((gItemDescAttachmentRegions[cnt]), HelpTextDoneCallback);
                }
            }
        }
        else
        {
            //            memset(gRemoveMoney, 0, sizeof(REMOVE_MONEY));
            gRemoveMoney.uiTotalAmount = gpItemDescObject.uiMoneyAmount;
            gRemoveMoney.uiMoneyRemaining = gpItemDescObject.uiMoneyAmount;
            gRemoveMoney.uiMoneyRemoving = 0;

            // Load graphic
            //            VObjectDesc.fCreateFlags = VOBJECT_CREATE_FROMFILE;
            //            strcpy(VObjectDesc.ImageFile, "INTERFACE\\info_bil.sti");
            //            CHECKF(AddVideoObject(&VObjectDesc, guiMoneyGraphicsForDescBox));

            //Create buttons for the money
            //		if (guiCurrentScreen ==  MAP_SCREEN )
            if (guiCurrentItemDescriptionScreen == ScreenName.MAP_SCREEN)
            {
                //                guiMoneyButtonImage = LoadButtonImage("INTERFACE\\Info_bil.sti", -1, 1, -1, 2, -1);
                for (cnt = 0; cnt < MAX_ATTACHMENTS - 1; cnt++)
                {
                    //                    guiMoneyButtonBtn[cnt] = CreateIconAndTextButton(guiMoneyButtonImage, gzMoneyAmounts[cnt], FontStyle.BLOCKFONT2,
                    //                                                                     5, DEFAULT_SHADOW,
                    //                                                                     5, DEFAULT_SHADOW,
                    //                                                                     TEXT_CJUSTIFIED,
                    //                                                                     (int)(gMapMoneyButtonLoc.x + gMoneyButtonOffsets[cnt].x), (int)(gMapMoneyButtonLoc.y + gMoneyButtonOffsets[cnt].y), BUTTON_TOGGLE, MSYS_PRIORITY.HIGHEST,
                    //                                                                     DEFAULT_MOVE_CALLBACK, BtnMoneyButtonCallback);
                    ButtonSubSystem.MSYS_SetBtnUserData(guiMoneyButtonBtn[cnt], 0, cnt);
                    if (cnt == (int)MoneyButtons.M_1000 && gRemoveMoney.uiTotalAmount < 1000)
                    {
                        //                        DisableButton(guiMoneyButtonBtn[cnt]);
                    }
                    else if (cnt == (int)MoneyButtons.M_100 && gRemoveMoney.uiTotalAmount < 100)
                    {
                        //                        DisableButton(guiMoneyButtonBtn[cnt]);
                    }
                    else if (cnt == (int)MoneyButtons.M_10 && gRemoveMoney.uiTotalAmount < 10)
                    {
                        //                        DisableButton(guiMoneyButtonBtn[cnt]);
                    }
                }
                //Create the Done button
                //                guiMoneyDoneButtonImage = UseLoadedButtonImage(guiMoneyButtonImage, -1, 3, -1, 4, -1);
                //                guiMoneyButtonBtn[cnt] = CreateIconAndTextButton(guiMoneyDoneButtonImage, gzMoneyAmounts[cnt], FontStyle.BLOCKFONT2,
                //                                                                 5, DEFAULT_SHADOW,
                //                                                                 5, DEFAULT_SHADOW,
                //                                                                 TEXT_CJUSTIFIED,
                //                                                                 (int)(gMapMoneyButtonLoc.x + gMoneyButtonOffsets[cnt].x), (int)(gMapMoneyButtonLoc.y + gMoneyButtonOffsets[cnt].y), BUTTON_TOGGLE, MSYS_PRIORITY.HIGHEST,
                //                                                                 DEFAULT_MOVE_CALLBACK, BtnMoneyButtonCallback);
                ButtonSubSystem.MSYS_SetBtnUserData(guiMoneyButtonBtn[cnt], 0, cnt);

            }
            else
            {
                //                guiMoneyButtonImage = LoadButtonImage("INTERFACE\\Info_bil.sti", -1, 1, -1, 2, -1);
                for (cnt = 0; cnt < MAX_ATTACHMENTS - 1; cnt++)
                {
                    //                    guiMoneyButtonBtn[cnt] = CreateIconAndTextButton(guiMoneyButtonImage, gzMoneyAmounts[cnt], FontStyle.BLOCKFONT2,
                    //                                                                     5, DEFAULT_SHADOW,
                    //                                                                     5, DEFAULT_SHADOW,
                    //                                                                     TEXT_CJUSTIFIED,
                    //                                                                     (int)(gMoneyButtonLoc.x + gMoneyButtonOffsets[cnt].x), (int)(gMoneyButtonLoc.y + gMoneyButtonOffsets[cnt].y), BUTTON_TOGGLE, MSYS_PRIORITY.HIGHEST,
                    //                                                                     DEFAULT_MOVE_CALLBACK, BtnMoneyButtonCallback);
                    ButtonSubSystem.MSYS_SetBtnUserData(guiMoneyButtonBtn[cnt], 0, cnt);
                    if (cnt == (int)MoneyButtons.M_1000 && gRemoveMoney.uiTotalAmount < 1000)
                    {
                        //                        DisableButton(guiMoneyButtonBtn[cnt]);
                    }
                    else if (cnt == (int)MoneyButtons.M_100 && gRemoveMoney.uiTotalAmount < 100)
                    {
                        //                        DisableButton(guiMoneyButtonBtn[cnt]);
                    }
                    else if (cnt == (int)MoneyButtons.M_10 && gRemoveMoney.uiTotalAmount < 10)
                    {
                        //                        DisableButton(guiMoneyButtonBtn[cnt]);
                    }
                }

                //Create the Done button
                //                guiMoneyDoneButtonImage = UseLoadedButtonImage(guiMoneyButtonImage, -1, 3, 6, 4, 5);
                //                guiMoneyButtonBtn[cnt] = CreateIconAndTextButton(guiMoneyDoneButtonImage, gzMoneyAmounts[cnt], FontStyle.BLOCKFONT2,
                //                                                                 5, DEFAULT_SHADOW,
                //                                                                 5, DEFAULT_SHADOW,
                //                                                                 TEXT_CJUSTIFIED,
                //                                                                 (int)(gMoneyButtonLoc.x + gMoneyButtonOffsets[cnt].x), (int)(gMoneyButtonLoc.y + gMoneyButtonOffsets[cnt].y), BUTTON_TOGGLE, MSYS_PRIORITY.HIGHEST,
                //                                                                 DEFAULT_MOVE_CALLBACK, BtnMoneyButtonCallback);
                ButtonSubSystem.MSYS_SetBtnUserData(guiMoneyButtonBtn[cnt], 0, cnt);
            }
        }


        fInterfacePanelDirty = DIRTYLEVEL2;


        gfInItemDescBox = true;

        CHECKF(ReloadItemDesc());

        if (gpItemPointer is not null)
        {
            gpAttachSoldier = gpItemPointerSoldier;
        }
        else
        {
            gpAttachSoldier = pSoldier;
        }
        // store attachments that item originally had
        for (cnt = 0; cnt < MAX_ATTACHMENTS; cnt++)
        {
            gusOriginalAttachItem[cnt] = pObject.usAttachItem[cnt];
            gbOriginalAttachStatus[cnt] = pObject.bAttachStatus[cnt];
        }


        //        if ((gpItemPointer != null) && (gfItemDescHelpTextOffset == false) && (Facts.CheckFact(FACT.ATTACHED_ITEM_BEFORE, 0) == false))
        //        {
        //            // set up help text for attachments
        //            for (cnt = 0; cnt < NUM_INV_HELPTEXT_ENTRIES; cnt++)
        //            {
        //                gItemDescHelpText.iXPosition[cnt] += gsInvDescX;
        //                gItemDescHelpText.iYPosition[cnt] += gsInvDescY;
        //            }
        //
        //            if (!(Item[pObject.usItem].fFlags & ITEM_HIDDEN_ADDON) && (ValidAttachment(gpItemPointer.usItem, pObject.usItem) || ValidLaunchable(gpItemPointer.usItem, pObject.usItem) || ValidMerge(gpItemPointer.usItem, pObject.usItem)))
        //            {
        //                SetUpFastHelpListRegions(
        //                    gItemDescHelpText.iXPosition,
        //                    gItemDescHelpText.iYPosition,
        //                    gItemDescHelpText.iWidth,
        //                    gItemDescHelpText.sString1,
        //                    NUM_INV_HELPTEXT_ENTRIES);
        //            }
        //            else
        //            {
        //                SetUpFastHelpListRegions(
        //                    gItemDescHelpText.iXPosition,
        //                    gItemDescHelpText.iYPosition,
        //                    gItemDescHelpText.iWidth,
        //                    gItemDescHelpText.sString2,
        //                    NUM_INV_HELPTEXT_ENTRIES);
        //            }
        //
        //            StartShowingInterfaceFastHelpText();
        //
        //            SetFactTrue(FACT_ATTACHED_ITEM_BEFORE);
        //            gfItemDescHelpTextOffset = true;
        //        }



        return (true);
    }


    bool ReloadItemDesc()
    {
        if (!LoadTileGraphicForItem((Item[gpItemDescObject.usItem]), out guiItemGraphic))
        {
            return (false);
        }

        //
        // Load name, desc
        //

        //if the player is extracting money from the players account, use a different item name and description
        if (gfAddingMoneyToMercFromPlayersAccount && gpItemDescObject.usItem == Items.MONEY)
        {
            //            if (!LoadItemInfo(MONEY_FOR_PLAYERS_ACCOUNT, gzItemName, gzItemDesc))
            {
                return (false);
            }
        }
        else
        {
            //            if (!LoadItemInfo(gpItemDescObject.usItem, gzItemName, gzItemDesc))
            {
                return (false);
            }
        }

        /*
            if (Item[ gpItemDescObject.usItem ].usItemClass & IC_WEAPON)
            {
                // load item pros and cons
                if ( !LoadItemProsAndCons( gpItemDescObject.usItem, gzItemPros, gzItemCons ) )
                {
                    return( false );
                }
            }
            else
            {
                wcscpy( gzItemPros, "" );
                wcscpy( gzItemCons, "" );
            }
            */

        return (true);
    }


    static bool fRightDown = false;
    void ItemDescAmmoCallback(GUI_BUTTON btn, MSYS_CALLBACK_REASON reason)
    {
        string pStr;// = new int[10];

        /*	region gets disabled in SKI for shopkeeper boxes.  It now works normally for merc's inventory boxes!
            //if we are currently in the shopkeeper interface, return;
            if( guiTacticalInterfaceFlags & INTERFACE_SHOPKEEP_INTERFACE )
            {
                btn.uiFlags &= (~BUTTON_CLICKED_ON );
                return;
            }
        */

        if (reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_DWN))
        {
            fRightDown = true;
            gfItemAmmoDown = true;
            btn.uiFlags |= ButtonFlags.BUTTON_CLICKED_ON;
        }
        else if (reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP) && fRightDown)
        {
            fRightDown = false;
            gfItemAmmoDown = false;

            if (guiCurrentItemDescriptionScreen == ScreenName.MAP_SCREEN)
            {
                //                if (gpItemPointer == null && EmptyWeaponMagazine(gpItemDescObject, gItemPointer))
                {
                    // OK, END the description box
                    //fItemDescDelete = true;
                    fInterfacePanelDirty = DIRTYLEVEL2;
                    gpItemPointer = gItemPointer;
                    gpItemPointerSoldier = gpItemDescSoldier;

                    pStr = wprintf("0");
                    //                    SpecifyButtonText(giItemDescAmmoButton, pStr);

                    // Set mouse
                    //                    guiExternVo = GetInterfaceGraphicForItem((Item[gpItemPointer.usItem]));
                    //                    gusExternVoSubIndex = Item[gpItemPointer.usItem].ubGraphicNum;

                    MouseSubSystem.MSYS_ChangeRegionCursor(gMPanelRegion, CURSOR.EXTERN_CURSOR);
                    MouseSubSystem.MSYS_SetCurrentCursor(CURSOR.EXTERN_CURSOR);
                    fMapInventoryItem = true;
                    fTeamPanelDirty = true;
                }
            }
            else
            {
                // Set pointer to item
                //                if (gpItemPointer == null && EmptyWeaponMagazine(gpItemDescObject, gItemPointer))
                {
                    gpItemPointer = gItemPointer;
                    gpItemPointerSoldier = gpItemDescSoldier;

                    // if in SKI, load item into SKI's item pointer
                    if (guiTacticalInterfaceFlags.HasFlag(INTERFACE.SHOPKEEP_INTERFACE))
                    {
                        // pick up bullets from weapon into cursor (don't try to sell)
                        //                        BeginSkiItemPointer(PLAYERS_INVENTORY, -1, false);
                    }

                    // OK, END the description box
                    //fItemDescDelete = true;
                    fInterfacePanelDirty = DIRTYLEVEL2;

                    //                    wprintf(pStr, "0");
                    //                    SpecifyButtonText(giItemDescAmmoButton, pStr);


                    fItemDescDelete = true;
                }

            }
            btn.uiFlags &= (~ButtonFlags.BUTTON_CLICKED_ON);

        }

    }


    void DoAttachment()
    {
        //        if (AttachObject(gpItemDescSoldier, gpItemDescObject, gpItemPointer))
        {
            if (gpItemPointer.usItem == NOTHING)
            {
                // attachment attached, merge item consumed, etc

                //                if (guiTacticalInterfaceFlags & INTERFACE.MAPSCREEN)
                {
                    //                    MAPEndItemPointer();
                }
                //                else
                {
                    // End Item pickup
                    gpItemPointer = null;
                    //                    EnableSMPanelButtons(true, true);

                    //                    MouseSubSystem.MSYS_ChangeRegionCursor(gSMPanelRegion, CURSOR.NORMAL);
                    //                    SetCurrentCursorFromDatabase(CURSOR_NORMAL);

                    //if we are currently in the shopkeeper interface
                    //                    if (guiTacticalInterfaceFlags & INTERFACE_SHOPKEEP_INTERFACE)
                    {
                        //Clear out the moving cursor
                        //                        memset(gMoveingItem, 0, sizeof(INVENTORY_IN_SLOT));

                        //change the curosr back to the normal one
                        //                        SetSkiCursor(CURSOR_NORMAL);
                    }
                }
            }

            if (gpItemDescObject.usItem == NOTHING)
            {
                // close desc panel panel
                DeleteItemDescriptionBox();
            }
            //Dirty interface
            fInterfacePanelDirty = DIRTYLEVEL2;

            ReloadItemDesc();
        }

        // re-evaluate repairs
        //        gfReEvaluateEveryonesNothingToDo = true;
    }

    void PermanantAttachmentMessageBoxCallBack(MessageBoxReturnCode ubExitValue)
    {
        if (ubExitValue == MessageBoxReturnCode.MSG_BOX_RETURN_YES)
        {
            DoAttachment();
        }
        // else do nothing
    }

    static OBJECTTYPE Object2;
    void ItemDescAttachmentsCallback(ref MOUSE_REGION pRegion, MSYS_CALLBACK_REASON iReason)
    {
        int uiItemPos;

        if (gfItemDescObjectIsAttachment)
        {
            // screen out completely
            return;
        }

        uiItemPos = (int)MouseSubSystem.MSYS_GetRegionUserData(ref pRegion, 0);

        if (iReason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP))
        {
            // if the item being described belongs to a shopkeeper, ignore attempts to pick it up / replace it
            if ((guiTacticalInterfaceFlags.HasFlag(INTERFACE.SHOPKEEP_INTERFACE)) && (pShopKeeperItemDescObject != null))
            {
                return;
            }

            // Try to place attachment if something is in our hand
            // require as many APs as to reload
            if (gpItemPointer != null)
            {
                // nb pointer could be null because of inventory manipulation in mapscreen from sector inv
                //                if (gpItemPointerSoldier is null || EnoughPoints(gpItemPointerSoldier, AP.RELOAD_GUN, 0, true))
                {
                    //                    if ((Item[gpItemPointer.usItem].fFlags.HasFlag(ItemAttributes.ITEM_INSEPARABLE) && ValidAttachment(gpItemPointer.usItem, gpItemDescObject.usItem)))
                    //                    {
                    //                        DoScreenIndependantMessageBox(Message[STR_PERMANENT_ATTACHMENT], MSG_BOX_FLAG.YESNO, PermanantAttachmentMessageBoxCallBack);
                    //                        return;
                    //                    }

                    DoAttachment();
                }
            }
            else
            {
                // ATE: Make sure we have enough AP's to drop it if we pick it up!
                //                if (EnoughPoints(gpItemDescSoldier, (AP.RELOAD_GUN + AP.PICKUP_ITEM), 0, true))
                {
                    // Get attachment if there is one
                    // The follwing function will handle if no attachment is here
                    //                    if (RemoveAttachment(gpItemDescObject, (int)uiItemPos, gItemPointer))
                    {
                        gpItemPointer = gItemPointer;
                        gpItemPointerSoldier = gpItemDescSoldier;

                        //				if( guiCurrentScreen == MAP_SCREEN )
                        if (guiCurrentItemDescriptionScreen == ScreenName.MAP_SCREEN)
                        {
                            // Set mouse
                            //                            guiExternVo = GetInterfaceGraphicForItem((Item[gpItemPointer.usItem]));
                            //                            gusExternVoSubIndex = Item[gpItemPointer.usItem].ubGraphicNum;

                            MouseSubSystem.MSYS_ChangeRegionCursor(gMPanelRegion, CURSOR.EXTERN_CURSOR);
                            MouseSubSystem.MSYS_SetCurrentCursor(CURSOR.EXTERN_CURSOR);
                            fMapInventoryItem = true;
                            fTeamPanelDirty = true;
                        }

                        //if we are currently in the shopkeeper interface
                        //                        else if (guiTacticalInterfaceFlags & INTERFACE_SHOPKEEP_INTERFACE)
                        {
                            // pick up attachment from item into cursor (don't try to sell)
                            //                            BeginSkiItemPointer(PLAYERS_INVENTORY, -1, false);
                        }

                        //Dirty interface
                        fInterfacePanelDirty = DIRTYLEVEL2;

                        // re-evaluate repairs
                        //                        gfReEvaluateEveryonesNothingToDo = true;

                        UpdateItemHatches();
                    }
                }
            }
        }
        else if (iReason.HasFlag(MSYS_CALLBACK_REASON.RBUTTON_DWN))
        {
            fRightDown = true;
        }
        else if (iReason.HasFlag(MSYS_CALLBACK_REASON.RBUTTON_UP) && fRightDown)
        {
            fRightDown = false;

            if (gpItemDescObject.usAttachItem[uiItemPos] != NOTHING)
            {
                bool fShopkeeperItem = false;

                // remember if this is a shopkeeper's item we're viewing ( pShopKeeperItemDescObject will get nuked on deletion )
                //                if (guiTacticalInterfaceFlags & INTERFACE_SHOPKEEP_INTERFACE && pShopKeeperItemDescObject != null)
                {
                    fShopkeeperItem = true;
                }

                DeleteItemDescriptionBox();

                //                if (CreateItem(gpItemDescObject.usAttachItem[uiItemPos], gpItemDescObject.bAttachStatus[uiItemPos], Object2))
                {
                    gfItemDescObjectIsAttachment = true;
                    InternalInitItemDescriptionBox(Object2, gsInvDescX, gsInvDescY, 0, gpItemDescSoldier);

                    if (fShopkeeperItem)
                    {
                        pShopKeeperItemDescObject = Object2;
                        //                        StartSKIDescriptionBox();
                    }
                }
            }
        }
    }

    void RenderItemDescriptionBox()
    {
        ETRLEObject pTrav;
        int usHeight, usWidth;
        int sCenX, sCenY, sStrX;
        HVOBJECT hVObject = new();
        string sTempString = string.Empty;

        int uiStringLength, uiRightLength;
        int cnt;
        float fWeight = 0.0f;
        int usX, usY;
        int ubAttackAPs = 0;
        bool fHatchOutAttachments = gfItemDescObjectIsAttachment; // if examining attachment, always hatch out attachment slots
        int sProsConsIndent = 0;

        if ((guiCurrentItemDescriptionScreen == ScreenName.MAP_SCREEN) && (gfInItemDescBox))
        {
            // TAKE A LOOK AT THE VIDEO OBJECT SIZE ( ONE OF TWO SIZES ) AND CENTER!
            //            GetVideoObject(hVObject, guiItemGraphic);
            //            pTrav = (hVObject.pETRLEObject[0]);
            //            usHeight = (int)pTrav.usHeight;
            //            usWidth = (int)pTrav.usWidth;
            //
            //            // CENTER IN SLOT!
            //            // REMOVE OFFSETS!
            //            sCenX = MAP_ITEMDESC_ITEM_X + (Math.Abs(ITEMDESC_ITEM_WIDTH - usWidth) / 2) - pTrav.sOffsetX;
            //            sCenY = MAP_ITEMDESC_ITEM_Y + (Math.Abs(ITEMDESC_ITEM_HEIGHT - usHeight) / 2) - pTrav.sOffsetY;

            //            BltVideoObjectFromIndex(guiSAVEBUFFER, guiMapItemDescBox, 0, gsInvDescX, gsInvDescY, VO_BLT.SRCTRANSPARENCY, null);

//            BltVideoObjectFromIndex(guiSAVEBUFFER, guiMapItemDescBox, 0, gsInvDescX, gsInvDescY, VO_BLT.SRCTRANSPARENCY, null);

            //Display the money 'seperating' border
            if (gpItemDescObject.usItem == Items.MONEY)
            {
                //Render the money Boxes
                //                BltVideoObjectFromIndex(guiSAVEBUFFER, guiMoneyGraphicsForDescBox, 0, (int)(gMapMoneyButtonLoc.x + gMoneyButtonOffsets[0].x), (int)(gMapMoneyButtonLoc.y + gMoneyButtonOffsets[0].y), VO_BLT.SRCTRANSPARENCY, null);
            }


            // Display item
            //            BltVideoObjectOutlineShadowFromIndex(guiSAVEBUFFER, guiItemGraphic, 0, sCenX - 2, sCenY + 2);

            //            BltVideoObjectFromIndex(guiSAVEBUFFER, guiItemGraphic, 0, sCenX, sCenY, VO_BLT.SRCTRANSPARENCY, null);


            // Display ststus
            //            DrawItemUIBarEx(gpItemDescObject, gubItemDescStatusIndex, (int)MAP_ITEMDESC_ITEM_STATUS_X, (int)MAP_ITEMDESC_ITEM_STATUS_Y, ITEMDESC_ITEM_STATUS_WIDTH, ITEMDESC_ITEM_STATUS_HEIGHT_MAP, Get16BPPColor(DESC_STATUS_BAR), Get16BPPColor(DESC_STATUS_BAR_SHADOW), true, guiSAVEBUFFER);

            //            if (gpItemPointer)
            {
                //                if ((Item[gpItemPointer.usItem].fFlags.HasFlag(ItemAttributes.ITEM_HIDDEN_ADDON))
                //                    || (!ValidItemAttachment(gpItemDescObject, gpItemPointer.usItem, false)
                //                        && !ValidMerge(gpItemPointer.usItem, gpItemDescObject.usItem)
                //                        && !ValidLaunchable(gpItemPointer.usItem, gpItemDescObject.usItem)))
                //                {
                //                    // hatch out the attachment panels
                //                    fHatchOutAttachments = true;
                //                }
            }

            // Display attachments
            for (cnt = 0; cnt < MAX_ATTACHMENTS; cnt++)
            {
                if (gpItemDescObject.usAttachItem[cnt] != NOTHING)
                {

                    //        if (guiTacticalInterfaceFlags & INTERFACE_MAPSCREEN )
                    if (guiCurrentItemDescriptionScreen == ScreenName.MAP_SCREEN)
                    {
                        sCenX = (int)(gsInvDescX + gMapItemDescAttachmentsXY[cnt].sX + 5);
                        sCenY = (int)(gsInvDescY + gMapItemDescAttachmentsXY[cnt].sY - 1);

                        INVRenderItem(guiSAVEBUFFER, null, gpItemDescObject, sCenX, sCenY, gMapItemDescAttachmentsXY[cnt].sWidth, gMapItemDescAttachmentsXY[cnt].sHeight, DIRTYLEVEL2, null, (int)(RENDER_ITEM_ATTACHMENT1 + cnt), false, 0);

                        sCenX = sCenX - gMapItemDescAttachmentsXY[cnt].sBarDx;
                        sCenY = sCenY + gMapItemDescAttachmentsXY[cnt].sBarDy;
                        //                        DrawItemUIBarEx(gpItemDescObject, (int)(DRAW_ITEM_STATUS_ATTACHMENT1 + cnt), sCenX, sCenY, ITEM_BAR_WIDTH, ITEM_BAR_HEIGHT, Get16BPPColor(STATUS_BAR), Get16BPPColor(STATUS_BAR_SHADOW), true, guiSAVEBUFFER);

                    }
                    else
                    {
                        sCenX = (int)(gsInvDescX + gMapItemDescAttachmentsXY[cnt].sX + 5);
                        sCenY = (int)(gsInvDescY + gMapItemDescAttachmentsXY[cnt].sY - 1);

                        INVRenderItem(guiSAVEBUFFER, null, gpItemDescObject, sCenX, sCenY, gMapItemDescAttachmentsXY[cnt].sWidth, gMapItemDescAttachmentsXY[cnt].sHeight, DIRTYLEVEL2, null, (int)(RENDER_ITEM_ATTACHMENT1 + cnt), false, 0);

                        sCenX = sCenX - gItemDescAttachmentsXY[cnt].sBarDx;
                        sCenY = sCenY + gItemDescAttachmentsXY[cnt].sBarDy;
                        //                        DrawItemUIBarEx(gpItemDescObject, (int)(DRAW_ITEM_STATUS_ATTACHMENT1 + cnt), sCenX, sCenY, ITEM_BAR_WIDTH, ITEM_BAR_HEIGHT, Get16BPPColor(STATUS_BAR), Get16BPPColor(STATUS_BAR_SHADOW), true, guiSAVEBUFFER);
                    }
                }

                if (fHatchOutAttachments)
                {
                    //                    DrawHatchOnInventory(guiSAVEBUFFER, (int)(gsInvDescX + gMapItemDescAttachmentsXY[cnt].sX), (int)(gsInvDescY + gMapItemDescAttachmentsXY[cnt].sY - 2), (int)(gMapItemDescAttachmentsXY[cnt].sWidth + gMapItemDescAttachmentsXY[cnt].sBarDx), (int)(gMapItemDescAttachmentsXY[cnt].sHeight + 2));
                }

            }

            if (Item[gpItemDescObject.usItem].usItemClass.HasFlag(IC.GUN))
            {
                // display bullets for ROF
                //                BltVideoObjectFromIndex(guiSAVEBUFFER, guiBullet, 0, MAP_BULLET_SING_X, MAP_BULLET_SING_Y, VO_BLT.SRCTRANSPARENCY, null);

                //                if (Weapon[gpItemDescObject.usItem].ubShotsPerBurst > 0)
                //                {
                //                    for (cnt = 0; cnt < Weapon[gpItemDescObject.usItem].ubShotsPerBurst; cnt++)
                //                    {
                //                        BltVideoObjectFromIndex(guiSAVEBUFFER, guiBullet, 0, MAP_BULLET_BURST_X + cnt * (BULLET_WIDTH + 1), MAP_BULLET_BURST_Y, VO_BLT_SRCTRANSPARENCY, null);
                //                    }
                //                }

            }

            //            RestoreExternBackgroundRect(gsInvDescX, gsInvDescY, MAP_ITEMDESC_WIDTH, MAP_ITEMDESC_HEIGHT);

            // Render font desc
            FontSubSystem.SetFont(ITEMDESC_FONT);
            FontSubSystem.SetFontBackground(FontColor.FONT_MCOLOR_BLACK);
            FontSubSystem.SetFontForeground(FontColor.FONT_FCOLOR_WHITE);
            FontSubSystem.SetFontShadow(ITEMDESC_FONTSHADOW3);

            // Render name
#if JA2TESTVERSION
            mprintf(MAP_ITEMDESC_NAME_X, MAP_ITEMDESC_NAME_Y, "%s (%d)", gzItemName, gpItemDescObject.usItem);
#else
            //            mprintf(MAP_ITEMDESC_NAME_X, MAP_ITEMDESC_NAME_Y, "%s", gzItemName);
#endif

            FontSubSystem.SetFontForeground(FontColor.FONT_BLACK);
            FontSubSystem.SetFontShadow(ITEMDESC_FONTSHADOW2);

//            FontSubSystem.DisplayWrappedString((int)MAP_ITEMDESC_DESC_START_X, (int)MAP_ITEMDESC_DESC_START_Y, MAP_ITEMDESC_DESC_WIDTH, 2, ITEMDESC_FONT, FontColor.FONT_BLACK, gzItemDesc, FontColor.FONT_MCOLOR_BLACK, false, LEFT_JUSTIFIED);

            if (ITEM_PROS_AND_CONS(gpItemDescObject.usItem))
            {
                if ((gpItemDescObject.usItem == Items.ROCKET_RIFLE || gpItemDescObject.usItem == Items.AUTO_ROCKET_RIFLE)
                    && gpItemDescObject.ubImprintID < NO_PROFILE)
                {
                    // add name noting imprint
                    //                    wprintf(pStr, "%s %s (%s)", AmmoCaliber[Weapon[gpItemDescObject.usItem].ubCalibre], WeaponType[Weapon[gpItemDescObject.usItem].ubWeaponType], gMercProfiles[gpItemDescObject.ubImprintID].zNickname);
                }
                else
                {
                    //                    wprintf(pStr, "%s %s", AmmoCaliber[Weapon[gpItemDescObject.usItem].ubCalibre], WeaponType[Weapon[gpItemDescObject.usItem].ubWeaponType]);
                }

                //                FindFontRightCoordinates((int)MAP_ITEMDESC_CALIBER_X, (int)MAP_ITEMDESC_CALIBER_Y, MAP_ITEMDESC_CALIBER_WIDTH, ITEM_STATS_HEIGHT, pStr, ITEMDESC_FONT, out usX, out usY);
                //                mprintf(usX, usY, pStr);

                FontSubSystem.SetFontForeground(FontColor.FONT_MCOLOR_DKWHITE2);
                FontSubSystem.SetFontShadow(ITEMDESC_FONTSHADOW3);
                //                mprintf((int)MAP_ITEMDESC_PROS_START_X, (int)MAP_ITEMDESC_PROS_START_Y, gzProsLabel);

                //                sProsConsIndent = Math.Max(FontSubSystem.StringPixLength(gzProsLabel, ITEMDESC_FONT), FontSubSystem.StringPixLength(gzConsLabel, ITEMDESC_FONT)) + 10;

                //                GenerateProsString(gzItemPros, gpItemDescObject, MAP_ITEMDESC_DESC_WIDTH - sProsConsIndent - FontSubSystem.StringPixLength(DOTDOTDOT, ITEMDESC_FONT));
                if (gzItemPros[0] != 0)
                {
                    FontSubSystem.SetFontForeground(FontColor.FONT_BLACK);
                    FontSubSystem.SetFontShadow(ITEMDESC_FONTSHADOW2);
//                    FontSubSystem.DisplayWrappedString((int)(MAP_ITEMDESC_PROS_START_X + sProsConsIndent), (int)MAP_ITEMDESC_PROS_START_Y, (int)(ITEMDESC_DESC_WIDTH - sProsConsIndent), 2, ITEMDESC_FONT, FontColor.FONT_BLACK, gzItemPros, FontColor.FONT_MCOLOR_BLACK, false, LEFT_JUSTIFIED);
                }

                FontSubSystem.SetFontForeground(FontColor.FONT_MCOLOR_DKWHITE2);
                FontSubSystem.SetFontShadow(ITEMDESC_FONTSHADOW3);
                //                mprintf((int)MAP_ITEMDESC_CONS_START_X, (int)MAP_ITEMDESC_CONS_START_Y, gzConsLabel);

                //                GenerateConsString(gzItemCons, gpItemDescObject, MAP_ITEMDESC_DESC_WIDTH - sProsConsIndent - FontSubSystem.StringPixLength(DOTDOTDOT, ITEMDESC_FONT));
                if (gzItemCons[0] != 0)
                {
                    FontSubSystem.SetFontForeground(FontColor.FONT_BLACK);
                    FontSubSystem.SetFontShadow(ITEMDESC_FONTSHADOW2);
//                    FontSubSystem.DisplayWrappedString((int)(MAP_ITEMDESC_CONS_START_X + sProsConsIndent), (int)MAP_ITEMDESC_CONS_START_Y, (int)(ITEMDESC_DESC_WIDTH - sProsConsIndent), 2, ITEMDESC_FONT, FontColor.FONT_BLACK, gzItemCons, FontColor.FONT_MCOLOR_BLACK, false, LEFT_JUSTIFIED);
                }
            }

            /*
                    DisplayWrappedString( (int)MAP_ITEMDESC_PROS_START_X, (int)MAP_ITEMDESC_PROS_START_Y, MAP_ITEMDESC_DESC_WIDTH, 2, ITEMDESC_FONT, FONT_BLACK,  gzProsLabel, FONT_MCOLOR_BLACK, false, LEFT_JUSTIFIED);
                    if (gzItemPros[0] != 0)
                    {
                        DisplayWrappedString( (int)MAP_ITEMDESC_PROS_START_X, (int)MAP_ITEMDESC_PROS_START_Y, MAP_ITEMDESC_DESC_WIDTH, 2, ITEMDESC_FONT, FONT_BLACK,  gzItemPros, FONT_MCOLOR_BLACK, false, LEFT_JUSTIFIED);
                    }

                    DisplayWrappedString( (int)MAP_ITEMDESC_CONS_START_X, (int)MAP_ITEMDESC_CONS_START_Y, MAP_ITEMDESC_DESC_WIDTH, 2, ITEMDESC_FONT, FONT_BLACK,  gzConsLabel, FONT_MCOLOR_BLACK, false, LEFT_JUSTIFIED);
                    if (gzItemCons[0] != 0)
                    {
                        DisplayWrappedString( (int)MAP_ITEMDESC_CONS_START_X, (int)MAP_ITEMDESC_CONS_START_Y, MAP_ITEMDESC_DESC_WIDTH, 2, ITEMDESC_FONT, FONT_BLACK,  gzItemCons, FONT_MCOLOR_BLACK, false, LEFT_JUSTIFIED);
                    }
            */

            // Get length of string
            uiRightLength = 35;


            //            fWeight = (float)(CalculateObjectWeight(gpItemDescObject)) / 10;
            if (!GameSettings.fOptions[TOPTION.USE_METRIC_SYSTEM]) // metric units not enabled
            {
                //                fWeight = fWeight * 2.2f;
            }

            // Add weight of attachments here !

            //            if (fWeight < 0.1)
            //            {
            //                fWeight = 0.1f;
            //            }

            // Render, stat  name
            if (Item[gpItemDescObject.usItem].usItemClass.HasFlag(IC.WEAPON))
            {
                FontSubSystem.SetFont(FontStyle.BLOCKFONT2);
                FontSubSystem.SetFontForeground((FontColor)6);
                FontSubSystem.SetFontShadow(FontShadow.DEFAULT_SHADOW);

                //LABELS
                //                wprintf(sTempString, gWeaponStatsDesc[0], GetWeightUnitString());
                //                mprintf(gMapWeaponStats[0].sX + gsInvDescX, gMapWeaponStats[0].sY + gsInvDescY, "%s", sTempString);
                //mprintf( gMapWeaponStats[ 2 ].sX + gsInvDescX, gMapWeaponStats[ 2 ].sY + gsInvDescY, "%s", gMapWeaponStats[ 2 ].zDesc );
                if (Item[gpItemDescObject.usItem].usItemClass.HasFlag((IC.GUN | IC.LAUNCHER)))
                {
                    //                    mprintf(gMapWeaponStats[3].sX + gsInvDescX, gMapWeaponStats[3].sY + gsInvDescY, "%s", gWeaponStatsDesc[3]);
                }
                if (!(Item[gpItemDescObject.usItem].usItemClass.HasFlag(IC.LAUNCHER)) && gpItemDescObject.usItem != Items.ROCKET_LAUNCHER)
                {
                    //                    mprintf(gMapWeaponStats[4].sX + gsInvDescX, gMapWeaponStats[4].sY + gsInvDescY, "%s", gWeaponStatsDesc[4]);
                }
                //                mprintf(gMapWeaponStats[5].sX + gsInvDescX, gMapWeaponStats[5].sY + gsInvDescY, "%s", gWeaponStatsDesc[5]);
                if (Item[gpItemDescObject.usItem].usItemClass.HasFlag(IC.GUN))
                {
                    // equals sign
                    //                    mprintf(gMapWeaponStats[7].sX + gsInvDescX, gMapWeaponStats[7].sY + gsInvDescY, "%s", gWeaponStatsDesc[7]);
                }
                //                mprintf(gMapWeaponStats[1].sX + gsInvDescX, gMapWeaponStats[1].sY + gsInvDescY, "%s", gWeaponStatsDesc[1]);


                //                if (Weapon[gpItemDescObject.usItem].ubShotsPerBurst > 0)
                {
                    //                    mprintf(gMapWeaponStats[8].sX + gsInvDescX, gMapWeaponStats[8].sY + gsInvDescY, "%s", gWeaponStatsDesc[8]);
                }

                FontSubSystem.SetFontForeground((FontColor)5);
                //Status
                // This is gross, but to get the % to work out right...
                wprintf(pStr, "%2d%%", gpItemDescObject.bStatus[gubItemDescStatusIndex]);
                FontSubSystem.FindFontRightCoordinates((int)(gMapWeaponStats[1].sX + gsInvDescX + gMapWeaponStats[1].sValDx + 6), (int)(gMapWeaponStats[1].sY + gsInvDescY), ITEM_STATS_WIDTH, ITEM_STATS_HEIGHT, pStr, FontStyle.BLOCKFONT2, out usX, out usY);
                wcscat(pStr, "%%");
                mprintf(usX, usY, pStr);

                // Values
                //                if (fWeight <= (EXCEPTIONAL_WEIGHT / 10))
                //                {
                //                    FontSubSystem.SetFontForeground(ITEMDESC_FONTHIGHLIGHT);
                //                }
                //                else
                {
                    FontSubSystem.SetFontForeground((FontColor)5);
                }
                //Weight
                wprintf(pStr, "%1.1f", fWeight);
                FontSubSystem.FindFontRightCoordinates((int)(gMapWeaponStats[0].sX + gsInvDescX + gMapWeaponStats[0].sValDx + 6), (int)(gMapWeaponStats[0].sY + gsInvDescY), ITEM_STATS_WIDTH, ITEM_STATS_HEIGHT, pStr, FontStyle.BLOCKFONT2, out usX, out usY);
                mprintf(usX, usY, pStr);

                if (Item[gpItemDescObject.usItem].usItemClass.HasFlag((IC.GUN | IC.LAUNCHER)))
                {

                    //                    if (GunRange(gpItemDescObject) >= EXCEPTIONAL_RANGE)
                    {
                        FontSubSystem.SetFontForeground(ITEMDESC_FONTHIGHLIGHT);
                    }
                    //                    else
                    {
                        FontSubSystem.SetFontForeground((FontColor)5);
                    }

                    //Range
                    //                    wprintf(pStr, "%2d", (GunRange(gpItemDescObject)) / 10);
                    FontSubSystem.FindFontRightCoordinates((gMapWeaponStats[3].sX + gsInvDescX + gMapWeaponStats[3].sValDx), (int)(gMapWeaponStats[3].sY + gsInvDescY), ITEM_STATS_WIDTH, ITEM_STATS_HEIGHT, pStr, FontStyle.BLOCKFONT2, out usX, out usY);
                    mprintf(usX, usY, pStr);
                }

                //                if (!(Item[gpItemDescObject.usItem].usItemClass & IC.LAUNCHER) && gpItemDescObject.usItem != Items.ROCKET_LAUNCHER)
                {

                    //                    if (Weapon[gpItemDescObject.usItem].ubImpact >= EXCEPTIONAL_DAMAGE)
                    {
                        FontSubSystem.SetFontForeground(ITEMDESC_FONTHIGHLIGHT);
                    }
                    //                    else
                    {
                        FontSubSystem.SetFontForeground((FontColor)5);
                    }

                    //Damage
                    //                    wprintf(pStr, "%2d", Weapon[gpItemDescObject.usItem].ubImpact);
                    FontSubSystem.FindFontRightCoordinates((int)(gMapWeaponStats[4].sX + gsInvDescX + gMapWeaponStats[4].sValDx), (int)(gMapWeaponStats[4].sY + gsInvDescY), ITEM_STATS_WIDTH, ITEM_STATS_HEIGHT, pStr, FontStyle.BLOCKFONT2, out usX, out usY);
                    mprintf(usX, usY, pStr);
                }

                //                ubAttackAPs = BaseAPsToShootOrStab(DEFAULT_APS, DEFAULT_AIMSKILL, gpItemDescObject);

                //                if (ubAttackAPs <= EXCEPTIONAL_AP_COST)
                {
                    FontSubSystem.SetFontForeground(ITEMDESC_FONTHIGHLIGHT);
                }
                //                else
                {
                    FontSubSystem.SetFontForeground((FontColor)5);
                }

                //Ap's
                wprintf(pStr, "%2d", ubAttackAPs);
                FontSubSystem.FindFontRightCoordinates((int)(gMapWeaponStats[5].sX + gsInvDescX + gMapWeaponStats[5].sValDx), (int)(gMapWeaponStats[5].sY + gsInvDescY), ITEM_STATS_WIDTH, ITEM_STATS_HEIGHT, pStr, FontStyle.BLOCKFONT2, out usX, out usY);
                mprintf(usX, usY, pStr);

                //               if (Weapon[gpItemDescObject.usItem].ubShotsPerBurst > 0)
                {

                    //                    if (Weapon[gpItemDescObject.usItem].ubShotsPerBurst >= EXCEPTIONAL_BURST_SIZE || gpItemDescObject.usItem == G11)
                    {
                        FontSubSystem.SetFontForeground(ITEMDESC_FONTHIGHLIGHT);
                    }
                    //                    else
                    {
                        FontSubSystem.SetFontForeground((FontColor)5);
                    }

                    //                    wprintf(pStr, "%2d", ubAttackAPs + CalcAPsToBurst(DEFAULT_APS, gpItemDescObject));
                    FontSubSystem.FindFontRightCoordinates((int)(gMapWeaponStats[6].sX + gsInvDescX + gMapWeaponStats[6].sValDx), (int)(gMapWeaponStats[6].sY + gsInvDescY), ITEM_STATS_WIDTH, ITEM_STATS_HEIGHT, pStr, FontStyle.BLOCKFONT2, out usX, out usY);
                    mprintf(usX, usY, pStr);
                }

            }
            else if (gpItemDescObject.usItem == Items.MONEY)
            {

                FontSubSystem.SetFontForeground(FontColor.FONT_FCOLOR_WHITE);
                FontSubSystem.SetFontShadow(FontShadow.DEFAULT_SHADOW);

                //
                // Display the total amount of money
                //

                // if the player is taking money from their account
                if (gfAddingMoneyToMercFromPlayersAccount)
                {
                    wprintf(pStr, "%ld", LaptopSaveInfo.iCurrentBalance);
                }
                else
                {
                    wprintf(pStr, "%ld", gRemoveMoney.uiTotalAmount);
                }

                //                InsertCommasForDollarFigure(pStr);
                //                InsertDollarSignInToString(pStr);
                uiStringLength = FontSubSystem.StringPixLength(pStr, ITEMDESC_FONT);
                sStrX = MAP_ITEMDESC_NAME_X + (245 - uiStringLength);
                mprintf(sStrX, MAP_ITEMDESC_NAME_Y, pStr);

                FontSubSystem.SetFont(FontStyle.BLOCKFONT2);

                FontSubSystem.SetFontForeground((FontColor)6);
                FontSubSystem.SetFontShadow(FontShadow.DEFAULT_SHADOW);

                //Display the 'Removing'
                //                mprintf(gMapMoneyStats[0].sX + gsInvDescX, gMapMoneyStats[0].sY + gsInvDescY, "%s", gMoneyStatsDesc[MONEY_DESC_AMOUNT]);
                //                //Display the 'REmaining'
                //                mprintf(gMapMoneyStats[2].sX + gsInvDescX, gMapMoneyStats[2].sY + gsInvDescY, "%s", gMoneyStatsDesc[MONEY_DESC_AMOUNT_2_SPLIT]);
                //
                //                //Display the 'Amt removing'
                //                mprintf(gMapMoneyStats[1].sX + gsInvDescX, gMapMoneyStats[1].sY + gsInvDescY, "%s", gMoneyStatsDesc[MONEY_DESC_REMAINING]);
                //                //Display the 'REmaining amount'
                //                mprintf(gMapMoneyStats[3].sX + gsInvDescX, gMapMoneyStats[3].sY + gsInvDescY, "%s", gMoneyStatsDesc[MONEY_DESC_TO_SPLIT]);

                FontSubSystem.SetFontForeground((FontColor)5);

                //Display the 'Seperate text'
//                mprintf((int)(gMapMoneyButtonLoc.x + gMoneyButtonOffsets[cnt].x), (int)(gMapMoneyButtonLoc.y + gMoneyButtonOffsets[cnt].y), gzMoneyAmounts[4]);

                // The Money Remaining
                wprintf(pStr, "%ld", gRemoveMoney.uiMoneyRemaining);
                //                InsertCommasForDollarFigure(pStr);
                //                InsertDollarSignInToString(pStr);
                uiStringLength = FontSubSystem.StringPixLength(pStr, ITEMDESC_FONT);
                sStrX = gMapMoneyStats[1].sX + gsInvDescX + gMapMoneyStats[1].sValDx + (uiRightLength - uiStringLength);
                mprintf(sStrX, gMapMoneyStats[1].sY + gsInvDescY, pStr);


                // The money removing
                FontSubSystem.SetFontForeground((FontColor)5);
                wprintf(pStr, "%ld", gRemoveMoney.uiMoneyRemoving);
                //                InsertCommasForDollarFigure(pStr);
                //                InsertDollarSignInToString(pStr);
                uiStringLength = FontSubSystem.StringPixLength(pStr, ITEMDESC_FONT);
                sStrX = gMapMoneyStats[3].sX + gsInvDescX + gMapMoneyStats[3].sValDx + (uiRightLength - uiStringLength);
                mprintf(sStrX, gMapMoneyStats[3].sY + gsInvDescY, pStr);

                // print label for amount

                //			SetFontForeground( ITEMDESC_FONTFORE1 );
                //			mprintf( gMapMoneyStats[ 1 ].sX + gsInvDescX, gMapMoneyStats[ 1 ].sY + gsInvDescY, "%s", gMapMoneyStats[ 1 ].zDesc );


            }
            else if (Item[gpItemDescObject.usItem].usItemClass == IC.MONEY)
            {
                FontSubSystem.SetFontForeground(FontColor.FONT_FCOLOR_WHITE);
                FontSubSystem.SetFontShadow(FontShadow.DEFAULT_SHADOW);
                wprintf(pStr, "%ld", gpItemDescObject.uiMoneyAmount);
                //                InsertCommasForDollarFigure(pStr);
                //                InsertDollarSignInToString(pStr);
                uiStringLength = FontSubSystem.StringPixLength(pStr, ITEMDESC_FONT);
                sStrX = MAP_ITEMDESC_NAME_X + (245 - uiStringLength);
                mprintf(sStrX, MAP_ITEMDESC_NAME_Y, pStr);
            }
            else
            {
                //Labels
                FontSubSystem.SetFont(FontStyle.BLOCKFONT2);

                FontSubSystem.SetFontForeground((FontColor)6);
                FontSubSystem.SetFontShadow(FontShadow.DEFAULT_SHADOW);

                if (Item[gpItemDescObject.usItem].usItemClass.HasFlag(IC.AMMO))
                {
                    //                    mprintf(gMapWeaponStats[2].sX + gsInvDescX, gMapWeaponStats[2].sY + gsInvDescY, "%s", gWeaponStatsDesc[2]);
                }
                else
                {
                    //                  mprintf(gMapWeaponStats[1].sX + gsInvDescX, gMapWeaponStats[1].sY + gsInvDescY, "%s", gWeaponStatsDesc[1]);
                }
                //                wprintf(sTempString, gWeaponStatsDesc[0], GetWeightUnitString());
                //                mprintf(gMapWeaponStats[0].sX + gsInvDescX, gMapWeaponStats[0].sY + gsInvDescY, sTempString);

                // Values
                FontSubSystem.SetFontForeground((FontColor)5);


                if (Item[gpItemDescObject.usItem].usItemClass.HasFlag(IC.AMMO))
                {
                    // Ammo
                    //                    wprintf(pStr, "%d/%d", gpItemDescObject.ubShotsLeft[0], Magazine[Item[gpItemDescObject.usItem].ubClassIndex].ubMagSize);
                    uiStringLength = FontSubSystem.StringPixLength(pStr, ITEMDESC_FONT);
                    //			sStrX =  gMapWeaponStats[ 0 ].sX + gsInvDescX + gMapWeaponStats[ 0 ].sValDx + ( uiRightLength - uiStringLength );
                    FontSubSystem.FindFontRightCoordinates((int)(gMapWeaponStats[2].sX + gsInvDescX + gMapWeaponStats[2].sValDx + 6), (int)(gMapWeaponStats[2].sY + gsInvDescY), ITEM_STATS_WIDTH, ITEM_STATS_HEIGHT, pStr, FontStyle.BLOCKFONT2, out sStrX, out usY);
                    mprintf(sStrX, gMapWeaponStats[2].sY + gsInvDescY, pStr);
                }
                else
                {
                    //Status
                    wprintf(pStr, "%2d%%", gpItemDescObject.bStatus[gubItemDescStatusIndex]);
                    uiStringLength = FontSubSystem.StringPixLength(pStr, ITEMDESC_FONT);
                    //			sStrX =  gMapWeaponStats[ 1 ].sX + gsInvDescX + gMapWeaponStats[ 1 ].sValDx + ( uiRightLength - uiStringLength );
                    FontSubSystem.FindFontRightCoordinates((int)(gMapWeaponStats[1].sX + gsInvDescX + gMapWeaponStats[1].sValDx + 6), (int)(gMapWeaponStats[1].sY + gsInvDescY), ITEM_STATS_WIDTH, ITEM_STATS_HEIGHT, pStr, FontStyle.BLOCKFONT2, out sStrX, out usY);
                    wcscat(pStr, "%%");
                    mprintf(sStrX, gMapWeaponStats[1].sY + gsInvDescY, pStr);
                }

                //Weight
                wprintf(pStr, "%1.1f", fWeight);
                uiStringLength = FontSubSystem.StringPixLength(pStr, ITEMDESC_FONT);
                //			sStrX =  gMapWeaponStats[ 0 ].sX + gsInvDescX + gMapWeaponStats[ 0 ].sValDx + ( uiRightLength - uiStringLength );
                FontSubSystem.FindFontRightCoordinates((int)(gMapWeaponStats[0].sX + gsInvDescX + gMapWeaponStats[0].sValDx + 6), (int)(gMapWeaponStats[0].sY + gsInvDescY), ITEM_STATS_WIDTH, ITEM_STATS_HEIGHT, pStr, FontStyle.BLOCKFONT2, out sStrX, out usY);
                mprintf(sStrX, gMapWeaponStats[0].sY + gsInvDescY, pStr);

                if ((InKeyRingPopup()) || (Item[gpItemDescObject.usItem].usItemClass.HasFlag(IC.KEY)))
                {
                    FontSubSystem.SetFontForeground((FontColor)6);

                    // build description for keys .. the sector found
                    //                    wprintf(pStr, "%s", sKeyDescriptionStrings[0]);
                    mprintf(gMapWeaponStats[4].sX + gsInvDescX, gMapWeaponStats[4].sY + gsInvDescY, pStr);
                    //                    wprintf(pStr, "%s", sKeyDescriptionStrings[1]);
                    //                    mprintf(gMapWeaponStats[4].sX + gsInvDescX, gMapWeaponStats[4].sY + gsInvDescY + GetFontHeight(FontStyle.BLOCKFONT) + 2, pStr);


                    FontSubSystem.SetFontForeground((FontColor)5);
                    //                    GetShortSectorString((int)SECTORX(KeyTable[gpItemDescObject.ubKeyID].usSectorFound), (int)SECTORY(KeyTable[gpItemDescObject.ubKeyID].usSectorFound), sTempString);
//                    wprintf(pStr, "%s", sTempString);
                    FontSubSystem.FindFontRightCoordinates((int)(gMapWeaponStats[4].sX + gsInvDescX), (int)(gMapWeaponStats[4].sY + gsInvDescY), 110, ITEM_STATS_HEIGHT, pStr, FontStyle.BLOCKFONT2, out usX, out usY);
                    mprintf(usX, usY, pStr);

                    //                    wprintf(pStr, "%d", KeyTable[gpItemDescObject.ubKeyID].usDateFound);
//                    FontSubSystem.FindFontRightCoordinates((int)(gMapWeaponStats[4].sX + gsInvDescX), (int)(gMapWeaponStats[4].sY + gsInvDescY + GetFontHeight(FontStyle.BLOCKFONT) + 2), 110, ITEM_STATS_HEIGHT, pStr, FontStyle.BLOCKFONT2, out usX, out usY);
                    mprintf(usX, usY, pStr);
                }

            }

            FontSubSystem.SetFontShadow(FontShadow.DEFAULT_SHADOW);
        }
        else if (gfInItemDescBox)
        {

            // TAKE A LOOK AT THE VIDEO OBJECT SIZE ( ONE OF TWO SIZES ) AND CENTER!
            //            GetVideoObject(hVObject, guiItemGraphic);
            pTrav = (hVObject.pETRLEObject[0]);
            usHeight = (int)pTrav.usHeight;
            usWidth = (int)pTrav.usWidth;

            // CENTER IN SLOT!
            sCenX = ITEMDESC_ITEM_X + (Math.Abs(ITEMDESC_ITEM_WIDTH - usWidth) / 2) - pTrav.sOffsetX;
            sCenY = ITEMDESC_ITEM_Y + (Math.Abs(ITEMDESC_ITEM_HEIGHT - usHeight) / 2) - pTrav.sOffsetY;

            //            BltVideoObjectFromIndex(guiSAVEBUFFER, guiItemDescBox, 0, gsInvDescX, gsInvDescY, VO_BLT_SRCTRANSPARENCY, null);

            if (gpItemDescObject.usItem == Items.MONEY)
            {
                //Render the money Boxes
                //                BltVideoObjectFromIndex(guiSAVEBUFFER, guiMoneyGraphicsForDescBox, 0, (int)(gsInvDescX + gItemDescAttachmentsXY[0].sX - 1), (int)(gsInvDescY + gItemDescAttachmentsXY[0].sY - 2), VO_BLT_SRCTRANSPARENCY, null);
            }


            // Display item
            //            BltVideoObjectOutlineShadowFromIndex(guiSAVEBUFFER, guiItemGraphic, 0, sCenX - 2, sCenY + 2);
            //            BltVideoObjectFromIndex(guiSAVEBUFFER, guiItemGraphic, 0, sCenX, sCenY, VO_BLT_SRCTRANSPARENCY, null);

            // Display status
            //            DrawItemUIBarEx(gpItemDescObject, gubItemDescStatusIndex, (int)ITEMDESC_ITEM_STATUS_X, (int)ITEMDESC_ITEM_STATUS_Y, ITEMDESC_ITEM_STATUS_WIDTH, ITEMDESC_ITEM_STATUS_HEIGHT, Get16BPPColor(DESC_STATUS_BAR), Get16BPPColor(DESC_STATUS_BAR_SHADOW), true, guiSAVEBUFFER);

            if (gpItemPointer is not null)
            {
//                if ((Item[gpItemPointer.usItem].fFlags & ITEM_HIDDEN_ADDON)
//                    || (!ValidItemAttachment(gpItemDescObject, gpItemPointer.usItem, false)
//                    && !ValidMerge(gpItemPointer.usItem, gpItemDescObject.usItem)
//                    && !ValidLaunchable(gpItemPointer.usItem, gpItemDescObject.usItem)))
                {
                    // hatch out the attachment panels
                    fHatchOutAttachments = true;
                }
            }

            // Display attachments
            for (cnt = 0; cnt < MAX_ATTACHMENTS; cnt++)
            {
                if (gpItemDescObject.usAttachItem[cnt] != NOTHING)
                {
                    sCenX = (int)(gsInvDescX + gItemDescAttachmentsXY[cnt].sX + 5);
                    sCenY = (int)(gsInvDescY + gItemDescAttachmentsXY[cnt].sY - 1);

                    INVRenderItem(guiSAVEBUFFER, null, gpItemDescObject, sCenX, sCenY, gItemDescAttachmentsXY[cnt].sWidth, gItemDescAttachmentsXY[cnt].sHeight, DIRTYLEVEL2, null, (int)(RENDER_ITEM_ATTACHMENT1 + cnt), false, 0);

                    sCenX = sCenX - gItemDescAttachmentsXY[cnt].sBarDx;
                    sCenY = sCenY + gItemDescAttachmentsXY[cnt].sBarDy;
                    //                    DrawItemUIBarEx(gpItemDescObject, (int)(DRAW_ITEM_STATUS_ATTACHMENT1 + cnt), sCenX, sCenY, ITEM_BAR_WIDTH, ITEM_BAR_HEIGHT, Get16BPPColor(STATUS_BAR), Get16BPPColor(STATUS_BAR_SHADOW), true, guiSAVEBUFFER);

                    //                    SetRegionFastHelpText((gItemDescAttachmentRegions[cnt]), ItemNames[gpItemDescObject.usAttachItem[cnt]]);
                    //                    SetRegionHelpEndCallback((gItemDescAttachmentRegions[cnt]), HelpTextDoneCallback);
                }
                else
                {
                    //                    SetRegionFastHelpText((gItemDescAttachmentRegions[cnt]), Message[STR_ATTACHMENTS]);
                    //                    SetRegionHelpEndCallback((gItemDescAttachmentRegions[cnt]), HelpTextDoneCallback);
                }
                if (fHatchOutAttachments)
                {
                    //int uiWhichBuffer = ( guiCurrentItemDescriptionScreen == MAP_SCREEN ) ? guiSAVEBUFFER : guiRENDERBUFFER;
                    //                    DrawHatchOnInventory(guiSAVEBUFFER, (int)(gsInvDescX + gItemDescAttachmentsXY[cnt].sX), (int)(gsInvDescY + gItemDescAttachmentsXY[cnt].sY - 2), (int)(gItemDescAttachmentsXY[cnt].sWidth + gItemDescAttachmentsXY[cnt].sBarDx), (int)(gItemDescAttachmentsXY[cnt].sHeight + 2));
                }
            }

            if (Item[gpItemDescObject.usItem].usItemClass.HasFlag(IC.GUN))
            {
                // display bullets for ROF
                //                BltVideoObjectFromIndex(guiSAVEBUFFER, guiBullet, 0, BULLET_SING_X, BULLET_SING_Y, VO_BLT_SRCTRANSPARENCY, null);

                //                if (Weapon[gpItemDescObject.usItem].ubShotsPerBurst > 0)
                //                {
                //                    for (cnt = 0; cnt < Weapon[gpItemDescObject.usItem].ubShotsPerBurst; cnt++)
                //                    {
                //                        BltVideoObjectFromIndex(guiSAVEBUFFER, guiBullet, 0, BULLET_BURST_X + cnt * (BULLET_WIDTH + 1), BULLET_BURST_Y, VO_BLT_SRCTRANSPARENCY, null);
                //                    }
                //                }

            }

            //            RestoreExternBackgroundRect(gsInvDescX, gsInvDescY, ITEMDESC_WIDTH, ITEMDESC_HEIGHT);

            // Render font desc
            FontSubSystem.SetFont(ITEMDESC_FONT);
            FontSubSystem.SetFontBackground(FontColor.FONT_MCOLOR_BLACK);
            FontSubSystem.SetFontForeground(FontColor.FONT_FCOLOR_WHITE);
            FontSubSystem.SetFontShadow(ITEMDESC_FONTSHADOW3);

            // Render name
            // SET CLIPPING RECT FOR FONTS
#if JA2TESTVERSION
            mprintf(ITEMDESC_NAME_X, ITEMDESC_NAME_Y, "%s (%d)", gzItemName, gpItemDescObject.usItem);
#else
            //        mprintf(ITEMDESC_NAME_X, ITEMDESC_NAME_Y, "%s", gzItemName);
#endif

            // Render caliber and description

            FontSubSystem.SetFontForeground(FontColor.FONT_BLACK);
            FontSubSystem.SetFontShadow(ITEMDESC_FONTSHADOW2);

//            FontSubSystem.DisplayWrappedString((int)ITEMDESC_DESC_START_X, (int)ITEMDESC_DESC_START_Y, ITEMDESC_DESC_WIDTH, 2, ITEMDESC_FONT, FontColor.FONT_BLACK, gzItemDesc, FontColor.FONT_MCOLOR_BLACK, false, LEFT_JUSTIFIED);

            if (ITEM_PROS_AND_CONS(gpItemDescObject.usItem))
            {
                if ((gpItemDescObject.usItem == Items.ROCKET_RIFLE || gpItemDescObject.usItem == Items.AUTO_ROCKET_RIFLE)
                    && gpItemDescObject.ubImprintID < NO_PROFILE)
                {
                    // add name noting imprint
                    //                    wprintf(pStr, "%s %s (%s)", AmmoCaliber[Weapon[gpItemDescObject.usItem].ubCalibre], WeaponType[Weapon[gpItemDescObject.usItem].ubWeaponType], gMercProfiles[gpItemDescObject.ubImprintID].zNickname);
                }
                else
                {
                    //                    wprintf(pStr, "%s %s", AmmoCaliber[Weapon[gpItemDescObject.usItem].ubCalibre], WeaponType[Weapon[gpItemDescObject.usItem].ubWeaponType]);
                }

                FontSubSystem.FindFontRightCoordinates((int)ITEMDESC_CALIBER_X, (int)ITEMDESC_CALIBER_Y, ITEMDESC_CALIBER_WIDTH, ITEM_STATS_HEIGHT, pStr, ITEMDESC_FONT, out usX, out usY);
                mprintf(usX, usY, pStr);

                FontSubSystem.SetFontForeground(FontColor.FONT_MCOLOR_DKWHITE2);
                FontSubSystem.SetFontShadow(ITEMDESC_FONTSHADOW3);
//                mprintf((int)ITEMDESC_PROS_START_X, (int)ITEMDESC_PROS_START_Y, gzProsLabel);

//                sProsConsIndent = Math.Max(FontSubSystem.StringPixLength(gzProsLabel, ITEMDESC_FONT), FontSubSystem.StringPixLength(gzConsLabel, ITEMDESC_FONT)) + 10;

                gzItemPros[0] = 0;
//                GenerateProsString(gzItemPros, gpItemDescObject, ITEMDESC_DESC_WIDTH - sProsConsIndent - FontSubSystem.StringPixLength(DOTDOTDOT, ITEMDESC_FONT));
                if (gzItemPros[0] != 0)
                {
                    FontSubSystem.SetFontForeground(FontColor.FONT_BLACK);
                    FontSubSystem.SetFontShadow(ITEMDESC_FONTSHADOW2);
//                    FontSubSystem.DisplayWrappedString((int)(ITEMDESC_PROS_START_X + sProsConsIndent), (int)ITEMDESC_PROS_START_Y, (int)(ITEMDESC_DESC_WIDTH - sProsConsIndent), 2, ITEMDESC_FONT, FontColor.FONT_BLACK, gzItemPros, FontColor.FONT_MCOLOR_BLACK, false, LEFT_JUSTIFIED);
                }

                FontSubSystem.SetFontForeground(FontColor.FONT_MCOLOR_DKWHITE2);
                FontSubSystem.SetFontShadow(ITEMDESC_FONTSHADOW3);
//                mprintf((int)ITEMDESC_CONS_START_X, (int)ITEMDESC_CONS_START_Y, gzConsLabel);

//                GenerateConsString(gzItemCons, gpItemDescObject, ITEMDESC_DESC_WIDTH - sProsConsIndent - FontSubSystem.StringPixLength(DOTDOTDOT, ITEMDESC_FONT));
                if (gzItemCons[0] != 0)
                {
                    FontSubSystem.SetFontForeground(FontColor.FONT_BLACK);
                    FontSubSystem.SetFontShadow(ITEMDESC_FONTSHADOW2);
//                    FontSubSystem.DisplayWrappedString((int)(ITEMDESC_CONS_START_X + sProsConsIndent), (int)ITEMDESC_CONS_START_Y, (int)(ITEMDESC_DESC_WIDTH - sProsConsIndent), 2, ITEMDESC_FONT, FontColor.FONT_BLACK, gzItemCons, FontColor.FONT_MCOLOR_BLACK, false, LEFT_JUSTIFIED);
                }


            }


            // Get length of string
            uiRightLength = 35;

            // Calculate total weight of item and attachments
//            fWeight = (float)(CalculateObjectWeight(gpItemDescObject)) / 10;
            if (!Globals.gGameSettings[TOPTION.USE_METRIC_SYSTEM])
            {
                fWeight = fWeight * 2.2f;
            }

            if (fWeight < 0.1)
            {
                fWeight = (float)0.1;
            }

            // Render, stat  name
            //            if (Item[gpItemDescObject.usItem].usItemClass & IC.WEAPON)
            {

                FontSubSystem.SetFont(FontStyle.BLOCKFONT2);
                FontSubSystem.SetFontForeground((FontColor)6);
                FontSubSystem.SetFontShadow(FontShadow.DEFAULT_SHADOW);

                //LABELS
                //                wprintf(sTempString, gWeaponStatsDesc[0], GetWeightUnitString());
                mprintf(gWeaponStats[0].sX + gsInvDescX, gWeaponStats[0].sY + gsInvDescY, sTempString);
                //		mprintf( gWeaponStats[ 1 ].sX + gsInvDescX, gWeaponStats[ 1 ].sY + gsInvDescY, "%s", gWeaponStatsDesc[ 1 ].zDesc );
                //		mprintf( gWeaponStats[ 2 ].sX + gsInvDescX, gWeaponStats[ 2 ].sY + gsInvDescY, "%s", gWeaponStats[ 2 ].zDesc );
                //                if (Item[gpItemDescObject.usItem].usItemClass.HasFlag(IC.GUN | IC.LAUNCHER))
                //                {
                //                    mprintf(gWeaponStats[3].sX + gsInvDescX, gWeaponStats[3].sY + gsInvDescY, "%s", gWeaponStatsDesc[3]);
                //                }
                //                if (!(Item[gpItemDescObject.usItem].usItemClass.HasFlag(IC.LAUNCHER)) && gpItemDescObject.usItem != Items.ROCKET_LAUNCHER)
                //                {
                //                    mprintf(gWeaponStats[4].sX + gsInvDescX, gWeaponStats[4].sY + gsInvDescY, "%s", gWeaponStatsDesc[4]);
                //                }
                //                mprintf(gWeaponStats[5].sX + gsInvDescX, gWeaponStats[5].sY + gsInvDescY, "%s", gWeaponStatsDesc[5]);
                //                if (Item[gpItemDescObject.usItem].usItemClass.HasFlag(IC.GUN))
                //                {
                //                    mprintf(gWeaponStats[7].sX + gsInvDescX, gWeaponStats[7].sY + gsInvDescY, "%s", gWeaponStatsDesc[7]);
                //                }
                //                mprintf(gWeaponStats[1].sX + gsInvDescX, gWeaponStats[1].sY + gsInvDescY, "%s", gWeaponStatsDesc[1]);
                //
                //                if (Weapon[gpItemDescObject.usItem].ubShotsPerBurst > 0)
                //                {
                //                    mprintf(gWeaponStats[8].sX + gsInvDescX, gWeaponStats[8].sY + gsInvDescY, "%s", gWeaponStatsDesc[8]);
                //                }

                // Values
                if (fWeight <= (EXCEPTIONAL_WEIGHT / 10))
                {
                    FontSubSystem.SetFontForeground(ITEMDESC_FONTHIGHLIGHT);
                }
                else
                {
                    FontSubSystem.SetFontForeground((FontColor)5);
                }

                //Status
                wprintf(pStr, "%2d%%", gpItemDescObject.bGunStatus);
                FontSubSystem.FindFontRightCoordinates((int)(gWeaponStats[1].sX + gsInvDescX + gWeaponStats[1].sValDx), (int)(gWeaponStats[1].sY + gsInvDescY), ITEM_STATS_WIDTH, ITEM_STATS_HEIGHT, pStr, FontStyle.BLOCKFONT2, out usX, out usY);
                wcscat(pStr, "%%");
                mprintf(usX, usY, pStr);

                //Wieght
                wprintf(pStr, "%1.1f", fWeight);
                FontSubSystem.FindFontRightCoordinates((int)(gWeaponStats[0].sX + gsInvDescX + gWeaponStats[0].sValDx), (int)(gWeaponStats[0].sY + gsInvDescY), ITEM_STATS_WIDTH, ITEM_STATS_HEIGHT, pStr, FontStyle.BLOCKFONT2, out usX, out usY);
                mprintf(usX, usY, pStr);

                if (Item[gpItemDescObject.usItem].usItemClass.HasFlag(IC.GUN | IC.LAUNCHER))
                {
                    //                    if (GunRange(gpItemDescObject) >= EXCEPTIONAL_RANGE)
                    {
                        FontSubSystem.SetFontForeground(ITEMDESC_FONTHIGHLIGHT);
                    }
                    //                    else
                    {
                        FontSubSystem.SetFontForeground((FontColor)5);
                    }

                    //                    wprintf(pStr, "%2d", (GunRange(gpItemDescObject)) / 10);
                    FontSubSystem.FindFontRightCoordinates((int)(gWeaponStats[3].sX + gsInvDescX + gWeaponStats[3].sValDx), (int)(gWeaponStats[3].sY + gsInvDescY), ITEM_STATS_WIDTH, ITEM_STATS_HEIGHT, pStr, FontStyle.BLOCKFONT2, out usX, out usY);
                    mprintf(usX, usY, pStr);
                }

                //                if (!(Item[gpItemDescObject.usItem].usItemClass & IC.LAUNCHER) && gpItemDescObject.usItem != ROCKET_LAUNCHER)
                {

                    //                    if (Weapon[gpItemDescObject.usItem].ubImpact >= EXCEPTIONAL_DAMAGE)
                    {
                        FontSubSystem.SetFontForeground(ITEMDESC_FONTHIGHLIGHT);
                    }
                    //                    else
                    {
                        //                        FontSubSystem.SetFontForeground(5);
                    }

                    //                    wprintf(pStr, "%2d", Weapon[gpItemDescObject.usItem].ubImpact);
                    FontSubSystem.FindFontRightCoordinates((int)(gWeaponStats[4].sX + gsInvDescX + gWeaponStats[4].sValDx), (int)(gWeaponStats[4].sY + gsInvDescY), ITEM_STATS_WIDTH, ITEM_STATS_HEIGHT, pStr, FontStyle.BLOCKFONT2, out usX, out usY);
                    mprintf(usX, usY, pStr);
                }

                //                ubAttackAPs = BaseAPsToShootOrStab(DEFAULT_APS, DEFAULT_AIMSKILL, gpItemDescObject);

                if (ubAttackAPs <= EXCEPTIONAL_AP_COST)
                {
                    FontSubSystem.SetFontForeground(ITEMDESC_FONTHIGHLIGHT);
                }
                else
                {
                    FontSubSystem.SetFontForeground((FontColor)5);
                }

                wprintf(pStr, "%2d", ubAttackAPs);
                FontSubSystem.FindFontRightCoordinates((int)(gWeaponStats[5].sX + gsInvDescX + gWeaponStats[5].sValDx), (int)(gWeaponStats[5].sY + gsInvDescY), ITEM_STATS_WIDTH, ITEM_STATS_HEIGHT, pStr, FontStyle.BLOCKFONT2, out usX, out usY);
                mprintf(usX, usY, pStr);

                //                if (Weapon[gpItemDescObject.usItem].ubShotsPerBurst > 0)
                //                {
                //                    if (Weapon[gpItemDescObject.usItem].ubShotsPerBurst >= EXCEPTIONAL_BURST_SIZE || gpItemDescObject.usItem == G11)
                //                    {
                //                        FontSubSystem.SetFontForeground(ITEMDESC_FONTHIGHLIGHT);
                //                    }
                //                    else
                //                    {
                //                        FontSubSystem.SetFontForeground((FontColor)5);
                //                    }
                //
                ////                    wprintf(pStr, "%2d", ubAttackAPs + CalcAPsToBurst(DEFAULT_APS, gpItemDescObject));
                //                    FontSubSystem.FindFontRightCoordinates((int)(gWeaponStats[6].sX + gsInvDescX + gWeaponStats[6].sValDx), (int)(gWeaponStats[6].sY + gsInvDescY), ITEM_STATS_WIDTH, ITEM_STATS_HEIGHT, pStr, BLOCKFONT2, out usX, out usY);
                //                    mprintf(usX, usY, pStr);
                //                }

            }
            //            else if (gpItemDescObject.usItem == Items.MONEY)
            {
                //Labels
                FontSubSystem.SetFont(FontStyle.BLOCKFONT2);
                FontSubSystem.SetFontShadow(FontShadow.DEFAULT_SHADOW);

                FontSubSystem.SetFontForeground((FontColor)6);

                //Display the 'Seperate text'

                //if the player is removing money from the players account
                //                if (gfAddingMoneyToMercFromPlayersAccount)
                //                {
                //                    mprintf((int)(gMoneyButtonLoc.x + gMoneyButtonOffsets[4].x), (int)(gMoneyButtonLoc.y + gMoneyButtonOffsets[4].y), gzMoneyAmounts[5]);
                //                }
                //                else
                //                {
                //                    mprintf((int)(gMoneyButtonLoc.x + gMoneyButtonOffsets[4].x), (int)(gMoneyButtonLoc.y + gMoneyButtonOffsets[4].y), gzMoneyAmounts[4]);
                //                }


                // if the player is taking money from their account
                //                if (gfAddingMoneyToMercFromPlayersAccount)
                //                {
                //                    //Display the 'Removing'
                //                    mprintf(gMoneyStats[0].sX + gsInvDescX, gMoneyStats[0].sY + gsInvDescY, "%s", gMoneyStatsDesc[MONEY_DESC_PLAYERS]);
                //                    //Display the 'REmaining'
                //                    mprintf(gMoneyStats[2].sX + gsInvDescX, gMoneyStats[2].sY + gsInvDescY, "%s", gMoneyStatsDesc[MONEY_DESC_AMOUNT_2_WITHDRAW]);
                //                }
                //                else
                //                {
                //                    //Display the 'Removing'
                //                    mprintf(gMoneyStats[0].sX + gsInvDescX, gMoneyStats[0].sY + gsInvDescY, "%s", gMoneyStatsDesc[0]);
                //                    //Display the 'REmaining'
                //                    mprintf(gMoneyStats[2].sX + gsInvDescX, gMoneyStats[2].sY + gsInvDescY, "%s", gMoneyStatsDesc[2]);
                //                }

                // Total Amount 
                FontSubSystem.SetFontForeground(FontColor.FONT_WHITE);
                wprintf(pStr, "%d", gRemoveMoney.uiTotalAmount);
                //                InsertCommasForDollarFigure(pStr);
                //                InsertDollarSignInToString(pStr);
                FontSubSystem.FindFontRightCoordinates((int)(ITEMDESC_NAME_X), (int)(ITEMDESC_NAME_Y), 295, ITEM_STATS_HEIGHT, pStr, FontStyle.BLOCKFONT2, out usX, out usY);
                mprintf(usX, usY, pStr);

                FontSubSystem.SetFontForeground((FontColor)6);

                // if the player is taking money from their account
                //                if (gfAddingMoneyToMercFromPlayersAccount)
                //                {
                //                    //Display the 'Amt removing'
                //                    mprintf(gMoneyStats[1].sX + gsInvDescX, gMoneyStats[1].sY + gsInvDescY, "%s", gMoneyStatsDesc[MONEY_DESC_BALANCE]);
                //                    //Display the 'REmaining amount'
                //                    mprintf(gMoneyStats[3].sX + gsInvDescX, gMoneyStats[3].sY + gsInvDescY, "%s", gMoneyStatsDesc[MONEY_DESC_TO_WITHDRAW]);
                //                }
                //                else
                //                {
                //                    //Display the 'Amt removing'
                //                    mprintf(gMoneyStats[1].sX + gsInvDescX, gMoneyStats[1].sY + gsInvDescY, "%s", gMoneyStatsDesc[1]);
                //                    //Display the 'REmaining amount'
                //                    mprintf(gMoneyStats[3].sX + gsInvDescX, gMoneyStats[3].sY + gsInvDescY, "%s", gMoneyStatsDesc[3]);
                //                }



                // Values
                FontSubSystem.SetFontForeground((FontColor)5);

                //Display the total amount of money remaining
                wprintf(pStr, "%ld", gRemoveMoney.uiMoneyRemaining);
                //                InsertCommasForDollarFigure(pStr);
                //                InsertDollarSignInToString(pStr);
                FontSubSystem.FindFontRightCoordinates((int)(gMoneyStats[1].sX + gsInvDescX + gMoneyStats[1].sValDx), (int)(gMoneyStats[1].sY + gsInvDescY), (int)(ITEM_STATS_WIDTH - 3), ITEM_STATS_HEIGHT, pStr, FontStyle.BLOCKFONT2, out usX, out usY);
                mprintf(usX, usY, pStr);

                //Display the total amount of money removing
                wprintf(pStr, "%ld", gRemoveMoney.uiMoneyRemoving);
                //                InsertCommasForDollarFigure(pStr);
                //                InsertDollarSignInToString(pStr);
                FontSubSystem.FindFontRightCoordinates((int)(gMoneyStats[3].sX + gsInvDescX + gMoneyStats[3].sValDx), (int)(gMoneyStats[3].sY + gsInvDescY), (int)(ITEM_STATS_WIDTH - 3), ITEM_STATS_HEIGHT, pStr, FontStyle.BLOCKFONT2, out usX, out usY);
                mprintf(usX, usY, pStr);

            }
            //          else if (Item[gpItemDescObject.usItem].usItemClass == IC.MONEY)
            {
                //                FontSubSystem.SetFontForeground(FONT_FCOLOR_WHITE);
                //                FontSubSystem.SetFontShadow(DEFAULT_SHADOW);
                //                wprintf(pStr, "%ld", gpItemDescObject.uiMoneyAmount);
                //                InsertCommasForDollarFigure(pStr);
                //                InsertDollarSignInToString(pStr);

                //                FontSubSystem.FindFontRightCoordinates((int)(ITEMDESC_NAME_X), (int)(ITEMDESC_NAME_Y), 295, ITEM_STATS_HEIGHT, pStr, BLOCKFONT2, out usX, out usY);
                mprintf(usX, usY, pStr);
            }
            //            else
            {
                //Labels
                //                FontSubSystem.SetFont(BLOCKFONT2);
                //                FontSubSystem.SetFontForeground(6);
                //                FontSubSystem.SetFontShadow(DEFAULT_SHADOW);


                if (Item[gpItemDescObject.usItem].usItemClass.HasFlag(IC.AMMO))
                {
                    //Status
                    //                    mprintf(gWeaponStats[2].sX + gsInvDescX, gWeaponStats[2].sY + gsInvDescY, "%s", gWeaponStatsDesc[2]);
                }
                else
                {
                    //                    mprintf(gWeaponStats[1].sX + gsInvDescX, gWeaponStats[1].sY + gsInvDescY, "%s", gWeaponStatsDesc[1]);
                }

                //Weight
                //                wprintf(sTempString, gWeaponStatsDesc[0], GetWeightUnitString());
                mprintf(gWeaponStats[0].sX + gsInvDescX, gWeaponStats[0].sY + gsInvDescY, sTempString);

                // Values
                //                FontSubSystem.SetFontForeground(5);

                //                if (Item[gpItemDescObject.usItem].usItemClass & IC.AMMO)
                {
                    // Ammo - print amount
                    //Status
                    //                    wprintf(pStr, "%d/%d", gpItemDescObject.ubShotsLeft[0], Magazine[Item[gpItemDescObject.usItem].ubClassIndex].ubMagSize);
                    FontSubSystem.FindFontRightCoordinates((int)(gWeaponStats[2].sX + gsInvDescX + gWeaponStats[2].sValDx), (int)(gWeaponStats[2].sY + gsInvDescY), ITEM_STATS_WIDTH, ITEM_STATS_HEIGHT, pStr, FontStyle.BLOCKFONT2, out usX, out usY);
                    mprintf(usX, usY, pStr);
                }
                //                else
                {
                    //Status
                    wprintf(pStr, "%2d%%", gpItemDescObject.bStatus[gubItemDescStatusIndex]);
                    FontSubSystem.FindFontRightCoordinates((int)(gWeaponStats[1].sX + gsInvDescX + gWeaponStats[1].sValDx), (int)(gWeaponStats[1].sY + gsInvDescY), ITEM_STATS_WIDTH, ITEM_STATS_HEIGHT, pStr, FontStyle.BLOCKFONT2, out usX, out usY);
                    wcscat(pStr, "%%");
                    mprintf(usX, usY, pStr);
                }

                if ((InKeyRingPopup() == true) || (Item[gpItemDescObject.usItem].usItemClass.HasFlag(IC.KEY)))
                {
//                    FontSubSystem.SetFontForeground(6);
//
//                    // build description for keys .. the sector found
//                    wprintf(pStr, "%s", sKeyDescriptionStrings[0]);
//                    mprintf(gWeaponStats[4].sX + gsInvDescX, gWeaponStats[4].sY + gsInvDescY, pStr);
//                    wprintf(pStr, "%s", sKeyDescriptionStrings[1]);
//                    mprintf(gWeaponStats[4].sX + gsInvDescX, gWeaponStats[4].sY + gsInvDescY + GetFontHeight(BLOCKFONT) + 2, pStr);


//                    FontSubSystem.SetFontForeground(5);
//                    GetShortSectorString((int)SECTORX(KeyTable[gpItemDescObject.ubKeyID].usSectorFound), (int)SECTORY(KeyTable[gpItemDescObject.ubKeyID].usSectorFound), sTempString);
//                    wprintf(pStr, "%s", sTempString);
//                    FontSubSystem.FindFontRightCoordinates((int)(gWeaponStats[4].sX + gsInvDescX), (int)(gWeaponStats[4].sY + gsInvDescY), 110, ITEM_STATS_HEIGHT, pStr, BLOCKFONT2, out usX, out usY);
//                    mprintf(usX, usY, pStr);
//
//                    wprintf(pStr, "%d", KeyTable[gpItemDescObject.ubKeyID].usDateFound);
//                    FontSubSystem.FindFontRightCoordinates((int)(gWeaponStats[4].sX + gsInvDescX), (int)(gWeaponStats[4].sY + gsInvDescY + GetFontHeight(BLOCKFONT) + 2), 110, ITEM_STATS_HEIGHT, pStr, BLOCKFONT2, out usX, out usY);
//                    mprintf(usX, usY, pStr);
                }




                //Weight
                wprintf(pStr, "%1.1f", fWeight);
                FontSubSystem.FindFontRightCoordinates((int)(gWeaponStats[0].sX + gsInvDescX + gWeaponStats[0].sValDx), (int)(gWeaponStats[0].sY + gsInvDescY), ITEM_STATS_WIDTH, ITEM_STATS_HEIGHT, pStr, FontStyle.BLOCKFONT2, out usX, out usY);
                mprintf(usX, usY, pStr);
            }

            FontSubSystem.SetFontShadow(FontShadow.DEFAULT_SHADOW);

        }

    }

    void HandleItemDescriptionBox(out int pfDirty)
    {
        pfDirty = 0;

        if (fItemDescDelete)
        {
            DeleteItemDescriptionBox();
            fItemDescDelete = false;
            pfDirty = DIRTYLEVEL2;
        }

    }


    void DeleteItemDescriptionBox()
    {
        int cnt, cnt2;
        bool fFound, fAllFound;

        if (gfInItemDescBox == false)
        {
            return;
        }

        //	DEF:

        //Used in the shopkeeper interface
        if (guiTacticalInterfaceFlags.HasFlag(INTERFACE.SHOPKEEP_INTERFACE))
        {
//            DeleteShopKeeperItemDescBox();
        }

        // check for any AP costs
        if ((gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.TURNBASED)) && (gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.INCOMBAT)))
        {
            if (gpAttachSoldier is not null)
            {
                // check for change in attachments, starting with removed attachments
                fAllFound = true;
                for (cnt = 0; cnt < MAX_ATTACHMENTS; cnt++)
                {
                    if (gusOriginalAttachItem[cnt] != NOTHING)
                    {
                        fFound = false;
                        for (cnt2 = 0; cnt2 < MAX_ATTACHMENTS; cnt2++)
                        {
                            if ((gusOriginalAttachItem[cnt] == gpItemDescObject.usAttachItem[cnt2]) && (gpItemDescObject.bAttachStatus[cnt2] == gbOriginalAttachStatus[cnt]))
                            {
                                fFound = true;
                            }
                        }
                        if (!fFound)
                        {
                            // charge APs
                            fAllFound = false;
                            break;
                        }
                    }
                }

                if (fAllFound)
                {
                    // nothing was removed; search for attachment added
                    for (cnt = 0; cnt < MAX_ATTACHMENTS; cnt++)
                    {
                        if (gpItemDescObject.usAttachItem[cnt] != NOTHING)
                        {
                            fFound = false;
                            for (cnt2 = 0; cnt2 < MAX_ATTACHMENTS; cnt2++)
                            {
                                if ((gpItemDescObject.usAttachItem[cnt] == gusOriginalAttachItem[cnt2]) && (gbOriginalAttachStatus[cnt2] == gpItemDescObject.bAttachStatus[cnt]))
                                {
                                    fFound = true;
                                }
                            }
                            if (!fFound)
                            {
                                // charge APs
                                fAllFound = false;
                                break;
                            }
                        }
                    }
                }

                if (!fAllFound)
                {
                    //                    DeductPoints(gpAttachSoldier, AP.RELOAD_GUN, 0);
                }
            }
        }

        //Remove
        //        DeleteVideoObjectFromIndex(guiItemDescBox);
        //        DeleteVideoObjectFromIndex(guiMapItemDescBox);
        //        DeleteVideoObjectFromIndex(guiBullet);
        // Delete item graphic
        //        DeleteVideoObjectFromIndex(guiItemGraphic);

        gfInItemDescBox = false;

        //	if( guiTacticalInterfaceFlags & INTERFACE_MAPSCREEN  )
        if (guiCurrentItemDescriptionScreen == ScreenName.MAP_SCREEN)
        {
            //            UnloadButtonImage(giMapInvDescButtonImage);
            //            RemoveButton(giMapInvDescButton);
        }

        // Remove region
        MouseSubSystem.MSYS_RemoveRegion(gInvDesc);


        if (gpItemDescObject.usItem != Items.MONEY)
        {
            for (cnt = 0; cnt < MAX_ATTACHMENTS; cnt++)
            {
                MouseSubSystem.MSYS_RemoveRegion(gItemDescAttachmentRegions[cnt]);
            }
        }
        else
        {
            //            UnloadButtonImage(guiMoneyButtonImage);
            //            UnloadButtonImage(guiMoneyDoneButtonImage);
            for (cnt = 0; cnt < MAX_ATTACHMENTS; cnt++)
            {
                //                RemoveButton(guiMoneyButtonBtn[cnt]);
            }
        }

        if (ITEM_PROS_AND_CONS(gpItemDescObject.usItem))
        {
            MouseSubSystem.MSYS_RemoveRegion(gProsAndConsRegions[0]);
            MouseSubSystem.MSYS_RemoveRegion(gProsAndConsRegions[1]);
        }

        if (((Item[gpItemDescObject.usItem].usItemClass.HasFlag(IC.GUN)) && gpItemDescObject.usItem != Items.ROCKET_LAUNCHER))
        {
            // Remove button
            //            UnloadButtonImage(giItemDescAmmoButtonImages);
            //            RemoveButton(giItemDescAmmoButton);
        }
        //	if(guiTacticalInterfaceFlags & INTERFACE_MAPSCREEN )
        if (guiCurrentItemDescriptionScreen == ScreenName.MAP_SCREEN)
        {
            fCharacterInfoPanelDirty = true;
            fMapPanelDirty = true;
            fTeamPanelDirty = true;
            fMapScreenBottomDirty = true;
        }

        if (InKeyRingPopup() == true)
        {
            //            DeleteKeyObject(gpItemDescObject);
            gpItemDescObject = null;
            fShowDescriptionFlag = false;
            fInterfacePanelDirty = DIRTYLEVEL2;
            return;
        }

        fShowDescriptionFlag = false;
        fInterfacePanelDirty = DIRTYLEVEL2;

        if (gpItemDescObject.usItem == Items.MONEY)
        {
            //if there is no money remaining
            if (gRemoveMoney.uiMoneyRemaining == 0 && !gfAddingMoneyToMercFromPlayersAccount)
            {
                //get rid of the money in the slot
                //                memset(gpItemDescObject, 0, sizeof(OBJECTTYPE));
                gpItemDescObject = null;
            }
        }

        if (gfAddingMoneyToMercFromPlayersAccount)
        {
            gfAddingMoneyToMercFromPlayersAccount = false;
        }

        gfItemDescObjectIsAttachment = false;
    }


    void InternalBeginItemPointer(SOLDIERTYPE pSoldier, OBJECTTYPE pObject, InventorySlot bHandPos)
    {
        //	bool fOk;

        // If not null return
        if (gpItemPointer != null)
        {
            return;
        }

        // Copy into cursor...
        //memcpy(gItemPointer, pObject, sizeof(OBJECTTYPE));

        // Dirty interface
        fInterfacePanelDirty = DIRTYLEVEL2;
        gpItemPointer = gItemPointer;
        gpItemPointerSoldier = pSoldier;
        gbItemPointerSrcSlot = bHandPos;
        gbItemPointerLocateGood = true;

        //        CheckForDisabledForGiveItem();

        //        EnableSMPanelButtons(false, true);

        gfItemPointerDifferentThanDefault = false;

        // re-evaluate repairs
        //        gfReEvaluateEveryonesNothingToDo = true;
    }

    void BeginItemPointer(SOLDIERTYPE pSoldier, InventorySlot ubHandPos)
    {
        bool fOk = false;
        OBJECTTYPE pObject = new();

        //memset(&pObject, 0, sizeof(OBJECTTYPE));

        //        if (_KeyDown(SHIFT))
        //        {
        //            // Remove all from soldier's slot
        //            fOk = RemoveObjectFromSlot(pSoldier, ubHandPos, pObject);
        //        }
        //        else
        //        {
        //            GetObjFrom((pSoldier.inv[ubHandPos]), 0, pObject);
        //            fOk = (pObject.ubNumberOfObjects == 1);
        //        }
        if (fOk)
        {
            InternalBeginItemPointer(pSoldier, pObject, ubHandPos);
        }
    }


    void BeginKeyRingItemPointer(SOLDIERTYPE pSoldier, InventorySlot ubKeyRingPosition)
    {
        bool fOk = false;

        // If not null return
        if (gpItemPointer != null)
        {
            return;
        }

        //        if (_KeyDown(SHIFT))
        //        {
        //            // Remove all from soldier's slot
        //            fOk = RemoveKeysFromSlot(pSoldier, ubKeyRingPosition, pSoldier.pKeyRing[ubKeyRingPosition].ubNumber, gItemPointer);
        //        }
        //        else
        //        {
        //            RemoveKeyFromSlot(pSoldier, ubKeyRingPosition, gItemPointer);
        //            fOk = (gItemPointer.ubNumberOfObjects == 1);
        //        }


        if (fOk)
        {
            // ATE: Look if we are a BLOODIED KNIFE, and change if so, making guy scream...

            // Dirty interface
            fInterfacePanelDirty = DIRTYLEVEL2;
            gpItemPointer = gItemPointer;
            gpItemPointerSoldier = pSoldier;
            gbItemPointerSrcSlot = ubKeyRingPosition;

            if ((guiTacticalInterfaceFlags.HasFlag(INTERFACE.MAPSCREEN)))
            {
                //                guiExternVo = GetInterfaceGraphicForItem((Item[gpItemPointer.usItem]));
                //                gusExternVoSubIndex = Item[gpItemPointer.usItem].ubGraphicNum;

                fMapInventoryItem = true;
                MouseSubSystem.MSYS_ChangeRegionCursor(gMPanelRegion, CURSOR.EXTERN_CURSOR);
                MouseSubSystem.MSYS_SetCurrentCursor(CURSOR.EXTERN_CURSOR);
            }
        }
        else
        {
            //Debug mesg
        }



        gfItemPointerDifferentThanDefault = false;
    }

    void EndItemPointer()
    {
        if (gpItemPointer != null)
        {
            gpItemPointer = null;
            gbItemPointerSrcSlot = NO_SLOT;
            //            MouseSubSystem.MSYS_ChangeRegionCursor(gSMPanelRegion, CURSOR.NORMAL);
            MouseSubSystem.MSYS_SetCurrentCursor(CURSOR.NORMAL);

            if (guiTacticalInterfaceFlags.HasFlag(INTERFACE.SHOPKEEP_INTERFACE))
            {
                //memset(gMoveingItem, 0, sizeof(INVENTORY_IN_SLOT));
                //                SetSkiCursor(CURSOR.NORMAL);
            }
            else
            {
                //                EnableSMPanelButtons(true, true);
            }

            gbItemPointerLocateGood = false;

            // re-evaluate repairs
            //            gfReEvaluateEveryonesNothingToDo = true;
        }
    }

    void DrawItemFreeCursor()
    {
        //OBJECTTYPE		*gpItemPointer;
        //int				usItemSnapCursor;

        // Get usIndex and then graphic for item
        //        guiExternVo = GetInterfaceGraphicForItem((Item[gpItemPointer.usItem]));
        //        gusExternVoSubIndex = Item[gpItemPointer.usItem].ubGraphicNum;

        //        MouseSubSystem.MSYS_ChangeRegionCursor(gSMPanelRegion, CURSOR.EXTERN_CURSOR);
        MouseSubSystem.MSYS_SetCurrentCursor(CURSOR.EXTERN_CURSOR);
    }

    void HideItemTileCursor()
    {
        //	RemoveTopmost( gusCurMousePos, gusItemPointer );

    }

    bool SoldierCanSeeCatchComing(SOLDIERTYPE pSoldier, int sSrcGridNo)
    {
        return (true);
        /*-
            int							cnt;
            int							bDirection, bTargetDirection;

            bTargetDirection = (int)GetDirectionToGridNoFromGridNo( pSoldier.sGridNo, sSrcGridNo );

            // Look 3 directions Clockwise from what we are facing....
            bDirection = pSoldier.bDirection;

            for ( cnt = 0; cnt < 3; cnt++ )
            {
                if ( bDirection == bTargetDirection )
                {
                    return( true );
                }

                bDirection = gOneCDirection[ bDirection ];
            }

            // Look 3 directions CounterClockwise from what we are facing....
            bDirection = pSoldier.bDirection;

            for ( cnt = 0; cnt < 3; cnt++ )
            {
                if ( bDirection == bTargetDirection )
                {
                    return( true );
                }

                bDirection = gOneCCDirection[ bDirection ];
            }

            // If here, nothing good can happen!
            return( false );
        -*/

    }

    static CURSOR uiOldCursorId = 0;
    static int usOldMousePos = 0;
    void DrawItemTileCursor()
    {
        int usMapPos;
        TileIndexes usIndex = TileIndexes.UNSET;
        int ubSoldierID;
        int sAPCost;
        bool fRecalc;
        MOUSE uiCursorFlags;
        int sFinalGridNo;
        CURSOR uiCursorId = CURSOR.ITEM_GOOD_THROW;
        SOLDIERTYPE pSoldier;
        bool fGiveItem = false;
        int sActionGridNo;
        int ubDirection;
        int sEndZ = 0;
        int sDist;
        int bLevel;

        //        if (GetMouseMapPos(out usMapPos))
        //        {
        //            if (gfUIFullTargetFound)
        //            {
        //                // Force mouse position to guy...
        //                usMapPos = MercPtrs[gusUIFullTargetID].sGridNo;
        //            }
        //
        //            gusCurMousePos = usMapPos;
        //
        //            if (gusCurMousePos != usOldMousePos)
        //            {
        //                gfItemPointerDifferentThanDefault = false;
        //            }
        //
        //            // Save old one..
        //            usOldMousePos = gusCurMousePos;
        //
        //            // Default to turning adjacent area gridno off....
        //            gfUIHandleShowMoveGrid = false;
        //
        //            // If we are over a talkable guy, set flag
        //            if (IsValidTalkableNPCFromMouse(out ubSoldierID, true, false, true))
        //            {
        //                fGiveItem = true;
        //            }
        //
        //
        //            // OK, if different than default, change....
        //            if (gfItemPointerDifferentThanDefault)
        //            {
        //                fGiveItem = !fGiveItem;
        //            }
        //
        //
        //            // Get recalc and cursor flags
        //            fRecalc = GetMouseRecalcAndShowAPFlags(out uiCursorFlags, null);
        //
        //            // OK, if we begin to move, reset the cursor...
        //            if (uiCursorFlags & MOUSE_MOVING)
        //            {
        //                EndPhysicsTrajectoryUI();
        //            }
        //
        //            // Get Pyth spaces away.....
        //            sDist = PythSpacesAway(gpItemPointerSoldier.sGridNo, gusCurMousePos);
        //
        //
        //            // If we are here and we are not selected, select!
        //            // ATE Design discussion propably needed here...
        //            if (gpItemPointerSoldier.ubID != gusSelectedSoldier)
        //            {
        //                SelectSoldier(gpItemPointerSoldier.ubID, false, false);
        //            }
        //
        //            // ATE: if good for locate, locate to selected soldier....
        //            if (gbItemPointerLocateGood)
        //            {
        //                gbItemPointerLocateGood = false;
        //                LocateSoldier(gusSelectedSoldier, false);
        //            }
        //
        //            if (!fGiveItem)
        //            {
        //                if (UIHandleOnMerc(false) && usMapPos != gpItemPointerSoldier.sGridNo)
        //                {
        //                    // We are on a guy.. check if they can catch or not....
        //                    if (gfUIFullTargetFound)
        //                    {
        //                        // Get soldier
        //                        pSoldier = MercPtrs[gusUIFullTargetID];
        //
        //                        // Are they on our team?
        //                        // ATE: Can't be an EPC
        //                        if (pSoldier.bTeam == gbPlayerNum && !AM_AN_EPC(pSoldier) && !(pSoldier.uiStatusFlags.HasFlag(SOLDIER.VEHICLE)))
        //                        {
        //                            if (sDist <= PASSING_ITEM_DISTANCE_OKLIFE)
        //                            {
        //                                // OK, on a valid pass
        //                                gfUIMouseOnValidCatcher = 4;
        //                                gubUIValidCatcherID = gusUIFullTargetID;
        //                            }
        //                            else
        //                            {
        //                                // Can they see the throw?
        //                                if (SoldierCanSeeCatchComing(pSoldier, gpItemPointerSoldier.sGridNo))
        //                                {
        //                                    // OK, set global that this buddy can see catch...
        //                                    gfUIMouseOnValidCatcher = true;
        //                                    gubUIValidCatcherID = gusUIFullTargetID;
        //                                }
        //                            }
        //                        }
        //                    }
        //                }
        //
        //                // We're going to toss it!
        //                if (gTacticalStatus.uiFlags & INCOMBAT)
        //                {
        //                    gfUIDisplayActionPoints = true;
        //                    gUIDisplayActionPointsOffX = 15;
        //                    gUIDisplayActionPointsOffY = 15;
        //                }
        //
        //                // If we are tossing...
        //                if (sDist <= 1 && gfUIMouseOnValidCatcher == 0 || gfUIMouseOnValidCatcher == 4)
        //                {
        //                    gsCurrentActionPoints = AP.PICKUP_ITEM;
        //                }
        //                else
        //                {
        //                    gsCurrentActionPoints = AP.TOSS_ITEM;
        //                }
        //
        //            }
        //            else
        //            {
        //                if (gfUIFullTargetFound)
        //                {
        //                    UIHandleOnMerc(false);
        //
        //                    // OK, set global that this buddy can see catch...
        //                    gfUIMouseOnValidCatcher = 2;
        //                    gubUIValidCatcherID = (int)gusUIFullTargetID;
        //
        //                    // If this is a robot, change to say 'reload'
        //                    if (MercPtrs[gusUIFullTargetID].uiStatusFlags.HasFlag(SOLDIER.ROBOT))
        //                    {
        //                        gfUIMouseOnValidCatcher = 3;
        //                    }
        //
        //                    if (!(uiCursorFlags.HasFlag(MOUSE.MOVING)))
        //                    {
        //                        // Find adjacent gridno...
        //                        sActionGridNo = Overhead.FindAdjacentGridEx(gpItemPointerSoldier, gusCurMousePos, ref ubDirection, null, false, false);
        //                        if (sActionGridNo == -1)
        //                        {
        //                            sActionGridNo = gusCurMousePos;
        //                        }
        //
        //                        // Display location...
        //                        gsUIHandleShowMoveGridLocation = sActionGridNo;
        //                        gfUIHandleShowMoveGrid = 1;
        //
        //
        //                        // Get AP cost
        //                        if (MercPtrs[gusUIFullTargetID].uiStatusFlags.HasFlag(SOLDIER.ROBOT))
        //                        {
        //                            sAPCost = GetAPsToReloadRobot(gpItemPointerSoldier, MercPtrs[gusUIFullTargetID]);
        //                        }
        //                        else
        //                        {
        //                            sAPCost = GetAPsToGiveItem(gpItemPointerSoldier, sActionGridNo);
        //                        }
        //
        //                        gsCurrentActionPoints = sAPCost;
        //                    }
        //
        //                    // Set value
        //                    if (gTacticalStatus.uiFlags & INCOMBAT)
        //                    {
        //                        gfUIDisplayActionPoints = true;
        //                        gUIDisplayActionPointsOffX = 15;
        //                        gUIDisplayActionPointsOffY = 15;
        //                    }
        //                }
        //            }


        if (fGiveItem)
        {
            uiCursorId = CURSOR.ITEM_GIVE;
        }
        else
        {
            // How afar away are we?
            sDist = 0;
            if (sDist <= 1 && gfUIMouseOnValidCatcher == 0)
            {
                // OK, we want to drop.....

                // Write the word 'drop' on cursor...
                //                    wcscpy(gzIntTileLocation, pMessageStrings[MSG.DROP]);
                gfUIIntTileLocation = true;
            }
            else
            {
                usMapPos = 0;
                if (usMapPos == gpItemPointerSoldier.sGridNo)
                {
                    //                        EndPhysicsTrajectoryUI();
                }
                else if (gfUIMouseOnValidCatcher == 4)
                {
                    // ATE: Don't do if we are passing....
                }
                else
                // ( sDist > PASSING_ITEM_DISTANCE_OKLIFE )
                {
                    // Write the word 'drop' on cursor...
                    if (gfUIMouseOnValidCatcher == 0)
                    {
                        //                            wcscpy(gzIntTileLocation, pMessageStrings[MSG.THROW]);
                        gfUIIntTileLocation = true;
                    }

                    gfUIHandlePhysicsTrajectory = true;

                    //                        if (fRecalc && usMapPos != gpItemPointerSoldier.sGridNo)
                    {
                        //                            if (gfUIMouseOnValidCatcher)
                        {
                            switch (gAnimControl[MercPtrs[gubUIValidCatcherID].usAnimState].ubHeight)
                            {
                                case AnimationHeights.ANIM_STAND:

                                    sEndZ = 150;
                                    break;

                                case AnimationHeights.ANIM_CROUCH:

                                    sEndZ = 80;
                                    break;

                                case AnimationHeights.ANIM_PRONE:

                                    sEndZ = 10;
                                    break;
                            }

                            if (MercPtrs[gubUIValidCatcherID].bLevel > 0)
                            {
                                sEndZ = 0;
                            }
                        }

                        // Calculate chance to throw here.....
                        //                            if (!CalculateLaunchItemChanceToGetThrough(gpItemPointerSoldier, gpItemPointer, usMapPos, (int)gsInterfaceLevel, (int)((gsInterfaceLevel * 256) + sEndZ), out sFinalGridNo, false, out bLevel, true))
                        {
                            gfBadThrowItemCTGH = true;
                        }
                        //                            else
                        {
                            gfBadThrowItemCTGH = false;
                        }

                        //                            BeginPhysicsTrajectoryUI(sFinalGridNo, bLevel, gfBadThrowItemCTGH);
                    }
                }

                if (gfBadThrowItemCTGH)
                {
                    uiCursorId = CURSOR.ITEM_BAD_THROW;
                }
            }
        }

        //Erase any cursor in viewport
        //MSYS_ChangeRegionCursor( gViewportRegion , VIDEO_NO_CURSOR );	

        // Get tile graphic fro item
        //            usIndex = GetTileGraphicForItem((Item[gpItemPointer.usItem]));

        // ONly load if different....
        if (usIndex != gusItemPointer || uiOldCursorId != uiCursorId)
        {
            // OK, Tile database gives me subregion and video object to use...
            //                SetExternVOData(uiCursorId, gTileDatabase[usIndex].hTileSurface, gTileDatabase[usIndex].usRegionIndex);
            gusItemPointer = usIndex;
            uiOldCursorId = uiCursorId;
        }


        MouseSubSystem.MSYS_ChangeRegionCursor(gViewportRegion, uiCursorId);
    }


    bool IsValidAmmoToReloadRobot(SOLDIERTYPE pSoldier, OBJECTTYPE pObject)
    {
        //        if (!CompatibleAmmoForGun(pObject, (pSoldier.inv[InventorySlot.HANDPOS])))
        {
            // Build string...
            //            Messages.ScreenMsg(FontColor.FONT_MCOLOR_LTYELLOW, MSG.UI_FEEDBACK, TacticalStr[ROBOT_NEEDS_GIVEN_CALIBER_STR], AmmoCaliber[Weapon[pSoldier.inv[HANDPOS].usItem].ubCalibre]);

            return (false);
        }

        return (true);
    }


    bool HandleItemPointerClick(int usMapPos)
    {
        // Determine what to do
        WorldDirections ubDirection = 0;
        int ubSoldierID;
        Items usItem;
        int sAPCost;
        SOLDIERTYPE? pSoldier = null;
        int ubThrowActionCode = 0;
        int uiThrowActionData = 0;
        int sEndZ = 0;
        bool fGiveItem = false;
        OBJECTTYPE TempObject;
        int sGridNo;
        int sDist;
        int sDistVisible;


        if (HandleUI.SelectedGuyInBusyAnimation())
        {
            return (false);
        }

        if (giUIMessageOverlay != -1)
        {
            //            EndUIMessage();
            return (false);
        }

        // Don't allow if our soldier is a # of things...
        if (AM_AN_EPC(gpItemPointerSoldier) || gpItemPointerSoldier.bLife < OKLIFE || gpItemPointerSoldier.bOverTerrainType == TerrainTypeDefines.DEEP_WATER)
        {
            return (false);
        }

        // This implies we have no path....
        if (gsCurrentActionPoints == 0)
        {
            //            Messages.ScreenMsg(FontColor.FONT_MCOLOR_LTYELLOW, MSG.UI_FEEDBACK, TacticalStr[NO_PATH]);
            return (false);
        }


        if (gfUIFullTargetFound)
        {
            // Force mouse position to guy...
            usMapPos = MercPtrs[gusUIFullTargetID].sGridNo;

            if (gAnimControl[MercPtrs[gusUIFullTargetID].usAnimState].uiFlags.HasFlag(ANIM.MOVING))
            {
                return (false);
            }

        }

        // Check if we have APs....
//        if (!EnoughPoints(gpItemPointerSoldier, gsCurrentActionPoints, 0, true))
        {
            if (gfDontChargeAPsToPickup && gsCurrentActionPoints == AP.PICKUP_ITEM)
            {

            }
            else
            {
                return (false);
            }
        }

        // SEE IF WE ARE OVER A TALKABLE GUY!
        if (HandleUI.IsValidTalkableNPCFromMouse(out ubSoldierID, true, false, true))
        {
            fGiveItem = true;
        }

        // OK, if different than default, change....
        if (gfItemPointerDifferentThanDefault)
        {
            fGiveItem = !fGiveItem;
        }


        // Get Pyth spaces away.....
        sDist = IsometricUtils.PythSpacesAway(gpItemPointerSoldier.sGridNo, gusCurMousePos);


        if (fGiveItem)
        {
            usItem = gpItemPointer.usItem;

            // If the target is a robot, 
            if (MercPtrs[ubSoldierID].uiStatusFlags.HasFlag(SOLDIER.ROBOT))
            {
                // Charge APs to reload robot!
                //                sAPCost = GetAPsToReloadRobot(gpItemPointerSoldier, MercPtrs[ubSoldierID]);
            }
            else
            {
                // Calculate action point costs!
                //                sAPCost = GetAPsToGiveItem(gpItemPointerSoldier, usMapPos);
            }

            // Place it back in our hands!

            //            memcpy(&TempObject, gpItemPointer, sizeof(OBJECTTYPE));

            if (gbItemPointerSrcSlot != NO_SLOT)
            {
                //                PlaceObject(gpItemPointerSoldier, gbItemPointerSrcSlot, gpItemPointer);
                fInterfacePanelDirty = DIRTYLEVEL2;
            }
            /*
                    //if the user just clicked on an arms dealer
                    if( IsMercADealer( MercPtrs[ ubSoldierID ].ubProfile ) )
                    {
                        if ( EnoughPoints( gpItemPointerSoldier, sAPCost, 0, true ) )
                        {
                            //Enter the shopkeeper interface
                            EnterShopKeeperInterfaceScreen( MercPtrs[ ubSoldierID ].ubProfile );

                            EndItemPointer( );
                        }

                        return( true );
                    }
            */

            if (false)//EnoughPoints(gpItemPointerSoldier, sAPCost, 0, true))
            {
                // If we are a robot, check if this is proper item to reload!
                if (MercPtrs[ubSoldierID].uiStatusFlags.HasFlag(SOLDIER.ROBOT))
                {
                    // Check if we can reload robot....
                    if (IsValidAmmoToReloadRobot(MercPtrs[ubSoldierID], TempObject))
                    {
                        int sActionGridNo;
                        int sAdjustedGridNo;

                        // Walk up to him and reload!
                        // See if we can get there to stab	
                        sActionGridNo = Overhead.FindAdjacentGridEx(gpItemPointerSoldier, MercPtrs[ubSoldierID].sGridNo, ref ubDirection, out sAdjustedGridNo, true, false);

                        if (sActionGridNo != -1 && gbItemPointerSrcSlot != NO_SLOT)
                        {
                            // Make a temp object for ammo...
                            //                            gpItemPointerSoldier.pTempObject = MemAlloc(sizeof(OBJECTTYPE));
                            //                            memcpy(gpItemPointerSoldier.pTempObject, &TempObject, sizeof(OBJECTTYPE));

                            // Remove from soldier's inv...
                            //                            RemoveObjs((gpItemPointerSoldier.inv[gbItemPointerSrcSlot]), 1);

                            gpItemPointerSoldier.sPendingActionData2 = sAdjustedGridNo;
                            gpItemPointerSoldier.uiPendingActionData1 = (int)gbItemPointerSrcSlot;
                            gpItemPointerSoldier.bPendingActionData3 = ubDirection;
                            gpItemPointerSoldier.ubPendingActionAnimCount = 0;

                            // CHECK IF WE ARE AT THIS GRIDNO NOW
                            if (gpItemPointerSoldier.sGridNo != sActionGridNo)
                            {
                                // SEND PENDING ACTION
                                gpItemPointerSoldier.ubPendingAction = MERC.RELOADROBOT;

                                // WALK UP TO DEST FIRST
//                                SoldierControl.EVENT_InternalGetNewSoldierPath(gpItemPointerSoldier, sActionGridNo, gpItemPointerSoldier.usUIMovementMode, false, false);
                            }
                            else
                            {
                                SoldierControl.EVENT_SoldierBeginReloadRobot(gpItemPointerSoldier, sAdjustedGridNo, ubDirection, gbItemPointerSrcSlot);
                            }

                            // OK, set UI
                            //                            SetUIBusy(gpItemPointerSoldier.ubID);
                        }

                    }

                    gfDontChargeAPsToPickup = false;
                    EndItemPointer();
                }
                else
                {
                    //if (gbItemPointerSrcSlot != NO_SLOT )
                    {
                        // Give guy this item.....
                        //                        SoldierGiveItem(gpItemPointerSoldier, MercPtrs[ubSoldierID], out TempObject, gbItemPointerSrcSlot);

                        gfDontChargeAPsToPickup = false;
                        EndItemPointer();

                        // If we are giving it to somebody not on our team....
                        if (MercPtrs[ubSoldierID].ubProfile < FIRST_RPC || RPC_RECRUITED(MercPtrs[ubSoldierID]))
                        {

                        }
                        else
                        {
                            //                            SetEngagedInConvFromPCAction(gpItemPointerSoldier);
                        }
                    }
                }
            }

            return (true);
        }

        // CHECK IF WE ARE NOT ON THE SAME GRIDNO
        if (sDist <= 1 && !(gfUIFullTargetFound && gusUIFullTargetID != gpItemPointerSoldier.ubID))
        {
            // Check some things here....
            // 1 ) are we at the exact gridno that we stand on?
            if (usMapPos == gpItemPointerSoldier.sGridNo)
            {
                // Drop
                if (!gfDontChargeAPsToPickup)
                {
                    // Deduct points
                    //                    DeductPoints(gpItemPointerSoldier, AP.PICKUP_ITEM, 0);
                }

                //                SoldierDropItem(gpItemPointerSoldier, gpItemPointer);
            }
            else
            {
                // Try to drop in an adjacent area....
                // 1 ) is this not a good OK destination
                // this will sound strange, but this is OK......
                //                if (!NewOKDestination(gpItemPointerSoldier, usMapPos, false, gpItemPointerSoldier.bLevel) || FindBestPath(gpItemPointerSoldier, usMapPos, gpItemPointerSoldier.bLevel, WALKING, NO_COPYROUTE, 0) == 1)
                {
                    // Drop
                    if (!gfDontChargeAPsToPickup)
                    {
                        // Deduct points
                        //                        DeductPoints(gpItemPointerSoldier, AP.PICKUP_ITEM, 0);
                    }

                    // Play animation....
                    // Don't show animation of dropping item, if we are not standing



                    switch (gAnimControl[gpItemPointerSoldier.usAnimState].ubHeight)
                    {
                        case AnimationHeights.ANIM_STAND:

                            //                            gpItemPointerSoldier.pTempObject = MemAlloc(sizeof(OBJECTTYPE));
                            if (gpItemPointerSoldier.pTempObject != null)
                            {
                                //                                memcpy(gpItemPointerSoldier.pTempObject, gpItemPointer, sizeof(OBJECTTYPE));
                                gpItemPointerSoldier.sPendingActionData2 = usMapPos;

                                // Turn towards.....gridno	
                                //                                SoldierControl.EVENT_SetSoldierDesiredDirection(gpItemPointerSoldier, GetDirectionFromGridNo(usMapPos, gpItemPointerSoldier));

                                SoldierControl.EVENT_InitNewSoldierAnim(gpItemPointerSoldier, AnimationStates.DROP_ADJACENT_OBJECT, 0, false);
                            }
                            break;

                        case AnimationHeights.ANIM_CROUCH:
                        case AnimationHeights.ANIM_PRONE:

                            //                            AddItemToPool(usMapPos, gpItemPointer, 1, gpItemPointerSoldier.bLevel, 0, -1);
                            //                            NotifySoldiersToLookforItems();
                            break;
                    }
                }
                //                else
                {
                    // Drop in place...
                    if (!gfDontChargeAPsToPickup)
                    {
                        // Deduct points
                        //                        DeductPoints(gpItemPointerSoldier, AP.PICKUP_ITEM, 0);
                    }

                    //                    SoldierDropItem(gpItemPointerSoldier, gpItemPointer);
                }
            }
        }
        else
        {
            sGridNo = usMapPos;

            if (sDist <= PASSING_ITEM_DISTANCE_OKLIFE && gfUIFullTargetFound
                && MercPtrs[gusUIFullTargetID].bTeam == gbPlayerNum
                && !AM_AN_EPC(MercPtrs[gusUIFullTargetID])
                && !(MercPtrs[gusUIFullTargetID].uiStatusFlags.HasFlag(SOLDIER.VEHICLE)))
            {
                // OK, do the transfer...
                {
                    pSoldier = MercPtrs[gusUIFullTargetID];

                    {
                        // Change to inventory....
                        //gfSwitchPanel = true;
                        //gbNewPanel = SM_PANEL;
                        //gubNewPanelParam = (int)pSoldier.ubID;
                        //                        if (!EnoughPoints(pSoldier, 3, 0, true)
                        //                            || !EnoughPoints(gpItemPointerSoldier, 3, 0, true))
                        //                        {
                        //                            return (false);
                        //                        }

                        sDistVisible = OppList.DistanceVisible(pSoldier, WorldDirections.DIRECTION_IRRELEVANT, WorldDirections.DIRECTION_IRRELEVANT, gpItemPointerSoldier.sGridNo, gpItemPointerSoldier.bLevel);

                        // Check LOS....
                        if (!LOS.SoldierTo3DLocationLineOfSightTest(pSoldier, gpItemPointerSoldier.sGridNo, gpItemPointerSoldier.bLevel, 3, (int)sDistVisible, 1))
                        {
                            return (false);
                        }

                        // Charge AP values...
                        //                        DeductPoints(pSoldier, 3, 0);
                        //                        DeductPoints(gpItemPointerSoldier, 3, 0);

                        usItem = gpItemPointer.usItem;

                        // try to auto place object....
                        //                        if (AutoPlaceObject(pSoldier, gpItemPointer, true))
                        {
                            Messages.ScreenMsg(FontColor.FONT_MCOLOR_LTYELLOW, MSG.INTERFACE, pMessageStrings[MSG.ITEM_PASSED_TO_MERC], ShortItemNames[usItem], pSoldier.name);

                            // Check if it's the same now!
                            if (gpItemPointer.ubNumberOfObjects == 0)
                            {
                                EndItemPointer();
                            }

                            // OK, make guys turn towards each other and do animation...
                            {
                                WorldDirections ubFacingDirection = 0;

                                // Get direction to face.....
                                //                                ubFacingDirection = GetDirectionFromGridNo(gpItemPointerSoldier.sGridNo, pSoldier);

                                // Stop merc first....
                                SoldierControl.EVENT_StopMerc(pSoldier, pSoldier.sGridNo, pSoldier.bDirection);

                                // If we are standing only...
                                //                                if (gAnimControl[pSoldier.usAnimState].ubEndHeight == AnimationHeights.ANIM_STAND && !MercInWater(pSoldier))
                                {
                                    // Turn to face, then do animation....
                                    SoldierControl.EVENT_SetSoldierDesiredDirection(pSoldier, ubFacingDirection);
                                    pSoldier.fTurningUntilDone = true;
                                    pSoldier.usPendingAnimation = AnimationStates.PASS_OBJECT;
                                }

                                //                                if (gAnimControl[gpItemPointerSoldier.usAnimState].ubEndHeight == AnimationHeights.ANIM_STAND && !MercInWater(gpItemPointerSoldier))
                                {
                                    SoldierControl.EVENT_SetSoldierDesiredDirection(gpItemPointerSoldier, gOppositeDirection[ubFacingDirection]);
                                    gpItemPointerSoldier.fTurningUntilDone = true;
                                    gpItemPointerSoldier.usPendingAnimation = AnimationStates.PASS_OBJECT;
                                }
                            }

                            return (true);
                        }
                        // else
                        {
                            //                            Messages.ScreenMsg(FontColor.FONT_MCOLOR_LTYELLOW, MSG.INTERFACE, pMessageStrings[MSG.NO_ROOM_TO_PASS_ITEM], ShortItemNames[usItem], pSoldier.name);
                            return (false);
                        }
                    }
                }
            }
            else
            {
                // CHECK FOR VALID CTGH
                if (gfBadThrowItemCTGH)
                {
                    //                    Messages.ScreenMsg(FontColor.FONT_MCOLOR_LTYELLOW, MSG.UI_FEEDBACK, TacticalStr[CANNOT_THROW_TO_DEST_STR]);
                    return (false);
                }

                // Deduct points
                //DeductPoints( gpItemPointerSoldier, AP_TOSS_ITEM, 0 );
                gpItemPointerSoldier.fDontChargeTurningAPs = true;
                // Will be dome later....	

                //                ubThrowActionCode = NO_THROW_ACTION;

                // OK, CHECK FOR VALID THROW/CATCH
                // IF OVER OUR GUY...
                if (gfUIFullTargetFound)
                {
                    pSoldier = MercPtrs[gusUIFullTargetID];

                    if (pSoldier.bTeam == gbPlayerNum && pSoldier.bLife >= OKLIFE && !AM_AN_EPC(pSoldier) && !(pSoldier.uiStatusFlags.HasFlag(SOLDIER.VEHICLE)))
                    {
                        // OK, on our team, 

                        // How's our direction?
                        if (SoldierCanSeeCatchComing(pSoldier, gpItemPointerSoldier.sGridNo))
                        {
                            // Setup as being the catch target
                            //                            ubThrowActionCode = THROW_TARGET_MERC_CATCH;
                            uiThrowActionData = pSoldier.ubID;

                            sGridNo = pSoldier.sGridNo;

                            switch (gAnimControl[pSoldier.usAnimState].ubHeight)
                            {
                                case AnimationHeights.ANIM_STAND:

                                    sEndZ = 150;
                                    break;

                                case AnimationHeights.ANIM_CROUCH:

                                    sEndZ = 80;
                                    break;

                                case AnimationHeights.ANIM_PRONE:

                                    sEndZ = 10;
                                    break;
                            }

                            if (pSoldier.bLevel > 0)
                            {
                                sEndZ = 0;
                            }

                            // Get direction
                            //                            ubDirection = (int)GetDirectionFromGridNo(gpItemPointerSoldier.sGridNo, pSoldier);

                            // ATE: Goto stationary...
                            //                            SoldierGotoStationaryStance(pSoldier);

                            // Set direction to turn...
                            SoldierControl.EVENT_SetSoldierDesiredDirection(pSoldier, ubDirection);

                        }
                    }
                }


                // CHANGE DIRECTION AT LEAST
                //                ubDirection = (int)GetDirectionFromGridNo(sGridNo, gpItemPointerSoldier);
                SoldierControl.EVENT_SetSoldierDesiredDirection(gpItemPointerSoldier, ubDirection);
                gpItemPointerSoldier.fTurningUntilDone = true;

                // Increment attacker count...
                gTacticalStatus.ubAttackBusyCount++;
                //                DebugMsg(TOPIC_JA2, DBG_LEVEL_3, String("INcremtning ABC: Throw item to %d", gTacticalStatus.ubAttackBusyCount));


                // Given our gridno, throw grenate!
                //                CalculateLaunchItemParamsForThrow(gpItemPointerSoldier, sGridNo, gpItemPointerSoldier.bLevel, (int)((gsInterfaceLevel * 256) + sEndZ), gpItemPointer, 0, ubThrowActionCode, uiThrowActionData);

                // OK, goto throw animation
                //                HandleSoldierThrowItem(gpItemPointerSoldier, usMapPos);
            }
        }

        gfDontChargeAPsToPickup = false;
        EndItemPointer();


        return (true);
    }

    bool ItemCursorInLobRange(int usMapPos)
    {
        // Draw item depending on distance from buddy
        if (IsometricUtils.GetRangeFromGridNoDiff(usMapPos, gpItemPointerSoldier.sGridNo) > MIN_LOB_RANGE)
        {
            return (false);
        }
        else
        {
            return (true);
        }
    }




    bool InItemStackPopup()
    {
        return (gfInItemStackPopup);
    }


    bool InKeyRingPopup()
    {
        return (gfInKeyRingPopup);
    }

    bool InitItemStackPopup(SOLDIERTYPE pSoldier, InventorySlot ubPosition, int sInvX, int sInvY, int sInvWidth, int sInvHeight)
    {
        //        VOBJECT_DESC VObjectDesc;
        int sX, sY, sCenX, sCenY;
        Rectangle aRect;
        int ubLimit = 0;
        ETRLEObject? pTrav;
        HVOBJECT hVObject;
        int cnt;
        int usPopupWidth = 1;
        int sItemSlotWidth, sItemSlotHeight;


        // Set some globals
        gsItemPopupInvX = sInvX;
        gsItemPopupInvY = sInvY;
        gsItemPopupInvWidth = sInvWidth;
        gsItemPopupInvHeight = sInvHeight;


        gpItemPopupSoldier = pSoldier;


        // Determine # of items
        gpItemPopupObject = (pSoldier.inv[ubPosition]);
        //        ubLimit = ItemSlotLimit(gpItemPopupObject.usItem, ubPosition);

        // Return false if #objects not >1
        if (ubLimit < 1)
        {
            return (false);
        }

        if (guiCurrentItemDescriptionScreen == ScreenName.MAP_SCREEN)
        {
            if (ubLimit > 6)
            {
                ubLimit = 6;
            }
        }

        // Load graphics
        //        VObjectDesc.fCreateFlags = VOBJECT_CREATE_FROMFILE;
        //        strcpy(VObjectDesc.ImageFile, "INTERFACE\\extra_inventory.STI");
        //        CHECKF(AddVideoObject(&VObjectDesc, guiItemPopupBoxes));

        // Get size
        //        GetVideoObject(hVObject, guiItemPopupBoxes);
        //        pTrav = (hVObject.pETRLEObject[0]);
        //        usPopupWidth = pTrav.usWidth;

        // Determine position, height and width of mouse region, area
        GetSlotInvXY(ubPosition, out sX, out sY);
        GetSlotInvHeightWidth(ubPosition, out sItemSlotWidth, out sItemSlotHeight);

        // Get Width, Height
        gsItemPopupWidth = ubLimit * usPopupWidth;
        //        gsItemPopupHeight = pTrav.usHeight;
        gubNumItemPopups = ubLimit;

        // Calculate X,Y, first center
        sCenX = sX - ((gsItemPopupWidth / 2) + (sItemSlotWidth / 2));
        sCenY = sY;

        // Limit it to window for item desc
        if (sCenX < gsItemPopupInvX)
        {
            sCenX = gsItemPopupInvX;
        }
        if ((sCenX + gsItemPopupWidth) > (gsItemPopupInvX + gsItemPopupInvWidth))
        {
            sCenX = gsItemPopupInvX;
        }

        // Cap it at 0....
        if (sCenX < 0)
        {
            sCenX = 0;
        }

        // Set
        gsItemPopupX = sCenX;
        gsItemPopupY = sCenY;

        for (cnt = 0; cnt < gubNumItemPopups; cnt++)
        {
            // Build a mouse region here that is over any others.....
            MouseSubSystem.MSYS_DefineRegion(
                gItemPopupRegions[cnt], 
                new((sCenX + (cnt * usPopupWidth)),
                    sCenY,
                    (sCenX + ((cnt + 1) * usPopupWidth)),
                    (sCenY + gsItemPopupHeight)),
                MSYS_PRIORITY.HIGHEST,
                MSYS_NO_CURSOR,
                MSYS_NO_CALLBACK,
                ItemPopupRegionCallback);

            // Add region
            MouseSubSystem.MSYS_AddRegion(ref gItemPopupRegions[cnt]);
            MouseSubSystem.MSYS_SetRegionUserData(gItemPopupRegions[cnt], 0, cnt);

            //OK, for each item, set dirty text if applicable!
            //            SetRegionFastHelpText((gItemPopupRegions[cnt]), ItemNames[pSoldier.inv[ubPosition].usItem]);
            //            SetRegionHelpEndCallback((gItemPopupRegions[cnt]), HelpTextDoneCallback);
            gfItemPopupRegionCallbackEndFix = false;
        }


        // Build a mouse region here that is over any others.....
        MouseSubSystem.MSYS_DefineRegion(
            gItemPopupRegion, 
            new(gsItemPopupInvX, 
                gsItemPopupInvY, 
                (gsItemPopupInvX + gsItemPopupInvWidth), 
                (gsItemPopupInvY + gsItemPopupInvHeight)),
            MSYS_PRIORITY.HIGH,
            MSYS_NO_CURSOR,
            MSYS_NO_CALLBACK,
            ItemPopupFullRegionCallback);

        // Add region
        MouseSubSystem.MSYS_AddRegion(ref gItemPopupRegion);


        //Disable all faces
        //        SetAllAutoFacesInactive();


        fInterfacePanelDirty = DIRTYLEVEL2;

        //guiTacticalInterfaceFlags |= INTERFACE_NORENDERBUTTONS;


        gfInItemStackPopup = true;

        //	if ( !(guiTacticalInterfaceFlags & INTERFACE_MAPSCREEN ) )
        if (guiCurrentItemDescriptionScreen != ScreenName.MAP_SCREEN)
        {
            //            EnableSMPanelButtons(false, false);
        }

        //Reserict mouse cursor to panel
        //        aRect.Top = sInvY;
        //        aRect.Left = sInvX;
        //        aRect.Bottom = sInvY + sInvHeight;
        //        aRect.Right = sInvX + sInvWidth;

        //        RestrictMouseCursor(&aRect);

        return (true);
    }

    void EndItemStackPopupWithItemInHand()
    {
        if (gpItemPointer != null)
        {
            DeleteItemStackPopup();
        }
    }

    void RenderItemStackPopup(bool fFullRender)
    {
        ETRLEObject? pTrav;
        int usHeight, usWidth = 1;
        HVOBJECT hVObject;
        int cnt;
        int sX, sY, sNewX, sNewY;

        if (gfInItemStackPopup)
        {

            //Disable all faces
            //            SetAllAutoFacesInactive();

            // Shadow Area
            if (fFullRender)
            {
                //                ShadowVideoSurfaceRect(Surfaces.FRAME_BUFFER, gsItemPopupInvX, gsItemPopupInvY, gsItemPopupInvX + gsItemPopupInvWidth, gsItemPopupInvY + gsItemPopupInvHeight);
            }

        }
        // TAKE A LOOK AT THE VIDEO OBJECT SIZE ( ONE OF TWO SIZES ) AND CENTER!
        //        GetVideoObject(hVObject, guiItemPopupBoxes);
        //        pTrav = (hVObject.pETRLEObject[0]);
        //        usHeight = (int)pTrav.usHeight;
        //        usWidth = (int)pTrav.usWidth;


        for (cnt = 0; cnt < gubNumItemPopups; cnt++)
        {
            //            BltVideoObjectFromIndex(Surfaces.FRAME_BUFFER, guiItemPopupBoxes, 0, gsItemPopupX + (cnt * usWidth), gsItemPopupY, VO_BLT.SRCTRANSPARENCY, null);

            if (cnt < gpItemPopupObject.ubNumberOfObjects)
            {
                sX = (int)(gsItemPopupX + (cnt * usWidth) + 11);
                sY = (int)(gsItemPopupY + 3);

                //                INVRenderItem(Surfaces.FRAME_BUFFER, null, gpItemPopupObject, sX, sY, 29, 23, DIRTYLEVEL2, null, (int)RENDER_ITEM_NOSTATUS, false, 0);

                // Do status bar here...
                sNewX = (int)(gsItemPopupX + (cnt * usWidth) + 7);
                sNewY = gsItemPopupY + INV_BAR_DY + 3;
                //                DrawItemUIBarEx(gpItemPopupObject, (int)cnt, sNewX, sNewY, ITEM_BAR_WIDTH, ITEM_BAR_HEIGHT, Get16BPPColor(STATUS_BAR), Get16BPPColor(STATUS_BAR_SHADOW), true, FRAME_BUFFER);

            }
        }

        //RestoreExternBackgroundRect( gsItemPopupInvX, gsItemPopupInvY, gsItemPopupInvWidth, gsItemPopupInvHeight );
        //        InvalidateRegion(gsItemPopupInvX, gsItemPopupInvY, gsItemPopupInvX + gsItemPopupInvWidth, gsItemPopupInvY + gsItemPopupInvHeight);

    }

    void HandleItemStackPopup()
    {

    }


    void DeleteItemStackPopup()
    {
        int cnt;

        //Remove
        //        DeleteVideoObjectFromIndex(guiItemPopupBoxes);

        MouseSubSystem.MSYS_RemoveRegion(gItemPopupRegion);


        gfInItemStackPopup = false;

        for (cnt = 0; cnt < gubNumItemPopups; cnt++)
        {
            MouseSubSystem.MSYS_RemoveRegion(gItemPopupRegions[cnt]);
        }


        fInterfacePanelDirty = DIRTYLEVEL2;

        //guiTacticalInterfaceFlags &= (~INTERFACE_NORENDERBUTTONS);

        //	if ( !(guiTacticalInterfaceFlags & INTERFACE_MAPSCREEN ) )
        if (guiCurrentItemDescriptionScreen != ScreenName.MAP_SCREEN)
        {
            //            EnableSMPanelButtons(true, false);
        }

        //        FreeMouseCursor();

    }


    bool InitKeyRingPopup(SOLDIERTYPE pSoldier, int sInvX, int sInvY, int sInvWidth, int sInvHeight)
    {
        //        VOBJECT_DESC VObjectDesc;
        Rectangle aRect;
        ETRLEObject? pTrav;
        HVOBJECT hVObject;
        int cnt;
        int usPopupWidth, usPopupHeight;
        int ubSlotSimilarToKeySlot = 10;
        int sKeyRingItemWidth = 0;
        int sOffSetY = 0, sOffSetX = 0;

        if (guiCurrentScreen == ScreenName.MAP_SCREEN)
        {
            gsKeyRingPopupInvX = 0;
            sKeyRingItemWidth = MAP_KEY_RING_ROW_WIDTH;
            sOffSetX = 40;
            sOffSetY = 15;
        }
        else
        {
            // Set some globals
            gsKeyRingPopupInvX = sInvX + TACTICAL_INVENTORY_KEYRING_GRAPHIC_OFFSET_X;
            sKeyRingItemWidth = KEY_RING_ROW_WIDTH;
            sOffSetY = 8;
        }

        gsKeyRingPopupInvY = sInvY;
        gsKeyRingPopupInvWidth = sInvWidth;
        gsKeyRingPopupInvHeight = sInvHeight;


        gpItemPopupSoldier = pSoldier;

        //        // Load graphics
        //        VObjectDesc.fCreateFlags = VOBJECT_CREATE_FROMFILE;
        //        strcpy(VObjectDesc.ImageFile, "INTERFACE\\extra_inventory.STI");
        //        CHECKF(AddVideoObject(&VObjectDesc, guiItemPopupBoxes));

        // Get size
        //        GetVideoObject(&hVObject, guiItemPopupBoxes);
        //        pTrav = (hVObject.pETRLEObject[0]);
        //        usPopupWidth = pTrav.usWidth;
        //        usPopupHeight = pTrav.usHeight;

        // Determine position, height and width of mouse region, area
        //GetSlotInvHeightWidth( ubSlotSimilarToKeySlot, &sItemSlotWidth, &sItemSlotHeight );

        for (cnt = 0; cnt < NUMBER_KEYS_ON_KEYRING; cnt++)
        {
            // Build a mouse region here that is over any others.....
            //            MouseSubSystem.MSYS_DefineRegion(gKeyRingRegions[cnt],
            //                    (int)(gsKeyRingPopupInvX + (cnt % sKeyRingItemWidth * usPopupWidth) + sOffSetX), // top left
            //                    (int)(sInvY + sOffSetY + (cnt / sKeyRingItemWidth * usPopupHeight)), // top right
            //                    (int)(gsKeyRingPopupInvX + ((cnt % sKeyRingItemWidth) + 1) * usPopupWidth + sOffSetX), // bottom left
            //                    (int)(sInvY + ((cnt / sKeyRingItemWidth + 1) * usPopupHeight) + sOffSetY), // bottom right
            //                    MSYS_PRIORITY.HIGHEST,
            //                    MSYS_NO_CURSOR, MSYS_NO_CALLBACK, KeyRingSlotInvClickCallback);
            // Add region
            MouseSubSystem.MSYS_AddRegion(ref gKeyRingRegions[cnt]);
            MouseSubSystem.MSYS_SetRegionUserData(gKeyRingRegions[cnt], 0, cnt);
            //gfItemPopupRegionCallbackEndFix = false;
        }


        // Build a mouse region here that is over any others.....
        MouseSubSystem.MSYS_DefineRegion(
            gItemPopupRegion, new(sInvX, sInvY, (int)(sInvX + sInvWidth), (int)(sInvY + sInvHeight)), MSYS_PRIORITY.HIGH,
                             MSYS_NO_CURSOR, MSYS_NO_CALLBACK, ItemPopupFullRegionCallback);

        // Add region
        MouseSubSystem.MSYS_AddRegion(ref gItemPopupRegion);


        //Disable all faces
        //        SetAllAutoFacesInactive();


        fInterfacePanelDirty = DIRTYLEVEL2;

        //guiTacticalInterfaceFlags |= INTERFACE_NORENDERBUTTONS;


        //	if ( !(guiTacticalInterfaceFlags & INTERFACE_MAPSCREEN ) )
        if (guiCurrentItemDescriptionScreen != ScreenName.MAP_SCREEN)
        {
            //            EnableSMPanelButtons(false, false);
        }

        gfInKeyRingPopup = true;

        //Reserict mouse cursor to panel
        //        aRect.Top = sInvY;
        //        aRect.Left = sInvX;
        //        aRect.Bottom = sInvY + sInvHeight;
        //        aRect.Right = sInvX + sInvWidth;

        //        RestrictMouseCursor(&aRect);

        return (true);
    }


    void RenderKeyRingPopup(bool fFullRender)
    {
        ETRLEObject? pTrav;
        int usHeight, usWidth;
        HVOBJECT hVObject = new();
        int cnt;
        OBJECTTYPE pObject = new();
        int sKeyRingItemWidth = 0;
        int sOffSetY = 0, sOffSetX = 0;

        if (guiCurrentScreen != ScreenName.MAP_SCREEN)
        {
            sOffSetY = 8;
        }
        else
        {
            sOffSetX = 40;
            sOffSetY = 15;
        }

        if (gfInKeyRingPopup)
        {

            //Disable all faces
            //            SetAllAutoFacesInactive();

            // Shadow Area
            if (fFullRender)
            {
                //                ShadowVideoSurfaceRect(FRAME_BUFFER, 0, gsKeyRingPopupInvY, gsKeyRingPopupInvX + gsKeyRingPopupInvWidth, gsKeyRingPopupInvY + gsKeyRingPopupInvHeight);
            }

        }

        //memset(&pObject, 0, sizeof(OBJECTTYPE));

        pObject.usItem = Items.KEY_1;
        pObject.bStatus[0] = 100;

        // TAKE A LOOK AT THE VIDEO OBJECT SIZE ( ONE OF TWO SIZES ) AND CENTER!
        //        GetVideoObject(hVObject, guiItemPopupBoxes);
        pTrav = (hVObject.pETRLEObject[0]);
        //        usHeight = (int)pTrav.usHeight;
        //        usWidth = (int)pTrav.usWidth;

        if (guiCurrentScreen == ScreenName.MAP_SCREEN)
        {
            sKeyRingItemWidth = MAP_KEY_RING_ROW_WIDTH;
        }
        else
        {
            // Set some globals
            sKeyRingItemWidth = KEY_RING_ROW_WIDTH;
        }

        for (cnt = 0; cnt < NUMBER_KEYS_ON_KEYRING; cnt++)
        {
            //            BltVideoObjectFromIndex(FRAME_BUFFER, guiItemPopupBoxes, 0, (int)(gsKeyRingPopupInvX + (cnt % sKeyRingItemWidth * usWidth) + sOffSetX), (int)(gsKeyRingPopupInvY + sOffSetY + (cnt / sKeyRingItemWidth * usHeight)), VO_BLT_SRCTRANSPARENCY, null);
            //
            //            // will want to draw key here.. if there is one
            //            if ((gpItemPopupSoldier.pKeyRing[cnt].ubKeyID != INVALID_KEY_NUMBER) && (gpItemPopupSoldier.pKeyRing[cnt].ubNumber > 0))
            //            {
            //                pObject.ubNumberOfObjects = gpItemPopupSoldier.pKeyRing[cnt].ubNumber;
            //
            //                // show 100% status for each
            //                DrawItemUIBarEx(&pObject, 0, (int)(gsKeyRingPopupInvX + sOffSetX + (cnt % sKeyRingItemWidth * usWidth) + 7), (int)(gsKeyRingPopupInvY + sOffSetY + (cnt / sKeyRingItemWidth * usHeight) + 24)
            //                , ITEM_BAR_WIDTH, ITEM_BAR_HEIGHT, Get16BPPColor(STATUS_BAR), Get16BPPColor(STATUS_BAR_SHADOW), true, FRAME_BUFFER);
            //
            //                // set item type
            //                pObject.usItem = FIRST_KEY + LockTable[gpItemPopupSoldier.pKeyRing[cnt].ubKeyID].usKeyItem;
            //
            //                // render the item
            //                INVRenderItem(FRAME_BUFFER, null, &pObject, (int)(gsKeyRingPopupInvX + sOffSetX + (cnt % sKeyRingItemWidth * usWidth) + 8), (int)(gsKeyRingPopupInvY + sOffSetY + (cnt / sKeyRingItemWidth * usHeight)),
            //                    (int)(usWidth - 8), (int)(usHeight - 2), DIRTYLEVEL2, null, 0, 0, 0);
            //            }
            //
            //BltVideoObjectFromIndex( FRAME_BUFFER, guiItemPopupBoxes, 0, (int)(gsKeyRingPopupInvX + ( cnt % KEY_RING_ROW_WIDTH * usWidth ) ), ( int )( gsKeyRingPopupInvY + ( cnt / KEY_RING_ROW_WIDTH * usHeight ) ), VO_BLT_SRCTRANSPARENCY, null );


        }

        //RestoreExternBackgroundRect( gsItemPopupInvX, gsItemPopupInvY, gsItemPopupInvWidth, gsItemPopupInvHeight );
        //        InvalidateRegion(gsKeyRingPopupInvX, gsKeyRingPopupInvY, gsKeyRingPopupInvX + gsKeyRingPopupInvWidth, gsKeyRingPopupInvY + gsKeyRingPopupInvHeight);
    }


    void DeleteKeyRingPopup()
    {
        int cnt;

        if (gfInKeyRingPopup == false)
        {
            // done, 
            return;
        }

        //Remove
        //        DeleteVideoObjectFromIndex(guiItemPopupBoxes);

        MouseSubSystem.MSYS_RemoveRegion(gItemPopupRegion);


        gfInKeyRingPopup = false;

        for (cnt = 0; cnt < NUMBER_KEYS_ON_KEYRING; cnt++)
        {
            MouseSubSystem.MSYS_RemoveRegion(gKeyRingRegions[cnt]);
        }


        fInterfacePanelDirty = DIRTYLEVEL2;

        //guiTacticalInterfaceFlags &= (~INTERFACE_NORENDERBUTTONS);

        //	if ( !(guiTacticalInterfaceFlags & INTERFACE_MAPSCREEN ) )
        if (guiCurrentItemDescriptionScreen != ScreenName.MAP_SCREEN)
        {
//            EnableSMPanelButtons(true, false);
        }

//        FreeMouseCursor();
    }

    int GetInterfaceGraphicForItem(INVTYPE pItem)
    {
        return 0;
        // CHECK SUBCLASS
//        if (pItem.ubGraphicType == 0)
//        {
//            return (guiGUNSM);
//        }
//        else if (pItem.ubGraphicType == 1)
//        {
//            return (guiP1ITEMS);
//        }
//        else if (pItem.ubGraphicType == 2)
//        {
//            return (guiP2ITEMS);
//        }
//        else
//        {
//            return (guiP3ITEMS);
//        }

    }


    TileIndexes GetTileGraphicForItem(INVTYPE pItem)
    {
        TileIndexes usIndex;

        // CHECK SUBCLASS
        if (pItem.ubGraphicType == 0)
        {
            TileDefine.GetTileIndexFromTypeSubIndex(TileTypeDefines.GUNS, (int)(pItem.ubGraphicNum + 1), out usIndex);
        }
        else if (pItem.ubGraphicType == 1)
        {
            TileDefine.GetTileIndexFromTypeSubIndex(TileTypeDefines.P1ITEMS, (int)(pItem.ubGraphicNum + 1), out usIndex);
        }
        else if (pItem.ubGraphicType == 2)
        {
            TileDefine.GetTileIndexFromTypeSubIndex(TileTypeDefines.P2ITEMS, (int)(pItem.ubGraphicNum + 1), out usIndex);
        }
        else
        {
            TileDefine.GetTileIndexFromTypeSubIndex(TileTypeDefines.P3ITEMS, (int)(pItem.ubGraphicNum + 1), out usIndex);
        }
        return (usIndex);
    }


    bool LoadTileGraphicForItem(INVTYPE pItem, out int puiVo)
    {
        string zName;// [100];
        int uiVo = 0;
        //VOBJECT_DESC VObjectDesc;
        int ubGraphic;

        // CHECK SUBCLASS
        ubGraphic = pItem.ubGraphicNum;

        if (pItem.ubGraphicType == 0)
        {
            // CHECK SUBCLASS
            //ubGraphic++;

            if (ubGraphic < 10)
            {
                zName = sprintf("gun0%d.sti", ubGraphic);
            }
            else
            {
                zName = sprintf("gun%d.sti", ubGraphic);
            }
        }
        else if (pItem.ubGraphicType == 1)
        {
            if (ubGraphic < 10)
            {
                zName = sprintf("p1item0%d.sti", ubGraphic);
            }
            else
            {
                zName = sprintf("p1item%d.sti", ubGraphic);
            }
        }
        else if (pItem.ubGraphicType == 2)
        {
            if (ubGraphic < 10)
            {
                zName = sprintf("p2item0%d.sti", ubGraphic);
            }
            else
            {
                zName = sprintf("p2item%d.sti", ubGraphic);
            }
        }
        else
        {
            if (ubGraphic < 10)
            {
                zName = sprintf("p3item0%d.sti", ubGraphic);
            }
            else
            {
                zName = sprintf("p3item%d.sti", ubGraphic);
            }
        }

        //Load item
        //        VObjectDesc.fCreateFlags = VOBJECT_CREATE_FROMFILE;
        //        sprintf(VObjectDesc.ImageFile, "BIGITEMS\\%s", zName);
        //        CHECKF(AddVideoObject(VObjectDesc, uiVo));

        puiVo = uiVo;

        return (true);
    }

    void ItemDescMoveCallback(MOUSE_REGION pRegion, int iReason)
    {
    }

    static bool fLeftDown = false;
    void ItemDescCallback(ref MOUSE_REGION pRegion, MSYS_CALLBACK_REASON iReason)
    {

        if (iReason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_DWN))
        {
            fLeftDown = true;
        }
        else if (iReason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP))
        {
            if (fLeftDown)
            {
                fLeftDown = false;

                //Only exit the screen if we are NOT in the money interface.  Only the DONE button should exit the money interface.
                if (gpItemDescObject.usItem != Items.MONEY)
                {
                    DeleteItemDescriptionBox();
                }
            }
        }
        else if (iReason.HasFlag(MSYS_CALLBACK_REASON.RBUTTON_DWN))
        {
            fRightDown = true;
        }
        else if (iReason.HasFlag(MSYS_CALLBACK_REASON.RBUTTON_UP))
        {
            if (fRightDown)
            {
                fRightDown = false;

                //Only exit the screen if we are NOT in the money interface.  Only the DONE button should exit the money interface.
                //			if( gpItemDescObject.usItem != MONEY )
                {
                    DeleteItemDescriptionBox();
                }
            }
        }
    }

    void ItemDescDoneButtonCallback(GUI_BUTTON btn, MSYS_CALLBACK_REASON reason)
    {

        if (reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_DWN))
        {
            //            btn.uiFlags |= (BUTTON_CLICKED_ON);
        }
        else if (reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP))
        {
            //            if (btn.uiFlags & BUTTON_CLICKED_ON)
            //            {
            //                btn.uiFlags &= ~(BUTTON_CLICKED_ON);
            //
            //                if (gpItemDescObject.usItem == MONEY)
            //                {
            //                    RemoveMoney();
            //                }
            //
            //                DeleteItemDescriptionBox();
            //            }
        }

        if (reason.HasFlag(MSYS_CALLBACK_REASON.RBUTTON_DWN))
        {
            btn.uiFlags |= (ButtonFlags.BUTTON_CLICKED_ON);
        }
        else if (reason.HasFlag(MSYS_CALLBACK_REASON.RBUTTON_UP))
        {
            if (btn.uiFlags.HasFlag(ButtonFlags.BUTTON_CLICKED_ON))
            {
                btn.uiFlags &= ~(ButtonFlags.BUTTON_CLICKED_ON);
                DeleteItemDescriptionBox();
            }
        }
    }


    void ItemPopupRegionCallback(ref MOUSE_REGION pRegion, MSYS_CALLBACK_REASON iReason)
    {
        int uiItemPos;

        uiItemPos = (int)MouseSubSystem.MSYS_GetRegionUserData(ref pRegion, 0);

        // TO ALLOW ME TO DELETE REGIONS IN CALLBACKS!
        if (gfItemPopupRegionCallbackEndFix)
        {
            return;
        }

        if (iReason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_DWN))
        {

            //If one in our hand, place it
            if (gpItemPointer != null)
            {
//                if (!PlaceObjectAtObjectIndex(gpItemPointer, gpItemPopupObject, (int)uiItemPos))
//                {
//                    if ((guiTacticalInterfaceFlags.HasFlag(INTERFACE.MAPSCREEN)))
//                    {
//                        //                        MAPEndItemPointer();
//                    }
//                    else
//                    {
//                        gpItemPointer = null;
////                        MouseSubSystem.MSYS_ChangeRegionCursor(gSMPanelRegion, CURSOR.NORMAL);
////                        SetCurrentCursorFromDatabase(CURSOR.NORMAL);
//
//                        if (guiTacticalInterfaceFlags.HasFlag(INTERFACE.SHOPKEEP_INTERFACE))
//                        {
//                            // memset(gMoveingItem, 0, sizeof(INVENTORY_IN_SLOT));
////                            SetSkiCursor(CURSOR.NORMAL);
//                        }
//                    }
//
//                    // re-evaluate repairs
////                    gfReEvaluateEveryonesNothingToDo = true;
//                }

                //Dirty interface
                //fInterfacePanelDirty = DIRTYLEVEL2;
                //RenderItemStackPopup( false );
            }
            else
            {
                if (uiItemPos < gpItemPopupObject.ubNumberOfObjects)
                {
                    // Here, grab an item and put in cursor to swap
                    //RemoveObjFrom( OBJECTTYPE * pObj, int ubRemoveIndex )
//                    GetObjFrom(gpItemPopupObject, (int)uiItemPos, gItemPointer);

                    if ((guiTacticalInterfaceFlags.HasFlag(INTERFACE.MAPSCREEN)))
                    {
                        // pick it up
//                        InternalMAPBeginItemPointer(gpItemPopupSoldier);
                    }
                    else
                    {
                        gpItemPointer = gItemPointer;
                        gpItemPointerSoldier = gpItemPopupSoldier;
                    }

                    //if we are in the shop keeper interface
//                    if (guiTacticalInterfaceFlags & INTERFACE_SHOPKEEP_INTERFACE)
//                    {
//                        // pick up stacked item into cursor and try to sell it ( unless CTRL is held down )
//                        BeginSkiItemPointer(PLAYERS_INVENTORY, -1, (bool)!gfKeyState[CTRL]);
//
//                        // if we've just removed the last one there
//                        if (gpItemPopupObject.ubNumberOfObjects == 0)
//                        {
//                            // we must immediately get out of item stack popup, because the item has been deleted (memset to 0), and
//                            // errors like a right bringing up an item description for item 0 could happen then.  ARM.
//                            DeleteItemStackPopup();
//                        }
//                    }

                    // re-evaluate repairs
//                    gfReEvaluateEveryonesNothingToDo = true;

                    //Dirty interface
                    //RenderItemStackPopup( false );
                    //fInterfacePanelDirty = DIRTYLEVEL2;
                }
            }

            UpdateItemHatches();
        }
        else if (iReason.HasFlag(MSYS_CALLBACK_REASON.RBUTTON_UP))
        {
            // Get Description....
            // Some global stuff here - for esc, etc
            //Remove
            gfItemPopupRegionCallbackEndFix = true;


            DeleteItemStackPopup();

            if (!InItemDescriptionBox())
            {
                // RESTORE BACKGROUND
//                RestoreExternBackgroundRect(gsItemPopupInvX, gsItemPopupInvY, gsItemPopupInvWidth, gsItemPopupInvHeight);
                if (guiCurrentItemDescriptionScreen == ScreenName.MAP_SCREEN)
                {
//                    MAPInternalInitItemDescriptionBox(gpItemPopupObject, (int)uiItemPos, gpItemPopupSoldier);
                }
                else
                {
                    InternalInitItemDescriptionBox(gpItemPopupObject, (int)ITEMDESC_START_X, (int)ITEMDESC_START_Y, (int)uiItemPos, gpItemPopupSoldier);
                }
            }


        }
    }

    void ItemPopupFullRegionCallback(ref MOUSE_REGION pRegion, MSYS_CALLBACK_REASON iReason)
    {
        int uiItemPos;

        uiItemPos = (int)MouseSubSystem.MSYS_GetRegionUserData(ref pRegion, 0);

        if (iReason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP))
        {
            if (InItemStackPopup())
            {
                // End stack popup and retain pointer
                EndItemStackPopupWithItemInHand();
            }
            else if (InKeyRingPopup())
            {
                // end pop up with key in hand
                DeleteKeyRingPopup();
                fTeamPanelDirty = true;

            }
        }
        else if (iReason.HasFlag(MSYS_CALLBACK_REASON.RBUTTON_UP))
        {
            if (InItemStackPopup())
            {
                DeleteItemStackPopup();
                fTeamPanelDirty = true;
            }
            else
            {
                DeleteKeyRingPopup();
                fTeamPanelDirty = true;
            }
        }
    }


    // STUFF FOR POPUP ITEM INFO BOX
    void SetItemPickupMenuDirty(int fDirtyLevel)
    {
        gItemPickupMenu.fDirtyLevel = fDirtyLevel;
    }


    bool InitializeItemPickupMenu(SOLDIERTYPE pSoldier, int sGridNo, ITEM_POOL pItemPool, int sScreenX, int sScreenY, int bZLevel)
    {
//        VOBJECT_DESC VObjectDesc;
        string ubString;
        ITEM_POOL? pTempItemPool;
        int cnt;
        int sCenX, sCenY, sX, sY, sCenterYVal;

        // Erase other menus....
//        EraseInterfaceMenus(true);

        // Make sure menu is located if not on screen
//        LocateSoldier(pSoldier.ubID, false);

        // memset values
        //memset(gItemPickupMenu, 0, sizeof(gItemPickupMenu));

        //Set item pool value
        gItemPickupMenu.pItemPool = pItemPool;

//        InterruptTime();
//        PauseGame();
//        LockPauseState(18);
        // Pause timers as well....
//        PauseTime(true);


        // Alrighty, cancel lock UI if we havn't done so already
//        UnSetUIBusy(pSoldier.ubID);

        // Change to INV panel if not there already...
//        gfSwitchPanel = true;
//        gbNewPanel = SM_PANEL;
//        gubNewPanelParam = (int)pSoldier.ubID;

        //Determine total #
        cnt = 0;
        pTempItemPool = pItemPool;
        while (pTempItemPool != null)
        {
//            if (ItemPoolOKForDisplay(pTempItemPool, bZLevel))
            {
                cnt++;
            }

            pTempItemPool = pTempItemPool.pNext;
        }
        gItemPickupMenu.ubTotalItems = (int)cnt;

        // Determine # of slots per page
        if (gItemPickupMenu.ubTotalItems > NUM_PICKUP_SLOTS)
        {
            gItemPickupMenu.bNumSlotsPerPage = NUM_PICKUP_SLOTS;
        }
        else
        {
            gItemPickupMenu.bNumSlotsPerPage = gItemPickupMenu.ubTotalItems;
        }

//        VObjectDesc.fCreateFlags = VOBJECT_CREATE_FROMFILE;
//        FilenameForBPP("INTERFACE\\itembox.sti", VObjectDesc.ImageFile);
//        CHECKF(AddVideoObject(&VObjectDesc, (gItemPickupMenu.uiPanelVo)));

        // Memalloc selection array...
//        gItemPickupMenu.pfSelectedArray = MemAlloc((sizeof(int) * gItemPickupMenu.ubTotalItems));
        // seto to 0
//        memset(gItemPickupMenu.pfSelectedArray, 0, (sizeof(int) * gItemPickupMenu.ubTotalItems));

        // Calcualate dimensions
        CalculateItemPickupMenuDimensions();

        // Get XY
        {
            // First get mouse xy screen location
            if (sGridNo != NOWHERE)
            {
                sX = gusMouseXPos;
                sY = gusMouseYPos;
            }
            else
            {
                sX = sScreenX;
                sY = sScreenY;
            }

            // CHECK FOR LEFT/RIGHT
            if ((sX + gItemPickupMenu.sWidth) > 640)
            {
                sX = 640 - gItemPickupMenu.sWidth - ITEMPICK_START_X_OFFSET;
            }
            else
            {
                sX = sX + ITEMPICK_START_X_OFFSET;
            }

            // Now check for top
            // Center in the y
            sCenterYVal = gItemPickupMenu.sHeight / 2;

            sY -= sCenterYVal;

            if (sY < gsVIEWPORT_WINDOW_START_Y)
            {
                sY = gsVIEWPORT_WINDOW_START_Y;
            }

            // Check for bottom
            if ((sY + gItemPickupMenu.sHeight) > 340)
            {
                sY = 340 - gItemPickupMenu.sHeight;
            }

        }

        // Set some values
        gItemPickupMenu.sX = sX;
        gItemPickupMenu.sY = sY;
        gItemPickupMenu.bCurSelect = 0;
        gItemPickupMenu.pSoldier = pSoldier;
        gItemPickupMenu.fHandled = false;
        gItemPickupMenu.sGridNo = sGridNo;
        gItemPickupMenu.bZLevel = bZLevel;
        gItemPickupMenu.fAtLeastOneSelected = false;
        gItemPickupMenu.fAllSelected = false;

        //Load images for buttons
//        FilenameForBPP("INTERFACE\\itembox.sti", ubString);
//        gItemPickupMenu.iUpButtonImages = LoadButtonImage(ubString, -1, 5, -1, 10, -1);
//        gItemPickupMenu.iDownButtonImages = UseLoadedButtonImage(gItemPickupMenu.iUpButtonImages, -1, 7, -1, 12, -1);
//        gItemPickupMenu.iAllButtonImages = UseLoadedButtonImage(gItemPickupMenu.iUpButtonImages, -1, 6, -1, 11, -1);
//        gItemPickupMenu.iCancelButtonImages = UseLoadedButtonImage(gItemPickupMenu.iUpButtonImages, -1, 8, -1, 13, -1);
//        gItemPickupMenu.iOKButtonImages = UseLoadedButtonImage(gItemPickupMenu.iUpButtonImages, -1, 4, -1, 9, -1);


        // Build a mouse region here that is over any others.....
        MouseSubSystem.MSYS_DefineRegion(
            (gItemPickupMenu.BackRegion),
            new((532), (367), (640), (480)),
            MSYS_PRIORITY.HIGHEST,
            CURSOR.NORMAL,
            MSYS_NO_CALLBACK,
            MSYS_NO_CALLBACK);

        // Add region
        MouseSubSystem.MSYS_AddRegion(ref (gItemPickupMenu.BackRegion));


        // Build a mouse region here that is over any others.....
        MouseSubSystem.MSYS_DefineRegion(
            (gItemPickupMenu.BackRegions),
            new((gItemPickupMenu.sX),
                (gItemPickupMenu.sY),
                (gItemPickupMenu.sX + gItemPickupMenu.sWidth),
                (gItemPickupMenu.sY + gItemPickupMenu.sHeight)),
            MSYS_PRIORITY.HIGHEST,
            CURSOR.NORMAL,
            MSYS_NO_CALLBACK,
            MSYS_NO_CALLBACK);

        // Add region
        MouseSubSystem.MSYS_AddRegion(ref (gItemPickupMenu.BackRegions));


        // Create buttons
        if (gItemPickupMenu.bNumSlotsPerPage == NUM_PICKUP_SLOTS && gItemPickupMenu.ubTotalItems > NUM_PICKUP_SLOTS)
        {
            //            gItemPickupMenu.iUpButton = QuickCreateButton(gItemPickupMenu.iUpButtonImages, (int)(sX + ITEMPICK_UP_X), (int)(sY + gItemPickupMenu.sButtomPanelStartY + ITEMPICK_UP_Y),
            //                                              BUTTON_TOGGLE, MSYS_PRIORITY.HIGHEST,
            //                                              DEFAULT_MOVE_CALLBACK, (GUI_CALLBACK)ItemPickupScrollUp);

            //            SetButtonFastHelpText(gItemPickupMenu.iUpButton, ItemPickupHelpPopup[1]);


            //            gItemPickupMenu.iDownButton = QuickCreateButton(gItemPickupMenu.iDownButtonImages, (int)(sX + ITEMPICK_DOWN_X), (int)(sY + gItemPickupMenu.sButtomPanelStartY + ITEMPICK_DOWN_Y),
            //                                              BUTTON_TOGGLE, MSYS_PRIORITY.HIGHEST,
            //                                              DEFAULT_MOVE_CALLBACK, (GUI_CALLBACK)ItemPickupScrollDown);

            //            SetButtonFastHelpText(gItemPickupMenu.iDownButton, ItemPickupHelpPopup[3]);

        }


//        gItemPickupMenu.iOKButton = QuickCreateButton(gItemPickupMenu.iOKButtonImages, (int)(sX + ITEMPICK_OK_X), (int)(sY + gItemPickupMenu.sButtomPanelStartY + ITEMPICK_OK_Y),
//                                              ButtonFlags.BUTTON_TOGGLE, MSYS_PRIORITY.HIGHEST,
//                                              DEFAULT_MOVE_CALLBACK, (GUI_CALLBACK)ItemPickupOK);
//        SetButtonFastHelpText(gItemPickupMenu.iOKButton, ItemPickupHelpPopup[0]);
//
//
//        gItemPickupMenu.iAllButton = QuickCreateButton(gItemPickupMenu.iAllButtonImages, (int)(sX + ITEMPICK_ALL_X), (int)(sY + gItemPickupMenu.sButtomPanelStartY + ITEMPICK_ALL_Y),
//                                              ButtonFlags.BUTTON_TOGGLE, MSYS_PRIORITY.HIGHEST,
//                                              DEFAULT_MOVE_CALLBACK, (GUI_CALLBACK)ItemPickupAll);
//        SetButtonFastHelpText(gItemPickupMenu.iAllButton, ItemPickupHelpPopup[2]);
//
//        gItemPickupMenu.iCancelButton = QuickCreateButton(gItemPickupMenu.iCancelButtonImages, (int)(sX + ITEMPICK_CANCEL_X), (int)(sY + gItemPickupMenu.sButtomPanelStartY + ITEMPICK_CANCEL_Y),
//                                              ButtonFlags.BUTTON_TOGGLE, MSYS_PRIORITY.HIGHEST,
//                                              DEFAULT_MOVE_CALLBACK, (GUI_CALLBACK)ItemPickupCancel);
//        SetButtonFastHelpText(gItemPickupMenu.iCancelButton, ItemPickupHelpPopup[4]);


//        DisableButton(gItemPickupMenu.iOKButton);



        // Create regions...
        sCenX = gItemPickupMenu.sX;
        sCenY = gItemPickupMenu.sY + ITEMPICK_GRAPHIC_Y;

        for (cnt = 0; cnt < gItemPickupMenu.bNumSlotsPerPage; cnt++)
        {
            // Build a mouse region here that is over any others.....
            MouseSubSystem.MSYS_DefineRegion(
                (gItemPickupMenu.Regions[cnt]),
                new((sCenX),
                    (sCenY + 1),
                    (sCenX + gItemPickupMenu.sWidth),
                    (sCenY + ITEMPICK_GRAPHIC_YSPACE)),
                MSYS_PRIORITY.HIGHEST,
                CURSOR.NORMAL,
                ItemPickMenuMouseMoveCallback,
                ItemPickMenuMouseClickCallback);

            // Add region
            MouseSubSystem.MSYS_AddRegion(ref (gItemPickupMenu.Regions[cnt]));
            MouseSubSystem.MSYS_SetRegionUserData((gItemPickupMenu.Regions[cnt]), 0, cnt);

            sCenY += ITEMPICK_GRAPHIC_YSPACE;
        }

        //Save dirty rect
        //gItemPickupMenu.iDirtyRect = RegisterBackgroundRect( BGND_FLAG_PERMANENT | BGND_FLAG_SAVERECT, null, gItemPickupMenu.sX, gItemPickupMenu.sY, (int)(gItemPickupMenu.sX + gItemPickupMenu.sWidth ) , (int)(gItemPickupMenu.sY + gItemPickupMenu.sHeight ) );


        SetupPickupPage(0);

        gfInItemPickupMenu = true;

        // Ignore scrolling
        gfIgnoreScrolling = true;

//        HandleAnyMercInSquadHasCompatibleStuff((int)CurrentSquad(), null, true);
//        gubSelectSMPanelToMerc = pSoldier.ubID;
//        ReEvaluateDisabledINVPanelButtons();
//        DisableTacticalTeamPanelButtons(true);

        //gfSMDisableForItems = true;
        return (true);

    }

    void SetupPickupPage(int bPage)
    {
        int cnt, iStart, iEnd;
        ITEM_POOL? pTempItemPool;
        int sValue;
        OBJECTTYPE pObject;

        // Zero out page slots
        // memset(gItemPickupMenu.ItemPoolSlots, 0, sizeof(gItemPickupMenu.ItemPoolSlots));

        // Zero page flags
        gItemPickupMenu.fCanScrollUp = false;
        gItemPickupMenu.fCanScrollDown = false;

        // Get lower bound
        iStart = bPage * NUM_PICKUP_SLOTS;
        if (iStart > gItemPickupMenu.ubTotalItems)
        {
            return;
        }

        if (bPage > 0)
        {
            gItemPickupMenu.fCanScrollUp = true;
        }


        iEnd = iStart + NUM_PICKUP_SLOTS;
        if (iEnd >= gItemPickupMenu.ubTotalItems)
        {
            iEnd = gItemPickupMenu.ubTotalItems;
        }
        else
        {
            // We can go for more!
            gItemPickupMenu.fCanScrollDown = true;
        }

        // Setup slots!
        // These slots contain an inventory pool pointer for each slot...
        pTempItemPool = gItemPickupMenu.pItemPool;

        // ATE: Patch fix here for crash :(
        // Clear help text!
        for (cnt = 0; cnt < NUM_PICKUP_SLOTS; cnt++)
        {
//            SetRegionFastHelpText((gItemPickupMenu.Regions[cnt]), "");
        }

        for (cnt = 0; cnt < iEnd;)
        {
            // Move to the closest one that can be displayed....
//            while (!ItemPoolOKForDisplay(pTempItemPool, gItemPickupMenu.bZLevel))
            {
                pTempItemPool = pTempItemPool.pNext;
            }

            if (cnt >= iStart)
            {
                gItemPickupMenu.ItemPoolSlots[cnt - iStart] = pTempItemPool;

                pObject = (gWorldItems[pTempItemPool.iItemIndex].o);

                sValue = pObject.bStatus[0];

                // Adjust for ammo, other thingys..
                if (Item[pObject.usItem].usItemClass.HasFlag(IC.AMMO) || Item[pObject.usItem].usItemClass.HasFlag(IC.KEY))
                {
                    wprintf(pStr, "");
                }
                else
                {
                    wprintf(pStr, "%d%%", sValue);
                }

//                SetRegionFastHelpText((gItemPickupMenu.Regions[cnt - iStart]), pStr);
            }

            cnt++;

            pTempItemPool = pTempItemPool.pNext;
        }

        gItemPickupMenu.bScrollPage = bPage;
        gItemPickupMenu.ubScrollAnchor = (int)iStart;

        if (gItemPickupMenu.bNumSlotsPerPage == NUM_PICKUP_SLOTS && gItemPickupMenu.ubTotalItems > NUM_PICKUP_SLOTS)
        {
            // Setup enabled/disabled buttons
            if (gItemPickupMenu.fCanScrollUp)
            {
//                EnableButton(gItemPickupMenu.iUpButton);
            }
            else
            {
//                DisableButton(gItemPickupMenu.iUpButton);
            }

            // Setup enabled/disabled buttons
            if (gItemPickupMenu.fCanScrollDown)
            {
//                EnableButton(gItemPickupMenu.iDownButton);
            }
            else
            {
//                DisableButton(gItemPickupMenu.iDownButton);
            }
        }
        SetItemPickupMenuDirty(DIRTYLEVEL2);

    }


    void CalculateItemPickupMenuDimensions()
    {
        int cnt;
        int sX, sY;
        int usSubRegion, usHeight, usWidth;

        // Build background
        sX = 0;
        sY = 0;

        for (cnt = 0; cnt < gItemPickupMenu.bNumSlotsPerPage; cnt++)
        {
            if (cnt == 0)
            {
                usSubRegion = 0;
            }
            else
            {
                usSubRegion = 1;
            }

            // Add hieght of object
            //            GetVideoObjectETRLESubregionProperties(gItemPickupMenu.uiPanelVo, usSubRegion, &usWidth, &usHeight);

            //            sY += usHeight;

        }
        gItemPickupMenu.sButtomPanelStartY = sY;

        // Do end
        //        GetVideoObjectETRLESubregionProperties(gItemPickupMenu.uiPanelVo, 2, &usWidth, &usHeight);

        //        sY += usHeight;

        // Set height, width
        gItemPickupMenu.sHeight = sY;
        //        gItemPickupMenu.sWidth = usWidth;

    }


    // set pick up menu dirty level
    void SetPickUpMenuDirtyLevel(int fDirtyLevel)
    {
        gItemPickupMenu.fDirtyLevel = fDirtyLevel;

        return;
    }


    void RenderItemPickupMenu()
    {
        int cnt;
        TileIndexes usItemTileIndex;
        int sX, sY, sCenX, sCenY, sFontX, sFontY, sNewX, sNewY;
        int uiDestPitchBYTES;
        int? pDestBuf;
        string pStr = string.Empty;// [100];
        int usSubRegion, usHeight = 1, usWidth;
        INVTYPE? pItem;
        OBJECTTYPE pObject;
        int uiStringLength;

        if (!gfInItemPickupMenu)
        {
            return;
        }


        // Do everything!
        if (gItemPickupMenu.fDirtyLevel == DIRTYLEVEL2)
        {
            //            MarkButtonsDirty();

            // Build background
            sX = gItemPickupMenu.sX;
            sY = gItemPickupMenu.sY;

            for (cnt = 0; cnt < gItemPickupMenu.bNumSlotsPerPage; cnt++)
            {
                if (cnt == 0)
                {
                    usSubRegion = 0;
                }
                else
                {
                    usSubRegion = 1;
                }

                //                BltVideoObjectFromIndex(FRAME_BUFFER, gItemPickupMenu.uiPanelVo, usSubRegion, sX, sY, VO_BLT_SRCTRANSPARENCY, null);

                // Add hieght of object
                //                GetVideoObjectETRLESubregionProperties(gItemPickupMenu.uiPanelVo, usSubRegion, &usWidth, &usHeight);

                sY += usHeight;

            }

            // Do end
            if (gItemPickupMenu.bNumSlotsPerPage == NUM_PICKUP_SLOTS && gItemPickupMenu.ubTotalItems > NUM_PICKUP_SLOTS)
            {
                //                BltVideoObjectFromIndex(FRAME_BUFFER, gItemPickupMenu.uiPanelVo, 2, sX, sY, VO_BLT_SRCTRANSPARENCY, null);
            }
            else
            {
                //                BltVideoObjectFromIndex(Surfaces.FRAME_BUFFER, gItemPickupMenu.uiPanelVo, 3, sX, sY, VO_BLT.SRCTRANSPARENCY, null);
            }

            // Render items....
            sX = ITEMPICK_GRAPHIC_X + gItemPickupMenu.sX;
            sY = ITEMPICK_GRAPHIC_Y + gItemPickupMenu.sY;

            //            pDestBuf = LockVideoSurface(Surfaces.FRAME_BUFFER, out uiDestPitchBYTES);

            FontSubSystem.SetFont(ITEMDESC_FONT);
            FontSubSystem.SetFontBackground(FontColor.FONT_MCOLOR_BLACK);
            FontSubSystem.SetFontShadow(ITEMDESC_FONTSHADOW2);

            for (cnt = 0; cnt < gItemPickupMenu.bNumSlotsPerPage; cnt++)
            {
                if (gItemPickupMenu.ItemPoolSlots[cnt] != null)
                {
                    // Get item to render
                    pObject = (gWorldItems[gItemPickupMenu.ItemPoolSlots[cnt].iItemIndex].o);
                    pItem = (Item[pObject.usItem]);

                    usItemTileIndex = GetTileGraphicForItem(pItem);

                    // Render
                    sX = ITEMPICK_GRAPHIC_X + gItemPickupMenu.sX;

                    sCenX = sX;
                    sCenY = sY;

                    // ATE: Adjust to basic shade.....
                    //                    gTileDatabase[usItemTileIndex].hTileSurface.pShadeCurrent = gTileDatabase[usItemTileIndex].hTileSurface.pShades[4];

                    //else
                    {
                        //                        if (gItemPickupMenu.pfSelectedArray[cnt + gItemPickupMenu.ubScrollAnchor])
                        {
                            //SetFontForeground( FONT_MCOLOR_LTYELLOW );
                            //SetFontShadow( ITEMDESC_FONTSHADOW2 );
                            //                            Blt8BPPDataTo16BPPBufferOutline((int)pDestBuf, uiDestPitchBYTES, gTileDatabase[usItemTileIndex].hTileSurface, sCenX, sCenY, gTileDatabase[usItemTileIndex].usRegionIndex, Get16BPPColor(FROMRGB(255, 255, 0)), true);
                        }
                        //                        else
                        {
                            //SetFontForeground( FONT_BLACK );
                            //SetFontShadow( ITEMDESC_FONTSHADOW2 );
                            //                            Blt8BPPDataTo16BPPBufferOutline(pDestBuf, uiDestPitchBYTES, gTileDatabase[usItemTileIndex].hTileSurface, sCenX, sCenY, gTileDatabase[usItemTileIndex].usRegionIndex, 0, false);
                        }
                    }

                    // Draw text.....
                    FontSubSystem.SetFont(ITEM_FONT);
                    if (pObject.ubNumberOfObjects > 1)
                    {
                        FontSubSystem.SetFontForeground(FontColor.FONT_GRAY4);
                        FontSubSystem.SetFontShadow(FontShadow.DEFAULT_SHADOW);

                        sCenX = sX - 4;
                        sCenY = sY + 14;

                        //                        wprintf(pStr, "%d", pObject.ubNumberOfObjects);
                        //
                        //                        VarFindFontRightCoordinates(sCenX, sCenY, 42, 1, ITEM_FONT, out sFontX, out sFontY, pStr);
                        //                        mprintf_buffer(pDestBuf, uiDestPitchBYTES, ITEM_FONT, sFontX, sFontY, pStr);
                    }

                    FontSubSystem.SetFont(ITEMDESC_FONT);


                    // Render attachment symbols
                    //                    if (ItemHasAttachments(pObject))
                    {
                        FontSubSystem.SetFontForeground(FontColor.FONT_GREEN);
                        FontSubSystem.SetFontShadow(FontShadow.DEFAULT_SHADOW);

                        sNewY = sCenY + 2;
                        //                        wprintf(pStr, "*");

                        // Get length of string
                        uiStringLength = FontSubSystem.StringPixLength(pStr, ITEM_FONT);

                        sNewX = sCenX + 43 - uiStringLength - 4;

                        //                        mprintf_buffer(pDestBuf, uiDestPitchBYTES, ITEMDESC_FONT, sNewX, sNewY, pStr);
                        //gprintfinvalidate( sNewX, sNewY, pStr );
                    }

                    if (gItemPickupMenu.bCurSelect == (cnt + gItemPickupMenu.ubScrollAnchor))
                    {
                        //SetFontForeground( ITEMDESC_FONTSHADOW2 );
                        //if ( gItemPickupMenu.pfSelectedArray[  cnt + gItemPickupMenu.ubScrollAnchor ] )
                        //{
                        //	SetFontForeground( FONT_MCOLOR_LTYELLOW );
                        //	SetFontShadow( ITEMDESC_FONTSHADOW2 );
                        //}
                        //else
                        //{
                        FontSubSystem.SetFontForeground(FontColor.FONT_WHITE);
                        FontSubSystem.SetFontShadow(FontShadow.DEFAULT_SHADOW);
                        //}
                        // Blt8BPPDataTo16BPPBufferOutline( (int*)pDestBuf, uiDestPitchBYTES, gTileDatabase[ usItemTileIndex ].hTileSurface, sCenX, sCenY, gTileDatabase[ usItemTileIndex ].usRegionIndex, Get16BPPColor( FROMRGB( 255, 0, 0 ) ), true );
                        // Blt8BPPDataTo16BPPBufferOutline( (int*)pDestBuf, uiDestPitchBYTES, gTileDatabase[ usItemTileIndex ].hTileSurface, sCenX, sCenY, gTileDatabase[ usItemTileIndex ].usRegionIndex, Get16BPPColor( FROMRGB( 255, 0, 0 ) ), true );
                    }
                    else
                    {
                        FontSubSystem.SetFontForeground(FontColor.FONT_BLACK);
                        FontSubSystem.SetFontShadow(ITEMDESC_FONTSHADOW2);
                    }

                    // Render name
                    sCenX = ITEMPICK_TEXT_X + gItemPickupMenu.sX;
                    sCenY = ITEMPICK_TEXT_Y + gItemPickupMenu.sY + (ITEMPICK_TEXT_YSPACE * (int)cnt);

                    // If we are money...
                    if (Item[pObject.usItem].usItemClass == IC.MONEY)
                    {
                        string pStr2;// [20];
                        pStr2 = wprintf("%ld", pObject.uiMoneyAmount);
                        //                        InsertCommasForDollarFigure(pStr2);
                        //                        InsertDollarSignInToString(pStr2);

                        //                        pStr = wprintf("%s (%ls)", ItemNames[pObject.usItem], pStr2);
                    }
                    else
                    {
                        pStr = wprintf("%s", ShortItemNames[pObject.usItem]);
                    }
                    //                    VarFindFontCenterCoordinates(sCenX, sCenY, ITEMPICK_TEXT_WIDTH, 1, ITEMDESC_FONT, out sFontX, out sFontY, pStr);
                    //                    mprintf_buffer(pDestBuf, uiDestPitchBYTES, ITEMDESC_FONT, sFontX, sFontY, pStr);

                    sY += ITEMPICK_GRAPHIC_YSPACE;
                }
            }

            FontSubSystem.SetFontShadow(FontShadow.DEFAULT_SHADOW);


            //            UnLockVideoSurface(Surfaces.FRAME_BUFFER);

            //            InvalidateRegion(gItemPickupMenu.sX, gItemPickupMenu.sY, gItemPickupMenu.sX + gItemPickupMenu.sWidth, gItemPickupMenu.sY + gItemPickupMenu.sHeight);

            gItemPickupMenu.fDirtyLevel = 0;

        }
    }


    void RemoveItemPickupMenu()
    {
        int cnt;

        if (gfInItemPickupMenu)
        {
            gfSMDisableForItems = false;

//            HandleAnyMercInSquadHasCompatibleStuff((int)CurrentSquad(), null, true);

//            UnLockPauseState();
//            UnPauseGame();
            // UnPause timers as well....
//            PauseTime(false);

            // Unfreese guy!
            gItemPickupMenu.pSoldier.fPauseAllAnimation = false;

            // Remove graphics!
//            DeleteVideoObjectFromIndex(gItemPickupMenu.uiPanelVo);

            // Remove buttons
            if (gItemPickupMenu.bNumSlotsPerPage == NUM_PICKUP_SLOTS && gItemPickupMenu.ubTotalItems > NUM_PICKUP_SLOTS)
            {
//                ButtonSubSystem.RemoveButton(gItemPickupMenu.iUpButton);
//                ButtonSubSystem.RemoveButton(gItemPickupMenu.iDownButton);
            }

//            ButtonSubSystem.RemoveButton(gItemPickupMenu.iAllButton);
//            ButtonSubSystem.RemoveButton(gItemPickupMenu.iOKButton);
//            ButtonSubSystem.RemoveButton(gItemPickupMenu.iCancelButton);

            // Remove button images
//            ButtonSubSystem.UnloadButtonImage(gItemPickupMenu.iUpButtonImages);
//            ButtonSubSystem.UnloadButtonImage(gItemPickupMenu.iDownButtonImages);
//            ButtonSubSystem.UnloadButtonImage(gItemPickupMenu.iAllButtonImages);
//            ButtonSubSystem.UnloadButtonImage(gItemPickupMenu.iCancelButtonImages);
//            ButtonSubSystem.UnloadButtonImage(gItemPickupMenu.iOKButtonImages);

            MouseSubSystem.MSYS_RemoveRegion((gItemPickupMenu.BackRegions));
            MouseSubSystem.MSYS_RemoveRegion((gItemPickupMenu.BackRegion));

            // Remove regions
            for (cnt = 0; cnt < gItemPickupMenu.bNumSlotsPerPage; cnt++)
            {
                MouseSubSystem.MSYS_RemoveRegion((gItemPickupMenu.Regions[cnt]));
            }

            // Remove register rect
            if (gItemPickupMenu.iDirtyRect != -1)
            {
                //FreeBackgroundRect( gItemPickupMenu.iDirtyRect );
            }

            // Free selection list...
            MemFree(gItemPickupMenu.pfSelectedArray);
            gItemPickupMenu.pfSelectedArray = null;


            // Set cursor back to normal mode...
//            guiPendingOverrideEvent = A_CHANGE_TO_MOVE;

            // Rerender world
//            SetRenderFlags(RENDER_FLAG_FULL);

            gfInItemPickupMenu = false;

            //gfSMDisableForItems = false;
            //EnableButtonsForInItemBox( true );
//            EnableSMPanelButtons(true, true);
            gfSMDisableForItems = false;

            fInterfacePanelDirty = DIRTYLEVEL2;

            // Turn off Ignore scrolling
            gfIgnoreScrolling = false;
//            DisableTacticalTeamPanelButtons(false);
            gubSelectSMPanelToMerc = gpSMCurrentMerc.ubID;

        }
    }


    void ItemPickupScrollUp(GUI_BUTTON btn, MSYS_CALLBACK_REASON reason)
    {
        if (reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_DWN))
        {
            btn.uiFlags |= ButtonFlags.BUTTON_CLICKED_ON;
        }
        else if (reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP))
        {
            btn.uiFlags &= (~ButtonFlags.BUTTON_CLICKED_ON);
            SetupPickupPage((int)(gItemPickupMenu.bScrollPage - 1));
        }
        else if (reason.HasFlag(MSYS_CALLBACK_REASON.LOST_MOUSE))
        {
            btn.uiFlags &= (~ButtonFlags.BUTTON_CLICKED_ON);
        }

    }


    void ItemPickupScrollDown(GUI_BUTTON btn, MSYS_CALLBACK_REASON reason)
    {
        if (reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_DWN))
        {
            btn.uiFlags |= ButtonFlags.BUTTON_CLICKED_ON;
        }
        else if (reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP))
        {
            btn.uiFlags &= (~ButtonFlags.BUTTON_CLICKED_ON);
            SetupPickupPage((int)(gItemPickupMenu.bScrollPage + 1));
        }
        else if (reason.HasFlag(MSYS_CALLBACK_REASON.LOST_MOUSE))
        {
            btn.uiFlags &= (~ButtonFlags.BUTTON_CLICKED_ON);
        }
    }

    void ItemPickupAll(GUI_BUTTON btn, MSYS_CALLBACK_REASON reason)
    {
        int cnt;


        if (reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_DWN))
        {
            btn.uiFlags |= ButtonFlags.BUTTON_CLICKED_ON;
        }
        else if (reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP))
        {
            btn.uiFlags &= (~ButtonFlags.BUTTON_CLICKED_ON);

            gItemPickupMenu.fAllSelected = !gItemPickupMenu.fAllSelected;


            // OK, pickup item....
            //gItemPickupMenu.fHandled = true;
            // Tell our soldier to pickup this item!
            //SoldierGetItemFromWorld( gItemPickupMenu.pSoldier, ITEM_PICKUP_ACTION_ALL, gItemPickupMenu.sGridNo, gItemPickupMenu.bZLevel, null );
            for (cnt = 0; cnt < gItemPickupMenu.ubTotalItems; cnt++)
            {
//                gItemPickupMenu.pfSelectedArray[cnt] = gItemPickupMenu.fAllSelected;
            }

            if (gItemPickupMenu.fAllSelected)
            {
//                EnableButton(gItemPickupMenu.iOKButton);
            }
            else
            {
//                DisableButton(gItemPickupMenu.iOKButton);
            }

        }
        else if (reason.HasFlag(MSYS_CALLBACK_REASON.LOST_MOUSE))
        {
            btn.uiFlags &= (~ButtonFlags.BUTTON_CLICKED_ON);
        }
    }


    void ItemPickupOK(GUI_BUTTON btn, MSYS_CALLBACK_REASON reason)
    {
        int cnt = 0;

        if (reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_DWN))
        {
            btn.uiFlags |= ButtonFlags.BUTTON_CLICKED_ON;
        }
        else if (reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP))
        {
            btn.uiFlags &= (~ButtonFlags.BUTTON_CLICKED_ON);

            // OK, pickup item....
            gItemPickupMenu.fHandled = true;

            // Tell our soldier to pickup this item!
//            SoldierGetItemFromWorld(gItemPickupMenu.pSoldier, ITEM_PICKUP_SELECTION, gItemPickupMenu.sGridNo, gItemPickupMenu.bZLevel, gItemPickupMenu.pfSelectedArray);
        }
        else if (reason.HasFlag(MSYS_CALLBACK_REASON.LOST_MOUSE))
        {
            btn.uiFlags &= (~ButtonFlags.BUTTON_CLICKED_ON);
        }
    }

    void ItemPickupCancel(GUI_BUTTON btn, MSYS_CALLBACK_REASON reason)
    {
        int cnt = 0;

        if (reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_DWN))
        {
            btn.uiFlags |= ButtonFlags.BUTTON_CLICKED_ON;
        }
        else if (reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP))
        {
            btn.uiFlags &= (~ButtonFlags.BUTTON_CLICKED_ON);

            // OK, pickup item....
            gItemPickupMenu.fHandled = true;
        }
        else if (reason.HasFlag(MSYS_CALLBACK_REASON.LOST_MOUSE))
        {
            btn.uiFlags &= (~ButtonFlags.BUTTON_CLICKED_ON);
        }
    }


    static bool bChecked = false;

    void ItemPickMenuMouseMoveCallback(ref MOUSE_REGION pRegion, MSYS_CALLBACK_REASON iReason)
    {
        int uiItemPos;
        ITEM_POOL? pTempItemPool;
        int bPos;

        uiItemPos = (int)MouseSubSystem.MSYS_GetRegionUserData(ref pRegion, 0);


        if (iReason.HasFlag(MSYS_CALLBACK_REASON.MOVE))
        {
            bPos = (uiItemPos + gItemPickupMenu.ubScrollAnchor);

            if (bPos < gItemPickupMenu.ubTotalItems)
            {
                // Set current selected guy....
                gItemPickupMenu.bCurSelect = bPos;

                if (false)//!bChecked)
                {
                    // Show compatible ammo...
                    pTempItemPool = gItemPickupMenu.ItemPoolSlots[gItemPickupMenu.bCurSelect - gItemPickupMenu.ubScrollAnchor];

                    //                    memcpy((gItemPickupMenu.CompAmmoObject), &(gWorldItems[pTempItemPool.iItemIndex].o), sizeof(OBJECTTYPE));
                    //
                    //                    // Turn off first...
                    //                    HandleAnyMercInSquadHasCompatibleStuff((int)CurrentSquad(), null, true);
                    //                    InternalHandleCompatibleAmmoUI(gpSMCurrentMerc, (gItemPickupMenu.CompAmmoObject), true);
                    //
                    //                    HandleAnyMercInSquadHasCompatibleStuff((int)CurrentSquad(), &(gWorldItems[pTempItemPool.iItemIndex].o), false);
                    //
                    //                    SetItemPickupMenuDirty(DIRTYLEVEL2);
                    //
                    //                    bChecked = true;
                }
            }
        }
        else if (iReason.HasFlag(MSYS_CALLBACK_REASON.LOST_MOUSE))
        {
            gItemPickupMenu.bCurSelect = 255;

            //            InternalHandleCompatibleAmmoUI(gpSMCurrentMerc, (gItemPickupMenu.CompAmmoObject), false);
            //            HandleAnyMercInSquadHasCompatibleStuff((int)CurrentSquad(), null, true);
            //
            //            SetItemPickupMenuDirty(DIRTYLEVEL2);
            //
            //            bChecked = false;
        }


    }


    void ItemPickupBackgroundClick(MOUSE_REGION pRegion, MSYS_CALLBACK_REASON iReason)
    {
        if (iReason.HasFlag(MSYS_CALLBACK_REASON.RBUTTON_UP))
        {
            // OK, goto team panel....
            //            ToggleTacticalPanels();
        }
    }



    void ItemPickMenuMouseClickCallback(ref MOUSE_REGION pRegion, MSYS_CALLBACK_REASON iReason)
    {
        int uiItemPos;
        int cnt;
        bool fEnable = false;

        uiItemPos = (int)MouseSubSystem.MSYS_GetRegionUserData(ref pRegion, 0);


        if (iReason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP))
        {
            if (uiItemPos + gItemPickupMenu.ubScrollAnchor < gItemPickupMenu.ubTotalItems)
            {
                // Toggle selection... ONLY IF LEGAL!!
                //                gItemPickupMenu.pfSelectedArray[uiItemPos + gItemPickupMenu.ubScrollAnchor] = !gItemPickupMenu.pfSelectedArray[uiItemPos + gItemPickupMenu.ubScrollAnchor];

                // OK, pickup item....
                //gItemPickupMenu.fHandled = true;

                //pTempItemPool = gItemPickupMenu.ItemPoolSlots[ gItemPickupMenu.bCurSelect - gItemPickupMenu.ubScrollAnchor ];

                // Tell our soldier to pickup this item!
                //SoldierGetItemFromWorld( gItemPickupMenu.pSoldier, pTempItemPool.iItemIndex, gItemPickupMenu.sGridNo, gItemPickupMenu.bZLevel );
            }

            // Loop through all and set /unset OK
            for (cnt = 0; cnt < gItemPickupMenu.ubTotalItems; cnt++)
            {
                //                if (gItemPickupMenu.pfSelectedArray[cnt])
                {
                    fEnable = true;
                    break;
                }
            }

            if (fEnable)
            {
                //                EnableButton(gItemPickupMenu.iOKButton);
            }
            else
            {
                //                DisableButton(gItemPickupMenu.iOKButton);
            }
        }
        else if (iReason.HasFlag(MSYS_CALLBACK_REASON.RBUTTON_UP))
        {

        }
    }

    bool HandleItemPickupMenu()
    {

        if (!gfInItemPickupMenu)
        {
            return (false);
        }

        if (gItemPickupMenu.fHandled)
        {
            //            RemoveItemPickupMenu();
        }

        return (gItemPickupMenu.fHandled);
    }


    void BtnMoneyButtonCallback(GUI_BUTTON btn, MSYS_CALLBACK_REASON reason)
    {
        int i;
        if (reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_DWN))
        {
            //            btn.uiFlags |= BUTTON_CLICKED_ON;
            //            InvalidateRegion(btn.Area.RegionTopLeftX, btn.Area.RegionTopLeftY, btn.Area.RegionBottomRightX, btn.Area.RegionBottomRightY);
        }
        if (reason.HasFlag(MSYS_CALLBACK_REASON.RBUTTON_DWN))
        {
            //            btn.uiFlags |= BUTTON_CLICKED_ON;
            //            InvalidateRegion(btn.Area.RegionTopLeftX, btn.Area.RegionTopLeftY, btn.Area.RegionBottomRightX, btn.Area.RegionBottomRightY);
        }
        if (reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP))
        {
            MoneyButtons ubButton = (MoneyButtons)ButtonSubSystem.MSYS_GetBtnUserData(btn, 0);

            //            btn.uiFlags &= (~BUTTON_CLICKED_ON);

            switch (ubButton)
            {
                case MoneyButtons.M_1000:
                    if (gRemoveMoney.uiMoneyRemaining >= 1000)
                    {
                        //if the player is removing money from their account, and they are removing more then $20,000
                        if (gfAddingMoneyToMercFromPlayersAccount && (gRemoveMoney.uiMoneyRemoving + 1000) > MAX_MONEY_PER_SLOT)
                        {
                            if (guiCurrentScreen == ScreenName.SHOPKEEPER_SCREEN)
                            {
                                //                                MessageBoxSubSystem.DoMessageBox(MSG.BOX_BASIC_STYLE, gzMoneyWithdrawMessageText[MONEY_TEXT_WITHDRAW_MORE_THEN_MAXIMUM], ScreenName.SHOPKEEPER_SCREEN, (int)MSG.BOX_FLAG_OK, null, null);
                            }
                            else
                            {
                                //                                MessageBoxSubSystem.DoMessageBox(MSG.BOX_BASIC_STYLE, gzMoneyWithdrawMessageText[MONEY_TEXT_WITHDRAW_MORE_THEN_MAXIMUM], ScreenName.GAME_SCREEN, (int)MSG.BOX_FLAG_OK, null, null);
                            }

                            return;
                        }

                        gRemoveMoney.uiMoneyRemaining -= 1000;
                        gRemoveMoney.uiMoneyRemoving += 1000;
                    }
                    break;
                case MoneyButtons.M_100:
                    if (gRemoveMoney.uiMoneyRemaining >= 100)
                    {
                        //if the player is removing money from their account, and they are removing more then $20,000
                        if (gfAddingMoneyToMercFromPlayersAccount && (gRemoveMoney.uiMoneyRemoving + 100) > MAX_MONEY_PER_SLOT)
                        {
                            //                            MessageBoxSubSystem.DoMessageBox(MSG.BOX_BASIC_STYLE, gzMoneyWithdrawMessageText[MONEY_TEXT_WITHDRAW_MORE_THEN_MAXIMUM], ScreenName.GAME_SCREEN, (int)MSG.BOX_FLAG_OK, null, null);
                            return;
                        }

                        gRemoveMoney.uiMoneyRemaining -= 100;
                        gRemoveMoney.uiMoneyRemoving += 100;
                    }
                    break;
                case MoneyButtons.M_10:
                    if (gRemoveMoney.uiMoneyRemaining >= 10)
                    {
                        //if the player is removing money from their account, and they are removing more then $20,000
                        if (gfAddingMoneyToMercFromPlayersAccount && (gRemoveMoney.uiMoneyRemoving + 10) > MAX_MONEY_PER_SLOT)
                        {
                            //                            MessageBoxSubSystem.DoMessageBox(MSG.BOX_BASIC_STYLE, gzMoneyWithdrawMessageText[MONEY_TEXT_WITHDRAW_MORE_THEN_MAXIMUM], ScreenName.GAME_SCREEN, (int)MSG.BOX_FLAG_OK, null, null);
                            return;
                        }

                        gRemoveMoney.uiMoneyRemaining -= 10;
                        gRemoveMoney.uiMoneyRemoving += 10;
                    }
                    break;
                case MoneyButtons.M_DONE:
                    {
                        //                        RemoveMoney();
                        //
                        //                        DeleteItemDescriptionBox();
                    }
                    break;
            }
            if (ubButton != MoneyButtons.M_DONE)
            {
                //                RenderItemDescriptionBox();
                for (i = 0; i < MAX_ATTACHMENTS; i++)
                {
                    //                    MarkAButtonDirty(guiMoneyButtonBtn[i]);
                }
            }

            //            InvalidateRegion(btn.Area.RegionTopLeftX, btn.Area.RegionTopLeftY, btn.Area.RegionBottomRightX, btn.Area.RegionBottomRightY);
        }


        if (reason.HasFlag(MSYS_CALLBACK_REASON.RBUTTON_UP))
        {
            MoneyButtons ubButton = (MoneyButtons)ButtonSubSystem.MSYS_GetBtnUserData(btn, 0);

            btn.uiFlags &= (~ButtonFlags.BUTTON_CLICKED_ON);


            switch (ubButton)
            {
                case MoneyButtons.M_1000:
                    if (gRemoveMoney.uiMoneyRemoving >= 1000)
                    {
                        gRemoveMoney.uiMoneyRemaining += 1000;
                        gRemoveMoney.uiMoneyRemoving -= 1000;
                    }
                    break;
                case MoneyButtons.M_100:
                    if (gRemoveMoney.uiMoneyRemoving >= 100)
                    {
                        gRemoveMoney.uiMoneyRemaining += 100;
                        gRemoveMoney.uiMoneyRemoving -= 100;
                    }
                    break;
                case MoneyButtons.M_10:
                    if (gRemoveMoney.uiMoneyRemoving >= 10)
                    {
                        gRemoveMoney.uiMoneyRemaining += 10;
                        gRemoveMoney.uiMoneyRemoving -= 10;
                    }
                    break;
            }

            //            RenderItemDescriptionBox();
            for (i = 0; i < MAX_ATTACHMENTS; i++)
            {
                //                MarkAButtonDirty(guiMoneyButtonBtn[i]);
            }

            //            InvalidateRegion(btn.Area.RegionTopLeftX, btn.Area.RegionTopLeftY, btn.Area.RegionBottomRightX, btn.Area.RegionBottomRightY);
        }
    }

    void RemoveMoney()
    {
        if (gRemoveMoney.uiMoneyRemoving != 0)
        {
            //if we are in the shop keeper interface
            if (guiTacticalInterfaceFlags.HasFlag(INTERFACE.SHOPKEEP_INTERFACE))
            {
                INVENTORY_IN_SLOT InvSlot;

                //                memset(&InvSlot, 0, sizeof(INVENTORY_IN_SLOT));

                InvSlot.fActive = true;
                InvSlot.sItemIndex = Items.MONEY;
                InvSlot.bSlotIdInOtherLocation = -1;

                //Remove the money from the money in the pocket
                gpItemDescObject.uiMoneyAmount = gRemoveMoney.uiMoneyRemaining;

                //Create an item to get the money that is being removed
                //                CreateItem(Items.MONEY, 0, InvSlot.ItemObject);

                //Set the amount thast is being removed
                //                InvSlot.ItemObject.uiMoneyAmount = gRemoveMoney.uiMoneyRemoving;
                //                InvSlot.ubIdOfMercWhoOwnsTheItem = gpItemDescSoldier.ubProfile;

                //if we are removing money from the players account
                if (gfAddingMoneyToMercFromPlayersAccount)
                {
                    gpItemDescObject.uiMoneyAmount = gRemoveMoney.uiMoneyRemoving;

                    //take the money from the player
                    //                    AddTransactionToPlayersBook(TRANSFER_FUNDS_TO_MERC, gpSMCurrentMerc.ubProfile, GetWorldTotalMin(), -(int)(gpItemDescObject.uiMoneyAmount));
                }

                //                memcpy(gMoveingItem, InvSlot, sizeof(INVENTORY_IN_SLOT));

                //                memcpy(gItemPointer, InvSlot.ItemObject, sizeof(OBJECTTYPE));
                gpItemPointer = gItemPointer;
                gpItemPointerSoldier = gpSMCurrentMerc;

                // Set mouse
                //                SetSkiCursor(EXTERN_CURSOR);

                //Restrict the cursor to the proper area
                //                RestrictSkiMouseCursor();
            }
            else
            {
                //                CreateMoney(gRemoveMoney.uiMoneyRemoving, gItemPointer);
                gpItemPointer = gItemPointer;
                //Asign the soldier to be the currently selected soldier
                gpItemPointerSoldier = gpItemDescSoldier;

                //Remove the money from the money in the pocket
                //if we are removing money from the players account
                if (gfAddingMoneyToMercFromPlayersAccount)
                {
                    gpItemDescObject.uiMoneyAmount = gRemoveMoney.uiMoneyRemoving;

                    //take the money from the player
                    //                    AddTransactionToPlayersBook(TRANSFER_FUNDS_TO_MERC, gpSMCurrentMerc.ubProfile, GetWorldTotalMin(), -(int)(gpItemDescObject.uiMoneyAmount));
                }
                else
                {
                    gpItemDescObject.uiMoneyAmount = gRemoveMoney.uiMoneyRemaining;
                }

                if (guiCurrentItemDescriptionScreen == ScreenName.MAP_SCREEN)
                {
                    // Set mouse
                    //                    guiExternVo = GetInterfaceGraphicForItem((Item[gpItemPointer.usItem]));
                    //                    gusExternVoSubIndex = Item[gpItemPointer.usItem].ubGraphicNum;

                    MouseSubSystem.MSYS_ChangeRegionCursor(gMPanelRegion, CURSOR.EXTERN_CURSOR);
                    MouseSubSystem.MSYS_SetCurrentCursor(CURSOR.EXTERN_CURSOR);
                    fMapInventoryItem = true;
                    fTeamPanelDirty = true;
                }

            }
        }

        //	if( gfAddingMoneyToMercFromPlayersAccount )
        //		gfAddingMoneyToMercFromPlayersAccount = false;
    }


    bool AttemptToApplyCamo(SOLDIERTYPE pSoldier, int usItemIndex)
    {
        return (false);
    }


    void GetHelpTextForItem(string pzStr, OBJECTTYPE pObject, SOLDIERTYPE pSoldier)
    {
        string pStr = string.Empty;// [250];
        Items usItem = pObject.usItem;
        int cnt = 0;
        int iNumAttachments = 0;

        if (pSoldier != null)
        {
            if (pSoldier.uiStatusFlags.HasFlag(SOLDIER.DEAD))
            {
                pStr = wprintf("");
                pzStr = wprintf("%s", pStr);
                return;
            }
        }

        if (usItem == Items.MONEY)
        {
            pStr = wprintf("%ld", pObject.uiMoneyAmount);
            //            InsertCommasForDollarFigure(pStr);
            //            InsertDollarSignInToString(pStr);
        }
        else if (Item[usItem].usItemClass == IC.MONEY)
        { // alternate money like silver or gold
            string pStr2;// [20];
            pStr2 = wprintf("%ld", pObject.uiMoneyAmount);
            //            InsertCommasForDollarFigure(pStr2);
            //            InsertDollarSignInToString(pStr2);

            //            wprintf(pStr, "%s (%ls)", ItemNames[usItem], pStr2);
        }
        else if (usItem != NOTHING)
        {
            if (!gGameOptions.GunNut
                && Item[usItem].usItemClass == IC.GUN
                && usItem != Items.ROCKET_LAUNCHER
                && usItem != Items.ROCKET_RIFLE)
            {
                //                wprintf(pStr, "%s (%s)", ItemNames[usItem], AmmoCaliber[Weapon[usItem].ubCalibre]);
            }
            else
            {
                //                wprintf(pStr, "%s", ItemNames[usItem]);
            }

            if ((pObject.usItem == Items.ROCKET_RIFLE || pObject.usItem == Items.AUTO_ROCKET_RIFLE)
                && pObject.ubImprintID < NO_PROFILE)
            {
                string pStr2;// [20];
                pStr2 = wprintf(" [%s]", gMercProfiles[pObject.ubImprintID].zNickname);
                //                wcscat(pStr, pStr2);
            }

            // Add attachment string....
            for (cnt = 0; cnt < MAX_ATTACHMENTS; cnt++)
            {
                if (pObject.usAttachItem[cnt] != NOTHING)
                {
                    iNumAttachments++;

                    if (iNumAttachments == 1)
                    {
                        wcscat(pStr, " ( ");
                    }
                    else
                    {
                        wcscat(pStr, ", \n");
                    }

                    //                    wcscat(pStr, ItemNames[pObject.usAttachItem[cnt]]);
                }
            }

            if (iNumAttachments > 0)
            {
                wcscat(pStr, pMessageStrings[MSG.END_ATTACHMENT_LIST]);
            }
        }
        else
        {
            wprintf(pStr, "");
        }

        // Copy over...
        wprintf(pzStr, "%s", pStr);
    }


    int GetPrefferedItemSlotGraphicNum(Items usItem)
    {
        // Check for small item...
        if (Item[usItem].ubPerPocket >= 1)
        {
            // Small
            return (2);
        }

        // Now it could be large or armour, check class...
        if (Item[usItem].usItemClass == IC.ARMOUR)
        {
            return (1);
        }

        // OK, it's a big one...
        return (0);
    }


    void CancelItemPointer()
    {
        // ATE: If we have an item pointer end it!
        if (gpItemPointer != null)
        {
            if (gbItemPointerSrcSlot != NO_SLOT)
            {
                // Place it back in our hands!
                //                PlaceObject(gpItemPointerSoldier, gbItemPointerSrcSlot, gpItemPointer);

                // ATE: This could potnetially swap!
                // Make sure # of items is 0, if not, auto place somewhere else...
                if (gpItemPointer.ubNumberOfObjects > 0)
                {
                    //                    if (!AutoPlaceObject(gpItemPointerSoldier, gpItemPointer, false))
                    {
                        // Alright, place of the friggen ground!
                        //                        AddItemToPool(gpItemPointerSoldier.sGridNo, gpItemPointer, 1, gpItemPointerSoldier.bLevel, 0, -1);
                        //                        NotifySoldiersToLookforItems();
                    }
                }
            }
            else
            {
                // We drop it here.....
                //                AddItemToPool(gpItemPointerSoldier.sGridNo, gpItemPointer, 1, gpItemPointerSoldier.bLevel, 0, -1);
                //                NotifySoldiersToLookforItems();
            }
            //            EndItemPointer();
        }
    }


    bool LoadItemCursorFromSavedGame(Stream hFile)
    {
        int uiLoadSize = 0;
        int uiNumBytesRead = 0;
        ITEM_CURSOR_SAVE_INFO SaveStruct = new();

        // Load structure
        //        uiLoadSize = sizeof(ITEM_CURSOR_SAVE_INFO);
        //        FileRead(hFile, SaveStruct, uiLoadSize, out uiNumBytesRead);
        if (uiNumBytesRead != uiLoadSize)
        {
            return (false);
        }
//        uiLoadSize = sizeof(ITEM_CURSOR_SAVE_INFO);
//        FileRead(hFile, SaveStruct, uiLoadSize, out uiNumBytesRead);
//        if (uiNumBytesRead != uiLoadSize)
//        {
//            return (false);
//        }

        // Now set things up.....
        // Copy object
        // memcpy(gItemPointer, (SaveStruct.ItemPointerInfo), sizeof(OBJECTTYPE));

        // Copy soldier ID
        //        if (SaveStruct.ubSoldierID == NOBODY)
        {
            gpItemPointerSoldier = null;
        }
        //        else
        {
            //            gpItemPointerSoldier = MercPtrs[SaveStruct.ubSoldierID];
        }

        // Inv slot
        //        gbItemPointerSrcSlot = SaveStruct.ubInvSlot;

        // Boolean
        //        if (SaveStruct.fCursorActive)
        //        {
        //            gpItemPointer = (gItemPointer);
        //            ReEvaluateDisabledINVPanelButtons();
        //        }
        //        else
        {
            gpItemPointer = null;
        }

        return (true);
    }

    bool SaveItemCursorToSavedGame(Stream hFile)
    {
        int uiSaveSize = 0;
        int uiNumBytesWritten = 0;

        ITEM_CURSOR_SAVE_INFO SaveStruct;

        // Setup structure;
        //memset(&SaveStruct, 0, sizeof(ITEM_CURSOR_SAVE_INFO));
        //memcpy((SaveStruct.ItemPointerInfo), gItemPointer, sizeof(OBJECTTYPE));

        // Soldier
        if (gpItemPointerSoldier != null)
        {
            SaveStruct.ubSoldierID = gpItemPointerSoldier.ubID;
        }
        else
        {
            SaveStruct.ubSoldierID = NOBODY;
        }

        // INv slot
        SaveStruct.ubInvSlot = gbItemPointerSrcSlot;

        // Boolean
        if (gpItemPointer != null)
        {
            SaveStruct.fCursorActive = true;
        }
        else
        {
            SaveStruct.fCursorActive = false;
        }

        // save locations of watched points
//        uiSaveSize = sizeof(ITEM_CURSOR_SAVE_INFO);
//        FileWrite(hFile, SaveStruct, uiSaveSize, out uiNumBytesWritten);
//        if (uiNumBytesWritten != uiSaveSize)
//        {
//            return (false);
//        }

        // All done...

        return (true);
    }

    public static void UpdateItemHatches()
    {
        SOLDIERTYPE? pSoldier = null;

        if (guiTacticalInterfaceFlags.HasFlag(INTERFACE.MAPSCREEN))
        {
            //            if (fShowInventoryFlag && bSelectedInfoChar >= 0)
            {
                //                pSoldier = MercPtrs[gCharactersList[bSelectedInfoChar].usSolID];
            }
        }
        else
        {
            pSoldier = gpSMCurrentMerc;
        }

        if (pSoldier != null)
        {
            //            ReevaluateItemHatches(pSoldier, (bool)(gpItemPointer == null));
        }
    }
}

public class ITEM_PICKUP_MENU_STRUCT
{
    public ITEM_POOL? pItemPool;
    public int sX;
    public int sY;
    public int sWidth;
    public int sHeight;
    public int bScrollPage;
    public int ubScrollAnchor;
    public int ubTotalItems;
    public int bCurSelect;
    public int bNumSlotsPerPage;
    public int uiPanelVo;
    public int iUpButtonImages;
    public int iDownButtonImages;
    public int iAllButtonImages;
    public int iCancelButtonImages;
    public int iOKButtonImages;
    public int iUpButton;
    public int iDownButton;
    public int iAllButton;
    public int iOKButton;
    public int iCancelButton;
    public bool fCanScrollUp;
    public bool fCanScrollDown;
    public int fDirtyLevel;
    public int iDirtyRect;
    public bool fHandled;
    public int sGridNo;
    public int bZLevel;
    public int sButtomPanelStartY;
    public SOLDIERTYPE pSoldier;
    public ITEM_POOL[] ItemPoolSlots = new ITEM_POOL[NUM_PICKUP_SLOTS];
    public MOUSE_REGION[] Regions = new MOUSE_REGION[NUM_PICKUP_SLOTS];
    public MOUSE_REGION BackRegions;
    public MOUSE_REGION BackRegion;
    public bool? pfSelectedArray;
    public bool fAtLeastOneSelected;
    public OBJECTTYPE CompAmmoObject;
    public bool fAllSelected;
}

public unsafe struct ITEM_CURSOR_SAVE_INFO
{
    public OBJECTTYPE ItemPointerInfo;
    public int ubSoldierID;
    public InventorySlot ubInvSlot;
    public bool fCursorActive;
    public fixed int bPadding[5];
}
