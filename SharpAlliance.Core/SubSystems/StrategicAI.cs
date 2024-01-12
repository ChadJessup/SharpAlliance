using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpAlliance.Core.Managers;
using SharpAlliance.Platform.Interfaces;
using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core.SubSystems;

public class StrategicAI
{
    private static IFileManager files;
    public StrategicAI(IFileManager fileManager) => files = fileManager;
    /*
    STRATEGIC AI -- UNDERLYING PHILOSOPHY
    The most fundamental part of the strategic AI which takes from reality and gives to gameplay is the manner
    the queen attempts to take her towns back.  Finances and owning mines are the most important way
    to win the game.  As the player takes more mines over, the queen will focus more on quality and defense.  In
    the beginning of the game, she will focus more on offense than mid-game or end-game.  

    REALITY
    The queen owns the entire country, and the player starts the game with a small lump of cash, enough to hire
    some mercenaries for about a week.  In that week, the queen may not notice what is going on, and the player
    would believably take over one of the towns before she could feasibly react.  As soon as her military was
    aware of the situation, she would likely proceed to send 300-400 troops to annihilate the opposition, and the 
    game would be over relatively quickly.  If the player was a prodigy, and managed to hold the town against such  
    a major assault, he would probably lose in the long run being forced into a defensive position and running out 
    of money quickly while the queen could continue to pump out the troops.  On the other hand, if the player 
    somehow managed to take over most of the mines, he would be able to casually walk over the queen eventually 
    just from the sheer income allowing him to purchase several of the best mercs.  That would have the effect of
    making the game impossibly difficult in the beginning of the game, and a joke at the end (this is very much
    like Master Of Orion II on the more difficult settings )

    GAMEPLAY
    Because we want the game to be like a normal game and make it fun, we need to make the game easy in the 
    beginning and harder at the end.  In order to accomplish this, I feel that pure income shouldn't be the factor 
    for the queen, because she would likely crucify a would-be leader in his early days.  So, in the beginning of
    the game, the forces would already be situated with the majority of forces being the administrators in the towns, 
    and army troops and elites in the more important sectors.  Restricting the queen's offensive
    abilities using a distance penalty would mean that the furthest sectors from the queen's palace would be
    much easier to defend because she would only be allowed to send x number of troops.  As you get closer to the 
    queen, she would be allowed to send larger forces to attack those towns in question.  Also, to further 
    increase the games difficulty as the campaign progresses in the player's favor, we could also increase the
    quality of the queen's troops based purely on the peek progress percentage.  This is calculated using a formula 
    that determines how well the player is doing by combining loyalty of towns owned, income generated, etc.  So, 
    in the beginning of the game, the quality is at the worst, but once you capture your first mines/towns, it 
    permanently  increase the queen's quality rating, effectively bumping up the stakes.  By the time you capture 
    four or five mines, the queen is going to focus more (but not completely) on quality defense as she prepares 
    for your final onslaught.  This quality rating will augment the experience level, equipment rating, and/or 
    attribute ratings of the queen's troops.  I would maintain a table of these enhancements based on the current 
    quality rating hooking into the difficulty all along.

    //EXPLANATION OF THE WEIGHT SYSTEM:
    The strategic AI has two types of groups:  garrisons and patrol groups.  Each of these groups contain
    information of it's needs, mainly desired population.  If the current population is greater than the
    desired population, and the group will get a negative weight assigned to it, which means that it is willing
    to give up troops to areas that need them more.  On the other hand, if a group has less than the desired population, 
    then the weight will be positive, meaning they are requesting reinforcements.

    The weight generated will range between -100 and +100.  The calculated weight is modified by the priority
    of the group.  If the priority of the group is high, they 
    */


    //The army composition defines attributes for the various garrisons.  The priority reflects how important the sector is
    //to the queen, the elite/troop percentages refer to the desired composition of the group.  The admin percentage has recently been
    //changed to reflect the starting percentage of the garrison that are administrators.  Note that elite% + troop% = 100, and the admin% is
    //not related in this effect.  If the admin% is non-zero, then that garrison is assigned only x% of the force as admins, with NO troops or elites.
    //All reinforcements use the composition of the troop/elite for refilling.
    //@@@Alex, the send reinforcement composition isn't complete.  Either sends all troops or troops based off of the composition of the source garrison.
    //  It is my intention to add this.

    //returns the number of reinforcements permitted to be sent.  Will increased if the denied counter is non-zero.
    int GarrisonReinforcementsRequested(Garrisons iGarrisonID, int? pubExtraReinforcements)
    {
        int iReinforcementsRequested;
        int iExistingForces;
        SECTORINFO? pSector;

        pSector = SectorInfo[gGarrisonGroup[(int)iGarrisonID].ubSectorID];
        iExistingForces = pSector.ubNumAdmins + pSector.ubNumTroops + pSector.ubNumElites;
        iReinforcementsRequested = gArmyComp[gGarrisonGroup[(int)iGarrisonID].ubComposition].bDesiredPopulation - iExistingForces;

        //Record how many of the reinforcements are additionally provided due to being denied in the past.  This will grow
        //until it is finally excepted or an absolute max is made.
        //        pubExtraReinforcements = (int)(gubGarrisonReinforcementsDenied[iGarrisonID] / (6 - gGameOptions.ubDifficultyLevel));
        //Make sure the number of extra reinforcements don't bump the force size past the max of MAX_STRATEGIC_TEAM_SIZE.
        pubExtraReinforcements = (int)Math.Min((int)pubExtraReinforcements, Math.Min((int)pubExtraReinforcements, MAX_STRATEGIC_TEAM_SIZE - iReinforcementsRequested));

        iReinforcementsRequested = Math.Min(MAX_STRATEGIC_TEAM_SIZE, iReinforcementsRequested);

        if (iReinforcementsRequested + pubExtraReinforcements + iExistingForces > MAX_STRATEGIC_TEAM_SIZE)
        {
            iExistingForces = iExistingForces;
        }

        return iReinforcementsRequested;
    }

    public static int PatrolReinforcementsRequested(int iPatrolID)
    {
        GROUP? pGroup;
        pGroup = StrategicMovement.GetGroup(gPatrolGroup[iPatrolID].ubGroupID);
        if (pGroup is null)
        {
            return gPatrolGroup[iPatrolID].bSize;
        }
        else
        {
            return gPatrolGroup[iPatrolID].bSize - pGroup.ubGroupSize;
        }
    }

    public static int ReinforcementsAvailable(Garrisons iGarrisonID)
    {
        SECTORINFO? pSector;
        int iReinforcementsAvailable;

        pSector = SectorInfo[gGarrisonGroup[(int)iGarrisonID].ubSectorID];
        iReinforcementsAvailable = pSector.ubNumTroops + pSector.ubNumElites + pSector.ubNumAdmins;
        iReinforcementsAvailable -= gArmyComp[gGarrisonGroup[(int)iGarrisonID].ubComposition].bDesiredPopulation;

        switch (gGarrisonGroup[(int)iGarrisonID].ubComposition)
        {
            case Garrisons.LEVEL1_DEFENCE:
            case Garrisons.LEVEL2_DEFENCE:
            case Garrisons.LEVEL3_DEFENCE:
            case Garrisons.ALMA_DEFENCE:
            case Garrisons.ALMA_MINE:
                //Legal spawning locations
                break;
            default:
                //No other sector permitted to send surplus troops
                return 0;
        }

        return iReinforcementsAvailable;
    }

    //
    bool PlayerForceTooStrong(SEC ubSectorID, int usOffensePoints, out int pusDefencePoints)
    {
        SECTORINFO? pSector;
        int ubSectorX;
        MAP_ROW ubSectorY;
        pusDefencePoints = 0;

        ubSectorX = SECTORINFO.SECTORX(ubSectorID);
        ubSectorY = SECTORINFO.SECTORY(ubSectorID);
        pSector = SectorInfo[ubSectorID];

        //        pusDefencePoints = pSector.ubNumberOfCivsAtLevel[MilitiaExperience.GREEN_MILITIA] * 1 +
        //                                                pSector.ubNumberOfCivsAtLevel[MilitiaExperience.REGULAR_MILITIA] * 2 +
        //                                                pSector.ubNumberOfCivsAtLevel[MilitiaExperience.ELITE_MILITIA] * 3 +
        //                                                PlayerMercsInSector(ubSectorX, ubSectorY, 0) * 5;
        //        if (pusDefencePoints > usOffensePoints)
        //        {
        //            return true;
        //        }
        return false;
    }

    void RequestAttackOnSector(SEC ubSectorID, int usDefencePoints)
    {
        for (int i = 0; i < giGarrisonArraySize; i++)
        {
            if (gGarrisonGroup[i].ubSectorID == ubSectorID && gGarrisonGroup[i].ubPendingGroupID == 0)
            {

                this.SendReinforcementsForGarrison(i, usDefencePoints, null);
                return;
            }
        }
    }



    bool AdjacentSectorIsImportantAndUndefended(SEC ubSectorID)
    {
        SECTORINFO? pSector;
        switch (ubSectorID)
        {
            case SEC.A9:
            case SEC.A10:                               //Omerta
            case SEC.C5:
            case SEC.C6:
            case SEC.D5:    //San Mona
            case SEC.I6:                                                            //Estoni
                                                                                    //These sectors aren't important.
                return false;
        }
        pSector = SectorInfo[ubSectorID];
        if (pSector.ubNumTroops > 0 || pSector.ubNumElites > 0 || pSector.ubNumAdmins > 0)
        {
            return false;
        }

        if (pSector.ubTraversability[(StrategicMove)4] == Traversability.TOWN)
        {
            //            if (!PlayerSectorDefended(ubSectorID))
            //            {
            //                return true;
            //            }
        }
        return false;
    }

    public static void ValidatePendingGroups()
    {
    }

    public static void ValidateWeights(int iID)
    {
    }

    public static void ValidateGroup(GROUP? pGroup)
    {
        //        if (!pGroup.ubSectorX || !pGroup.ubSectorY || pGroup.ubSectorX > 16 || pGroup.ubSectorY > 16)
        //        {
        //            if (gTacticalStatus.uiFlags & LOADING_SAVED_GAME)
        //            {
        //                ClearPreviousAIGroupAssignment(pGroup);
        //                RemovePGroup(pGroup);
        //                return;
        //            }
        //        }

        //        if (!pGroup.ubNextX || !pGroup.ubNextY)
        //        {
        //            if (!pGroup.fPlayer && pGroup.pEnemyGroup.ubIntention != STAGING
        //                                                     && pGroup.pEnemyGroup.ubIntention != REINFORCEMENTS)
        //            {
        //                if (gTacticalStatus.uiFlags & LOADING_SAVED_GAME)
        //                {
        //                    ClearPreviousAIGroupAssignment(pGroup);
        //                    ReassignAIGroup(pGroup);
        //                    return;
        //                }
        //            }
        //        }
    }

    public static void ValidateLargeGroup(GROUP? pGroup)
    {
    }

    public static void InitStrategicAI()
    {
        int cnt, iRandom;
        int iEliteChance, iTroopChance, iAdminChance;
        int iWeight;
        int iStartPop, iDesiredPop, iPriority;
        SECTORINFO? pSector = null;
        GROUP? pGroup;
        int ubNumTroops;
        //Initialize the basic variables.

        gbPadding2[0] = 0;
        gbPadding2[1] = 0;
        gbPadding2[2] = 0;
        gfExtraElites = false;
        giGarrisonArraySize = 0;
        giPatrolArraySize = 0;
        giForcePercentage = 0;
        giArmyAlertness = 0;
        giArmyAlertnessDecay = 0;
        gubNumAwareBattles = 0;
        gfQueenAIAwake = false;
        giReinforcementPool = 0;
        giReinforcementPoints = 0;
        giRequestPoints = 0;
        gubSAIVersion = SAI_VERSION;
        gubQueenPriorityPhase = 0;
        gfFirstBattleMeanwhileScenePending = false;
        gfMassFortificationOrdered = false;
        gubMinEnemyGroupSize = 0;
        gubHoursGracePeriod = 0;
        gusPlayerBattleVictories = 0;
        gfUseAlternateQueenPosition = false;

        switch (gGameOptions.ubDifficultyLevel)
        {
            case DifficultyLevel.Easy:
                giReinforcementPool = EASY_QUEENS_POOL_OF_TROOPS;
                giForcePercentage = EASY_INITIAL_GARRISON_PERCENTAGES;
                giArmyAlertness = EASY_ENEMY_STARTING_ALERT_LEVEL;
                giArmyAlertnessDecay = EASY_ENEMY_STARTING_ALERT_DECAY;
                gubMinEnemyGroupSize = EASY_MIN_ENEMY_GROUP_SIZE;
                gubHoursGracePeriod = EASY_GRACE_PERIOD_IN_HOURS;
                // 475 is 7:55am in minutes since midnight, the time the game starts on day 1
                GameEvents.AddStrategicEvent(EVENT.EVALUATE_QUEEN_SITUATION, (uint)(475 + EASY_TIME_EVALUATE_IN_MINUTES + Globals.Random.Next(EASY_TIME_EVALUATE_VARIANCE)), 0);
                break;
            case DifficultyLevel.Medium:
                giReinforcementPool = NORMAL_QUEENS_POOL_OF_TROOPS;
                giForcePercentage = NORMAL_INITIAL_GARRISON_PERCENTAGES;
                giArmyAlertness = NORMAL_ENEMY_STARTING_ALERT_LEVEL;
                giArmyAlertnessDecay = NORMAL_ENEMY_STARTING_ALERT_DECAY;
                gubMinEnemyGroupSize = NORMAL_MIN_ENEMY_GROUP_SIZE;
                gubHoursGracePeriod = NORMAL_GRACE_PERIOD_IN_HOURS;
                GameEvents.AddStrategicEvent(EVENT.EVALUATE_QUEEN_SITUATION, (uint)(475 + NORMAL_TIME_EVALUATE_IN_MINUTES + Globals.Random.Next(NORMAL_TIME_EVALUATE_VARIANCE)), 0);
                break;
            case DifficultyLevel.Hard:
                giReinforcementPool = HARD_QUEENS_POOL_OF_TROOPS;
                giForcePercentage = HARD_INITIAL_GARRISON_PERCENTAGES;
                giArmyAlertness = HARD_ENEMY_STARTING_ALERT_LEVEL;
                giArmyAlertnessDecay = HARD_ENEMY_STARTING_ALERT_DECAY;
                gubMinEnemyGroupSize = HARD_MIN_ENEMY_GROUP_SIZE;
                gubHoursGracePeriod = HARD_GRACE_PERIOD_IN_HOURS;
                GameEvents.AddStrategicEvent(EVENT.EVALUATE_QUEEN_SITUATION, (uint)(475 + HARD_TIME_EVALUATE_IN_MINUTES + Globals.Random.Next(HARD_TIME_EVALUATE_VARIANCE)), 0);
                break;
        }

        //Initialize the sectorinfo structure so all sectors don't point to a garrisonID.
        for (SEC i = 0; i <= (SEC)255; i++)
        {
            SectorInfo[i].ubGarrisonID = NO_GARRISON;
        }

        //copy over the original army composition as it does get modified during the campaign.  This
        //bulletproofs starting the game over again.
        gArmyComp = gOrigArmyComp;

        //Eliminate more perimeter defenses on the easier levels.
        switch (gGameOptions.ubDifficultyLevel)
        {
            case DifficultyLevel.Easy:
                gArmyComp[Garrisons.LEVEL2_DEFENCE].bDesiredPopulation = 0;
                gArmyComp[Garrisons.LEVEL2_DEFENCE].bStartPopulation = 0;
                gArmyComp[Garrisons.LEVEL3_DEFENCE].bDesiredPopulation = 0;
                gArmyComp[Garrisons.LEVEL3_DEFENCE].bStartPopulation = 0;
                break;
            case DifficultyLevel.Medium:
                gArmyComp[Garrisons.LEVEL3_DEFENCE].bDesiredPopulation = 0;
                gArmyComp[Garrisons.LEVEL3_DEFENCE].bStartPopulation = 0;
                break;
        }

        //initialize the patrol group definitions
        giPatrolArraySize = gOrigPatrolGroup.Length;

        if (gPatrolGroup is null)
        { //Allocate it (otherwise, we just overwrite it because the size never changes)
            gPatrolGroup = new();
        }

        gPatrolGroup = gOrigPatrolGroup.ToList();

        gubPatrolReinforcementsDenied = new int[giPatrolArraySize];
        gubPatrolReinforcementsDenied = new int[giPatrolArraySize];

        //initialize the garrison group definitions
        giGarrisonArraySize = gOrigGarrisonGroup.Length;
        if (gGarrisonGroup is null)
        {
            gGarrisonGroup = Array.Empty<GARRISON_GROUP>();
        }

        gGarrisonGroup = gOrigGarrisonGroup;

        gubGarrisonReinforcementsDenied = [];

        //Modify initial force sizes?
        if (giForcePercentage != 100)
        { //The initial force sizes are being modified, so go through each of the army compositions
          //and adjust them accordingly.
            for (Garrisons i = 0; i < Garrisons.NUM_ARMY_COMPOSITIONS; i++)
            {
                if (i != Garrisons.QUEEN_DEFENCE)
                {
                    gArmyComp[i].bDesiredPopulation = (int)Math.Min(MAX_STRATEGIC_TEAM_SIZE, (gArmyComp[i].bDesiredPopulation * giForcePercentage / 100));
                    if (gArmyComp[i].bStartPopulation != MAX_STRATEGIC_TEAM_SIZE)
                    { //if the value is MAX_STRATEGIC_TEAM_SIZE, then that means the particular sector is a spawning location.  
                      //Don't modify the value if it is MAX_STRATEGIC_TEAM_SIZE.  Everything else is game.
                        gArmyComp[i].bStartPopulation = (int)Math.Min(MAX_STRATEGIC_TEAM_SIZE, (gArmyComp[i].bStartPopulation * giForcePercentage / 100));
                    }
                }
                else
                {
                    gArmyComp[i].bDesiredPopulation = (int)Math.Min(32, (gArmyComp[i].bDesiredPopulation * giForcePercentage / 100));
                    gArmyComp[i].bStartPopulation = gArmyComp[i].bDesiredPopulation;
                }
            }
            for (int i = 0; i < giPatrolArraySize; i++)
            { //force modified range within 1-MAX_STRATEGIC_TEAM_SIZE.
                gPatrolGroup[i].bSize = (int)Math.Max(gubMinEnemyGroupSize, Math.Min(MAX_STRATEGIC_TEAM_SIZE, (gPatrolGroup[i].bSize * giForcePercentage / 100)));
            }
        }

        //Now, initialize the garrisons based on the initial sizes (all variances are plus or minus 1).
        for (int i = 0; i < giGarrisonArraySize; i++)
        {
            pSector = SectorInfo[gGarrisonGroup[i].ubSectorID];
            pSector.ubGarrisonID = (Garrisons)i;
            iStartPop = gArmyComp[gGarrisonGroup[i].ubComposition].bStartPopulation;
            iDesiredPop = gArmyComp[gGarrisonGroup[i].ubComposition].bDesiredPopulation;
            iPriority = gArmyComp[gGarrisonGroup[i].ubComposition].bPriority;
            iEliteChance = gArmyComp[gGarrisonGroup[i].ubComposition].bElitePercentage;
            iTroopChance = gArmyComp[gGarrisonGroup[i].ubComposition].bTroopPercentage + iEliteChance;
            iAdminChance = gArmyComp[gGarrisonGroup[i].ubComposition].bAdminPercentage;

            switch (gGarrisonGroup[i].ubComposition)
            {
                case Garrisons.ROADBLOCK:
                    pSector.uiFlags |= SF.ENEMY_AMBUSH_LOCATION;
                    if (Globals.Random.Chance(20))
                    {
                        iStartPop = gArmyComp[gGarrisonGroup[i].ubComposition].bDesiredPopulation;
                    }
                    else
                    {
                        iStartPop = 0;
                    }

                    break;
                case Garrisons.SANMONA_SMALL:
                    iStartPop = 0; //not appropriate until Kingpin is killed.
                    break;
            }

            if (iStartPop != 0)
            {
                if (gGarrisonGroup[i].ubSectorID != SEC.P3)
                {
                    // if population is less than maximum
                    if (iStartPop != MAX_STRATEGIC_TEAM_SIZE)
                    {
                        // then vary it a bit (+/- 25%)
                        iStartPop = iStartPop * (100 + (Globals.Random.Next(51) - 25)) / 100;
                    }

                    iStartPop = Math.Max(gubMinEnemyGroupSize, Math.Min(MAX_STRATEGIC_TEAM_SIZE, iStartPop));
                }
                cnt = iStartPop;

                if (iAdminChance != 0)
                {
                    pSector.ubNumAdmins = iAdminChance * iStartPop / 100;
                }
                else
                {
                    while (cnt-- > 0)
                    { //for each person, randomly determine the types of each soldier.
                        {
                            iRandom = Globals.Random.Next(100);
                            if (iRandom < iEliteChance)
                            {
                                pSector.ubNumElites++;
                            }
                            else if (iRandom < iTroopChance)
                            {
                                pSector.ubNumTroops++;
                            }
                        }
                    }
                }

                switch (gGarrisonGroup[i].ubComposition)
                {
                    case Garrisons.CAMBRIA_DEFENCE:
                    case Garrisons.CAMBRIA_MINE:
                    case Garrisons.ALMA_MINE:
                    case Garrisons.GRUMM_MINE:
                        //Fill up extra start slots with troops
                        pSector.ubNumTroops = (int)(iStartPop -= pSector.ubNumAdmins);
                        break;
                    case Garrisons.DRASSEN_AIRPORT:
                    case Garrisons.DRASSEN_DEFENCE:
                    case Garrisons.DRASSEN_MINE:
                        pSector.ubNumAdmins = (int)Math.Max(5, pSector.ubNumAdmins);
                        break;
                    case Garrisons.TIXA_PRISON:
                        pSector.ubNumAdmins = (int)Math.Max(8, pSector.ubNumAdmins);
                        break;

                }
            }
            if (iAdminChance > 0 && pSector.ubNumAdmins < gubMinEnemyGroupSize)
            {
                pSector.ubNumAdmins = gubMinEnemyGroupSize;
            }
            //Calculate weight (range is -20 to +20 before multiplier).
            //The multiplier of 3 brings it to a range of -96 to +96 which is
            //close enough to a plus/minus 100%.  The resultant percentage is then
            //converted based on the priority.
            iWeight = (iDesiredPop - iStartPop) * 3;
            if (iWeight > 0)
            { //modify it by it's priority.
              //generates a value between 2 and 100
                iWeight = iWeight * iPriority / 96;
                iWeight = Math.Max(iWeight, 2);
                giRequestPoints += iWeight;
            }
            else if (iWeight < 0)
            { //modify it by it's reverse priority 
              //generates a value between -2 and -100
                iWeight = iWeight * (100 - iPriority) / 96;
                iWeight = Math.Min(iWeight, -2);
                giReinforcementPoints -= iWeight;
            }
            gGarrisonGroup[i].bWeight = (int)iWeight;

            //Now post an event which allows them to check adjacent sectors periodically.
            //Spread them out so that they process at different times.
            GameEvents.AddPeriodStrategicEventWithOffset(EVENT.CHECK_ENEMY_CONTROLLED_SECTOR, (uint)(140 - 20 * (int)gGameOptions.ubDifficultyLevel + Globals.Random.Next(4)), 475 + (uint)i, (int)gGarrisonGroup[i].ubSectorID);
        }

        // Now, initialize each of the patrol groups
        for (int i = 0; i < giPatrolArraySize; i++)
        {   // IGNORE COMMENT, FEATURE REMOVED!
            //Some of the patrol groups aren't there at the beginning of the game.  This is 
            //based on the difficulty settings in the above patrol table.
            //if( gPatrolGroup[ i ].ubUNUSEDStartIfDifficulty <= gGameOptions.ubDifficultyLevel )
            { //Add this patrol group now.
                ubNumTroops = (int)(gPatrolGroup[i].bSize + Globals.Random.Next(3) - 1);
                ubNumTroops = (int)Math.Max(gubMinEnemyGroupSize, Math.Min(MAX_STRATEGIC_TEAM_SIZE, ubNumTroops));
                //ubNumTroops = (int)Math.Max( gubMinEnemyGroupSize, Math.Min( MAX_STRATEGIC_TEAM_SIZE, gPatrolGroup[ i ].bSize + Globals.Random.Next( 3 ) - 1 ) );
                //Note on adding patrol groups...
                //The patrol group can't actually start on the first waypoint, so we set it to the second way
                //point for initialization, and then add the waypoints from 0 up
                pGroup = StrategicMovement.CreateNewEnemyGroupDepartingFromSector(gPatrolGroup[i].ubSectorID[1], 0, ubNumTroops, 0);
                Debug.Assert(pGroup is not null);

                if (i == 3 || i == 4)
                { //Special case:  Two patrol groups are administrator groups -- rest are troops
                    pGroup.pEnemyGroup.ubNumAdmins = pGroup.pEnemyGroup.ubNumTroops;
                    pGroup.pEnemyGroup.ubNumTroops = 0;
                }
                gPatrolGroup[i].ubGroupID = pGroup.ubGroupID;
                pGroup.pEnemyGroup.ubIntention = ENEMY_INTENTIONS.PATROL;
                pGroup.ubMoveType = MOVE_TYPES.ENDTOEND_FORWARDS;
                StrategicMovement.AddWaypointIDToPGroup(pGroup, gPatrolGroup[i].ubSectorID[0]);
                StrategicMovement.AddWaypointIDToPGroup(pGroup, gPatrolGroup[i].ubSectorID[1]);
                if (gPatrolGroup[i].ubSectorID[2] != 0)
                { //Add optional waypoints if included.
                    StrategicMovement.AddWaypointIDToPGroup(pGroup, gPatrolGroup[i].ubSectorID[2]);
                    if (gPatrolGroup[i].ubSectorID[3] != 0)
                    {
                        StrategicMovement.AddWaypointIDToPGroup(pGroup, gPatrolGroup[i].ubSectorID[3]);
                    }
                }

                StrategicMovement.RandomizePatrolGroupLocation(pGroup);
                ValidateGroup(pGroup);
            }
            //else
            //{ //we aren't creating this patrol group at the beginning of the game, so we
            //need to set up the weighting values to prioritize it's reinforcement request so that
            //it gets filled up later in the game.
            //	iWeight = gPatrolGroup[ i ].bSize * 3 * gPatrolGroup[ i ].bPriority / 96;
            //	gPatrolGroup[ i ].bWeight = (int)iWeight;
            //	giRequestPoints += iWeight;
            //}
        }

        //Setup the flags for the four sam sites.
        SectorInfo[SEC.D2].uiFlags |= SF.SAM_SITE;
        SectorInfo[SEC.D15].uiFlags |= SF.SAM_SITE;
        SectorInfo[SEC.I8].uiFlags |= SF.SAM_SITE;
        SectorInfo[SEC.N4].uiFlags |= SF.SAM_SITE;

        //final thing to do is choose 1 cache map out of 5 possible maps.  Simply select the sector randomly,
        //set up the flags to use the alternate map, then place 8-12 regular troops there (no ai though).
        //changing MAX_STRATEGIC_TEAM_SIZE may require changes to to the defending force here.
        switch (Globals.Random.Next(5))
        {
            case 0:
                pSector = SectorInfo[SEC.E11];
                break;
            case 1:
                pSector = SectorInfo[SEC.H5];
                break;
            case 2:
                pSector = SectorInfo[SEC.H10];
                break;
            case 3:
                pSector = SectorInfo[SEC.J12];
                break;
            case 4:
                pSector = SectorInfo[SEC.M9];
                break;
        }
        pSector.uiFlags |= SF.USE_ALTERNATE_MAP;
        pSector.ubNumTroops = (int)(6 + (int)gGameOptions.ubDifficultyLevel * 2);

        ValidateWeights(1);
    }

