using System;
using System.Diagnostics;
using SharpAlliance.Core.Managers;

using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core.SubSystems;

public class MapScreenInterfaceBottom
{
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
