namespace SharpAlliance.Core.LibraryManager
{
    public readonly struct FileHeader
    {
        public string pFileName { get; init; }
        public uint uiFileLength { get; init; }
        public uint uiFileOffset { get; init; }
    }
}
