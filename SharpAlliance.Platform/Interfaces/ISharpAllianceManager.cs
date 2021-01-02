using System;
using System.Threading.Tasks;

namespace SharpAlliance.Platform.Interfaces
{
    public interface ISharpAllianceManager : IDisposable
    {
        ValueTask<bool> Initialize();
        bool IsInitialized { get; }
    }
}
