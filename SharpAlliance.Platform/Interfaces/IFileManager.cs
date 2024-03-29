﻿using System;
using System.IO;

namespace SharpAlliance.Platform.Interfaces
{
    public interface IFileManager : ISharpAllianceManager
    {
        Stream FileOpen(string pFileName, FileAccess read, FileMode mode = FileMode.Open, bool fDeleteOnClose = false);
        void FileClose(Stream fptr);
        bool FileExists(string pFilename);
        bool FileRead(Stream stream, Span<byte> buffer, out uint bytesRead);
        bool FileRead(Stream stream, ref byte[] buffer, int count, out int uiBytesRead);
        bool FileSeek(Stream stream, int uiStoredSize, SeekOrigin current);
        bool FileRead<T>(Stream stream, ref T[] fillArray, int uiFileSectionSize, out int uiBytesRead)
            where T : struct;
        bool FileRead<T>(Stream stream, ref T fillArray, int uiFileSectionSize, out int uiBytesRead)
            where T : struct;
        bool LoadEncryptedDataFromFile(string fileName, out string destination, int seekFrom, uint seekAmount);
        void FileWrite<T>(Stream stream, T value, int size, out int bytesWritten);
        FileAttributes FileGetAttributes(string filePath);
        bool MakeFileManDirectory(string saveDir);
        bool FileDelete(string filePath);
        long FileGetSize(Stream hFileHandle);
        void FileClearAttributes(string path);
        long FileGetPos(Stream hFileHandle);
    }
}