    void KillStrategicAI()
    {
        if (gPatrolGroup is not null)
        {
            gPatrolGroup = null;
        }

        Array.Clear(gGarrisonGroup);

        if (gubPatrolReinforcementsDenied.Length > 0)
        {
            MemFree(gubPatrolReinforcementsDenied);
            gubPatrolReinforcementsDenied = null;
        }

        if (gubGarrisonReinforcementsDenied.Any())
        {
            MemFree(gubGarrisonReinforcementsDenied);
            //            gubGarrisonReinforcementsDenied = null;
        }

        GameEvents.DeleteAllStrategicEventsOfType(EVENT.EVALUATE_QUEEN_SITUATION);
    }

    public static bool OkayForEnemyToMoveThroughSector(SEC ubSectorID)
    {
        SECTORINFO? pSector;
        pSector = SectorInfo[ubSectorID];
        if (pSector.uiTimeLastPlayerLiberated > 0 && (pSector.uiTimeLastPlayerLiberated + (gubHoursGracePeriod * 3600) > GameClock.GetWorldTotalSeconds()))
        {
            return false;
        }
        return true;
    }

    public static bool EnemyPermittedToAttackSector(GROUP? pGroup, SEC ubSectorID)
    {
        SECTORINFO? pSector;
        bool fPermittedToAttack = true;

        pSector = SectorInfo[ubSectorID];
        fPermittedToAttack = OkayForEnemyToMoveThroughSector(ubSectorID);
        if (pGroup is not null && pSector.ubGarrisonID != NO_GARRISON)
        {
            if (gGarrisonGroup[(int)pSector.ubGarrisonID].ubPendingGroupID > 0)
            {
                GROUP? pPendingGroup;
                pPendingGroup = StrategicMovement.GetGroup(gGarrisonGroup[(int)pSector.ubGarrisonID].ubPendingGroupID);
                if (pPendingGroup == pGroup)
                {
                    if (fPermittedToAttack)
                    {
                        if (StrategicMovement.GroupAtFinalDestination(pGroup))
                        { //High priority reinforcements have arrived.  This overrides most other situations.
                            return true;
                        }
                    }
                    else
                    {
                        //Reassign the group
                        //                        ReassignAIGroup(pGroup);
                    }
                }
            }
        }
        if (!fPermittedToAttack)
        {
            return false;
        }
        //If Hill-billies are alive, then enemy won't attack the sector.
        switch (ubSectorID)
        {
            case SEC.F10:
                //Hill-billy farm -- not until hill billies are dead.
                if (Facts.CheckFact((FACT)273, 0))
                {
                    return false;
                }

                break;
            case SEC.A9:
            case SEC.A10:
                //Omerta -- not until Day 2 at 7:45AM.	
                if (GameClock.GetWorldTotalMin() < 3345)
                {
                    return false;
                }

                break;
            case SEC.B13:
            case SEC.C13:
            case SEC.D13:
                //Drassen -- not until Day 3 at 6:30AM.
                if (GameClock.GetWorldTotalMin() < 4710)
                {
                    return false;
                }

                break;
            case SEC.C5:
            case SEC.C6:
            case SEC.D5:
                //San Mona -- not until Kingpin is dead.
                if (Facts.CheckFact(FACT.KINGPIN_DEAD, 0) == false)
                {
                    return false;
                }

                break;
            case SEC.G1:
                //                if (PlayerSectorDefended(SEC.G2) && (PlayerSectorDefended(SEC.H1) || PlayerSectorDefended(SEC.H2)))
                {
                    return false;
                }
                break;
            case SEC.H2:
                //                if (PlayerSectorDefended(SEC.H2) && (PlayerSectorDefended(SEC.G1) || PlayerSectorDefended(SEC.G2)))
                {
                    return false;
                }
                break;
        }
        return true;
    }

    bool HandlePlayerGroupNoticedByPatrolGroup(GROUP? pPlayerGroup, GROUP? pEnemyGroup)
    {
        int usDefencePoints;
        int usOffensePoints;
        SEC ubSectorID;

        ubSectorID = SECTORINFO.SECTOR(pPlayerGroup.ubSectorX, pPlayerGroup.ubSectorY);
        //        usOffensePoints = pEnemyGroup.pEnemyGroup.ubNumAdmins * 2 +
        //                                            pEnemyGroup.pEnemyGroup.ubNumTroops * 4 +
        //                                            pEnemyGroup.pEnemyGroup.ubNumElites * 6;
        //        if (PlayerForceTooStrong(ubSectorID, usOffensePoints, out usDefencePoints))
        //        {
        //            RequestAttackOnSector(ubSectorID, usDefencePoints);
        //            return false;
        //        }
        //For now, automatically attack.
        //        if (pPlayerGroup.ubNextX > 0)
        //        {
        //            MoveSAIGroupToSector(pEnemyGroup, SECTORINFO.SECTOR(pPlayerGroup.ubNextX, pPlayerGroup.ubNextY), DIRECT, PURSUIT);
        //        }
        //        else
        //        {
        //            MoveSAIGroupToSector(pEnemyGroup, SECTORINFO.SECTOR(pPlayerGroup.ubSectorX, pPlayerGroup.ubSectorY), DIRECT, PURSUIT);
        //        }

        return true;
    }

    void HandlePlayerGroupNoticedByGarrison(GROUP? pPlayerGroup, SEC ubSectorID)
    {
        SECTORINFO? pSector;
        GROUP? pGroup;
        int iReinforcementsApproved;
        int usOffensePoints;
        int ubEnemies;
        pSector = SectorInfo[ubSectorID];
        //First check to see if the player is at his final destination.
        if (!StrategicMovement.GroupAtFinalDestination(pPlayerGroup))
        {
            return;
        }

        usOffensePoints = pSector.ubNumAdmins * 2 +
                                            pSector.ubNumTroops * 4 +
                                            pSector.ubNumElites * 6;
        if (this.PlayerForceTooStrong(ubSectorID, usOffensePoints, out int usDefencePoints))
        {
            this.RequestAttackOnSector(ubSectorID, usDefencePoints);
            return;
        }

        if (pSector.ubGarrisonID != NO_GARRISON)
        {
            //Decide whether or not they will attack them with some of the troops.
            ubEnemies = (int)(pSector.ubNumAdmins + pSector.ubNumTroops + pSector.ubNumElites);
            iReinforcementsApproved = ubEnemies - gArmyComp[gGarrisonGroup[(int)pSector.ubGarrisonID].ubComposition].bDesiredPopulation / 2;
            if (iReinforcementsApproved * 2 > pPlayerGroup.ubGroupSize * 3 && iReinforcementsApproved > gubMinEnemyGroupSize)
            { //Then enemy's available outnumber the player by at least 3:2, so attack them.
                pGroup = StrategicMovement.CreateNewEnemyGroupDepartingFromSector(ubSectorID, 0, (int)iReinforcementsApproved, 0);

                //                ConvertGroupTroopsToComposition(pGroup, gGarrisonGroup[pSector.ubGarrisonID].ubComposition);

                //               MoveSAIGroupToSector(pGroup, SECTORINFO.SECTOR(pPlayerGroup.ubSectorX, pPlayerGroup.ubSectorY), DIRECT, REINFORCEMENTS);

                RemoveSoldiersFromGarrisonBasedOnComposition((int)pSector.ubGarrisonID, pGroup.ubGroupSize);

                if (pSector.ubNumTroops + pSector.ubNumElites + pSector.ubNumAdmins > MAX_STRATEGIC_TEAM_SIZE)
                {
                }
            }
        }
    }

    bool HandleMilitiaNoticedByPatrolGroup(SEC ubSectorID, GROUP? pEnemyGroup)
    {
        //For now, automatically attack.
        int usOffensePoints = 0;
        int ubSectorX = ((int)ubSectorID % 16) + 1;
        MAP_ROW ubSectorY = (MAP_ROW)((int)ubSectorID / 16) + 1;
        //        usOffensePoints = pEnemyGroup.pEnemyGroup.ubNumAdmins * 2 +
        //                                            pEnemyGroup.pEnemyGroup.ubNumTroops * 4 +
        //                                            pEnemyGroup.pEnemyGroup.ubNumElites * 6;
        if (this.PlayerForceTooStrong(ubSectorID, usOffensePoints, out int usDefencePoints))
        {
            this.RequestAttackOnSector(ubSectorID, usDefencePoints);
            return false;
        }

        MoveSAIGroupToSector(pEnemyGroup, SECTORINFO.SECTOR(ubSectorX, ubSectorY), SAIMOVECODE.DIRECT, ENEMY_INTENTIONS.REINFORCEMENTS);


        return false;
    }

    bool AttemptToNoticeEmptySectorSucceeds()
    {
        if (gubNumAwareBattles > 0 || gfAutoAIAware)
        { //The queen is in high-alert and is searching for players.  All adjacent checks will automatically succeed.
            return true;
        }

        //        if (DayTime())
        //        { //Day time chances are normal
        //            if (Chance(giArmyAlertness))
        //            {
        //                giArmyAlertness -= giArmyAlertnessDecay;
        //                //Minimum alertness should always be at least 0.
        //                giArmyAlertness = Math.Max(0, giArmyAlertness);
        //                return true;
        //            }
        //            giArmyAlertness++;
        //            return false;
        //        }
        //        //Night time chances are one third of normal.
        //        if (Chance(giArmyAlertness / 3))
        //        {
        //            giArmyAlertness -= giArmyAlertnessDecay;
        //            //Minimum alertness should always be at least 0.
        //            giArmyAlertness = Math.Max(0, giArmyAlertness);
        //            return true;
        //        }
        //        if (Chance(33))
        //        {
        //            giArmyAlertness++;
        //        }
        return false;
    }

    //Calling the function assumes that a player group is found to be adjacent to an enemy group.
    //This uses the alertness rating to emulate the chance that the group will notice.  If it does 
    //notice, then the alertness drops accordingly to simulate a period of time where the enemy would
    //not notice as much.  If it fails, the alertness gradually increases until it succeeds.
    bool AttemptToNoticeAdjacentGroupSucceeds()
    {
        if (gubNumAwareBattles > 0 || gfAutoAIAware)
        { //The queen is in high-alert and is searching for players.  All adjacent checks will automatically succeed.
            return true;
        }

        //        if (DayTime())
        //        { //Day time chances are normal
        //            if (Chance(giArmyAlertness))
        //            {
        //                giArmyAlertness -= giArmyAlertnessDecay;
        //                //Minimum alertness should always be at least 0.
        //                giArmyAlertness = Math.Max(0, giArmyAlertness);
        //                return true;
        //            }
        //            giArmyAlertness++;
        //            return false;
        //        }
        //        //Night time chances are one third of normal.
        //        if (Chance(giArmyAlertness / 3))
        //        {
        //            giArmyAlertness -= giArmyAlertnessDecay;
        //            //Minimum alertness should always be at least 0.
        //            giArmyAlertness = Math.Max(0, giArmyAlertness);
        //            return true;
        //        }
        //        if (Chance(33))
        //        {
        //            giArmyAlertness++;
        //        }
        return false;
    }

    bool HandleEmptySectorNoticedByPatrolGroup(GROUP? pGroup, SEC ubEmptySectorID)
    {
        Garrisons ubGarrisonID;
        int ubSectorX = ((int)ubEmptySectorID % 16) + 1;
        MAP_ROW ubSectorY = (MAP_ROW)((int)ubEmptySectorID / 16) + 1;

        ubGarrisonID = SectorInfo[ubEmptySectorID].ubGarrisonID;
        if (ubGarrisonID != NO_GARRISON)
        {
            if (gGarrisonGroup[(int)ubGarrisonID].ubPendingGroupID > 0)
            {
                return false;
            }
        }
        else
        {
            return false;
        }

        //Clear the patrol group's previous orders.
        ClearPreviousAIGroupAssignment(pGroup);

        gGarrisonGroup[(int)ubGarrisonID].ubPendingGroupID = pGroup.ubGroupID;
        MoveSAIGroupToSector(pGroup, SECTORINFO.SECTOR(ubSectorX, ubSectorY), SAIMOVECODE.DIRECT, ENEMY_INTENTIONS.REINFORCEMENTS);



        return true;
    }

    void HandleEmptySectorNoticedByGarrison(SEC ubGarrisonSectorID, SEC ubEmptySectorID)
    {
        SECTORINFO? pSector;
        GROUP? pGroup;
        int ubAvailableTroops;
        Garrisons ubSrcGarrisonID = (Garrisons)255, ubDstGarrisonID = (Garrisons)255;

        //Make sure that the destination sector doesn't already have a pending group.
        pSector = SectorInfo[ubEmptySectorID];

        ubSrcGarrisonID = SectorInfo[ubGarrisonSectorID].ubGarrisonID;
        ubDstGarrisonID = SectorInfo[ubEmptySectorID].ubGarrisonID;

        if (ubSrcGarrisonID == NO_GARRISON || ubDstGarrisonID == NO_GARRISON)
        { //Bad logic
            return;
        }

        if (gGarrisonGroup[(int)ubDstGarrisonID].ubPendingGroupID > 0)
        { //A group is already on-route, so don't send anybody from here.
            return;
        }

        //An opportunity has arisen, where the enemy has noticed an important sector that is undefended.
        pSector = SectorInfo[ubGarrisonSectorID];
        ubAvailableTroops = pSector.ubNumTroops + pSector.ubNumElites + pSector.ubNumAdmins;

        if (ubAvailableTroops >= gubMinEnemyGroupSize * 2)
        { //split group into two groups, and move one of the groups to the next sector.
            pGroup = StrategicMovement.CreateNewEnemyGroupDepartingFromSector(ubGarrisonSectorID, 0, (int)(ubAvailableTroops / 2), 0);
            //            ConvertGroupTroopsToComposition(pGroup, gGarrisonGroup[ubDstGarrisonID].ubComposition);
            //            RemoveSoldiersFromGarrisonBasedOnComposition(ubSrcGarrisonID, pGroup.ubGroupSize);
            //            gGarrisonGroup[ubDstGarrisonID].ubPendingGroupID = pGroup.ubGroupID;
            //            MoveSAIGroupToSector(pGroup, ubEmptySectorID, DIRECT, REINFORCEMENTS);
        }
    }

    bool ReinforcementsApproved(Garrisons iGarrisonID, out int pusDefencePoints)
    {
        SECTORINFO? pSector;
        int usOffensePoints;
        int ubSectorX;
        MAP_ROW ubSectorY;
        pusDefencePoints = 0;

        pSector = SectorInfo[gGarrisonGroup[(int)iGarrisonID].ubSectorID];
        ubSectorX = SECTORINFO.SECTORX(gGarrisonGroup[(int)iGarrisonID].ubSectorID);
        ubSectorY = SECTORINFO.SECTORY(gGarrisonGroup[(int)iGarrisonID].ubSectorID);

        //        pusDefencePoints = pSector.ubNumberOfCivsAtLevel[GREEN_MILITIA] * 1 +
        //                                            pSector.ubNumberOfCivsAtLevel[REGULAR_MILITIA] * 2 +
        //                                            pSector.ubNumberOfCivsAtLevel[ELITE_MILITIA] * 3 +
        //                                            PlayerMercsInSector(ubSectorX, ubSectorY, 0) * 4;
        usOffensePoints = gArmyComp[gGarrisonGroup[(int)iGarrisonID].ubComposition].bAdminPercentage * 2 +
                                            gArmyComp[gGarrisonGroup[(int)iGarrisonID].ubComposition].bTroopPercentage * 3 +
                                            gArmyComp[gGarrisonGroup[(int)iGarrisonID].ubComposition].bElitePercentage * 4 +
                                            gubGarrisonReinforcementsDenied[iGarrisonID];
        usOffensePoints = usOffensePoints * gArmyComp[gGarrisonGroup[(int)iGarrisonID].ubComposition].bDesiredPopulation / 100;

        if (usOffensePoints > pusDefencePoints)
        {
            return true;
        }
        //Before returning false, determine if reinforcements have been denied repeatedly.  If so, then
        //we might send an augmented force to take it back.
        if (gubGarrisonReinforcementsDenied[iGarrisonID] + usOffensePoints > pusDefencePoints)
        {
            return true;
        }
        //Reinforcements will have to wait.  For now, increase the reinforcements denied.  The amount increase is 20 percent
        //of the garrison's priority.
        gubGarrisonReinforcementsDenied[iGarrisonID] += (int)(gArmyComp[gGarrisonGroup[(int)iGarrisonID].ubComposition].bPriority / 2);

        return false;
    }

    //if the group has arrived in a sector, and doesn't have any particular orders, then
    //send him back where they came from.
    //RETURNS true if the group is deleted or told to move somewhere else.  
    //This is important as the calling function will need
    //to abort processing of the group for obvious reasons.
    public static bool EvaluateGroupSituation(GROUP? pGroup)
    {
        SECTORINFO? pSector;
        GROUP? pPatrolGroup;

        ValidateWeights(2);

        if (!gfQueenAIAwake)
        {
            return false;
        }
        Debug.Assert(!pGroup.fPlayer);
        //        if (pGroup.pEnemyGroup.ubIntention == PURSUIT)
        //        { //Lost the player group that he was going to attack.  Return to original position.
        //            ReassignAIGroup(pGroup);
        //            return true;
        //        }
        //        else if (pGroup.pEnemyGroup.ubIntention == REINFORCEMENTS)
        { //The group has arrived at the location where he is supposed to reinforce.
          //Step 1 -- Check for matching garrison location
            for (int i = 0; i < giGarrisonArraySize; i++)
            {
                if (gGarrisonGroup[i].ubSectorID == SECTORINFO.SECTOR(pGroup.ubSectorX, pGroup.ubSectorY) &&
                        gGarrisonGroup[i].ubPendingGroupID == pGroup.ubGroupID)
                {
                    pSector = SectorInfo[SECTORINFO.SECTOR(pGroup.ubSectorX, pGroup.ubSectorY)];

                    if (gGarrisonGroup[i].ubSectorID != SEC.P3)
                    {
                        //                        EliminateSurplusTroopsForGarrison(pGroup, pSector);
                        //                        pSector.ubNumAdmins = (int)(pSector.ubNumAdmins + pGroup.pEnemyGroup.ubNumAdmins);
                        //                        pSector.ubNumTroops = (int)(pSector.ubNumTroops + pGroup.pEnemyGroup.ubNumTroops);
                        //                        pSector.ubNumElites = (int)(pSector.ubNumElites + pGroup.pEnemyGroup.ubNumElites);

                        //                        if (IsThisSectorASAMSector(pGroup.ubSectorX, pGroup.ubSectorY, 0))
                        //                        {
                        //                            StrategicMap[pGroup.ubSectorX + pGroup.ubSectorY * MAP_WORLD_X].bSAMCondition = 100;
                        //                            UpdateSAMDoneRepair(pGroup.ubSectorX, pGroup.ubSectorY, 0);
                        //                        }
                    }
                    else
                    { //The group was sent back to the queen's palace (probably because they couldn't be reassigned 
                      //anywhere else, but it is possible that the queen's sector is requesting the reinforcements.  In
                      //any case, if the queen's sector is less than full strength, fill it up first, then
                      //simply add the rest to the global pool.
                        if (pSector.ubNumElites < MAX_STRATEGIC_TEAM_SIZE)
                        {
                            if (pSector.ubNumElites + pGroup.ubGroupSize >= MAX_STRATEGIC_TEAM_SIZE)
                            { //Fill up the queen's guards, then apply the rest to the reinforcement pool
                                giReinforcementPool += MAX_STRATEGIC_TEAM_SIZE - pSector.ubNumElites;
                                pSector.ubNumElites = MAX_STRATEGIC_TEAM_SIZE;
                            }
                            else
                            { //Add all the troops to the queen's guard.
                                pSector.ubNumElites += pGroup.ubGroupSize;
                            }
                        }
                        else
                        { //Add all the troops to the reinforcement pool as the queen's guard is at full strength.
                            giReinforcementPool += pGroup.ubGroupSize;
                        }
                    }

                    //                    SetThisSectorAsEnemyControlled(pGroup.ubSectorX, pGroup.ubSectorY, 0, true);
                    //                    RemovePGroup(pGroup);
                    RecalculateGarrisonWeight(i);

                    return true;
                }
            }
            //Step 2 -- Check for Patrol groups matching waypoint index.
            for (int i = 0; i < giPatrolArraySize; i++)
            {
                //                if (gPatrolGroup[i].ubSectorID[1] == SECTORINFO.SECTOR(pGroup.ubSectorX, pGroup.ubSectorY) &&
                //                        gPatrolGroup[i].ubPendingGroupID == pGroup.ubGroupID)
                //                {
                //                    gPatrolGroup[i].ubPendingGroupID = 0;
                //                    if (gPatrolGroup[i].ubGroupID > 0 && gPatrolGroup[i].ubGroupID != pGroup.ubGroupID)
                //                    { //cheat, and warp our reinforcements to them!
                //                        pPatrolGroup = StrategicMovement.GetGroup(gPatrolGroup[i].ubGroupID);
                //                        pPatrolGroup.pEnemyGroup.ubNumTroops += pGroup.pEnemyGroup.ubNumTroops;
                //                        pPatrolGroup.pEnemyGroup.ubNumElites += pGroup.pEnemyGroup.ubNumElites;
                //                        pPatrolGroup.pEnemyGroup.ubNumAdmins += pGroup.pEnemyGroup.ubNumAdmins;
                //                        pPatrolGroup.ubGroupSize += (int)(pGroup.pEnemyGroup.ubNumTroops + pGroup.pEnemyGroup.ubNumElites + pGroup.pEnemyGroup.ubNumAdmins);
                //                        if (pPatrolGroup.ubGroupSize > MAX_STRATEGIC_TEAM_SIZE)
                //                        {
                //                            int ubCut;
                //                            //truncate the group size.
                //                            ubCut = pPatrolGroup.ubGroupSize - MAX_STRATEGIC_TEAM_SIZE;
                //                            while (ubCut-- > 0)
                //                            {
                //                                if (pGroup.pEnemyGroup.ubNumAdmins)
                //                                {
                //                                    pGroup.pEnemyGroup.ubNumAdmins--;
                //                                    pPatrolGroup.pEnemyGroup.ubNumAdmins--;
                //                                }
                //                                else if (pGroup.pEnemyGroup.ubNumTroops)
                //                                {
                //                                    pGroup.pEnemyGroup.ubNumTroops--;
                //                                    pPatrolGroup.pEnemyGroup.ubNumTroops--;
                //                                }
                //                                else if (pGroup.pEnemyGroup.ubNumElites)
                //                                {
                //                                    pGroup.pEnemyGroup.ubNumElites--;
                //                                    pPatrolGroup.pEnemyGroup.ubNumElites--;
                //                                }
                //                            }
                //                            pPatrolGroup.ubGroupSize = MAX_STRATEGIC_TEAM_SIZE;
                //                            Debug.Assert(pPatrolGroup.pEnemyGroup.ubNumAdmins +
                //                                            pPatrolGroup.pEnemyGroup.ubNumTroops +
                //                                            pPatrolGroup.pEnemyGroup.ubNumElites == MAX_STRATEGIC_TEAM_SIZE);
                //                        }
                //                        RemovePGroup(pGroup);
                //                        RecalculatePatrolWeight(i);
                //                        ValidateLargeGroup(pPatrolGroup);
                //                    }
                //                    else
                //                    { //the reinforcements have become the new patrol group (even if same group)
                //                        gPatrolGroup[i].ubGroupID = pGroup.ubGroupID;
                //                        pGroup.pEnemyGroup.ubIntention = PATROL;
                //                        pGroup.ubMoveType = MOVE_TYPES.ENDTOEND_FORWARDS;
                //                        RemovePGroupWaypoints(pGroup);
                //                        AddWaypointIDToPGroup(pGroup, gPatrolGroup[i].ubSectorID[0]);
                //                        AddWaypointIDToPGroup(pGroup, gPatrolGroup[i].ubSectorID[1]);
                //                        if (gPatrolGroup[i].ubSectorID[2])
                //                        { //Add optional waypoints if included.
                //                            AddWaypointIDToPGroup(pGroup, gPatrolGroup[i].ubSectorID[2]);
                //                            if (gPatrolGroup[i].ubSectorID[3])
                //                            {
                //                                AddWaypointIDToPGroup(pGroup, gPatrolGroup[i].ubSectorID[3]);
                //                            }
                //                        }
                //
                //                        //Otherwise, the engine assumes they are being deployed.
                //                        //pGroup.fWaypointsCancelled = false;
                //
                //                        RecalculatePatrolWeight(i);
                //                    }
                //                    return true;
                //                }
            }
            //        }
            //        else
            //        {   //This is a floating group at his final destination...
            //            if (pGroup.pEnemyGroup.ubIntention != STAGING && pGroup.pEnemyGroup.ubIntention != REINFORCEMENTS)
            //            {
            //                ReassignAIGroup(pGroup);
            //                return true;
            //            }
            //        }
            ValidateWeights(3);
            return false;
        }
    }

