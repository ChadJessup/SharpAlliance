namespace SharpAlliance.Core.Managers.Library
{
    public class FileOpen
    {
        public uint uiFileID;                                            // id of the file ( they start at 1 )
        public int uiFilePosInFile;                         // current position in the file
        public uint uiActualPositionInLibrary;       // Current File pointer position in actuall library
        public FileHeader pFileHeader;
    }
}
