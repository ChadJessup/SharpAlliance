﻿using System.Collections.Generic;
using SharpAlliance.Core.Screens;
using SharpAlliance.Core.SubSystems;

namespace SharpAlliance.Core;

public partial class Globals
{
    public const int NUM_TERRORISTS = 6;

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

    public static Dictionary<NPCID, Dictionary<int, MAP_ROW>> gsTerroristSector = new()
    {
        {
            // Elgin... preplaced
            NPCID.DRUGGIST,
            new()
            {
                { 0, 0 },
                { 0, 0 },
                { 0, 0 },
                { 0, 0 },
                { 0, 0 },
            }
        },
        {
            NPCID.SLAY,
            new()
            {
                { 9,    MAP_ROW.F },
                { 14,   MAP_ROW.I },
                { 1,    MAP_ROW.G },
                { 2,    MAP_ROW.G },
                { 8,    MAP_ROW.G }
            }
        },
        {
	        // Matron
            NPCID.ANNIE,
            new()
            {
                { 14,   MAP_ROW.I },
                { 6,    MAP_ROW.C },
                { 2,    MAP_ROW.B },
                { 11,   MAP_ROW.L },
                { 8,    MAP_ROW.G },
            }
        },
        {
	        // Imposter
            NPCID.CHRIS,
            new()
            {
                { 1,    MAP_ROW.G },
                { 9,    MAP_ROW.F },
                { 11,   MAP_ROW.L },
                { 8,    MAP_ROW.G },
                { 2,    MAP_ROW.G },
            }
        },
        {
            // Tiffany
            NPCID.TIFFANY,
            new()
            {
                { 14,   MAP_ROW.I },
                { 2,    MAP_ROW.G },
                { 14,   MAP_ROW.H },
                { 6,    MAP_ROW.C },
                { 2,    MAP_ROW.B },
            }
        },
        {
            // Rexall
            NPCID.T_REX,
            new()
            {
                { 9,    MAP_ROW.F },
                { 14,   MAP_ROW.H },
                { 2,    MAP_ROW.H },
                { 1,    MAP_ROW.G },
                { 2,    MAP_ROW.B }
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