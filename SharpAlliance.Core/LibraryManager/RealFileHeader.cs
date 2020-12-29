namespace SharpAlliance.Core.LibraryManager
{
    public struct RealFileHeader
    {
        public int iNumFilesOpen;
        public int iSizeOfOpenFileArray;
        public RealFileOpen pRealFilesOpen;
    }
}
