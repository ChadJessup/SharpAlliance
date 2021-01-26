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

        public bool FileRead(Stream stream, ref byte[] pSTCIPalette, uint uiFileSectionSize, out uint uiBytesRead)
        {
            throw new NotImplementedException();
        }

        public bool FileSeek(Stream stream, ref uint uiStoredSize, SeekOrigin current)
        {
            throw new NotImplementedException();
        }

        public bool FileRead<T>(Stream stream, ref T[] fillArray, uint uiFileSectionSize, out uint uiBytesRead)
            where T : unmanaged
        {
            throw new NotImplementedException();
        }
    }
}
