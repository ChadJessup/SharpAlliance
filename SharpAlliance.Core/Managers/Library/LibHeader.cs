namespace SharpAlliance.Core.Managers.Library
{
    public record LibHeader
    {
        public string sLibName { get; set; }
        public string sPathToLibrary { get; set; }
        public int iEntries { get; set; }
        public int iUsed { get; set; }
        public ushort iSort { get; set; }
        public ushort iVersion { get; set; }
        public bool fContainsSubDirectories { get; set; }
        public int iReserved { get; set; }
    }
}
