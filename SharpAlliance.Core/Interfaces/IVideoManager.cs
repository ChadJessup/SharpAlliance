using SharpAlliance.Core.Managers;
using SharpAlliance.Core.Managers.Image;
using SharpAlliance.Core.Managers.VideoSurfaces;
using SharpAlliance.Core.SubSystems;
using SharpAlliance.Platform.Interfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

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
        int gbPixelDepth { get; }

        void DrawFrame();
        void RefreshScreen();
        void InvalidateScreen();
        void InvalidateRegion(Rectangle bounds);
        void EndFrameBufferRender();
        HVOBJECT AddVideoObject(ref VOBJECT_DESC vObjectDesc, out string key);
        void GetVideoObject(string key, out HVOBJECT hPixHandle);
        void BltVideoObject(HVOBJECT videoObject, int regionIndex, int X, int Y, int textureIndex);
        void DrawTextToScreen(string v1, int v2, int v3, int v4, FontStyle fONT10ARIAL, FontColor fONT_MCOLOR_WHITE, FontColor fONT_MCOLOR_BLACK, bool v5, TextJustifies cENTER_JUSTIFIED);
        void GetVideoSurface(out HVSURFACE hSrcVSurface, uint uiTempMap);
        void AddVideoSurface(out VSURFACE_DESC vs_desc, out uint uiTempMap);
        void GetVSurfacePaletteEntries(HVSURFACE hSrcVSurface, SGPPaletteEntry[] pPalette);
        ushort Create16BPPPaletteShaded(ref SGPPaletteEntry[] pPalette, int redScale, int greenScale, int blueScale, bool mono);
        void DeleteVideoSurfaceFromIndex(uint uiTempMap);
        void DeleteVideoObjectFromIndex(string logoKey);
        HVOBJECT CreateVideoObject(ref VOBJECT_DESC vo_desc);
        void LineDraw(bool v1, int v2, int v3, int v4, int v5, int v6, byte[] pDestBuf);
        byte[] LockVideoSurface(Surfaces buttonDestBuffer, out uint uiDestPitchBYTES);
        void SetClippingRegionAndImageWidth(uint uiDestPitchBYTES, int v1, int v2, int v3, int v4);
        void UnLockVideoSurface(Surfaces buttonDestBuffer);
        void Blt16BPPBufferHatchRect(ref byte[] pDestBuf, uint uiDestPitchBYTES, ref Rectangle clipRect);
        void Blt16BPPBufferShadowRect(ref byte[] pDestBuf, uint uiDestPitchBYTES, ref Rectangle clipRect);
        void GetClippingRect(out Rectangle clipRect);
        void ColorFillVideoSurfaceArea(Surfaces buttonDestBuffer, int regionTopLeftX, int regionTopLeftY, int regionBottomRightX, int regionBottomRightY, Rgba32 rgba32);
        void ImageFillVideoSurfaceArea(Surfaces buttonDestBuffer, int v1, int v2, int regionBottomRightX, int regionBottomRightY, HVOBJECT hVOBJECT, ushort v3, short v4, short v5);
        void Blt8BPPDataTo16BPPBufferTransparentClip(ref byte[] pDestBuf, uint uiDestPitchBYTES, HVOBJECT bPic, int v, int yLoc, ushort imgNum, ref Rectangle clipRect);
        void Blt8BPPDataTo8BPPBufferTransparentClip(ref byte[] pDestBuf, uint uiDestPitchBYTES, HVOBJECT bPic, int v, int yLoc, ushort imgNum, ref Rectangle clipRect);
        void SetClippingRect(ref Rectangle newClip);
    }
}
