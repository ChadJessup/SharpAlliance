using System;
using System.IO;

namespace SharpAlliance.Platform.Interfaces
{
    public interface IFileManager : ISharpAllianceManager
    {
        Stream FileOpen(string pFileName, FileAccess read, bool fDeleteOnClose = false);
        void FileClose(Stream fptr);
        bool FileExists(string pFilename);
        bool FileRead(Stream stream, Span<byte> buffer, out uint bytesRead);
        bool FileRead(Stream stream, ref byte[] buffer, uint count, out uint uiBytesRead);
        bool FileSeek(Stream stream, ref uint uiStoredSize, SeekOrigin current);
        bool FileRead<T>(Stream stream, ref T[] fillArray, uint uiFileSectionSize, out uint uiBytesRead)
            where T : unmanaged;
        bool LoadEncryptedDataFromFile(string fileName, out string destination, uint seekFrom, uint seekAmount);
    }
}
