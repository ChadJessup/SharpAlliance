using System;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Diagnostics;
using System.Numerics;
using static SharpAlliance.Core.Globals;
using SharpAlliance.Core.Managers;

namespace SharpAlliance.Core.SubSystems;

public class HandleItems
{
    // INVENTORY POOL STUFF
    public static OBJECTTYPE? AddItemToPool(int sGridNo, OBJECTTYPE? pObject, ItemVisibility bVisible, int ubLevel, WORLD_ITEM usFlags, int bRenderZHeightAboveLevel)
    {
        return InternalAddItemToPool(ref sGridNo, pObject, bVisible, ubLevel, usFlags, bRenderZHeightAboveLevel, null);
    }

    private static OBJECTTYPE? InternalAddItemToPool(ref int psGridNo, OBJECTTYPE? pObject, ItemVisibility bVisible, int ubLevel, WORLD_ITEM usFlags, int bRenderZHeightAboveLevel, int? piItemIndex)
    {
        ITEM_POOL? pItemPool;
        ITEM_POOL? pItemPoolTemp;
        int iWorldItem;
        STRUCTURE? pStructure, pBase;
        STRUCTURE_ON sDesiredLevel;
        int sNewGridNo = psGridNo;
        LEVELNODE? pNode;
        bool fForceOnGround = false;
        bool fObjectInOpenable = false;
        TerrainTypeDefines bTerrainID;

        Debug.Assert(pObject.ubNumberOfObjects <= MAX_OBJECTS_PER_SLOT);

        // ATE: Check if the gridno is OK
        if ((psGridNo) == NOWHERE)
        {
            // Display warning.....
            //Messages.ScreenMsg(FontColor.FONT_MCOLOR_LTYELLOW, MSG_BETAVERSION, "Error: Item %d was given invalid grid location %d for item pool. Please Report.", pObject.usItem.ToString(), (psGridNo));

            (psGridNo) = sNewGridNo = gMapInformation.sCenterGridNo;

            //return( null );
        }

        // CHECK IF THIS ITEM IS IN DEEP WATER....
        // IF SO, CHECK IF IT SINKS...
        // IF SO, DONT'T ADD!
        bTerrainID = WorldManager.GetTerrainType(psGridNo);

        if (bTerrainID == TerrainTypeDefines.DEEP_WATER
            || bTerrainID == TerrainTypeDefines.LOW_WATER
            || bTerrainID == TerrainTypeDefines.MED_WATER)
        {
            if (Item[pObject.usItem].fFlags.HasFlag(ItemAttributes.ITEM_SINKS))
            {
                return (null);
            }
        }

        // First things first - look at where we are to place the items, and
        // set some flags appropriately

        // On a structure?
        //Locations on roofs without a roof is not possible, so
        //we convert the onroof intention to ground.
        if (ubLevel && !FlatRoofAboveGridNo(psGridNo))
        {
            ubLevel = 0;
        }

        if (bRenderZHeightAboveLevel == -1)
        {
            fForceOnGround = true;
            bRenderZHeightAboveLevel = 0;
        }

        // Check structure database
        if (gpWorldLevelData[psGridNo].pStructureHead is not null
            && (pObject.usItem != Items.OWNERSHIP)
            && (pObject.usItem != Items.ACTION_ITEM))
        {
            // Something is here, check obstruction in future
            sDesiredLevel = ubLevel > 0 ? STRUCTURE_ON.ROOF : STRUCTURE_ON.GROUND;
            pStructure = WorldStructures.FindStructure(psGridNo, STRUCTUREFLAGS.BLOCKSMOVES);
            while (pStructure is not null)
            {
                if (!(pStructure.fFlags.HasFlag(STRUCTUREFLAGS.PERSON | STRUCTUREFLAGS.CORPSE))
                    && pStructure.sCubeOffset == sDesiredLevel)
                {
                    // If we are going into a raised struct AND we have above level set to -1
                    if (StructureInternals.StructureBottomLevel(pStructure) != 1 && fForceOnGround)
                    {
                        break;
                    }

                    // Adjust the item's gridno to the base of struct.....
                    pBase = WorldStructures.FindBaseStructure(pStructure);

                    // Get LEVELNODE for struct and remove!
                    sNewGridNo = pBase.sGridNo;

                    // Check for openable flag....
                    if (pStructure.fFlags.HasFlag(STRUCTUREFLAGS.OPENABLE))
                    {
                        // ATE: Set a flag here - we need to know later that we're in an openable...
                        fObjectInOpenable = true;

                        // Something of note is here....
                        // SOME sort of structure is here.... set render flag to off
                        usFlags |= WORLD_ITEM.DONTRENDER;

                        // Openable.. check if it's closed, if so, set visiblity...
                        if (!(pStructure.fFlags.HasFlag(STRUCTUREFLAGS.OPEN)))
                        {
                            // -2 means - don't reveal!
                            bVisible = -2;
                        }

                        bRenderZHeightAboveLevel = CONVERT_INDEX_TO_PIXELS(StructureHeight(pStructure));
                        break;

                    }
                    // Else can we place an item on top?
                    else if (pStructure.fFlags.HasFlag(STRUCTUREFLAGS.GENERIC))
                    {
                        int ubLevel0, ubLevel1, ubLevel2, ubLevel3;

                        // If we are going into a raised struct AND we have above level set to -1
                        if (StructureInternals.StructureBottomLevel(pStructure) != 1 && fForceOnGround)
                        {
                            break;
                        }

                        // Find most dence area...
                        if (StructureInternals.StructureDensity(pStructure, out ubLevel0, out ubLevel1, out ubLevel2, out ubLevel3))
                        {
                            if (ubLevel3 == 0 && ubLevel2 == 0 && ubLevel1 == 0 && ubLevel0 == 0)
                            {
                                bRenderZHeightAboveLevel = 0;
                            }
                            else if (ubLevel3 >= ubLevel0 && ubLevel3 >= ubLevel2 && ubLevel3 >= ubLevel1)
                            {
                                bRenderZHeightAboveLevel = CONVERT_INDEX_TO_PIXELS(4);
                            }
                            else if (ubLevel2 >= ubLevel0 && ubLevel2 >= ubLevel1 && ubLevel2 >= ubLevel3)
                            {
                                bRenderZHeightAboveLevel = CONVERT_INDEX_TO_PIXELS(3);
                            }
                            else if (ubLevel1 >= ubLevel0 && ubLevel1 >= ubLevel2 && ubLevel1 >= ubLevel3)
                            {
                                bRenderZHeightAboveLevel = CONVERT_INDEX_TO_PIXELS(2);
                            }
                            else if (ubLevel0 >= ubLevel1 && ubLevel0 >= ubLevel2 && ubLevel0 >= ubLevel3)
                            {
                                bRenderZHeightAboveLevel = CONVERT_INDEX_TO_PIXELS(1);
                            }
                        }

                        // Set flag indicating it has an item on top!
                        pStructure.fFlags |= STRUCTURE_HASITEMONTOP;
                        break;
                    }

                }

                pStructure = StructureInternals.FindNextStructure(pStructure, STRUCTUREFLAGS.BLOCKSMOVES);
            }
        }

        if (pObject.usItem == Items.SWITCH && !fObjectInOpenable)
        {
            if (bVisible != -2)
            {
                // switch items which are not hidden inside objects should be considered buried
                bVisible = ItemVisibility.BURIED;
                // and they are pressure-triggered unless there is a switch structure there
                if (WorldStructures.FindStructure(psGridNo, STRUCTUREFLAGS.SWITCH) != null)
                {
                    pObject.bDetonatorType = DetonatorType.BOMB_SWITCH;
                }
                else
                {
                    pObject.bDetonatorType = DetonatorType.BOMB_PRESSURE;
                }
            }
            else
            {
                // else they are manually controlled
                pObject.bDetonatorType = DetonatorType.BOMB_SWITCH;
            }
        }
        else if (pObject.usItem == Items.ACTION_ITEM)
        {
            switch (pObject.bActionValue)
            {
                case ACTION_ITEM.SMALL_PIT:
                case ACTION_ITEM.LARGE_PIT:
                    // mark as known about by civs and creatures
                    gpWorldLevelData[sNewGridNo].uiFlags |= MAPELEMENTFLAGS.ENEMY_MINE_PRESENT;
                    break;
                default:
                    break;
            }
        }

        if (psGridNo != sNewGridNo)
        {
            psGridNo = sNewGridNo;
        }


        //First add the item to the global list.  This is so the game can keep track 
        //of where the items are, for file i/o, etc.
        iWorldItem = AddItemToWorld(psGridNo, pObject, ubLevel, usFlags, bRenderZHeightAboveLevel, bVisible);

        // Check for and existing pool on the object layer
        if (GetItemPool(psGridNo, out pItemPool, ubLevel))
        {

            // Add to exitsing pool
            // Add graphic
            pNode = AddItemGraphicToWorld((Item[pObject.usItem]), psGridNo, ubLevel);

            // Set pool head value in levelnode
            pNode.pItemPool = pItemPool;

            // Add New Node
            pItemPoolTemp = pItemPool;
            // Create new pool
            pItemPool = new();

            // Set Next to null
            pItemPool.pNext = null;
            // Set Item index
            pItemPool.iItemIndex = iWorldItem;
            // Get a link back!
            pItemPool.pLevelNode = pNode;

            if (pItemPoolTemp is not null)
            {
                // Get last item in list
                while (pItemPoolTemp.pNext != null)
                {
                    pItemPoolTemp = pItemPoolTemp.pNext;
                }

                // Set Next of previous
                pItemPoolTemp.pNext = pItemPool;
            }
            // Set Previous of new one
            pItemPool.pPrev = pItemPoolTemp;

        }
        else
        {
            pNode = AddItemGraphicToWorld((Item[pObject.usItem]), psGridNo, ubLevel);

            // Create new pool
            pItemPool = new ITEM_POOL();

            pNode.pItemPool = pItemPool;

            // Set prev to null
            pItemPool.pPrev = null;
            // Set next to null
            pItemPool.pNext = null;
            // Set Item index
            pItemPool.iItemIndex = iWorldItem;
            // Get a link back!
            pItemPool.pLevelNode = pNode;

            // Set flag to indicate item pool presence
            gpWorldLevelData[psGridNo].uiFlags |= MAPELEMENTFLAGS.ITEMPOOL_PRESENT;
        }

        // Set visible!
        pItemPool.bVisible = bVisible;

        // If bbisible is true, render makered world
        if (bVisible == 1 && GridNoOnScreen((psGridNo)))
        {
            //gpWorldLevelData[*psGridNo].uiFlags|=MAPELEMENT_REDRAW;
            //SetRenderFlags(RENDER_FLAG_MARKED);
            SetRenderFlags(RENDER_FLAG_FULL);
        }

        // Set flahs timer
        pItemPool.bFlashColor = 0;
        pItemPool.sGridNo = psGridNo;
        pItemPool.ubLevel = ubLevel;
        pItemPool.usFlags = usFlags;
        pItemPool.bVisible = bVisible;
        pItemPool.bRenderZHeightAboveLevel = bRenderZHeightAboveLevel;

        // ATE: Get head of pool again....
        if (GetItemPool(psGridNo, out pItemPool, ubLevel))
        {
            AdjustItemPoolVisibility(pItemPool);
        }

        if (piItemIndex is not null)
        {
            piItemIndex = iWorldItem;
        }

        return ((gWorldItems[iWorldItem].o));
    }

