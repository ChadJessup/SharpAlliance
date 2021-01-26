using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpAlliance.Platform.Interfaces
{
    public interface IFileManager : ISharpAllianceManager
    {
        Stream FileOpen(string pFileName, FileAccess read, bool fDeleteOnClose);
        void FileClose(Stream fptr);
        bool FileExists(string pFilename);
        bool FileRead(Stream stream, ref byte[] pSTCIPalette, uint uiFileSectionSize, out uint uiBytesRead);
        bool FileSeek(Stream stream, ref uint uiStoredSize, SeekOrigin current);
        bool FileRead<T>(Stream stream, ref T[] fillArray, uint uiFileSectionSize, out uint uiBytesRead)
            where T : unmanaged;
    }
}
