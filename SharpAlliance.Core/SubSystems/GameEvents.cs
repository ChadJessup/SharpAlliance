using System;
using System.Diagnostics;
using System.IO;
using SharpAlliance.Core.Screens;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core.SubSystems;

public class GameEvents
{
    //Determines if there are any events that will be processed between the current global time,
    //and the beginning of the next global time.
    public static bool GameEventsPending(uint uiAdjustment)
    {
        if (gpEventList is null)
        {
            return false;
        }

        if (gpEventList.uiTimeStamp <= GameClock.GetWorldTotalSeconds() + uiAdjustment)
        {
            return true;
        }
        return false;
    }

    public static bool AddGameEvent(eJA2Events uiEvent, int usDelay, object pEventData)
    {
        if (usDelay == DEMAND_EVENT_DELAY)
        {
            //DebugMsg( TOPIC_JA2, DBG_LEVEL_3, String("AddGameEvent: Sending Local and network #%d", uiEvent));
            return AddGameEventToQueue(uiEvent, 0, pEventData, EVENT_QUEUE.DEMAND_EVENT_QUEUE);
        }
        else if (uiEvent < eJA2Events.EVENTS_LOCAL_AND_NETWORK)
        {
            //DebugMsg( TOPIC_JA2, DBG_LEVEL_3, String("AddGameEvent: Sending Local and network #%d", uiEvent));
            return AddGameEventToQueue(uiEvent, usDelay, pEventData, EVENT_QUEUE.PRIMARY_EVENT_QUEUE);
        }
        else if (uiEvent < eJA2Events.EVENTS_ONLY_USED_LOCALLY)
        {
            //DebugMsg( TOPIC_JA2, DBG_LEVEL_3, String("AddGameEvent: Sending Local #%d", uiEvent));
            return AddGameEventToQueue(uiEvent, usDelay, pEventData, EVENT_QUEUE.PRIMARY_EVENT_QUEUE);
        }
        else if (uiEvent < eJA2Events.EVENTS_ONLY_SENT_OVER_NETWORK)
        {
            //DebugMsg( TOPIC_JA2, DBG_LEVEL_3, String("AddGameEvent: Sending network #%d", uiEvent));
            return true;
        }
        // There is an error with the event
        else
        {
            return false;
        }
    }

    public static bool AddGameEventToQueue(eJA2Events uiEvent, int usDelay, object pEventData, EVENT_QUEUE ubQueueID)
    {
        int uiDataSize;

        // Check range of Event ui
        if (uiEvent < 0 || uiEvent > eJA2Events.NUM_EVENTS)
        {
            // Set debug message!
            // DebugMsg(TOPIC_JA2, DBG_LEVEL_3, "Event Pump: Unknown event type");
            return false;
        }

        // Switch on event type and set size accordingly
        switch (uiEvent)
        {
            case eJA2Events.E_PLAYSOUND:

//                uiDataSize = sizeof(EV_E_PLAYSOUND);
                break;

            case eJA2Events.S_CHANGESTATE:

//                uiDataSize = sizeof(EV_S_CHANGESTATE);
                break;


            case eJA2Events.S_CHANGEDEST:

//                uiDataSize = sizeof(EV_S_CHANGEDEST);
                break;


            case eJA2Events.S_SETPOSITION:

//                uiDataSize = sizeof(EV_S_SETPOSITION);
                break;

            case eJA2Events.S_GETNEWPATH:

//                uiDataSize = sizeof(EV_S_GETNEWPATH);
                break;

            case eJA2Events.S_BEGINTURN:

//                uiDataSize = sizeof(EV_S_BEGINTURN);
                break;

            case eJA2Events.S_CHANGESTANCE:

//                uiDataSize = sizeof(EV_S_CHANGESTANCE);
                break;

            case eJA2Events.S_SETDIRECTION:

//                uiDataSize = sizeof(EV_S_SETDIRECTION);
                break;

            case eJA2Events.S_SETDESIREDDIRECTION:

//                uiDataSize = sizeof(EV_S_SETDESIREDDIRECTION);
                break;

            case eJA2Events.S_FIREWEAPON:

//                uiDataSize = sizeof(EV_S_FIREWEAPON);
                break;

            case eJA2Events.S_BEGINFIREWEAPON:

//                uiDataSize = sizeof(EV_S_BEGINFIREWEAPON);
                //Delay this event
                break;

            case eJA2Events.S_WEAPONHIT:

//                uiDataSize = sizeof(EV_S_WEAPONHIT);
                break;

            case eJA2Events.S_STRUCTUREHIT:
//                uiDataSize = sizeof(EV_S_STRUCTUREHIT);
                break;

            case eJA2Events.S_WINDOWHIT:
//                uiDataSize = sizeof(EV_S_STRUCTUREHIT);
                break;

            case eJA2Events.S_MISS:
//                uiDataSize = sizeof(EV_S_MISS);
                break;

            case eJA2Events.S_NOISE:
//                uiDataSize = sizeof(EV_S_NOISE);
                break;

            case eJA2Events.S_STOP_MERC:
//                uiDataSize = sizeof(EV_S_STOP_MERC);
                break;

            case eJA2Events.S_SENDPATHTONETWORK:
//                uiDataSize = sizeof(EV_S_SENDPATHTONETWORK);
                break;

            case eJA2Events.S_UPDATENETWORKSOLDIER:
//                uiDataSize = sizeof(EV_S_UPDATENETWORKSOLDIER);
                break;

            default:

                // Set debug msg: unknown message!
                //DebugMsg(TOPIC_JA2, DBG_LEVEL_3, "Event Pump: Event Type mismatch");
                return false;

        }


//        CHECKF(AddEvent(uiEvent, usDelay, pEventData, uiDataSize, ubQueueID));

        // successful
        return true;
    }

