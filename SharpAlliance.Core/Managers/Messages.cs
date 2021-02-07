using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpAlliance.Core.Managers
{
    public class Messages
    {
        public void DisableScrollMessages()
        {
            // will stop the scroll of messages in tactical and hide them during an NPC's dialogue
            // disble video overlay for tatcitcal scroll messages
            EnableDisableScrollStringVideoOverlay(false);
        }

        private void EnableDisableScrollStringVideoOverlay(bool fEnable)
        {
        }
    }
}