    //returns true if the group was deleted.
    bool StrategicAILookForAdjacentGroups(GROUP? pGroup)
    {
        SECTORINFO? pSector;
        GROUP? pEnemyGroup, pPlayerGroup;
        int ubNumEnemies;
        SEC ubSectorID;
        if (!gfQueenAIAwake)
        { //The queen isn't aware the player's presence yet, so she is oblivious to any situations.

            if (!pGroup.fPlayer)
            {
                //Exception case!
                //In the beginning of the game, a group is sent to A9 after the first battle.  If you leave A9, when they arrive,
                //they will stay there indefinately because the AI isn't awake.  What we do, is if this is a group in A9, then
                //send them home.
                //                if (GroupAtFinalDestination(pGroup))
                //                {
                //                    //Wake up the queen now, if she hasn't woken up already.
                //                    WakeUpQueen();
                //                    if (pGroup.ubSectorX == 9 && pGroup.ubSectorY == (MAP_ROW)1 ||
                //                            pGroup.ubSectorX == 3 && pGroup.ubSectorY == (MAP_ROW)16)
                //                    {
                //                        SendGroupToPool(pGroup);
                //                        if (pGroup is null)
                //                        { //Group was transferred to the pool
                //                            return true;
                //                        }
                //                    }
                //                }
            }

            if (!gfQueenAIAwake)
            {
                return false;
            }
        }
        if (!pGroup.fPlayer)
        {   //The enemy group has arrived at a new sector and now controls it.  
            //Look in each of the four directions, and the alertness rating will
            //determine the chance to detect any players that may exist in that sector.
            pEnemyGroup = pGroup;
            //            if (GroupAtFinalDestination(pEnemyGroup))
            //            {
            //                return EvaluateGroupSituation(pEnemyGroup);
            //            }
            ubSectorID = SECTORINFO.SECTOR(pEnemyGroup.ubSectorX, pEnemyGroup.ubSectorY);
            if (pEnemyGroup is not null && pEnemyGroup.ubSectorY > (MAP_ROW)1 && EnemyPermittedToAttackSector(pEnemyGroup, ubSectorID - 16))
            {
                pPlayerGroup = StrategicMovement.FindMovementGroupInSector(pEnemyGroup.ubSectorX, pEnemyGroup.ubSectorY - 1, true);
                //                if (pPlayerGroup is not null && AttemptToNoticeAdjacentGroupSucceeds())
                //                {
                //                    return HandlePlayerGroupNoticedByPatrolGroup(pPlayerGroup, pEnemyGroup);
                //                }
                //                else if (CountAllMilitiaInSector(pEnemyGroup.ubSectorX, (int)(pEnemyGroup.ubSectorY - 1)) &&
                //                                AttemptToNoticeAdjacentGroupSucceeds())
                //                {
                //                    return HandleMilitiaNoticedByPatrolGroup(SECTORINFO.SECTOR(pEnemyGroup.ubSectorX, pEnemyGroup.ubSectorY - 1), pEnemyGroup);
                //                }
                //                else if (AdjacentSectorIsImportantAndUndefended((SEC)(ubSectorID - 16)) && AttemptToNoticeEmptySectorSucceeds())
                //                {
                //                    return HandleEmptySectorNoticedByPatrolGroup(pEnemyGroup, (SEC)(ubSectorID - 16));
                //                }
            }
            if (pEnemyGroup is not null && pEnemyGroup.ubSectorX > 1 && EnemyPermittedToAttackSector(pEnemyGroup, ubSectorID - 1))
            {
                pPlayerGroup = StrategicMovement.FindMovementGroupInSector((int)(pEnemyGroup.ubSectorX - 1), pEnemyGroup.ubSectorY, true);
                if (pPlayerGroup is not null && this.AttemptToNoticeAdjacentGroupSucceeds())
                {
                    return this.HandlePlayerGroupNoticedByPatrolGroup(pPlayerGroup, pEnemyGroup);
                }
                //                else if (CountAllMilitiaInSector((int)(pEnemyGroup.ubSectorX - 1), pEnemyGroup.ubSectorY) &&
                //                                AttemptToNoticeAdjacentGroupSucceeds())
                //                {
                //                    return HandleMilitiaNoticedByPatrolGroup(SECTORINFO.SECTOR(pEnemyGroup.ubSectorX - 1, pEnemyGroup.ubSectorY), pEnemyGroup);
                //                }
                else if (this.AdjacentSectorIsImportantAndUndefended(ubSectorID - 1) && this.AttemptToNoticeEmptySectorSucceeds())
                {
                    return this.HandleEmptySectorNoticedByPatrolGroup(pEnemyGroup, ubSectorID - 1);
                }
            }
            if (pEnemyGroup is not null && pEnemyGroup.ubSectorY < (MAP_ROW)16 && EnemyPermittedToAttackSector(pEnemyGroup, ubSectorID + 16))
            {
                pPlayerGroup = StrategicMovement.FindMovementGroupInSector(pEnemyGroup.ubSectorX, pEnemyGroup.ubSectorY + 1, true);
                if (pPlayerGroup is not null && this.AttemptToNoticeAdjacentGroupSucceeds())
                {
                    return this.HandlePlayerGroupNoticedByPatrolGroup(pPlayerGroup, pEnemyGroup);
                }
                //                else if (CountAllMilitiaInSector(pEnemyGroup.ubSectorX, (int)(pEnemyGroup.ubSectorY + 1)) &&
                //                                AttemptToNoticeAdjacentGroupSucceeds())
                //                {
                //                    return HandleMilitiaNoticedByPatrolGroup(SECTORINFO.SECTOR(pEnemyGroup.ubSectorX, pEnemyGroup.ubSectorY + 1), pEnemyGroup);
                //                }
                else if (this.AdjacentSectorIsImportantAndUndefended(ubSectorID + 16) && this.AttemptToNoticeEmptySectorSucceeds())
                {
                    return this.HandleEmptySectorNoticedByPatrolGroup(pEnemyGroup, ubSectorID + 16);
                }
            }
            if (pEnemyGroup is not null && pEnemyGroup.ubSectorX < 16 && EnemyPermittedToAttackSector(pEnemyGroup, ubSectorID + 1))
            {
                pPlayerGroup = StrategicMovement.FindMovementGroupInSector(pEnemyGroup.ubSectorX + 1, pEnemyGroup.ubSectorY, true);
                if (pPlayerGroup is not null && this.AttemptToNoticeAdjacentGroupSucceeds())
                {
                    return this.HandlePlayerGroupNoticedByPatrolGroup(pPlayerGroup, pEnemyGroup);
                }
                //                else if (CountAllMilitiaInSector((pEnemyGroup.ubSectorX + 1), pEnemyGroup.ubSectorY) &&
                //                                AttemptToNoticeAdjacentGroupSucceeds())
                //                {
                //                    return HandleMilitiaNoticedByPatrolGroup(SECTORINFO.SECTOR(pEnemyGroup.ubSectorX + 1, pEnemyGroup.ubSectorY), pEnemyGroup);
                //                }
                else if (this.AdjacentSectorIsImportantAndUndefended(ubSectorID + 1) && this.AttemptToNoticeEmptySectorSucceeds())
                {
                    return this.HandleEmptySectorNoticedByPatrolGroup(pEnemyGroup, ubSectorID + 1);
                }
            }

            if (pEnemyGroup is null)
            { //group deleted.
                return true;
            }
        }
        else
        { //The player group has arrived at a new sector and now controls it.  
          //Look in each of the four directions, and the enemy alertness rating will
          //determine if the enemy notices that the player is here.
          //Additionally, there are also stationary enemy groups that may also notice the
          //player's new presence.
          //NOTE:  Always returns false because it is the player group that we are handling.  We
          //       don't mess with the player group here!
            pPlayerGroup = pGroup;
            //            if (pPlayerGroup.ubSectorZ)
            //            {
            //                return false;
            //            }

            if (!EnemyPermittedToAttackSector(null, SECTORINFO.SECTOR(pPlayerGroup.ubSectorX, pPlayerGroup.ubSectorY)))
            {
                return false;
            }

            //            if (pPlayerGroup.ubSectorY > 1)
            //            {
            //                pEnemyGroup = StrategicMovement.FindMovementGroupInSector(pPlayerGroup.ubSectorX, (int)(pPlayerGroup.ubSectorY - 1), false);
            //                if (pEnemyGroup && AttemptToNoticeAdjacentGroupSucceeds())
            //                {
            //                    HandlePlayerGroupNoticedByPatrolGroup(pPlayerGroup, pEnemyGroup);
            //                    return false;
            //                }
            //                pSector = SectorInfo[SECTORINFO.SECTOR(pPlayerGroup.ubSectorX, pPlayerGroup.ubSectorY - 1)];
            //                ubNumEnemies = pSector.ubNumAdmins + pSector.ubNumTroops + pSector.ubNumElites;
            //                if (ubNumEnemies > 0 && pSector.ubGarrisonID != NO_GARRISON && AttemptToNoticeAdjacentGroupSucceeds())
            //                {
            //                    HandlePlayerGroupNoticedByGarrison(pPlayerGroup, SECTORINFO.SECTOR(pPlayerGroup.ubSectorX, pPlayerGroup.ubSectorY - 1));
            //                    return false;
            //                }
            //            }
            if (pPlayerGroup.ubSectorX < 16)
            {
                pEnemyGroup = StrategicMovement.FindMovementGroupInSector((int)(pPlayerGroup.ubSectorX + 1), pPlayerGroup.ubSectorY, false);
                if (pEnemyGroup is not null && this.AttemptToNoticeAdjacentGroupSucceeds())
                {
                    this.HandlePlayerGroupNoticedByPatrolGroup(pPlayerGroup, pEnemyGroup);
                    return false;
                }
                pSector = SectorInfo[SECTORINFO.SECTOR(pPlayerGroup.ubSectorX - 1, pPlayerGroup.ubSectorY)];
                ubNumEnemies = pSector.ubNumAdmins + pSector.ubNumTroops + pSector.ubNumElites;
                if (ubNumEnemies > 0 && pSector.ubGarrisonID != NO_GARRISON && this.AttemptToNoticeAdjacentGroupSucceeds())
                {
                    this.HandlePlayerGroupNoticedByGarrison(pPlayerGroup, SECTORINFO.SECTOR(pPlayerGroup.ubSectorX - 1, pPlayerGroup.ubSectorY));
                    return false;
                }
            }
            if (pPlayerGroup.ubSectorY < (MAP_ROW)16)
            {
                pEnemyGroup = StrategicMovement.FindMovementGroupInSector(pPlayerGroup.ubSectorX, pPlayerGroup.ubSectorY + 1, false);
                if (pEnemyGroup is not null && this.AttemptToNoticeAdjacentGroupSucceeds())
                {
                    this.HandlePlayerGroupNoticedByPatrolGroup(pPlayerGroup, pEnemyGroup);
                    return false;
                }
                pSector = SectorInfo[SECTORINFO.SECTOR(pPlayerGroup.ubSectorX, pPlayerGroup.ubSectorY + 1)];
                ubNumEnemies = pSector.ubNumAdmins + pSector.ubNumTroops + pSector.ubNumElites;
                if (ubNumEnemies > 0 && pSector.ubGarrisonID != NO_GARRISON && this.AttemptToNoticeAdjacentGroupSucceeds())
                {
                    this.HandlePlayerGroupNoticedByGarrison(pPlayerGroup, SECTORINFO.SECTOR(pPlayerGroup.ubSectorX, pPlayerGroup.ubSectorY + 1));
                    return false;
                }
            }
            if (pPlayerGroup.ubSectorX > 1)
            {
                pEnemyGroup = StrategicMovement.FindMovementGroupInSector(pPlayerGroup.ubSectorX - 1, pPlayerGroup.ubSectorY, false);
                if (pEnemyGroup is not null && this.AttemptToNoticeAdjacentGroupSucceeds())
                {
                    this.HandlePlayerGroupNoticedByPatrolGroup(pPlayerGroup, pEnemyGroup);
                    return false;
                }
                pSector = SectorInfo[SECTORINFO.SECTOR(pPlayerGroup.ubSectorX + 1, pPlayerGroup.ubSectorY)];
                ubNumEnemies = pSector.ubNumAdmins + pSector.ubNumTroops + pSector.ubNumElites;
                if (ubNumEnemies > 0 && pSector.ubGarrisonID != NO_GARRISON && this.AttemptToNoticeAdjacentGroupSucceeds())
                {
                    this.HandlePlayerGroupNoticedByGarrison(pPlayerGroup, SECTORINFO.SECTOR(pPlayerGroup.ubSectorX + 1, pPlayerGroup.ubSectorY));
                    return false;
                }
            }
        }
        return false;
    }

    //This is called periodically for each enemy occupied sector containing garrisons.
    void CheckEnemyControlledSector(SEC ubSectorID)
    {
        SECTORINFO? pSector;
        int ubSectorX;
        MAP_ROW ubSectorY;
        if (!gfQueenAIAwake)
        {
            return;
        }
        //First, determine if the sector is still owned by the enemy.  
        pSector = SectorInfo[ubSectorID];
        if (pSector.ubGarrisonID != NO_GARRISON)
        {
            if (gGarrisonGroup[(int)pSector.ubGarrisonID].ubPendingGroupID > 0)
            { //Look for a staging group.
                GROUP? pGroup;
                pGroup = StrategicMovement.GetGroup(gGarrisonGroup[(int)pSector.ubGarrisonID].ubPendingGroupID);
                //                if (pGroup)
                //                { //We have a staging group
                //                    if (GroupAtFinalDestination(pGroup))
                //                    {
                //                        if (pGroup.pEnemyGroup.ubPendingReinforcements)
                //                        {
                //                            if (pGroup.pEnemyGroup.ubPendingReinforcements > 4)
                //                            {
                //                                int ubNum = (int)(3 + Globals.Random.Next(3));
                //                                pGroup.pEnemyGroup.ubNumTroops += ubNum;
                //                                pGroup.ubGroupSize += ubNum;
                //                                pGroup.pEnemyGroup.ubPendingReinforcements -= ubNum;
                //                                RecalculateGroupWeight(pGroup);
                //                                ValidateLargeGroup(pGroup);
                //                            }
                //                            else
                //                            {
                //                                pGroup.pEnemyGroup.ubNumTroops += pGroup.pEnemyGroup.ubPendingReinforcements;
                //                                pGroup.ubGroupSize += pGroup.pEnemyGroup.ubPendingReinforcements;
                //                                pGroup.pEnemyGroup.ubPendingReinforcements = 0;
                //                                ValidateLargeGroup(pGroup);
                //                            }
                //                            //RequestHighPriorityStagingGroupReinforcements( pGroup );
                //                        }
                //                        else if (SECTORINFO.SECTOR(pGroup.ubSectorX, pGroup.ubSectorY) != gGarrisonGroup[pSector.ubGarrisonID].ubSectorID)
                //                        {
                //                            MoveSAIGroupToSector(pGroup, gGarrisonGroup[pSector.ubGarrisonID].ubSectorID, DIRECT, pGroup.pEnemyGroup.ubIntention);
                //                        }
                //                    }
                //                    //else the group is on route to stage hopefully...
                //                }
            }
        }
        //        if (pSector.ubNumAdmins + pSector.ubNumTroops + pSector.ubNumElites)
        //        {
        //
        //            //The sector is still controlled, so look around to see if there are any players nearby.
        //            ubSectorX = SECTORINFO.SECTORX(ubSectorID);
        //            ubSectorY = SECTORINFO.SECTORY(ubSectorID);
        //            if (ubSectorY > 1 && EnemyPermittedToAttackSector(null, (ubSectorID - 16)))
        //            {
        //                /*
        //                pPlayerGroup = FindMovementGroupInSector( ubSectorX, (int)(ubSectorY-1), true );
        //                if( pPlayerGroup && AttemptToNoticeAdjacentGroupSucceeds() )
        //                {
        //                    HandlePlayerGroupNoticedByGarrison( pPlayerGroup, ubSectorID );
        //                    return;
        //                }
        //                else
        //                */
        //                if (AdjacentSectorIsImportantAndUndefended((ubSectorID - 16)) && AttemptToNoticeEmptySectorSucceeds())
        //                {
        //                    HandleEmptySectorNoticedByGarrison(ubSectorID, (ubSectorID - 16));
        //                    return;
        //                }
        //            }
        //            if (ubSectorX < 16 && EnemyPermittedToAttackSector(null, (ubSectorID + 1)))
        //            {
        //                /*
        //                pPlayerGroup = FindMovementGroupInSector( (int)(ubSectorX+1), ubSectorY, true );
        //                if( pPlayerGroup && AttemptToNoticeAdjacentGroupSucceeds() )
        //                {
        //                    HandlePlayerGroupNoticedByGarrison( pPlayerGroup, ubSectorID );
        //                    return;
        //                }
        //                else 
        //                */
        //                if (AdjacentSectorIsImportantAndUndefended((ubSectorID + 1)) && AttemptToNoticeEmptySectorSucceeds())
        //                {
        //                    HandleEmptySectorNoticedByGarrison(ubSectorID, (ubSectorID + 1));
        //                    return;
        //                }
        //            }
        //            if (ubSectorY < 16 && EnemyPermittedToAttackSector(null, (ubSectorID + 16)))
        //            {
        //                /*
        //                pPlayerGroup = FindMovementGroupInSector( ubSectorX, (int)(ubSectorY+1), true );
        //                if( pPlayerGroup && AttemptToNoticeAdjacentGroupSucceeds() )
        //                {
        //                    HandlePlayerGroupNoticedByGarrison( pPlayerGroup, ubSectorID );
        //                    return;
        //                }
        //                else 
        //                */
        //                if (AdjacentSectorIsImportantAndUndefended((ubSectorID + 16)) && AttemptToNoticeEmptySectorSucceeds())
        //                {
        //                    HandleEmptySectorNoticedByGarrison(ubSectorID, (ubSectorID + 16));
        //                    return;
        //                }
        //            }
        //            if (ubSectorX > 1 && EnemyPermittedToAttackSector(null, (ubSectorID - 1)))
        //            {
        //                /*
        //                pPlayerGroup = FindMovementGroupInSector( (int)(ubSectorX-1), ubSectorY, true );
        //                if( pPlayerGroup && AttemptToNoticeAdjacentGroupSucceeds() )
        //                {
        //                    HandlePlayerGroupNoticedByGarrison( pPlayerGroup, ubSectorID );
        //                    return;
        //                }
        //                else 
        //                */
        //                if (AdjacentSectorIsImportantAndUndefended((ubSectorID - 1)) && AttemptToNoticeEmptySectorSucceeds())
        //                {
        //                    HandleEmptySectorNoticedByGarrison(ubSectorID, (ubSectorID - 1));
        //                    return;
        //                }
        //            }
        //        }
    }


    public static void RemoveGroupFromStrategicAILists(int ubGroupID)
    {
        for (int i = 0; i < giPatrolArraySize; i++)
        {
            if (gPatrolGroup[i].ubGroupID == ubGroupID)
            { //Patrol group was destroyed.
                gPatrolGroup[i].ubGroupID = 0;
                RecalculatePatrolWeight(i);
                return;
            }
            if (gPatrolGroup[i].ubPendingGroupID == ubGroupID)
            { //Group never arrived to reinforce.
                gPatrolGroup[i].ubPendingGroupID = 0;
                return;
            }
        }
        for (int i = 0; i < giGarrisonArraySize; i++)
        {
            if (gGarrisonGroup[i].ubPendingGroupID == ubGroupID)
            { //Group never arrived to reinforce.
                gGarrisonGroup[i].ubPendingGroupID = 0;
                return;
            }
        }
    }

    public static void RecalculatePatrolWeight(int iPatrolID)
    {
        GROUP? pGroup;
        int iWeight, iPrevWeight;
        int iNeedPopulation;

        ValidateWeights(4);

        //First, remove the previous weight from the applicable field.
        iPrevWeight = gPatrolGroup[iPatrolID].bWeight;
        if (iPrevWeight > 0)
        {
            giRequestPoints -= iPrevWeight;
        }

        if (gPatrolGroup[iPatrolID].ubGroupID > 0)
        {
            pGroup = StrategicMovement.GetGroup(gPatrolGroup[iPatrolID].ubGroupID);
            iNeedPopulation = gPatrolGroup[iPatrolID].bSize - pGroup.ubGroupSize;
            if (iNeedPopulation < 0)
            {
                gPatrolGroup[iPatrolID].bWeight = 0;
                ValidateWeights(27);
                return;
            }
        }
        else
        {
            iNeedPopulation = gPatrolGroup[iPatrolID].bSize;
        }
        iWeight = iNeedPopulation * 3 * gPatrolGroup[iPatrolID].bPriority / 96;
        iWeight = Math.Min(2, iWeight);
        gPatrolGroup[iPatrolID].bWeight = (int)iWeight;
        giRequestPoints += iWeight;

        ValidateWeights(5);
    }

    public static void RecalculateGarrisonWeight(int iGarrisonID)
    {
        SECTORINFO? pSector;
        int iWeight, iPrevWeight;
        int iDesiredPop, iCurrentPop, iPriority;

        ValidateWeights(6);

        pSector = SectorInfo[gGarrisonGroup[iGarrisonID].ubSectorID];
        iDesiredPop = gArmyComp[gGarrisonGroup[iGarrisonID].ubComposition].bDesiredPopulation;
        iCurrentPop = pSector.ubNumAdmins + pSector.ubNumTroops + pSector.ubNumElites;
        iPriority = gArmyComp[gGarrisonGroup[iGarrisonID].ubComposition].bPriority;

        //First, remove the previous weight from the applicable field.
        iPrevWeight = gGarrisonGroup[iGarrisonID].bWeight;
        if (iPrevWeight > 0)
        {
            giRequestPoints -= iPrevWeight;
        }
        else if (iPrevWeight < 0)
        {
            giReinforcementPoints += iPrevWeight;
        }

        //Calculate weight (range is -20 to +20 before multiplier).
        //The multiplier of 3 brings it to a range of -96 to +96 which is
        //close enough to a plus/minus 100%.  The resultant percentage is then
        //converted based on the priority.
        iWeight = (iDesiredPop - iCurrentPop) * 3;
        if (iWeight > 0)
        { //modify it by it's priority.
          //generates a value between 2 and 100
            iWeight = iWeight * iPriority / 96;
            iWeight = Math.Max(iWeight, 2);
            giRequestPoints += iWeight;
        }
        else if (iWeight < 0)
        { //modify it by it's reverse priority 
          //generates a value between -2 and -100
            iWeight = iWeight * (100 - iPriority) / 96;
            iWeight = Math.Min(iWeight, -2);
            giReinforcementPoints -= (int)iWeight;
        }

        gGarrisonGroup[iGarrisonID].bWeight = (int)iWeight;

        ValidateWeights(7);
    }

    public static void RecalculateSectorWeight(SEC ubSectorID)
    {
        for (int i = 0; i < giGarrisonArraySize; i++)
        {
            if (gGarrisonGroup[i].ubSectorID == ubSectorID)
            {
                RecalculateGarrisonWeight(i);
                return;
            }
        }
    }

    void RecalculateGroupWeight(GROUP? pGroup)
    {
        int i;
        for (i = 0; i < giPatrolArraySize; i++)
        {
            if (gPatrolGroup[i].ubGroupID == pGroup.ubGroupID)
            {
                if (pGroup.ubGroupSize == 0)
                {
                    this.TagSAIGroupWithGracePeriod(pGroup);
                    gPatrolGroup[i].ubGroupID = 0;
                }
                RecalculatePatrolWeight(i);
                return;
            }
        }

    }

