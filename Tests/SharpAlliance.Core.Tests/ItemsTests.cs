using System;
using SharpAlliance.Core;
using SharpAlliance.Core.SubSystems;
using Xunit;

namespace SharpAlliance.Core.Tests;

/// <summary>
/// Behaviour ported from Items.c. Expected values come from reading the original C.
/// </summary>
public class ItemsTests
{
    private static SOLDIERTYPE NewSoldier()
    {
        var s = new SOLDIERTYPE();
        foreach (InventorySlot slot in Enum.GetValues<InventorySlot>())
        {
            if (slot != InventorySlot.NUM_INV_SLOTS && slot >= 0)
            {
                s.inv[slot] = new OBJECTTYPE();
            }
        }

        return s;
    }

    // ---- OBJECTTYPE union ---------------------------------------------------------------------

    [Fact]
    public void GunStatusAliasesStatusArray()
    {
        var o = new OBJECTTYPE();
        o.bGunStatus = 77;
        Assert.Equal(77, o.bStatus[0]);
        Assert.Equal(77, o.ubShotsLeft[0]);
        Assert.Equal(77, o.bMoneyStatus);
        Assert.Equal(77, o.bBombStatus);
    }

    [Fact]
    public void GunShotsLeftIsThirdByteOfTheUnion()
    {
        var o = new OBJECTTYPE();
        o.ubGunShotsLeft = 12;
        Assert.Equal(12, o.bStatus[2]);
        Assert.Equal(12, o.ubShotsLeft[2]);
    }

    [Fact]
    public void SixteenBitAndThirtyTwoBitMembersRoundTripAndShareBytes()
    {
        var o = new OBJECTTYPE();
        o.usGunAmmoItem = Items.GLOCK_17;
        Assert.Equal(Items.GLOCK_17, o.usGunAmmoItem);

        o.uiMoneyAmount = 123456;
        Assert.Equal(123456, o.uiMoneyAmount);

        o.uiMoneyAmount = 0;
        o.bStatus[1] = 0x34;
        o.bStatus[2] = 0x12;
        Assert.Equal(0x1234, o.uiMoneyAmount & 0xFFFF);   // money lives in bytes 1..4
    }

    [Fact]
    public void BombAndKeyViewsOverlayTheSameBytes()
    {
        var o = new OBJECTTYPE();
        o.bDelay = 7;
        Assert.Equal(7, o.bFrequency);          // bDelay / bFrequency share a byte
        o.ubTolerance = 9;
        Assert.Equal(9, o.ubLocationID);
        o.ubKeyID = 5;
        Assert.Equal(5, o.bStatus[6]);
        o.bKeyStatus[0] = 33;
        Assert.Equal(33, o.bStatus[0]);
    }

    [Fact]
    public void ObjectInitializerStillAcceptsArrays()
    {
        var o = new OBJECTTYPE { bStatus = new[] { 90, 80, 70 } };
        Assert.Equal(90, o.bStatus[0]);
        Assert.Equal(70, o.bStatus[2]);
    }

    [Fact]
    public void CopyFromCopiesEverythingAndClearEmptiesIt()
    {
        var a = new OBJECTTYPE { usItem = Items.GLOCK_17, ubNumberOfObjects = 2, ubWeight = 5 };
        a.bStatus[0] = 60;
        a.usAttachItem[1] = Items.SILENCER;
        a.bAttachStatus[1] = 40;

        var b = new OBJECTTYPE();
        b.CopyFrom(a);
        Assert.Equal(Items.GLOCK_17, b.usItem);
        Assert.Equal(2, b.ubNumberOfObjects);
        Assert.Equal(60, b.bStatus[0]);
        Assert.Equal(Items.SILENCER, b.usAttachItem[1]);
        Assert.Equal(40, b.bAttachStatus[1]);

        b.bStatus[0] = 1;                       // independent storage
        Assert.Equal(60, a.bStatus[0]);

        a.Clear();
        Assert.Equal(Items.NONE, a.usItem);
        Assert.Equal(0, a.bStatus[0]);
        Assert.Equal(Items.NONE, a.usAttachItem[1]);
    }

    // ---- creating and finding items -------------------------------------------------------------

    [Fact]
    public void CreateItemGunIsUsableBecauseStatusIsInTheUnion()
    {
        var o = new OBJECTTYPE();
        Assert.True(ItemSubSystem.CreateItem(Items.GLOCK_17, 80, o));
        Assert.Equal(80, o.bStatus[0]);          // was always 0 before the union was modelled

        var s = NewSoldier();
        s.inv[InventorySlot.HANDPOS] = o;
        Assert.Equal(InventorySlot.HANDPOS, ItemSubSystem.FindUsableObj(s, Items.GLOCK_17));
        Assert.Equal(InventorySlot.HANDPOS, ItemSubSystem.FindObj(s, Items.GLOCK_17));
        Assert.Equal(InventorySlot.HANDPOS, ItemSubSystem.FindAIUsableObjClass(s, IC.GUN));
    }