    //returns true if any events were deleted
    public static bool DeleteEventsWithDeletionPending()
    {
        STRATEGICEVENT? curr, prev, temp;
        bool fEventDeleted = false;
        //ValidateGameEvents();
        curr = gpEventList;
        prev = null;
        while (curr is not null)
        {
            //ValidateGameEvents();
            if (curr.ubFlags.HasFlag(SEF.DELETION_PENDING))
            {
                if (prev is not null)
                { //deleting node in middle
                    prev.next = curr.next;
                    temp = curr;
                    curr = curr.next;
                    temp = null;
                    fEventDeleted = true;
                    //ValidateGameEvents();
                    continue;
                }
                else
                { //deleting head
                    gpEventList = gpEventList.next;
                    temp = curr;
                    prev = null;
                    curr = curr.next;
                    temp = null;
                    fEventDeleted = true;
                    //ValidateGameEvents();
                    continue;
                }
            }
            prev = curr;
            curr = curr.next;
        }
        gfEventDeletionPending = false;
        return fEventDeleted;
    }


    public static void AdjustClockToEventStamp(STRATEGICEVENT? pEvent, ref uint puiAdjustment)
    {
        uint uiDiff;

        uiDiff = pEvent.uiTimeStamp - guiGameClock;
        guiGameClock += uiDiff;
        puiAdjustment -= uiDiff;

        //Calculate the day, hour, and minutes.
        guiDay = guiGameClock / NUM_SEC_IN_DAY;
        guiHour = (guiGameClock - (guiDay * NUM_SEC_IN_DAY)) / NUM_SEC_IN_HOUR;
        guiMin = (guiGameClock - ((guiDay * NUM_SEC_IN_DAY) + (guiHour * NUM_SEC_IN_HOUR))) / NUM_SEC_IN_MIN;

        //wprintf(gswzWorldTimeStr, "%s %d, %02d:%02d", gpGameClockString[STR_GAMECLOCK_DAY_NAME], guiDay, guiHour, guiMin);
    }

