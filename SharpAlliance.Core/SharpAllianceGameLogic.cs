using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpAlliance.Platform;
using SharpAlliance.Platform.Interfaces;

namespace SharpAlliance.Core
{
    public class SharpAllianceGameLogic : IGameLogic
    {
        private readonly GameContext context;

        public SharpAllianceGameLogic(GameContext context)
        => this.context = context;

        public bool Initialize()
        {
            return true;
        }

        public void Dispose()
        {
        }
    }
}