    Garrisons ChooseSuitableGarrisonToProvideReinforcements(int iDstGarrisonID, int iReinforcementsRequested)
    {
        Garrisons iSrcGarrisonID, iBestGarrisonID = NO_GARRISON;
        int iReinforcementsAvailable;
        int iRandom, iWeight;
        Garrisons i;
        int bBestWeight;

        //Check to see if we could send reinforcements from Alma.  Only Drassen/Cambria get preferred
        //service from Alma, due to it's proximity and Alma's purpose as a forward military base.
        SEC ubSectorID = gGarrisonGroup[iDstGarrisonID].ubSectorID;
        switch (ubSectorID)
        {
            case SEC.B13:
            case SEC.C13:
            case SEC.D13:
            case SEC.D15:                               //Drassen + nearby SAM site
            case SEC.F8:
            case SEC.F9:
            case SEC.G8:
            case SEC.G9:
            case SEC.H8:    //Cambria
                            //reinforcements will be primarily sent from Alma whenever possible.

                //find which the first sector that contains Alma soldiers.
                for (i = 0; i < (Garrisons)giGarrisonArraySize; i++)
                {
                    if (gGarrisonGroup[(int)i].ubComposition == Garrisons.ALMA_DEFENCE)
                    {
                        break;
                    }
                }
                iSrcGarrisonID = i;
                //which of these 4 Alma garrisons have the most reinforcements available?  It is
                //possible that none of these garrisons can provide any reinforcements.
                bBestWeight = 0;
                for (i = iSrcGarrisonID; i < iSrcGarrisonID + 4; i++)
                {
                    RecalculateGarrisonWeight((int)i);
                    if (bBestWeight > gGarrisonGroup[(int)i].bWeight && StrategicAI.GarrisonCanProvideMinimumReinforcements((int)i))
                    {
                        bBestWeight = gGarrisonGroup[(int)i].bWeight;
                        iBestGarrisonID = i;
                    }
                }
                //If we can provide reinforcements from Alma, then make sure that it can provide at least 67% of
                //the requested reinforcements.
                if (bBestWeight < 0)
                {
                    iReinforcementsAvailable = ReinforcementsAvailable(iBestGarrisonID);
                    if (iReinforcementsAvailable * 100 >= iReinforcementsRequested * 67)
                    { //This is the approved group to provide the reinforcements.
                        return iBestGarrisonID;
                    }
                }
                break;
        }

        //The Alma case either wasn't applicable or failed to have the right reinforcements.  Do a general weighted search.
        iRandom = Globals.Random.Next(giReinforcementPoints);
        for (iSrcGarrisonID = 0; iSrcGarrisonID < (Garrisons)giGarrisonArraySize; iSrcGarrisonID++)
        { //go through the garrisons
            RecalculateGarrisonWeight((int)iSrcGarrisonID);
            iWeight = -gGarrisonGroup[(int)iSrcGarrisonID].bWeight;
            if (iWeight > 0)
            { //if group is able to provide reinforcements.
                if (iRandom < iWeight && GarrisonCanProvideMinimumReinforcements((int)iSrcGarrisonID))
                {
                    iReinforcementsAvailable = ReinforcementsAvailable(iSrcGarrisonID);
                    if (iReinforcementsAvailable * 100 >= iReinforcementsRequested * 67)
                    { //This is the approved group to provide the reinforcements.
                        return iSrcGarrisonID;
                    }
                }
                iRandom -= iWeight;
            }
        }

        //So far we have failed on all accounts.  Now, simply process all the garrisons, and return the first garrison that can 
        //provide the reinforcements.
        for (iSrcGarrisonID = 0; iSrcGarrisonID < (Garrisons)giGarrisonArraySize; iSrcGarrisonID++)
        { //go through the garrisons
            RecalculateGarrisonWeight((int)iSrcGarrisonID);
            iWeight = -gGarrisonGroup[(int)iSrcGarrisonID].bWeight;
            if (iWeight > 0 && GarrisonCanProvideMinimumReinforcements((int)iSrcGarrisonID))
            { //if group is able to provide reinforcements.
                iReinforcementsAvailable = ReinforcementsAvailable(iSrcGarrisonID);
                if (iReinforcementsAvailable * 100 >= iReinforcementsRequested * 67)
                { //This is the approved group to provide the reinforcements.
                    return iSrcGarrisonID;
                }
            }
        }

        //Well, if we get this far, the queen must be low on troops.  Send whatever we can.
        iRandom = Globals.Random.Next(giReinforcementPoints);
        for (iSrcGarrisonID = 0; iSrcGarrisonID < (Garrisons)giGarrisonArraySize; iSrcGarrisonID++)
        { //go through the garrisons
            RecalculateGarrisonWeight((int)iSrcGarrisonID);
            iWeight = -gGarrisonGroup[(int)iSrcGarrisonID].bWeight;
            if (iWeight > 0 && GarrisonCanProvideMinimumReinforcements((int)iSrcGarrisonID))
            { //if group is able to provide reinforcements.
                if (iRandom < iWeight)
                {
                    iReinforcementsAvailable = ReinforcementsAvailable(iSrcGarrisonID);
                    return iSrcGarrisonID;
                }
                iRandom -= iWeight;
            }
        }

        //Failed completely.
        return (Garrisons)(-1);
    }

    void SendReinforcementsForGarrison(int iDstGarrisonID, int usDefencePoints, GROUP? pOptionalGroup)
    {
        SECTORINFO? pSector;
        int iChance, iRandom;
        Garrisons iSrcGarrisonID;
        int iMaxReinforcementsAllowed, iReinforcementsAvailable, iReinforcementsRequested, iReinforcementsApproved;
        GROUP? pGroup;
        int ubSrcSectorX, ubDstSectorX;
        MAP_ROW ubSrcSectorY, ubDstSectorY;
        int? ubNumExtraReinforcements = null;
        int ubGroupSize;
        bool fLimitMaxTroopsAllowable = false;

        ValidateWeights(8);

        if (gGarrisonGroup[iDstGarrisonID].ubSectorID == SEC.B13 ||
              gGarrisonGroup[iDstGarrisonID].ubSectorID == SEC.C13 ||
              gGarrisonGroup[iDstGarrisonID].ubSectorID == SEC.D13)
        {
            pSector = null;
        }
        pSector = SectorInfo[gGarrisonGroup[iDstGarrisonID].ubSectorID];
        //Determine how many units the garrison needs.
        iReinforcementsRequested = this.GarrisonReinforcementsRequested((Garrisons)iDstGarrisonID, ubNumExtraReinforcements);

        //The maximum number of reinforcements can't be offsetted past a certain point based on the 
        //priority of the garrison.
        iMaxReinforcementsAllowed = //from 1 to 3 times the desired size of the normal force.
            gArmyComp[gGarrisonGroup[iDstGarrisonID].ubComposition].bDesiredPopulation +
            gArmyComp[gGarrisonGroup[iDstGarrisonID].ubComposition].bDesiredPopulation *
            gArmyComp[gGarrisonGroup[iDstGarrisonID].ubComposition].bPriority / 50;

        if (iReinforcementsRequested + ubNumExtraReinforcements > iMaxReinforcementsAllowed)
        { //adjust the extra reinforcements so that it doesn't exceed the maximum allowed.
            fLimitMaxTroopsAllowable = true;
            ubNumExtraReinforcements = (int)(iMaxReinforcementsAllowed - iReinforcementsRequested);
        }

        //        iReinforcementsRequested += ubNumExtraReinforcements;

        if (iReinforcementsRequested <= 0)
        {
            ValidateWeights(9);
            return;
        }

        ubDstSectorX = SECTORINFO.SECTORX(gGarrisonGroup[iDstGarrisonID].ubSectorID);
        ubDstSectorY = SECTORINFO.SECTORY(gGarrisonGroup[iDstGarrisonID].ubSectorID);

        if (pOptionalGroup is not null && pOptionalGroup is not null)
        { //This group will provide the reinforcements
            pGroup = pOptionalGroup;

            //            gGarrisonGroup[iDstGarrisonID].ubPendingGroupID = pGroup.ubGroupID;
            //            ConvertGroupTroopsToComposition(pGroup, gGarrisonGroup[iDstGarrisonID].ubComposition);
            //            MoveSAIGroupToSector(pOptionalGroup, gGarrisonGroup[iDstGarrisonID].ubSectorID, STAGE, REINFORCEMENTS);

            ValidateWeights(10);

            return;
        }
        iRandom = Globals.Random.Next(giReinforcementPoints + giReinforcementPool);
        if (iRandom < giReinforcementPool)
        { //use the pool and send the requested amount from SECTOR P3 (queen's palace)
        QUEEN_POOL:

            //KM : Sep 9, 1999
            //If the player owns sector P3, any troops that spawned there were causing serious problems, seeing battle checks
            //were not performed!
            if (!strategicMap[CALCULATE_STRATEGIC_INDEX(3, (MAP_ROW)16)].fEnemyControlled)
            { //Queen can no longer send reinforcements from the palace if she doesn't control it!
                return;
            }


            //            if (!giReinforcementPool)
            //            {
            //                ValidateWeights(11);
            //                return;
            //            }
            iReinforcementsApproved = Math.Min(iReinforcementsRequested, giReinforcementPool);

            if (iReinforcementsApproved * 3 < usDefencePoints)
            { //The enemy force that would be sent would likely be decimated by the player forces.
                gubGarrisonReinforcementsDenied[(Garrisons)iDstGarrisonID] += (int)(gArmyComp[gGarrisonGroup[iDstGarrisonID].ubComposition].bPriority / 2);
                ValidateWeights(12);
                return;
            }
            else
            {
                //The force is strong enough to be able to take the sector.
                gubGarrisonReinforcementsDenied[(Garrisons)iDstGarrisonID] = 0;
            }

            //The chance she will send them is related with the strength difference between the
            //player's force and the queen's.
            //            if (ubNumExtraReinforcements && fLimitMaxTroopsAllowable && iReinforcementsApproved == iMaxReinforcementsAllowed)
            //            {
            //                iChance = (iReinforcementsApproved + ubNumExtraReinforcements) * 100 / usDefencePoints;
            //                if (!Chance(iChance))
            //                {
            //                    ValidateWeights(13);
            //                    return;
            //                }
            //            }

            pGroup = StrategicMovement.CreateNewEnemyGroupDepartingFromSector(SEC.P3, 0, (int)iReinforcementsApproved, 0);
            //            ConvertGroupTroopsToComposition(pGroup, gGarrisonGroup[iDstGarrisonID].ubComposition);
            //            pGroup.ubOriginalSector = SECTORINFO.SECTOR(ubDstSectorX, ubDstSectorY);
            //            giReinforcementPool -= iReinforcementsApproved;
            //            pGroup.ubMoveType = ONE_WAY;
            //            gGarrisonGroup[iDstGarrisonID].ubPendingGroupID = pGroup.ubGroupID;
            //
            //            ubGroupSize = (int)(pGroup.pEnemyGroup.ubNumTroops + pGroup.pEnemyGroup.ubNumElites + pGroup.pEnemyGroup.ubNumAdmins);
            //
            //            if (ubNumExtraReinforcements)
            //            {
            //                MoveSAIGroupToSector(pGroup, gGarrisonGroup[iDstGarrisonID].ubSectorID, STAGE, STAGING);
            //            }
            //            else
            //            {
            //                MoveSAIGroupToSector(pGroup, gGarrisonGroup[iDstGarrisonID].ubSectorID, STAGE, REINFORCEMENTS);
            //            }
            ValidateWeights(14);
            return;
        }
        else
        {
            iSrcGarrisonID = this.ChooseSuitableGarrisonToProvideReinforcements(iDstGarrisonID, iReinforcementsRequested);
            if (iSrcGarrisonID == Garrisons.UNSET)
            {
                ValidateWeights(15);
                //                goto QUEEN_POOL;
            }

            //            ubSrcSectorX = (gGarrisonGroup[iSrcGarrisonID].ubSectorID % 16) + 1;
            //            ubSrcSectorY = (gGarrisonGroup[iSrcGarrisonID].ubSectorID / 16) + 1;
            //            if (ubSrcSectorX != gWorldSectorX || ubSrcSectorY != gWorldSectorY || gbWorldSectorZ > 0)
            { //The reinforcements aren't coming from the currently loaded sector!
                iReinforcementsAvailable = ReinforcementsAvailable(iSrcGarrisonID);
                if (iReinforcementsAvailable <= 0)
                {
                    //                    SAIReportError("Attempting to send reinforcements from a garrison that doesn't have any! -- KM:0 (with prior saved game and strategic decisions.txt)");
                    return;
                }
                //Send the lowest of the two:  number requested or number available

                iReinforcementsApproved = Math.Min(iReinforcementsRequested, iReinforcementsAvailable);
                if (iReinforcementsApproved > iMaxReinforcementsAllowed - ubNumExtraReinforcements)
                { //The force isn't strong enough, but the queen isn't willing to apply extra resources
                  //                    iReinforcementsApproved = iMaxReinforcementsAllowed - ubNumExtraReinforcements;
                }
                else if ((iReinforcementsApproved + ubNumExtraReinforcements) * 3 < usDefencePoints)
                { //The enemy force that would be sent would likely be decimated by the player forces.
                    gubGarrisonReinforcementsDenied[(Garrisons)iDstGarrisonID] += (int)(gArmyComp[gGarrisonGroup[iDstGarrisonID].ubComposition].bPriority / 2);
                    ValidateWeights(17);
                    return;
                }
                else
                {
                    //The force is strong enough to be able to take the sector.
                    gubGarrisonReinforcementsDenied[(Garrisons)iDstGarrisonID] = 0;
                }

                //The chance she will send them is related with the strength difference between the
                //player's force and the queen's.
                if (iReinforcementsApproved + ubNumExtraReinforcements == iMaxReinforcementsAllowed && usDefencePoints > 0)
                {
                    //                    iChance = (iReinforcementsApproved + ubNumExtraReinforcements) * 100 / usDefencePoints;
                    //                    if (!Chance(iChance))
                    //                    {
                    //                        ValidateWeights(18);
                    //                        return;
                    //                    }
                }

                pGroup = StrategicMovement.CreateNewEnemyGroupDepartingFromSector(gGarrisonGroup[(int)iSrcGarrisonID].ubSectorID, 0, (int)iReinforcementsApproved, 0);
                //                ConvertGroupTroopsToComposition(pGroup, gGarrisonGroup[iDstGarrisonID].ubComposition);
                RemoveSoldiersFromGarrisonBasedOnComposition((int)iSrcGarrisonID, pGroup.ubGroupSize);
                pGroup.ubOriginalSector = SECTORINFO.SECTOR(ubDstSectorX, ubDstSectorY);
                //                pGroup.ubMoveType = ONE_WAY;
                gGarrisonGroup[iDstGarrisonID].ubPendingGroupID = pGroup.ubGroupID;
                //                ubGroupSize = (int)(pGroup.pEnemyGroup.ubNumTroops + pGroup.pEnemyGroup.ubNumElites + pGroup.pEnemyGroup.ubNumAdmins);

                //                if (ubNumExtraReinforcements)
                //                {
                //                    pGroup.pEnemyGroup.ubPendingReinforcements = ubNumExtraReinforcements;
                //
                //                    MoveSAIGroupToSector(pGroup, gGarrisonGroup[iDstGarrisonID].ubSectorID, STAGE, STAGING);
                //                }
                //                else
                //                {
                //                    MoveSAIGroupToSector(pGroup, gGarrisonGroup[iDstGarrisonID].ubSectorID, STAGE, REINFORCEMENTS);
                //                }

                ValidateWeights(19);
                return;
            }
        }
        ValidateWeights(20);
    }

    public static void SendReinforcementsForPatrol(int iPatrolID, GROUP? pOptionalGroup)
    {
        GROUP? pGroup;
        int iRandom, iWeight;
        Garrisons iSrcGarrisonID;
        int iReinforcementsAvailable, iReinforcementsRequested, iReinforcementsApproved;
        int ubSrcSectorX, ubDstSectorX = 0;
        MAP_ROW ubDstSectorY = 0, ubSrcSectorY;

        ValidateWeights(21);

        //Determine how many units the patrol group needs.
        iReinforcementsRequested = PatrolReinforcementsRequested(iPatrolID);

        if (iReinforcementsRequested <= 0)
        {
            return;
        }

        //        ubDstSectorX = (gPatrolGroup[iPatrolID].ubSectorID[1] % 16) + 1;
        //        ubDstSectorY = (gPatrolGroup[iPatrolID].ubSectorID[1] / 16) + 1;

        if (pOptionalGroup is not null && pOptionalGroup is not null)
        { //This group will provide the reinforcements
            pGroup = pOptionalGroup;

            gPatrolGroup[iPatrolID].ubPendingGroupID = pGroup.ubGroupID;

            //            MoveSAIGroupToSector(pOptionalGroup, gPatrolGroup[iPatrolID].ubSectorID[1], EVASIVE, REINFORCEMENTS);

            ValidateWeights(22);
            return;
        }
        iRandom = Globals.Random.Next(giReinforcementPoints + giReinforcementPool);
        if (iRandom < giReinforcementPool)
        { //use the pool and send the requested amount from SECTOR P3 (queen's palace)
            iReinforcementsApproved = Math.Min(iReinforcementsRequested, giReinforcementPool);
            if (iReinforcementsApproved == 0)
            {
                iReinforcementsApproved = iReinforcementsApproved;
            }
            pGroup = StrategicMovement.CreateNewEnemyGroupDepartingFromSector(SEC.P3, 0, (int)iReinforcementsApproved, 0);
            pGroup.ubOriginalSector = SECTORINFO.SECTOR(ubDstSectorX, ubDstSectorY);
            giReinforcementPool -= iReinforcementsApproved;

            gPatrolGroup[iPatrolID].ubPendingGroupID = pGroup.ubGroupID;

            MoveSAIGroupToSector(pGroup, gPatrolGroup[iPatrolID].ubSectorID[1], SAIMOVECODE.EVASIVE, ENEMY_INTENTIONS.REINFORCEMENTS);

            ValidateWeights(23);
            return;
        }
        else
        {
            iRandom -= giReinforcementPool;
            for (iSrcGarrisonID = 0; iSrcGarrisonID < (Garrisons)giGarrisonArraySize; iSrcGarrisonID++)
            { //go through the garrisons
                RecalculateGarrisonWeight((int)iSrcGarrisonID);
                iWeight = -gGarrisonGroup[(int)iSrcGarrisonID].bWeight;
                if (iWeight > 0)
                { //if group is able to provide reinforcements.
                    if (iRandom < iWeight)
                    { //This is the group that gets the reinforcements!
                        ubSrcSectorX = SECTORINFO.SECTORX(gGarrisonGroup[(int)iSrcGarrisonID].ubSectorID);
                        ubSrcSectorY = SECTORINFO.SECTORY(gGarrisonGroup[(int)iSrcGarrisonID].ubSectorID);
                        if (ubSrcSectorX != gWorldSectorX || ubSrcSectorY != gWorldSectorY || gbWorldSectorZ > 0)
                        { //The reinforcements aren't coming from the currently loaded sector!
                            iReinforcementsAvailable = ReinforcementsAvailable(iSrcGarrisonID);
                            //Send the lowest of the two:  number requested or number available
                            iReinforcementsApproved = Math.Min(iReinforcementsRequested, iReinforcementsAvailable);
                            pGroup = StrategicMovement.CreateNewEnemyGroupDepartingFromSector(gGarrisonGroup[(int)iSrcGarrisonID].ubSectorID, 0, (int)iReinforcementsApproved, 0);
                            pGroup.ubOriginalSector = SECTORINFO.SECTOR(ubDstSectorX, ubDstSectorY);
                            gPatrolGroup[iPatrolID].ubPendingGroupID = pGroup.ubGroupID;

                            RemoveSoldiersFromGarrisonBasedOnComposition((int)iSrcGarrisonID, pGroup.ubGroupSize);

                            MoveSAIGroupToSector(pGroup, gPatrolGroup[iPatrolID].ubSectorID[1], SAIMOVECODE.EVASIVE, ENEMY_INTENTIONS.REINFORCEMENTS);

                            ValidateWeights(24);

                            return;
                        }
                    }
                    iRandom -= iWeight;
                }
            }
        }
        ValidateWeights(25);
    }

    //Periodically does a general poll and check on each of the groups and garrisons, determines
    //reinforcements, new patrol groups, planned assaults, etc.
    public static void EvaluateQueenSituation()
    {
        int i, iRandom;
        int iWeight;
        int uiOffset = 0;
        int usDefencePoints;
        int iOrigRequestPoints;
        int iSumOfAllWeights = 0;

        ValidateWeights(26);

        // figure out how long it shall be before we call this again

        // The more work to do there is (request points the queen's army is asking for), the more often she will make decisions
        // This can increase the decision intervals by up to 500 extra minutes (> 8 hrs)
        //        uiOffset = (uint)Math.Max(100 - giRequestPoints, 0);
        uiOffset = uiOffset + Globals.Random.Next(uiOffset * 4);
        switch (gGameOptions.ubDifficultyLevel)
        {
            case DifficultyLevel.Easy:
                uiOffset += EASY_TIME_EVALUATE_IN_MINUTES + Globals.Random.Next(EASY_TIME_EVALUATE_VARIANCE);
                break;
            case DifficultyLevel.Medium:
                uiOffset += NORMAL_TIME_EVALUATE_IN_MINUTES + Globals.Random.Next(NORMAL_TIME_EVALUATE_VARIANCE);
                break;
            case DifficultyLevel.Hard:
                uiOffset += HARD_TIME_EVALUATE_IN_MINUTES + Globals.Random.Next(HARD_TIME_EVALUATE_VARIANCE);
                break;
        }

        if (giReinforcementPool == 0)
        {
            //Queen has run out of reinforcements.  Simulate recruiting and training new troops
            uiOffset *= 10;
            giReinforcementPool += 30;
            GameEvents.AddStrategicEvent(EVENT.EVALUATE_QUEEN_SITUATION, GameClock.GetWorldTotalMin() + (uint)uiOffset, 0);
            return;
        }

        //Re-post the event
        GameEvents.AddStrategicEvent(EVENT.EVALUATE_QUEEN_SITUATION, GameClock.GetWorldTotalMin() + (uint)uiOffset, 0);

        // if the queen hasn't been alerted to player's presence yet
        if (!gfQueenAIAwake)
        { //no decisions can be made yet.
            return;
        }

        // Adjust queen's disposition based on player's progress
        EvolveQueenPriorityPhase(false);

        // Gradually promote any remaining admins into troops 
        UpgradeAdminsToTroops();

        if ((giRequestPoints <= 0) || ((giReinforcementPoints <= 0) && (giReinforcementPool <= 0)))
        { //we either have no reinforcements or request for reinforcements.
            return;
        }

        //now randomly choose who gets the reinforcements.
        // giRequestPoints is the combined sum of all the individual weights of all garrisons and patrols requesting reinforcements
        iRandom = Globals.Random.Next(giRequestPoints);

        iOrigRequestPoints = giRequestPoints;   // debug only!

        //go through garrisons first
        //        for (i = 0; i < giGarrisonArraySize; i++)
        //        {
        //            RecalculateGarrisonWeight(i);
        //            iWeight = gGarrisonGroup[i].bWeight;
        //            if (iWeight > 0)
        //            {   //if group is requesting reinforcements.
        //
        //                iSumOfAllWeights += iWeight;    // debug only!
        //
        //                if (iRandom < iWeight && !gGarrisonGroup[i].ubPendingGroupID &&
        //                        EnemyPermittedToAttackSector(null, gGarrisonGroup[i].ubSectorID) &&
        //                        GarrisonRequestingMinimumReinforcements(i))
        //                { //This is the group that gets the reinforcements!
        //                    if (ReinforcementsApproved(i, usDefencePoints))
        //                    {
        //                        SendReinforcementsForGarrison(i, usDefencePoints, null);
        //                    }
        //                    else
        //                    {
        //                    }
        //                    return;
        //                }
        //                iRandom -= iWeight;
        //            }
        //        }

        //go through the patrol groups
        for (i = 0; i < giPatrolArraySize; i++)
        {
            RecalculatePatrolWeight(i);
            iWeight = gPatrolGroup[i].bWeight;
            if (iWeight > 0)
            {
                iSumOfAllWeights += iWeight;    // debug only!

                //                if (iRandom < iWeight && gPatrolGroup[i].ubPendingGroupID == 0 && PatrolRequestingMinimumReinforcements(i))
                //                { //This is the group that gets the reinforcements!
                //                    SendReinforcementsForPatrol(i, null);
                //                    return;
                //                }
                iRandom -= iWeight;
            }
        }

        ValidateWeights(27);
    }


