using System;
using SharpAlliance.Core;
using SharpAlliance.Core.SubSystems;
using Xunit;
using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core.Tests;

/// <summary>
/// Item pools from Handle Items.c: per-tile linked lists of world items, hung off level nodes.
/// The tile database is not loaded here, so the graphic nodes are linked directly (headless mode).
/// </summary>
[Collection("World")]
public class ItemPoolTests : IDisposable
{
    public ItemPoolTests() => ResetWorld();

    public void Dispose() => ResetWorld();

    private static void ResetWorld(int cells = 200)
    {
        gWorldItems.Clear();
        gWorldBombs.Clear();
        guiNumWorldBombs = 0;
        gpWorldLevelData.Clear();
        for (int i = 0; i < cells; i++)
        {
            gpWorldLevelData.Add(new MAP_ELEMENT());
        }

        HandleItems.guiNumFlashItemSlots = 0;
        foreach (var slot in HandleItems.FlashItemSlots)
        {
            slot.fAllocated = false;
            slot.Callback = null;
            slot.pItemPool = null;
        }
    }

    private static OBJECTTYPE Make(Items item, int status = 100)
    {
        var o = new OBJECTTYPE();
        ItemSubSystem.CreateItem(item, status, o);
        return o;
    }

    private static int Add(int grid, Items item, ItemVisibility vis = ItemVisibility.INVISIBLE, int level = 0, int z = 0)
    {
        HandleItems.AddItemToPoolAndGetIndex(grid, Make(item), vis, level, 0, z, out int index);
        return index;
    }

    // ---- adding ---------------------------------------------------------------------------------

    [Fact]
    public void AddingAnItemCreatesAWorldItemAndAPool()
    {
        var obj = Make(Items.GLOCK_17, 80);
        OBJECTTYPE? placed = HandleItems.AddItemToPoolAndGetIndex(10, obj, ItemVisibility.INVISIBLE, 0, 0, 0, out int index);

        Assert.True(index >= 0);
        Assert.True(gWorldItems[index].fExists);
        Assert.Equal(10, gWorldItems[index].sGridNo);
        Assert.Same(gWorldItems[index].o, placed);                  // the caller gets the world's copy
        Assert.Equal(Items.GLOCK_17, placed!.usItem);
        Assert.Equal(80, placed.bStatus[0]);

        Assert.True(HandleItems.GetItemPool(10, out ITEM_POOL? pool, 0));
        Assert.Equal(index, pool!.iItemIndex);
        Assert.Equal(10, pool.sGridNo);
        Assert.True(gpWorldLevelData[10].uiFlags.HasFlag(MAPELEMENTFLAGS.ITEMPOOL_PRESENT));
        Assert.False(HandleItems.GetItemPool(11, out _, 0));
    }

    [Fact]
    public void TheWorldKeepsItsOwnCopyOfTheObject()
    {
        var obj = Make(Items.GLOCK_17, 80);
        HandleItems.AddItemToPoolAndGetIndex(10, obj, ItemVisibility.INVISIBLE, 0, 0, 0, out int index);

        obj.bStatus[0] = 1;
        obj.Clear();

        Assert.Equal(Items.GLOCK_17, gWorldItems[index].o.usItem);
        Assert.Equal(80, gWorldItems[index].o.bStatus[0]);
    }

    [Fact]
    public void SecondItemAtTheSameTileJoinsTheListAtTheTail()
    {
        int a = Add(10, Items.GLOCK_17);
        int b = Add(10, Items.SILENCER);
        int c = Add(10, Items.FIRSTAIDKIT);

        Assert.True(HandleItems.GetItemPool(10, out ITEM_POOL? head, 0));
        Assert.Equal(a, head!.iItemIndex);
        Assert.Null(head.pPrev);
        Assert.Equal(b, head.pNext!.iItemIndex);
        Assert.Same(head, head.pNext.pPrev);
        Assert.Equal(c, head.pNext.pNext!.iItemIndex);
        Assert.Null(head.pNext.pNext.pNext);

        Assert.True(HandleItems.ItemExistsAtLocation(10, b, 0));
        Assert.False(HandleItems.ItemExistsAtLocation(10, 999, 0));
        Assert.Same(head.pNext, HandleItems.GetItemPoolForIndex(10, b, 0));
        Assert.Null(HandleItems.GetItemPoolForIndex(10, 999, 0));
    }

