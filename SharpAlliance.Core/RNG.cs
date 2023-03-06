using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core;

public class RNG
{
    public static int PreRandom(int uiRange)
    {
        int uiNum;
# if JA2BETAVERSION
        if (gfCountRandoms)
        {
            guiPreRandoms++;
        }
#endif
        if (uiRange == 0)
        {
            return 0;
        }

        //Extract the current pregenerated number
        uiNum = guiPreRandomNums[guiPreRandomIndex] * uiRange / RAND_MAX % uiRange;
        //Replace the current pregenerated number with a new one.

        //This was removed in the name of optimization.  Uncomment if you hate recycling.
        //guiPreRandomNums[ guiPreRandomIndex ] = rand();

        //Go to the next index.
        guiPreRandomIndex++;
        if (guiPreRandomIndex >= (int)MAX_PREGENERATED_NUMS)
        {
            guiPreRandomIndex = 0;
        }

        return uiNum;
    }
}