    //If there are any events pending, they are processed, until the time limit is reached, or
    //a major event is processed (one that requires the player's attention).
    public static void ProcessPendingGameEvents(uint uiAdjustment, WARPTIME ubWarpCode)
    {
        STRATEGICEVENT? curr, pEvent, prev, temp;
        bool fDeleteEvent = false, fDeleteQueuedEvent = false;

        gfTimeInterrupt = false;
        gfProcessingGameEvents = true;

        //While we have events inside the time range to be updated, process them...
        curr = gpEventList;
        prev = null; //prev only used when warping time to target time.
        while (!gfTimeInterrupt && curr is not null && curr.uiTimeStamp <= guiGameClock + uiAdjustment)
        {
            fDeleteEvent = false;
            //Update the time by the difference, but ONLY if the event comes after the current time.
            //In the beginning of the game, series of events are created that are placed in the list
            //BEFORE the start time.  Those events will be processed without influencing the actual time.
            if (curr.uiTimeStamp > guiGameClock && ubWarpCode != WARPTIME.PROCESS_TARGET_TIME_FIRST)
            {
                AdjustClockToEventStamp(curr, ref uiAdjustment);
            }
            //Process the event
            if (ubWarpCode != WARPTIME.PROCESS_TARGET_TIME_FIRST)
            {
                fDeleteEvent = GameEventHook.ExecuteStrategicEvent(curr);
            }
            else if (curr.uiTimeStamp == guiGameClock + uiAdjustment)
            { //if we are warping to the target time to process that event first,
                if (curr.next is null || curr.next.uiTimeStamp > guiGameClock + uiAdjustment)
                { //make sure that we are processing the last event for that second
                    AdjustClockToEventStamp(curr, ref uiAdjustment);

                    fDeleteEvent = GameEventHook.ExecuteStrategicEvent(curr);

                    if (curr is not null && prev is not null && fDeleteQueuedEvent)
                    { //The only case where we are deleting a node in the middle of the list
                        prev.next = curr.next;
                    }
                }
                else
                { //We are at the current target warp time however, there are still other events following in this time cycle.
                  //We will only target the final event in this time.  NOTE:  Events are posted using a FIFO method
                    prev = curr;
                    curr = curr.next;
                    continue;
                }
            }
            else
            { //We are warping time to the target time.  We haven't found the event yet,
              //so continuing will keep processing the list until we find it.  NOTE:  Events are posted using a FIFO method
                prev = curr;
                curr = curr.next;
                continue;
            }
            if (fDeleteEvent)
            {
                //Determine if event node is a special event requiring reposting
                switch (curr.ubEventType)
                {
                    case EVENTPERIOD.RANGED_EVENT:
                        AddAdvancedStrategicEvent(EVENTPERIOD.ENDRANGED_EVENT, curr.ubCallbackID, curr.uiTimeStamp + curr.uiTimeOffset, curr.uiParam);
                        break;
                    case EVENTPERIOD.PERIODIC_EVENT:
                        pEvent = AddAdvancedStrategicEvent(EVENTPERIOD.PERIODIC_EVENT, curr.ubCallbackID, curr.uiTimeStamp + curr.uiTimeOffset, curr.uiParam);
                        if (pEvent is not null)
                        {
                            pEvent.uiTimeOffset = curr.uiTimeOffset;
                        }
                        break;
                    case EVENTPERIOD.EVERYDAY_EVENT:
                        AddAdvancedStrategicEvent(EVENTPERIOD.EVERYDAY_EVENT, curr.ubCallbackID, curr.uiTimeStamp + NUM_SEC_IN_DAY, curr.uiParam);
                        break;
                }
                if (curr == gpEventList)
                {
                    gpEventList = gpEventList.next;
                    curr = null;
                    curr = gpEventList;
                    prev = null;
                    //ValidateGameEvents();
                }
                else
                {
                    temp = curr;
                    prev.next = curr.next;
                    curr = curr.next;
                    temp = null;
                    //ValidateGameEvents();
                }
            }
            else
            {
                prev = curr;
                curr = curr.next;
            }
        }

        gfProcessingGameEvents = false;

        if (gfEventDeletionPending)
        {
            DeleteEventsWithDeletionPending();
        }

        if (uiAdjustment > 0 && !gfTimeInterrupt)
        {
            guiGameClock += (uint)uiAdjustment;
        }
    }


    public static bool AddSameDayStrategicEvent(EVENT ubCallbackID, uint uiMinStamp, int uiParam)
    {
        return AddStrategicEvent(ubCallbackID, uiMinStamp + GameClock.GetWorldDayInMinutes(), uiParam);
    }

    public static bool AddSameDayStrategicEventUsingSeconds(EVENT ubCallbackID, uint uiSecondStamp, int uiParam)
    {
        return AddStrategicEventUsingSeconds(ubCallbackID, uiSecondStamp + GameClock.GetWorldDayInSeconds(), uiParam);
    }

