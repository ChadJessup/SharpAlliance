using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.DXGI;

namespace SharpAlliance.Core
{
    public static class DirectXHelpers
    {
        public static void DDReleaseSurface(ref Surface ppOldSurface1, ref Surface2 ppOldSurface2)
        {
            //Assert(ppOldSurface1 != NULL);
            //Assert(ppOldSurface2 != NULL);
            //Assert(*ppOldSurface1 != NULL);
            //Assert(*ppOldSurface2 != NULL);

            ppOldSurface2.Dispose();
            ppOldSurface1.Dispose();

            //ppOldSurface1 = null;
            //ppOldSurface2 = null;
        }
    }
}
