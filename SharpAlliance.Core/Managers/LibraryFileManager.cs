using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SharpAlliance.Core.Interfaces;
using SharpAlliance.Core.Managers.Library;
using SharpAlliance.Platform;

namespace SharpAlliance.Core.Managers
{
    public class LibraryFileManager : ILibraryManager
    {
        private const int INITIAL_NUM_HANDLES = 20;
        private LibraryNames gsCurrentLibrary;

        public LibraryFileManager(ILogger<ILibraryManager> logger, GameContext context)
        {
            var dataDir = context.Configuration["DataDirectory"] ?? ".\\Data";

            if (!Directory.Exists(dataDir))
            {
                throw new DirectoryNotFoundException(dataDir);
            }

            this.DataDirectory = dataDir;
        }

        public string ManagerName { get; set; }
        public Dictionary<LibraryNames, LibraryHeader> Libraries { get; set; } = new();
        public int NumberOfLibraries => this.Libraries.Count;
        public bool IsInitialized { get; set; }
        public RealFileHeader RealFiles;
        public string DataDirectory { get; init; }

        public ValueTask<bool> Initialize()
        {
            bool fLibraryInited = false;

            //Load up each library
            foreach (var libraryName in Enum.GetValues<LibraryNames>().Cast<LibraryNames>())
            {
                //if you want to init the library at the begining of the game
                if (GameLibraries[libraryName].InitOnStart)
                {
                    //if the library exists
                    if (this.OpenLibrary(libraryName))
                    {
                        fLibraryInited = true;
                    }
                    //else the library doesnt exist
                    else
                    {
                        //FastDebugMsg("Warning in InitializeFileDatabase( ): Library Id #%d (%s) is to be loaded but cannot be found.\n", i, gGameLibaries[i].sLibraryName);
                        this.Libraries[libraryName].fLibraryOpen = false;
                    }
                }
            }

            //signify that the database has been initialized ( only if there was a library loaded )
            this.IsInitialized = fLibraryInited;

            //set the initial number how many files can be opened at the one time
            this.RealFiles.iSizeOfOpenFileArray = INITIAL_NUM_HANDLES;

            return ValueTask.FromResult(true);
        }

        //************************************************************************
        //
        //	OpenLibrary() Opens a library from the 'array' of library names
        //	that was passd in at game initialization.  Pass in an enum for the
        //	library.
        //
        //************************************************************************
        private bool OpenLibrary(LibraryNames libraryName)
        {
            //if the library is already opened, report an error
            if (this.Libraries.TryGetValue(libraryName, out var library) && library.fLibraryOpen)
            {
                return false;
            }

            this.Libraries.TryAdd(libraryName, new LibraryHeader());

            //if we cant open the library
            if (!this.InitializeLibrary(GameLibraries[libraryName].LibraryName, this.Libraries[libraryName], GameLibraries[libraryName].OnCdRom))
            {
                return false;
            }

            return true;
        }

        private bool InitializeLibrary(string pLibraryName, LibraryHeader pLibHeader, bool fCanBeOnCDrom)
        {
            int numEntries;
            DirEntry dirEntry;

            //open the library for reading ( if it exists )
            using var hFile = File.OpenRead(Path.Combine(this.DataDirectory, pLibraryName));
            using BinaryReader br = new(hFile);

            LibHeader libHeader = this.ParseLibHeader(br);

            //place the file pointer at the begining of the file headers (they are at the end of the file)
            // 280 is the size of the LibHeader on disk, with padding involved.
            hFile.Seek(-(libHeader.iEntries * 280), SeekOrigin.End);

            //loop through the library and determine the number of files that are FILE_OK
            //ie.  so we dont load the old or deleted files
            numEntries = 0;
            for (int loop = 0; loop < libHeader.iEntries; loop++)
            {
                //read in the file header
                dirEntry = this.ParseDirEntry(br);

                if (dirEntry.ubState == 0) // FILE_OK
                {
                    pLibHeader.pFileHeader.Add(new()
                    {
                        pFileName = dirEntry.sFileName,
                        uiFileOffset = dirEntry.uiOffset,
                        uiFileLength = dirEntry.uiLength,
                    });

                    numEntries++;
                }
            }

            pLibHeader.sLibraryPath = libHeader.sPathToLibrary;
            pLibHeader.hLibraryHandle = hFile;
            pLibHeader.usNumberOfEntries = numEntries;
            pLibHeader.fLibraryOpen = true;
            pLibHeader.iNumFilesOpen = 0;
            pLibHeader.iSizeOfOpenFileArray = INITIAL_NUM_HANDLES;

            return true;
        }

