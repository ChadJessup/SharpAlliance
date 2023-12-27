using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using SharpAlliance.Core.Managers;
using SharpAlliance.Core.Managers.Image;
using SharpAlliance.Core.Managers.VideoSurfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace SharpAlliance.Core.Interfaces;

public interface IVideoManager : ISharpAllianceManager
{
    ValueTask<bool> ISharpAllianceManager.Initialize() => ValueTask.FromResult(true);

    public const int MAX_CURSOR_WIDTH = 64;
    public IWindow Window { get; }
    public const int MAX_CURSOR_HEIGHT = 64;
    public static Rgba32 AlphaPixel = new(255, 255, 255, 0);

    public ISurfaceManager Surfaces { get; }
    public nint Renderer { get; }
    void DrawFrame();
    void RefreshScreen();
    void InvalidateScreen();
    void InvalidateRegion(Rectangle bounds);
    void InvalidateRegion(int v1, int v2, int v3, int v4);
    void EndFrameBufferRender();
    HVOBJECT GetVideoObject(string assetPath);
    HVOBJECT GetVideoObject(string image, out string key);
    bool DrawTextToScreen(string text, int x, int y, int width, FontStyle fontStyle, FontColor fontColorForeground, FontColor fontColorBackground, TextJustifies justification);
    bool GetVideoSurface(out HVSURFACE hSrcVSurface, SurfaceType uiTempMap);
    void GetVSurfacePaletteEntries(HVSURFACE hSrcVSurface, List<SGPPaletteEntry> pPalette);
    ushort[] Create16BPPPaletteShaded(List<SGPPaletteEntry> pPalette, int redScale, int greenScale, int blueScale, bool mono);
    void DeleteVideoSurfaceFromIndex(SurfaceType uiTempMap);
    void DeleteVideoObjectFromIndex(string logoKey);
    void RestoreBackgroundRects();
    void GetCurrentVideoSettings(out int usWidth, out int usHeight, out int ubBitDepth);
    void LineDraw(bool fClip, PointF startPoint, PointF endPoint, Color Color, Image<Rgba32> dst);
    void GetClippingRect(out Rectangle clipRect);
    void ColorFillVideoSurfaceArea(Image<Rgba32> surface, Rectangle region, Rgba32 rgba32);
    void SaveBackgroundRects();
    void ImageFillVideoSurfaceArea(Rectangle region, HVOBJECT hVOBJECT, ushort v3, short v4, short v5);
    void ExecuteBaseDirtyRectQueue();
    void SetClippingRect(ref Rectangle newClip);
    void ColorFillVideoSurfaceArea(Image<Rgba32> surface, Rectangle rectangle, Color color);
    bool ShadowVideoSurfaceRectUsingLowPercentTable(SurfaceType surface, Rectangle rectangle);
    void DeleteVideoObject(HVOBJECT vobj);
    bool BlitBufferToBuffer(SurfaceType srcBuffer, SurfaceType dstBuffer, Rectangle srcRect);
    void SetVideoSurfaceTransparency(SurfaceType uiVideoSurfaceImage, Rgba32 pixel);
    void ClearElements();
    bool Blt16BPPTo16BPP(Image<Rgba32> pDest, Image<Rgba32> pSrc, Point iDestPos, Point iSrcPos, int uiWidth, int uiHeight);
    bool Blt8BPPDataSubTo16BPPBuffer(Image<Rgba32> pDestBuf, Size size, Image<Rgba32> pSrcBuf, int iX, int iY, out Rectangle clip);
    void Blt8BPPTo8BPP(Image<Rgba32> pDestBuf, int uiDestPitchBYTES, Image<Rgba32> pSrcBuf, int uiSrcPitchBYTES, int sLeft1, int sTop1, int sLeft2, int sTop2, int sWidth, int sHeight);
    void ColorFillVideoSurfaceArea(Rectangle rectangle, Color color);
    void ColorFillVideoSurfaceArea(Rectangle region, Rgba32 rgba32);
    void ColorFillVideoSurfaceArea(SurfaceType buffer, Rectangle rectangle, Color black);
   // bool GetVideoObject(out HVOBJECT? hVObject, int uiIndex);
    static Stream OpenEmbeddedAssetStream(string name) => throw new NotImplementedException();
    static byte[] ReadEmbeddedAssetBytes(string name) => throw new NotImplementedException();
    void InvalidateRegionEx(int sLeft, int sTop, int v1, int v2, int flags);
    void InvalidateRegionEx(Rectangle bounds, int flags);
    bool GetVideoObjectETRLEPropertiesFromIndex(string uiVideoObject, out ETRLEObject eTRLEObject, int index);
    bool BltVideoObjectFromIndex(SurfaceType uiSourceBufferIndex, int guiSkullIcons, int v1, int v2, int v3, VO_BLT sRCTRANSPARENCY, blt_fx? value);
    void DeleteVideoObjectFromIndex(SurfaceType uiMercTextPopUpBackground);
    HVOBJECT LoadImage(string assetPath);
    Texture[] CreateSurfaces(nint renderer, Image<Rgba32>[] image);
    void BlitSurfaceToSurface(Image<Rgba32> src, SurfaceType dst, Point dstPoint, VO_BLT bltFlags);
    void Draw();
    void BltVideoObject(SurfaceType surface, HVOBJECT hPixHandle, int index, int x, int y, VO_BLT bltFlags = VO_BLT.SRCTRANSPARENCY, int? _ = default)
        => this.BlitSurfaceToSurface(hPixHandle.Images[index], surface, new(x, y), bltFlags);
    void SetClippingRegionAndImageWidth(int width, Rectangle rectangle);
    bool ShadowVideoSurfaceRect(SurfaceType fRAME_BUFFER, Rectangle rectangle);
    void BltVideoSurface(SurfaceType uiBuffer, SurfaceType surfaceType, int v, int sDestX, int sDestY, BlitTypes blitTypes, object value);
}
