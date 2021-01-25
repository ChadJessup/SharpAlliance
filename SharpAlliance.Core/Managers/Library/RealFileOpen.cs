using System.IO;
using Microsoft.Win32.SafeHandles;

namespace SharpAlliance.Core.Managers.Library
{
    public record RealFileOpen
    {
        public int uiFileID { get; set; }                        // id of the file ( they start at 1 )
        public Stream Stream { get; set; }     // if the file is a Real File, this its handle
    }
}
