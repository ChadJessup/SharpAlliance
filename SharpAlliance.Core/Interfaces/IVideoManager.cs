﻿using SharpAlliance.Core.Managers;
using SharpAlliance.Core.Managers.Image;
using SharpAlliance.Core.Managers.VideoSurfaces;
using SharpAlliance.Core.SubSystems;
using SharpAlliance.Platform.Interfaces;

namespace SharpAlliance.Core.Interfaces
{
    public interface IVideoManager : ISharpAllianceManager
    {
        public const int MAX_CURSOR_WIDTH = 64;
        public const int MAX_CURSOR_HEIGHT = 64;

        // TODO move to better area
        public uint guiBOTTOMPANEL { get; set; }
        public uint guiRIGHTPANEL { get; set; }
        public uint guiRENDERBUFFER { get; set; }
        public uint guiSAVEBUFFER { get; set; }
        public uint guiEXTRABUFFER { get; set; }
        public bool gfExtraBuffer { get; set; }

        void DrawFrame();
        void RefreshScreen();
        void InvalidateScreen();
        void InvalidateRegion(int v1, int v2, int v3, int v4);
        void EndFrameBufferRender();
        bool AddVideoObject(ref VOBJECT_DESC vObjectDesc, out int uiLogoID);
        void GetVideoObject(out HVOBJECT hPixHandle, int guiMainMenuBackGroundImage);
        void BltVideoObject(uint fRAME_BUFFER, HVOBJECT hPixHandle, int v1, int v2, int v3, int vO_BLT_SRCTRANSPARENCY, object p);
        void DrawTextToScreen(string v1, int v2, int v3, int v4, FontStyle fONT10ARIAL, FontColor fONT_MCOLOR_WHITE, FontColor fONT_MCOLOR_BLACK, bool v5, TextJustifies cENTER_JUSTIFIED);
        void GetVideoSurface(out HVSURFACE hSrcVSurface, uint uiTempMap);
        void AddVideoSurface(out VSURFACE_DESC vs_desc, out uint uiTempMap);
        void GetVSurfacePaletteEntries(HVSURFACE hSrcVSurface, SGPPaletteEntry[] pPalette);
        ushort Create16BPPPaletteShaded(ref SGPPaletteEntry[] pPalette, int redScale, int greenScale, int blueScale, bool mono);
        void DeleteVideoSurfaceFromIndex(uint uiTempMap);
    }
}
