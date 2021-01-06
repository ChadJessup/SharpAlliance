using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpAlliance.Platform.Interfaces;

namespace SharpAlliance.Core.SubSystems
{
    public class CursorSubSystem : IDisposable
    {
        public void Dispose()
        {
        }

        internal void SetCurrentCursorFromDatabase(int cursor)
        {
            throw new NotImplementedException();
        }
    }
}