    bool SaveStrategicAI(Stream hFile)
    {
        GARRISON_GROUP gTempGarrisonGroup;
        PATROL_GROUP gTempPatrolGroup;
        ARMY_COMPOSITION gTempArmyComp;
        int i;

        //memset(&gTempPatrolGroup, 0, sizeof(PATROL_GROUP));
        //memset(&gTempArmyComp, 0, sizeof(ARMY_COMPOSITION));

        files.FileWrite(hFile, gbPadding2, 3, out int uiNumBytesWritten);
        if (uiNumBytesWritten != 3)
        {
            return false;
        }

        files.FileWrite(hFile, gfExtraElites, 1, out uiNumBytesWritten);
        if (uiNumBytesWritten != 1)
        {
            return false;
        }

        files.FileWrite(hFile, giGarrisonArraySize, 4, out uiNumBytesWritten);
        if (uiNumBytesWritten != 4)
        {
            return false;
        }

        files.FileWrite(hFile, giPatrolArraySize, 4, out uiNumBytesWritten);
        if (uiNumBytesWritten != 4)
        {
            return false;
        }

        files.FileWrite(hFile, giReinforcementPool, 4, out uiNumBytesWritten);
        if (uiNumBytesWritten != 4)
        {
            return false;
        }

        files.FileWrite(hFile, giForcePercentage, 4, out uiNumBytesWritten);
        if (uiNumBytesWritten != 4)
        {
            return false;
        }

        files.FileWrite(hFile, giArmyAlertness, 4, out uiNumBytesWritten);
        if (uiNumBytesWritten != 4)
        {
            return false;
        }

        files.FileWrite(hFile, giArmyAlertnessDecay, 4, out uiNumBytesWritten);
        if (uiNumBytesWritten != 4)
        {
            return false;
        }

        files.FileWrite(hFile, gfQueenAIAwake, 1, out uiNumBytesWritten);
        if (uiNumBytesWritten != 1)
        {
            return false;
        }

        files.FileWrite(hFile, giReinforcementPoints, 4, out uiNumBytesWritten);
        if (uiNumBytesWritten != 4)
        {
            return false;
        }

        files.FileWrite(hFile, giRequestPoints, 4, out uiNumBytesWritten);
        if (uiNumBytesWritten != 4)
        {
            return false;
        }

        files.FileWrite(hFile, gubNumAwareBattles, 1, out uiNumBytesWritten);
        if (uiNumBytesWritten != 1)
        {
            return false;
        }

        files.FileWrite(hFile, gubSAIVersion, 1, out uiNumBytesWritten);
        if (uiNumBytesWritten != 1)
        {
            return false;
        }

        files.FileWrite(hFile, gubQueenPriorityPhase, 1, out uiNumBytesWritten);
        if (uiNumBytesWritten != 1)
        {
            return false;
        }

        files.FileWrite(hFile, gfFirstBattleMeanwhileScenePending, 1, out uiNumBytesWritten);
        if (uiNumBytesWritten != 1)
        {
            return false;
        }

        files.FileWrite(hFile, gfMassFortificationOrdered, 1, out uiNumBytesWritten);
        if (uiNumBytesWritten != 1)
        {
            return false;
        }

        files.FileWrite(hFile, gubMinEnemyGroupSize, 1, out uiNumBytesWritten);
        if (uiNumBytesWritten != 1)
        {
            return false;
        }

        files.FileWrite(hFile, gubHoursGracePeriod, 1, out uiNumBytesWritten);
        if (uiNumBytesWritten != 1)
        {
            return false;
        }

        files.FileWrite(hFile, gusPlayerBattleVictories, 2, out uiNumBytesWritten);
        if (uiNumBytesWritten != 2)
        {
            return false;
        }

        files.FileWrite(hFile, gfUseAlternateQueenPosition, 1, out uiNumBytesWritten);
        if (uiNumBytesWritten != 1)
        {
            return false;
        }

        files.FileWrite(hFile, gbPadding, SAI_PADDING_BYTES, out uiNumBytesWritten);
        if (uiNumBytesWritten != SAI_PADDING_BYTES)
        {
            return false;
        }
        //Save the army composition (which does get modified)
        //        files.FileWrite(hFile, gArmyComp, NUM_ARMY_COMPOSITIONS * sizeof(ARMY_COMPOSITION), out uiNumBytesWritten);
        //        if (uiNumBytesWritten != NUM_ARMY_COMPOSITIONS * sizeof(ARMY_COMPOSITION))
        //        {
        //            return false;
        //        }

        i = SAVED_ARMY_COMPOSITIONS - NUM_ARMY_COMPOSITIONS;
        while (i-- > 0)
        {
            //            files.files.FileWrite(hFile, gTempArmyComp, sizeof(ARMY_COMPOSITION), out uiNumBytesWritten);
            //            if (uiNumBytesWritten != sizeof(ARMY_COMPOSITION))
            //            {
            //                return false;
            //            }
        }

        //Save the patrol group definitions
        //        files.FileWrite(hFile, gPatrolGroup, giPatrolArraySize * sizeof(PATROL_GROUP), out uiNumBytesWritten);
        //        if (uiNumBytesWritten != giPatrolArraySize * sizeof(PATROL_GROUP))
        //        {
        //            return false;
        //        }

        i = SAVED_PATROL_GROUPS - giPatrolArraySize;
        while (i-- > 0)
        {
            //            files.FileWrite(hFile, gTempPatrolGroup, sizeof(PATROL_GROUP), out uiNumBytesWritten);
            //            if (uiNumBytesWritten != sizeof(PATROL_GROUP))
            //            {
            //                return false;
            //            }
        }
        //Save the garrison information!
        //memset(&gTempGarrisonGroup, 0, sizeof(GARRISON_GROUP));
        //        files.FileWrite(hFile, gGarrisonGroup, giGarrisonArraySize * sizeof(GARRISON_GROUP), out uiNumBytesWritten);
        //        if (uiNumBytesWritten != giGarrisonArraySize * sizeof(GARRISON_GROUP))
        //        {
        //            return false;
        //        }

        //        i = SAVED_GARRISON_GROUPS - giGarrisonArraySize;
        while (i-- > 0)
        {
            //            files.FileWrite(hFile, gTempGarrisonGroup, sizeof(GARRISON_GROUP), out uiNumBytesWritten);
            //            if (uiNumBytesWritten != sizeof(GARRISON_GROUP))
            //            {
            //                return false;
            //            }
        }

        files.FileWrite(hFile, gubPatrolReinforcementsDenied, giPatrolArraySize, out uiNumBytesWritten);
        if (uiNumBytesWritten != (int)giPatrolArraySize)
        {
            return false;
        }

        //        files.FileWrite(hFile, gubGarrisonReinforcementsDenied, giGarrisonArraySize, out uiNumBytesWritten);
        //        if (uiNumBytesWritten != (int)giGarrisonArraySize)
        //        {
        //            return false;
        //        }

        return true;
    }

    public static bool LoadStrategicAI(Stream hFile)
    {
        GROUP? pGroup, next;
        GARRISON_GROUP gTempGarrisonGroup;
        PATROL_GROUP gTempPatrolGroup;
        ARMY_COMPOSITION gTempArmyComp;
        int i;
        int ubSAIVersion = 0;

        files.FileRead(hFile, ref gbPadding2, 3, out int uiNumBytesRead);
        if (uiNumBytesRead != 3)
        {
            return false;
        }

        files.FileRead(hFile, ref gfExtraElites, 1, out uiNumBytesRead);
        if (uiNumBytesRead != 1)
        {
            return false;
        }

        files.FileRead(hFile, ref giGarrisonArraySize, 4, out uiNumBytesRead);
        if (uiNumBytesRead != 4)
        {
            return false;
        }

        files.FileRead(hFile, ref giPatrolArraySize, 4, out uiNumBytesRead);
        if (uiNumBytesRead != 4)
        {
            return false;
        }

        files.FileRead(hFile, ref giReinforcementPool, 4, out uiNumBytesRead);
        if (uiNumBytesRead != 4)
        {
            return false;
        }

        files.FileRead(hFile, ref giForcePercentage, 4, out uiNumBytesRead);
        if (uiNumBytesRead != 4)
        {
            return false;
        }

        files.FileRead(hFile, ref giArmyAlertness, 4, out uiNumBytesRead);
        if (uiNumBytesRead != 4)
        {
            return false;
        }

        files.FileRead(hFile, ref giArmyAlertnessDecay, 4, out uiNumBytesRead);
        if (uiNumBytesRead != 4)
        {
            return false;
        }

        files.FileRead(hFile, ref gfQueenAIAwake, 1, out uiNumBytesRead);
        if (uiNumBytesRead != 1)
        {
            return false;
        }

        files.FileRead(hFile, ref giReinforcementPoints, 4, out uiNumBytesRead);
        if (uiNumBytesRead != 4)
        {
            return false;
        }

        files.FileRead(hFile, ref giRequestPoints, 4, out uiNumBytesRead);
        if (uiNumBytesRead != 4)
        {
            return false;
        }

        files.FileRead(hFile, ref gubNumAwareBattles, 1, out uiNumBytesRead);
        if (uiNumBytesRead != 1)
        {
            return false;
        }

        files.FileRead(hFile, ref ubSAIVersion, 1, out uiNumBytesRead);
        if (uiNumBytesRead != 1)
        {
            return false;
        }

        files.FileRead(hFile, ref gubQueenPriorityPhase, 1, out uiNumBytesRead);
        if (uiNumBytesRead != 1)
        {
            return false;
        }

        //        files.FileRead(hFile, ref gfFirstBattleMeanwhileScenePending, 1, out uiNumBytesRead);
        //        if (uiNumBytesRead != 1)
        //        {
        //            return false;
        //        }

        files.FileRead(hFile, ref gfMassFortificationOrdered, 1, out uiNumBytesRead);
        if (uiNumBytesRead != 1)
        {
            return false;
        }

        files.FileRead(hFile, ref gubMinEnemyGroupSize, 1, out uiNumBytesRead);
        if (uiNumBytesRead != 1)
        {
            return false;
        }

        files.FileRead(hFile, ref gubHoursGracePeriod, 1, out uiNumBytesRead);
        if (uiNumBytesRead != 1)
        {
            return false;
        }

        files.FileRead(hFile, ref gusPlayerBattleVictories, 2, out uiNumBytesRead);
        if (uiNumBytesRead != 2)
        {
            return false;
        }

        files.FileRead(hFile, ref gfUseAlternateQueenPosition, 1, out uiNumBytesRead);
        if (uiNumBytesRead != 1)
        {
            return false;
        }

        files.FileRead(hFile, ref gbPadding, SAI_PADDING_BYTES, out uiNumBytesRead);
        if (uiNumBytesRead != SAI_PADDING_BYTES)
        {
            return false;
        }

        //Restore the army composition 
        //        files.FileRead(hFile, gArmyComp, NUM_ARMY_COMPOSITIONS * sizeof(ARMY_COMPOSITION), out uiNumBytesRead);
        //        if (uiNumBytesRead != NUM_ARMY_COMPOSITIONS * sizeof(ARMY_COMPOSITION))
        //        {
        //            return false;
        //        }

        i = SAVED_ARMY_COMPOSITIONS - NUM_ARMY_COMPOSITIONS;
        //        while (i-- > 0)
        {
            //            files.FileRead(hFile, gTempArmyComp, sizeof(ARMY_COMPOSITION), out uiNumBytesRead);
            //            if (uiNumBytesRead != sizeof(ARMY_COMPOSITION))
            //            {
            //                return false;
            //            }
        }

        //Restore the patrol group definitions
        //        if (gPatrolGroup)
        //        {
        //            MemFree(gPatrolGroup);
        //        }

        //        gPatrolGroup = (PATROL_GROUP?)MemAlloc(giPatrolArraySize * sizeof(PATROL_GROUP));
        //        files.FileRead(hFile, gPatrolGroup, giPatrolArraySize * sizeof(PATROL_GROUP), out uiNumBytesRead);
        //
        //        if (uiNumBytesRead != giPatrolArraySize * sizeof(PATROL_GROUP))
        {
            //            return false;
        }

        i = SAVED_PATROL_GROUPS - giPatrolArraySize;
        //        while (i-- > 0)
        //        {
        //            //            files.FileRead(hFile, gTempPatrolGroup, sizeof(PATROL_GROUP), out uiNumBytesRead);
        //            //            if (uiNumBytesRead != sizeof(PATROL_GROUP))
        //            {
        //                return false;
        //            }
        //        }

        gubSAIVersion = SAI_VERSION;
        //Load the garrison information!
        //        if (gGarrisonGroup)
        {
            MemFree(gGarrisonGroup);
        }

        //        gGarrisonGroup = (GARRISON_GROUP?)MemAlloc(giGarrisonArraySize * sizeof(GARRISON_GROUP));
        //        files.FileRead(hFile, gGarrisonGroup, giGarrisonArraySize * sizeof(GARRISON_GROUP), &uiNumBytesRead);
        //        if (uiNumBytesRead != giGarrisonArraySize * sizeof(GARRISON_GROUP))
        {
            //            return false;
        }
        //        i = SAVED_GARRISON_GROUPS - giGarrisonArraySize;
        //        while (i-- > 0)
        //        {
        //            //            files.FileRead(hFile, gTempGarrisonGroup, sizeof(GARRISON_GROUP), out uiNumBytesRead);
        //            //            if (uiNumBytesRead != sizeof(GARRISON_GROUP))
        //            {
        //                return false;
        //            }
        //        }

        //Load the list of reinforcement patrol points.
        //        if (gubPatrolReinforcementsDenied)
        {
            MemFree(gubPatrolReinforcementsDenied);
            gubPatrolReinforcementsDenied = null;
        }
        //        gubPatrolReinforcementsDenied = (int?)MemAlloc(giPatrolArraySize);
        //        files.FileRead(hFile, gubPatrolReinforcementsDenied, giPatrolArraySize, out uiNumBytesRead);
        if (uiNumBytesRead != (int)giPatrolArraySize)
        {
            //            return false;
        }

        //Load the list of reinforcement garrison points.
        //        if (gubGarrisonReinforcementsDenied)
        {
            MemFree(gubGarrisonReinforcementsDenied);
            gubGarrisonReinforcementsDenied = null;
        }
        //        gubGarrisonReinforcementsDenied = (int?)MemAlloc(giGarrisonArraySize);
        //        files.FileRead(hFile, gubGarrisonReinforcementsDenied, giGarrisonArraySize, out uiNumBytesRead);
        if (uiNumBytesRead != (int)giGarrisonArraySize)
        {
            return false;
        }

        if (ubSAIVersion < 6)
        { //Reinitialize the costs since they have changed.

            //Recreate the compositions
            //            memcpy(gArmyComp, gOrigArmyComp, NUM_ARMY_COMPOSITIONS * sizeof(ARMY_COMPOSITION));
            EvolveQueenPriorityPhase(true);

            //Recreate the patrol desired sizes
            for (i = 0; i < giPatrolArraySize; i++)
            {
                gPatrolGroup[i].bSize = gOrigPatrolGroup[i].bSize;
            }
        }
        if (ubSAIVersion < 7)
        {
            //            BuildUndergroundSectorInfoList();
        }
        if (ubSAIVersion < 8)
        {
            //            ReinitializeUnvisitedGarrisons();
        }
        if (ubSAIVersion < 10)
        {
            for (i = 0; i < giPatrolArraySize; i++)
            {
                if (gPatrolGroup[i].bSize >= 16)
                {
                    gPatrolGroup[i].bSize = 10;
                }
            }
            //            pGroup = gpGroupList;
            //            while (pGroup)
            {
                //                if (pGroup.fPlayer && pGroup.ubGroupSize >= 16)
                //                { //accident in patrol groups being too large
                //                    int ubGetRidOfXTroops = pGroup.ubGroupSize - 10;
                //                    if (gbWorldSectorZ > 0 || pGroup.ubSectorX != gWorldSectorX || pGroup.ubSectorY != gWorldSectorY)
                //                    { //don't modify groups in the currently loaded sector.
                //                      //                        if (pGroup.pEnemyGroup.ubNumTroops >= ubGetRidOfXTroops)
                //                      //                        {
                //                      //                            pGroup.pEnemyGroup.ubNumTroops -= ubGetRidOfXTroops;
                //                      //                            pGroup.ubGroupSize -= ubGetRidOfXTroops;
                //                      //                        }
                //                      //                    }
                //                    }
                //                    pGroup = pGroup.next;
                //                }
            }
        }

        if (ubSAIVersion < 13)
        {
            for (i = 0; i < 255; i++)
            {
                //                SectorInfo[i].bBloodCatPlacements = 0;
                //                SectorInfo[i].bBloodCats = -1;
            }
            //            InitBloodCatSectors();
            //This info is used to clean up the two new codes inserted into the
            //middle of the enumeration for battle codes.
            if (gubEnemyEncounterCode > ENCOUNTER_CODE.CREATURE_ATTACK_CODE)
            {
                gubEnemyEncounterCode += 2;
            }
            if (gubExplicitEnemyEncounterCode > ENCOUNTER_CODE.CREATURE_ATTACK_CODE)
            {
                gubExplicitEnemyEncounterCode += 2;
            }

        }
        if (ubSAIVersion < 14)
        {
            UNDERGROUND_SECTORINFO? pSector = null;
            //            pSector = FindUnderGroundSector(4, 11, 1);
            if (pSector.ubNumTroops + pSector.ubNumElites > 20)
            {
                pSector.ubNumTroops -= 2;
            }
            //            pSector = FindUnderGroundSector(3, 15, 1);
            if (pSector.ubNumTroops + pSector.ubNumElites > 20)
            {
                pSector.ubNumTroops -= 2;
            }
        }
        if (ubSAIVersion < 16)
        {
            UNDERGROUND_SECTORINFO? pSector = null;
            //            pSector = FindUnderGroundSector(3, 15, 1);
            //            if (pSector)
            //            {
            //                pSector.ubAdjacentSectors |= SOUTH_ADJACENT_SECTOR;
            //            }
            //            pSector = FindUnderGroundSector(3, 16, 1);
            //            if (pSector)
            //            {
            //                pSector.ubAdjacentSectors |= NORTH_ADJACENT_SECTOR;
            //            }
        }
        if (ubSAIVersion < 17)
        { //Patch all groups that have this flag set
            gubNumGroupsArrivedSimultaneously = 0;
            {
                //                    pGroup = gpGroupList;
                //                    while (pGroup)
                //                    {
                //                        if (pGroup.uiFlags & GROUPFLAG_GROUP_ARRIVED_SIMULTANEOUSLY)
                //                        {
                //                            pGroup.uiFlags &= ~GROUPFLAG_GROUP_ARRIVED_SIMULTANEOUSLY;
                //                        }
                //                        pGroup = pGroup.next;
                //                    }
            }
        }
        if (ubSAIVersion < 18)
        { //adjust down the number of bloodcats based on difficulty in the two special bloodcat levels
            switch (gGameOptions.ubDifficultyLevel)
            {
                case DifficultyLevel.Easy: //50%
                    SectorInfo[SEC.I16].bBloodCatPlacements = 14;
                    SectorInfo[SEC.N5].bBloodCatPlacements = 13;
                    SectorInfo[SEC.I16].bBloodCats = 14;
                    SectorInfo[SEC.N5].bBloodCats = 13;
                    break;
                case DifficultyLevel.Medium: //75%
                    SectorInfo[SEC.I16].bBloodCatPlacements = 19;
                    SectorInfo[SEC.N5].bBloodCatPlacements = 18;
                    SectorInfo[SEC.I16].bBloodCats = 19;
                    SectorInfo[SEC.N5].bBloodCats = 18;
                    break;
                case DifficultyLevel.Hard: //100%
                    SectorInfo[SEC.I16].bBloodCatPlacements = 26;
                    SectorInfo[SEC.N5].bBloodCatPlacements = 25;
                    SectorInfo[SEC.I16].bBloodCats = 26;
                    SectorInfo[SEC.N5].bBloodCats = 25;
                    break;
            }
        }
        if (ubSAIVersion < 19)
        {
            //Clear the garrison in C5
            gArmyComp[gGarrisonGroup[(int)SectorInfo[SEC.C5].ubGarrisonID].ubComposition].bPriority = 0;
            gArmyComp[gGarrisonGroup[(int)SectorInfo[SEC.C5].ubGarrisonID].ubComposition].bDesiredPopulation = 0;
        }
        if (ubSAIVersion < 20)
        {
            gArmyComp[Garrisons.QUEEN_DEFENCE].bDesiredPopulation = 32;
            SectorInfo[SEC.P3].ubNumElites = 32;
        }
        if (ubSAIVersion < 21)
        {
            //                pGroup = gpGroupList;
            //                while (pGroup)
            //                {
            //                    pGroup.uiFlags = 0;
            //                    pGroup = pGroup.next;
            //                }
        }
        if (ubSAIVersion < 22)
        { //adjust down the number of bloodcats based on difficulty in the two special bloodcat levels
            switch (gGameOptions.ubDifficultyLevel)
            {
                case DifficultyLevel.Easy: //50%
                    SectorInfo[SEC.N5].bBloodCatPlacements = 8;
                    SectorInfo[SEC.N5].bBloodCats = 10;
                    break;
                case DifficultyLevel.Medium: //75%
                    SectorInfo[SEC.N5].bBloodCatPlacements = 8;
                    SectorInfo[SEC.N5].bBloodCats = 10;
                    break;
                case DifficultyLevel.Hard: //100%
                    SectorInfo[SEC.N5].bBloodCatPlacements = 8;
                    SectorInfo[SEC.N5].bBloodCats = 10;
                    break;
            }
        }
        if (ubSAIVersion < 23)
        {
            if (gWorldSectorX != 3 || gWorldSectorY != (MAP_ROW)16 || gbWorldSectorZ == 0)
            {
                SectorInfo[SEC.P3].ubNumElites = 32;
            }
        }
        if (ubSAIVersion < 24)
        {
            //If the queen has escaped to the basement, do not use the profile insertion info
            //when we finally go down there, otherwise she will end up in the wrong spot, possibly inside
            //the walls.
            if (gubFact[FACT.QUEEN_DEAD] == false && gMercProfiles[NPCID.QUEEN].bSectorZ == 1)
            {
                if (gbWorldSectorZ != 1 || gWorldSectorX != 16 || gWorldSectorY != (MAP_ROW)3)
                { //We aren't in the basement sector
                    gMercProfiles[NPCID.QUEEN].fUseProfileInsertionInfo = 0;
                }
                else
                {
                    //We are in the basement sector, relocate queen to proper position.
                    for (i = gTacticalStatus.Team[CIV_TEAM].bFirstID; i <= gTacticalStatus.Team[CIV_TEAM].bLastID; i++)
                    {
                        if (MercPtrs[i].ubProfile == NPCID.QUEEN)
                        { //Found queen, relocate her to 16866
                          //                            BumpAnyExistingMerc(16866);
                          //                            TeleportSoldier(MercPtrs[i], 16866, true);
                            break;
                        }
                    }
                }
            }
        }

        if (ubSAIVersion < 25)
        {
            if (gubFact[FACT.SKYRIDER_CLOSE_TO_CHOPPER])
            {
                gMercProfiles[NPCID.SKYRIDER].fUseProfileInsertionInfo = 0;
            }
        }

        //KM : July 21, 1999 patch fix
        if (ubSAIVersion < 26)
        {
            //            int i;
            //            for (i = 0; i < 255; i++)
            //            {
            //                if (SectorInfo[i].ubNumberOfCivsAtLevel[GREEN_MILITIA] +
            //                        SectorInfo[i].ubNumberOfCivsAtLevel[REGULAR_MILITIA] +
            //                        SectorInfo[i].ubNumberOfCivsAtLevel[ELITE_MILITIA] > 20)
            //                {
            //                    SectorInfo[i].ubNumberOfCivsAtLevel[GREEN_MILITIA] = 0;
            //                    SectorInfo[i].ubNumberOfCivsAtLevel[REGULAR_MILITIA] = 20;
            //                    SectorInfo[i].ubNumberOfCivsAtLevel[ELITE_MILITIA] = 0;
            //                }
            //            }
        }

        //KM : August 4, 1999 patch fix
        //     This addresses the problem of not having any soldiers in sector N7 when playing the game under easy difficulty.
        //		 If captured and interrogated, the player would find no soldiers defending the sector.  This changes the composition
        //     so that it will always be there, and adds the soldiers accordingly if the sector isn't loaded when the update is made.
        if (ubSAIVersion < 27)
        {
            //            if (gGameOptions.ubDifficultyLevel == DIF_LEVEL_EASY)
            //            {
            //                if (gWorldSectorX != 7 || gWorldSectorY != 14 || gbWorldSectorZ)
            //                {
            //                    int cnt, iRandom;
            //                    int iEliteChance, iTroopChance, iAdminChance;
            //                    int iStartPop, iDesiredPop, iPriority;
            //                    SECTORINFO? pSector = null;
            //
            //                    //Change the garrison composition to LEVEL1_DEFENCE from LEVEL2_DEFENCE
            //                    pSector = SectorInfo[SEC.N7];
            //                    gGarrisonGroup[pSector.ubGarrisonID].ubComposition = LEVEL1_DEFENCE;
            //
            //                    iStartPop = gArmyComp[gGarrisonGroup[pSector.ubGarrisonID].ubComposition].bStartPopulation;
            //                    iDesiredPop = gArmyComp[gGarrisonGroup[pSector.ubGarrisonID].ubComposition].bDesiredPopulation;
            //                    iPriority = gArmyComp[gGarrisonGroup[pSector.ubGarrisonID].ubComposition].bPriority;
            //                    iEliteChance = gArmyComp[gGarrisonGroup[pSector.ubGarrisonID].ubComposition].bElitePercentage;
            //                    iTroopChance = gArmyComp[gGarrisonGroup[pSector.ubGarrisonID].ubComposition].bTroopPercentage + iEliteChance;
            //                    iAdminChance = gArmyComp[gGarrisonGroup[pSector.ubGarrisonID].ubComposition].bAdminPercentage;
            //
            //                    if (iStartPop)
            //                    {
            //                        // if population is less than maximum
            //                        if (iStartPop != MAX_STRATEGIC_TEAM_SIZE)
            //                        {
            //                            // then vary it a bit (+/- 25%)
            //                            iStartPop = iStartPop * (100 + (Globals.Random.Next(51) - 25)) / 100;
            //                        }
            //
            //                        iStartPop = Math.Max(gubMinEnemyGroupSize, Math.Min(MAX_STRATEGIC_TEAM_SIZE, iStartPop));
            //                        cnt = iStartPop;
            //
            //                        if (iAdminChance)
            //                        {
            //                            pSector.ubNumAdmins = iAdminChance * iStartPop / 100;
            //                        }
            //                        else
            //                        {
            //                            while (cnt--)
            //                            { //for each person, randomly determine the types of each soldier.
            //                                {
            //                                    iRandom = Globals.Random.Next(100);
            //                                    if (iRandom < iEliteChance)
            //                                    {
            //                                        pSector.ubNumElites++;
            //                                    }
            //                                    else if (iRandom < iTroopChance)
            //                                    {
            //                                        pSector.ubNumTroops++;
            //                                    }
            //                                }
            //                            }
            //                        }
            //                    }
            //                }
            //            }
        }

        if (ubSAIVersion < 28)
        {
            //            GROUP? pNext;
            //            if (!StrategicMap[CALCULATE_STRATEGIC_INDEX(3, 16)].fEnemyControlled)
            //            { //Eliminate all enemy groups in this sector, because the player owns the sector, and it is not
            //              //possible for them to spawn there!
            //                pGroup = gpGroupList;
            //                while (pGroup)
            //                {
            //                    pNext = pGroup.next;
            //                    if (!pGroup.fPlayer)
            //                    {
            //                        if (pGroup.ubSectorX == 3 && pGroup.ubSectorY == 16 && !pGroup.ubPrevX && !pGroup.ubPrevY)
            //                        {
            //                            ClearPreviousAIGroupAssignment(pGroup);
            //                            RemovePGroup(pGroup);
            //                        }
            //                    }
            //                    pGroup = pNext;
            //                }
            //            }
        }
        if (ubSAIVersion < 29)
        {
            //            InitStrategicMovementCosts();
        }

        //KM : Aug 11, 1999 -- Patch fix:  Blindly update the airspace control.  There is a bug somewhere 
        //		 that is failing to keep this information up to date, and I failed to find it.  At least this 
        //		 will patch saves.
        //        UpdateAirspaceControl();

        EvolveQueenPriorityPhase(true);

        //Count and correct the floating groups
        //        pGroup = gpGroupList;
        //        while (pGroup)
        //        {
        //            next = pGroup.next; //store the next node as pGroup could be deleted!
        //            if (!pGroup.fPlayer)
        //            {
        //                if (!pGroup.fBetweenSectors)
        //                {
        //                    if (pGroup.ubSectorX != gWorldSectorX ||
        //                            pGroup.ubSectorY != gWorldSectorY ||
        //                            gbWorldSectorZ)
        //                    {
        //                        RepollSAIGroup(pGroup);
        //                        ValidateGroup(pGroup);
        //                    }
        //                }
        //            }
        //            pGroup = next; //advance the node
        //        }

        //Update the version number to the most current.
        gubSAIVersion = SAI_VERSION;

        ValidateWeights(28);
        ValidatePendingGroups();


        return true;
    }

