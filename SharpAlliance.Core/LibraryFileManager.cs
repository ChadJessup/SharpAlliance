using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SharpAlliance.Platform.Interfaces;
using SharpAlliance.Core.LibraryManager;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SharpAlliance.Platform;
using System.Threading.Tasks;

namespace SharpAlliance.Core
{
    public class LibraryFileManager : ILibraryManager
    {
        private const int INITIAL_NUM_HANDLES = 20;

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
                    if (OpenLibrary(libraryName))
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
                return (false);
            }

            this.Libraries.TryAdd(libraryName, new LibraryHeader());

            //if we cant open the library
            if (!InitializeLibrary(GameLibraries[libraryName].LibraryName, this.Libraries[libraryName], GameLibraries[libraryName].OnCdRom))
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
            dirEntry.uiOffset = br.ReadUInt32();
            dirEntry.uiLength = br.ReadUInt32();
            dirEntry.ubState = br.ReadByte();
            dirEntry.ubReserved = br.ReadByte();
            dirEntry.sFileTime = this.ParseFileTime(br);
            dirEntry.usReserved2 = br.ReadUInt16();

            // padding
            br.ReadByte();
            br.ReadByte();

            return dirEntry;
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
            { LibraryNames.DATA,        new("Data.slf", false, true) },
            { LibraryNames.AMBIENT,     new("Ambient.slf", false, true) },
            { LibraryNames.ANIMS,       new("Anims.slf", false, true) },
            { LibraryNames.BATTLESNDS,  new("BattleSnds.slf", false, true) },
            { LibraryNames.BIGITEMS,    new("BigItems.slf", false, true) },
            { LibraryNames.BINARY_DATA, new("BinaryData.slf", false, true) },
            { LibraryNames.CURSORS,     new("Cursors.slf", false, true) },
            { LibraryNames.FACES,       new("Faces.slf", false, true) },
            { LibraryNames.FONTS,       new("Fonts.slf", false, true) },
            { LibraryNames.INTERFACE,   new("Interface.slf", false, true) },
            { LibraryNames.LAPTOP,      new("Laptop.slf", false, true) },
            { LibraryNames.MAPS,        new("Maps.slf", true, true) },
            { LibraryNames.MERCEDT,     new("MercEdt.slf", false, true) },
            { LibraryNames.MUSIC,       new("Music.slf", true, true) },
            { LibraryNames.NPC_SPEECH,  new("Npc_Speech.slf", true, true) },
            { LibraryNames.NPC_DATA,    new("NpcData.slf", false, true) },
            { LibraryNames.RADAR_MAPS,  new("RadarMaps.slf", false, true) },
            { LibraryNames.SOUNDS,      new("Sounds.slf", false, true) },
            { LibraryNames.SPEECH,      new("Speech.slf", true, true) },
            { LibraryNames.TILESETS,    new("TileSets.slf", true, true) },
            { LibraryNames.LOADSCREENS, new("LoadScreens.slf", true, true) },
            { LibraryNames.INTRO,       new("Intro.slf", true, true) },
        };
    }
}