    [Fact]
    public void FindsItemsByTypeAndClass()
    {
        Add(10, Items.FIRSTAIDKIT);
        int gun = Add(10, Items.GLOCK_17);

        Assert.Equal(gun, HandleItems.GetItemOfClassTypeInPool(10, IC.GUN, 0));
        Assert.Equal(-1, HandleItems.GetItemOfClassTypeInPool(10, IC.BLADE, 0));
        Assert.True(HandleItems.ItemTypeExistsAtLocation(10, Items.GLOCK_17, 0, out int found));
        Assert.Equal(gun, found);
        Assert.False(HandleItems.ItemTypeExistsAtLocation(10, Items.M24, 0, out int none));
        Assert.Equal(-1, none);
    }

    [Fact]
    public void ItemsOnDifferentTilesHaveSeparatePools()
    {
        int a = Add(10, Items.GLOCK_17);
        int b = Add(11, Items.SILENCER);

        HandleItems.GetItemPool(10, out ITEM_POOL? p10, 0);
        HandleItems.GetItemPool(11, out ITEM_POOL? p11, 0);
        Assert.Equal(a, p10!.iItemIndex);
        Assert.Equal(b, p11!.iItemIndex);
        Assert.Null(p10.pNext);
    }

    [Fact]
    public void InvalidGridNoIsRejectedByTheWorldTable()
    {
        Assert.Equal(-1, WorldItems.AddItemToWorld(NOWHERE, Make(Items.GLOCK_17), 0, 0, 0, ItemVisibility.INVISIBLE));
    }

    [Fact]
    public void WorldTableGrowsByTenAndReusesFreedSlots()
    {
        Assert.Equal(0, WorldItems.GetFreeWorldItemIndex());
        Assert.Equal(10, gWorldItems.Count);
        Assert.Equal(10, guiNumWorldItems);

        int a = Add(10, Items.GLOCK_17);
        int b = Add(10, Items.SILENCER);
        Assert.Equal(0, a);
        Assert.Equal(1, b);

        HandleItems.RemoveItemFromPool(10, a, 0);
        int c = Add(12, Items.FIRSTAIDKIT);
        Assert.Equal(a, c);                                     // the freed slot is reused
    }

    [Fact]
    public void ArmedBombsAreRegisteredAndUnregistered()
    {
        int index = WorldItems.AddItemToWorld(10, Make(Items.TNT), 0, WORLD_ITEM.ARMED_BOMB, 0, ItemVisibility.INVISIBLE);

        Assert.True(index >= 0);
        Assert.Contains(gWorldBombs, b => b.fExists && b.iItemIndex == index);

        WorldItems.RemoveItemFromWorld(index);
        Assert.DoesNotContain(gWorldBombs, b => b.fExists && b.iItemIndex == index);
    }

    // ---- removing -------------------------------------------------------------------------------

    [Fact]
    public void RemovingTheHeadPromotesTheNextItem()
    {
        int a = Add(10, Items.GLOCK_17);
        int b = Add(10, Items.SILENCER);

        Assert.True(HandleItems.RemoveItemFromPool(10, a, 0));

        Assert.False(gWorldItems[a].fExists);
        Assert.True(HandleItems.GetItemPool(10, out ITEM_POOL? head, 0));
        Assert.Equal(b, head!.iItemIndex);
        Assert.Null(head.pPrev);
        Assert.False(HandleItems.ItemExistsAtLocation(10, a, 0));
        Assert.True(gpWorldLevelData[10].uiFlags.HasFlag(MAPELEMENTFLAGS.ITEMPOOL_PRESENT));
    }

    [Fact]
    public void RemovingAMiddleItemRelinksTheNeighbours()
    {
        int a = Add(10, Items.GLOCK_17);
        int b = Add(10, Items.SILENCER);
        int c = Add(10, Items.FIRSTAIDKIT);

        Assert.True(HandleItems.RemoveItemFromPool(10, b, 0));

        HandleItems.GetItemPool(10, out ITEM_POOL? head, 0);
        Assert.Equal(a, head!.iItemIndex);
        Assert.Equal(c, head.pNext!.iItemIndex);
        Assert.Same(head, head.pNext.pPrev);
    }

