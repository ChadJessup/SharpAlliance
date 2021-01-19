using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpAlliance.Core
{
    public static class Utils
    {
        private static int GETPIXELDEPTH() => 16;

        public static string FilenameForBPP(string pFilename)
        {
            //            char Drive[128], Dir[128], Name[128], Ext[128];

            //        if (GETPIXELDEPTH() == 16)
            //        {
            // no processing for 16 bit names
            return pFilename;
            //          }
            //          else
            //          {
            //              _splitpath(pFilename, Drive, Dir, Name, Ext);
            //
            //              strcat(Name, "_8");
            //
            //              strcpy(pDestination, Drive);
            //              //strcat(pDestination, Dir);
            //              strcat(pDestination, DATA_8_BIT_DIR);
            //              strcat(pDestination, Name);
            //              strcat(pDestination, Ext);
            //          }

        }

    }
}
