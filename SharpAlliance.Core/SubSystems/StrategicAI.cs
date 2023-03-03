using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core.SubSystems;

public class StrategicAI
{
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
    NUM_ARMY_COMPOSITIONS
};
