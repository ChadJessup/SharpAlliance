﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using SharpAlliance.Core.Interfaces;
using SharpAlliance.Core.Managers;
using SharpAlliance.Core.Managers.VideoSurfaces;

namespace SharpAlliance.Core.SubSystems.LaptopSubSystem.BobbyRSubSystem;

public partial class BobbyR
{
    public BobbyR(
        ILogger<BobbyR> logger,
        IVideoManager videoManager)
    {
        this.logger = logger;
        video = videoManager;
    }

    bool EnterBobbyR()
    {
        //VOBJECT_DESC VObjectDesc;
        int i;

        // an array of mouse regions for the bobbies signs.  Top Left corner, bottom right corner
        List<int> usMouseRegionPosArray = new()
        {
            BOBBIES_USED_SIGN_X,
            BOBBIES_USED_SIGN_Y,
            BOBBIES_USED_SIGN_X+BOBBIES_USED_SIGN_WIDTH,
            BOBBIES_USED_SIGN_Y+BOBBIES_USED_SIGN_HEIGHT,
            BOBBIES_MISC_SIGN_X,
            BOBBIES_MISC_SIGN_Y,
            BOBBIES_MISC_SIGN_X+BOBBIES_MISC_SIGN_WIDTH,
            BOBBIES_MISC_SIGN_Y+BOBBIES_MISC_SIGN_HEIGHT,
            BOBBIES_GUNS_SIGN_X,
            BOBBIES_GUNS_SIGN_Y,
            BOBBIES_GUNS_SIGN_X+BOBBIES_GUNS_SIGN_WIDTH,
            BOBBIES_GUNS_SIGN_Y+BOBBIES_GUNS_SIGN_HEIGHT,
            BOBBIES_AMMO_SIGN_X,
            BOBBIES_AMMO_SIGN_Y,
            BOBBIES_AMMO_SIGN_X+BOBBIES_AMMO_SIGN_WIDTH,
            BOBBIES_AMMO_SIGN_Y+BOBBIES_AMMO_SIGN_HEIGHT,
            BOBBIES_ARMOUR_SIGN_X,
            BOBBIES_ARMOUR_SIGN_Y,
            BOBBIES_ARMOUR_SIGN_X+BOBBIES_ARMOUR_SIGN_WIDTH,
            BOBBIES_ARMOUR_SIGN_Y+BOBBIES_ARMOUR_SIGN_HEIGHT
        };

        this.InitBobbyRWoodBackground();

        // load the Bobbyname graphic and add it
        //
        MultilanguageGraphicUtils.GetMLGFilename(out string imageFile, MLG.BOBBYNAME);
        video.GetVideoObject(imageFile, out guiBobbyName);

        // load the plaque graphic and add it
        //
        //FilenameForBPP("LAPTOP\\BobbyPlaques.sti", VObjectDesc.ImageFile);
        video.GetVideoObject("LAPTOP\\BobbyPlaques.sti", out guiPlaque);

        // load the TopHinge graphic and add it
        //
        //FilenameForBPP("LAPTOP\\BobbyTopHinge.sti", VObjectDesc.ImageFile);
        video.GetVideoObject("LAPTOP\\BobbyTopHinge.sti", out guiTopHinge);

        // load the BottomHinge graphic and add it
        //
        //Utils.FilenameForBPP("LAPTOP\\BobbyBottomHinge.sti", VObjectDesc.ImageFile);
        video.GetVideoObject("LAPTOP\\BobbyBottomHinge.sti", out guiBottomHinge);

        // load the Store Plaque graphic and add it
        //
        MultilanguageGraphicUtils.GetMLGFilename(out imageFile, MLG.STOREPLAQUE);
        video.GetVideoObject(imageFile, out guiStorePlaque);

        // load the Handle graphic and add it
        //
        //FilenameForBPP("LAPTOP\\BobbyHandle.sti", VObjectDesc.ImageFile);
        video.GetVideoObject("LAPTOP\\BobbyHandle.sti", out guiHandle);


        this.InitBobbiesMouseRegion(BOBBIES_NUMBER_SIGNS, usMouseRegionPosArray, gSelectedBobbiesSignMenuRegion);


        if (!LaptopSaveInfo.fBobbyRSiteCanBeAccessed)
        {
            // load the Handle graphic and add it
            //
            //Utils.FilenameForBPP("LAPTOP\\UnderConstruction.sti", VObjectDesc.ImageFile);

            var hvobject = video.GetVideoObject("LAPTOP\\UnderConstruction.sti", out guiUnderConstructionImage);

            for (i = 0; i < BOBBIES_NUMBER_SIGNS; i++)
            {
                MouseSubSystem.MSYS_DisableRegion(gSelectedBobbiesSignMenuRegion[i]);
            }

            LaptopSaveInfo.ubHaveBeenToBobbyRaysAtLeastOnceWhileUnderConstruction = BOBBYR_VISITS.BEEN_TO_SITE_ONCE;
        }


        Laptop.SetBookMark(BOOKMARK.BOBBYR_BOOKMARK);
        HandleBobbyRUnderConstructionAni(true);

        this.RenderBobbyR();

        return true;
    }

