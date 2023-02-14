using System;
using System.Threading.Tasks;

namespace SharpAlliance.Core.SubSystems
{
    public class World
    {
        public const int LANDHEAD = 0;
        public const int MAXDIR = 8;

        public void TrashWorld()
        {
        }

        public ValueTask<bool> InitializeWorld()
        {
            return ValueTask.FromResult(true);
        }
    }
}
