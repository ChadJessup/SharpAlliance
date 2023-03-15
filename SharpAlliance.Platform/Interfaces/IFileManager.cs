using System;
using System.IO;

namespace SharpAlliance.Platform.Interfaces
{
    public interface IFileManager : ISharpAllianceManager
    {
        abstract static Stream FileOpen(string pFileName, FileAccess read, bool fDeleteOnClose = false);
        abstract static void FileClose(Stream fptr);
        abstract static bool FileExists(string pFilename);
        abstract static bool FileRead(Stream stream, Span<byte> buffer, out uint bytesRead);
        abstract static bool FileRead(Stream stream, ref byte[] buffer, int count, out int uiBytesRead);
        abstract static bool FileSeek(Stream stream, ref uint uiStoredSize, SeekOrigin current);
        abstract static bool FileRead<T>(Stream stream, ref T[] fillArray, int uiFileSectionSize, out int uiBytesRead)
            where T : unmanaged;
        abstract static bool FileRead<T>(Stream stream, ref T fillArray, int uiFileSectionSize, out int uiBytesRead)
            where T : unmanaged;
        abstract static bool LoadEncryptedDataFromFile(string fileName, out string destination, uint seekFrom, uint seekAmount);
    }
}