    [Fact]
    public void RemovingTheLastItemClearsThePoolFlag()
    {
        int a = Add(10, Items.GLOCK_17);

        Assert.True(HandleItems.RemoveItemFromPool(10, a, 0));

        Assert.False(HandleItems.GetItemPool(10, out _, 0));
        Assert.False(gpWorldLevelData[10].uiFlags.HasFlag(MAPELEMENTFLAGS.ITEMPOOL_PRESENT));
        Assert.Null(gpWorldLevelData[10].pStructHead);          // the graphic node is gone too
    }

    [Fact]
    public void RemovingAnItemThatIsNotThereFails()
    {
        Add(10, Items.GLOCK_17);
        Assert.False(HandleItems.RemoveItemFromPool(10, 999, 0));
        Assert.False(HandleItems.RemoveItemFromPool(11, 0, 0));
    }

    [Fact]
    public void RemoveItemPoolEmptiesTheTile()
    {
        Add(10, Items.GLOCK_17);
        Add(10, Items.SILENCER);
        Add(10, Items.FIRSTAIDKIT);

        HandleItems.RemoveItemPool(10, 0);

        Assert.False(HandleItems.GetItemPool(10, out _, 0));
        Assert.All(gWorldItems, w => Assert.False(w.fExists));
    }

    [Fact]
    public void RemoveAllUnburiedItemsLeavesBuriedOnes()
    {
        int visible = Add(10, Items.GLOCK_17);
        int buried = Add(10, Items.SILENCER, ItemVisibility.BURIED);
        int visible2 = Add(10, Items.FIRSTAIDKIT);

        HandleItems.RemoveAllUnburiedItems(10, 0);

        Assert.False(gWorldItems[visible].fExists);
        Assert.False(gWorldItems[visible2].fExists);
        Assert.True(gWorldItems[buried].fExists);
        Assert.True(HandleItems.GetItemPool(10, out ITEM_POOL? head, 0));
        Assert.Equal(buried, head!.iItemIndex);
    }

    [Fact]
    public void ClearAllItemPoolsEmptiesTheWorld()
    {
        Add(10, Items.GLOCK_17);
        Add(50, Items.SILENCER);

        HandleItems.ClearAllItemPools();

        Assert.False(HandleItems.GetItemPool(10, out _, 0));
        Assert.False(HandleItems.GetItemPool(50, out _, 0));
    }

    [Fact]
    public void MoveItemPoolsMovesEverythingToTheNewTile()
    {
        Add(10, Items.GLOCK_17);
        Add(10, Items.SILENCER);

        Assert.True(HandleItems.MoveItemPools(10, 20));

        Assert.False(HandleItems.GetItemPool(10, out _, 0));
        Assert.True(HandleItems.GetItemPool(20, out ITEM_POOL? head, 0));
        Assert.Equal(Items.GLOCK_17, gWorldItems[head!.iItemIndex].o.usItem);
        Assert.Equal(Items.SILENCER, gWorldItems[head.pNext!.iItemIndex].o.usItem);
        Assert.Equal(20, gWorldItems[head.iItemIndex].sGridNo);
    }

    // ---- visibility -----------------------------------------------------------------------------

    [Fact]
    public void PoolVisibilityFollowsItsItems()
    {
        Add(10, Items.GLOCK_17, ItemVisibility.INVISIBLE);
        HandleItems.GetItemPool(10, out ITEM_POOL? pool, 0);
        Assert.Equal(ItemVisibility.INVISIBLE, pool!.bVisible);

        Add(10, Items.SILENCER, ItemVisibility.VISIBLE);
        HandleItems.GetItemPool(10, out pool, 0);
        Assert.Equal(ItemVisibility.VISIBLE, pool!.bVisible);   // any visible item makes the pool visible
    }

