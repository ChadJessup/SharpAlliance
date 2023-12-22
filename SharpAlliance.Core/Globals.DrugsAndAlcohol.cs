using System;
using System.Collections.Generic;

namespace SharpAlliance.Core;

public partial class Globals
{
    public const int DRUG_TYPE_ADRENALINE = 0;
    public const int DRUG_TYPE_ALCOHOL = 1;
    public const int NO_DRUG = 2;
    public const int NUM_COMPLEX_DRUGS = 2;
    public const int DRUG_TYPE_REGENERATION = 3;
    public const int REGEN_POINTS_PER_BOOSTER = 4;
    public const int LIFE_GAIN_PER_REGEN_POINT = 10;

    public static Dictionary<DrunkLevel, int> giDrunkModifier = new()
    {
        { DrunkLevel.SOBER, 100 },		// Sober
    	{ DrunkLevel.FEELING_GOOD,  75 },			// Feeling good,
    	{ DrunkLevel.BORDERLINE,  65 },			// Bporderline
    	{ DrunkLevel.DRUNK ,  50 },			// Drunk
    	{ DrunkLevel.HUNGOVER , 100 },		// HungOver
    };

    public const int HANGOVER_AP_REDUCE = 5;
    public const int HANGOVER_BP_REDUCE = 200;
}

public enum DrunkLevel
{
    SOBER = 0,
    FEELING_GOOD = 1,
    BORDERLINE = 2,
    DRUNK = 3,
    HUNGOVER = 4,
}
