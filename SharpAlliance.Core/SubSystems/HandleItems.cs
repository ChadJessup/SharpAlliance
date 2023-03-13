using System;
using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core.SubSystems;

public class HandleItems
{
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
    public int usFlags;
    public int bRenderZHeightAboveLevel;
    public LEVELNODE? pLevelNode;

}

public delegate void ITEM_POOL_LOCATOR_HOOK();
public struct ITEM_POOL_LOCATOR
{

    ITEM_POOL? pItemPool;

// Additional info for locators
public int  bRadioFrame;
public int  uiLastFrameUpdate;
public ITEM_POOL_LOCATOR_HOOK Callback;
public bool fAllocated;
public int   ubFlags;

}
