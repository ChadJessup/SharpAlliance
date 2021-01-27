using System;
using System.Threading.Tasks;

namespace SharpAlliance.Core.Screens
{
    public class TileCache
    {
        public ValueTask<bool> InitTileCache()
        {
            return ValueTask.FromResult(true);
        }
    }
}
