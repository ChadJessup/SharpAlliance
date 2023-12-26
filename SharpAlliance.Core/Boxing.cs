using SharpAlliance.Core.Managers;
using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core;

public class Boxing
{
    public static bool BoxerExists()
    {
        for (var uiLoop = 0; uiLoop < NUM_BOXERS; uiLoop++)
        {
            if (WorldManager.WhoIsThere2(gsBoxerGridNo[uiLoop], 0) != NOBODY)
            {
                return true;
            }
        }

        return false;
    }
}

public enum DisqualificationReasons
{
    BOXER_OUT_OF_RING,
    NON_BOXER_IN_RING,
    BAD_ATTACK
}
