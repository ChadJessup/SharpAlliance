namespace SharpAlliance.Platform.LibraryDatabase
{
    public struct RealFileHeader
    {
        public int iNumFilesOpen;
        public int iSizeOfOpenFileArray;
        public RealFileOpen pRealFilesOpen;
    }
}
