﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpAlliance.Core.Managers;
using SharpAlliance.Platform.Interfaces;

using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core.Interfaces;

public interface IVideoObjectManager : ISharpAllianceManager
{
    bool GetVideoObject(string key, out HVOBJECT hPixHandle);
    bool BltVideoObject(
        uint uiDestVSurface,
        HVOBJECT hSrcVObject,
        ushort usRegionIndex,
        int iDestX,
        int iDestY,
        int fBltFlags,
        blt_fx? pBltFx);

    bool DeleteVideoObjectFromIndex(int uiLogoID);
}
