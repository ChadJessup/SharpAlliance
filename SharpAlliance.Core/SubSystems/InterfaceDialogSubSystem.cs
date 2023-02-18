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

        public int gubTargetNPC;
        public int gubTargetRecord;
        public int gubTargetApproach;
        public bool gfShowDialogueMenu;
        public bool gfWaitingForTriggerTimer;
        public int guiWaitingForTriggerTime;
        public int iInterfaceDialogueBox = -1;
        public int ubRecordThatTriggeredLiePrompt;
        public bool gfConversationPending = false;
        public int gbPendingApproach;
        public int guiPendingApproachData;

        public bool ProfileCurrentlyTalkingInDialoguePanel(NPCID ubProfile)
        {
            if (Globals.gfInTalkPanel)
            {
                if (Globals.gpDestSoldier != null)
                {
                    if (Globals.gpDestSoldier.ubProfile == ubProfile)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
