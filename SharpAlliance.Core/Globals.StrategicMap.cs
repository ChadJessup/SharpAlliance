using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