    public static bool ItemTypeExistsAtLocation(int sGridNo, Items usItem, int ubLevel, out int piItemIndex)
    {
        ITEM_POOL? pItemPool;
        ITEM_POOL? pItemPoolTemp;
        bool fItemFound = false;

        // Check for an existing pool on the object layer
        if (GetItemPool(sGridNo, out pItemPool, ubLevel))
        {
            // LOOP THROUGH LIST TO FIND ITEM WE WANT
            pItemPoolTemp = pItemPool;
            while (pItemPoolTemp != null)
            {
                if (Globals.gWorldItems[pItemPoolTemp.iItemIndex].o.usItem == usItem)
                {
                    piItemIndex = pItemPoolTemp.iItemIndex;

                    return (true);
                }
                pItemPoolTemp = pItemPoolTemp.pNext;
            }
        }

        piItemIndex = -1;
        return (false);
    }

    public static bool GetItemPool(int usMapPos, out ITEM_POOL? ppItemPool, int ubLevel)
    {
        LEVELNODE? pObject;

        if (ubLevel == 0)
        {
            pObject = Globals.gpWorldLevelData[usMapPos].pStructHead;
        }
        else
        {
            pObject = Globals.gpWorldLevelData[usMapPos].pOnRoofHead;
        }

        // LOOP THORUGH OBJECT LAYER
        while (pObject != null)
        {
            if (pObject.uiFlags.HasFlag(LEVELNODEFLAGS.ITEM))
            {
                (ppItemPool) = pObject.pItemPool;

                //DEF added the check because pObject.pItemPool was null which was causing problems
                if (ppItemPool is not null)
                {
                    return (true);
                }
                else
                {
                    return (false);
                }
            }

            pObject = pObject.pNext;
        }

        ppItemPool = null;
        return (false);
    }
}

