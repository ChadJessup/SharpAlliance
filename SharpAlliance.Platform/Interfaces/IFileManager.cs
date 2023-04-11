using System;
using System.IO;

namespace SharpAlliance.Platform.Interfaces
{
    public interface IFileManager : ISharpAllianceManager
    {
        static Stream FileOpen(string pFileName, FileAccess read, bool fDeleteOnClose = false) => throw new NotImplementedException();
        static void FileClose(Stream fptr) => throw new NotImplementedException();
        static bool FileExists(string pFilename) => throw new NotImplementedException();
        static bool FileRead(Stream stream, Span<byte> buffer, out uint bytesRead) => throw new NotImplementedException();
        static bool FileRead(Stream stream, ref byte[] buffer, int count, out int uiBytesRead) => throw new NotImplementedException();
        static bool FileSeek(Stream stream, ref uint uiStoredSize, SeekOrigin current) => throw new NotImplementedException();
        static bool FileRead<T>(Stream stream, ref T[] fillArray, int uiFileSectionSize, out int uiBytesRead) where T : unmanaged
            => throw new NotImplementedException();
        static bool FileRead<T>(Stream stream, ref T fillArray, int uiFileSectionSize, out int uiBytesRead) where T : unmanaged
             => throw new NotImplementedException();
        static bool LoadEncryptedDataFromFile(string fileName, out string destination, uint seekFrom, uint seekAmount) => throw new NotImplementedException();
    }
}
