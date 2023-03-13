using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpAlliance.Core.SubSystems;

namespace SharpAlliance.Core;

public partial class Globals
{

    // min condition for sam site to be functional
    public const int MIN_CONDITION_FOR_SAM_SITE_TO_WORK = 80;


    public const int FOOT_TRAVEL_TIME = 89;
    public const int CAR_TRAVEL_TIME = 30;
    public const int TRUCK_TRAVEL_TIME = 32;
    public const int TRACKED_TRAVEL_TIME = 46;
    public const int AIR_TRAVEL_TIME = 10;


    // FUNCTIONS FOR DERTERMINING GOOD SECTOR EXIT DATA
    public const int CHECK_DIR_X_DELTA = (WORLD_TILE_X * 4);
    public const int CHECK_DIR_Y_DELTA = (WORLD_TILE_Y * 10);

    // get index into aray
    public static int CALCULATE_STRATEGIC_INDEX(int x, MAP_ROW y) => (x + ((int)y * MAP_WORLD_X));
    public static int GET_X_FROM_STRATEGIC_INDEX(int i) => (i % MAP_WORLD_X);
    public static MAP_ROW GET_Y_FROM_STRATEGIC_INDEX(int i) => (MAP_ROW)(i / MAP_WORLD_X);

    // macros to convert between the 2 different sector numbering systems
    public static int SECTOR_INFO_TO_STRATEGIC_INDEX(SEC i) => (CALCULATE_STRATEGIC_INDEX(SECTORINFO.SECTORX(i), SECTORINFO.SECTORY(i)));
    public static SEC STRATEGIC_INDEX_TO_SECTOR_INFO(int i) => (SEC)(SECTORINFO.SECTOR(GET_X_FROM_STRATEGIC_INDEX(i), GET_Y_FROM_STRATEGIC_INDEX(i)));

    public static bool gfGettingNameFromSaveLoadScreen;

    public static GROUP? gpAdjacentGroup = null;
    public static int gsAdjacentSectorX;
    public static MAP_ROW gsAdjacentSectorY;
    public static int gbAdjacentSectorZ;
    public static int gubAdjacentJumpCode;
    public static int guiAdjacentTraverseTime;
    public static int gubTacticalDirection;
    public static int gsAdditionalData;
    public static int gusDestExitGridNo;

    public static bool fUsingEdgePointsForStrategicEntry = false;

    public static int gbGreenToElitePromotions = 0;
    public static int gbGreenToRegPromotions = 0;
    public static int gbRegToElitePromotions = 0;
    public static int gbMilitiaPromotions = 0;

    public static bool gfUseAlternateMap = false;
    // whether or not we have found Orta yet
    public static bool fFoundOrta = false;

    // have any of the sam sites been found
    public static Dictionary<SAM_SITE, bool> fSamSiteFound = new()
    {
        { SAM_SITE.ONE, false },
        { SAM_SITE.TWO, false },
        { SAM_SITE.THREE, false },
        { SAM_SITE.FOUR, false },
    };

    public static SEC[] pSamList =
    {
        SECTORINFO.SECTOR((int)SAM.SAM_1_X, (MAP_ROW)SAM.SAM_1_Y),
        SECTORINFO.SECTOR((int)SAM.SAM_2_X, (MAP_ROW)SAM.SAM_2_Y),
        SECTORINFO.SECTOR((int)SAM.SAM_3_X, (MAP_ROW)SAM.SAM_3_Y),
        SECTORINFO.SECTOR((int)SAM.SAM_4_X, (MAP_ROW)SAM.SAM_4_Y),
    };

    public static int[] pSamGridNoAList =
    {
        10196,
        11295,
        16080,
        11913,
    };

    public static int[] pSamGridNoBList =
    {
        10195,
        11135,
        15920,
        11912,
    };

    // ATE: Update this w/ graphic used 
    // Use 3 if / orientation, 4 if \ orientation
    public int[] gbSAMGraphicList =
    {
        4,
        3,
        3,
        3,
    };

    // the amount of time that a soldier will wait to return to desired/old squad
    public const int DESIRE_SQUAD_RESET_DELAY = 12 * 60;

    public int[,] ubSAMControlledSectors =
    {
    //     1  2  3  4  5  6  7  8  9  10 11 12 13 14 15 16
      { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
      { 0, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 2, 2, 2, 2, 2, 2, 0 }, // A
      { 0, 1, 1, 1, 1, 1, 1, 1, 1, 2, 2, 2, 2, 2, 2, 2, 2, 0 }, // B
      { 0, 1, 1, 1, 1, 1, 1, 1, 3, 2, 2, 2, 2, 2, 2, 2, 2, 0 }, // C
      { 0, 1, 1, 1, 1, 1, 1, 1, 3, 3, 2, 2, 2, 2, 2, 2, 2, 0 }, // D
      { 0, 1, 1, 1, 1, 1, 1, 3, 3, 3, 3, 2, 2, 2, 2, 2, 2, 0 }, // E
      { 0, 1, 1, 1, 1, 1, 3, 3, 3, 3, 3, 3, 2, 2, 2, 2, 2, 0 }, // F
      { 0, 1, 1, 1, 1, 3, 3, 3, 3, 3, 3, 3, 3, 2, 2, 2, 2, 0 }, // G
      { 0, 1, 1, 1, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 2, 2, 2, 0 }, // H
      { 0, 1, 1, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 2, 2, 2, 0 }, // I
      { 0, 1, 4, 4, 4, 3, 3, 3, 3, 3, 3, 3, 3, 3, 2, 2, 2, 0 }, // J
      { 0, 4, 4, 4, 4, 4, 3, 3, 3, 3, 3, 3, 3, 3, 2, 2, 2, 0 }, // K
      { 0, 4, 4, 4, 4, 4, 4, 3, 3, 3, 3, 3, 3, 3, 2, 2, 2, 0 }, // L
      { 0, 4, 4, 4, 4, 4, 4, 4, 3, 3, 3, 3, 3, 3, 2, 2, 2, 0 }, // M
      { 0, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 2, 2, 2, 0 }, // N
      { 0, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 2, 2, 0 }, // O
      { 0, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 2, 0 }, // P
      { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
    };

    public static int[] DirXIncrementer =
    {
         0,        //N
         1,        //NE
         1,        //E
         1,        //SE
         0,         //S
        -1,       //SW
        -1,       //W
        -1,       //NW
    };

    public static int[] DirYIncrementer =
    {
        -1, //N
        -1, //NE
        0,  //E
        1,  //SE
        1,  //S
        1,  //SW
        0,  //W
        -1, //NW
    };


    public static string[] pVertStrings =
    {
        "X",
        "A",
        "B",
        "C",
        "D",
        "E",
        "F",
        "G",
        "H",
        "I",
        "J",
        "K",
        "L",
        "M",
        "N",
        "O",
        "P",
        "Q",
        "R",
    };

    public static string[] pHortStrings =
    {
        "X",
        "1",
        "2",
        "3",
        "4",
        "5",
        "6",
        "7",
        "8",
        "9",
        "10",
        "11",
        "12",
        "13",
        "14",
        "15",
        "16",
        "17",
    };
}

public enum SAM
{
    // SAM sites
    SAM_1_X = 2,
    SAM_2_X = 15,
    SAM_3_X = 8,
    SAM_4_X = 4,

    SAM_1_Y = 4,
    SAM_2_Y = 4,
    SAM_3_Y = 9,
    SAM_4_Y = 14,
}
