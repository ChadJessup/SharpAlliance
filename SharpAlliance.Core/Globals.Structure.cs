using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpAlliance.Core;

public partial class Globals
{
}

public enum BLOCKING
{
    NOTHING_BLOCKING = 0,
    REDUCE_RANGE = 1,
    NEXT_TILE = 10,
    TOPLEFT_WINDOW = 30,
    TOPRIGHT_WINDOW = 40,
    TOPLEFT_DOOR = 50,
    TOPRIGHT_DOOR = 60,
    FULL_BLOCKING = 70,
    TOPLEFT_OPEN_WINDOW = 90,
    TOPRIGHT_OPEN_WINDOW = 100,
}
