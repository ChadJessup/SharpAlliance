using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core.Managers.Library;

public record LibraryHeader
{
    public string sLibraryPath;
    private Stream stream;
    public Stream hLibraryHandle
    {
        get => this.stream;
        set => this.stream = value;
    }

    public int usNumberOfEntries;
    public bool fLibraryOpen { get; set; }
    //	bool	fAnotherFileAlreadyOpenedLibrary;				//this variable is set when a file is opened from the library and reset when the file is close.  No 2 files can have access to the library at 1 time.
    public uint uiIdOfOtherFileAlreadyOpenedLibrary;             //this variable is set when a file is opened from the library and reset when the file is close.  No 2 files can have access to the library at 1 time.
    public int iNumFilesOpen => this.pOpenFiles.Count;
    public Dictionary<string, FileHeader> pFileHeader { get; } = new();
    public Dictionary<string, FileOpenStruct> pOpenFiles { get; } = new();

    public uint uiTotalMemoryAllocatedForLibrary;

    public bool FileExists(string fileName)
    {
        return this.pFileHeader.ContainsKey(fileName);
//            .Any(fh => fh.pFileName.Equals(fileName, StringComparison.OrdinalIgnoreCase));
    }

    public bool TryGetValue(string fileName, out FileHeader? fileHeader)
    {
        var fileNameNoLibrary = fileName;

        if (!string.IsNullOrWhiteSpace(this.sLibraryPath))
        {
            // remove the library's path once, from the front: a plain Replace would also eat the same text inside a
            // sub folder name (the "faces\\" library and "FACES\BIGFACES\00.STI" would become "BIG00.STI")
            int at = fileName.IndexOf(this.sLibraryPath, StringComparison.OrdinalIgnoreCase);
            fileNameNoLibrary = at >= 0
                ? fileName.Remove(at, this.sLibraryPath.Length)
                : fileName;
        }

        //this.pFileHeader.TryGetValue(fileName, out var fileHeader);
        fileHeader = this.pFileHeader.FirstOrDefault(fh => fh.Value.pFileName.Equals(fileNameNoLibrary, StringComparison.OrdinalIgnoreCase)).Value;

        return !string.IsNullOrWhiteSpace(fileHeader.Value.pFileName);
    }
}
