using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpAlliance.Core.Managers;

namespace SharpAlliance.Core.SubSystems;

public class MapScreenInterfaceBottom
{
    private static bool fMapScreenBottomDirty;

    public static void MoveToEndOfMapScreenMessageList()
    {
        int ubDesiredMessageIndex;
        int ubNumMessages;

        ubNumMessages = Messages.GetRangeOfMapScreenMessages();

        ubDesiredMessageIndex = ubNumMessages - Math.Min(ubNumMessages, Globals.MAX_MESSAGES_ON_MAP_BOTTOM);
        ChangeCurrentMapscreenMessageIndex(ubDesiredMessageIndex);
    }

    public static void ChangeCurrentMapscreenMessageIndex(int ubNewMessageIndex)
    {
        Debug.Assert(ubNewMessageIndex + Globals.MAX_MESSAGES_ON_MAP_BOTTOM <= Math.Max(Globals.MAX_MESSAGES_ON_MAP_BOTTOM, Messages.GetRangeOfMapScreenMessages()));

        Globals.gubFirstMapscreenMessageIndex = ubNewMessageIndex;
        Globals.gubCurrentMapMessageString = (Globals.gubStartOfMapScreenMessageList + Globals.gubFirstMapscreenMessageIndex) % 256;

        // set fact we just went to a new message
        //	gfNewScrollMessage = TRUE;

        // refresh screen
        fMapScreenBottomDirty = true;
    }
}
