using System.Runtime.InteropServices;
using SharpAlliance.Platform.Interfaces;

namespace SharpAlliance.Platform
{
    public class FileManager : IFileManager
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
