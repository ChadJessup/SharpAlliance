using System;
using System.Threading.Tasks;

namespace SharpAlliance.Core.SubSystems
{
    public class Overhead
    {
        public TacticalStatusType gTacticalStatus { get; set; } = new TacticalStatusType();

        public void InitOverhead()
        {
        }

        public ValueTask<bool> InitTacticalEngine()
        {
            return ValueTask.FromResult(true);
        }
    }

    public class TacticalStatusType
    {
        public bool fHasAGameBeenStarted { get; set; }
    }
}
