using System;
using System.Collections.Generic;
using SharpAlliance.Core.SubSystems;

namespace SharpAlliance.Core;

public partial class Globals
{
    public static string[] gpGameClockString =
    {
    	//This is the day represented in the game clock.  Must be very short, 4 characters max.
    	"Day",
    };

    // where the time string itself is rendered
    public const int CLOCK_X = 554;
    public const int CLOCK_Y = 459;
    // the mouse region around the clock (bigger)
    public const int CLOCK_REGION_START_X = 552;
    public const int CLOCK_REGION_START_Y = 456;
    public const int CLOCK_REGION_WIDTH = (620 - CLOCK_REGION_START_X);
    public const int CLOCK_REGION_HEIGHT = (468 - CLOCK_REGION_START_Y);
    public const int NUM_SEC_IN_DAY = 86400;
    public const int NUM_SEC_IN_HOUR = 3600;
    public const int NUM_SEC_IN_MIN = 60;
    public const int ROUNDTO_MIN = 5;
    public const int NUM_MIN_IN_DAY = 1440;
    public const int NUM_MIN_IN_HOUR = 60;

    public static uint STARTING_TIME => ((1 * NUM_SEC_IN_HOUR) + (0 * NUM_SEC_IN_MIN) + NUM_SEC_IN_DAY);     // 1am
    public static int FIRST_ARRIVAL_DELAY => ((6 * NUM_SEC_IN_HOUR) + (0 * NUM_SEC_IN_MIN));        // 7am ( 6hours later)

    public const int SECONDS_PER_COMPRESSION = 1; // 1/2 minute passes every 1 second of real time 
    public const int SECONDS_PER_COMPRESSION_IN_RTCOMBAT = 10;
    public const int SECONDS_PER_COMPRESSION_IN_TBCOMBAT = 10;
    public const int CLOCK_STRING_HEIGHT = 13;
    public const int CLOCK_STRING_WIDTH = 66;
    public const FontStyle CLOCK_FONT = FontStyle.COMPFONT;

    //These contain all of the information about the game time, rate of time, etc.
    //All of these get saved and loaded.
    public static TIME_COMPRESS giTimeCompressMode = TIME_COMPRESS.TIME_COMPRESS_X0;
    public static uint gubClockResolution = 1;
    public static bool gfGamePaused { get; set; }
    public static bool gfTimeInterrupt { get; set; } = false;
    public static bool gfAtLeastOneMercWasHired { get; internal set; }
    public static bool fDisplayMessageFlag { get; internal set; }
    public static bool gfPageButtonsWereCreated { get; internal set; }
    public static int gubVideoConferencingMode { get; internal set; }
    public static bool gfFirstBattleMeanwhileScenePending { get; set; }
    public static bool gfPlotPathToExitGrid { get; internal set; }
    public static int giNPCReferenceCount { get; internal set; }

    public static bool gfTimeInterruptPause = false;
    public static bool fSuperCompression = false;
    public static uint guiGameClock = STARTING_TIME;
    public static uint guiPreviousGameClock = 0;        // used only for error-checking purposes
    public static uint guiGameSecondsPerRealSecond;
    public static uint guiTimesThisSecondProcessed = 0;
    public static int iPausedPopUpBox = -1;
    public static Dictionary<TIME_COMPRESS, uint> giTimeCompressSpeeds = new()
    {
        { TIME_COMPRESS.TIME_COMPRESS_X0, 0},
        { TIME_COMPRESS.TIME_COMPRESS_X1, 1 },
        { TIME_COMPRESS.TIME_COMPRESS_5MINS,5 * 60 },
        { TIME_COMPRESS.TIME_COMPRESS_30MINS, 30 * 60 },
        { TIME_COMPRESS.TIME_COMPRESS_60MINS, 60 * 60 }
    };

    public static int usPausedActualWidth;
    public static int usPausedActualHeight;
    public static bool gfLockPauseState = false;
    public static bool gfTimeCompressionOn = false;
    public static int guiLockPauseStateLastReasonId = 0;
    //***When adding new saved time variables, make sure you remove the appropriate amount from the paddingbytes and
    //   more IMPORTANTLY, add appropriate code in Save/LoadGameClock()!
    public const int TIME_PADDINGBYTES = 20;
    public static int[] gubUnusedTimePadding = new int[TIME_PADDINGBYTES];
    public static int guiEnvTime;
    public static int guiEnvDay;

    public static string gswzWorldTimeStr = string.Empty; //Day 99, 23:55
    public static uint guiDay;
    public static uint guiHour;
    public static uint guiMin;

    //Advanced function used by certain event callbacks.  In the case where time is warped, certain event
    //need to know how much time was warped since the last query to the event list.  
    //This function returns that value
    public static uint guiTimeOfLastEventQuery;

    //This value represents the time that the sector was loaded.  If you are in sector A9, and leave
    //the game clock at that moment will get saved into the temp file associated with it.  The next time you
    //enter A9, this value will contain that time.  Used for scheduling purposes.
    public static int guiTimeCurrentSectorWasLastLoaded;

    // is the current pause state due to the player?
    public static bool gfPauseDueToPlayerGamePause;

    // we've just clued up a pause by the player, the tactical screen will need a full one shot refresh to remove a 2 frame update problem
    public static bool gfJustFinishedAPause;

    public static bool gfResetAllPlayerKnowsEnemiesFlags;

    public static void CHECKF(object? check)
    {
        if (check is null)
        {
            throw new ArgumentNullException(nameof(check));
        }

        if (check is bool predicate && !predicate)
        {
            throw new ArgumentException();
        }
    }

    public static void CHECKN(object? check)
    {
        if (check is null)
        {
            throw new ArgumentNullException(nameof(check));
        }
    }
}

public enum WARPTIME
{
    NO_PROCESSING_OF_EVENTS,
    PROCESS_EVENTS_NORMALLY,
    PROCESS_TARGET_TIME_FIRST,
};

//time compression defines
public enum TIME_COMPRESS
{
    NOT_USING_TIME_COMPRESSION = -1,
    TIME_COMPRESS_X0,
    TIME_COMPRESS_X1,
    TIME_COMPRESS_5MINS,
    TIME_COMPRESS_30MINS,
    TIME_COMPRESS_60MINS,
    TIME_SUPER_COMPRESS,
    NUM_TIME_COMPRESS_SPEEDS
};
