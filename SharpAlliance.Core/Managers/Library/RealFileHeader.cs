﻿namespace SharpAlliance.Core.Managers.Library
{
    public struct RealFileHeader
    {
        public int iNumFilesOpen;
        public int iSizeOfOpenFileArray;
        public RealFileOpen pRealFilesOpen;
    }
}