    public static void ExitBobbyR()
    {

        video.DeleteVideoObjectFromIndex(guiBobbyName);
        video.DeleteVideoObjectFromIndex(guiPlaque);
        video.DeleteVideoObjectFromIndex(guiTopHinge);
        video.DeleteVideoObjectFromIndex(guiBottomHinge);
        video.DeleteVideoObjectFromIndex(guiStorePlaque);
        video.DeleteVideoObjectFromIndex(guiHandle);

        if (!LaptopSaveInfo.fBobbyRSiteCanBeAccessed)
        {
            video.DeleteVideoObjectFromIndex(guiUnderConstructionImage);
        }


        DeleteBobbyRWoodBackground();

        RemoveBobbiesMouseRegion(BOBBIES_NUMBER_SIGNS, gSelectedBobbiesSignMenuRegion);

        guiLastBobbyRayPage = LAPTOP_MODE.BOBBY_R;
    }

    public static void HandleBobbyR()
    {
        HandleBobbyRUnderConstructionAni(false);
    }

    void RenderBobbyR()
    {
        HVOBJECT? hPixHandle;
        HVOBJECT? hStorePlaqueHandle;

        this.DrawBobbyRWoodBackground();

        // Bobby's Name
        // hPixHandle = video.GetVideoObject(guiBobbyName);
        // VideoObjectManager.BltVideoObject(SurfaceType.FRAME_BUFFER, hPixHandle, 0, BOBBY_RAYS_NAME_X, BOBBY_RAYS_NAME_Y, VO_BLT.SRCTRANSPARENCY, null);
        // 
        // // Plaque
        // hPixHandle = video.GetVideoObject(guiPlaque);
        // VideoObjectManager.BltVideoObject(SurfaceType.FRAME_BUFFER, hPixHandle, 0, BOBBYS_PLAQUES_X, BOBBYS_PLAQUES_Y, VO_BLT.SRCTRANSPARENCY, null);
        // 
        // // Top Hinge
        // hPixHandle = video.GetVideoObject(guiTopHinge);
        // VideoObjectManager.BltVideoObject(SurfaceType.FRAME_BUFFER, hPixHandle, 0, BOBBIES_TOPHINGE_X, BOBBIES_TOPHINGE_Y, VO_BLT.SRCTRANSPARENCY, null);
        // 
        // // Bottom Hinge
        // hPixHandle = video.GetVideoObject(guiBottomHinge);
        // VideoObjectManager.BltVideoObject(SurfaceType.FRAME_BUFFER, hPixHandle, 0, BOBBIES_BOTTOMHINGE_X, BOBBIES_BOTTOMHINGE_Y, VO_BLT.SRCTRANSPARENCY, null);
        // 
        // // StorePlaque
        // hStorePlaqueHandle = video.GetVideoObject(guiStorePlaque);
        // VideoObjectManager.BltVideoObject(SurfaceType.FRAME_BUFFER, hStorePlaqueHandle, 0, BOBBIES_STORE_PLAQUE_X, BOBBIES_STORE_PLAQUE_Y, VO_BLT.SRCTRANSPARENCY, null);
        // 
        // // Handle
        // hPixHandle = video.GetVideoObject(guiHandle);
        // VideoObjectManager.BltVideoObject(SurfaceType.FRAME_BUFFER, hPixHandle, 0, BOBBIES_HANDLE_X, BOBBIES_HANDLE_Y, VO_BLT.SRCTRANSPARENCY, null);

        /*
            if( !LaptopSaveInfo.fBobbyRSiteCanBeAccessed )
            {
                // The undercontsruction graphic 
                GetVideoObject(&hPixHandle, guiUnderConstructionImage );
                BltVideoObject(FRAME_BUFFER, hPixHandle, 0,BOBBIES_FIRST_SENTENCE_X, BOBBIES_FIRST_SENTENCE_Y, VO_BLT.SRCTRANSPARENCY,null);
                BltVideoObject(FRAME_BUFFER, hPixHandle, 0,BOBBIES_3RD_SENTENCE_X, BOBBIES_3RD_SENTENCE_Y, VO_BLT.SRCTRANSPARENCY,null);
            }
        */

        FontSubSystem.SetFontShadow(BOBBIES_SENTENCE_BACKGROUNDCOLOR);


        if (LaptopSaveInfo.fBobbyRSiteCanBeAccessed)
        {
            //Bobbys first sentence
            //	ShadowText( FRAME_BUFFER, BobbyRaysFrontText[BOBBYR_ADVERTISMENT_1], BOBBIES_SENTENCE_FONT, BOBBIES_FIRST_SENTENCE_X, BOBBIES_FIRST_SENTENCE_Y );
            FontSubSystem.DrawTextToScreen(
                BobbyRaysFrontText[(int)BOBBYR.ADVERTISMENT_1],
                new(BOBBIES_FIRST_SENTENCE_X, BOBBIES_FIRST_SENTENCE_Y),
                BOBBIES_FIRST_SENTENCE_WIDTH,
                BOBBIES_SENTENCE_FONT,
                BOBBIES_SENTENCE_COLOR,
                BOBBIES_SIGN_BACKCOLOR,
                TextJustifies.CENTER_JUSTIFIED | TextJustifies.TEXT_SHADOWED);

            //Bobbys second sentence
            FontSubSystem.DrawTextToScreen(
                BobbyRaysFrontText[(int)BOBBYR.ADVERTISMENT_2],
                new(BOBBIES_2ND_SENTENCE_X, BOBBIES_2ND_SENTENCE_Y),
                BOBBIES_2ND_SENTENCE_WIDTH,
                BOBBIES_SENTENCE_FONT, BOBBIES_SENTENCE_COLOR,
                BOBBIES_SIGN_BACKCOLOR,
                TextJustifies.CENTER_JUSTIFIED | TextJustifies.TEXT_SHADOWED);

            FontSubSystem.SetFontShadow(FontShadow.DEFAULT_SHADOW);
        }


        FontSubSystem.SetFontShadow(BOBBIES_SIGN_BACKGROUNDCOLOR);
        //Text on the Used Sign
        FontSubSystem.DisplayWrappedString(
            new(BOBBIES_USED_SIGN_X, BOBBIES_USED_SIGN_TEXT_OFFSET),
            BOBBIES_USED_SIGN_WIDTH - 5,
            2,
            BOBBIES_SIGN_FONT,
            BOBBIES_SIGN_COLOR,
            BobbyRaysFrontText[(int)BOBBYR.USED],
            BOBBIES_SIGN_BACKCOLOR,
            TextJustifies.CENTER_JUSTIFIED);

        //Text on the Misc Sign
        FontSubSystem.DisplayWrappedString(new(BOBBIES_MISC_SIGN_X, BOBBIES_MISC_SIGN_TEXT_OFFSET), BOBBIES_MISC_SIGN_WIDTH, 2, BOBBIES_SIGN_FONT, BOBBIES_SIGN_COLOR, BobbyRaysFrontText[(int)BOBBYR.MISC], BOBBIES_SIGN_BACKCOLOR, TextJustifies.CENTER_JUSTIFIED);
        //Text on the Guns Sign
        FontSubSystem.DisplayWrappedString(new(BOBBIES_GUNS_SIGN_X, BOBBIES_GUNS_SIGN_TEXT_OFFSET), BOBBIES_GUNS_SIGN_WIDTH, 2, BOBBIES_SIGN_FONT, BOBBIES_SIGN_COLOR, BobbyRaysFrontText[(int)BOBBYR.GUNS], BOBBIES_SIGN_BACKCOLOR, TextJustifies.CENTER_JUSTIFIED);
        //Text on the Ammo Sign
        FontSubSystem.DisplayWrappedString(new(BOBBIES_AMMO_SIGN_X, BOBBIES_AMMO_SIGN_TEXT_OFFSET), BOBBIES_AMMO_SIGN_WIDTH, 2, BOBBIES_SIGN_FONT, BOBBIES_SIGN_COLOR, BobbyRaysFrontText[(int)BOBBYR.AMMO], BOBBIES_SIGN_BACKCOLOR, TextJustifies.CENTER_JUSTIFIED);
        //Text on the Armour Sign
        FontSubSystem.DisplayWrappedString(new(BOBBIES_ARMOUR_SIGN_X, BOBBIES_ARMOUR_SIGN_TEXT_OFFSET), BOBBIES_ARMOUR_SIGN_WIDTH, 2, BOBBIES_SIGN_FONT, BOBBIES_SIGN_COLOR, BobbyRaysFrontText[(int)BOBBYR.ARMOR], BOBBIES_SIGN_BACKCOLOR, TextJustifies.CENTER_JUSTIFIED);
        FontSubSystem.SetFontShadow(FontShadow.DEFAULT_SHADOW);


        if (LaptopSaveInfo.fBobbyRSiteCanBeAccessed)
        {
            //Bobbys Third sentence
            FontSubSystem.SetFontShadow(BOBBIES_SENTENCE_BACKGROUNDCOLOR);
            FontSubSystem.DrawTextToScreen(BobbyRaysFrontText[(int)BOBBYR.ADVERTISMENT_3], new(BOBBIES_3RD_SENTENCE_X, BOBBIES_3RD_SENTENCE_Y), BOBBIES_3RD_SENTENCE_WIDTH, BOBBIES_SENTENCE_FONT, BOBBIES_SENTENCE_COLOR, BOBBIES_SIGN_BACKCOLOR, TextJustifies.CENTER_JUSTIFIED | TextJustifies.TEXT_SHADOWED);
            FontSubSystem.SetFontShadow(FontShadow.DEFAULT_SHADOW);
        }

        //if we cant go to any sub pages, darken the page out
        if (!LaptopSaveInfo.fBobbyRSiteCanBeAccessed)
        {
            VideoSurfaceManager.ShadowVideoSurfaceRect(SurfaceType.FRAME_BUFFER, new(LAPTOP_SCREEN_UL_X, LAPTOP_SCREEN_WEB_UL_Y, LAPTOP_SCREEN_LR_X, LAPTOP_SCREEN_WEB_LR_Y));
        }

        Laptop.RenderWWWProgramTitleBar();
        video.InvalidateRegion(LAPTOP_SCREEN_UL_X, LAPTOP_SCREEN_WEB_UL_Y, LAPTOP_SCREEN_LR_X, LAPTOP_SCREEN_WEB_LR_Y);
    }

