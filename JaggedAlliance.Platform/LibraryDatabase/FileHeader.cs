namespace JaggedAlliance.Platform.LibraryDatabase
{
    public readonly struct FileHeader
    {
        public string pFileName { get; init; }
        public uint uiFileLength { get; init; }
        public uint uiFileOffset { get; init; }
    }
}
