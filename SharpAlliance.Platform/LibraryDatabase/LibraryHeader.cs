using System.Collections.Generic;
using System.IO;

namespace SharpAlliance.Platform.LibraryDatabase
{
    public record LibraryHeader
    {
        public string sLibraryPath;
        public Stream hLibraryHandle;
        public int usNumberOfEntries;
        public bool fLibraryOpen { get; set; }
        //	BOOLEAN	fAnotherFileAlreadyOpenedLibrary;				//this variable is set when a file is opened from the library and reset when the file is close.  No 2 files can have access to the library at 1 time.
        public uint uiIdOfOtherFileAlreadyOpenedLibrary;             //this variable is set when a file is opened from the library and reset when the file is close.  No 2 files can have access to the library at 1 time.
        public int iNumFilesOpen;
        public int iSizeOfOpenFileArray;
        public List<FileHeader> pFileHeader { get; } = new();
        public List<FileOpen> pOpenFiles { get; } = new();
        public uint uiTotalMemoryAllocatedForLibrary;
    }
}
