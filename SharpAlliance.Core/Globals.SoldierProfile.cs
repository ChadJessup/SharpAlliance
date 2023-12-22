using System.Collections.Generic;
using SharpAlliance.Core.Screens;
using SharpAlliance.Core.SubSystems;

namespace SharpAlliance.Core;

public record MapPoint(int X, MAP_ROW Y);

public partial class Globals
{
    // the values for categories of stats
    public const int SUPER_STAT_VALUE = 80;
    public const int NEEDS_TRAINING_STAT_VALUE = 50;
    public const int NO_CHANCE_IN_HELL_STAT_VALUE = 40;
    public const int SUPER_SKILL_VALUE = 80;
    public const int NEEDS_TRAINING_SKILL_VALUE = 50;
    public const int NO_CHANCE_IN_HELL_SKILL_VALUE = 0;

    public const int NUM_TERRORISTS = 6;

    public const int NERVOUS_RADIUS = 10;

    public const int NUM_PROFILES = 170;
    public const int MAX_ADDITIONAL_TERRORISTS = 4;
    public const int NUM_TERRORIST_POSSIBLE_LOCATIONS = 5;

    // A.I.M. is 0-39, M.E.R.C.s are 40-50
    public const int AIM_AND_MERC_MERCS = 51;
    public const NPCID FIRST_NPC = (NPCID)75;

    public const NPCID FIRST_RPC = (NPCID)57;


    public static Dictionary<SkillTrait, int> gbSkillTraitBonus = new()
    {
        { SkillTrait.NO_SKILLTRAIT,  0 },  //NO_SKILLTRAIT
        { SkillTrait.LOCKPICKING,   25 },  //LOCKPICKING
        { SkillTrait.HANDTOHAND,    15 },  //HANDTOHAND
        { SkillTrait.ELECTRONICS,   15 },  //ELECTRONICS
        { SkillTrait.NIGHTOPS,      15 },  //NIGHTOPS
        { SkillTrait.THROWING,      12 },  //THROWING
        { SkillTrait.TEACHING,      15 },  //TEACHING
        { SkillTrait.HEAVY_WEAPS,   15 },  //HEAVY_WEAPS
        { SkillTrait.AUTO_WEAPS,     0 },  //AUTO_WEAPS
        { SkillTrait.STEALTHY,      15 },  //STEALTHY
        { SkillTrait.AMBIDEXT,       0 },  //AMBIDEXT
        { SkillTrait.THIEF,          0 },  //THIEF				// UNUSED!
        { SkillTrait.MARTIALARTS,   30 },  //MARTIALARTS
        { SkillTrait.KNIFING,       30 },  //KNIFING
        { SkillTrait.ONROOF,        15 },  //ONROOF
        { SkillTrait.CAMOUFLAGED,    0 },  //CAMOUFLAGED
    };

    public static InventorySlot[] gubBasicInventoryPositions =
    {
        InventorySlot.HELMETPOS,
        InventorySlot.VESTPOS,
        InventorySlot.LEGPOS,
        InventorySlot.HANDPOS,
        InventorySlot.BIGPOCK1POS,
        InventorySlot.BIGPOCK2POS,
        InventorySlot.BIGPOCK3POS,
        InventorySlot.BIGPOCK4POS
    };

    public static NPCID[] gubTerrorists =
    {
        NPCID.DRUGGIST,
        NPCID.SLAY,
        NPCID.ANNIE,
        NPCID.CHRIS,
        NPCID.TIFFANY,
        NPCID.T_REX,
        0
    };

    public static int gubNumTerrorists = 0;

    public static Dictionary<NPCID, List<MapPoint>> gsTerroristSector = new()
    {
        {
            // Elgin... preplaced
            NPCID.DRUGGIST,
            new()
            {
                new(0, 0),
                new(0, 0),
                new(0, 0),
                new(0, 0),
                new(0, 0),
            }
        },
        {
            NPCID.SLAY,
            new()
            {
                new(9,  MAP_ROW.F),
                new(14, MAP_ROW.I),
                new(1,  MAP_ROW.G),
                new(2,  MAP_ROW.G),
                new(8,  MAP_ROW.G),
            }
        },
        {
	        // Matron
            NPCID.ANNIE,
            new()
            {
                new(14, MAP_ROW.I),
                new(6,  MAP_ROW.C),
                new(2,  MAP_ROW.B),
                new(11, MAP_ROW.L),
                new(8,  MAP_ROW.G),
            }
        },
        {
	        // Imposter
            NPCID.CHRIS,
            new()
            {
                new(1,  MAP_ROW.G),
                new(9,  MAP_ROW.F),
                new(11, MAP_ROW.L),
                new(8,  MAP_ROW.G),
                new(2,  MAP_ROW.G),
            }
        },
        {
            // Tiffany
            NPCID.TIFFANY,
            new()
            {
                new(14, MAP_ROW.I),
                new(2,  MAP_ROW.G),
                new(14, MAP_ROW.H),
                new(6,  MAP_ROW.C),
                new(2,  MAP_ROW.B),
            }
        },
        {
            // Rexall
            NPCID.T_REX,
            new()
            {
                new(9,  MAP_ROW.F),
                new(14, MAP_ROW.H),
                new(2,  MAP_ROW.H),
                new(1,  MAP_ROW.G),
                new(2,  MAP_ROW.B),
            }
        }
    };

    public static int gsRobotGridNo;

    public const int NUM_ASSASSINS = 6;

    public static NPCID[] gubAssassins =
    {
        NPCID.JIM,
        NPCID.JACK,
        NPCID.OLAF,
        NPCID.RAY,
        NPCID.OLGA,
        NPCID.TYRONE
    };

    public const int NUM_ASSASSIN_POSSIBLE_TOWNS = 5;

    public static Dictionary<NPCID, List<TOWNS>> gbAssassinTown = new()
    {
        { NPCID.JIM,    new() { TOWNS.CAMBRIA, TOWNS.DRASSEN, TOWNS.ALMA, TOWNS.BALIME, TOWNS.GRUMM } },
        { NPCID.JACK,   new() { TOWNS.CHITZENA, TOWNS.ESTONI, TOWNS.ALMA, TOWNS.BALIME, TOWNS.GRUMM } },
        { NPCID.OLAF,   new() { TOWNS.DRASSEN, TOWNS.ESTONI, TOWNS.ALMA, TOWNS.CAMBRIA, TOWNS.BALIME } },
        { NPCID.RAY,    new() { TOWNS.CAMBRIA, TOWNS.OMERTA, TOWNS.BALIME, TOWNS.GRUMM, TOWNS.DRASSEN } },
        { NPCID.OLGA,   new() { TOWNS.CHITZENA, TOWNS.OMERTA, TOWNS.CAMBRIA, TOWNS.ALMA, TOWNS.GRUMM } },
        { NPCID.TYRONE, new() { TOWNS.CAMBRIA, TOWNS.BALIME, TOWNS.ALMA, TOWNS.GRUMM, TOWNS.DRASSEN } },
    };
}