    [Fact]
    public void LookingForHiddenItemsRevealsThemAndSetsALocator()
    {
        int hidden = Add(10, Items.GLOCK_17, ItemVisibility.HIDDEN_ITEM);
        HandleItems.GetItemPool(10, out ITEM_POOL? pool, 0);
        Assert.True(HandleItems.DoesItemPoolContainAnyHiddenItems(pool));
        Assert.True(HandleItems.DoesItemPoolContainAllHiddenItems(pool));

        Assert.True(HandleItems.LookForHiddenItems(10, 0, fSetLocator: true, 0));

        Assert.Equal(ItemVisibility.VISIBLE, gWorldItems[hidden].bVisible);
        Assert.Equal(ItemVisibility.VISIBLE, pool!.bVisible);
        Assert.Equal(59, pool.bFlashColor);
        Assert.True(HandleItems.FlashItemSlots[pool.uiTimerID].fAllocated);
        Assert.False(HandleItems.LookForHiddenItems(10, 0, true, 0));   // nothing left to find
    }

    [Fact]
    public void ActionAndOwnershipItemsAreNeverMadeVisible()
    {
        int action = Add(10, Items.ACTION_ITEM, ItemVisibility.INVISIBLE);
        int owner = Add(10, Items.OWNERSHIP, ItemVisibility.INVISIBLE);
        int normal = Add(10, Items.GLOCK_17, ItemVisibility.INVISIBLE);
        HandleItems.GetItemPool(10, out ITEM_POOL? pool, 0);

        Assert.True(HandleItems.SetItemPoolVisibilityOn(pool!, ItemVisibility.INVISIBLE, fSetLocator: false));

        Assert.Equal(ItemVisibility.INVISIBLE, gWorldItems[action].bVisible);
        Assert.Equal(ItemVisibility.INVISIBLE, gWorldItems[owner].bVisible);
        Assert.Equal(ItemVisibility.VISIBLE, gWorldItems[normal].bVisible);
    }

    [Fact]
    public void BuriedItemsStayBuriedUnlessTheThresholdAllowsThem()
    {
        int buried = Add(10, Items.GLOCK_17, ItemVisibility.BURIED);
        HandleItems.GetItemPool(10, out ITEM_POOL? pool, 0);

        Assert.False(HandleItems.SetItemPoolVisibilityOn(pool!, ItemVisibility.INVISIBLE, false));   // BURIED < INVISIBLE
        Assert.Equal(ItemVisibility.BURIED, gWorldItems[buried].bVisible);

        Assert.True(HandleItems.SetItemPoolVisibilityOn(pool!, ItemVisibility.BURIED, false));
        Assert.Equal(ItemVisibility.VISIBLE, gWorldItems[buried].bVisible);
    }

    [Fact]
    public void HiddenInObjectCanBeSetOnAWholePool()
    {
        int a = Add(10, Items.GLOCK_17, ItemVisibility.VISIBLE);
        HandleItems.GetItemPool(10, out ITEM_POOL? pool, 0);

        HandleItems.SetItemPoolVisibilityHidden(pool);

        Assert.Equal(ItemVisibility.HIDDEN_IN_OBJECT, gWorldItems[a].bVisible);
        Assert.Equal(ItemVisibility.HIDDEN_IN_OBJECT, pool!.bVisible);
    }

    [Fact]
    public void DisplayAndPickupFiltersUseVisibilityAndHeight()
    {
        Add(10, Items.GLOCK_17, ItemVisibility.VISIBLE, z: 0);
        Add(10, Items.SILENCER, ItemVisibility.VISIBLE, z: 16);
        Add(10, Items.FIRSTAIDKIT, ItemVisibility.INVISIBLE, z: 0);
        HandleItems.GetItemPool(10, out ITEM_POOL? pool, 0);

        Assert.Equal(2, HandleItems.GetNumOkForDisplayItemsInPool(pool, -1));   // -1 means any height
        Assert.Equal(1, HandleItems.GetNumOkForDisplayItemsInPool(pool, 0));
        Assert.Equal(1, HandleItems.GetNumOkForDisplayItemsInPool(pool, 16));
        Assert.True(HandleItems.AnyItemsVisibleOnLevel(pool, 16));
        Assert.False(HandleItems.AnyItemsVisibleOnLevel(pool, 8));

        var merc = new SOLDIERTYPE { bTeam = gbPlayerNum };
        Assert.True(HandleItems.ItemPoolOKForPickup(merc, pool!, 0));
        Assert.False(HandleItems.ItemPoolOKForPickup(merc, pool!.pNext!.pNext!, 0));   // invisible to the player
        var enemy = new SOLDIERTYPE { bTeam = TEAM.ENEMY_TEAM };
        Assert.True(HandleItems.ItemPoolOKForPickup(enemy, pool.pNext.pNext, 0));        // AI ignores visibility
    }

