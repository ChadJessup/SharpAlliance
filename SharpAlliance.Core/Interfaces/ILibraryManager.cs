using System.Collections.Generic;
using System.IO;
using SharpAlliance.Core.Managers.Library;
using SharpAlliance.Platform.Interfaces;

namespace SharpAlliance.Core.Interfaces
{
    public interface ILibraryManager : ISharpAllianceManager
    {
        bool IsLibraryOpened(LibraryNames iNTRO);
        bool CheckIfFileExistsInLibrary(string pFilename);
        Stream OpenFileFromLibrary(string strFilename);
    }

    public class DatabaseManagerHeader
    {
        public string sManagerName;
        public Dictionary<LibraryNames, LibraryHeader> pLibraries { get; set; }
        public int usNumberOfLibraries;
        public bool fInitialized;
        public RealFileHeader RealFiles { get; } = new();
    }
}
