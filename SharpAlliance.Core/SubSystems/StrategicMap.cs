using System;
using System.Threading.Tasks;

namespace SharpAlliance.Core.SubSystems
{
    public class StrategicMap
    {
        // For speed, etc lets make these globals, forget the functions if you want
        public int gWorldSectorX { get; set; }
        public int gWorldSectorY { get; set; }
        public int gbWorldSectorZ { get; set; }

        public ValueTask<bool> InitStrategicMovementCosts()
        {
            return ValueTask.FromResult(true);
        }

        public ValueTask<bool> InitStrategicEngine()
        {
            return ValueTask.FromResult(true);
        }
    }
}
