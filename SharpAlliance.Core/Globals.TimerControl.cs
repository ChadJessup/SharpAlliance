using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TIMECOUNTER = System.UInt32;

using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core;

public delegate void CUSTOMIZABLE_TIMER_CALLBACK();
public partial class Globals
{
    // Base resultion of callback timer
    public const int BASETIMESLICE = 10;

    public const int NUMTIMERS = (int)TIMER.NUMTIMERS;

    // TIMER INTERVALS
    public static int[] giTimerIntervals = new int[NUMTIMERS];
    // TIMER COUNTERS
    public static int[] giTimerCounters = new int[NUMTIMERS];

    // GLOBAL SYNC TEMP TIME
    public static int giClockTimer;
    public static int giTimerDiag;
    public static int giTimerTeamTurnUpdate;

    //Don't modify this value
    public static int guiBaseJA2Clock;
    public static CUSTOMIZABLE_TIMER_CALLBACK? gpCustomizableTimerCallback;

    // MACROS
    //																CHeck if new counter < 0														 | set to 0 |										 Decrement

    public static void UPDATECOUNTER(TIMECOUNTER c)
    {
        if ((giTimerCounters[c] - BASETIMESLICE) < 0)
        {
            giTimerCounters[c] = 0;
        }
        else
        {
            giTimerCounters[c] -= BASETIMESLICE;
        }
    }

    public static void RESETCOUNTER(TIMECOUNTER c) { giTimerCounters[c] = giTimerIntervals[c]; }
    public static bool COUNTERDONE(TIMECOUNTER c) => giTimerCounters[c] == 0 ? true : false;
    public static void UPDATETIMECOUNTER(ref TIMECOUNTER c)
    {
        if ((c - BASETIMESLICE) < 0)
        {
            c = 0;
        }
        else
        {
            c -= BASETIMESLICE;
        }
    }
    public static void RESETTIMECOUNTER(ref TIMECOUNTER c, TIMECOUNTER d) { c = d; }
    public static bool TIMECOUNTERDONE(TIMECOUNTER c) => c == 0 ? true : false;

    public static void SYNCTIMECOUNTER() { }
    public static void ZEROTIMECOUNTER(ref TIMECOUNTER c) { c = 0; }
}

// CALLBACK TIMER DEFINES
public enum TIMER_CALLBACK
{
    ITEM_LOCATOR_CALLBACK,
    NUM_TIMER_CALLBACKS
};

// TIMER DEFINES
public enum TIMER
{
    TOVERHEAD = 0,                                          // Overhead time slice
    NEXTSCROLL,                                                 // Scroll Speed timer
    STARTSCROLL,                                                // Scroll Start timer
    ANIMATETILES,                                               // Animate tiles timer
    FPSCOUNTER,                                                 // FPS value
    PATHFINDCOUNTER,                                        // PATH FIND COUNTER
    CURSORCOUNTER,                                          // ANIMATED CURSOR
    RMOUSECLICK_DELAY_COUNTER,                  // RIGHT BUTTON CLICK DELAY
    LMOUSECLICK_DELAY_COUNTER,                  // LEFT	 BUTTON CLICK DELAY
    SLIDETEXT,                                                  // DAMAGE DISPLAY				
    TARGETREFINE,                                               // TARGET REFINE
    CURSORFLASH,                                                // Cursor/AP flash
    FADE_GUY_OUT,                                               // FADE MERCS OUT
    PANELSLIDE_UNUSED,                                  // PANLE SLIDE
    TCLOCKUPDATE,                                               // CLOCK UPDATE
    PHYSICSUPDATE,                                          // PHYSICS UPDATE.
    GLOW_ENEMYS,
    STRATEGIC_OVERHEAD,                                 // STRATEGIC OVERHEAD
    CYCLERENDERITEMCOLOR,                               // CYCLE COLORS
    NONGUNTARGETREFINE,                                 // TARGET REFINE
    CURSORFLASHUPDATE,                                  // 
    INVALID_AP_HOLD,                                        // TIME TO HOLD INVALID AP
    RADAR_MAP_BLINK,                                        // BLINK DELAY FOR RADAR MAP
    OVERHEAD_MAP_BLINK,                                 // OVERHEADMAP
    MUSICOVERHEAD,                                          // MUSIC TIMER
    RUBBER_BAND_START_DELAY,
    NUMTIMERS
};
