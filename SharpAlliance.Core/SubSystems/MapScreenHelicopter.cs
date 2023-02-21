namespace SharpAlliance.Core.SubSystems;

public class MapScreenHelicopter
{

    public static void CheckAndHandleSkyriderMonologues()
    {
        // wait at least this many days between Skyrider monologues
        if ((GetWorldTotalMin() - Globals.guiTimeOfLastSkyriderMonologue) >= (MIN_DAYS_BETWEEN_SKYRIDER_MONOLOGUES * 24 * 60))
        {
            if (Globals.guiHelicopterSkyriderTalkState == 0)
            {
                HandleSkyRiderMonologueEvent(SKYRIDER_MONOLOGUE_EVENT_DRASSEN_SAM_SITE, 0);
                Globals.guiHelicopterSkyriderTalkState = 1;
            }
            else if (Globals.guiHelicopterSkyriderTalkState == 1)
            {
                // if enemy still controls the Cambria hospital sector
                if (Globals.StrategicMap[CALCULATE_STRATEGIC_INDEX(HOSPITAL_SECTOR_X, HOSPITAL_SECTOR_Y)].fEnemyControlled)
                {
                    HandleSkyRiderMonologueEvent(SKYRIDER_MONOLOGUE_EVENT_CAMBRIA_HOSPITAL, 0);
                }

                // advance state even if player already has Cambria's hospital sector!!!
                Globals.guiHelicopterSkyriderTalkState = 2;
            }
            else if (Globals.guiHelicopterSkyriderTalkState == 2)
            {
                // wait until player has taken over a SAM site before saying this and advancing state
                if (Globals.gfSkyriderSaidCongratsOnTakingSAM)
                {
                    HandleSkyRiderMonologueEvent(SKYRIDER_MONOLOGUE_EVENT_OTHER_SAM_SITES, 0);
                    Globals.guiHelicopterSkyriderTalkState = 3;
                }
            }
            else if (Globals.guiHelicopterSkyriderTalkState == 3)
            {
                // wait until Estoni refuelling site becomes available
                if (Globals.fRefuelingSiteAvailable[ESTONI_REFUELING_SITE])
                {
                    HandleSkyRiderMonologueEvent(SKYRIDER_MONOLOGUE_EVENT_ESTONI_REFUEL, 0);
                    Globals.guiHelicopterSkyriderTalkState = 4;
                }
            }
        }
    }

    public static bool IsHelicopterPilotAvailable()
    {
        // what is state of skyrider?
        if (Globals.fSkyRiderAvailable == false)
        {
            return (false);
        }

        // owe any money to skyrider?
        if (Globals.gMercProfiles[NPCID.SKYRIDER].iBalance < 0)
        {
            return (false);
        }

        // Drassen too disloyal to wanna help player?
        if (Facts.CheckFact(FACT.LOYALTY_LOW, NPCID.SKYRIDER))
        {
            return (false);
        }

        return (true);
    }
}
