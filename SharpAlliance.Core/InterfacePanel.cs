using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core;

public partial class Globals
{
}

public class InterfacePanel
{
    public static void ShowRadioLocator(int ubID, SHOW_LOCATOR ubLocatorSpeed)
    {
        RESETTIMECOUNTER(ref MercPtrs[ubID].FlashSelCounter, FLASH_SELECTOR_DELAY);

        //LocateSoldier( ubID, FALSE );	// IC - this is already being done outside of this function :)
        MercPtrs[ubID].fFlashLocator = true;
        //gbPanelSelectedGuy = ubID;	IC - had to move this outside to make this function versatile
        MercPtrs[ubID].sLocatorFrame = 0;

        if (ubLocatorSpeed == SHOW_LOCATOR.NORMAL)
        {
            // If we are an AI guy, and we have the baton, make lower...
            // ( MercPtrs[ ubID ]->uiStatusFlags & SOLDIER_UNDERAICONTROL && MercPtrs[ ubID ]->bTeam != gbPlayerNum )
            //
            //ercPtrs[ ubID ]->ubNumLocateCycles = 3;
            //
            //se
            //
            MercPtrs[ubID].ubNumLocateCycles = 5;
            //
        }
        else
        {
            MercPtrs[ubID].ubNumLocateCycles = 3;
        }
    }
}

public enum SHOW_LOCATOR
{
    NORMAL = 1,
    FAST = 2,
}