        private DirEntry ParseDirEntry(BinaryReader br)
        {
            DirEntry dirEntry = new();
            dirEntry.sFileName = new string(br.ReadChars(256)).TrimEnd('\0');
            dirEntry.uiOffset = (int)br.ReadUInt32();
            dirEntry.uiLength = (int)br.ReadUInt32();
            dirEntry.ubState = br.ReadByte();
            dirEntry.ubReserved = br.ReadByte();
            dirEntry.sFileTime = this.ParseFileTime(br);
            dirEntry.usReserved2 = br.ReadUInt16();

            // padding
            br.ReadByte();
            br.ReadByte();

            return dirEntry;
        }

        //************************************************************************
        //
        // CheckIfFileExistInLibrary() determines if a file exists in a library.
        //
        //************************************************************************
        private bool CheckIfFileExistInLibrary(string pFileName)
        {
            LibraryNames sLibraryID;

            //get thelibrary that file is in
            sLibraryID = this.GetLibraryIDFromFileName(pFileName);
            if (sLibraryID == LibraryNames.Unknown)
            {
                //not in any library
                return false;
            }

            return this.GetFileHeaderFromLibrary(
                sLibraryID,
                pFileName,
                out FileHeader? pFileHeader);
        }

        //************************************************************************
        //
        //	GetFileHeaderFromLibrary() performsperforms a binary search of the
        //	library.  It adds the libraries path to the file in the
        //	library and then string compared that to the name that we are
        //	searching for.
        //
        //************************************************************************

        private bool GetFileHeaderFromLibrary(LibraryNames sLibraryID, string pstrFileName, out FileHeader? pFileHeader)
        {
            //combi ne the library path to the file name (need it for the search of the library )
            string sFileNameWithPath = pstrFileName;

            this.gsCurrentLibrary = sLibraryID;

            /* try to find the filename using a binary search algorithm: */
            pFileHeader = null;
            // (FileHeader)bsearch(
            // (char*)&sFileNameWithPath,
            // (FileHeader)Libraries[sLibraryID].pFileHeader,
            // Libraries[sLibraryID].usNumberOfEntries,
            // sizeof(FileHeader),
            // (int(*)(const void*, const void*))CompareFileNames );

            if (pFileHeader.HasValue)
            {
                return true;
            }
            else
            {
                pFileHeader = null;
                return false;
            }
        }

        //************************************************************************
        //
        //	This function finds out if the file CAN be in a library.  It determines
        //	if the library that the file MAY be in is open.
        //	( eg. File is  Laptop\Test.sti, if the Laptop\ library is open, it returns true
        //
        //************************************************************************
        private LibraryNames GetLibraryIDFromFileName(string pFileName)
        {
            int sLoop1, sBestMatch = (int)LibraryNames.Unknown;

            //loop through all the libraries to check which library the file is in
            for (sLoop1 = 0; sLoop1 < this.Libraries.Count; sLoop1++)
            {
                //if the library is not loaded, dont try to access the array
                if (this.IsLibraryOpened((LibraryNames)sLoop1))
                {
                    //if the library path name is of size zero, ( the library is for the default path )
                    if (this.Libraries[(LibraryNames)sLoop1].sLibraryPath.Length == 0)
                    {
                        //determine if there is a directory in the file name
                        if (!pFileName.Contains('\\') && !pFileName.Contains('/'))
                        {
                            //There is no directory in the file name
                            return (LibraryNames)sLoop1;
                        }
                    }

                    //compare the library name to the file name that is passed in
                    else
                    {
                        // if the directory paths are the same, to the length of the lib's path
                        if (this.Libraries[(LibraryNames)sLoop1].sLibraryPath.Equals(pFileName) || this.Libraries[(LibraryNames)sLoop1].sLibraryPath.Length == 0)
                        {
                            // if we've never matched, or this match's path is longer than the previous match (meaning it's more exact)
                            if (sBestMatch == (int)LibraryNames.Unknown || this.Libraries[(LibraryNames)sLoop1].sLibraryPath.Length > this.Libraries[(LibraryNames)sBestMatch].sLibraryPath.Length)
                            {
                                sBestMatch = sLoop1;
                            }
                        }
                    }
                }
            }

            //no library was found, return an error
            return (LibraryNames)sBestMatch;
        }

