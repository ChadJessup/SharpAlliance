using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core.SubSystems;

public class MercHiring
{
    public const int MERC_HIRE_OVER_20_MERCS_HIRED = -1;
    public const int MERC_HIRE_FAILED = 0;
    public const int MERC_HIRE_OK = 1;

    private const int MIN_FLIGHT_PREP_TIME = 6;
    private const int MAX_MERCS_ON_TEAM = 20;

    /// <summary>When the next flight carrying a newly hired merc lands, in minutes of game time.</summary>
    public static uint GetMercArrivalTimeOfDay()
    {
        // First get the current time of day.....
        uint uiCurrHour = GameClock.GetWorldHour();

        // Subtract the min time for any arrival....
        uint uiMinHour = uiCurrHour + MIN_FLIGHT_PREP_TIME;

        // See if we have missed the last flight for the day...
        if (uiCurrHour > 13)
        {
            return GameClock.GetMidnightOfFutureDayInMinutes(1) + (uint)MERC_ARRIVE_TIME_SLOT_1;
        }

        // Find next possible flight
        if (uiMinHour <= 7)
        {
            return GameClock.GetWorldDayInMinutes() + (uint)MERC_ARRIVE_TIME_SLOT_1;
        }
        else if (uiMinHour <= 13)
        {
            return GameClock.GetWorldDayInMinutes() + (uint)MERC_ARRIVE_TIME_SLOT_2;
        }

        return GameClock.GetWorldDayInMinutes() + (uint)MERC_ARRIVE_TIME_SLOT_3;
    }

    /// <summary>How many mercs the player currently employs.</summary>
    public static int NumberOfMercsOnPlayerTeam()
    {
        int count = 0;
        for (int i = gTacticalStatus.Team[gbPlayerNum].bFirstID; i <= gTacticalStatus.Team[gbPlayerNum].bLastID; i++)
        {
            if (MercPtrs[i].bActive)
            {
                count++;
            }
        }

        return count;
    }

    /// <summary>The hour of day a merc's contract ends, which is the hour they arrived.</summary>
    public static uint GetHourWhenContractDone(SOLDIERTYPE pSoldier)
    {
        uint uiArrive = (uint)pSoldier.uiTimeSoldierWillArrive;
        return (uiArrive - ((uiArrive / 1440) * 1440)) / 60;
    }

