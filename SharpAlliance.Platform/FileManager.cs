using System.Runtime.InteropServices;
using System.Threading.Tasks;
using SharpAlliance.Platform.Interfaces;

namespace SharpAlliance.Platform
{
    public class FileManager : IFileManager
    {
        public bool IsInitialized { get; private set; }

        public ValueTask<bool> Initialize()
        {
            this.IsInitialized = true;
            return ValueTask.FromResult(true);
        }

        public void Dispose()
        {
        }
    }
}
