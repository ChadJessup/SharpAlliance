using System;
using System.Threading.Tasks;
using Veldrid.OpenGLBinding;

namespace SharpAlliance.Core.SubSystems
{
    public class Overhead
    {
        public TacticalStatusType gTacticalStatus { get; set; } = new TacticalStatusType();

        public ushort gusSelectedSoldier { get; set; }
        public ushort gusOldSelectedSoldier { get; set; }

        public TEAM gbPlayerNum { get; set; }
        public bool gbShowEnemies { get; set; }


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

        public bool GetSoldier(out SOLDIERTYPE? ppSoldier, int usSoldierIndex)
        {
            // Check range of index given
            ppSoldier = null;

            if (usSoldierIndex < 0 || usSoldierIndex > OverheadTypes.TOTAL_SOLDIERS - 1)
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
        public TEAM ubCurrentTeam { get; set; }

        public TacticalTeamType[] Team = new TacticalTeamType[OverheadTypes.MAXTEAMS];
        public bool fHasAGameBeenStarted { get; set; }
        public int ubAttackBusyCount { get; set; }
        public TacticalEngineStatus uiFlags { get; set; }
        public bool fAtLeastOneGuyOnMultiSelect { get; set; }
        public bool fUnLockUIAfterHiddenInterrupt { get; set; }
        public uint uiTactialTurnLimitClock { get; set; }
    }
}

// TACTICAL ENGINE STATUS FLAGS
public struct TacticalTeamType
{
    public int RadarColor;
    public int bFirstID;
    public int bLastID;
    public int bSide;
    public int bMenInSector;
    public int ubLastMercToRadio;
    public int bTeamActive;
    public int bAwareOfOpposition;
    public int bHuman;
}
