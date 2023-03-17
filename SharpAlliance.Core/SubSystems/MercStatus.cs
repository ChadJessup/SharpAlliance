using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core.SubSystems;

public enum MercStatus
{
    //Merc is ready
    MERC_OK = 0,

    //if the merc doesnt have a EDT file
    MERC_HAS_NO_TEXT_FILE = -1,

    //used in the aim video conferencing screen
    MERC_ANNOYED_BUT_CAN_STILL_CONTACT = -2,
    MERC_ANNOYED_WONT_CONTACT = -3,
    MERC_HIRED_BUT_NOT_ARRIVED_YET = -4,

    //self explanatory
    MERC_IS_DEAD = -5,

    //set when the merc is returning home.  A delay for 1,2 or 3 days
    MERC_RETURNING_HOME = -6,

    // used when merc starts game on assignment, goes on assignment later, or leaves to go on another contract
    MERC_WORKING_ELSEWHERE = -7,

    //When the merc was fired, they were a POW, make sure they dont show up in AIM, or MERC as available
    MERC_FIRED_AS_A_POW = -8,
}
