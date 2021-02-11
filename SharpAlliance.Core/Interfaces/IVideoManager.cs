﻿using System.Threading.Tasks;
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
        HVOBJECT AddVideoObject(string assetPath, out string key);
        HVOBJECT GetVideoObject(string key);
        void BltVideoObject(HVOBJECT videoObject, int regionIndex, int X, int Y, int textureIndex);
        void DrawTextToScreen(string v1, int v2, int v3, int v4, FontStyle fONT10ARIAL, FontColor fONT_MCOLOR_WHITE, FontColor fONT_MCOLOR_BLACK, bool v5, TextJustifies cENTER_JUSTIFIED);
        void GetVideoSurface(out HVSURFACE hSrcVSurface, uint uiTempMap);
        void AddVideoSurface(out VSURFACE_DESC vs_desc, out uint uiTempMap);
        void GetVSurfacePaletteEntries(HVSURFACE hSrcVSurface, SGPPaletteEntry[] pPalette);
        ushort Create16BPPPaletteShaded(ref SGPPaletteEntry[] pPalette, int redScale, int greenScale, int blueScale, bool mono);
        void DeleteVideoSurfaceFromIndex(uint uiTempMap);
        void DeleteVideoObjectFromIndex(string logoKey);
        void RestoreBackgroundRects();
        HVOBJECT CreateVideoObject(string assetPath);
        void LineDraw(int v2, int v3, int v4, int v5, Color v6, Image<Rgba32> image);
        byte[] LockVideoSurface(Surfaces buttonDestBuffer, out uint uiDestPitchBYTES);
        void SetClippingRegionAndImageWidth(uint uiDestPitchBYTES, int v1, int v2, int v3, int v4);
        void UnLockVideoSurface(Surfaces buttonDestBuffer);
        void Blt16BPPBufferHatchRect(ref byte[] pDestBuf, uint uiDestPitchBYTES, ref Rectangle clipRect);
        void Blt16BPPBufferShadowRect(ref byte[] pDestBuf, uint uiDestPitchBYTES, ref Rectangle clipRect);
        void GetClippingRect(out Rectangle clipRect);
        void ColorFillVideoSurfaceArea(Rectangle region, Rgba32 rgba32);
        void SaveBackgroundRects();
        void ImageFillVideoSurfaceArea(Rectangle region, HVOBJECT hVOBJECT, ushort v3, short v4, short v5);
        void ExecuteBaseDirtyRectQueue();
        void Blt8BPPDataTo16BPPBufferTransparentClip(ref byte[] pDestBuf, uint uiDestPitchBYTES, HVOBJECT bPic, int v, int yLoc, ushort imgNum, ref Rectangle clipRect);
        void Blt8BPPDataTo8BPPBufferTransparentClip(ref byte[] pDestBuf, uint uiDestPitchBYTES, HVOBJECT bPic, int v, int yLoc, ushort imgNum, ref Rectangle clipRect);
        void SetClippingRect(ref Rectangle newClip);

        // SpriteRenderer SpriteRenderer { get; }
        static DebugRenderer DebugRenderer { get; protected set; }

        void ColorFillVideoSurfaceArea(Rectangle rectangle, Color color);
        void ShadowVideoSurfaceRectUsingLowPercentTable(Rectangle rectangle);
        void DeleteVideoObject(HVOBJECT vobj);
        void BlitBufferToBuffer(int left, int top, int v1, int v2);
    }
}
