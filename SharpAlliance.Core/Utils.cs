using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpAlliance.Core;

public static class Utils
{
    private static int GETPIXELDEPTH() => 16;
    private static string DATA_8_BIT_DIR = "8-Bit\\";

    public static string FilenameForBPP(string pFilename)
    {
        string Drive, Dir, Name, Ext;

        if (GETPIXELDEPTH() == 16)
        {
            // no processing for 16 bit names
            return pFilename;
        }
        else
        {
            //_splitpath(pFilename, Drive, Dir, Name, Ext);

            //strcat(Name, "_8");

            //pDestination = Drive
            //    + DATA_8_BIT_DIR
            //    + Name + "_8"
            //    + Ext;
        }

        return pFilename;
    }

}

public static class RandomHelpers
{
    // Maximum value that can be returned by the rand function:
    private const int RAND_MAX = 0x7fff;

    // Returns a pseudo-random integer between 0 and uiRange
    public static int GetRandom(this Random rand, int uiRange)
    {
        // Always return 0, if no range given (it's not an error)

        if (uiRange == 0)
        {
            return 0;
        }

        return rand.Next(0, RAND_MAX) * uiRange / RAND_MAX % uiRange;
    }
}
