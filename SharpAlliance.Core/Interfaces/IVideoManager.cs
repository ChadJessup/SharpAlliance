using System.Threading.Tasks;
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
        public static Rgba32 AlphaPixel = new(255, 255, 255, 0);

        // TODO move to better area
        public Veldrid.GraphicsDevice GraphicDevice { get; }

        void DrawFrame();
        void RefreshScreen();
        void InvalidateScreen();
        static abstract void InvalidateRegion(Rectangle bounds);
        static abstract void EndFrameBufferRender();
        HVOBJECT AddVideoObject(string assetPath, out string key);
        static abstract HVOBJECT GetVideoObject(string key);
        static abstract void BltVideoObject(HVOBJECT videoObject, int regionIndex, int X, int Y, int textureIndex);
        bool DrawTextToScreen(string text, int x, int y, int width, FontStyle fontStyle, FontColor fontColorForeground, FontColor fontColorBackground, TextJustifies justification);
        void GetVideoSurface(out HVSURFACE hSrcVSurface, Surfaces uiTempMap);
        int AddVideoSurface(out VSURFACE_DESC vs_desc, out Surfaces uiTempMap);
        void GetVSurfacePaletteEntries(HVSURFACE hSrcVSurface, SGPPaletteEntry[] pPalette);
        ushort Create16BPPPaletteShaded(ref SGPPaletteEntry[] pPalette, int redScale, int greenScale, int blueScale, bool mono);
        void DeleteVideoSurfaceFromIndex(Surfaces uiTempMap);
        static abstract void DeleteVideoObjectFromIndex(string logoKey);
        static abstract void RestoreBackgroundRects();
        void GetCurrentVideoSettings(out int usWidth, out int usHeight, out int ubBitDepth);
        HVOBJECT CreateVideoObject(string assetPath);
        static abstract void LineDraw(int v2, int v3, int v4, int v5, Color v6, Image<Rgba32> image);
        static abstract void SetClippingRegionAndImageWidth(uint uiDestPitchBYTES, int v1, int v2, int v3, int v4);
        void Blt16BPPBufferHatchRect(ref byte[] pDestBuf, uint uiDestPitchBYTES, ref Rectangle clipRect);
        static abstract void GetClippingRect(out Rectangle clipRect);
        static abstract void ColorFillVideoSurfaceArea(Surfaces surface, Rectangle region, Rgba32 rgba32);
        static abstract void SaveBackgroundRects();
        static abstract void ImageFillVideoSurfaceArea(Rectangle region, HVOBJECT hVOBJECT, ushort v3, short v4, short v5);
        void ExecuteBaseDirtyRectQueue();
        void Blt8BPPDataTo8BPPBufferTransparentClip(ref byte[] pDestBuf, uint uiDestPitchBYTES, HVOBJECT bPic, int v, int yLoc, ushort imgNum, ref Rectangle clipRect);
        static abstract void SetClippingRect(ref Rectangle newClip);

        // SpriteRenderer SpriteRenderer { get; }
        static DebugRenderer DebugRenderer { get; protected set; }

        static abstract void ColorFillVideoSurfaceArea(Surfaces surface, Rectangle rectangle, Color color);
        static abstract void ShadowVideoSurfaceRectUsingLowPercentTable(Rectangle rectangle);
        void DeleteVideoObject(HVOBJECT vobj);
        void BlitBufferToBuffer(int left, int top, int v1, int v2);
        void SetVideoSurfaceTransparency(Surfaces uiVideoSurfaceImage, int v);
        void ClearElements();
    }
}
