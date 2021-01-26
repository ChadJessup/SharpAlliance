using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpAlliance.Core.Managers;
using SharpAlliance.Platform.Interfaces;

namespace SharpAlliance.Core.Interfaces
{
    public interface IVideoObjectManager : ISharpAllianceManager
    {
        bool GetVideoObject(int uiLogoID, out HVOBJECT hPixHandle);
        bool BltVideoObject(uint fRAME_BUFFER, HVOBJECT hPixHandle, int v1, int v2, int v3, int vO_BLT_SRCTRANSPARENCY, object? p);
        bool DeleteVideoObjectFromIndex(int uiLogoID);
    }
}
