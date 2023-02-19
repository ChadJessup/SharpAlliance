using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpAlliance.Core.SubSystems;

public class Squads
{
    public static Squad iCurrentTacticalSquad = Squad.FIRST_SQUAD;


    public static Squad CurrentSquad()
    {
        // returns which squad is current squad

        return (iCurrentTacticalSquad);
    }

}


// enums for squads
public enum Squad
{
    FIRST_SQUAD = 0,
    SECOND_SQUAD,
    THIRD_SQUAD,
    FOURTH_SQUAD,
    FIFTH_SQUAD,
    SIXTH_SQUAD,
    SEVENTH_SQUAD,
    EIGTH_SQUAD,
    NINTH_SQUAD,
    TENTH_SQUAD,
    ELEVENTH_SQUAD,
    TWELTH_SQUAD,
    THIRTEENTH_SQUAD,
    FOURTEENTH_SQUAD,
    FIFTHTEEN_SQUAD,
    SIXTEENTH_SQUAD,
    SEVENTEENTH_SQUAD,
    EIGTHTEENTH_SQUAD,
    NINTEENTH_SQUAD,
    TWENTYTH_SQUAD,
    NUMBER_OF_SQUADS,
}