    public static bool AddFutureDayStrategicEvent(EVENT ubCallbackID, uint uiMinStamp, int uiParam, uint uiNumDaysFromPresent)
    {
        uint uiDay;
        uiDay = GameClock.GetWorldDay();
        return AddStrategicEvent(ubCallbackID, uiMinStamp + GameClock.GetFutureDayInMinutes(uiDay + uiNumDaysFromPresent), uiParam);
    }

    bool AddFutureDayStrategicEventUsingSeconds(EVENT ubCallbackID, uint uiSecondStamp, int uiParam, uint uiNumDaysFromPresent)
    {
        uint uiDay;
        uiDay = GameClock.GetWorldDay();
        return AddStrategicEventUsingSeconds(ubCallbackID, uiSecondStamp + GameClock.GetFutureDayInMinutes(uiDay + uiNumDaysFromPresent) * 60, uiParam);
    }

    public static STRATEGICEVENT? AddAdvancedStrategicEvent(EVENTPERIOD ubEventType, EVENT ubCallbackID, uint uiTimeStamp, object uiParam)
    {
        STRATEGICEVENT? pNode, pNewNode, pPrevNode;

        if (gfProcessingGameEvents && uiTimeStamp <= guiTimeStampOfCurrentlyExecutingEvent)
        { //Prevents infinite loops of posting events that are the same time or earlier than the event
          //currently being processed.
          return null;
        }

        pNewNode = new STRATEGICEVENT();// MemAlloc(sizeof(STRATEGICEVENT));
        Debug.Assert(pNewNode is not null);
        
        pNewNode.ubCallbackID = ubCallbackID;
        pNewNode.uiParam = uiParam;
        pNewNode.ubEventType = ubEventType;
        pNewNode.uiTimeStamp = uiTimeStamp;
        pNewNode.uiTimeOffset = 0;

        // Search list for a place to insert
        pNode = gpEventList;

        // If it's the first head, do this!
        if (pNode is null)
        {
            gpEventList = pNewNode;
            pNewNode.next = null;
        }
        else
        {
            pPrevNode = null;
            while (pNode is not null)
            {
                if (uiTimeStamp < pNode.uiTimeStamp)
                {
                    break;
                }
                pPrevNode = pNode;
                pNode = pNode.next;
            }

            // If we are at the end, set at the end!
            if (pNode is null)
            {
                pPrevNode.next = pNewNode;
                pNewNode.next = null;
            }
            else
            {
                // We have a previous node here
                // Insert IN FRONT!
                if (pPrevNode is not null)
                {
                    pNewNode.next = pPrevNode.next;
                    pPrevNode.next = pNewNode;
                }
                else
                {   // It's the head
                    pNewNode.next = gpEventList;
                    gpEventList = pNewNode;
                }
            }
        }

        return pNewNode;
    }

    public static bool AddStrategicEvent(EVENT ubCallbackID, uint uiMinStamp, object uiParam)
    {
        if (AddAdvancedStrategicEvent(EVENTPERIOD.ONETIME_EVENT, ubCallbackID, uiMinStamp * 60, uiParam) is not null)
        {
            return true;
        }

        return false;
    }

    public static bool AddStrategicEventUsingSeconds(EVENT ubCallbackID, uint uiSecondStamp, int uiParam)
    {
        if (AddAdvancedStrategicEvent(EVENTPERIOD.ONETIME_EVENT, ubCallbackID, uiSecondStamp, uiParam) is not null)
        {
            return true;
        }

        return false;
    }


    public static bool AddRangedStrategicEvent(EVENT ubCallbackID, uint uiStartMin, uint uiLengthMin, int uiParam)
    {
        STRATEGICEVENT? pEvent;
        pEvent = AddAdvancedStrategicEvent(EVENTPERIOD.RANGED_EVENT, ubCallbackID, uiStartMin * 60, uiParam);
        if (pEvent is not null)
        {
            pEvent.uiTimeOffset = uiLengthMin * 60;
            return true;
        }
        return false;
    }