    public static void EvolveQueenPriorityPhase(bool fForceChange)
    {
        Garrisons index = 0;
        int num = 0, iFactor = 0;
        int iChange = 0, iNew, iNumSoldiers, iNumPromotions;
        SECTORINFO? pSector;
        int[] ubOwned = new int[NUM_ARMY_COMPOSITIONS];
        int[] ubTotal = new int[NUM_ARMY_COMPOSITIONS];
        int ubNewPhase;
        ubNewPhase = Campaign.CurrentPlayerProgressPercentage() / 10;

        if (!fForceChange && ubNewPhase == gubQueenPriorityPhase)
        {
            return;
        }

        if (gubQueenPriorityPhase > ubNewPhase)
        {

        }
        else if (gubQueenPriorityPhase < ubNewPhase)
        {

        }
        else
        {

        }

        gubQueenPriorityPhase = ubNewPhase;

        //The phase value refers to the deviation percentage she will apply to original garrison values.  
        //All sector values are evaluated to see how many of those sectors are enemy controlled.  If they 
        //are controlled by her, the desired number will be increased as well as the priority.  On the other
        //hand, if she doesn't own those sectors, the values will be decreased instead.  All values are based off of
        //the originals.
        //memset(ubOwned, 0, NUM_ARMY_COMPOSITIONS);
        //memset(ubTotal, 0, NUM_ARMY_COMPOSITIONS);

        //Record the values required to calculate the percentage of each composition type that the queen controls.
        for (var i = 0; i < giGarrisonArraySize; i++)
        {
            index = gGarrisonGroup[i].ubComposition;
            if (strategicMap[SECTOR_INFO_TO_STRATEGIC_INDEX(gGarrisonGroup[i].ubSectorID)].fEnemyControlled)
            {
                ubOwned[(int)index]++;
            }
            ubTotal[(int)index]++;
        }

        //Go through the *majority* of compositions and modify the priority/desired values.
        for (Garrisons i = 0; i < Garrisons.NUM_ARMY_COMPOSITIONS; i++)
        {
            switch (i)
            {
                case Garrisons.QUEEN_DEFENCE:
                case Garrisons.MEDUNA_DEFENCE:
                case Garrisons.MEDUNA_SAMSITE:
                case Garrisons.LEVEL1_DEFENCE:
                case Garrisons.LEVEL2_DEFENCE:
                case Garrisons.LEVEL3_DEFENCE:
                case Garrisons.OMERTA_WELCOME_WAGON:
                case Garrisons.ROADBLOCK:
                    //case SANMONA_SMALL:
                    //don't consider these compositions
                    continue;
            }
            //If the queen owns them ALL, then she gets the maximum defensive bonus.  If she owns NONE,
            //then she gets a maximum defensive penalty.  Everything else lies in the middle.  The legal
            //range is +-50.
            //            if (ubTotal[i])
            //            {
            //                iFactor = (ubOwned[i] * 100 / ubTotal[i]) - 50;
            //            }
            //            else
            //            {
            //                iFactor = -50;
            //            }
            //            iFactor = iFactor * gubQueenPriorityPhase / 10;

            //modify priority by + or - 25% of original
            if (gArmyComp[i].bPriority > 0)
            {
                //                num = gOrigArmyComp[i].bPriority + iFactor / 2;
                //                num = Math.Min(Math.Max(0, num), 100);
                //                gArmyComp[i].bPriority = (int)num;
            }

            //modify desired population by + or - 50% of original population
            //            num = gOrigArmyComp[i].bDesiredPopulation * (100 + iFactor) / 100;
            num = Math.Min(Math.Max(6, num), MAX_STRATEGIC_TEAM_SIZE);
            gArmyComp[i].bDesiredPopulation = (int)num;

            //if gfExtraElites is set, then augment the composition sizes
            if (gfExtraElites && iFactor >= 15 && gArmyComp[i].bElitePercentage > 0)
            {
                //                iChange = gGameOptions.ubDifficultyLevel * 5;

                //increase elite % (Math.Max 100)
                iNew = gArmyComp[i].bElitePercentage + iChange;
                iNew = (int)Math.Min(100, iNew);
                gArmyComp[i].bElitePercentage = (int)iNew;

                //decrease troop % (Math.Min 0)
                iNew = gArmyComp[i].bTroopPercentage - iChange;
                iNew = (int)Math.Max(0, iNew);
                gArmyComp[i].bTroopPercentage = (int)iNew;
            }
        }
        if (gfExtraElites)
        {
            //Turn off the flag so that this doesn't happen everytime this function is called!
            gfExtraElites = false;

            for (int i = 0; i < giGarrisonArraySize; i++)
            {
                //if we are dealing with extra elites, then augment elite compositions (but only if they exist in the sector).  
                //If the queen still owns the town by more than 65% (iFactor >= 15), then upgrade troops to elites in those sectors.
                var idx = gGarrisonGroup[i].ubComposition;
                //                index = (int)idx;
                switch (idx)
                {
                    case Garrisons.QUEEN_DEFENCE:
                    case Garrisons.MEDUNA_DEFENCE:
                    case Garrisons.MEDUNA_SAMSITE:
                    case Garrisons.LEVEL1_DEFENCE:
                    case Garrisons.LEVEL2_DEFENCE:
                    case Garrisons.LEVEL3_DEFENCE:
                    case Garrisons.OMERTA_WELCOME_WAGON:
                    case Garrisons.ROADBLOCK:
                        //case SANMONA_SMALL:
                        //don't consider these compositions
                        continue;
                }
                pSector = SectorInfo[gGarrisonGroup[i].ubSectorID];
                //                if (ubTotal[index] > 0)
                //                {
                //                    iFactor = (ubOwned[index] * 100 / ubTotal[index]) - 50;
                //                }
                //                else
                //                {
                //                    iFactor = -50;
                //                }
                if (iFactor >= 15)
                { //Make the actual elites in sector match the new garrison percentage
                    if (!gfWorldLoaded || gbWorldSectorZ > 0 ||
                            gWorldSectorX != SECTORINFO.SECTORX(gGarrisonGroup[i].ubSectorID) ||
                            gWorldSectorY != SECTORINFO.SECTORY(gGarrisonGroup[i].ubSectorID))
                    { //Also make sure the sector isn't currently loaded!
                        iNumSoldiers = pSector.ubNumAdmins + pSector.ubNumTroops + pSector.ubNumElites;
                        iNumPromotions = gArmyComp[(Garrisons)index].bElitePercentage * iNumSoldiers / 100 - pSector.ubNumElites;

                        if (iNumPromotions > 0)
                        {
                            while (iNumPromotions-- > 0)
                            {
                                if (pSector.ubNumAdmins > 0)
                                {
                                    pSector.ubNumAdmins--;
                                }
                                else if (pSector.ubNumTroops > 0)
                                {
                                    pSector.ubNumTroops--;
                                }
                                else
                                {
                                    Debug.Assert(false);
                                }
                                pSector.ubNumElites++;
                            }
                            Debug.Assert(iNumSoldiers == pSector.ubNumAdmins + pSector.ubNumTroops + pSector.ubNumElites);
                        }
                    }
                }
            }
        }
        //Recalculate all of the weights.
        for (int i = 0; i < giGarrisonArraySize; i++)
        {
            RecalculateGarrisonWeight(i);
        }
    }

    void ExecuteStrategicAIAction(NPC_ACTION usActionCode, int sSectorX, MAP_ROW sSectorY)
    {
        GROUP? pGroup, pPendingGroup = null;
        SECTORINFO? pSector;
        SEC ubSectorID = 0;
        int ubNumSoldiers = 0;
        switch (usActionCode)
        {
            case NPC_ACTION.STRATEGIC_AI_ACTION_WAKE_QUEEN:
                this.WakeUpQueen();
                break;

            case NPC_ACTION.STRATEGIC_AI_ACTION_QUEEN_DEAD:
                gfQueenAIAwake = false;
                break;

            case NPC_ACTION.STRATEGIC_AI_ACTION_KINGPIN_DEAD:
                //Immediate send a small garrison to C5 (to discourage access to Tony the dealer)
                /*
                for( i = 0; i < giGarrisonArraySize; i++ )
                {
                    if( gGarrisonGroup[ i ].ubComposition == SANMONA_SMALL )
                    {
                        //Setup the composition so from now on the queen will consider this an important sector
                        //to hold.
                        gArmyComp[ gGarrisonGroup[ i ].ubComposition ].bPriority = 65;
                        gArmyComp[ gGarrisonGroup[ i ].ubComposition ].bTroopPercentage = 100;
                        gArmyComp[ gGarrisonGroup[ i ].ubComposition ].bDesiredPopulation = 5;
                        RequestHighPriorityGarrisonReinforcements( i, (int)(2 + Globals.Random.Next( 4 )) ); //send 2-5 soldiers now.
                        break;
                    }
                }
                */
                break;
            case NPC_ACTION.SEND_SOLDIERS_TO_DRASSEN:
                //Send 6, 9, or 12 troops (based on difficulty) one of the Drassen sectors.  If nobody is there when they arrive,
                //those troops will get reassigned.

                //                if (Chance(50))
                //                {
                //                    ubSectorID = SEC.D13;
                //                }
                //                else if (Chance(60))
                //                {
                //                    ubSectorID = SEC.B13;
                //                }
                //                else
                //                {
                //                    ubSectorID = SEC.C13;
                //                }

                //                    ubNumSoldiers = (int)(3 + gGameOptions.ubDifficultyLevel * 3);
                //                    pGroup = StrategicMovement.CreateNewEnemyGroupDepartingFromSector(SEC.P3, 0, ubNumSoldiers, 0);

                //                    if (!gGarrisonGroup[SectorInfo[ubSectorID].ubGarrisonID].ubPendingGroupID)
                //                    {
                //                        pGroup.pEnemyGroup.ubIntention = STAGE;
                //                        gGarrisonGroup[SectorInfo[ubSectorID].ubGarrisonID].ubPendingGroupID = pGroup.ubGroupID;
                //                    }
                //                    else
                //                    { //this should never happen (but if it did, then this is the best way to deal with it).
                //                      //                        pGroup.pEnemyGroup.ubIntention = PURSUIT;
                //                    }
                //                    giReinforcementPool -= ubNumSoldiers;
                giReinforcementPool = Math.Max(giReinforcementPool, 0);

                //                    MoveSAIGroupToSector(pGroup, ubSectorID, EVASIVE, pGroup.pEnemyGroup.ubIntention);

                break;
            case NPC_ACTION.SEND_SOLDIERS_TO_BATTLE_LOCATION:

                //Send 4, 8, or 12 troops (based on difficulty) to the location of the first battle.  If nobody is there when they arrive,
                //those troops will get reassigned.
                //                    ubSectorID = (int)STRATEGIC_INDEX_TO_SECTOR_INFO(sWorldSectorLocationOfFirstBattle);
                //                    pSector = SectorInfo[ubSectorID];
                //                    ubNumSoldiers = (int)(gGameOptions.ubDifficultyLevel * 4);
                //                    pGroup = CreateNewEnemyGroupDepartingFromSector(SEC.P3, 0, ubNumSoldiers, 0);
                //                    giReinforcementPool -= ubNumSoldiers;
                giReinforcementPool = Math.Max(giReinforcementPool, 0);

                //Determine if the battle location actually has a garrison assignment.  If so, and the following
                //checks succeed, the enemies will be sent to attack and reinforce that sector.  Otherwise, the
                //enemies will simply check it out, then leave.
                //                    if (pSector.ubGarrisonID != NO_GARRISON)
                //                    { //sector has a garrison
                //                      //                        if (!NumEnemiesInSector((int)SECTORINFO.SECTORX(ubSectorID), (int)SECTORINFO.SECTORY(ubSectorID)))
                //                        { //no enemies are here
                //                            if (gArmyComp[!gGarrisonGroup[pSector.ubGarrisonID].ubComposition].bPriority)
                //                            { //the garrison is important
                //                                if (gGarrisonGroup[pSector.ubGarrisonID].ubPendingGroupID == 0)
                //                                { //the garrison doesn't have reinforcements already on route.
                //                                    gGarrisonGroup[pSector.ubGarrisonID].ubPendingGroupID = pGroup.ubGroupID;
                //                                    MoveSAIGroupToSector(pGroup, ubSectorID, SAIMOVECODE.STAGE, ENEMY_INTENTIONS.REINFORCEMENTS);
                //                                    break;
                //                                }
                //                            }
                //                        }
                //                    }
                //                    else
                //                    {
                //                        MoveSAIGroupToSector(pGroup, ubSectorID, SAIMOVECODE.EVASIVE, ENEMY_INTENTIONS.PURSUIT);
                //                    }

                break;
            case NPC_ACTION.SEND_SOLDIERS_TO_OMERTA:
                //                    ubNumSoldiers = (int)(gGameOptions.ubDifficultyLevel * 6); //6, 12, or 18 based on difficulty.
                pGroup = StrategicMovement.CreateNewEnemyGroupDepartingFromSector(SEC.P3, 0, ubNumSoldiers, (int)(ubNumSoldiers / 7)); //add 1 elite to normal, and 2 for hard
                ubNumSoldiers = (int)(ubNumSoldiers + ubNumSoldiers / 7);
                giReinforcementPool -= ubNumSoldiers;
                giReinforcementPool = Math.Max(giReinforcementPool, 0);
                //                    if (PlayerMercsInSector(9, 1, 1) && !PlayerMercsInSector(10, 1, 1) && !PlayerMercsInSector(10, 1, 2))
                //                    { //send to A9 (if mercs in A9, but not in A10 or A10 basement)
                //                        ubSectorID = SEC.A9;
                //                    }
                //                    else
                //                    { //send to A10
                //                        ubSectorID = SEC.A10;
                //                    }

                MoveSAIGroupToSector(pGroup, ubSectorID, SAIMOVECODE.EVASIVE, ENEMY_INTENTIONS.PURSUIT);

                ValidateGroup(pGroup);
                break;
            case NPC_ACTION.SEND_TROOPS_TO_SAM:
                ubSectorID = SECTORINFO.SECTOR(sSectorX, sSectorY);
                //                    ubNumSoldiers = (int)(3 + gGameOptions.ubDifficultyLevel + HighestPlayerProgressPercentage() / 15);
                giReinforcementPool -= ubNumSoldiers;
                giReinforcementPool = Math.Max(giReinforcementPool, 0);
                pGroup = StrategicMovement.CreateNewEnemyGroupDepartingFromSector(SEC.P3, 0, 0, ubNumSoldiers);
                MoveSAIGroupToSector(pGroup, ubSectorID, SAIMOVECODE.STAGE, ENEMY_INTENTIONS.REINFORCEMENTS);

                //                  if (gGarrisonGroup[SectorInfo[ubSectorID].ubGarrisonID].ubPendingGroupID)
                //                  {   //Clear the pending group's assignment.
                //                      pPendingGroup = StrategicMovement.GetGroup(gGarrisonGroup[SectorInfo[ubSectorID].ubGarrisonID].ubPendingGroupID);
                //                      Debug.Assert(pPendingGroup);
                //                      ClearPreviousAIGroupAssignment(pPendingGroup);
                //                  }
                //                  //Assign the elite squad to attack the SAM site
                //                  pGroup.pEnemyGroup.ubIntention = ENEMY_INTENTIONS.REINFORCEMENTS;
                //                  gGarrisonGroup[SectorInfo[ubSectorID].ubGarrisonID].ubPendingGroupID = pGroup.ubGroupID;
                //
                //                  if (pPendingGroup)
                //                  { //Reassign the pending group
                //                      ReassignAIGroup(pPendingGroup);
                //                  }

                break;
            case NPC_ACTION.ADD_MORE_ELITES:
                gfExtraElites = true;
                EvolveQueenPriorityPhase(true);
                break;
            case NPC_ACTION.GIVE_KNOWLEDGE_OF_ALL_MERCS:
                //temporarily make the queen's forces more aware (high alert)
                switch (gGameOptions.ubDifficultyLevel)
                {
                    case DifficultyLevel.Easy:
                        gubNumAwareBattles = EASY_NUM_AWARE_BATTLES;
                        break;
                    case DifficultyLevel.Medium:
                        gubNumAwareBattles = NORMAL_NUM_AWARE_BATTLES;
                        break;
                    case DifficultyLevel.Hard:
                        gubNumAwareBattles = HARD_NUM_AWARE_BATTLES;
                        break;
                }
                break;
            default:
                Messages.ScreenMsg(FontColor.FONT_RED, MSG.DEBUG, "QueenAI failed to handle action code %d.", usActionCode.ToString());
                break;
        }
    }



    void InvestigateSector(int ubSectorID)
    {
        /*
            int i;
            SECTORINFO *pSector;
            int sSectorX, sSectorY;
            int ubAdmins[4], ubTroops[4], ubElites[4], ubNumToSend, ubTotal;

            //@@@ Disabled!  Also commented out the posting of the event
            return;

            sSectorX = (int)SECTORINFO.SECTORX( ubSectorID );
            sSectorY = (int)SECTORINFO.SECTORY( ubSectorID );

            if( guiCurrentScreen != GAME_SCREEN )
            { //If we aren't in tactical, then don't do this.  It is strictly added flavour and would be irritating if
                //you got the prebattle interface in mapscreen while compressing time (right after clearing it...)
                return;
            }

            if( sSectorX != gWorldSectorX || sSectorY != gWorldSectorY || gbWorldSectorZ )
            { //The sector isn't loaded, so don't bother...
                return;
            }

            //Now, we will investigate this sector if there are forces in adjacent towns.  For each 
            //sector that applies, we will add 1-2 soldiers.

            ubTotal = 0;
            for( i = 0; i < 4; i++ )
            {
                ubAdmins[i] = ubTroops[i] = ubElites[i] = 0;
                switch( i )
                {
                    case 0: //NORTH
                        if( sSectorY == 1 )
                            continue;
                        pSector = SectorInfo[ ubSectorID - 16 ];
                        break;
                    case 1: //EAST
                        if( sSectorX == 16 )
                            continue;
                        pSector = SectorInfo[ ubSectorID + 1 ];
                        break;
                    case 2: //SOUTH
                        if( sSectorY == 16 )
                            continue;
                        pSector = SectorInfo[ ubSectorID + 16 ];
                        break;
                    case 3: //WEST
                        if( sSectorX == 1 )
                            continue;
                        pSector = SectorInfo[ ubSectorID - 1 ];
                        break;
                }
                if( pSector.ubNumAdmins + pSector.ubNumTroops + pSector.ubNumElites > 4 )
                {
                    ubNumToSend = (int)(Globals.Random.Next( 2 ) + 1);
                    while( ubNumToSend )
                    {
                        if( pSector.ubNumAdmins )
                        {
                            pSector.ubNumAdmins--;
                            ubNumToSend--;
                            ubAdmins[i]++;
                            ubTotal++;
                        }
                        else if( pSector.ubNumTroops )
                        {
                            pSector.ubNumTroops--;
                            ubNumToSend--;
                            ubTroops[i]++;
                            ubTotal++;
                        }
                        else if( pSector.ubNumElites )
                        {
                            pSector.ubNumTroops--;
                            ubNumToSend--;
                            ubTroops[i]++;
                            ubTotal++;
                        }
                        else
                        {
                            break; //???
                        }
                    }
                }
            }
            if( !ubTotal )
            { //Nobody is available to investigate
                return;
            }
            //Now we have decided who to send, so send them.
            for( i = 0; i < 4; i++ )
            {
                if( ubAdmins[i] + ubTroops[i] + ubElites[i] )
                {
                    switch( i )
                    {
                        case 0: //NORTH 
                            AddEnemiesToBattle( null, INSERTION_CODE_NORTH, ubAdmins[i], ubTroops[i], ubElites[i], true );	
                            break;
                        case 1: //EAST 
                            AddEnemiesToBattle( null, INSERTION_CODE_EAST, ubAdmins[i], ubTroops[i], ubElites[i], true );	
                            break;
                        case 2: //SOUTH 
                            AddEnemiesToBattle( null, INSERTION_CODE_SOUTH, ubAdmins[i], ubTroops[i], ubElites[i], true );	
                            break;
                        case 3: //WEST 
                            AddEnemiesToBattle( null, INSERTION_CODE_WEST, ubAdmins[i], ubTroops[i], ubElites[i], true );	
                            break;
                    }
                }
            }
            if( !gfQueenAIAwake )
            {
                gfFirstBattleMeanwhileScenePending = true;
            }
        */
    }

    void StrategicHandleQueenLosingControlOfSector(int sSectorX, MAP_ROW sSectorY, int sSectorZ)
    {
        SECTORINFO? pSector;
        SEC ubSectorID;
        //        if (sSectorZ)
        //        { //The queen doesn't care about anything happening under the ground.
        //            return;
        //        }

        //        if (strategicMap[sSectorX + sSectorY * MAP_WORLD_X].fEnemyControlled)
        //        { //If the sector doesn't belong to the player, then we shouldn't be calling this function!
        //            SAIReportError("StrategicHandleQueenLosingControlOfSector() was called for a sector that is internally considered to be enemy controlled.");
        //            return;
        //        }

        ubSectorID = SECTORINFO.SECTOR(sSectorX, sSectorY);
        pSector = SectorInfo[ubSectorID];

        //Keep track of victories and wake up the queen after x number of battles.
        gusPlayerBattleVictories++;
        //        if (gusPlayerBattleVictories == 5 - gGameOptions.ubDifficultyLevel)
        { //4 victories for easy, 3 for normal, 2 for hard
            this.WakeUpQueen();
        }

        if (pSector.ubGarrisonID == NO_GARRISON)
        { //Queen doesn't care if the sector lost wasn't a garrison sector.
            return;
        }
        else
        { //check to see if there are any pending reinforcements.  If so, then cancel their orders and have them
          //reassigned, so the player doesn't get pestered.  This is a feature that *dumbs* down the AI, and is done
          //for the sake of gameplay.  We don't want the game to be tedious.
            if (pSector.uiTimeLastPlayerLiberated == 0)
            {
                pSector.uiTimeLastPlayerLiberated = GameClock.GetWorldTotalSeconds();
            }
            else
            { //convert hours to seconds and subtract up to half of it randomly "seconds - (hours*3600 / 2)"
                pSector.uiTimeLastPlayerLiberated = (uint)(GameClock.GetWorldTotalSeconds() - Globals.Random.Next(gubHoursGracePeriod * 1800));
            }

            if (gGarrisonGroup[(int)pSector.ubGarrisonID].ubPendingGroupID > 0)
            {
                GROUP? pGroup = StrategicMovement.GetGroup(gGarrisonGroup[(int)pSector.ubGarrisonID].ubPendingGroupID);
                if (pGroup is not null)
                {
                    ReassignAIGroup(pGroup);
                }
                gGarrisonGroup[(int)pSector.ubGarrisonID].ubPendingGroupID = 0;
            }
        }

        //If there are any enemy groups that will be moving through this sector due, they will have to repath which
        //will cause them to avoid the sector.  Returns the number of redirected groups.
        RedirectEnemyGroupsMovingThroughSector((int)sSectorX, (int)sSectorY);

        //For the purposes of a town being lost, we shall check to see if the queen wishes to investigate quickly after
        //losing.  This is done in town sectors when the character first takes it.
        switch (ubSectorID)
        {
            case SEC.B13:
            case SEC.C13:
            case SEC.D13:
                //Drassen
                SectorInfo[SEC.B13].ubInvestigativeState++;
                SectorInfo[SEC.C13].ubInvestigativeState++;
                SectorInfo[SEC.D13].ubInvestigativeState++;
                break;
            case SEC.A2:
            case SEC.B2:
                //Chitzena
                SectorInfo[SEC.A2].ubInvestigativeState++;
                SectorInfo[SEC.B2].ubInvestigativeState++;
                break;
            case SEC.G1:
            case SEC.G2:
            case SEC.H1:
            case SEC.H2:
            case SEC.H3:
                //Grumm
                SectorInfo[SEC.G1].ubInvestigativeState++;
                SectorInfo[SEC.G2].ubInvestigativeState++;
                SectorInfo[SEC.H1].ubInvestigativeState++;
                SectorInfo[SEC.H2].ubInvestigativeState++;
                SectorInfo[SEC.H3].ubInvestigativeState++;
                break;
            case SEC.F8:
            case SEC.F9:
            case SEC.G8:
            case SEC.G9:
            case SEC.H8:
                //Cambria
                SectorInfo[SEC.F8].ubInvestigativeState++;
                SectorInfo[SEC.F9].ubInvestigativeState++;
                SectorInfo[SEC.G8].ubInvestigativeState++;
                SectorInfo[SEC.G9].ubInvestigativeState++;
                SectorInfo[SEC.H8].ubInvestigativeState++;
                break;
            case SEC.H13:
            case SEC.H14:
            case SEC.I13:
            case SEC.I14:
                //Alma
                SectorInfo[SEC.H13].ubInvestigativeState++;
                SectorInfo[SEC.H14].ubInvestigativeState++;
                SectorInfo[SEC.I13].ubInvestigativeState++;
                SectorInfo[SEC.I14].ubInvestigativeState++;
                break;
            case SEC.L11:
            case SEC.L12:
                //Balime
                SectorInfo[SEC.L11].ubInvestigativeState++;
                SectorInfo[SEC.L12].ubInvestigativeState++;
                break;
            case SEC.N3:
            case SEC.N4:
            case SEC.N5:
            case SEC.O3:
            case SEC.O4:
            case SEC.P3:
                //Meduna
                SectorInfo[SEC.N3].ubInvestigativeState++;
                SectorInfo[SEC.N4].ubInvestigativeState++;
                SectorInfo[SEC.N5].ubInvestigativeState++;
                SectorInfo[SEC.O3].ubInvestigativeState++;
                SectorInfo[SEC.O4].ubInvestigativeState++;
                SectorInfo[SEC.P3].ubInvestigativeState++;
                break;
            default:
                return;
        }

        if (pSector.ubInvestigativeState >= 4)
        { //This is the 4th time the player has conquered this sector.  We won't pester the player with probing attacks here anymore.
            return;
        }
        if (sSectorX != gWorldSectorX || sSectorY != gWorldSectorY)
        { //The sector isn't loaded, so don't probe attack it.  Otherwise, autoresolve would get them smoked!
            return;
        }
        //@@@ disabled
        //AddStrategicEventUsingSeconds( EVENT_INVESTIGATE_SECTOR, GetWorldTotalSeconds() + 45 * pSector.ubInvestigativeState + Globals.Random.Next( 60 ), SECTOR( sSectorX, sSectorY ) );
    }

