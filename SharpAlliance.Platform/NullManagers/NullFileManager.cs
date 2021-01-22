using System;
using System.IO;
using System.Threading.Tasks;
using SharpAlliance.Platform.Interfaces;

namespace SharpAlliance.Platform.NullManagers
{
    public class NullFileManager : IFileManager
    {
        public bool IsInitialized { get; } = true;

        public ValueTask<bool> Initialize()
        {
            return ValueTask.FromResult(true);
        }

        public void Dispose()
        {
        }

        public Stream FileOpen(string pFileName, FileAccess read, bool fDeleteOnClose)
        {
            throw new NotImplementedException();
        }

        public void FileClose(Stream fptr)
        {
            throw new NotImplementedException();
        }

        public bool FileExists(string pFilename)
        {
            throw new NotImplementedException();
        }
    }
}