    public static bool AddSameDayRangedStrategicEvent(EVENT ubCallbackID, uint uiStartMin, uint uiLengthMin, int uiParam)
    {
        return AddRangedStrategicEvent(ubCallbackID, uiStartMin + GameClock.GetWorldDayInMinutes(), uiLengthMin, uiParam);
    }

    public static bool AddFutureDayRangedStrategicEvent(EVENT ubCallbackID, uint uiStartMin, uint uiLengthMin, int uiParam, uint uiNumDaysFromPresent)
    {
        return AddRangedStrategicEvent(ubCallbackID, uiStartMin + GameClock.GetFutureDayInMinutes(GameClock.GetWorldDay() + uiNumDaysFromPresent), uiLengthMin, uiParam);
    }

    public static bool AddRangedStrategicEventUsingSeconds(EVENT ubCallbackID, uint uiStartSeconds, uint uiLengthSeconds, int uiParam)
    {
        STRATEGICEVENT? pEvent;
        pEvent = AddAdvancedStrategicEvent(EVENTPERIOD.RANGED_EVENT, ubCallbackID, uiStartSeconds, uiParam);
        if (pEvent is not null)
        {
            pEvent.uiTimeOffset = uiLengthSeconds;
            return true;
        }
        return false;
    }

    public static bool AddSameDayRangedStrategicEventUsingSeconds(EVENT ubCallbackID, uint uiStartSeconds, uint uiLengthSeconds, int uiParam)
    {
        return AddRangedStrategicEventUsingSeconds(ubCallbackID, uiStartSeconds + GameClock.GetWorldDayInSeconds(), uiLengthSeconds, uiParam);
    }

    public static bool AddFutureDayRangedStrategicEventUsingSeconds(EVENT ubCallbackID, uint uiStartSeconds, uint uiLengthSeconds, int uiParam, uint uiNumDaysFromPresent)
    {
        return AddRangedStrategicEventUsingSeconds(ubCallbackID, uiStartSeconds + GameClock.GetFutureDayInMinutes(GameClock.GetWorldDay() + uiNumDaysFromPresent) * 60, uiLengthSeconds, uiParam);
    }

    public static bool AddEveryDayStrategicEvent(EVENT ubCallbackID, uint uiStartMin, object uiParam)
    {
        if (AddAdvancedStrategicEvent(EVENTPERIOD.EVERYDAY_EVENT, ubCallbackID, GameClock.GetWorldDayInSeconds() + uiStartMin * 60, uiParam) is not null)
        {
            return true;
        }

        return false;
    }

    public static bool AddEveryDayStrategicEventUsingSeconds(EVENT ubCallbackID, uint uiStartSeconds, int uiParam)
    {
        if (AddAdvancedStrategicEvent(EVENTPERIOD.EVERYDAY_EVENT, ubCallbackID, GameClock.GetWorldDayInSeconds() + uiStartSeconds, uiParam) is not null)
        {
            return true;
        }

        return false;
    }

    //NEW:  Period Events
    //Event will get processed automatically once every X minutes.
    public static bool AddPeriodStrategicEvent(EVENT ubCallbackID, uint uiOnceEveryXMinutes, int uiParam)
    {
        STRATEGICEVENT? pEvent;
        pEvent = AddAdvancedStrategicEvent(EVENTPERIOD.PERIODIC_EVENT, ubCallbackID, GameClock.GetWorldDayInSeconds() + uiOnceEveryXMinutes * 60, uiParam);
        if (pEvent is not null)
        {
            pEvent.uiTimeOffset = uiOnceEveryXMinutes * 60;
            return true;
        }
        return false;
    }

    public static bool AddPeriodStrategicEventUsingSeconds(EVENT ubCallbackID, uint uiOnceEveryXSeconds, int uiParam)
    {
        STRATEGICEVENT? pEvent;
        pEvent = AddAdvancedStrategicEvent(EVENTPERIOD.PERIODIC_EVENT, ubCallbackID, GameClock.GetWorldDayInSeconds() + uiOnceEveryXSeconds, uiParam);
        if (pEvent is not null)
        {
            pEvent.uiTimeOffset = uiOnceEveryXSeconds;
            return true;
        }
        return false;
    }