public enum ITEM_HANDLE
{
    OK = 1,
    RELOADING = -1,
    UNCONSCIOUS = -2,
    NOAPS = -3,
    NOAMMO = -4,
    CANNOT_GETTO_LOCATION = -5,
    BROKEN = -6,
    NOROOM = -7,
    REFUSAL = -8,
}

public class ITEM_POOL
{
    public ITEM_POOL? pNext;
    public ITEM_POOL? pPrev;

    public int iItemIndex;
    public int bVisible;
    public int bFlashColor;
    public int uiTimerID;
    public int sGridNo;
    public int ubLevel;
    public WORLD_ITEM usFlags;
    public int bRenderZHeightAboveLevel;
    public LEVELNODE? pLevelNode;

}

public delegate void ITEM_POOL_LOCATOR_HOOK();
public struct ITEM_POOL_LOCATOR
{

    ITEM_POOL? pItemPool;

    // Additional info for locators
    public int bRadioFrame;
    public int uiLastFrameUpdate;
    public ITEM_POOL_LOCATOR_HOOK Callback;
    public bool fAllocated;
    public int ubFlags;

}

// visibility defines
public enum ItemVisibility
{
    ANY_VISIBILITY_VALUE = -10,
    HIDDEN_ITEM = -4,
    BURIED = -3,
    HIDDEN_IN_OBJECT = -2,
    INVISIBLE = -1,
    VISIBLE = 1,
}
