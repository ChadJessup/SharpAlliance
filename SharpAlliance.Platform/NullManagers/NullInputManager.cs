using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpAlliance.Platform.Interfaces;

namespace SharpAlliance.Platform.NullManagers
{
    public class NullInputManager : IInputManager
    {
        public bool Initialize()
        {
            return true;
        }

        public void Dispose()
        {
        }
    }
}