    void RequestHighPriorityStagingGroupReinforcements(GROUP? pGroup)
    {
        //	GROUP *pClosestGroup;
        //            if (!pGroup.pEnemyGroup.ubPendingReinforcements)
        //            {
        //                return;
        //            }
        //pClosestGroup = SearchForClosestGroup( pGroup );
    }


    int SectorDistance(SEC ubSectorID1, SEC ubSectorID2)
    {
        int ubSectorX1, ubSectorX2;
        MAP_ROW ubSectorY1, ubSectorY2;
        int ubDist;
        ubSectorX2 = (int)SECTORINFO.SECTORX(ubSectorID2);
        ubSectorX1 = (int)SECTORINFO.SECTORX(ubSectorID1);
        ubSectorY1 = SECTORINFO.SECTORY(ubSectorID1);
        ubSectorY2 = SECTORINFO.SECTORY(ubSectorID2);

        ubDist = (int)(Math.Abs(ubSectorX1 - ubSectorX2) + Math.Abs(ubSectorY1 - ubSectorY2));

        return ubDist;
    }

    void RequestHighPriorityGarrisonReinforcements(Garrisons iGarrisonID, int ubSoldiersRequested)
    {
        int i, iBestIndex;
        GROUP? pGroup;
        int ubBestDist, ubDist;
        int ubDstSectorX;
        MAP_ROW ubDstSectorY;
        //AssertMsg( giPatrolArraySize == PATROL_GROUPS && giGarrisonArraySize == GARRISON_GROUPS, "Strategic AI -- Patrol and/or garrison group definition mismatch." );
        ubBestDist = 255;
        iBestIndex = -1;
        for (i = 0; i < giPatrolArraySize; i++)
        {
            if (gPatrolGroup[i].ubGroupID > 0)
            {
                pGroup = StrategicMovement.GetGroup(gPatrolGroup[i].ubGroupID);
                if (pGroup is not null && pGroup.ubGroupSize >= ubSoldiersRequested)
                {
                    ubDist = this.SectorDistance(SECTORINFO.SECTOR(pGroup.ubSectorX, pGroup.ubSectorY), gGarrisonGroup[(int)iGarrisonID].ubSectorID);
                    if (ubDist < ubBestDist)
                    {
                        ubBestDist = ubDist;
                        iBestIndex = i;
                    }
                }
            }
        }
        ubDstSectorX = SECTORINFO.SECTORX(gGarrisonGroup[(int)iGarrisonID].ubSectorID);
        ubDstSectorY = SECTORINFO.SECTORY(gGarrisonGroup[(int)iGarrisonID].ubSectorID);
        if (iBestIndex != -1)
        { //Send the group to the garrison
            pGroup = StrategicMovement.GetGroup(gPatrolGroup[iBestIndex].ubGroupID);
            if (pGroup.ubGroupSize > ubSoldiersRequested && pGroup.ubGroupSize - ubSoldiersRequested >= gubMinEnemyGroupSize)
            { //Split the group, and send to location
                GROUP? pNewGroup;
                pNewGroup = StrategicMovement.CreateNewEnemyGroupDepartingFromSector(SECTORINFO.SECTOR(pGroup.ubSectorX, pGroup.ubSectorY), 0, 0, 0);
                //Transfer the troops from group to new group
                //                    if (pGroup.pEnemyGroup.ubNumTroops >= ubSoldiersRequested)
                //                    { //All of them are troops, so do it in one shot.
                //                        pGroup.pEnemyGroup.ubNumTroops -= ubSoldiersRequested;
                //                        pGroup.ubGroupSize -= ubSoldiersRequested;
                //                        pNewGroup.pEnemyGroup.ubNumTroops = ubSoldiersRequested;
                //                        pNewGroup.ubGroupSize += ubSoldiersRequested;
                //                        ValidateLargeGroup(pGroup);
                //                        ValidateLargeGroup(pNewGroup);
                //                    }
                //                    else
                {
                    //                        while (ubSoldiersRequested)
                    //                        { //There aren't enough troops, so transfer other types when we run out of troops, prioritizing admins, then elites.
                    //                            if (pGroup.pEnemyGroup.ubNumTroops)
                    //                            {
                    //                                pGroup.pEnemyGroup.ubNumTroops--;
                    //                                pGroup.ubGroupSize--;
                    //                                pNewGroup.pEnemyGroup.ubNumTroops++;
                    //                                pNewGroup.ubGroupSize++;
                    //                                ubSoldiersRequested--;
                    //                                ValidateLargeGroup(pGroup);
                    //                                ValidateLargeGroup(pNewGroup);
                    //                            }
                    //                            else if (pGroup.pEnemyGroup.ubNumAdmins)
                    //                            {
                    //                                pGroup.pEnemyGroup.ubNumAdmins--;
                    //                                pGroup.ubGroupSize--;
                    //                                pNewGroup.pEnemyGroup.ubNumAdmins++;
                    //                                pNewGroup.ubGroupSize++;
                    //                                ubSoldiersRequested--;
                    //                                ValidateLargeGroup(pGroup);
                    //                                ValidateLargeGroup(pNewGroup);
                    //                            }
                    //                            else if (pGroup.pEnemyGroup.ubNumElites)
                    //                            {
                    //                                pGroup.pEnemyGroup.ubNumElites--;
                    //                                pGroup.ubGroupSize--;
                    //                                pNewGroup.pEnemyGroup.ubNumElites++;
                    //                                pNewGroup.ubGroupSize++;
                    //                                ubSoldiersRequested--;
                    //                                ValidateLargeGroup(pGroup);
                    //                                ValidateLargeGroup(pNewGroup);
                    //                            }
                    //                            else
                    //                            {
                    //                                //AssertMsg(0, "Strategic AI group transfer error.  KM : 0");
                    //                                return;
                    //                            }
                    //                        }
                }

                pNewGroup.ubOriginalSector = SECTORINFO.SECTOR(ubDstSectorX, ubDstSectorY);
                gGarrisonGroup[(int)iGarrisonID].ubPendingGroupID = pNewGroup.ubGroupID;
                RecalculatePatrolWeight(iBestIndex);

                MoveSAIGroupToSector(pNewGroup, gGarrisonGroup[(int)iGarrisonID].ubSectorID, SAIMOVECODE.EVASIVE, ENEMY_INTENTIONS.REINFORCEMENTS);
            }
            else
            { //Send the whole group and kill it's patrol assignment.
                gPatrolGroup[iBestIndex].ubGroupID = 0;
                gGarrisonGroup[(int)iGarrisonID].ubPendingGroupID = pGroup.ubGroupID;
                pGroup.ubOriginalSector = SECTORINFO.SECTOR(ubDstSectorX, ubDstSectorY);
                RecalculatePatrolWeight(iBestIndex);
                //The ONLY case where the group is told to move somewhere else when they could be BETWEEN sectors.  The movegroup functions
                //don't work if this is the case.  Teleporting them to their previous sector is the best and easiest way to deal with this.
                //                    SetEnemyGroupSector(pGroup, SECTORINFO.SECTOR(pGroup.ubSectorX, pGroup.ubSectorY));

                MoveSAIGroupToSector(pGroup, gGarrisonGroup[(int)iGarrisonID].ubSectorID, SAIMOVECODE.EVASIVE, ENEMY_INTENTIONS.REINFORCEMENTS);
                ValidateGroup(pGroup);
            }
        }
        else
        { //There are no groups that have enough troops.  Send a new force from the palace instead.
            pGroup = StrategicMovement.CreateNewEnemyGroupDepartingFromSector(SEC.P3, 0, ubSoldiersRequested, 0);
            pGroup.ubMoveType = MOVE_TYPES.ONE_WAY;
            //                pGroup.pEnemyGroup.ubIntention = REINFORCEMENTS;
            gGarrisonGroup[(int)iGarrisonID].ubPendingGroupID = pGroup.ubGroupID;
            pGroup.ubOriginalSector = SECTORINFO.SECTOR(ubDstSectorX, ubDstSectorY);
            giReinforcementPool -= (int)ubSoldiersRequested;

            MoveSAIGroupToSector(pGroup, gGarrisonGroup[(int)iGarrisonID].ubSectorID, SAIMOVECODE.EVASIVE, ENEMY_INTENTIONS.REINFORCEMENTS);
            ValidateGroup(pGroup);
        }
    }

    void WakeUpQueen()
    {
        gfQueenAIAwake = true;
        if (!gfMassFortificationOrdered)
        {
            gfMassFortificationOrdered = true;
            this.MassFortifyTowns();
        }
    }

    void MassFortifyTowns()
    {
        SECTORINFO? pSector;
        GROUP? pGroup;
        int ubNumTroops, ubDesiredTroops;
        for (int i = 0; i < giGarrisonArraySize; i++)
        {
            pSector = SectorInfo[gGarrisonGroup[i].ubSectorID];
            ubNumTroops = pSector.ubNumAdmins + pSector.ubNumTroops + pSector.ubNumElites;
            ubDesiredTroops = (int)gArmyComp[gGarrisonGroup[i].ubComposition].bDesiredPopulation;
            if (ubNumTroops < ubDesiredTroops)
            {
                if (gGarrisonGroup[i].ubPendingGroupID == 0 &&
                        gGarrisonGroup[i].ubComposition != Garrisons.ROADBLOCK &&
                        EnemyPermittedToAttackSector(null, gGarrisonGroup[i].ubSectorID))
                {
                    this.RequestHighPriorityGarrisonReinforcements((Garrisons)i, (int)(ubDesiredTroops - ubNumTroops));
                }
            }
        }

        //Convert the garrison sitting in Omerta (if alive), and reassign them
        pSector = SectorInfo[SEC.A9];
        if (pSector.ubNumTroops > 0)
        {
            pGroup = StrategicMovement.CreateNewEnemyGroupDepartingFromSector(SEC.A9, 0, pSector.ubNumTroops, 0);
            Debug.Assert(pGroup is not null);
            pSector.ubNumTroops = 0;
            //                pGroup.pEnemyGroup.ubIntention = PATROL;
            pGroup.ubMoveType = MOVE_TYPES.ONE_WAY;
            ReassignAIGroup(pGroup);
            ValidateGroup(pGroup);
            RecalculateSectorWeight(SEC.A9);
        }
    }

    void RenderAIViewerGarrisonInfo(int x, int y, SECTORINFO? pSector)
    {
        if (pSector.ubGarrisonID != NO_GARRISON)
        {
            int iDesired, iSurplus;
            iDesired = gArmyComp[gGarrisonGroup[(int)pSector.ubGarrisonID].ubComposition].bDesiredPopulation;
            iSurplus = pSector.ubNumTroops + pSector.ubNumAdmins + pSector.ubNumElites - iDesired;
            //            SetFontForeground(FONT_WHITE);
            if (iSurplus >= 0)
            {
                mprintf(x, y, "%d desired, %d surplus troops", iDesired, iSurplus);
            }
            else
            {
                mprintf(x, y, "%d desired, %d reinforcements requested", iDesired, -iSurplus);
            }

            if (gGarrisonGroup[(int)pSector.ubGarrisonID].ubPendingGroupID > 0)
            {
                GROUP? pGroup;
                pGroup = StrategicMovement.GetGroup(gGarrisonGroup[(int)pSector.ubGarrisonID].ubPendingGroupID);
                //                mprintf(x, y + 10, "%d reinforcements on route from group %d in %c%d", pGroup.ubGroupSize, pGroup.ubGroupID,
                //                    pGroup.ubSectorY + 'A' - 1, pGroup.ubSectorX);
            }
            else
            {
                mprintf(x, y + 10, "No pending reinforcements for this sector.");
            }
        }
        else
        {
            //            SetFontForeground(FONT_GRAY2);
            mprintf(x, y, "No garrison information for this sector.");
        }
    }

    void StrategicHandleMineThatRanOut(SEC ubSectorID)
    {
        switch (ubSectorID)
        {
            case SEC.B2:
                gArmyComp[gGarrisonGroup[(int)SectorInfo[SEC.A2].ubGarrisonID].ubComposition].bPriority /= 4;
                gArmyComp[gGarrisonGroup[(int)SectorInfo[SEC.B2].ubGarrisonID].ubComposition].bPriority /= 4;
                break;
            case SEC.D13:
                gArmyComp[gGarrisonGroup[(int)SectorInfo[SEC.B13].ubGarrisonID].ubComposition].bPriority /= 4;
                gArmyComp[gGarrisonGroup[(int)SectorInfo[SEC.C13].ubGarrisonID].ubComposition].bPriority /= 4;
                gArmyComp[gGarrisonGroup[(int)SectorInfo[SEC.D13].ubGarrisonID].ubComposition].bPriority /= 4;
                break;
            case SEC.H8:
                gArmyComp[gGarrisonGroup[(int)SectorInfo[SEC.F8].ubGarrisonID].ubComposition].bPriority /= 4;
                gArmyComp[gGarrisonGroup[(int)SectorInfo[SEC.F9].ubGarrisonID].ubComposition].bPriority /= 4;
                gArmyComp[gGarrisonGroup[(int)SectorInfo[SEC.G8].ubGarrisonID].ubComposition].bPriority /= 4;
                gArmyComp[gGarrisonGroup[(int)SectorInfo[SEC.G9].ubGarrisonID].ubComposition].bPriority /= 4;
                gArmyComp[gGarrisonGroup[(int)SectorInfo[SEC.H8].ubGarrisonID].ubComposition].bPriority /= 4;
                break;
            case SEC.I14:
                gArmyComp[gGarrisonGroup[(int)SectorInfo[SEC.H13].ubGarrisonID].ubComposition].bPriority /= 4;
                gArmyComp[gGarrisonGroup[(int)SectorInfo[SEC.H14].ubGarrisonID].ubComposition].bPriority /= 4;
                gArmyComp[gGarrisonGroup[(int)SectorInfo[SEC.I13].ubGarrisonID].ubComposition].bPriority /= 4;
                gArmyComp[gGarrisonGroup[(int)SectorInfo[SEC.I14].ubGarrisonID].ubComposition].bPriority /= 4;
                break;
        }
    }

    public static bool GarrisonCanProvideMinimumReinforcements(int iGarrisonID)
    {
        int iAvailable;
        int iDesired;
        SECTORINFO? pSector;
        int ubSectorX, ubSectorY;

        pSector = SectorInfo[gGarrisonGroup[iGarrisonID].ubSectorID];

        iAvailable = pSector.ubNumAdmins + pSector.ubNumTroops + pSector.ubNumElites;
        iDesired = gArmyComp[gGarrisonGroup[iGarrisonID].ubComposition].bDesiredPopulation;

        if (iAvailable - iDesired >= gubMinEnemyGroupSize)
        {
            //Do a more expensive check first to determine if there is a player presence here (combat in progress)
            //If so, do not provide reinforcements from here.
            ubSectorX = (int)SECTORINFO.SECTORX(gGarrisonGroup[iGarrisonID].ubSectorID);
            ubSectorY = (int)SECTORINFO.SECTORY(gGarrisonGroup[iGarrisonID].ubSectorID);
            //            if (PlayerMercsInSector(ubSectorX, ubSectorY, 0) || CountAllMilitiaInSector(ubSectorX, ubSectorY))
            //            {
            //                return false;
            //            }
            return true;
        }
        return false;
    }

    bool GarrisonRequestingMinimumReinforcements(int iGarrisonID)
    {
        int iAvailable;
        int iDesired;
        SECTORINFO? pSector;

        //        if (gGarrisonGroup[iGarrisonID].ubPendingGroupID)
        //        {
        //            return false;
        //        }

        pSector = SectorInfo[gGarrisonGroup[iGarrisonID].ubSectorID];
        iAvailable = pSector.ubNumAdmins + pSector.ubNumTroops + pSector.ubNumElites;
        iDesired = gArmyComp[gGarrisonGroup[iGarrisonID].ubComposition].bDesiredPopulation;

        if (iDesired - iAvailable >= gubMinEnemyGroupSize)
        {
            return true;
        }
        return false;
    }

    bool PatrolRequestingMinimumReinforcements(int iPatrolID)
    {
        GROUP? pGroup;

        if (gPatrolGroup[iPatrolID].ubPendingGroupID > 0)
        {
            return false;
        }
        if (!PermittedToFillPatrolGroup(iPatrolID))
        { //if the group was defeated, it won't be considered for reinforcements again for several days
            return false;
        }
        pGroup = StrategicMovement.GetGroup(gPatrolGroup[iPatrolID].ubGroupID);
        if (pGroup is not null)
        {
            if (gPatrolGroup[iPatrolID].bSize - pGroup.ubGroupSize >= gubMinEnemyGroupSize)
            {
                return true;
            }
        }
        return false;
    }


    void EliminateSurplusTroopsForGarrison(GROUP? pGroup, SECTORINFO? pSector)
    {
        int iTotal = 0;
        //            iTotal = pGroup.pEnemyGroup.ubNumTroops + pGroup.pEnemyGroup.ubNumElites + pGroup.pEnemyGroup.ubNumAdmins +
        //                             pSector.ubNumTroops + pSector.ubNumElites + pSector.ubNumAdmins;
        if (iTotal <= MAX_STRATEGIC_TEAM_SIZE)
        {
            return;
        }
        iTotal -= MAX_STRATEGIC_TEAM_SIZE;
        while (iTotal > 0)
        {
            //                if (pGroup.pEnemyGroup.ubNumAdmins)
            //                {
            //                    if (pGroup.pEnemyGroup.ubNumAdmins < iTotal)
            //                    {
            //                        iTotal -= pGroup.pEnemyGroup.ubNumAdmins;
            //                        pGroup.pEnemyGroup.ubNumAdmins = 0;
            //                    }
            //                    else
            //                    {
            //                        pGroup.pEnemyGroup.ubNumAdmins -= (int)iTotal;
            //                        iTotal = 0;
            //                    }
            //                }
            //                else if (pSector.ubNumAdmins)
            //                {
            //                    if (pSector.ubNumAdmins < iTotal)
            //                    {
            //                        iTotal -= pSector.ubNumAdmins;
            //                        pSector.ubNumAdmins = 0;
            //                    }
            //                    else
            //                    {
            //                        pSector.ubNumAdmins -= (int)iTotal;
            //                        iTotal = 0;
            //                    }
            //                }
            //                else if (pGroup.pEnemyGroup.ubNumTroops)
            //                {
            //                    if (pGroup.pEnemyGroup.ubNumTroops < iTotal)
            //                    {
            //                        iTotal -= pGroup.pEnemyGroup.ubNumTroops;
            //                        pGroup.pEnemyGroup.ubNumTroops = 0;
            //                    }
            //                    else
            //                    {
            //                        pGroup.pEnemyGroup.ubNumTroops -= (int)iTotal;
            //                        iTotal = 0;
            //                    }
            //                }
            //                else if (pSector.ubNumTroops)
            //                {
            //                    if (pSector.ubNumTroops < iTotal)
            //                    {
            //                        iTotal -= pSector.ubNumTroops;
            //                        pSector.ubNumTroops = 0;
            //                    }
            //                    else
            //                    {
            //                        pSector.ubNumTroops -= (int)iTotal;
            //                        iTotal = 0;
            //                    }
            //                }
            //                else if (pGroup.pEnemyGroup.ubNumElites)
            //                {
            //                    if (pGroup.pEnemyGroup.ubNumElites < iTotal)
            //                    {
            //                        iTotal -= pGroup.pEnemyGroup.ubNumElites;
            //                        pGroup.pEnemyGroup.ubNumElites = 0;
            //                    }
            //                    else
            //                    {
            //                        pGroup.pEnemyGroup.ubNumElites -= (int)iTotal;
            //                        iTotal = 0;
            //                    }
            //                }
            //                else if (pSector.ubNumElites)
            {
                if (pSector.ubNumElites < iTotal)
                {
                    iTotal -= pSector.ubNumElites;
                    pSector.ubNumElites = 0;
                }
                else
                {
                    pSector.ubNumElites -= (int)iTotal;
                    iTotal = 0;
                }
            }
        }
    }



    // once Queen is awake, she'll gradually begin replacing admins with regular troops.  This is mainly to keep player from
    // fighting many more admins once they are no longer any challenge for him.  Eventually all admins will vanish off map.
    public static void UpgradeAdminsToTroops()
    {
        int i;
        SECTORINFO? pSector;
        int bPriority;
        int ubAdminsToCheck;
        int sPatrolIndex;


        // on normal, AI evaluates approximately every 10 hrs.  There are about 130 administrators seeded on the map.
        // Some of these will be killed by the player.

        // check all garrisons for administrators
        //        for (i = 0; i < giGarrisonArraySize; i++)
        //        {
        //            // skip sector if it's currently loaded, we'll never upgrade guys in those
        //            if (SECTORINFO.SECTOR(gWorldSectorX, gWorldSectorY) == gGarrisonGroup[i].ubSectorID)
        //            {
        //                continue;
        //            }
        //
        //            pSector = SectorInfo[gGarrisonGroup[i].ubSectorID];
        //
        //            // if there are any admins currently in this garrison
        //            if (pSector.ubNumAdmins > 0)
        //            {
        //                bPriority = gArmyComp[gGarrisonGroup[i].ubComposition].bPriority;
        //
        //                // highest priority sectors are upgraded first. Each 1% of progress lower the 
        //                // priority threshold required to start triggering upgrades by 10%.
        ////                if ((100 - (10 * HighestPlayerProgressPercentage())) < bPriority)
        //                {
        //                    ubAdminsToCheck = pSector.ubNumAdmins;
        //
        //                    while (ubAdminsToCheck > 0)
        //                    {
        //                        // chance to upgrade at each check is random, and also dependant on the garrison's priority
        ////                        if (Chance(bPriority))
        ////                        {
        ////                            pSector.ubNumAdmins--;
        ////                            pSector.ubNumTroops++;
        ////                        }
        //
        //                        ubAdminsToCheck--;
        //                    }
        //                }
        //            }
        //        }


        // check all moving enemy groups for administrators
        foreach (var pGroup in gpGroupList)
        {
            if (pGroup.ubGroupSize > 0 && !pGroup.fPlayer && !pGroup.fVehicle)
            {
                // skip sector if it's currently loaded, we'll never upgrade guys in those
                if ((pGroup.ubSectorX == gWorldSectorX) && (pGroup.ubSectorY == gWorldSectorY))
                {
                    //                        pGroup = pGroup.next;
                    continue;
                }

                // if there are any admins currently in this group
                //                    if (pGroup.pEnemyGroup.ubNumAdmins > 0)
                //                    {
                //                        // if it's a patrol group
                //                        if (pGroup.pEnemyGroup.ubIntention == PATROL)
                //                        {
                //                            sPatrolIndex = FindPatrolGroupIndexForGroupID(pGroup.ubGroupID);
                //                            Debug.Assert(sPatrolIndex != -1);
                //
                //                            // use that patrol's priority
                //                            bPriority = gPatrolGroup[sPatrolIndex].bPriority;
                //                        }
                //                        else    // not a patrol group
                //                        {
                //                            // use a default priority
                //                            bPriority = 50;
                //                        }
                //
                //                        // highest priority groups are upgraded first. Each 1% of progress lower the 
                //                        // priority threshold required to start triggering upgrades by 10%.
                //                        if ((100 - (10 * HighestPlayerProgressPercentage())) < bPriority)
                //                        {
                //                            ubAdminsToCheck = pGroup.pEnemyGroup.ubNumAdmins;
                //
                //                            while (ubAdminsToCheck > 0)
                //                            {
                //                                // chance to upgrade at each check is random, and also dependant on the group's priority
                //                                if (Chance(bPriority))
                //                                {
                //                                    pGroup.pEnemyGroup.ubNumAdmins--;
                //                                    pGroup.pEnemyGroup.ubNumTroops++;
                //                                }
                //
                //                                ubAdminsToCheck--;
                //                            }
                //                        }
                //                    }
            }

            //            pGroup = pGroup.next;
        }
    }


