using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpAlliance.Core.SubSystems;

namespace SharpAlliance.Core;

public partial class Globals
{
    //Using the ESC key in the PBI will get rid of the PBI and go back to mapscreen, but 
    //only if the PBI isn't persistant (!gfPersistantPBI).
    public static bool gfPersistantPBI;

    //Contains general information about the type of encounter the player is faced with.  This
    //determines whether or not you can autoresolve the battle or even retreat.  This code
    //dictates the header that is used at the top of the PBI.
    public static ENCOUNTER_CODE gubEnemyEncounterCode;

    //The autoresolve during tactical battle option needs more detailed information than the 
    //gubEnemyEncounterCode can provide.  The explicit version contains possibly unique codes
    //for reasons not normally used in the PBI.  For example, if we were fighting the enemy
    //in a normal situation, then shot at a civilian, the civilians associated with the victim
    //would turn hostile, which would disable the ability to autoresolve the battle.
    public static ENCOUNTER_CODE gubExplicitEnemyEncounterCode;

    //Location of the current battle (determines where the animated icon is blitted) and if the
    //icon is to be blitted.
    public static bool gfBlitBattleSectorLocator;

    public static int gubPBSectorX;
    public static MAP_ROW gubPBSectorY;
    public static int gubPBSectorZ;

    public static bool gfCantRetreatInPBI;
    //SAVE END
    public static bool gfDisplayPotentialRetreatPaths = false;
    public static int gusRetreatButtonLeft, gusRetreatButtonTop, gusRetreatButtonRight, gusRetreatButtonBottom;

    public static List<GROUP> gpBattleGroup = new();
}

public enum ENCOUNTER_CODE
{
    //General encounter codes (gubEnemyEncounterCode)
    NO_ENCOUNTER_CODE,          //when there is no encounter
    ENEMY_INVASION_CODE,
    ENEMY_ENCOUNTER_CODE,
    ENEMY_AMBUSH_CODE,
    ENTERING_ENEMY_SECTOR_CODE,
    CREATURE_ATTACK_CODE,

    BLOODCAT_AMBUSH_CODE,
    ENTERING_BLOODCAT_LAIR_CODE,

    //Explicit encounter codes only (gubExplicitEnemyEncounterCode -- a superset of gubEnemyEncounterCode)
    FIGHTING_CREATURES_CODE,
    HOSTILE_CIVILIANS_CODE,
    HOSTILE_BLOODCATS_CODE,
};