        public bool IsLibraryOpened(LibraryNames sLibraryID)
        {
            //if the database is not initialized
            if (!this.IsInitialized)
            {
                return false;
            }

            //if we are trying to do something with an invalid library id
            if ((int)sLibraryID >= this.Libraries.Count)
            {
                return false;
            }

            //if the library is opened
            if (this.Libraries[sLibraryID].fLibraryOpen)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private DateTime ParseFileTime(BinaryReader br)
        {
            // padding
            br.ReadByte();
            br.ReadByte();

            // convert windows FILETIME to DateTime.
            var ticks = BitConverter.ToInt64(br.ReadBytes(8));

            return DateTime.FromFileTimeUtc(ticks);
        }

        private bool LoadDataFromLibrary(LibraryNames sLibraryID, int uiFileNum, byte[] pData, int uiBytesToRead, out int pBytesRead)
        {
            int uiOffsetInLibrary, uiLength;
            Stream hLibraryFile;
            int uiNumBytesRead;
            int uiCurPos;

            //get the offset into the library, the length and current position of the file pointer.
            uiOffsetInLibrary = this.Libraries[sLibraryID].pOpenFiles[uiFileNum].pFileHeader.uiFileOffset;
            uiLength = this.Libraries[sLibraryID].pOpenFiles[uiFileNum].pFileHeader.uiFileLength;
            hLibraryFile = this.Libraries[sLibraryID].hLibraryHandle;
            uiCurPos = this.Libraries[sLibraryID].pOpenFiles[uiFileNum].uiFilePosInFile;

            //set the file pointer to the right location
            this.SetFilePointer(hLibraryFile, (uiOffsetInLibrary + uiCurPos), SeekOrigin.Begin);

            //if we are trying to read more data then the size of the file, return an error
            if (uiBytesToRead + uiCurPos > uiLength)
            {
                pBytesRead = 0;
                return false;
            }

            //get the data
            if (!this.ReadFile(hLibraryFile, pData, uiBytesToRead, out uiNumBytesRead))
            {
                pBytesRead = 0;
                return false;
            }

            if (uiBytesToRead != uiNumBytesRead)
            {
                //		Gets the reason why the function failed
                //		UINT32 uiLastError = GetLastError();
                //		char zString[1024];
                //		FormatMessage( FORMAT_MESSAGE_FROM_SYSTEM, 0, uiLastError, 0, zString, 1024, NULL);
                pBytesRead = 0;
                return false;
            }

            this.Libraries[sLibraryID].pOpenFiles[uiFileNum].uiFilePosInFile += uiNumBytesRead;

            //	CloseHandle( hLibraryFile );

            pBytesRead = uiNumBytesRead;

            return true;
        }

        private bool ReadFile(Stream hLibraryFile, byte[] pData, int count, out int uiNumBytesRead)
        {
            uiNumBytesRead = hLibraryFile.Read(pData, 0, count);

            return true;
        }

        private long SetFilePointer(Stream hLibraryFile, int offset, SeekOrigin origin)
            => hLibraryFile.Seek(offset, origin);

        private LibHeader ParseLibHeader(BinaryReader br)
            => new()
            {
                sLibName = new string(br.ReadChars(256)).TrimEnd('\0'),
                sPathToLibrary = new string(br.ReadChars(256)).TrimEnd('\0'),
                iEntries = br.ReadInt32(),
                iUsed = br.ReadInt32(),
                iSort = br.ReadUInt16(),
                iVersion = br.ReadUInt16(),
                fContainsSubDirectories = br.ReadBoolean(),
                iReserved = br.ReadInt32()
            };

        public void Dispose()
        {
        }

        // TODO: Make data driven
        private static Dictionary<LibraryNames, LibraryInitHeader> GameLibraries = new()
        {
            { LibraryNames.DATA, new("Data.slf", false, true) },
            { LibraryNames.AMBIENT, new("Ambient.slf", false, true) },
            { LibraryNames.ANIMS, new("Anims.slf", false, true) },
            { LibraryNames.BATTLESNDS, new("BattleSnds.slf", false, true) },
            { LibraryNames.BIGITEMS, new("BigItems.slf", false, true) },
            { LibraryNames.BINARY_DATA, new("BinaryData.slf", false, true) },
            { LibraryNames.CURSORS, new("Cursors.slf", false, true) },
            { LibraryNames.FACES, new("Faces.slf", false, true) },
            { LibraryNames.FONTS, new("Fonts.slf", false, true) },
            { LibraryNames.INTERFACE, new("Interface.slf", false, true) },
            { LibraryNames.LAPTOP, new("Laptop.slf", false, true) },
            { LibraryNames.MAPS, new("Maps.slf", true, true) },
            { LibraryNames.MERCEDT, new("MercEdt.slf", false, true) },
            { LibraryNames.MUSIC, new("Music.slf", true, true) },
            { LibraryNames.NPC_SPEECH, new("Npc_Speech.slf", true, true) },
            { LibraryNames.NPC_DATA, new("NpcData.slf", false, true) },
            { LibraryNames.RADAR_MAPS, new("RadarMaps.slf", false, true) },
            { LibraryNames.SOUNDS, new("Sounds.slf", false, true) },
            { LibraryNames.SPEECH, new("Speech.slf", true, true) },
            { LibraryNames.TILESETS, new("TileSets.slf", true, true) },
            { LibraryNames.LOADSCREENS, new("LoadScreens.slf", true, true) },
            { LibraryNames.INTRO, new("Intro.slf", true, true) },
        };
    }
}