    public static bool AddPeriodStrategicEventWithOffset(EVENT ubCallbackID, uint uiOnceEveryXMinutes, uint uiOffsetFromCurrent, int uiParam)
    {
        STRATEGICEVENT? pEvent;
        pEvent = AddAdvancedStrategicEvent(EVENTPERIOD.PERIODIC_EVENT, ubCallbackID, GameClock.GetWorldDayInSeconds() + uiOffsetFromCurrent * 60, uiParam);
        if (pEvent is not null)
        {
            pEvent.uiTimeOffset = uiOnceEveryXMinutes * 60;
            return true;
        }
        return false;
    }

    public static bool AddPeriodStrategicEventUsingSecondsWithOffset(EVENT ubCallbackID, uint uiOnceEveryXSeconds, uint uiOffsetFromCurrent, int uiParam)
    {
        STRATEGICEVENT? pEvent;
        pEvent = AddAdvancedStrategicEvent(EVENTPERIOD.PERIODIC_EVENT, ubCallbackID, GameClock.GetWorldDayInSeconds() + uiOffsetFromCurrent, uiParam);
        if (pEvent is not null)
        {
            pEvent.uiTimeOffset = uiOnceEveryXSeconds;
            return true;
        }
        return false;
    }

    public static void DeleteAllStrategicEventsOfType(EVENT ubCallbackID)
    {
        STRATEGICEVENT? curr, prev, temp;
        prev = null;
        curr = gpEventList;
        while (curr is not null)
        {
            if (curr.ubCallbackID == ubCallbackID && !curr.ubFlags.HasFlag(SEF.DELETION_PENDING))
            {
                if (gfPreventDeletionOfAnyEvent)
                {
                    curr.ubFlags |= SEF.DELETION_PENDING;
                    gfEventDeletionPending = true;
                    prev = curr;
                    curr = curr.next;
                    continue;
                }
                //Detach the node
                if (prev is not null)
                {
                    prev.next = curr.next;
                }
                else
                {
                    gpEventList = curr.next;
                }

                //isolate and remove curr
                temp = curr;
                curr = curr.next;
                temp = null;
                //ValidateGameEvents();
            }
            else
            {   //Advance all the nodes
                prev = curr;
                curr = curr.next;
            }
        }
    }

    public static void DeleteAllStrategicEvents()
    {
        STRATEGICEVENT? temp;
        while (gpEventList is not null)
        {
            temp = gpEventList;
            gpEventList = gpEventList.next;
            //ValidateGameEvents();
            temp = null;
        }
        gpEventList = null;
    }

    //Searches for and removes the first event matching the supplied information.  There may very well be a need
    //for more specific event removal, so let me know (Kris), of any support needs.  Function returns false if
    //no events were found or if the event wasn't deleted due to delete lock,
    public static bool DeleteStrategicEvent(EVENT ubCallbackID, object uiParam)
    {
        STRATEGICEVENT? curr, prev;
        curr = gpEventList;
        prev = null;
        while (curr is not null)
        { //deleting middle
            if (curr.ubCallbackID == ubCallbackID && curr.uiParam == uiParam)
            {
                if (!curr.ubFlags.HasFlag(SEF.DELETION_PENDING))
                {
                    if (gfPreventDeletionOfAnyEvent)
                    {
                        curr.ubFlags |= SEF.DELETION_PENDING;
                        gfEventDeletionPending = true;
                        return false;
                    }

                    if (prev is not null)
                    {
                        prev.next = curr.next;
                    }
                    else
                    {
                        gpEventList = gpEventList.next;
                    }

                    curr = null;
                    //ValidateGameEvents();
                    return true;
                }
            }
            prev = curr;
            curr = curr.next;
        }
        return false;
    }