    [Fact]
    public void FindReturnsNoSlotWhenAbsent()
    {
        var s = NewSoldier();
        Assert.Equal(NO_SLOT_VALUE, ItemSubSystem.FindObj(s, Items.GLOCK_17));
        Assert.Equal(NO_SLOT_VALUE, ItemSubSystem.FindUsableObj(s, Items.GLOCK_17));
    }

    private static readonly InventorySlot NO_SLOT_VALUE = (InventorySlot)(-1);

    [Fact]
    public void FindEmptySlotSkipsSecondHandWhenHandHoldsTwoHandedItem()
    {
        var s = NewSoldier();
        ItemSubSystem.CreateItem(Items.M870, 100, s.inv[InventorySlot.HANDPOS]);     // two-handed shotgun
        Assert.Equal(InventorySlot.BIGPOCK1POS,
            ItemSubSystem.FindEmptySlotWithin(s, InventorySlot.SECONDHANDPOS, InventorySlot.BIGPOCK1POS));
    }

    // ---- tables ---------------------------------------------------------------------------------

    [Fact]
    public void AttachmentTable()
    {
        Assert.True(ItemSubSystem.ValidAttachment(Items.SILENCER, Items.GLOCK_17));
        Assert.True(ItemSubSystem.ValidAttachment(Items.SNIPERSCOPE, Items.M24));
        Assert.False(ItemSubSystem.ValidAttachment(Items.SILENCER, Items.M24));
        Assert.False(ItemSubSystem.ValidAttachment(Items.GLOCK_17, Items.SILENCER));
    }

    [Fact]
    public void LaunchableTable()
    {
        Assert.True(ItemSubSystem.ValidLaunchable(Items.GL_HE_GRENADE, Items.GLAUNCHER));
        Assert.Equal(Items.GLAUNCHER, ItemSubSystem.GetLauncherFromLaunchable(Items.GL_HE_GRENADE));
        Assert.Equal(Items.MORTAR, ItemSubSystem.GetLauncherFromLaunchable(Items.MORTAR_SHELL));
        Assert.Equal(Items.NONE, ItemSubSystem.GetLauncherFromLaunchable(Items.GLOCK_17));
    }

    [Fact]
    public void MergeTable()
    {
        Assert.True(ItemSubSystem.ValidMerge(Items.FIRSTAIDKIT, Items.FIRSTAIDKIT));
        Assert.True(ItemSubSystem.EvaluateValidMerge(Items.FIRSTAIDKIT, Items.FIRSTAIDKIT, out Items result, out MergeType type));
        Assert.Equal(Items.FIRSTAIDKIT, result);
        Assert.Equal(MergeType.COMBINE_POINTS, type);
        Assert.False(ItemSubSystem.ValidMerge(Items.GLOCK_17, Items.GLOCK_17));
    }

    [Fact]
    public void FaceItemCompatibility()
    {
        Assert.True(ItemSubSystem.CompatibleFaceItem(Items.EXTENDEDEAR, Items.NIGHTGOGGLES));
        Assert.False(ItemSubSystem.CompatibleFaceItem(Items.NIGHTGOGGLES, Items.GASMASK));
    }

    [Fact]
    public void ComboMergeNeedsAllAttachments()
    {
        var rod = new OBJECTTYPE();
        ItemSubSystem.CreateItem(Items.ALUMINUM_ROD, 100, rod);
        Assert.Equal(-1, ItemSubSystem.GetAttachmentComboMerge(rod));

        rod.usAttachItem[0] = Items.SPRING;
        rod.bAttachStatus[0] = 50;
        int combo = ItemSubSystem.GetAttachmentComboMerge(rod);
        Assert.NotEqual(-1, combo);

        ItemSubSystem.PerformAttachmentComboMerge(rod, combo);
        Assert.Equal(Items.SPRING_AND_BOLT_UPGRADE, rod.usItem);
        Assert.Equal(Items.NONE, rod.usAttachItem[0]);
        Assert.Equal(75, rod.bStatus[0]);        // average of 100 and 50
    }

    // ---- attachments and stacks -------------------------------------------------------------------

    [Fact]
    public void AttachAndFindAttachmentIndexes()
    {
        var gun = new OBJECTTYPE();
        ItemSubSystem.CreateItem(Items.GLOCK_17, 100, gun);
        var silencer = new OBJECTTYPE();
        ItemSubSystem.CreateItem(Items.SILENCER, 90, silencer);

        Assert.Equal(-1, ItemSubSystem.FindAttachment(gun, Items.SILENCER));
        Assert.True(ItemSubSystem.AttachObject(null, gun, silencer));
        Assert.Equal(0, ItemSubSystem.FindAttachment(gun, Items.SILENCER));
        Assert.Equal(90, gun.bAttachStatus[0]);
        Assert.Equal(0, ItemSubSystem.FindAttachmentByClass(gun, IC.MISC));
        Assert.Equal(Items.NONE, silencer.usItem);       // the attached object was consumed
    }