    /// <summary>
    /// Hires a merc (HireMerc in Merc Hiring.c). TODO(port): the free-flight arrival (MercArrivesCallback) and the
    /// chopper insertion are not ported, so the soldier stays in transit; the starting letter (AutoPlaceObject) and
    /// ChangeSoldiersAssignment / SetTimeOfAssignmentChangeForMerc are not done either.
    /// </summary>
    public static int HireMerc(MERC_HIRE_STRUCT pHireMerc)
    {
        NPCID ubCurrentSoldier = pHireMerc.ubProfileID;
        var pMerc = gMercProfiles[ubCurrentSoldier];

        //If the merc is away, Dont hire him, or if the merc is only slightly annoyed at the player
        if (pMerc.bMercStatus != MercStatus.MERC_OK
            && pMerc.bMercStatus != MercStatus.MERC_ANNOYED_BUT_CAN_STILL_CONTACT
            && pMerc.bMercStatus != MercStatus.MERC_HIRED_BUT_NOT_ARRIVED_YET)
        {
            return MERC_HIRE_FAILED;
        }

        if (NumberOfMercsOnPlayerTeam() >= 18)
        {
            return MERC_HIRE_OVER_20_MERCS_HIRED;
        }

        // ATE: if we are to use landing zone, update to latest value
        if (pHireMerc.fUseLandingZoneForArrival)
        {
            pHireMerc.sSectorX = gsMercArriveSectorX;
            pHireMerc.sSectorY = gsMercArriveSectorY;
            pHireMerc.bSectorZ = 0;
        }

        // BUILD STRUCTURES
        SOLDIERCREATE_STRUCT MercCreateStruct = new()
        {
            ubProfile = ubCurrentSoldier,
            fPlayerMerc = true,
            sSectorX = pHireMerc.sSectorX,
            sSectorY = pHireMerc.sSectorY,
            bSectorZ = pHireMerc.bSectorZ,
            bTeam = (TEAM)SOLDIER_CREATE_AUTO_TEAM,
            fCopyProfileItemsOver = pHireMerc.fCopyProfileItemsOver,
        };

        if (!SoldierCreate.TacticalCreateSoldier(MercCreateStruct, out int iNewIndex))
        {
            return MERC_HIRE_FAILED;
        }

        bool fGameJustStarted = gTacticalStatus.fDidGameJustStart;
        if (fGameJustStarted)
        {
            // Set insertion for first time in chopper
            pHireMerc.ubInsertionCode = INSERTION_CODE.CHOPPER;
        }

        //record how long the merc will be gone for
        pMerc.bMercStatus = (MercStatus)pHireMerc.iTotalContractLength;

        SOLDIERTYPE pSoldier = Menptr[iNewIndex];

        //Copy over insertion data....
        pSoldier.ubStrategicInsertionCode = pHireMerc.ubInsertionCode;
        pSoldier.usStrategicInsertionData = pHireMerc.usInsertionData;

        // ATE: Copy over value for using alnding zone to soldier type
        pSoldier.fUseLandingZoneForArrival = pHireMerc.fUseLandingZoneForArrival;

        // Set assignment
        pSoldier.bAssignment = Assignment.IN_TRANSIT;

        //set the contract length
        pSoldier.iTotalContractLength = pHireMerc.iTotalContractLength;

        //reset the insurance values
        pSoldier.iStartOfInsuranceContract = 0;
        pSoldier.iTotalLengthOfInsuranceContract = 0;

        // store arrival time in soldier structure so map screen can display it
        pSoldier.uiTimeSoldierWillArrive = (int)pHireMerc.uiTimeTillMercArrives;

        if (fGameJustStarted)
        {
            // Set time of initial merc arrival in minutes
            pHireMerc.uiTimeTillMercArrives = (uint)((STARTING_TIME + FIRST_ARRIVAL_DELAY) / NUM_SEC_IN_MIN);

            //set when the merc's contract is finished
            pSoldier.iEndofContractTime = (int)(GameClock.GetMidnightOfFutureDayInMinutes((uint)pSoldier.iTotalContractLength) + (GetHourWhenContractDone(pSoldier) * 60));
        }
        else
        {
            //set when the merc's contract is finished ( + 1 cause it takes a day for the merc to arrive )
            pSoldier.iEndofContractTime = (int)(GameClock.GetMidnightOfFutureDayInMinutes((uint)(1 + pSoldier.iTotalContractLength)) + (GetHourWhenContractDone(pSoldier) * 60));
        }

        //Set the time and ID of the last hired merc will arrive
        LaptopSaveInfo.sLastHiredMerc.iIdOfMerc = (int)pHireMerc.ubProfileID;
        LaptopSaveInfo.sLastHiredMerc.uiArrivalTime = (int)pHireMerc.uiTimeTillMercArrives;
        LaptopSaveInfo.sLastHiredMerc.fHaveDisplayedPopUpInLaptop = false;

        //if we are trying to hire a merc that should arrive later, put the merc in the queue
        if (pHireMerc.uiTimeTillMercArrives != 0)
        {
            GameEvents.AddStrategicEvent(EVENT.DELAYED_HIRING_OF_MERC, pHireMerc.uiTimeTillMercArrives, pSoldier.ubID);

            //specify that the merc is hired but hasnt arrived yet
            pMerc.bMercStatus = MercStatus.MERC_HIRED_BUT_NOT_ARRIVED_YET;
        }

        int ubProfile = (int)ubCurrentSoldier;
        if (ubProfile < 40)
        {
            // an AIM merc: remember what type of contract he got
            pSoldier.ubWhatKindOfMercAmI = MERC_TYPE.AIM_MERC;

            if (pHireMerc.iTotalContractLength == 1)
            {
                pSoldier.bTypeOfLastContract = 0; // CONTRACT_EXTEND_1_DAY
                pSoldier.iTimeCanSignElsewhere = (int)GameClock.GetWorldTotalMin();
            }
            else if (pHireMerc.iTotalContractLength == 7)
            {
                pSoldier.bTypeOfLastContract = 1; // CONTRACT_EXTEND_1_WEEK
                pSoldier.iTimeCanSignElsewhere = (int)GameClock.GetWorldTotalMin();
            }
            else if (pHireMerc.iTotalContractLength == 14)
            {
                pSoldier.bTypeOfLastContract = 2; // CONTRACT_EXTEND_2_WEEK

                // These luck fellows need to stay the whole duration!
                pSoldier.iTimeCanSignElsewhere = pSoldier.iEndofContractTime;
            }

            // remember the medical deposit we PAID.  The one in his profile can increase when he levels!
            pSoldier.usMedicalDeposit = gMercProfiles[pSoldier.ubProfile].sMedicalDepositAmount;
        }
        else if (ubProfile >= 40 && ubProfile <= 50)
        {
            // from M.E.R.C.
            pSoldier.ubWhatKindOfMercAmI = MERC_TYPE.MERC;
            gMercProfiles[pSoldier.ubProfile].iMercMercContractLength = 1;

            //Set starting conditions for the merc
            pSoldier.iStartContractTime = (int)GameClock.GetWorldDay();
            History.AddHistoryToPlayersLog(HISTORY.HIRED_MERC_FROM_MERC, ubCurrentSoldier, GameClock.GetWorldTotalMin(), -1, (MAP_ROW)(-1));
        }
        else if (ubProfile >= 51 && ubProfile < 57)
        {
            // from IMP, ie a player character
            pSoldier.ubWhatKindOfMercAmI = MERC_TYPE.PLAYER_CHARACTER;
        }
        else
        {
            pSoldier.ubWhatKindOfMercAmI = MERC_TYPE.NPC;
        }

        gfAtLeastOneMercWasHired = true;

        return MERC_HIRE_OK;
    }

