using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace SharpAlliance.Core.SubSystems
{
    public class TacticalSaveSubSystem : IDisposable
    {
        private readonly ILogger<TacticalSaveSubSystem> logger;

        public TacticalSaveSubSystem(ILogger<TacticalSaveSubSystem> logger)
        {
            this.logger = logger;
        }

        public void Dispose()
        {
        }

        internal void InitTacticalSave(bool fCreateTempDir)
        {
            throw new NotImplementedException();
        }
    }
}
