using System;
using System.Threading.Tasks;
using Veldrid.OpenGLBinding;

namespace SharpAlliance.Core.SubSystems
{
    public class Overhead
    {
        public TacticalStatusType gTacticalStatus { get; set; } = new TacticalStatusType();

        public ushort gusSelectedSoldier { get; set; }
        public ushort gusOldSelectedSoldier {get;set;}

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

        public bool GetSoldier(SOLDIERTYPE? ppSoldier, int usSoldierIndex)
        {
            // Check range of index given
            ppSoldier = null;

            if (usSoldierIndex < 0 || usSoldierIndex > TOTAL_SOLDIERS - 1)
            {
                // Set debug message
                return (false);
            }

            // Check if a guy exists here
            // Does another soldier exist here?
            if (MercPtrs[usSoldierIndex]->bActive)
            {
                // Set Existing guy
                ppSoldier = MercPtrs[usSoldierIndex];
                return (true);
            }
            else
            {
                return (false);
            }
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
