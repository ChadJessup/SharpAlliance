namespace SharpAlliance.Core.LibraryManager
{
    public struct FileOpen
    {
        uint uiFileID;                                            // id of the file ( they start at 1 )
        uint uiFilePosInFile;                         // current position in the file
        uint uiActualPositionInLibrary;       // Current File pointer position in actuall library
        FileHeader pFileHeader;
    }
}
