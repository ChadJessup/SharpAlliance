﻿using System.Collections.Generic;

namespace SharpAlliance.Core.Managers.Library
{
    public class RealFileHeader
    {
        public int iNumFilesOpen => this.pRealFilesOpen.Count;
        public List<RealFileOpen> pRealFilesOpen { get; } = new();
    }
}