    public static int FindPatrolGroupIndexForGroupID(int ubGroupID)
    {
        int sPatrolIndex;

        for (sPatrolIndex = 0; sPatrolIndex < giPatrolArraySize; sPatrolIndex++)
        {
            if (gPatrolGroup[sPatrolIndex].ubGroupID == ubGroupID)
            {
                // found it
                return sPatrolIndex;
            }
        }

        // not there!
        return -1;
    }


    int FindPatrolGroupIndexForGroupIDPending(int ubGroupID)
    {
        int sPatrolIndex;

        for (sPatrolIndex = 0; sPatrolIndex < giPatrolArraySize; sPatrolIndex++)
        {
            if (gPatrolGroup[sPatrolIndex].ubPendingGroupID == ubGroupID)
            {
                // found it
                return sPatrolIndex;
            }
        }

        // not there!
        return -1;
    }


    Garrisons FindGarrisonIndexForGroupIDPending(int ubGroupID)
    {
        for (int sGarrisonIndex = 0; sGarrisonIndex < giGarrisonArraySize; sGarrisonIndex++)
        {
            if (gGarrisonGroup[sGarrisonIndex].ubPendingGroupID == ubGroupID)
            {
                // found it
                return (Garrisons)sGarrisonIndex;
            }
        }

        // not there!
        return (Garrisons)(-1);
    }

    public static void TransferGroupToPool(GROUP? pGroup)
    {
        giReinforcementPool += pGroup.ubGroupSize;
        //        RemovePGroup(pGroup);
        pGroup = null;
    }

    //NOTE:  Make sure you call SetEnemyGroupSector() first if the group is between sectors!!  See example in ReassignAIGroup()...
    public static void SendGroupToPool(GROUP? pGroup)
    {
        if (pGroup.ubSectorX == 3 && pGroup.ubSectorY == (MAP_ROW)16)
        {
            TransferGroupToPool(pGroup);
        }
        else
        {
            pGroup.ubSectorIDOfLastReassignment = SECTORINFO.SECTOR(pGroup.ubSectorX, pGroup.ubSectorY);
            //                MoveSAIGroupToSector(pGroup, SEC.P3, EVASIVE, REINFORCEMENTS);
        }
    }


    public static void ReassignAIGroup(GROUP pGroup)
    {
        Garrisons i;
        int iRandom;
        int iWeight;
        int usDefencePoints;
        int iReloopLastIndex = -1;
        SEC ubSectorID;

        ubSectorID = SECTORINFO.SECTOR(pGroup.ubSectorX, pGroup.ubSectorY);

        pGroup.ubSectorIDOfLastReassignment = ubSectorID;

        ClearPreviousAIGroupAssignment(pGroup);

        //First thing to do, is teleport the group to be AT the sector he is currently moving from.  Otherwise, the 
        //strategic pathing can break if the group is between sectors upon reassignment.
        //            SetEnemyGroupSector(pGroup, ubSectorID);

        if (giRequestPoints <= 0)
        { //we have no request for reinforcements, so send the group to Meduna for reassignment in the pool.
            SendGroupToPool(pGroup);
            return;
        }

        //now randomly choose who gets the reinforcements.
        // giRequestPoints is the combined sum of all the individual weights of all garrisons and patrols requesting reinforcements
        iRandom = Globals.Random.Next(giRequestPoints);

        //go through garrisons first and begin considering where the random value dictates.  If that garrison doesn't require 
        //reinforcements, it'll continue on considering all subsequent garrisons till the end of the array.  If it fails at that
        //point, it'll restart the loop at zero, and consider all garrisons to the index that was first considered by the random value.
        //            for (i = 0; i < giGarrisonArraySize; i++)
        //            {
        //                RecalculateGarrisonWeight(i);
        //                iWeight = gGarrisonGroup[i].bWeight;
        //                if (iWeight > 0)
        //                {   //if group is requesting reinforcements.
        //                    if (iRandom < iWeight)
        //                    {
        //                        if (!gGarrisonGroup[i].ubPendingGroupID &&
        //                                EnemyPermittedToAttackSector(null, gGarrisonGroup[i].ubSectorID) &&
        //                                GarrisonRequestingMinimumReinforcements(i))
        //                        { //This is the group that gets the reinforcements!
        //                            if (ReinforcementsApproved(i, out usDefencePoints))
        //                            {
        //                                SendReinforcementsForGarrison(i, usDefencePoints, pGroup);
        //                                return;
        //                            }
        //                        }
        //                        if (iReloopLastIndex == -1)
        //                        { //go to the next garrison and clear the iRandom value so it attempts to use all subsequent groups.
        //                            iReloopLastIndex = i - 1;
        //                            iRandom = 0;
        //                        }
        //                    }
        //                    //Decrease the iRandom value until it hits 0.  When that happens, all garrisons will get considered until
        //                    //we either have a match or process all of the garrisons.
        //                    iRandom -= iWeight;
        //                }
        //            }
        if (iReloopLastIndex >= 0)
        { //Process the loop again to the point where the original random slot started considering, and consider
          //all of the garrisons.  If this fails, all patrol groups will be considered next.
          //                for (i = 0; i <= iReloopLastIndex; i++)
          //                {
          //                    RecalculateGarrisonWeight(i);
          //                    iWeight = gGarrisonGroup[i].bWeight;
          //                    if (iWeight > 0)
          //                    {   //if group is requesting reinforcements.
          //                        if (!gGarrisonGroup[i].ubPendingGroupID &&
          //                                EnemyPermittedToAttackSector(null, gGarrisonGroup[i].ubSectorID) &&
          //                                GarrisonRequestingMinimumReinforcements(i))
          //                        { //This is the group that gets the reinforcements!
          //                            if (ReinforcementsApproved(i, out usDefencePoints))
          //                            {
          //                                SendReinforcementsForGarrison(i, usDefencePoints, pGroup);
          //                                return;
          //                            }
          //                        }
          //                    }
          //                }
        }
        if (iReloopLastIndex == -1)
        {
            //go through the patrol groups
            //            for (i = 0; i < giPatrolArraySize; i++)
            //            {
            //                RecalculatePatrolWeight(i);
            //                iWeight = gPatrolGroup[i].bWeight;
            //                if (iWeight > 0)
            //                {
            //                    if (iRandom < iWeight)
            //                    {
            //                        if (!gPatrolGroup[i].ubPendingGroupID && PatrolRequestingMinimumReinforcements(i))
            //                        { //This is the group that gets the reinforcements!
            //                            SendReinforcementsForPatrol(i, pGroup);
            //                            return;
            //                        }
            //                    }
            //                    if (iReloopLastIndex == -1)
            //                    {
            //                        iReloopLastIndex = i - 1;
            //                        iRandom = 0;
            //                    }
            //                    iRandom -= iWeight;
            //                }
            //            }
            //        }
            //        else
            //        {
            //            iReloopLastIndex = giPatrolArraySize - 1;
            //        }
            //
            //        for (i = 0; i <= iReloopLastIndex; i++)
            //        {
            //            RecalculatePatrolWeight(i);
            //            iWeight = gPatrolGroup[i].bWeight;
            //            if (iWeight > 0)
            //            {
            //                if (!gPatrolGroup[i].ubPendingGroupID && PatrolRequestingMinimumReinforcements(i))
            //                { //This is the group that gets the reinforcements!
            //                    SendReinforcementsForPatrol(i, pGroup);
            //                    return;
            //                }
            //            }
            //        }
            TransferGroupToPool(pGroup);
        }
    }

    //When an enemy AI group is eliminated by the player, apply a grace period in which the 
    //group isn't allowed to be filled for several days.
    void TagSAIGroupWithGracePeriod(GROUP? pGroup)
    {
        int iPatrolID;
        if (pGroup is not null)
        {
            iPatrolID = FindPatrolGroupIndexForGroupID(pGroup.ubGroupID);
            if (iPatrolID != -1)
            {
                switch (gGameOptions.ubDifficultyLevel)
                {
                    case DifficultyLevel.Easy:
                        gPatrolGroup[iPatrolID].bFillPermittedAfterDayMod100 = (int)((GameClock.GetWorldDay() + EASY_PATROL_GRACE_PERIOD_IN_DAYS) % 100);
                        break;
                    case DifficultyLevel.Medium:
                        gPatrolGroup[iPatrolID].bFillPermittedAfterDayMod100 = (int)((GameClock.GetWorldDay() + NORMAL_PATROL_GRACE_PERIOD_IN_DAYS) % 100);
                        break;
                    case DifficultyLevel.Hard:
                        gPatrolGroup[iPatrolID].bFillPermittedAfterDayMod100 = (int)((GameClock.GetWorldDay() + HARD_PATROL_GRACE_PERIOD_IN_DAYS) % 100);
                        break;
                }
            }
        }
    }

    public static bool PermittedToFillPatrolGroup(int iPatrolID)
    {
        int iDay = 0;
        int iDayAllowed;
        //        iDay = GetWorldDay();
        iDayAllowed = gPatrolGroup[iPatrolID].bFillPermittedAfterDayMod100 + iDay / 100 * 100;
        return iDay >= iDayAllowed;
    }

    void RepollSAIGroup(GROUP? pGroup)
    {
        int i;
        Debug.Assert(!pGroup.fPlayer);
        //        if (GroupAtFinalDestination(pGroup))
        //        {
        //            EvaluateGroupSituation(pGroup);
        //            return;
        //        }
        for (i = 0; i < giPatrolArraySize; i++)
        {
            if (gPatrolGroup[i].ubGroupID == pGroup.ubGroupID)
            {
                RecalculatePatrolWeight(i); //in case there are any dead enemies
                                            //                CalculateNextMoveIntention(pGroup);
                return;
            }
        }
        //        for (i = 0; i < giGarrisonArraySize; i++)
        //        {
        //            //KM : August 6, 1999 Patch fix
        //            //     Ack, wasn't checking for the matching group to garrison
        //            if (gGarrisonGroup[i].ubPendingGroupID == pGroup.ubGroupID)
        //            //end
        //            {
        //                RecalculateGarrisonWeight(i); //in case there are any dead enemies
        ////                CalculateNextMoveIntention(pGroup);
        //                return;
        //            }
        //        }
    }

    public static void ClearPreviousAIGroupAssignment(GROUP pGroup)
    {
        int i;
        for (i = 0; i < giPatrolArraySize; i++)
        {
            if (gPatrolGroup[i].ubGroupID == pGroup.ubGroupID)
            {
                gPatrolGroup[i].ubGroupID = 0;
                RecalculatePatrolWeight(i);
                return;
            }
            if (gPatrolGroup[i].ubPendingGroupID == pGroup.ubGroupID)
            {
                gPatrolGroup[i].ubPendingGroupID = 0;
                return;
            }
        }
        //Also check if this group was a garrison's pending group
        //        for (i = 0; i < giGarrisonArraySize; i++)
        //        {
        //            if (gGarrisonGroup[i].ubPendingGroupID == pGroup.ubGroupID)
        //            {
        //                gGarrisonGroup[i].ubPendingGroupID = 0;
        //                return;
        //            }
        //        }
    }

    public static void CalcNumTroopsBasedOnComposition(int? pubNumTroops, int? pubNumElites, int ubTotal, Garrisons iCompositionID)
    {
        pubNumTroops = gArmyComp[iCompositionID].bTroopPercentage * ubTotal / 100;
        pubNumElites = gArmyComp[iCompositionID].bElitePercentage * ubTotal / 100;

        //Due to low roundoff, it is highly possible that we will be short one soldier.
        while (pubNumTroops + pubNumElites < ubTotal)
        {
            //            if (Chance(gArmyComp[iCompositionID].bTroopPercentage))
            //            {
            //                (pubNumTroops)++;
            //            }
            //            else
            //            {
            //                (pubNumElites)++;
            //            }
        }
        Debug.Assert(pubNumTroops + pubNumElites == ubTotal);
    }

    void ConvertGroupTroopsToComposition(GROUP? pGroup, int iCompositionID)
    {
        //        Debug.Assert(pGroup);
        //        Debug.Assert(!pGroup.fPlayer);
        //        CalcNumTroopsBasedOnComposition(&pGroup.pEnemyGroup.ubNumTroops, &pGroup.pEnemyGroup.ubNumElites, pGroup.ubGroupSize, iCompositionID);
        //        pGroup.pEnemyGroup.ubNumAdmins = 0;
        //        pGroup.ubGroupSize = pGroup.pEnemyGroup.ubNumTroops + pGroup.pEnemyGroup.ubNumElites;
        //        ValidateLargeGroup(pGroup);
    }

    public static void RemoveSoldiersFromGarrisonBasedOnComposition(int iGarrisonID, int ubSize)
    {
        SECTORINFO? pSector;
        Garrisons iCompositionID;
        int ubNumTroops = 0, ubNumElites = 0;

        //debug stuff
        int ubOrigSectorAdmins;
        int ubOrigSectorTroops;
        int ubOrigSectorElites;
        int ubOrigNumElites;
        int ubOrigNumTroops;
        int ubOrigSize;

        iCompositionID = gGarrisonGroup[iGarrisonID].ubComposition;

        //        CalcNumTroopsBasedOnComposition(ubNumTroops, ubNumElites, ubSize, iCompositionID);
        pSector = SectorInfo[gGarrisonGroup[iGarrisonID].ubSectorID];
        //if there are administrators in this sector, remove them first.

        ubOrigNumElites = ubNumElites;
        ubOrigNumTroops = ubNumTroops;
        ubOrigSectorAdmins = pSector.ubNumAdmins;
        ubOrigSectorTroops = pSector.ubNumTroops;
        ubOrigSectorElites = pSector.ubNumElites;
        ubOrigSize = ubSize;

        while (ubSize > 0 && pSector.ubNumAdmins > 0)
        {
            pSector.ubNumAdmins--;
            ubSize--;
            if (ubNumTroops > 0)
            {
                ubNumTroops--;
            }
            else
            {
                ubNumElites--;
            }
        }
        //No administrators are left.  

        //Eliminate the troops
        while (ubNumTroops > 0)
        {
            if (pSector.ubNumTroops > 0)
            {
                pSector.ubNumTroops--;
            }
            else if (pSector.ubNumElites > 0)
            {
                pSector.ubNumElites--;
            }
            else
            {
                Debug.Assert(false);
            }
            ubNumTroops--;
        }

        //Eliminate the elites 
        while (ubNumElites > 0)
        {
            if (pSector.ubNumElites > 0)
            {
                pSector.ubNumElites--;
            }
            else if (pSector.ubNumTroops > 0)
            {
                pSector.ubNumTroops--;
            }
            else
            {
                Debug.Assert(false);
            }
            ubNumElites--;
        }

        RecalculateGarrisonWeight(iGarrisonID);
    }

    public static void MoveSAIGroupToSector(GROUP? pGroup, SEC ubSectorID, SAIMOVECODE uiMoveCode, ENEMY_INTENTIONS ubIntention)
    {
        int ubDstSectorX;
        MAP_ROW ubDstSectorY;

        ubDstSectorX = SECTORINFO.SECTORX(ubSectorID);
        ubDstSectorY = SECTORINFO.SECTORY(ubSectorID);

        if (pGroup.fBetweenSectors)
        {
            //            SetEnemyGroupSector(pGroup, SECTORINFO.SECTOR((pGroup).ubSectorX, (pGroup).ubSectorY));
        }

        //        (pGroup).pEnemyGroup.ubIntention = ubIntention;
        pGroup.ubMoveType = MOVE_TYPES.ONE_WAY;

        if (ubIntention == ENEMY_INTENTIONS.PURSUIT)
        {   //Make sure that the group isn't moving into a garrison sector.  These sectors should be using ASSAULT intentions!
            if (SectorInfo[ubSectorID].ubGarrisonID != NO_GARRISON)
            {
                //Good place for a breakpoint.
                pGroup = pGroup;
            }
        }

        if (pGroup.ubSectorX == ubDstSectorX && pGroup.ubSectorY == ubDstSectorY)
        { //The destination sector is the current location.  Instead of causing code logic problems,
          //simply process them as if they just arrived.
            if (EvaluateGroupSituation(pGroup))
            { //The group was deleted.
                pGroup = null;
                return;
            }
        }

        switch (uiMoveCode)
        {
            case SAIMOVECODE.STAGE:
                //                MoveGroupFromSectorToSectorButAvoidPlayerInfluencedSectorsAndStopOneSectorBeforeEnd((pGroup).ubGroupID, (pGroup).ubSectorX, (pGroup).ubSectorY, ubDstSectorX, ubDstSectorY);
                break;
            case SAIMOVECODE.EVASIVE:
                //                MoveGroupFromSectorToSectorButAvoidPlayerInfluencedSectors((pGroup).ubGroupID, (pGroup).ubSectorX, (pGroup).ubSectorY, ubDstSectorX, ubDstSectorY);
                break;
            case SAIMOVECODE.DIRECT:
            default:
                //                MoveGroupFromSectorToSector((pGroup).ubGroupID, (pGroup).ubSectorX, (pGroup).ubSectorY, ubDstSectorX, ubDstSectorY);
                break;
        }
        //Make sure that the group is moving.  If this fails, then the pathing may have failed for some reason.
        ValidateGroup(pGroup);
    }

    //If there are any enemy groups that will be moving through this sector due, they will have to repath which
    //will cause them to avoid the sector.  Returns the number of redirected groups.
    public static int RedirectEnemyGroupsMovingThroughSector(int ubSectorX, int ubSectorY)
    {
        GROUP? pGroup = null;
        int ubNumGroupsRedirected = 0;
        WAYPOINT? pWaypoint;
        SEC ubDestSectorID;
        //        pGroup = gpGroupList;
        while (pGroup is not null)
        {
            if (!pGroup.fPlayer && pGroup.ubMoveType == MOVE_TYPES.ONE_WAY)
            {
                //check the waypoint list
                //                if (GroupWillMoveThroughSector(pGroup, ubSectorX, ubSectorY))
                //                {
                //                    //extract the group's destination.
                //                    pWaypoint = GetFinalWaypoint(pGroup);
                //                    Debug.Assert(pWaypoint);
                //                    ubDestSectorID = SECTORINFO.SECTOR(pWaypoint.x, pWaypoint.y);
                //                    SetEnemyGroupSector(pGroup, SECTORINFO.SECTOR(pGroup.ubSectorX, pGroup.ubSectorY));
                //                    MoveSAIGroupToSector(pGroup, ubDestSectorID, SAIMOVECODE.EVASIVE, pGroup.pEnemyGroup.ubIntention);
                //                    ubNumGroupsRedirected++;
                //                }
            }
            pGroup = pGroup.next;
        }

        //        if (ubNumGroupsRedirected)
        //        {
        //            Messages.ScreenMsg(FontColor.FONT_LTBLUE, MSG.BETAVERSION, "Test message for new feature:  %d enemy groups were redirected away from moving through sector %c%d.  Please don't report unless this number is greater than 5.",
        //                ubNumGroupsRedirected, ubSectorY.ToString() + "A", ubSectorX.ToString());
        //        }
        return ubNumGroupsRedirected;
    }

    //when the SAI compositions change, it is necessary to call this function upon version load,
    //to reflect the changes of the compositions to the sector that haven't been visited yet.
    public static void ReinitializeUnvisitedGarrisons()
    {
        SECTORINFO? pSector;
        ARMY_COMPOSITION? pArmyComp;
        GROUP? pGroup;
        int i, cnt, iEliteChance, iAdminChance;

        //Recreate the compositions
        //                memcpy(gArmyComp, gOrigArmyComp, NUM_ARMY_COMPOSITIONS * sizeof(ARMY_COMPOSITION));
        //        EvolveQueenPriorityPhase(true);

        //Go through each unvisited sector and recreate the garrison forces based on 
        //the desired population.
        //        for (i = 0; i < giGarrisonArraySize; i++)
        //        {
        //            if (gGarrisonGroup[i].ubComposition >= Garrisons.LEVEL1_DEFENCE && gGarrisonGroup[i].ubComposition <= Garrisons.LEVEL3_DEFENCE)
        //            { //These 3 compositions make up the perimeter around Meduna.  The existance of these are based on the
        //              //difficulty level, and we don't want to reset these anyways, due to the fact that many of the reinforcements
        //              //come from these sectors, and it could potentially add upwards of 150 extra troops which would seriously
        //              //unbalance the difficulty.
        //                continue;
        //            }
        //            pSector = SectorInfo[gGarrisonGroup[i].ubSectorID];
        //            pArmyComp = &gArmyComp[gGarrisonGroup[i].ubComposition];
        //            if (!(pSector.uiFlags & SF.ALREADY_VISITED))
        //            {
        //                pSector.ubNumAdmins = 0;
        //                pSector.ubNumTroops = 0;
        //                pSector.ubNumElites = 0;
        //                if (gfQueenAIAwake)
        //                {
        //                    cnt = pArmyComp.bDesiredPopulation;
        //                }
        //                else
        //                {
        //                    cnt = pArmyComp.bStartPopulation;
        //                }
        //
        //                if (gGarrisonGroup[i].ubPendingGroupID)
        //                { //if the garrison has reinforcements on route, then subtract the number of 
        //                  //reinforcements from the value we reset the size of the garrison.  This is to 
        //                  //prevent overfilling the group.
        //                    pGroup = StrategicMovement.GetGroup(gGarrisonGroup[i].ubPendingGroupID);
        //                    if (pGroup is not null)
        //                    {
        //                        cnt -= pGroup.ubGroupSize;
        //                        cnt = Math.Max(cnt, 0);
        //                    }
        //                }
        //
        //                iEliteChance = pArmyComp.bElitePercentage;
        //                iAdminChance = pArmyComp.bAdminPercentage;
        //                if (iAdminChance && !gfQueenAIAwake && cnt)
        //                {
        //                    pSector.ubNumAdmins = iAdminChance * cnt / 100;
        //                }
        //                else
        //                {
        //                    while (cnt-- > 0)
        //                    { //for each person, randomly determine the types of each soldier.
        //                        if (Chance(iEliteChance))
        //                        {
        //                            pSector.ubNumElites++;
        //                        }
        //                        else
        //                        {
        //                            pSector.ubNumTroops++;
        //                        }
        //                    }
        //                }
        //            }
        //        }
    }

    GROUP? FindPendingGroupForGarrisonSector(SEC ubSectorID)
    {
        GROUP? pGroup;
        SECTORINFO? pSector = SectorInfo[ubSectorID];

        if (pSector.ubGarrisonID != NO_GARRISON)
        {
            if (gGarrisonGroup[(int)pSector.ubGarrisonID].ubPendingGroupID > 0)
            {
                pGroup = StrategicMovement.GetGroup(gGarrisonGroup[(int)pSector.ubGarrisonID].ubPendingGroupID);
                Debug.Assert(pGroup is not null);
                return pGroup;
            }
        }
        return null;
    }
}

//These enumerations define all of the various types of stationary garrison
//groups, and index their compositions for forces, etc.
public enum Garrisons
{
    QUEEN_DEFENCE,          //The most important sector, the queen's palace.
    MEDUNA_DEFENCE,         //The town surrounding the queen's palace.
    MEDUNA_SAMSITE,         //A sam site within Meduna (higher priority)
    LEVEL1_DEFENCE,         //The sectors immediately adjacent to Meduna (defence and spawning area)
    LEVEL2_DEFENCE,         //Two sectors away from Meduna (defence and spawning area)
    LEVEL3_DEFENCE,         //Three sectors away from Meduna (defence and spawning area)
    ORTA_DEFENCE,               //The top secret military base containing lots of elites
    EAST_GRUMM_DEFENCE, //The most-industrial town in Arulco (more mine income)
    WEST_GRUMM_DEFENCE, //The most-industrial town in Arulco (more mine income)
    GRUMM_MINE,
    OMERTA_WELCOME_WAGON,//Small force that greets the player upon arrival in game.
    BALIME_DEFENCE,         //Rich town, paved roads, close to Meduna (in queen's favor)
    TIXA_PRISON,                //Prison, well defended, but no point in retaking
    TIXA_SAMSITE,               //The central-most sam site (important for queen to keep)
    ALMA_DEFENCE,               //The military town of Meduna.  Also very important for queen.
    ALMA_MINE,                  //Mine income AND administrators
    CAMBRIA_DEFENCE,        //Medical town, large, central.
    CAMBRIA_MINE,
    CHITZENA_DEFENCE,       //Small town, small mine, far away.
    CHITZENA_MINE,
    CHITZENA_SAMSITE,       //Sam site near Chitzena.
    DRASSEN_AIRPORT,        //Very far away, a supply depot of little importance.
    DRASSEN_DEFENCE,        //Medium town, normal.
    DRASSEN_MINE,
    DRASSEN_SAMSITE,        //Sam site near Drassen (least importance to queen of all samsites)
    ROADBLOCK,                  //General outside city roadblocks -- enhance chance of ambush?
    SANMONA_SMALL,
    NUM_ARMY_COMPOSITIONS,

    UNSET = -1,
};

public enum SAIMOVECODE
{
    DIRECT,
    EVASIVE,
    STAGE,
};
