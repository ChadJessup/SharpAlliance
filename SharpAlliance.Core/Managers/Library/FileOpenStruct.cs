using System.IO;
using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core.Managers.Library;

public class FileOpenStruct
{
    public int uiFileID;                                            // id of the file ( they start at 1 )
    public int uiFilePosInFile;                         // current position in the file
    public long uiActualPositionInLibrary;       // Current File pointer position in actuall library
    public FileHeader pFileHeader;
    public Stream FileStream;
}
