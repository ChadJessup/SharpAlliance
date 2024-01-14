using System;
using SharpAlliance.Core.Screens;
using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core.SubSystems;

public class MapScreenHelicopter
{
    private static SOLDIERTYPE SoldierSkyRider;
    private static bool fHelicopterIsAirBorne;
    private static bool fHeliReturnStraightToBase;
    private static bool fHoveringHelicopter;
    private static int uiStartHoverTime;
    public static bool fPlotForHelicopter;
    private static Path? pTempHelicopterPath;
    private static int iTotalAccumulatedCostByPlayer;
    private static bool fHelicopterDestroyed;
    private static bool fSkyRiderSetUp;
    private static SOLDIERTYPE? pSkyRider;
    private static bool fHelicopterAvailable;
    private static bool fShowEstoniRefuelHighLight;
    private static bool fShowOtherSAMHighLight;
    private static bool fShowDrassenSAMHighLight;
    private static bool fShowCambriaHospitalHighLight;
    private static bool gfSkyriderEmptyHelpGiven;
    private static int gubHelicopterHitsTaken;
    private static bool gfSkyriderSaidCongratsOnTakingSAM;
    private static int gubPlayerProgressSkyriderLastCommentedOn;

    // list of sector locations where SkyRider can be refueled
    private static int[,] ubRefuelList =
    {
    	{ 13, 2 },		// Drassen airport
    	{  6, 9 },		// Estoni
    };


    public static void CheckAndHandleSkyriderMonologues()
    {
        // wait at least this many days between Skyrider monologues
//        if ((GameClock.GetWorldTotalMin() - Globals.guiTimeOfLastSkyriderMonologue) >= (MIN_DAYS_BETWEEN_SKYRIDER_MONOLOGUES * 24 * 60))
//        {
//            if (Globals.guiHelicopterSkyriderTalkState == 0)
//            {
//                HandleSkyRiderMonologueEvent(SKYRIDER_MONOLOGUE_EVENT_DRASSEN_SAM_SITE, 0);
//                Globals.guiHelicopterSkyriderTalkState = 1;
//            }
//            else if (Globals.guiHelicopterSkyriderTalkState == 1)
//            {
//                // if enemy still controls the Cambria hospital sector
//                if (Globals.strategicMap[StrategicMap.CALCULATE_STRATEGIC_INDEX(HOSPITAL_SECTOR_X, HOSPITAL_SECTOR_Y)].fEnemyControlled)
//                {
//                    HandleSkyRiderMonologueEvent(SKYRIDER_MONOLOGUE_EVENT_CAMBRIA_HOSPITAL, 0);
//                }
//
//                // advance state even if player already has Cambria's hospital sector!!!
//                Globals.guiHelicopterSkyriderTalkState = 2;
//            }
//            else if (Globals.guiHelicopterSkyriderTalkState == 2)
//            {
//                // wait until player has taken over a SAM site before saying this and advancing state
//                if (Globals.gfSkyriderSaidCongratsOnTakingSAM)
//                {
//                    HandleSkyRiderMonologueEvent(SKYRIDER_MONOLOGUE_EVENT_OTHER_SAM_SITES, 0);
//                    Globals.guiHelicopterSkyriderTalkState = 3;
//                }
//            }
//            else if (Globals.guiHelicopterSkyriderTalkState == 3)
//            {
//                // wait until Estoni refuelling site becomes available
//                if (Globals.fRefuelingSiteAvailable[ESTONI_REFUELING_SITE])
//                {
//                    HandleSkyRiderMonologueEvent(SKYRIDER_MONOLOGUE_EVENT_ESTONI_REFUEL, 0);
//                    Globals.guiHelicopterSkyriderTalkState = 4;
//                }
//            }
//        }
    }

