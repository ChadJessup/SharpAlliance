using System;
using System.Diagnostics;
using SharpAlliance.Core.Managers;
using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core.SubSystems;

public class MapScreenInterfaceBottom
{
    public static void MoveToEndOfMapScreenMessageList()
    {
        int ubDesiredMessageIndex;
        int ubNumMessages;

        ubNumMessages = Messages.GetRangeOfMapScreenMessages();

        ubDesiredMessageIndex = ubNumMessages - Math.Min(ubNumMessages, Globals.MAX_MESSAGES_ON_MAP_BOTTOM);
        ChangeCurrentMapscreenMessageIndex(ubDesiredMessageIndex);
    }

    public static void ChangeCurrentMapscreenMessageIndex(int ubNewMessageIndex)
    {
        Debug.Assert(ubNewMessageIndex + Globals.MAX_MESSAGES_ON_MAP_BOTTOM <= Math.Max(Globals.MAX_MESSAGES_ON_MAP_BOTTOM, Messages.GetRangeOfMapScreenMessages()));

        Globals.gubFirstMapscreenMessageIndex = ubNewMessageIndex;
        Globals.gubCurrentMapMessageString = (Globals.gubStartOfMapScreenMessageList + Globals.gubFirstMapscreenMessageIndex) % 256;

        // set fact we just went to a new message
        //	gfNewScrollMessage = true;

        // refresh screen
        fMapScreenBottomDirty = true;
    }

    public static bool AllowedToTimeCompress()
    {
        // if already leaving, disallow any other attempts to exit
        if (fLeavingMapScreen)
        {
            return (false);
        }

        // if already going someplace
        if (gbExitingMapScreenToWhere != -1)
        {
            return (false);
        }

        // if we're locked into paused time compression by some event that enforces that
        if (GameClock.PauseStateLocked())
        {
            return (false);
        }

        // meanwhile coming up
        if (gfMeanwhileTryingToStart)
        {
            return (false);
        }

        // someone has something to say
//        if (!DialogueQueueIsEmpty())
//        {
//            return (false);
//        }

        // moving / confirming movement
//        if ((bSelectedDestChar != -1) || fPlotForHelicopter || gfInConfirmMapMoveMode || fShowMapScreenMovementList)
//        {
//            return (false);
//        }

//        if (fShowAssignmentMenu || fShowTrainingMenu || fShowAttributeMenu || fShowSquadMenu || fShowContractMenu || fShowRemoveMenu)
//        {
//            return (false);
//        }

//        if (fShowUpdateBox || fShowTownInfo || (sSelectedMilitiaTown != 0))
//        {
//            return (false);
//        }

        // renewing contracts
        if (gfContractRenewalSquenceOn)
        {
            return (false);
        }

        // disabled due to battle?
        if ((fDisableMapInterfaceDueToBattle) || (fDisableDueToBattleRoster))
        {
            return (false);
        }

        // if holding an inventory item
        if (fMapInventoryItem)
        {
            return (false);
        }

        // show the inventory pool?
        if (fShowMapInventoryPool)
        {
            // prevent time compress (items get stolen over time, etc.)
            return (false);
        }

        // no mercs have ever been hired
        if (gfAtLeastOneMercWasHired == false)
        {
            return (false);
        }

        /*
            //in air raid
            if( InAirRaid( ) == true )
            {
                return( false );
            }
        */

        // no usable mercs on team!
//        if (!AnyUsableRealMercenariesOnTeam())
//        {
//            return (false);
//        }

        // must wait till bombs go off
//        if (ActiveTimedBombExists())
//        {
//            return (false);
//        }

        // hostile sector / in battle
        if ((gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.INCOMBAT))
            || (gTacticalStatus.fEnemyInSector))
        {
            return (false);
        }

//        if (PlayerGroupIsInACreatureInfestedMine())
//        {
//            return false;
//        }

        return (true);
    }
}
