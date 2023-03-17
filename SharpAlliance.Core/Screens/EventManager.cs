using System;
using System.Threading.Tasks;

using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core.Screens;

public class EventManager
{
    public ValueTask<bool> InitializeEventManager()
    {
        return ValueTask.FromResult(true);
    }
}

public enum EVENT_QUEUE
{
    PRIMARY_EVENT_QUEUE = 0,
    SECONDARY_EVENT_QUEUE = 1,
    DEMAND_EVENT_QUEUE = 2,
}
