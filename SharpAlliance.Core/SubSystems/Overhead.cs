using System;

namespace SharpAlliance.Core.SubSystems
{
    public class Overhead
    {
        public TacticalStatusType gTacticalStatus { get; set; } = new TacticalStatusType();

        public void InitOverhead()
        {
        }
    }

    public class TacticalStatusType
    {
        public bool fHasAGameBeenStarted { get; set; }
    }
}
