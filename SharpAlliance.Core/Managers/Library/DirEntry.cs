using System;

namespace SharpAlliance.Core.Managers.Library
{
    public struct DirEntry
    {
        public string sFileName { get; set; }
        public int uiOffset { get; set; }
        public int uiLength { get; set; }
        public byte ubState { get; set; }
        public byte ubReserved { get; set; }
        public DateTime sFileTime { get; set; }
        public ushort usReserved2 { get; set; }
    }
}