    //part of the game.sav files (not map files)
    public static bool SaveStrategicEventsToSavedGame(Stream hFile)
    {
        int uiNumBytesWritten = 0;
        STRATEGICEVENT sGameEvent;

        int uiNumGameEvents = 0;
        STRATEGICEVENT? pTempEvent = gpEventList;

        //Go through the list and determine the number of events
        while (pTempEvent is not null)
        {
            pTempEvent = pTempEvent.next;
            uiNumGameEvents++;
        }


        //write the number of strategic events
        //FileWrite(hFile, out uiNumGameEvents, sizeof(int), out uiNumBytesWritten);
        if (uiNumBytesWritten != sizeof(int))
        {
            return false;
        }


        //loop through all the events and save them.
        pTempEvent = gpEventList;
        while (pTempEvent is not null)
        {
            //save the current structure
            sGameEvent = pTempEvent;

            //write the current strategic event
            //FileWrite(hFile, &sGameEvent, sizeof(STRATEGICEVENT), out uiNumBytesWritten);
            if (uiNumBytesWritten != 0)//sizeof(STRATEGICEVENT))
            {
                return false;
            }

            pTempEvent = pTempEvent.next;
        }


        return true;
    }


    public static bool LoadStrategicEventsFromSavedGame(Stream hFile)
    {
        int uiNumGameEvents = 10;
        STRATEGICEVENT? sGameEvent = null;
        int cnt;
        int uiNumBytesRead = 0;
        STRATEGICEVENT? pTemp = null;


        //erase the old Game Event queue
        DeleteAllStrategicEvents();


        //Read the number of strategic events
        //FileRead(hFile, &uiNumGameEvents, sizeof(int), &uiNumBytesRead);
        if (uiNumBytesRead != sizeof(int))
        {
            return false;
        }


        pTemp = null;

        //loop through all the events and save them.
        for (cnt = 0; cnt < uiNumGameEvents; cnt++)
        {
            STRATEGICEVENT? pTempEvent = null;

            // allocate memory for the event
            pTempEvent = new STRATEGICEVENT();//MemAlloc(sizeof(STRATEGICEVENT));

            //Read the current strategic event
            //FileRead(hFile, &sGameEvent, sizeof(STRATEGICEVENT), &uiNumBytesRead);
            if (uiNumBytesRead != 10)//sizeof(STRATEGICEVENT))
            {
                return false;
            }

            pTempEvent = sGameEvent;

            // Add the new node to the list

            //if its the first node, 
            if (cnt == 0)
            {
                // assign it as the head node
                gpEventList = pTempEvent;

                //assign the 'current node' to the head node
                pTemp = gpEventList;
            }
            else
            {
                // add the new node to the next field of the current node
                pTemp.next = pTempEvent;

                //advance the current node to the next node
                pTemp = pTemp.next;
            }

            // null out the next field ( cause there is no next field yet )
            pTempEvent.next = null;
        }

        return true;
    }

    public static void LockStrategicEventFromDeletion(STRATEGICEVENT? pEvent)
    {
        pEvent.ubFlags |= SEF.PREVENT_DELETION;
    }

    public static void UnlockStrategicEventFromDeletion(STRATEGICEVENT? pEvent)
    {
        pEvent.ubFlags &= ~SEF.PREVENT_DELETION;
    }

    public static void ValidateGameEvents()
    {
        STRATEGICEVENT? curr;
        curr = gpEventList;
        while (curr is not null)
        {
            curr = curr.next;
            if (curr is null)
            {
                return;
            }
        }
    }
}

public class STRATEGICEVENT
{
    public STRATEGICEVENT? next;
    public uint uiTimeStamp;
    public object uiParam;
    public uint uiTimeOffset;
    public EVENTPERIOD ubEventType;
    public EVENT ubCallbackID;
    public SEF ubFlags;
    public int[] bPadding = new int[6];
}

public enum EVENTPERIOD
{
    ONETIME_EVENT,
    RANGED_EVENT,
    ENDRANGED_EVENT,
    EVERYDAY_EVENT,
    PERIODIC_EVENT,
    QUEUED_EVENT
};


[Flags]
public enum SEF
{
    PREVENT_DELETION = 0x01,
    DELETION_PENDING = 0x02,
}