    bool InitBobbyRWoodBackground()
    {
        //VOBJECT_DESC VObjectDesc;
        // load the Wood bacground graphic and add it
        //
        string filename = Utils.FilenameForBPP("LAPTOP\\BobbyWood.sti");
        video.GetVideoObject(filename, out guiWoodBackground);

        return true;
    }

    private static bool DeleteBobbyRWoodBackground()
    {
        video.DeleteVideoObjectFromIndex(guiWoodBackground);
        return true;
    }


    bool DrawBobbyRWoodBackground()
    {
        int x, y, uiPosX, uiPosY;

        // Blt the Wood background
        HVOBJECT hWoodBackGroundHandle = null; //video.GetVideoObject(guiWoodBackground);

        uiPosY = BOBBY_WOOD_BACKGROUND_Y;
        for (y = 0; y < 4; y++)
        {
            uiPosX = BOBBY_WOOD_BACKGROUND_X;
            for (x = 0; x < 4; x++)
            {
                VideoObjectManager.BltVideoObject(SurfaceType.FRAME_BUFFER, hWoodBackGroundHandle, 0, uiPosX, uiPosY, VO_BLT.SRCTRANSPARENCY, null);
                uiPosX += BOBBY_WOOD_BACKGROUND_WIDTH;
            }
            uiPosY += BOBBY_WOOD_BACKGROUND_HEIGHT;
        }

        return true;
    }


