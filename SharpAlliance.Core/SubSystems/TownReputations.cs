using System;

namespace SharpAlliance.Core.SubSystems
{
    public class TownReputations
    {
        // init for town reputation at game start
        public const int INITIAL_TOWN_REPUTATION = 0;

        // the max and min town opinion of an individual merc can be
        public const int MAX_TOWN_OPINION = 50;
        public const int MIN_TOWN_OPINION = -50;

        // town reputation is currently updated 4x per day: at 9am, noon, 3pm, and 6pm

        // the number of events per day
        public const int TOWN_OPINION_NUMBER_OF_PERIODS = 4;

        // the start time after midnight that first town reputation event takes place .. in minutes
        public const int TOWN_OPINION_START_TIME = 9 * 60;

        // how often the town opinion events occur...right now every 3 hours
        public const int TOWN_OPINION_PERIOD = 3 * 60;

        public TownReputations()
        {
        }

        public void InitializeProfilesForTownReputation()
        {
            // initialize the town opinion values in each recruitable merc's profile structure
            for (int uiProfileId = 0; uiProfileId < SoldierProfileSubSystem.Constants.FIRST_NPC; uiProfileId++)
            {
                var npcId = (NPCIDs)uiProfileId;
                // set to 0 by default
                // this.soldiers.gMercProfiles[npcId].bMercTownReputation; INITIAL_TOWN_REPUTATION;
            }
        }
    }
}