    [Fact]
    public void ZLevelHelpers()
    {
        Add(10, Items.GLOCK_17, z: 0);
        Add(10, Items.SILENCER, z: 24);
        HandleItems.GetItemPool(10, out ITEM_POOL? pool, 0);

        Assert.Equal(24, HandleItems.GetLargestZLevelOfItemPool(pool));
        Assert.False(HandleItems.DoesItemPoolContainAllItemsOfHigherZLevel(pool));
        Assert.False(HandleItems.DoesItemPoolContainAllItemsOfZeroZLevel(pool));
        Assert.True(HandleItems.DoesItemPoolContainAllItemsOfZeroZLevel(pool!.pNext!.pNext));   // empty tail
        Assert.True(HandleItems.DoesItemPoolContainAllItemsOfHigherZLevel(pool.pNext));
        Assert.Equal(0, HandleItems.GetZLevelOfItemPoolGivenStructure(10, 0, null));
    }

    // ---- locators -------------------------------------------------------------------------------

    [Fact]
    public void FlashSlotsAreAllocatedReusedAndCallBack()
    {
        var pool1 = new ITEM_POOL();
        var pool2 = new ITEM_POOL();
        int called = 0;

        int s1 = HandleItems.AddFlashItemSlot(pool1, () => called++, 0);
        int s2 = HandleItems.AddFlashItemSlot(pool2, null, 0);

        Assert.Equal(0, s1);
        Assert.Equal(1, s2);
        Assert.Equal(HandleItems.ITEM_LOCATOR_LOCKED, HandleItems.FlashItemSlots[s1].ubFlags & HandleItems.ITEM_LOCATOR_LOCKED);

        Assert.True(HandleItems.RemoveFlashItemSlot(pool1));
        Assert.Equal(1, called);
        Assert.False(HandleItems.FlashItemSlots[s1].fAllocated);

        Assert.Equal(0, HandleItems.AddFlashItemSlot(new ITEM_POOL(), null, 0));   // freed slot reused

        // freeing the highest slot lets the count shrink back to the highest allocated one
        HandleItems.RemoveFlashItemSlot(pool2);
        Assert.Equal(2, HandleItems.guiNumFlashItemSlots);
        HandleItems.RecountFlashItemSlots();
        Assert.Equal(1, HandleItems.guiNumFlashItemSlots);
    }

    [Fact]
    public void RemovingAnItemKillsItsLocator()
    {
        int a = Add(10, Items.GLOCK_17, ItemVisibility.VISIBLE);
        HandleItems.GetItemPool(10, out ITEM_POOL? pool, 0);
        HandleItems.SetItemPoolLocator(pool!);
        int slot = pool!.uiTimerID;
        Assert.True(HandleItems.FlashItemSlots[slot].fAllocated);

        HandleItems.RemoveItemFromPool(10, a, 0);

        Assert.False(HandleItems.FlashItemSlots[slot].fAllocated);
    }

    // ---- glow -----------------------------------------------------------------------------------

    [Fact]
    public void ToggleItemGlowSetsAndClearsTheDynamicFlagOnItemNodes()
    {
        Add(10, Items.GLOCK_17);
        HandleItems.GetItemPool(10, out ITEM_POOL? pool, 0);

        HandleItems.ToggleItemGlow(true);
        Assert.True(pool!.pLevelNode!.uiFlags.HasFlag(LEVELNODEFLAGS.DYNAMIC));
        Assert.True(GameSettings.fOptions[TOPTION.GLOW_ITEMS]);

        HandleItems.ToggleItemGlow(false);
        Assert.False(pool.pLevelNode.uiFlags.HasFlag(LEVELNODEFLAGS.DYNAMIC));
        Assert.False(GameSettings.fOptions[TOPTION.GLOW_ITEMS]);
    }
}
