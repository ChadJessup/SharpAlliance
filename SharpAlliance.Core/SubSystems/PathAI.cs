using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpAlliance.Core.SubSystems;

public class PathAI
{
}

// PLOT PATH defines
public enum PlotPath
{
    NOT_STEALTH = 0,
    STEALTH = 1,
    NO_PLOT = 0,
    PLOT = 1,
    TEMPORARY = 0,
    PERMANENT = 1,
    FORWARD = 0,
    REVERSE = 1,
    NO_COPYROUTE = 0,
    COPYROUTE = 1,
    COPYREACHABLE = 2,
    COPYREACHABLE_AND_APS = 3,
    PATH_THROUGH_PEOPLE = 0x01,
    PATH_IGNORE_PERSON_AT_DEST = 0x02,
    PATH_CLOSE_GOOD_ENOUGH = 0x04,
    PATH_CLOSE_RADIUS = 5,
}
