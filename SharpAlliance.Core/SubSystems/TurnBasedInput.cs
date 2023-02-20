using System;

namespace SharpAlliance.Core.SubSystems
{
    public class TurnBasedInput
    {
        public static void HandleStanceChangeFromUIKeys(AnimationHeights ubAnimHeight)
        {
            // If we have multiple guys selected, make all change stance!
            SOLDIERTYPE? pSoldier;
            int cnt;
            SOLDIERTYPE? pFirstSoldier = null;

            if (Globals.gTacticalStatus.fAtLeastOneGuyOnMultiSelect)
            {
                // OK, loop through all guys who are 'multi-selected' and
                // check if our currently selected guy is amoung the
                // lucky few.. if not, change to a guy who is...
                cnt = Globals.gTacticalStatus.Team[Globals.gbPlayerNum].bFirstID;
                for (pSoldier = Globals.MercPtrs[cnt]; cnt <= Globals.gTacticalStatus.Team[Globals.gbPlayerNum].bLastID; cnt++)//, pSoldier++)
                {
                    if (pSoldier.bActive && pSoldier.bInSector)
                    {
                        if (pSoldier.uiStatusFlags.HasFlag(SOLDIER.MULTI_SELECTED))
                        {
                            UIHandleSoldierStanceChange(pSoldier.ubID, ubAnimHeight);
                        }
                    }
                }
            }
            else
            {
                if (Globals.gusSelectedSoldier != Globals.NO_SOLDIER)
                {
                    UIHandleSoldierStanceChange(Globals.gusSelectedSoldier, ubAnimHeight);
                }
            }
        }
    }
}
