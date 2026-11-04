using System;
using SharpAlliance.Core.Managers;
using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core.SubSystems;

/// <summary>
/// Item pool core from Handle Items.c: the per-tile linked lists of items lying on the ground,
/// their visibility, and the "locator" flash slots. The pool head lives on the LEVELNODE that carries
/// the item graphic (LEVELNODEFLAGS.ITEM) in the struct list (ground) or on-roof list (roof).
/// </summary>
public partial class HandleItems
{
    public const int NUM_ITEM_FLASH_SLOTS = 50;
    public const int ITEM_LOCATOR_LOCKED = 0x02;

    public static readonly ITEM_POOL_LOCATOR[] FlashItemSlots = CreateFlashItemSlots();
    public static int guiNumFlashItemSlots = 0;

    private static ITEM_POOL_LOCATOR[] CreateFlashItemSlots()
    {
        var slots = new ITEM_POOL_LOCATOR[NUM_ITEM_FLASH_SLOTS];
        for (int i = 0; i < slots.Length; i++)
        {
            slots[i] = new ITEM_POOL_LOCATOR();
        }

        return slots;
    }

    // ---- graphics (level nodes) -----------------------------------------------------------------

    /// <summary>
    /// False until the tile database has been loaded. Without it there are no tile indexes to look up, so
    /// item nodes are linked into the level-node lists directly (what the unit tests and tools use).
    /// </summary>
    private static bool TileDatabaseLoaded => gTileDatabase.Count > 0;

    private static TileIndexes GetTileGraphicForItem(INVTYPE pItem)
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