    bool InitBobbiesMouseRegion(int ubNumerRegions, List<int> usMouseRegionPosArray, List<MOUSE_REGION> MouseRegion)
    {
        int i, ubCount = 0;

        for (i = 0; i < ubNumerRegions; i++)
        {
            var region = MouseRegion[i];

            //Mouse region for the toc buttons
            MouseSubSystem.MSYS_DefineRegion(
                region,
                new SixLabors.ImageSharp.Rectangle(usMouseRegionPosArray[ubCount],
                usMouseRegionPosArray[ubCount + 1],
                usMouseRegionPosArray[ubCount + 2],
                usMouseRegionPosArray[ubCount + 3]),
                MSYS_PRIORITY.HIGH,
                CURSOR.WWW,
                MSYS_NO_CALLBACK,
                this.SelectBobbiesSignMenuRegionCallBack);

            MouseSubSystem.MSYS_AddRegion(ref region);
            MouseSubSystem.MSYS_SetRegionUserData(MouseRegion[i], 0, gubBobbyRPages[i]);

            ubCount += 4;
        }


        return true;
    }


    private static bool RemoveBobbiesMouseRegion(int ubNumberRegions, List<MOUSE_REGION> Mouse_Region)
    {
        int i;

        for (i = 0; i < ubNumberRegions; i++)
        {
            MouseSubSystem.MSYS_RemoveRegion(Mouse_Region[i]);
        }

        return true;
    }