    /// <summary>
    /// Called when a hired merc's flight lands (MercArrivesCallback). TODO(port): putting the soldier into the loaded
    /// sector (UpdateMercInSector), the helicopter run, clearing enemies from A9, the arrival quotes and the
    /// "bad equipment" complaint event are not ported.
    /// </summary>
    public static void MercArrivesCallback(int ubSoldierID)
    {
        SOLDIERTYPE pSoldier = Menptr[ubSoldierID];
        if (!pSoldier.bActive)
        {
            return;
        }

        var pMerc = gMercProfiles[pSoldier.ubProfile];

        // stop time compression until player restarts it
        GameClock.StopTimeCompression();

        // add the guy to a squad
        Squads.AddCharacterToAnySquad(pSoldier);

        // ATE: Make sure we use global.....
        if (pSoldier.fUseLandingZoneForArrival)
        {
            pSoldier.sSectorX = gsMercArriveSectorX;
            pSoldier.sSectorY = gsMercArriveSectorY;
            pSoldier.bSectorZ = 0;
        }

        // the guy has landed, so he is no longer in transit
        if (pSoldier.bAssignment == Assignment.IN_TRANSIT)
        {
            pSoldier.bAssignment = Assignment.ON_DUTY;
        }

        if (pSoldier.ubStrategicInsertionCode != INSERTION_CODE.CHOPPER)
        {
            int index = (int)STR.MERC_HAS_ARRIVED_STR;
            if (index < TacticalStr.Length)
            {
                SharpAlliance.Core.Managers.Messages.ScreenMsg(FontColor.FONT_MCOLOR_WHITE, MSG.INTERFACE, TacticalStr[index], pSoldier.name);
            }
        }

        //record how long the merc will be gone for
        pMerc.bMercStatus = (MercStatus)pSoldier.iTotalContractLength;

        // remember when excatly he ARRIVED in Arulco, in case he gets fired early
        pSoldier.uiTimeOfLastContractUpdate = (int)GameClock.GetWorldTotalMin();

        //set when the merc's contract is finished
        pSoldier.iEndofContractTime = (int)(GameClock.GetMidnightOfFutureDayInMinutes((uint)pSoldier.iTotalContractLength) + (GetHourWhenContractDone(pSoldier) * 60));

        fTeamPanelDirty = true;
    }

    internal static void HandleMercArrivesQuotes(SOLDIERTYPE sOLDIERTYPE)
    {
        throw new NotImplementedException();
    }
}

public class MERC_HIRE_STRUCT
{
    public NPCID  ubProfileID;
    public int  sSectorX;
    public MAP_ROW sSectorY;
    public int bSectorZ;
    public int  iTotalContractLength;
    public bool fCopyProfileItemsOver;
    public uint uiTimeTillMercArrives;
    public INSERTION_CODE ubInsertionCode;
    public int  usInsertionData;
    public bool fUseLandingZoneForArrival;
}

