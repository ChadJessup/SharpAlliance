using System;
using static SharpAlliance.Core.Globals;
namespace SharpAlliance.Core.SubSystems;

public class GameEventHook
{
    public static bool DelayEventIfBattleInProgress(STRATEGICEVENT? pEvent)
    {
        STRATEGICEVENT? pNewEvent;
        if (gTacticalStatus.fEnemyInSector)
        {
            pNewEvent = GameEvents.AddAdvancedStrategicEvent(pEvent.ubEventType, pEvent.ubCallbackID, pEvent.uiTimeStamp + 180 + (uint)Globals.Random.Next(121), pEvent.uiParam);
            ArgumentNullException.ThrowIfNull(pNewEvent);
            pNewEvent.uiTimeOffset = pEvent.uiTimeOffset;
            return true;
        }
        return false;
    }

    public static bool ExecuteStrategicEvent(STRATEGICEVENT pEvent)
    {
        bool fOrigPreventFlag;

        fOrigPreventFlag = gfPreventDeletionOfAnyEvent;
        gfPreventDeletionOfAnyEvent = true;
        //No events can be posted before this time when gfProcessingGameEvents is set, otherwise,
        //we have a chance of running into an infinite loop.
        guiTimeStampOfCurrentlyExecutingEvent = pEvent.uiTimeStamp;

        if (pEvent.ubFlags.HasFlag(SEF.DELETION_PENDING))
        {
            gfPreventDeletionOfAnyEvent = fOrigPreventFlag;
            return false;
        }

        // Look at the ID of event and do stuff according to that!
        switch (pEvent.ubCallbackID)
        {
            case EVENT.CHANGELIGHTVAL:
                // Change light to value
                gubEnvLightValue = (int)pEvent.uiParam;
                if (!gfBasement && !gfCaves)
                {
                    gfDoLighting = true;
                }

                break;
            case EVENT.CHECKFORQUESTS:
                Quests.CheckForQuests(GameClock.GetWorldDay());
                break;
            case EVENT.AMBIENT:
                if (pEvent.ubEventType == EVENTPERIOD.ENDRANGED_EVENT)
                {
//                    if (pEvent.uiParam != NO_SAMPLE)
                    {
//                        SoundRemoveSampleFlags(pEvent.uiParam, SAMPLE_RANDOM);
                    }
                }
                else
                {
//                    pEvent.uiParam = SetupNewAmbientSound(pEvent.uiParam);
                }
                break;
            case EVENT.AIM_RESET_MERC_ANNOYANCE:
//                ResetMercAnnoyanceAtPlayer((int)pEvent.uiParam);
                break;
            //The players purchase from Bobby Ray has arrived
            case EVENT.BOBBYRAY_PURCHASE:
//                BobbyRayPurchaseEventCallback((int)pEvent.uiParam);
                break;
            //Gets called once a day ( at BOBBYRAY_UPDATE_TIME).  To simulate the items being bought and sold at bobby rays 
            case EVENT.DAILY_UPDATE_BOBBY_RAY_INVENTORY:
//                DailyUpdateOfBobbyRaysNewInventory();
//                DailyUpdateOfBobbyRaysUsedInventory();
//                DailyUpdateOfArmsDealersInventory();
                break;
            //Add items to BobbyR's new/used inventory
            case EVENT.UPDATE_BOBBY_RAY_INVENTORY:
//                AddFreshBobbyRayInventory((int)pEvent.uiParam);
                break;
            //Called once a day to update the number of days that a hired merc from M.E.R.C. has been on contract.
            // Also if the player hasn't paid for a while Specks will start sending e-mails to the player
            case EVENT.DAILY_UPDATE_OF_MERC_SITE:
//                DailyUpdateOfMercSite(GameClock.GetWorldDay());
                break;
            case EVENT.DAY3_ADD_EMAIL_FROM_SPECK:
                Emails.AddEmail(MERC_INTRO, MERC_INTRO_LENGTH, EmailAddresses.SPECK_FROM_MERC, GameClock.GetWorldTotalMin());
                break;
            case EVENT.DAY2_ADD_EMAIL_FROM_IMP:
                Emails.AddEmail(IMP_EMAIL_PROFILE_RESULTS, IMP_EMAIL_PROFILE_RESULTS_LENGTH, EmailAddresses.IMP_PROFILE_RESULTS, GameClock.GetWorldTotalMin());
                break;
            //If a merc gets hired and they dont show up immediately, the merc gets added to the queue and shows up
            // uiTimeTillMercArrives  minutes later
            case EVENT.DELAYED_HIRING_OF_MERC:
//                MercArrivesCallback((int)pEvent.uiParam);
                break;
            //handles the life insurance contract for a merc from AIM.
            case EVENT.HANDLE_INSURED_MERCS:
//                DailyUpdateOfInsuredMercs();
                break;
            //handles when a merc is killed an there is a life insurance payout
            case EVENT.PAY_LIFE_INSURANCE_FOR_DEAD_MERC:
//                InsuranceContractPayLifeInsuranceForDeadMerc((int)pEvent.uiParam);
                break;
            //gets called every day at midnight.
            case EVENT.MERC_DAILY_UPDATE:
//                MercDailyUpdate();
                break;
            //gets when a merc is about to leave.
            case EVENT.MERC_ABOUT_TO_LEAVE_COMMENT:
                break;
            // show the update menu
            case EVENT.SHOW_UPDATE_MENU:
//                AddDisplayBoxToWaitingQueue();
                break;
            case EVENT.MERC_ABOUT_TO_LEAVE:
//                FindOutIfAnyMercAboutToLeaveIsGonnaRenew();
                break;
            //When a merc is supposed to leave
            case EVENT.MERC_CONTRACT_OVER:
//                MercsContractIsFinished((int)pEvent.uiParam);
                break;
            case EVENT.ADDSOLDIER_TO_UPDATE_BOX:
                // if the grunt is currently active, add to update box
                if (Menptr[(int)pEvent.uiParam].bActive)
                {
//                    AddSoldierToWaitingListQueue((Menptr[(int)pEvent.uiParam]));
                }
                break;
            case EVENT.SET_MENU_REASON:
//                AddReasonToWaitingListQueue((int)pEvent.uiParam);
                break;
            //Whenever any group (player or enemy) arrives in a new sector during movement.
            case EVENT.GROUP_ARRIVAL:
                //ValidateGameEvents();
//                GroupArrivedAtSector((int)pEvent.uiParam, true, false);
                //ValidateGameEvents();
                break;
            case EVENT.MERC_COMPLAIN_EQUIPMENT:
//                MercComplainAboutEquipment((int)pEvent.uiParam);
                break;
            case EVENT.HOURLY_UPDATE:
//                HandleHourlyUpdate();
                break;
            case EVENT.MINUTE_UPDATE:
//                HandleMinuteUpdate();
                break;
            case EVENT.HANDLE_MINE_INCOME:
//                HandleIncomeFromMines();
                //ScreenMsg( FONT_MCOLOR_DKRED, MSG.INTERFACE, "Income From Mines at %d", GetWorldTotalMin( ) );
                break;
            case EVENT.SETUP_MINE_INCOME:
//                PostEventsForMineProduction();
                break;
            case EVENT.SETUP_TOWN_OPINION:
//                PostEventsForSpreadOfTownOpinion();
                break;
            case EVENT.HANDLE_TOWN_OPINION:
//                HandleSpreadOfAllTownsOpinion();
                break;
            case EVENT.SET_BY_NPC_SYSTEM:
//                HandleNPCSystemEvent(pEvent.uiParam);
                break;
            case EVENT.SECOND_AIRPORT_ATTENDANT_ARRIVED:
//                AddSecondAirportAttendant();
                break;
            case EVENT.HELICOPTER_HOVER_TOO_LONG:
//                HandleHeliHoverLong();
                break;
            case EVENT.HELICOPTER_HOVER_WAY_TOO_LONG:
//                HandleHeliHoverTooLong();
                break;
            case EVENT.MERC_LEAVE_EQUIP_IN_DRASSEN:
//                HandleEquipmentLeftInDrassen(pEvent.uiParam);
                break;
            case EVENT.MERC_LEAVE_EQUIP_IN_OMERTA:
//                HandleEquipmentLeftInOmerta(pEvent.uiParam);
                break;
            case EVENT.BANDAGE_BLEEDING_MERCS:
//                BandageBleedingDyingPatientsBeingTreated();
                break;
            case EVENT.DAILY_EARLY_MORNING_EVENTS:
//                HandleEarlyMorningEvents();
                break;
            case EVENT.GROUP_ABOUT_TO_ARRIVE:
//                HandleGroupAboutToArrive();
                break;
            case EVENT.PROCESS_TACTICAL_SCHEDULE:
//                ProcessTacticalSchedule((int)pEvent.uiParam);
                break;
            case EVENT.BEGINRAINSTORM:
                //EnvBeginRainStorm( (int)pEvent.uiParam );
                break;
            case EVENT.ENDRAINSTORM:
                //EnvEndRainStorm( );
                break;
            case EVENT.RAINSTORM:

                // ATE: Disabled
                //
                //if( pEvent.ubEventType == ENDRANGED_EVENT )
                //{
                //	EnvEndRainStorm( );
                //}
                //else
                //{
                //	EnvBeginRainStorm( (int)pEvent.uiParam );
                //}
                break;

            case EVENT.MAKE_CIV_GROUP_HOSTILE_ON_NEXT_SECTOR_ENTRANCE:
//                MakeCivGroupHostileOnNextSectorEntrance((int)pEvent.uiParam);
                break;
            case EVENT.BEGIN_AIR_RAID:
//                BeginAirRaid();
                break;
            case EVENT.MEANWHILE:
                if (!DelayEventIfBattleInProgress(pEvent))
                {
                    Meanwhile.BeginMeanwhile((int)pEvent.uiParam);
                    GameClock.InterruptTime();
                }
                break;
            case EVENT.BEGIN_CREATURE_QUEST:
                break;
            case EVENT.CREATURE_SPREAD:
//                SpreadCreatures();
                break;
            case EVENT.DECAY_CREATURES:
//                DecayCreatures();
                break;
            case EVENT.CREATURE_NIGHT_PLANNING:
//                CreatureNightPlanning();
                break;
            case EVENT.CREATURE_ATTACK:
//                CreatureAttackTown((int)pEvent.uiParam, false);
                break;
            case EVENT.EVALUATE_QUEEN_SITUATION:
                StrategicAI.EvaluateQueenSituation();
                break;
            case EVENT.CHECK_ENEMY_CONTROLLED_SECTOR:
//                CheckEnemyControlledSector((int)pEvent.uiParam);
                break;
            case EVENT.TURN_ON_NIGHT_LIGHTS:
//                TurnOnNightLights();
                break;
            case EVENT.TURN_OFF_NIGHT_LIGHTS:
//                TurnOffNightLights();
                break;
            case EVENT.TURN_ON_PRIME_LIGHTS:
//                TurnOnPrimeLights();
                break;
            case EVENT.TURN_OFF_PRIME_LIGHTS:
//                TurnOffPrimeLights();
                break;
            case EVENT.INTERRUPT_TIME:
                GameClock.InterruptTime();
                break;
            case EVENT.ENRICO_MAIL:
//                HandleEnricoEmail();
                break;
            case EVENT.INSURANCE_INVESTIGATION_STARTED:
//                StartInsuranceInvestigation((int)pEvent.uiParam);
                break;
            case EVENT.INSURANCE_INVESTIGATION_OVER:
//                EndInsuranceInvestigation((int)pEvent.uiParam);
                break;
            case EVENT.TEMPERATURE_UPDATE:
//                UpdateTemperature((int)pEvent.uiParam);
                break;
            case EVENT.KEITH_GOING_OUT_OF_BUSINESS:
                // make sure killbillies are still alive, if so, set fact 274 true
                if (Facts.CheckFact(FACT.HILLBILLIES_KILLED, NPCID.KEITH) == false)
                {
                    //s et the fact true keith is out of business
                    Facts.SetFactTrue(FACT.KEITH_OUT_OF_BUSINESS);
                }
                break;
            case EVENT.MERC_SITE_BACK_ONLINE:
//                GetMercSiteBackOnline();
                break;
            case EVENT.INVESTIGATE_SECTOR:
//                InvestigateSector((int)pEvent.uiParam);
                break;
            case EVENT.CHECK_IF_MINE_CLEARED:
                // If so, the head miner will say so, and the mine's shutdown will be ended.
//                HourlyMinesUpdate();        // not-so hourly, in this case!
                break;
            case EVENT.REMOVE_ASSASSIN:
//                RemoveAssassin((int)pEvent.uiParam);
                break;
            case EVENT.BEGIN_CONTRACT_RENEWAL_SEQUENCE:
//                BeginContractRenewalSequence();
                break;
            case EVENT.RPC_WHINE_ABOUT_PAY:
//                RPCWhineAboutNoPay((int)pEvent.uiParam);
                break;

            case EVENT.HAVENT_MADE_IMP_CHARACTER_EMAIL:
//                HaventMadeImpMercEmailCallBack();
                break;

            case EVENT.QUARTER_HOUR_UPDATE:
//                HandleQuarterHourUpdate();
                break;

            case EVENT.MERC_MERC_WENT_UP_LEVEL_EMAIL_DELAY:
//                MERCMercWentUpALevelSendEmail((int)pEvent.uiParam);
                break;

            case EVENT.MERC_SITE_NEW_MERC_AVAILABLE:
//                NewMercsAvailableAtMercSiteCallBack();
                break;
        }

        gfPreventDeletionOfAnyEvent = fOrigPreventFlag;
        return true;
    }
}