    void SelectBobbiesSignMenuRegionCallBack(ref MOUSE_REGION pRegion, MSYS_CALLBACK_REASON iReason)
    {
        if (iReason.HasFlag(MSYS_CALLBACK_REASON.INIT))
        {

        }
        else if (iReason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP))
        {
            LAPTOP_MODE ubNewPage = (LAPTOP_MODE)MouseSubSystem.MSYS_GetRegionUserData(ref pRegion, 0);
            guiCurrentLaptopMode = ubNewPage;
            //		FindLastItemIndex(ubNewPage);

        }
        else if (iReason.HasFlag(MSYS_CALLBACK_REASON.RBUTTON_UP))
        {
        }
    }


    /*
    bool WebPageTileBackground(int ubNumX, int ubNumY, int usWidth, int usHeight, int uiBackground)
    {
      HVOBJECT hBackGroundHandle;
        int	x,y, uiPosX, uiPosY;

        // Blt the Wood background
        GetVideoObject(&hBackGroundHandle, uiBackground);

        uiPosY = LAPTOP_SCREEN_WEB_UL_Y;
        for(y=0; y<ubNumY; y++)
        {
            uiPosX = LAPTOP_SCREEN_UL_X;
            for(x=0; x<ubNumX; x++)
            {
              BltVideoObject(FRAME_BUFFER, hBackGroundHandle, 0,uiPosX, uiPosY, VO_BLT.SRCTRANSPARENCY,null);
                uiPosX += usWidth;
            }
            uiPosY += usHeight;
        }
        return(true);
    }
    */


    static uint uiLastTime = 1;
    static ushort usCount = 0;
    private readonly ILogger<BobbyR> logger;
    private static IVideoManager video;

    private static void HandleBobbyRUnderConstructionAni(bool fReset)
    {
        HVOBJECT hPixHandle;
        uint uiCurTime = ClockManager.GetJA2Clock();


        if (LaptopSaveInfo.fBobbyRSiteCanBeAccessed)
        {
            return;
        }

        if (fReset)
        {
            usCount = 1;
        }

        if (fShowBookmarkInfo)
        {
            fReDrawBookMarkInfo = true;
        }

        if (uiCurTime - uiLastTime > BOBBYR_UNDERCONSTRUCTION_ANI_DELAY || fReDrawScreenFlag)
        {
            // The undercontsruction graphic 
            hPixHandle = null; // video.GetVideoObject(guiUnderConstructionImage);
            VideoObjectManager.BltVideoObject(SurfaceType.FRAME_BUFFER, hPixHandle, usCount, BOBBYR_UNDERCONSTRUCTION_X, BOBBYR_UNDERCONSTRUCTION_Y, VO_BLT.SRCTRANSPARENCY, null);
            VideoObjectManager.BltVideoObject(SurfaceType.FRAME_BUFFER, hPixHandle, usCount, BOBBYR_UNDERCONSTRUCTION_X, BOBBYR_UNDERCONSTRUCTION1_Y, VO_BLT.SRCTRANSPARENCY, null);

            FontSubSystem.DrawTextToScreen(BobbyRaysFrontText[(int)BOBBYR.UNDER_CONSTRUCTION], new(BOBBYR_UNDER_CONSTRUCTION_TEXT_X, BOBBYR_UNDER_CONSTRUCTION_TEXT_Y), BOBBYR_UNDER_CONSTRUCTION_TEXT_WIDTH, FontStyle.FONT16ARIAL, BOBBIES_SENTENCE_COLOR, BOBBIES_SIGN_BACKCOLOR, TextJustifies.CENTER_JUSTIFIED | TextJustifies.INVALIDATE_TEXT);

            video.InvalidateRegion(new(BOBBYR_UNDERCONSTRUCTION_X, BOBBYR_UNDERCONSTRUCTION_Y, BOBBYR_UNDERCONSTRUCTION_X + BOBBYR_UNDERCONSTRUCTION_WIDTH, BOBBYR_UNDERCONSTRUCTION_Y + BOBBYR_UNDERCONSTRUCTION_HEIGHT));
            video.InvalidateRegion(new(BOBBYR_UNDERCONSTRUCTION_X, BOBBYR_UNDERCONSTRUCTION1_Y, BOBBYR_UNDERCONSTRUCTION_X + BOBBYR_UNDERCONSTRUCTION_WIDTH, BOBBYR_UNDERCONSTRUCTION1_Y + BOBBYR_UNDERCONSTRUCTION_HEIGHT));

            uiLastTime = ClockManager.GetJA2Clock();

            usCount++;

            if (usCount >= BOBBYR_UNDERCONSTRUCTION_NUM_FRAMES)
            {
                usCount = 0;
            }
        }
    }



    public void InitBobbyRayInventory()
    {
        //Initializes which NEW items can be bought at Bobby Rays
        this.InitBobbyRayNewInventory();

        //Initializes the starting values for Bobby Rays NEW Inventory
        LaptopSave.SetupStoreInventory(LaptopSaveInfo.BobbyRayInventory, BOBBY_RAY.NEW);

        //Initializes which USED items can be bought at Bobby Rays
        this.InitBobbyRayUsedInventory();

        //Initializes the starting values for Bobby Rays USED Inventory
        LaptopSave.SetupStoreInventory(LaptopSaveInfo.BobbyRayUsedInventory, BOBBY_RAY.USED);
    }


    bool InitBobbyRayNewInventory()
    {
        Items i;
        int usBobbyrIndex = 0;

        //memset(LaptopSaveInfo.BobbyRayInventory, 0, sizeof(STORE_INVENTORY) * MAXITEMS);

        // add all the NEW items he can ever sell into his possible inventory list, for now in order by item #
        for (i = 0; i < Items.MAXITEMS; i++)
        {
            //if Bobby Ray sells this, it can be sold, and it's allowed into this game (some depend on e.g. gun-nut option)
            if (storeInventory[(int)i, (int)BOBBY_RAY.NEW] != 0 && !Item[i].fFlags.HasFlag(ItemAttributes.ITEM_NOT_BUYABLE) && ItemSubSystem.ItemIsLegal(i))
            {
                LaptopSaveInfo.BobbyRayInventory.Add(new());
                LaptopSaveInfo.BobbyRayInventory[usBobbyrIndex].usItemIndex = i;
                usBobbyrIndex++;
            }
        }

        if (usBobbyrIndex > 1)
        {
            // sort this list by object category, and by ascending price within each category
            //qsort(LaptopSaveInfo.BobbyRayInventory, usBobbyrIndex, sizeof(STORE_INVENTORY), BobbyRayItemQsortCompare);
        }

        // remember how many entries in the list are valid
        LaptopSaveInfo.usInventoryListLength[BOBBY_RAY.NEW] = usBobbyrIndex;
        // also mark the end of the list of valid item entries
        LaptopSaveInfo.BobbyRayInventory.Add(new()
        {
            usItemIndex = BOBBYR_NO_ITEMS,
        });

        LaptopSaveInfo.BobbyRayInventory[usBobbyrIndex].usItemIndex = BOBBYR_NO_ITEMS;

        return true;
    }


    bool InitBobbyRayUsedInventory()
    {
        Items i;
        int usBobbyrIndex = 0;


        LaptopSaveInfo.BobbyRayUsedInventory = new List<STORE_INVENTORY>();

        // add all the NEW items he can ever sell into his possible inventory list, for now in order by item #
        for (i = 0; i < Items.MAXITEMS; i++)
        {
            //if Bobby Ray sells this, it can be sold, and it's allowed into this game (some depend on e.g. gun-nut option)
            if (storeInventory[(int)i, (int)BOBBY_RAY.USED] != 0 && !Item[i].fFlags.HasFlag(ItemAttributes.ITEM_NOT_BUYABLE) && ItemSubSystem.ItemIsLegal(i))
            {
                if (storeInventory[(int)i, (int)BOBBY_RAY.USED] != 0 && !Item[i].fFlags.HasFlag(ItemAttributes.ITEM_NOT_BUYABLE) && ItemSubSystem.ItemIsLegal(i))
                {
                    // in case his store inventory list is wrong, make sure this category of item can be sold used
                    if (ArmsDealerInit.CanDealerItemBeSoldUsed(i))
                    {
                        LaptopSaveInfo.BobbyRayUsedInventory.Add(new());
                        LaptopSaveInfo.BobbyRayUsedInventory[usBobbyrIndex].usItemIndex = i;
                        usBobbyrIndex++;
                    }
                }
            }
        }

        if (usBobbyrIndex > 1)
        {
            // sort this list by object category, and by ascending price within each category
            //qsort(LaptopSaveInfo.BobbyRayUsedInventory, usBobbyrIndex, sizeof(STORE_INVENTORY), BobbyRayItemQsortCompare);
        }

        // remember how many entries in the list are valid
        LaptopSaveInfo.usInventoryListLength[BOBBY_RAY.USED] = usBobbyrIndex;

        // also mark the end of the list of valid item entries
        LaptopSaveInfo.BobbyRayUsedInventory.Add(new());
        LaptopSaveInfo.BobbyRayUsedInventory[usBobbyrIndex].usItemIndex = BOBBYR_NO_ITEMS;

        return true;
    }

    void DailyUpdateOfBobbyRaysNewInventory()
    {
        int i;
        Items usItemIndex;
        bool fPrevElig;


        //simulate other buyers by reducing the current quantity on hand
        this.SimulateBobbyRayCustomer(LaptopSaveInfo.BobbyRayInventory, BOBBY_RAY.NEW);

        //loop through all items BR can stock to see what needs reordering
        for (i = 0; i < LaptopSaveInfo.usInventoryListLength[BOBBY_RAY.NEW]; i++)
        {
            // the index is NOT the item #, get that from the table
            usItemIndex = LaptopSaveInfo.BobbyRayInventory[i].usItemIndex;

            Debug.Assert(usItemIndex < Items.MAXITEMS);

            // make sure this item is still sellable in the latest version of the store inventory
            if (storeInventory[(int)usItemIndex, (int)BOBBY_RAY.NEW] == 0)
            {
                continue;
            }

            //if the item isn't already on order
            if (LaptopSaveInfo.BobbyRayInventory[i].ubQtyOnOrder == 0)
            {
                //if the qty on hand is half the desired amount or fewer
                if (LaptopSaveInfo.BobbyRayInventory[i].ubQtyOnHand <= storeInventory[(int)usItemIndex, (int)BOBBY_RAY.NEW] / 2)
                {
                    // remember value of the "previously eligible" flag
                    fPrevElig = LaptopSaveInfo.BobbyRayInventory[i].fPreviouslyEligible;

                    //determine if any can/should be ordered, and how many
                    LaptopSaveInfo.BobbyRayInventory[i].ubQtyOnOrder = this.HowManyBRItemsToOrder(usItemIndex, LaptopSaveInfo.BobbyRayInventory[i].ubQtyOnHand, BOBBY_RAY.NEW);

                    //if he found some to buy
                    if (LaptopSaveInfo.BobbyRayInventory[i].ubQtyOnOrder > 0)
                    {
                        // if this is the first day the player is eligible to have access to this thing
                        if (!fPrevElig)
                        {
                            // eliminate the ordering delay and stock the items instantly!
                            // This is just a way to reward the player right away for making progress without the reordering lag...
                            AddFreshBobbyRayInventory(usItemIndex);
                        }
                        else
                        {
                            this.OrderBobbyRItem(usItemIndex);

# if BR_INVENTORY_TURNOVER_DEBUG
                            if (usItemIndex == ROCKET_LAUNCHER)
                                MapScreenMessage(0, MSG.DEBUG, "%s: BR Ordered %d, Has %d", gswzWorldTimeStr, LaptopSaveInfo.BobbyRayInventory[i].ubQtyOnOrder, LaptopSaveInfo.BobbyRayInventory[i].ubQtyOnHand);
#endif
                        }
                    }
                }
            }
        }
    }


    void DailyUpdateOfBobbyRaysUsedInventory()
    {
        int i;
        Items usItemIndex;
        bool fPrevElig;


        //simulate other buyers by reducing the current quantity on hand
        this.SimulateBobbyRayCustomer(LaptopSaveInfo.BobbyRayUsedInventory, BOBBY_RAY.USED);

        for (i = 0; i < LaptopSaveInfo.usInventoryListLength[BOBBY_RAY.USED]; i++)
        {
            //if the used item isn't already on order
            if (LaptopSaveInfo.BobbyRayUsedInventory[i].ubQtyOnOrder == 0)
            {
                //if we don't have ANY
                if (LaptopSaveInfo.BobbyRayUsedInventory[i].ubQtyOnHand == 0)
                {
                    // the index is NOT the item #, get that from the table
                    usItemIndex = LaptopSaveInfo.BobbyRayUsedInventory[i].usItemIndex;
                    Debug.Assert(usItemIndex < Items.MAXITEMS);

                    // make sure this item is still sellable in the latest version of the store inventory
                    if (storeInventory[(int)usItemIndex, (int)BOBBY_RAY.USED] == 0)
                    {
                        continue;
                    }

                    // remember value of the "previously eligible" flag
                    fPrevElig = LaptopSaveInfo.BobbyRayUsedInventory[i].fPreviouslyEligible;

                    //determine if any can/should be ordered, and how many
                    LaptopSaveInfo.BobbyRayUsedInventory[i].ubQtyOnOrder = this.HowManyBRItemsToOrder(usItemIndex, LaptopSaveInfo.BobbyRayUsedInventory[i].ubQtyOnHand, BOBBY_RAY.USED);

                    //if he found some to buy
                    if (LaptopSaveInfo.BobbyRayUsedInventory[i].ubQtyOnOrder > 0)
                    {
                        // if this is the first day the player is eligible to have access to this thing
                        if (!fPrevElig)
                        {
                            // eliminate the ordering delay and stock the items instantly!
                            // This is just a way to reward the player right away for making progress without the reordering lag...
                            AddFreshBobbyRayInventory(usItemIndex);
                        }
                        else
                        {
                            this.OrderBobbyRItem(usItemIndex + BOBBY_R_USED_PURCHASE_OFFSET);
                        }
                    }
                }
            }
        }
    }


    //returns the number of items to order
    int HowManyBRItemsToOrder(Items usItemIndex, int ubCurrentlyOnHand, BOBBY_RAY ubBobbyRayNewUsed)
    {
        int ubItemsOrdered = 0;


        Debug.Assert(usItemIndex < Items.MAXITEMS);
        // formulas below will fail if there are more items already in stock than optimal
        Debug.Assert(ubCurrentlyOnHand <= storeInventory[(int)usItemIndex, (int)ubBobbyRayNewUsed]);
        Debug.Assert(ubBobbyRayNewUsed < BOBBY_RAY.LISTS);


        // decide if he can get stock for this item (items are reordered an entire batch at a time)
        if (ArmsDealerInit.ItemTransactionOccurs((ARMS_DEALER)(-1), usItemIndex, DEALER_BUYING, ubBobbyRayNewUsed))
        {
            if (ubBobbyRayNewUsed == BOBBY_RAY.NEW)
            {
                ubItemsOrdered = ArmsDealerInit.HowManyItemsToReorder(storeInventory[(int)usItemIndex, (int)ubBobbyRayNewUsed], ubCurrentlyOnHand);
            }
            else
            {
                //Since these are used items we only should order 1 of each type
                ubItemsOrdered = 1;
            }
        }
        else
        {
            // can't obtain this item from suppliers
            ubItemsOrdered = 0;
        }


        return ubItemsOrdered;
    }


    void OrderBobbyRItem(Items usItemIndex)
    {
        uint uiArrivalTime;

        //add the new item to the queue.  The new item will arrive in 'uiArrivalTime' minutes.
        uiArrivalTime = (uint)(BOBBY_R_NEW_PURCHASE_ARRIVAL_TIME + Globals.Random.Next(BOBBY_R_NEW_PURCHASE_ARRIVAL_TIME / 2));
        uiArrivalTime += GameClock.GetWorldTotalMin();
        GameEvents.AddStrategicEvent(EVENT.UPDATE_BOBBY_RAY_INVENTORY, uiArrivalTime, usItemIndex);
    }


    public static void AddFreshBobbyRayInventory(Items usItemIndex)
    {
        int sInventorySlot;
        List<STORE_INVENTORY> pInventoryArray;
        BOBBY_RAY fUsed;
        int ubItemQuality;


        if (usItemIndex >= (Items)BOBBY_R_USED_PURCHASE_OFFSET)
        {
            usItemIndex -= BOBBY_R_USED_PURCHASE_OFFSET;
            pInventoryArray = LaptopSaveInfo.BobbyRayUsedInventory;
            fUsed = BOBBY_RAY.USED;
            ubItemQuality = 20 + Globals.Random.Next(60);
        }
        else
        {
            pInventoryArray = LaptopSaveInfo.BobbyRayInventory;
            fUsed = BOBBY_RAY.NEW;
            ubItemQuality = 100;
        }


        // find out which inventory slot that item is stored in
        sInventorySlot = GetInventorySlotForItem(pInventoryArray, usItemIndex, fUsed);
        if (sInventorySlot == -1)
        {
            //AssertMsg(false, string.Format("AddFreshBobbyRayInventory(), Item %d not found.  AM-0.", usItemIndex));
            return;
        }

        pInventoryArray[sInventorySlot].ubQtyOnHand += pInventoryArray[sInventorySlot].ubQtyOnOrder;
        pInventoryArray[sInventorySlot].ubItemQuality = ubItemQuality;

        // cancel order
        pInventoryArray[sInventorySlot].ubQtyOnOrder = 0;
    }


    public static int GetInventorySlotForItem(List<STORE_INVENTORY> pInventoryArray, Items usItemIndex, BOBBY_RAY fUsed)
    {
        int i;

        for (i = 0; i < LaptopSaveInfo.usInventoryListLength[fUsed]; i++)
        {
            //if we have some of this item in stock
            if (pInventoryArray[i].usItemIndex == usItemIndex)
            {
                return i;
            }
        }

        // not found!
        return -1;
    }


    void SimulateBobbyRayCustomer(List<STORE_INVENTORY> pInventoryArray, BOBBY_RAY fUsed)
    {
        int i;
        int ubItemsSold;

        //loop through all items BR can stock to see what gets sold
        for (i = 0; i < LaptopSaveInfo.usInventoryListLength[fUsed]; i++)
        {
            //if we have some of this item in stock
            if (pInventoryArray[i].ubQtyOnHand > 0)
            {
                ubItemsSold = ArmsDealerInit.HowManyItemsAreSold((ARMS_DEALER)(-1), pInventoryArray[i].usItemIndex, pInventoryArray[i].ubQtyOnHand, fUsed);
                pInventoryArray[i].ubQtyOnHand -= ubItemsSold;
            }
        }
    }


    void CancelAllPendingBRPurchaseOrders()
    {
        Items i;

        // remove all the BR-Order events off the event queue
        GameEvents.DeleteAllStrategicEventsOfType(EVENT.UPDATE_BOBBY_RAY_INVENTORY);

        // zero out all the quantities on order
        for (i = 0; i < Items.MAXITEMS; i++)
        {
            LaptopSaveInfo.BobbyRayInventory[(int)i].ubQtyOnOrder = 0;
            LaptopSaveInfo.BobbyRayUsedInventory[(int)i].ubQtyOnOrder = 0;
        }

        // do an extra daily update immediately to create new reorders ASAP
        this.DailyUpdateOfBobbyRaysNewInventory();
        this.DailyUpdateOfBobbyRaysUsedInventory();
    }

    public static void GameInitBobbyR()
    {
    }
}

public enum BOBBYR
{
    ADVERTISMENT_1,
    ADVERTISMENT_2,
    USED,
    MISC,
    GUNS,
    AMMO,
    ARMOR,
    ADVERTISMENT_3,
    UNDER_CONSTRUCTION,
};

//used when the player goes to bobby rays when it is still down
public enum BOBBYR_VISITS
{
    NEVER_BEEN_TO_SITE,
    BEEN_TO_SITE_ONCE,
    ALREADY_SENT_EMAIL,
};