        return usIndex;
    }

    public static LEVELNODE? AddItemGraphicToWorld(INVTYPE pItem, int sGridNo, int ubLevel)
    {
        LEVELNODE? pNode;
        MAP_ELEMENT element = gpWorldLevelData[sGridNo];

        if (TileDatabaseLoaded)
        {
            TileIndexes usTileIndex = GetTileGraphicForItem(pItem);

            // OK, Do stuff differently base on level!
            if (ubLevel == 0)
            {
                pNode = WorldManager.AddStructToTail(sGridNo, usTileIndex);
            }
            else
            {
                WorldManager.AddOnRoofToHead(sGridNo, usTileIndex);
                pNode = element.pOnRoofHead;
            }
        }
        else
        {
            pNode = new LEVELNODE { usIndex = TileIndexes.NO_TILE };
            if (ubLevel == 0)
            {
                // add to the tail of the struct list
                if (element.pStructHead is null)
                {
                    element.pStructHead = pNode;
                }
                else
                {
                    LEVELNODE tail = element.pStructHead;
                    while (tail.pNext is not null)
                    {
                        tail = tail.pNext;
                    }

                    tail.pNext = pNode;
                }
            }
            else
            {
                // add to the head of the on-roof list
                pNode.pNext = element.pOnRoofHead;
                element.pOnRoofHead = pNode;
            }
        }

        if (pNode is null)
        {
            return null;
        }

        //SET FLAG FOR AN ITEM
        pNode.uiFlags |= LEVELNODEFLAGS.ITEM;

        // DIRTY INTERFACE
        fInterfacePanelDirty = DIRTYLEVEL2;

        // DIRTY TILE
        element.uiFlags |= MAPELEMENTFLAGS.REDRAW;
        RenderWorld.SetRenderFlags(RenderingFlags.MARKED);

        return pNode;
    }

    public static void RemoveItemGraphicFromWorld(INVTYPE pItem, int sGridNo, int ubLevel, LEVELNODE? pLevelNode)
    {
        MAP_ELEMENT element = gpWorldLevelData[sGridNo];

        // OK, Do stuff differently base on level!
        // Loop through and find pointer....
        LEVELNODE? pNode = ubLevel == 0 ? element.pStructHead : element.pOnRoofHead;

        while (pNode is not null)
        {
            if (pNode == pLevelNode)
            {
                // Found one!
                if (TileDatabaseLoaded)
                {
                    if (ubLevel == 0)
                    {
                        WorldManager.RemoveStructFromLevelNode(sGridNo, pNode);
                    }
                    else
                    {
                        WorldManager.RemoveOnRoofFromLevelNode(sGridNo, pNode);
                    }
                }
                else
                {
                    UnlinkLevelNode(element, ubLevel, pNode);
                }

                break;
            }

            pNode = pNode.pNext;
        }

        // DIRTY INTERFACE
        fInterfacePanelDirty = DIRTYLEVEL2;

        // DIRTY TILE
        element.uiFlags |= MAPELEMENTFLAGS.REDRAW;
        RenderWorld.SetRenderFlags(RenderingFlags.MARKED);

        //TEMP RENDER FULL!!!
        RenderWorld.SetRenderFlags(RenderingFlags.FULL);
    }

    private static void UnlinkLevelNode(MAP_ELEMENT element, int ubLevel, LEVELNODE pNode)
    {
        LEVELNODE? head = ubLevel == 0 ? element.pStructHead : element.pOnRoofHead;
        if (head == pNode)
        {
            if (ubLevel == 0)
            {
                element.pStructHead = pNode.pNext;
            }
            else
            {
                element.pOnRoofHead = pNode.pNext;
            }

            return;
        }

        for (LEVELNODE? n = head; n is not null; n = n.pNext)
        {
            if (n.pNext == pNode)
            {
                n.pNext = pNode.pNext;
                return;
            }
        }
    }

    // ---- queries ------------------------------------------------------------------------------------

    public static bool ItemExistsAtLocation(int sGridNo, int iItemIndex, int ubLevel)
    {
        // Check for an existing pool on the object layer
        if (GetItemPool(sGridNo, out ITEM_POOL? pItemPool, ubLevel))
        {
            // LOOP THROUGH LIST TO FIND NODE WE WANT
            for (ITEM_POOL? pItemPoolTemp = pItemPool; pItemPoolTemp is not null; pItemPoolTemp = pItemPoolTemp.pNext)
            {
                if (pItemPoolTemp.iItemIndex == iItemIndex)
                {
                    return true;
                }
            }
        }

        return false;
    }

    public static int GetItemOfClassTypeInPool(int sGridNo, IC uiItemClass, int ubLevel)
    {
        // Check for an existing pool on the object layer
        if (GetItemPool(sGridNo, out ITEM_POOL? pItemPool, ubLevel))
        {
            // LOOP THROUGH LIST TO FIND NODE WE WANT
            for (ITEM_POOL? pItemPoolTemp = pItemPool; pItemPoolTemp is not null; pItemPoolTemp = pItemPoolTemp.pNext)
            {
                if (Item[gWorldItems[pItemPoolTemp.iItemIndex].o.usItem].usItemClass.HasFlag(uiItemClass))
                {
                    return pItemPoolTemp.iItemIndex;
                }
            }
        }

        return -1;
    }

    public static ITEM_POOL? GetItemPoolForIndex(int sGridNo, int iItemIndex, int ubLevel)
    {
        // Check for an existing pool on the object layer
        if (GetItemPool(sGridNo, out ITEM_POOL? pItemPool, ubLevel))
        {
            // LOOP THROUGH LIST TO FIND NODE WE WANT
            for (ITEM_POOL? pItemPoolTemp = pItemPool; pItemPoolTemp is not null; pItemPoolTemp = pItemPoolTemp.pNext)
            {
                if (pItemPoolTemp.iItemIndex == iItemIndex)
                {
                    return pItemPoolTemp;
                }
            }
        }

        return null;
    }

    public static bool DoesItemPoolContainAnyHiddenItems(ITEM_POOL? pItemPool)
    {
        for (; pItemPool is not null; pItemPool = pItemPool.pNext)
        {
            if (gWorldItems[pItemPool.iItemIndex].bVisible == ItemVisibility.HIDDEN_ITEM)
            {
                return true;
            }
        }

        return false;
    }

    public static bool DoesItemPoolContainAllHiddenItems(ITEM_POOL? pItemPool)
    {
        for (; pItemPool is not null; pItemPool = pItemPool.pNext)
        {
            if (gWorldItems[pItemPool.iItemIndex].bVisible != ItemVisibility.HIDDEN_ITEM)
            {
                return false;
            }
        }

        return true;
    }

    public static bool LookForHiddenItems(int sGridNo, int ubLevel, bool fSetLocator, int bZLevel)
    {
        ITEM_POOL? pHeadItemPool = null;
        bool fFound = false;

        if (GetItemPool(sGridNo, out ITEM_POOL? pItemPool, ubLevel))
        {
            pHeadItemPool = pItemPool;

            // LOOP THROUGH LIST TO FIND NODE WE WANT
            for (; pItemPool is not null; pItemPool = pItemPool.pNext)
            {
                WORLDITEM worldItem = gWorldItems[pItemPool.iItemIndex];
                if (worldItem.bVisible == ItemVisibility.HIDDEN_ITEM && worldItem.o.usItem != Items.OWNERSHIP)
                {
                    fFound = true;

                    worldItem.bVisible = ItemVisibility.INVISIBLE;
                }
            }
        }

        // If found, set item pool visibility...
        if (fFound)
        {
            SetItemPoolVisibilityOn(pHeadItemPool!, ItemVisibility.INVISIBLE, fSetLocator);
        }

        return fFound;
    }

    public static int GetZLevelOfItemPoolGivenStructure(int sGridNo, int ubLevel, STRUCTURE? pStructure)
    {
        if (pStructure is null)
        {
            return 0;
        }

        // OK, check if this struct contains items....
        if (GetItemPool(sGridNo, out ITEM_POOL? pItemPool, ubLevel))
        {
            return GetLargestZLevelOfItemPool(pItemPool);
        }

        return 0;
    }

    public static int GetLargestZLevelOfItemPool(ITEM_POOL? pItemPool)
    {
        // OK, loop through pools and get any height != 0........
        for (; pItemPool is not null; pItemPool = pItemPool.pNext)
        {
            if (pItemPool.bRenderZHeightAboveLevel > 0)
            {
                return pItemPool.bRenderZHeightAboveLevel;
            }
        }

        return 0;
    }

    public static bool DoesItemPoolContainAllItemsOfHigherZLevel(ITEM_POOL? pItemPool)
    {
        for (; pItemPool is not null; pItemPool = pItemPool.pNext)
        {
            if (pItemPool.bRenderZHeightAboveLevel == 0)
            {
                return false;
            }
        }

        return true;
    }

    public static bool DoesItemPoolContainAllItemsOfZeroZLevel(ITEM_POOL? pItemPool)
    {
        for (; pItemPool is not null; pItemPool = pItemPool.pNext)
        {
            if (pItemPool.bRenderZHeightAboveLevel != 0)
            {
                return false;
            }
        }

        return true;
    }

    public static int GetNumOkForDisplayItemsInPool(ITEM_POOL? pItemPool, int bZLevel)
    {
        //Determine total #
        int cnt = 0;
        for (; pItemPool is not null; pItemPool = pItemPool.pNext)
        {
            if (ItemPoolOKForDisplay(pItemPool, bZLevel))
            {
                cnt++;
            }
        }

        return cnt;
    }

    public static bool AnyItemsVisibleOnLevel(ITEM_POOL? pItemPool, int bZLevel)
    {
        if (gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.SHOW_ALL_ITEMS))
        {
            return true;
        }

        //Determine total #
        for (; pItemPool is not null; pItemPool = pItemPool.pNext)
        {
            if (pItemPool.bRenderZHeightAboveLevel == bZLevel)
            {
                if (gWorldItems[pItemPool.iItemIndex].bVisible == ItemVisibility.VISIBLE)
                {
                    return true;
                }
            }
        }

        return false;
    }

    public static bool ItemPoolOKForDisplay(ITEM_POOL pItemPool, int bZLevel)
    {
        if (gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.SHOW_ALL_ITEMS))
        {
            return true;
        }

        // Setup some conditions!
        if (gWorldItems[pItemPool.iItemIndex].bVisible != ItemVisibility.VISIBLE)
        {
            return false;
        }

        // If -1, it means find all
        if (pItemPool.bRenderZHeightAboveLevel != bZLevel && bZLevel != -1)
        {
            return false;
        }

        return true;
    }

    public static bool ItemPoolOKForPickup(SOLDIERTYPE pSoldier, ITEM_POOL pItemPool, int bZLevel)
    {
        if (gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.SHOW_ALL_ITEMS))
        {
            return true;
        }

        if (pSoldier.bTeam == gbPlayerNum)
        {
            // Setup some conditions!
            if (gWorldItems[pItemPool.iItemIndex].bVisible != ItemVisibility.VISIBLE)
            {
                return false;
            }
        }

        // If -1, it means find all
        if (pItemPool.bRenderZHeightAboveLevel != bZLevel && bZLevel != -1)
        {
            return false;
        }

        return true;
    }

    // ---- visibility -----------------------------------------------------------------------------------

    private static void LoopLevelNodeForShowThroughFlag(LEVELNODE? pNode, int sGridNo, int ubLevel)
    {
        for (; pNode is not null; pNode = pNode.pNext)
        {
            if (pNode.uiFlags.HasFlag(LEVELNODEFLAGS.ITEM))
            {
                // (the original has the roof/cave/basement test commented out for ground level, too)
                pNode.uiFlags |= LEVELNODEFLAGS.SHOW_THROUGH;

                if (GameSettings.fOptions.TryGetValue(TOPTION.GLOW_ITEMS, out bool fGlow) && fGlow)
                {
                    pNode.uiFlags |= LEVELNODEFLAGS.DYNAMIC;
                }
            }
        }
    }

    public static void HandleItemObscuredFlag(int sGridNo, int ubLevel)
    {
        LEVELNODE? pNode = ubLevel == 0
            ? gpWorldLevelData[sGridNo].pStructHead
            : gpWorldLevelData[sGridNo].pOnRoofHead;

        LoopLevelNodeForShowThroughFlag(pNode, sGridNo, ubLevel);
    }

    public static bool SetItemPoolVisibilityOn(ITEM_POOL pItemPool, ItemVisibility bAllGreaterThan, bool fSetLocator)
    {
        bool fAtLeastModified = false;

        for (ITEM_POOL? pItemPoolTemp = pItemPool; pItemPoolTemp is not null; pItemPoolTemp = pItemPoolTemp.pNext)
        {
            WORLDITEM worldItem = gWorldItems[pItemPoolTemp.iItemIndex];
            ItemVisibility bVisibleValue = worldItem.bVisible;

            // Update each item...
            if (bVisibleValue != ItemVisibility.VISIBLE)
            {
                if (worldItem.o.usItem == Items.ACTION_ITEM)
                {
                    // NEVER MAKE VISIBLE!
                    continue;
                }

                // If we have reached a visible value we should not modify, ignore...
                if (bVisibleValue >= bAllGreaterThan && worldItem.o.usItem != Items.OWNERSHIP)
                {
                    // Update the world value
                    worldItem.bVisible = ItemVisibility.VISIBLE;

                    fAtLeastModified = true;
                }

                // (the original has a commented-out block here that armed or destroyed hidden pit action items)
            }
        }

        // If we didn;t find any that should be modified..
        if (!fAtLeastModified)
        {
            return false;
        }

        // Update global pool bVisible to true ( if at least one is visible... )
        for (ITEM_POOL? pItemPoolTemp = pItemPool; pItemPoolTemp is not null; pItemPoolTemp = pItemPoolTemp.pNext)
        {
            pItemPoolTemp.bVisible = ItemVisibility.VISIBLE;
        }

        // Handle obscured flag...
        HandleItemObscuredFlag(pItemPool.sGridNo, pItemPool.ubLevel);

        if (fSetLocator)
        {
            SetItemPoolLocator(pItemPool);
        }

        return true;
    }

    public static void SetItemPoolVisibilityHidden(ITEM_POOL? pItemPool)
    {
        for (ITEM_POOL? pItemPoolTemp = pItemPool; pItemPoolTemp is not null; pItemPoolTemp = pItemPoolTemp.pNext)
        {
            // Update the world value
            gWorldItems[pItemPoolTemp.iItemIndex].bVisible = ItemVisibility.HIDDEN_IN_OBJECT;
            pItemPoolTemp.bVisible = ItemVisibility.HIDDEN_IN_OBJECT;
        }
    }

    // This determines the overall initial visibility of the pool...
    // IF ANY are set to VISIBLE, MODIFY
    public static void AdjustItemPoolVisibility(ITEM_POOL pItemPool)
    {
        bool fAtLeastModified = false;

        for (ITEM_POOL? pItemPoolTemp = pItemPool; pItemPoolTemp is not null; pItemPoolTemp = pItemPoolTemp.pNext)
        {
            // DEFAULT ITEM POOL TO INVISIBLE....
            pItemPoolTemp.bVisible = ItemVisibility.INVISIBLE;

            // Update each item...
            // If we have reached a visible value we should not modify, ignore...
            if (gWorldItems[pItemPoolTemp.iItemIndex].bVisible == ItemVisibility.VISIBLE)
            {
                fAtLeastModified = true;
            }
        }

        // Handle obscured flag...
        HandleItemObscuredFlag(pItemPool.sGridNo, pItemPool.ubLevel);

        // If we didn;t find any that should be modified..
        if (!fAtLeastModified)
        {
            return;
        }

        // Update global pool bVisible to true ( if at least one is visible... )
        for (ITEM_POOL? pItemPoolTemp = pItemPool; pItemPoolTemp is not null; pItemPoolTemp = pItemPoolTemp.pNext)
        {
            pItemPoolTemp.bVisible = ItemVisibility.VISIBLE;
        }

        // Handle obscured flag...
        HandleItemObscuredFlag(pItemPool.sGridNo, pItemPool.ubLevel);
    }

    // ---- removal --------------------------------------------------------------------------------------

    public static bool RemoveItemFromPool(int sGridNo, int iItemIndex, int ubLevel)
    {
        bool fItemFound = false;

        // Check for and existing pool on the object layer
        if (!GetItemPool(sGridNo, out ITEM_POOL? pItemPool, ubLevel))
        {
            return false;
        }

        // REMOVE FROM LIST

        // LOOP THROUGH LIST TO FIND NODE WE WANT
        ITEM_POOL? pItemPoolTemp = pItemPool;
        while (pItemPoolTemp is not null)
        {
            if (pItemPoolTemp.iItemIndex == iItemIndex)
            {
                fItemFound = true;
                break;
            }

            pItemPoolTemp = pItemPoolTemp.pNext;
        }

        if (!fItemFound || pItemPoolTemp is null)
        {
            // COULDNOT FIND ITEM? MAYBE SOMEBODY GOT IT BEFORE WE GOT THERE!
            return false;
        }

        // REMOVE GRAPHIC
        RemoveItemGraphicFromWorld(Item[gWorldItems[iItemIndex].o.usItem], sGridNo, ubLevel, pItemPoolTemp.pLevelNode);

        // IF WE ARE LOCATIONG STILL, KILL LOCATOR!
        if (pItemPoolTemp.bFlashColor != 0)
        {
            // REMOVE TIMER!
            RemoveFlashItemSlot(pItemPoolTemp);
        }

        // REMOVE PREV
        if (pItemPoolTemp.pPrev is not null)
        {
            pItemPoolTemp.pPrev.pNext = pItemPoolTemp.pNext;
        }

        // REMOVE NEXT
        if (pItemPoolTemp.pNext is not null)
        {
            pItemPoolTemp.pNext.pPrev = pItemPoolTemp.pPrev;
        }

        // IF THIS NODE WAS THE HEAD, SET ANOTHER AS HEAD AT THIS GRIDNO
        if (pItemPoolTemp.pPrev is null)
        {
            // WE'RE HEAD
            LEVELNODE? pObject = ubLevel == 0
                ? gpWorldLevelData[sGridNo].pStructHead
                : gpWorldLevelData[sGridNo].pOnRoofHead;

            fItemFound = false;

            // LOOP THORUGH OBJECT LAYER
            while (pObject is not null)
            {
                if (pObject.uiFlags.HasFlag(LEVELNODEFLAGS.ITEM))
                {
                    // ADJUST TO NEXT GUY FOR HEAD
                    pObject.pItemPool = pItemPoolTemp.pNext;
                    fItemFound = true;
                }

                pObject = pObject.pNext;
            }

            if (!fItemFound)
            {
                // THIS WAS THE LAST ITEM IN THE POOL!
                gpWorldLevelData[sGridNo].uiFlags &= ~MAPELEMENTFLAGS.ITEMPOOL_PRESENT;
            }
        }

        // Find any structure with flag set as having items on top.. if this one did...
        if (pItemPoolTemp.bRenderZHeightAboveLevel > 0)
        {
            // Check if an item pool exists here....
            if (!GetItemPool(pItemPoolTemp.sGridNo, out _, pItemPoolTemp.ubLevel))
            {
                STRUCTURE? pStructure = WorldStructures.FindStructure(pItemPoolTemp.sGridNo, STRUCTUREFLAGS.HASITEMONTOP);

                if (pStructure is not null)
                {
                    // Remove...
                    pStructure.fFlags &= ~STRUCTUREFLAGS.HASITEMONTOP;

                    // Re-adjust interactive tile...
                    InteractiveTiles.BeginCurInteractiveTileCheck(INTILE_CHECK_SELECTIVE);
                }
            }
        }

        AdjustItemPoolVisibility(pItemPoolTemp);

        // (the original frees the pool node here; the garbage collector does that)

        WorldItems.RemoveItemFromWorld(iItemIndex);

        return true;
    }

    public static void RemoveItemPool(int sGridNo, int ubLevel)
    {
        // Check for and existing pool on the object layer
        while (GetItemPool(sGridNo, out ITEM_POOL? pItemPool, ubLevel))
        {
            RemoveItemFromPool(sGridNo, pItemPool!.iItemIndex, ubLevel);
        }
    }

    public static void RemoveAllUnburiedItems(int sGridNo, int ubLevel)
    {
        // Check for and existing pool on the object layer
        GetItemPool(sGridNo, out ITEM_POOL? pItemPool, ubLevel);

        while (pItemPool is not null)
        {
            if (gWorldItems[pItemPool.iItemIndex].bVisible == ItemVisibility.BURIED)
            {
                pItemPool = pItemPool.pNext;
            }
            else
            {
                RemoveItemFromPool(sGridNo, pItemPool.iItemIndex, ubLevel);

                // get new start pointer
                GetItemPool(sGridNo, out pItemPool, ubLevel);
            }
        }
    }

    public static bool MoveItemPools(int sStartPos, int sEndPos)
    {
        // note, only works between locations on the ground

        // While there is an existing pool
        while (GetItemPool(sStartPos, out ITEM_POOL? pItemPool, 0))
        {
            // copy the world item before the pool entry (and the world item) is removed
            WORLDITEM source = gWorldItems[pItemPool!.iItemIndex];
            var tempObject = source.o.Clone();
            int ubLevel = source.ubLevel;
            WORLD_ITEM usFlags = source.usFlags;
            int bRenderZ = source.bRenderZHeightAboveLevel;

            RemoveItemFromPool(sStartPos, pItemPool.iItemIndex, 0);
            AddItemToPool(sEndPos, tempObject, ItemVisibility.INVISIBLE, ubLevel, usFlags, bRenderZ);
        }

        return true;
    }

    public static void ClearAllItemPools()
    {
        for (int cnt = 0; cnt < Math.Min(WORLD_MAX, gpWorldLevelData.Count); cnt++)
        {
            RemoveItemPool(cnt, 0);
            RemoveItemPool(cnt, 1);
        }
    }

    // ---- glow -----------------------------------------------------------------------------------------

    private static void LoopLevelNodeForItemGlowFlag(LEVELNODE? pNode, int sGridNo, int ubLevel, bool fOn)
    {
        for (; pNode is not null; pNode = pNode.pNext)
        {
            if (pNode.uiFlags.HasFlag(LEVELNODEFLAGS.ITEM))
            {
                if (fOn)
                {
                    pNode.uiFlags |= LEVELNODEFLAGS.DYNAMIC;
                }
                else
                {
                    pNode.uiFlags &= ~LEVELNODEFLAGS.DYNAMIC;
                }
            }
        }
    }

    public static void HandleItemGlowFlag(int sGridNo, int ubLevel, bool fOn)
    {
        LEVELNODE? pNode = ubLevel == 0
            ? gpWorldLevelData[sGridNo].pStructHead
            : gpWorldLevelData[sGridNo].pOnRoofHead;

        LoopLevelNodeForItemGlowFlag(pNode, sGridNo, ubLevel, fOn);
    }

    public static void ToggleItemGlow(bool fOn)
    {
        for (int cnt = 0; cnt < Math.Min(WORLD_MAX, gpWorldLevelData.Count); cnt++)
        {
            HandleItemGlowFlag(cnt, 0, fOn);
            HandleItemGlowFlag(cnt, 1, fOn);
        }

        GameSettings.fOptions[TOPTION.GLOW_ITEMS] = fOn;

        RenderWorld.SetRenderFlags(RenderingFlags.FULL);
    }

    // ---- locators (flashing item indicators) ---------------------------------------------------------

    public static void SetItemPoolLocator(ITEM_POOL pItemPool)
    {
        pItemPool.bFlashColor = 59;

        pItemPool.uiTimerID = AddFlashItemSlot(pItemPool, null, 0);
    }

    public static void SetItemPoolLocatorWithCallback(ITEM_POOL pItemPool, ITEM_POOL_LOCATOR_HOOK? Callback)
    {
        pItemPool.bFlashColor = 59;

        pItemPool.uiTimerID = AddFlashItemSlot(pItemPool, Callback, 0);
    }

    public static int GetFreeFlashItemSlot()
    {
        for (int uiCount = 0; uiCount < guiNumFlashItemSlots; uiCount++)
        {
            if (!FlashItemSlots[uiCount].fAllocated)
            {
                return uiCount;
            }
        }

        if (guiNumFlashItemSlots < NUM_ITEM_FLASH_SLOTS)
        {
            return guiNumFlashItemSlots++;
        }

        return -1;
    }

    public static void RecountFlashItemSlots()
    {
        for (int uiCount = guiNumFlashItemSlots - 1; uiCount >= 0; uiCount--)
        {
            if (FlashItemSlots[uiCount].fAllocated)
            {
                guiNumFlashItemSlots = uiCount + 1;
                break;
            }
        }
    }

    public static int AddFlashItemSlot(ITEM_POOL pItemPool, ITEM_POOL_LOCATOR_HOOK? Callback, int ubFlags)
    {
        int iFlashItemIndex = GetFreeFlashItemSlot();
        if (iFlashItemIndex == -1)
        {
            return -1;
        }

        ubFlags |= ITEM_LOCATOR_LOCKED;

        ITEM_POOL_LOCATOR slot = FlashItemSlots[iFlashItemIndex];
        slot.pItemPool = pItemPool;
        slot.bRadioFrame = 0;
        slot.uiLastFrameUpdate = (int)GetJA2Clock();
        slot.Callback = Callback;
        slot.fAllocated = true;
        slot.ubFlags = ubFlags;

        return iFlashItemIndex;
    }

    public static bool RemoveFlashItemSlot(ITEM_POOL? pItemPool)
    {
        CHECKF(pItemPool != null);

        for (int uiCount = 0; uiCount < guiNumFlashItemSlots; uiCount++)
        {
            if (FlashItemSlots[uiCount].fAllocated && FlashItemSlots[uiCount].pItemPool == pItemPool)
            {
                FlashItemSlots[uiCount].fAllocated = false;

                // Check if we have a callback and call it if so!
                FlashItemSlots[uiCount].Callback?.Invoke();

                return true;
            }
        }

        return true;
    }
}
