﻿using System;
using System.Collections.Generic;
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
    Veldrid.GraphicsDevice GraphicDevice { get; }

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
    void GetVSurfacePaletteEntries(HVSURFACE hSrcVSurface, List<SGPPaletteEntry> pPalette);
    ushort[] Create16BPPPaletteShaded(List<SGPPaletteEntry> pPalette, int redScale, int greenScale, int blueScale, bool mono);
    void DeleteVideoSurfaceFromIndex(Surfaces uiTempMap);
    void DeleteVideoObjectFromIndex(string logoKey);
    void RestoreBackgroundRects();
    void GetCurrentVideoSettings(out int usWidth, out int usHeight, out int ubBitDepth);
    HVOBJECT CreateVideoObject(string assetPath);
    void LineDraw(int v2, int v3, int v4, int v5, Color v6, Image<Rgba32> image);
    void SetClippingRegionAndImageWidth(uint uiDestPitchBYTES, int v1, int v2, int v3, int v4);
    void Blt16BPPBufferHatchRect(ref byte[] pDestBuf, uint uiDestPitchBYTES, ref Rectangle clipRect);
    void GetClippingRect(out Rectangle clipRect);
    void ColorFillVideoSurfaceArea(Image<Rgba32> surface, Rectangle region, Rgba32 rgba32);
    void SaveBackgroundRects();
    void ImageFillVideoSurfaceArea(Rectangle region, HVOBJECT hVOBJECT, ushort v3, short v4, short v5);
    void ExecuteBaseDirtyRectQueue();
    void Blt8BPPDataTo8BPPBufferTransparentClip(ref byte[] pDestBuf, uint uiDestPitchBYTES, HVOBJECT bPic, int v, int yLoc, ushort imgNum, ref Rectangle clipRect);
    void SetClippingRect(ref Rectangle newClip);

    // SpriteRenderer SpriteRenderer { get; }
    static DebugRenderer DebugRenderer { get; protected set; }

    void ColorFillVideoSurfaceArea(Image<Rgba32> surface, Rectangle rectangle, Color color);
    bool ShadowVideoSurfaceRectUsingLowPercentTable(Surfaces surface, Rectangle rectangle);
    void DeleteVideoObject(HVOBJECT vobj);
    bool BlitBufferToBuffer(Surfaces srcBuffer, Surfaces dstBuffer, int srcX, int srcY, int width, int height);
    void SetVideoSurfaceTransparency(Surfaces uiVideoSurfaceImage, Rgba32 pixel);
    void ClearElements();
    Image<Rgba32> LockVideoSurface(Surfaces buffer, out int uiSrcPitchBYTES);
    bool Blt16BPPTo16BPP(Image<Rgba32> pDest, int uiDestPitch, Image<Rgba32> pSrc, int uiSrcPitch, int iDestXPos, int iDestYPos, int iSrcXPos, int iSrcYPos, int uiWidth, int uiHeight);
    void AddVideoObject(out VSURFACE_DESC vs_desc, out uint uiTempMap);
    void Blt8BPPDataSubTo16BPPBuffer(Image<Rgba32> pDestBuf, int uiDestPitchBYTES, HVSURFACE hSrcVSurface, Image<Rgba32> pSrcBuf, int uiSrcPitchBYTES, int v1, int v2, out Rectangle clip);
    void Blt8BPPTo8BPP(Image<Rgba32> pDestBuf, int uiDestPitchBYTES, Image<Rgba32> pSrcBuf, int uiSrcPitchBYTES, int sLeft1, int sTop1, int sLeft2, int sTop2, int sWidth, int sHeight);
    void ColorFillVideoSurfaceArea(Rectangle rectangle, Color color);
    void ColorFillVideoSurfaceArea(Rectangle region, Rgba32 rgba32);
    void ColorFillVideoSurfaceArea(Surfaces buttonDestBuffer, int regionTopLeftX, int regionTopLeftY, int regionBottomRightX, int regionBottomRightY, Rgba32 rgba32);
    void ColorFillVideoSurfaceArea(Surfaces buffer, Rectangle rectangle, Color black);
    bool GetETRLEImageData(HIMAGE? hImage, ref ETRLEData pBuffer);
    bool GetVideoObject(out HVOBJECT? hVObject, int uiIndex);
    bool GetVideoSurface(out HVSURFACE hSrcVSurface, uint uiTempMap);
    void ImageFillVideoSurfaceArea(Surfaces buttonDestBuffer, int v1, int v2, int regionBottomRightX, int regionBottomRightY, HVOBJECT hVOBJECT, ushort v3, short v4, short v5);
    ValueTask<bool> Initialize();
    static Stream OpenEmbeddedAssetStream(string name) => throw new NotImplementedException();
    static byte[] ReadEmbeddedAssetBytes(string name) => throw new NotImplementedException();
    bool SetVideoObjectPalette(HVOBJECT hVObject, HIMAGE hImage, List<SGPPaletteEntry> pSrcPalette);
    void UnLockVideoSurface(Surfaces buffer);
    void InvalidateRegionEx(int sLeft, int sTop, int v1, int v2, int v3);
    bool GetVideoObjectETRLEPropertiesFromIndex(string uiVideoObject, out ETRLEObject eTRLEObject, int index);
    bool TryCreateVideoSurface(VSURFACE_DESC vs_desc, out Surfaces uiVideoSurfaceImage);
    bool BltVideoObjectFromIndex(Surfaces uiSourceBufferIndex, int guiSkullIcons, int v1, int v2, int v3, VO_BLT sRCTRANSPARENCY, blt_fx? value);
    void DeleteVideoObjectFromIndex(Surfaces uiMercTextPopUpBackground);
    Image<Rgba32> AddVideoSurface(string v, out Surfaces uiMercTextPopUpBackground);
}
