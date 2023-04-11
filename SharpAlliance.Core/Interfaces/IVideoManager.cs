using System;
using System.Threading.Tasks;
using SharpAlliance.Core.Managers;
using SharpAlliance.Core.Managers.Image;
using SharpAlliance.Core.Managers.VideoSurfaces;
using SharpAlliance.Core.SubSystems;
using SharpAlliance.Platform.Interfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core.Interfaces;

public interface IVideoManager : ISharpAllianceManager
{
    ValueTask<bool> ISharpAllianceManager.Initialize() => ValueTask.FromResult(true);

    public const int MAX_CURSOR_WIDTH = 64;
    public const int MAX_CURSOR_HEIGHT = 64;
    public static Rgba32 AlphaPixel = new(255, 255, 255, 0);

    // TODO move to better area
    public static Veldrid.GraphicsDevice GraphicDevice { get; }

    static void DrawFrame() => throw new NotImplementedException();
    static void RefreshScreen() => throw new NotImplementedException();
    static void InvalidateScreen() => throw new NotImplementedException();
    static void InvalidateRegion(Rectangle bounds) => throw new NotImplementedException();
    static void EndFrameBufferRender() => throw new NotImplementedException();
    static HVOBJECT? AddVideoObject(string assetPath, out string key) => throw new NotImplementedException();
    static HVOBJECT? GetVideoObject(string key) => throw new NotImplementedException();
    static void BltVideoObject(HVOBJECT videoObject, int regionIndex, int X, int Y, int textureIndex) => throw new NotImplementedException();
    static bool DrawTextToScreen(string text, int x, int y, int width, FontStyle fontStyle, FontColor fontColorForeground, FontColor fontColorBackground, TextJustifies justification) => throw new NotImplementedException();
    static bool GetVideoSurface(out HVSURFACE hSrcVSurface, Surfaces uiTempMap) => throw new NotImplementedException();
    static int AddVideoSurface(out VSURFACE_DESC vs_desc, out Surfaces uiTempMap) => throw new NotImplementedException();
    static void GetVSurfacePaletteEntries(HVSURFACE hSrcVSurface, SGPPaletteEntry[] pPalette) => throw new NotImplementedException();
    static ushort Create16BPPPaletteShaded(ref SGPPaletteEntry[] pPalette, int redScale, int greenScale, int blueScale, bool mono) => throw new NotImplementedException();
    static void DeleteVideoSurfaceFromIndex(Surfaces uiTempMap) => throw new NotImplementedException();
    static void DeleteVideoObjectFromIndex(string logoKey) => throw new NotImplementedException();
    static void RestoreBackgroundRects() => throw new NotImplementedException();
    static void GetCurrentVideoSettings(out int usWidth, out int usHeight, out int ubBitDepth) => throw new NotImplementedException();
    static HVOBJECT CreateVideoObject(string assetPath) => throw new NotImplementedException();
    static void LineDraw(int v2, int v3, int v4, int v5, Color v6, Image<Rgba32> image) => throw new NotImplementedException();
    static void SetClippingRegionAndImageWidth(uint uiDestPitchBYTES, int v1, int v2, int v3, int v4) => throw new NotImplementedException();
    static void Blt16BPPBufferHatchRect(ref byte[] pDestBuf, uint uiDestPitchBYTES, ref Rectangle clipRect) => throw new NotImplementedException();
    static void GetClippingRect(out Rectangle clipRect) => throw new NotImplementedException();
    static void ColorFillVideoSurfaceArea(Surfaces surface, Rectangle region, Rgba32 rgba32) => throw new NotImplementedException();
    static void SaveBackgroundRects() => throw new NotImplementedException();
    static void ImageFillVideoSurfaceArea(Rectangle region, HVOBJECT hVOBJECT, ushort v3, short v4, short v5) => throw new NotImplementedException();
    static void ExecuteBaseDirtyRectQueue() => throw new NotImplementedException();
    static void Blt8BPPDataTo8BPPBufferTransparentClip(ref byte[] pDestBuf, uint uiDestPitchBYTES, HVOBJECT bPic, int v, int yLoc, ushort imgNum, ref Rectangle clipRect) => throw new NotImplementedException();
    static void SetClippingRect(ref Rectangle newClip) => throw new NotImplementedException();

    // SpriteRenderer SpriteRenderer { get; }
    static DebugRenderer DebugRenderer { get; protected set; }

    static void ColorFillVideoSurfaceArea(Surfaces surface, Rectangle rectangle, Color color) => throw new NotImplementedException();
    static void ShadowVideoSurfaceRectUsingLowPercentTable(Rectangle rectangle) => throw new NotImplementedException();
    static void DeleteVideoObject(HVOBJECT vobj) => throw new NotImplementedException();
    static void BlitBufferToBuffer(int left, int top, int v1, int v2) => throw new NotImplementedException();
    static void SetVideoSurfaceTransparency(Surfaces uiVideoSurfaceImage, int v) => throw new NotImplementedException();
    static void ClearElements() => throw new NotImplementedException();
}
