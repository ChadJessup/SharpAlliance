using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpAlliance.Core.SubSystems
{
    public class InterfaceDialogSubSystem
    {
        // GLOBAL NPC STRUCT
        //public NPC_DIALOGUE_TYPE gTalkPanel;
        public bool gfInTalkPanel = false;
        public SOLDIERTYPE gpSrcSoldier = null;
        public SOLDIERTYPE gpDestSoldier = null;
        public int gubSrcSoldierProfile;
        public int gubNiceNPCProfile = SoldierControl.NO_PROFILE;
        public int gubNastyNPCProfile = SoldierControl.NO_PROFILE;

        public int gubTargetNPC;
        public int gubTargetRecord;
        public int gubTargetApproach;
        public bool gfShowDialogueMenu;
        public bool gfWaitingForTriggerTimer;
        public int guiWaitingForTriggerTime;
        public int iInterfaceDialogueBox = -1;
        public int ubRecordThatTriggeredLiePrompt;
        public bool gfConversationPending = false;
        public SOLDIERTYPE gpPendingDestSoldier;
        public SOLDIERTYPE gpPendingSrcSoldier;
        public int gbPendingApproach;
        public int guiPendingApproachData;

        public bool ProfileCurrentlyTalkingInDialoguePanel(int ubProfile)
        {
            if (this.gfInTalkPanel)
            {
                if (this.gpDestSoldier != null)
                {
                    if (this.gpDestSoldier.ubProfile == ubProfile)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