    [Fact]
    public void RemoveObjsTakesFromTheFrontOfTheStack()
    {
        var stack = new OBJECTTYPE();
        Assert.True(ItemSubSystem.CreateItems(Items.FIRSTAIDKIT, 100, 3, stack));
        Assert.Equal(3, stack.ubNumberOfObjects);
        stack.bStatus[0] = 10;
        stack.bStatus[1] = 20;
        stack.bStatus[2] = 30;

        ItemSubSystem.RemoveObjs(stack, 1);

        Assert.Equal(2, stack.ubNumberOfObjects);
        Assert.Equal(20, stack.bStatus[0]);
        Assert.Equal(30, stack.bStatus[1]);

        ItemSubSystem.RemoveObjs(stack, 2);
        Assert.Equal(Items.NONE, stack.usItem);          // everything removed empties the object
    }

    [Fact]
    public void SwapObjsExchangesContents()
    {
        var a = new OBJECTTYPE();
        var b = new OBJECTTYPE();
        ItemSubSystem.CreateItem(Items.GLOCK_17, 80, a);
        ItemSubSystem.CreateItem(Items.SILENCER, 60, b);

        ItemSubSystem.SwapObjs(a, b);

        Assert.Equal(Items.SILENCER, a.usItem);
        Assert.Equal(60, a.bStatus[0]);
        Assert.Equal(Items.GLOCK_17, b.usItem);
        Assert.Equal(80, b.bStatus[0]);
    }

    [Fact]
    public void TotalPointsAddsStatusAcrossTheStack()
    {
        var stack = new OBJECTTYPE();
        ItemSubSystem.CreateItems(Items.FIRSTAIDKIT, 100, 3, stack);
        stack.bStatus[0] = 10;
        stack.bStatus[1] = 20;
        stack.bStatus[2] = 30;
        Assert.Equal(60, ItemSubSystem.TotalPoints(stack));
    }

    // ---- placing and stacking -----------------------------------------------------------------------

    [Fact]
    public void AutoPlaceObjectPutsAGunInTheHandThenInPockets()
    {
        var s = NewSoldier();

        var gun1 = new OBJECTTYPE();
        ItemSubSystem.CreateItem(Items.GLOCK_17, 90, gun1);
        Assert.True(ItemSubSystem.AutoPlaceObject(s, gun1, false));
        Assert.Equal(Items.GLOCK_17, s.inv[InventorySlot.HANDPOS].usItem);
        Assert.Equal(90, s.inv[InventorySlot.HANDPOS].bStatus[0]);

        var gun2 = new OBJECTTYPE();
        ItemSubSystem.CreateItem(Items.GLOCK_18, 70, gun2);
        Assert.True(ItemSubSystem.AutoPlaceObject(s, gun2, false));
        Assert.NotEqual(NO_SLOT_VALUE, ItemSubSystem.FindObj(s, Items.GLOCK_18));
        Assert.NotEqual(InventorySlot.HANDPOS, ItemSubSystem.FindObj(s, Items.GLOCK_18));
    }

    [Fact]
    public void StackObjsMovesItemsBetweenStacks()
    {
        var target = new OBJECTTYPE();
        var source = new OBJECTTYPE();
        ItemSubSystem.CreateItem(Items.FIRSTAIDKIT, 100, target);
        ItemSubSystem.CreateItems(Items.FIRSTAIDKIT, 60, 2, source);

        ItemSubSystem.StackObjs(source, target, 1);

        Assert.Equal(2, target.ubNumberOfObjects);
        Assert.Equal(1, source.ubNumberOfObjects);
        Assert.Equal(60, target.bStatus[1]);
    }

    [Fact]
    public void SlotLimits()
    {
        Assert.Equal(1, ItemSubSystem.ItemSlotLimit(Items.GLOCK_17, InventorySlot.HANDPOS));
        Assert.Equal(MAX_MONEY_PER_SLOT_VALUE, ItemSubSystem.MoneySlotLimit(InventorySlot.BIGPOCK1POS));
        Assert.Equal(MAX_MONEY_PER_SLOT_VALUE / 2, ItemSubSystem.MoneySlotLimit(InventorySlot.SMALLPOCK1POS));
    }

    private static readonly int MAX_MONEY_PER_SLOT_VALUE = Globals.MAX_MONEY_PER_SLOT;

    [Fact]
    public void CreateMagazineFillsTheCallersObject()
    {
        var o = new OBJECTTYPE();
        Assert.True(ItemSubSystem.CreateItem(Items.CLIP9_15, 100, o));
        Assert.Equal(Items.CLIP9_15, o.usItem);
        Assert.Equal(1, o.ubNumberOfObjects);
        Assert.Equal(15, o.ubShotsLeft[0]);
    }

    [Fact]
    public void CreateGunLoadsTheDefaultMagazine()
    {
        var o = new OBJECTTYPE();
        Assert.True(ItemSubSystem.CreateItem(Items.GLOCK_17, 100, o));
        Assert.Equal(15, o.ubGunShotsLeft);   // Glock 17 uses the 9mm 15-round clip in JA2
        Assert.Equal(100, o.bGunAmmoStatus);
        Assert.NotEqual(Items.NONE, o.usGunAmmoItem);
    }
}
