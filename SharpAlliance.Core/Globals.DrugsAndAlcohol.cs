using System;

namespace SharpAlliance.Core;

public partial class Globals
{
    public const int DRUG_TYPE_ADRENALINE = 0;
    public const int DRUG_TYPE_ALCOHOL = 1;
    public const int NO_DRUG = 2;
    public const int NUM_COMPLEX_DRUGS = 2;
    public const int DRUG_TYPE_REGENERATION = 3;
    public const int SOBER = 0;
    public const int FEELING_GOOD = 1;
    public const int BORDERLINE = 2;
    public const int DRUNK = 3;
    public const int HUNGOVER = 4;
    public const int REGEN_POINTS_PER_BOOSTER = 4;
    public const int LIFE_GAIN_PER_REGEN_POINT = 10;

    public static int[] giDrunkModifier =
    {
        100,		// Sober
    	75,			// Feeling good,
    	65,			// Bporderline
    	50,			// Drunk
    	100,		// HungOver
    };

    public const int HANGOVER_AP_REDUCE = 5;
    public const int HANGOVER_BP_REDUCE = 200;

}
