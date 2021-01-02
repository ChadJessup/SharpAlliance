using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SharpAlliance.Platform.Interfaces
{
    public interface IGameLogic : ISharpAllianceManager
    {
        Task<int> GameLoop(CancellationToken token = default);
    }
}
