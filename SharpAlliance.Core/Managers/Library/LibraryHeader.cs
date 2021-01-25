﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SharpAlliance.Core.Managers.Library
{
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
        //	BOOLEAN	fAnotherFileAlreadyOpenedLibrary;				//this variable is set when a file is opened from the library and reset when the file is close.  No 2 files can have access to the library at 1 time.
        public uint uiIdOfOtherFileAlreadyOpenedLibrary;             //this variable is set when a file is opened from the library and reset when the file is close.  No 2 files can have access to the library at 1 time.
        public int iNumFilesOpen => pOpenFiles.Count;
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
            fileHeader = this.pFileHeader.FirstOrDefault(fh => fh.pFileName.Equals(fileName, StringComparison.OrdinalIgnoreCase));

            return fileHeader.HasValue;
        }
    }
}
