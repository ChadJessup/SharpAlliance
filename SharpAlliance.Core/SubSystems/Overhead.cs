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

        public bool InOverheadMap()
        {
            return false;
        }
    }

    public class TacticalStatusType
    {
        public bool fHasAGameBeenStarted { get; set; }
        public int ubAttackBusyCount { get; set; }
        public TacticalEngineStatus uiFlags { get; set; }
        public bool fAtLeastOneGuyOnMultiSelect { get; set; }
        public bool fUnLockUIAfterHiddenInterrupt { get; set; }
        public uint uiTactialTurnLimitClock { get; set; }
    }
}
