using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpAlliance.Core.SubSystems;

public class HandleItems
{
}

public enum ITEM_HANDLE
{
    OK = 1,
    RELOADING = -1,
    UNCONSCIOUS = -2,
    NOAPS = -3,
    NOAMMO = -4,
    CANNOT_GETTO_LOCATION = -5,
    BROKEN = -6,
    NOROOM = -7,
    REFUSAL = -8,
}
