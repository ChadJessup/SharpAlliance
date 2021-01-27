using System;
using System.Threading.Tasks;

namespace SharpAlliance.Core.Screens
{
    public class EventManager
    {
        public ValueTask<bool> InitializeEventManager()
        {
            return ValueTask.FromResult(true);
        }
    }
}
