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
    public List<FileHeader> pFileHeader { get; } = new();
    public List<FileOpenStruct> pOpenFiles { get; } = new();
    public uint uiTotalMemoryAllocatedForLibrary;

    public bool FileExists(string fileName)
    {
        return this.pFileHeader
            .Any(fh => fh.pFileName.Equals(fileName, StringComparison.OrdinalIgnoreCase));
    }

    public bool TryGetValue(string fileName, out FileHeader? fileHeader)
    {
        var fileNameNoLibrary = fileName;

        if (!string.IsNullOrWhiteSpace(this.sLibraryPath))
        {
            fileNameNoLibrary = fileName.ToUpper().Replace(this.sLibraryPath.ToUpper(), "");
        }

        fileHeader = this.pFileHeader.FirstOrDefault(fh => fh.pFileName.Equals(fileNameNoLibrary, StringComparison.OrdinalIgnoreCase));

        return fileHeader.HasValue;
    }
}
