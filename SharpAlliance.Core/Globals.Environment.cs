namespace SharpAlliance.Core;

public partial class Globals
{
    public const int ENV_TIME_00 = 0;
    public const int ENV_TIME_01 = 1;
    public const int ENV_TIME_02 = 2;
    public const int ENV_TIME_03 = 3;
    public const int ENV_TIME_04 = 4;
    public const int ENV_TIME_05 = 5;
    public const int ENV_TIME_06 = 6;
    public const int ENV_TIME_07 = 7;
    public const int ENV_TIME_08 = 8;
    public const int ENV_TIME_09 = 9;
    public const int ENV_TIME_10 = 10;
    public const int ENV_TIME_11 = 11;
    public const int ENV_TIME_12 = 12;
    public const int ENV_TIME_13 = 13;
    public const int ENV_TIME_14 = 14;
    public const int ENV_TIME_15 = 15;
    public const int ENV_TIME_16 = 16;
    public const int ENV_TIME_17 = 17;
    public const int ENV_TIME_18 = 18;
    public const int ENV_TIME_19 = 19;
    public const int ENV_TIME_20 = 20;
    public const int ENV_TIME_21 = 21;
    public const int ENV_TIME_22 = 22;
    public const int ENV_TIME_23 = 23;
    public const int ENV_NUM_TIMES = 24;


    public const int ENV_TOD_FLAGS_DAY = 0x00000001;
    public const int ENV_TOD_FLAGS_DAWN = 0x00000002;
    public const int ENV_TOD_FLAGS_DUSK = 0x00000004;
    public const int ENV_TOD_FLAGS_NIGHT = 0x00000008;

    /*
    #define		DAWNLIGHT_START											( 5 * 60 )
    #define		DAWN_START													( 6 * 60 )
    #define   DAY_START														( 8 * 60 )
    #define		TWILLIGHT_START											( 19 * 60 )
    #define		DUSK_START													( 20 * 60 )
    #define   NIGHT_START													( 22 * 60 )
    */
    public const int DAWN_START = (6 * 60 + 47);        //6:47AM
    public const int DAY_START = (7 * 60 + 5);      //7:05AM
    public const int DUSK_START = (20 * 60 + 57);   //8:57PM
    public const int NIGHT_START = (21 * 60 + 15);  //9:15PM
    public const int DAWN_TO_DAY = (DAY_START - DAWN_START);
    public const int DAY_TO_DUSK = (DUSK_START - DAY_START);
    public const int DUSK_TO_NIGHT = (NIGHT_START - DUSK_START);
    public const int NIGHT_TO_DAWN = (24 * 60 - NIGHT_START + DAWN_START);

    public static int guiEnvWeather = 0;
    public static int guiRainLoop = NO_SAMPLE;

    // Sound error values (they're all the same)
    public const int NO_SAMPLE = 0xfffffff;
    public const int SOUND_ERROR = 0xfffffff;


    // frame cues for lightning
    public static int[,,] ubLightningTable = new int[3, 10, 2]
    {
        {
            {0, 15},
            {1, 0},
            {2, 0},
            {3, 6},
            {4, 0},
            {5, 0},
            {6, 0},
            {7, 0},
            {8, 0},
            {9, 0},
        },
        {
            {0, 15},
            {1, 0},
            {2, 0},
            {3, 6},
            {4, 0},
            {5, 15},
            {6, 0},
            {7, 6},
            {8, 0},
            {9, 0},
        },
        {
            {0, 15},
            {1, 0},
            {2, 15},
            {3, 0},
            {4, 0},
            {5, 0},
            {6, 0},
            {7, 0},
            {8, 0},
            {9, 0}
        }
    };

    // CJC: I don't think these are used anywhere!
    public static int[] guiTODFlags = new int[ENV_NUM_TIMES]
    {
        ENV_TOD_FLAGS_NIGHT,		// 00
        ENV_TOD_FLAGS_NIGHT,		// 01
        ENV_TOD_FLAGS_NIGHT,		// 02
        ENV_TOD_FLAGS_NIGHT,		// 03
        ENV_TOD_FLAGS_NIGHT,		// 04
        ENV_TOD_FLAGS_DAWN,			// 05
        ENV_TOD_FLAGS_DAWN,			// 06
        ENV_TOD_FLAGS_DAWN,			// 07
        ENV_TOD_FLAGS_DAY,		  // 08
        ENV_TOD_FLAGS_DAY,		  // 09
        ENV_TOD_FLAGS_DAY,		  // 10
        ENV_TOD_FLAGS_DAY,		  // 11
        ENV_TOD_FLAGS_DAY,			// 12
        ENV_TOD_FLAGS_DAY,		  // 13
        ENV_TOD_FLAGS_DAY,			// 14
        ENV_TOD_FLAGS_DAY,			// 15
        ENV_TOD_FLAGS_DAY,			// 16
        ENV_TOD_FLAGS_DAY,			// 17
        ENV_TOD_FLAGS_DAY,			// 18
        ENV_TOD_FLAGS_DUSK,			// 19
        ENV_TOD_FLAGS_DUSK,			// 20
        ENV_TOD_FLAGS_DUSK,			// 21
        ENV_TOD_FLAGS_NIGHT,		// 22
        ENV_TOD_FLAGS_NIGHT,         // 23 
    };

    public const int DESERT_WARM_START = (8 * 60);
    public const int DESERT_HOT_START = (9 * 60);
    public const int DESERT_HOT_END = (17 * 60);
    public const int DESERT_WARM_END = (19 * 60);
    public const int GLOBAL_WARM_START = (9 * 60);
    public const int GLOBAL_HOT_START = (12 * 60);
    public const int GLOBAL_HOT_END = (14 * 60);
    public const int GLOBAL_WARM_END = (17 * 60);
    public const int HOT_DAY_LIGHTLEVEL = 2;

    public static int guiEnvTime = 0;
    public static int guiEnvDay = 0;
    public static int gubEnvLightValue = 0;
    public static int gubDesertTemperature = 0;
    public static int gubGlobalTemperature = 0;
    public static bool fTimeOfDayControls = true;
    public static bool gfDoLighting = false;
}

public enum Temperatures
{
    COOL,
    WARM,
    HOT
}


public enum TemperatureEvents
{
    TEMPERATURE_DESERT_COOL,
    TEMPERATURE_DESERT_WARM,
    TEMPERATURE_DESERT_HOT,
    TEMPERATURE_GLOBAL_COOL,
    TEMPERATURE_GLOBAL_WARM,
    TEMPERATURE_GLOBAL_HOT,
}
