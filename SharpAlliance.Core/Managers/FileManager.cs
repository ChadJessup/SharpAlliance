using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using SharpAlliance.Platform.Interfaces;

namespace SharpAlliance.Core.Managers
{
    public class FileManager : IFileManager
    {
        private readonly LibraryFileManager library;

        public FileManager(LibraryFileManager libraryFileManager)
        {
            this.library = libraryFileManager;
        }

        public bool IsInitialized { get; private set; }

        public ValueTask<bool> Initialize()
        {
            this.IsInitialized = true;
            return ValueTask.FromResult(true);
        }

        public Stream FileOpen(string pFileName, FileAccess read, bool fDeleteOnClose)
        {
            return Stream.Null;
        }

        public void Dispose()
        {
        }

        public void FileClose(Stream fptr)
        {
        }

        private const int ROTATION_ARRAY_SIZE = 46;

        public bool FileExists(string pFilename)
        {
            return true;
        }

        byte[] ubRotationArray = new byte[] { 132, 235, 125, 99, 15, 220, 140, 89, 205, 132, 254, 144, 217, 78, 156, 58, 215, 76, 163, 187, 55, 49, 65, 48, 156, 140, 201, 68, 184, 13, 45, 69, 102, 185, 122, 225, 23, 250, 160, 220, 114, 240, 64, 175, 057, 233 };

        public unsafe bool JA2EncryptedFileRead(Stream hFile, byte[] pDest, uint uiBytesToRead, out uint puiBytesRead)
        {
            uint uiLoop;
            byte ubArrayIndex = 0;
            //byte		ubLastNonBlank = 0;
            byte ubLastByte = 0;
            byte ubLastByteForNextLoop;
            bool fRet;
            byte[] pMemBlock = new byte[uiBytesToRead];

            fRet = this.FileRead(hFile, pDest, uiBytesToRead, out puiBytesRead);
            if (fRet)
            {
                // pMemBlock = pDest;
                for (uiLoop = 0; uiLoop < puiBytesRead; uiLoop++)
                {
                    ubLastByteForNextLoop = pMemBlock[uiLoop];
                    pMemBlock[uiLoop] -= (byte)(ubLastByte + ubRotationArray[ubArrayIndex]);
                    ubArrayIndex++;
                    if (ubArrayIndex >= ROTATION_ARRAY_SIZE)
                    {
                        ubArrayIndex = 0;
                    }

                    ubLastByte = ubLastByteForNextLoop;
                }
            }

            return fRet;
        }

        private bool FileRead(Stream hFile, byte[] pDest, uint uiBytesToRead, out uint puiBytesRead)
        {
            puiBytesRead = 1;

            return true;
        }
    }
}