public enum EVENT
{
    CHANGELIGHTVAL = 1,
    WEATHERSTART,
    WEATHEREND,
    CHECKFORQUESTS,
    AMBIENT,
    AIM_RESET_MERC_ANNOYANCE,
    BOBBYRAY_PURCHASE,
    DAILY_UPDATE_BOBBY_RAY_INVENTORY,
    UPDATE_BOBBY_RAY_INVENTORY,
    DAILY_UPDATE_OF_MERC_SITE,
    DAY3_ADD_EMAIL_FROM_SPECK,
    DELAYED_HIRING_OF_MERC,
    HANDLE_INSURED_MERCS,
    PAY_LIFE_INSURANCE_FOR_DEAD_MERC,
    MERC_DAILY_UPDATE,
    MERC_ABOUT_TO_LEAVE_COMMENT,
    MERC_CONTRACT_OVER,
    GROUP_ARRIVAL,
    DAY2_ADD_EMAIL_FROM_IMP,
    MERC_COMPLAIN_EQUIPMENT,
    HOURLY_UPDATE,
    HANDLE_MINE_INCOME,
    SETUP_MINE_INCOME,
    QUEUED_BATTLE,
    LEAVING_MERC_ARRIVE_IN_DRASSEN,   // unused
    LEAVING_MERC_ARRIVE_IN_OMERTA,    // unused
    SET_BY_NPC_SYSTEM,
    SECOND_AIRPORT_ATTENDANT_ARRIVED,
    HELICOPTER_HOVER_TOO_LONG,
    HELICOPTER_HOVER_WAY_TOO_LONG,
    HELICOPTER_DONE_REFUELING,
    MERC_LEAVE_EQUIP_IN_OMERTA,
    MERC_LEAVE_EQUIP_IN_DRASSEN,
    DAILY_EARLY_MORNING_EVENTS,
    GROUP_ABOUT_TO_ARRIVE,
    PROCESS_TACTICAL_SCHEDULE,
    BEGINRAINSTORM,
    ENDRAINSTORM,
    HANDLE_TOWN_OPINION,
    SETUP_TOWN_OPINION,
    MAKE_CIV_GROUP_HOSTILE_ON_NEXT_SECTOR_ENTRANCE,
    BEGIN_AIR_RAID,
    TOWN_LOYALTY_UPDATE,      /* Delayed loyalty effects elimininated.  Sep.12/98.  ARM */
    MEANWHILE,
    BEGIN_CREATURE_QUEST,
    CREATURE_SPREAD,
    DECAY_CREATURES,
    CREATURE_NIGHT_PLANNING,
    CREATURE_ATTACK,
    EVALUATE_QUEEN_SITUATION,
    CHECK_ENEMY_CONTROLLED_SECTOR,
    TURN_ON_NIGHT_LIGHTS,
    TURN_OFF_NIGHT_LIGHTS,
    TURN_ON_PRIME_LIGHTS,
    TURN_OFF_PRIME_LIGHTS,
    MERC_ABOUT_TO_LEAVE,
    INTERRUPT_TIME,
    ENRICO_MAIL,
    INSURANCE_INVESTIGATION_STARTED,
    INSURANCE_INVESTIGATION_OVER,
    MINUTE_UPDATE,
    TEMPERATURE_UPDATE,
    KEITH_GOING_OUT_OF_BUSINESS,
    MERC_SITE_BACK_ONLINE,
    INVESTIGATE_SECTOR,
    CHECK_IF_MINE_CLEARED,
    REMOVE_ASSASSIN,
    BANDAGE_BLEEDING_MERCS,
    SHOW_UPDATE_MENU,
    SET_MENU_REASON,
    ADDSOLDIER_TO_UPDATE_BOX,
    BEGIN_CONTRACT_RENEWAL_SEQUENCE,
    RPC_WHINE_ABOUT_PAY,
    HAVENT_MADE_IMP_CHARACTER_EMAIL,
    RAINSTORM,
    QUARTER_HOUR_UPDATE,
    MERC_MERC_WENT_UP_LEVEL_EMAIL_DELAY,
    MERC_SITE_NEW_MERC_AVAILABLE,

    /*
	HEY, YOU GUYS AREN'T DOING THIS!!!!!!  (see below)



	!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
	!! IMPORTANT NOTE                                                                         !!
	!! FOR ALL NEW EVENTS:  For text debug support, make sure you add the text version of the !!
	!! new event into the gEventName[] at the top of Game Events.c.                           !!
	!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
	*/

    NUMBER_OF_EVENT_TYPES_PLUS_ONE,
    NUMBER_OF_EVENT_TYPES = NUMBER_OF_EVENT_TYPES_PLUS_ONE - 1
};