    public static bool IsHelicopterPilotAvailable()
    {
        // what is state of skyrider?
        if (Globals.fSkyRiderAvailable == false)
        {
            return false;
        }

        // owe any money to skyrider?
        if (Globals.gMercProfiles[NPCID.SKYRIDER].iBalance < 0)
        {
            return false;
        }

        // Drassen too disloyal to wanna help player?
        if (Facts.CheckFact(FACT.LOYALTY_LOW, NPCID.SKYRIDER))
        {
            return false;
        }

        return true;
    }

    internal static void InitializeHelicopter()
    {
        // must be called whenever a new game starts up!
        fHelicopterAvailable = false;
        iHelicopterVehicleId = -1;

        fSkyRiderAvailable = false;
        fSkyRiderSetUp = false;
        pSkyRider = null;
        SoldierSkyRider = new SOLDIERTYPE();

        fHelicopterIsAirBorne = false;
        fHeliReturnStraightToBase = false;

        fHoveringHelicopter = false;
        uiStartHoverTime = 0;

        fPlotForHelicopter = false;
        pTempHelicopterPath = null;

        //	iTotalHeliDistanceSinceRefuel = 0;
        iTotalAccumulatedCostByPlayer = 0;

        fHelicopterDestroyed = false;

        guiHelicopterSkyriderTalkState = 0;
        guiTimeOfLastSkyriderMonologue = 0;

        fShowEstoniRefuelHighLight = false;
        fShowOtherSAMHighLight = false;
        fShowDrassenSAMHighLight = false;
        fShowCambriaHospitalHighLight = false;

        gfSkyriderEmptyHelpGiven = false;

        gubHelicopterHitsTaken = 0;

        gfSkyriderSaidCongratsOnTakingSAM = false;
        gubPlayerProgressSkyriderLastCommentedOn = 0;
    }

    internal static void UpdateRefuelSiteAvailability()
    {
        // Generally, only Drassen is initially available for refuelling
        // Estoni must first be captured (although player may already have it when he gets Skyrider!)

        for (int iCounter = 0; iCounter < (int)REFUELING_SITE.NUMBER_OF_REFUEL_SITES; iCounter++)
        {
            // if enemy controlled sector (ground OR air, don't want to fly into enemy air territory)
            if ((strategicMap[CALCULATE_STRATEGIC_INDEX(ubRefuelList[iCounter,0], (MAP_ROW)ubRefuelList[iCounter,1])].fEnemyControlled == true)
                || (strategicMap[CALCULATE_STRATEGIC_INDEX(ubRefuelList[iCounter,0], (MAP_ROW)ubRefuelList[iCounter,1])].fEnemyAirControlled == true)
                || ((iCounter == (int)REFUELING_SITE.ESTONI_REFUELING_SITE) && (Facts.CheckFact(FACT.ESTONI_REFUELLING_POSSIBLE, 0) == false)))
            {
                // mark refueling site as unavailable
                fRefuelingSiteAvailable[iCounter] = false;
            }
            else
            {
                // mark refueling site as available
                fRefuelingSiteAvailable[iCounter] = true;

                // reactivate a grounded helicopter, if here
                if (!fHelicopterAvailable && !fHelicopterDestroyed && fSkyRiderAvailable && (iHelicopterVehicleId != -1))
                {
                    if ((pVehicleList[iHelicopterVehicleId].sSectorX == ubRefuelList[iCounter, 0]) &&
                          (pVehicleList[iHelicopterVehicleId].sSectorY == (MAP_ROW)ubRefuelList[iCounter, 1]))
                    {
                        // no longer grounded
                       MessageBoxScreen.DoScreenIndependantMessageBox(pSkyriderText[5], MSG_BOX_FLAG.OK, null);
                    }
                }
            }
        }

    }
}

// the sam site enums
public enum SAM_SITE
{
    ONE = 0,           // near Chitzena
    TWO,                   // near Drassen
    THREE,             // near Cambria
    FOUR,              // near Meduna
    NUMBER_OF_SAM_SITES,
};

public enum REFUELING_SITE
{
    DRASSEN_REFUELING_SITE = 0,
    ESTONI_REFUELING_SITE,
    NUMBER_OF_REFUEL_SITES,
};
