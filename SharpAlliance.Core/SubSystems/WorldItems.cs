using System;
using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core.SubSystems;

public class WorldItems
{
    public static void RemoveItemFromWorld(int iItemIndex)
    {
        // Ensure the item still exists, then if it's a bomb,
        // remove the appropriate entry from the bomb table
        if (gWorldItems[iItemIndex].fExists)
        {
            if (gWorldItems[iItemIndex].usFlags.HasFlag(WORLD_ITEM.ARMED_BOMB))
            {
                RemoveBombFromWorldByItemIndex(iItemIndex);
            }

            gWorldItems[iItemIndex].fExists = false;
        }
    }

    public static void RemoveBombFromWorld(int iBombIndex)
    {
        //Remove the world bomb from the table.
        gWorldBombs[iBombIndex].fExists = false;
    }

    public static void RemoveBombFromWorldByItemIndex(int iItemIndex)
    {
        // Find the world bomb which corresponds with a particular world item, then
        // remove the world bomb from the table.
        int uiBombIndex;

        for (uiBombIndex = 0; uiBombIndex < guiNumWorldBombs; uiBombIndex++)
        {
            if (gWorldBombs[uiBombIndex].fExists && gWorldBombs[uiBombIndex].iItemIndex == iItemIndex)
            {
                RemoveBombFromWorld(uiBombIndex);
                return;
            }
        }
    }
}

public class WORLDITEM
{
    public bool fExists;
    public int sGridNo;
    public int ubLevel;
    public OBJECTTYPE o;
    public WORLD_ITEM usFlags;
    public int bRenderZHeightAboveLevel;
    public int bVisible;

    //This is the chance associated with an item or a trap not-existing in the world.  The reason why 
    //this is reversed (10 meaning item has 90% chance of appearing, is because the order that the map 
    //is saved, we don't know if the version is older or not until after the items are loaded and added.
    //Because this value is zero in the saved maps, we can't change it to 100, hence the reversal method.
    //This check is only performed the first time a map is loaded.  Later, it is entirely skipped.
    public int ubNonExistChance;
}

public record WORLDBOMB
{
    public bool fExists;
    public int iItemIndex;
}

[Flags]
public enum WORLD_ITEM
{
    DONTRENDER = 0x0001,
    FIND_SWEETSPOT_FROM_GRIDNO = 0x0002,
    ARMED_BOMB = 0x0040,
    SCIFI_ONLY = 0x0080,
    REALISTIC_ONLY = 0x0100,
    REACHABLE = 0x0200,
    GRIDNO_NOT_SET_USE_ENTRY_POINT = 0x0400,
}
