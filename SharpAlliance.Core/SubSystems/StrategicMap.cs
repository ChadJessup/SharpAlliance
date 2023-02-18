using System;
using System.Threading.Tasks;

namespace SharpAlliance.Core.SubSystems
{
    public class StrategicMap
    {
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
