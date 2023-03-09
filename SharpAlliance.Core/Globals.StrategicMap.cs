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
    public static int GET_X_FROM_STRATEGIC_INDEX(int i ) => (i % MAP_WORLD_X);
    public static MAP_ROW GET_Y_FROM_STRATEGIC_INDEX(int i ) => (MAP_ROW)(i / MAP_WORLD_X);

    // macros to convert between the 2 different sector numbering systems
    public static int SECTOR_INFO_TO_STRATEGIC_INDEX(SEC i ) => (CALCULATE_STRATEGIC_INDEX(SECTORINFO.SECTORX(i), SECTORINFO.SECTORY(i)));
    public static SEC STRATEGIC_INDEX_TO_SECTOR_INFO(int i ) => (SEC)(SECTORINFO.SECTOR(GET_X_FROM_STRATEGIC_INDEX(i), GET_Y_FROM_STRATEGIC_INDEX(i)));
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
