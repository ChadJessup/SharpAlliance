using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpAlliance.Core.SubSystems
{
    public class LightingSystem
    {
        public ValueTask<bool> InitLightingSystem()
        {
            return ValueTask.FromResult(true);
        }
    }
}
