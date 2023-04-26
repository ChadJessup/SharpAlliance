using System;
using System.IO;
using System.Threading.Tasks;
using SharpAlliance.Core.Managers;
using SharpAlliance.Core.Managers.Image;
using SharpAlliance.Core.Managers.VideoSurfaces;
using SharpAlliance.Core.SubSystems;
using SharpAlliance.Platform.Interfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Veldrid.Sdl2;

namespace SharpAlliance.Core.Interfaces;

public interface IVideoManager : ISharpAllianceManager
{
    static Sdl2Window Window;
    ValueTask<bool> ISharpAllianceManager.Initialize() => ValueTask.FromResult(true);

    public const int MAX_CURSOR_WIDTH = 64;
    public const int MAX_CURSOR_HEIGHT = 64;
    public static Rgba32 AlphaPixel = new(255, 255, 255, 0);

    // TODO move to better area
    public static Veldrid.GraphicsDevice GraphicDevice { get; }

    void DrawFrame();
    void RefreshScreen();
    void InvalidateScreen();
    void InvalidateRegion(Rectangle bounds) => throw new NotImplementedException();
    void InvalidateRegion(int v1, int v2, int v3, int v4) => throw new NotImplementedException();
    void EndFrameBufferRender();
    HVOBJECT? AddVideoObject(string assetPath, out string key);
    HVOBJECT? GetVideoObject(string key);
    void BltVideoObject(HVOBJECT videoObject, int regionIndex, int X, int Y, int textureIndex);
    bool DrawTextToScreen(string text, int x, int y, int width, FontStyle fontStyle, FontColor fontColorForeground, FontColor fontColorBackground, TextJustifies justification);
    bool GetVideoSurface(out HVSURFACE hSrcVSurface, Surfaces uiTempMap);
    int AddVideoSurface(out VSURFACE_DESC vs_desc, out Surfaces uiTempMap);
    void GetVSurfacePaletteEntries(HVSURFACE hSrcVSurface, SGPPaletteEntry[] pPalette);
    ushort Create16BPPPaletteShaded(ref SGPPaletteEntry[] pPalette, int redScale, int greenScale, int blueScale, bool mono);
    void DeleteVideoSurfaceFromIndex(Surfaces uiTempMap);
    void DeleteVideoObjectFromIndex(string logoKey);
    void RestoreBackgroundRects();
    void GetCurrentVideoSettings(out int usWidth, out int usHeight, out int ubBitDepth);
    HVOBJECT CreateVideoObject(string assetPath);
    void LineDraw(int v2, int v3, int v4, int v5, Color v6, Image<Rgba32> image);
    void SetClippingRegionAndImageWidth(uint uiDestPitchBYTES, int v1, int v2, int v3, int v4);
    void Blt16BPPBufferHatchRect(ref byte[] pDestBuf, uint uiDestPitchBYTES, ref Rectangle clipRect);
    void GetClippingRect(out Rectangle clipRect);
    void ColorFillVideoSurfaceArea(Surfaces surface, Rectangle region, Rgba32 rgba32);
    void SaveBackgroundRects();
    void ImageFillVideoSurfaceArea(Rectangle region, HVOBJECT hVOBJECT, ushort v3, short v4, short v5);
    void ExecuteBaseDirtyRectQueue();
    void Blt8BPPDataTo8BPPBufferTransparentClip(ref byte[] pDestBuf, uint uiDestPitchBYTES, HVOBJECT bPic, int v, int yLoc, ushort imgNum, ref Rectangle clipRect);
    void SetClippingRect(ref Rectangle newClip);

    // SpriteRenderer SpriteRenderer { get; }
    static DebugRenderer DebugRenderer { get; protected set; }

    void ColorFillVideoSurfaceArea(Surfaces surface, Rectangle rectangle, Color color);
    void ShadowVideoSurfaceRectUsingLowPercentTable(Rectangle rectangle);
    void DeleteVideoObject(HVOBJECT vobj);
    void BlitBufferToBuffer(int left, int top, int v1, int v2);
    void SetVideoSurfaceTransparency(Surfaces uiVideoSurfaceImage, int v);
    void ClearElements();
    int LockVideoSurface(Surfaces buffer, out int uiSrcPitchBYTES);
    bool Blt16BPPTo16BPP(int pDest, int uiDestPitch, int pSrc, int uiSrcPitch, int iDestXPos, int iDestYPos, int iSrcXPos, int iSrcYPos, int uiWidth, int uiHeight);
    void AddVideoSurface(out VSURFACE_DESC vs_desc, out uint uiTempMap);
    void Blt8BPPDataSubTo16BPPBuffer(int pDestBuf, int uiDestPitchBYTES, HVSURFACE hSrcVSurface, int pSrcBuf, int uiSrcPitchBYTES, int v1, int v2, out Rectangle clip);
    void Blt8BPPTo8BPP(int pDestBuf, int uiDestPitchBYTES, int pSrcBuf, int uiSrcPitchBYTES, int sLeft1, int sTop1, int sLeft2, int sTop2, int sWidth, int sHeight);
    void ColorFillVideoSurfaceArea(Rectangle rectangle, Color color);
    void ColorFillVideoSurfaceArea(Rectangle region, Rgba32 rgba32);
    void ColorFillVideoSurfaceArea(Surfaces buttonDestBuffer, int regionTopLeftX, int regionTopLeftY, int regionBottomRightX, int regionBottomRightY, Rgba32 rgba32);
    bool GetETRLEImageData(HIMAGE? hImage, ref ETRLEData pBuffer);
    bool GetVideoObject(out HVOBJECT? hVObject, int uiIndex);
    bool GetVideoSurface(out HVSURFACE hSrcVSurface, uint uiTempMap);
    void ImageFillVideoSurfaceArea(Surfaces buttonDestBuffer, int v1, int v2, int regionBottomRightX, int regionBottomRightY, HVOBJECT hVOBJECT, ushort v3, short v4, short v5);
    ValueTask<bool> Initialize();
    static Stream OpenEmbeddedAssetStream(string name) => throw new NotImplementedException();
    static byte[] ReadEmbeddedAssetBytes(string name) => throw new NotImplementedException();
    bool SetVideoObjectPalette(HVOBJECT hVObject, HIMAGE hImage, SGPPaletteEntry[] pSrcPalette);
    void UnLockVideoSurface(Surfaces buffer);
    void InvalidateRegionEx(int sLeft, int sTop, int v1, int v2, int v3);
}
