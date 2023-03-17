using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SharpAlliance.Core.Interfaces;
using SharpAlliance.Core.Managers.Image;
using SharpAlliance.Core.Managers.VideoSurfaces;
using SharpAlliance.Platform;
using SharpAlliance.Platform.Interfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Veldrid.ImageSharp;
using Veldrid.MetalBindings;

using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core.Managers;

public class VideoSurfaceManager //: IVideoSurfaceManager
{
    //        public const uint PRIMARY_SURFACE = 0xFFFFFFF0;
    //        public const uint BACKBUFFER = 0xFFFFFFF1;
    //        public const uint FRAME_BUFFER = 0xFFFFFFF2;
    //        public const uint MOUSE_BUFFER = 0xFFFFFFF3;
    //        public const int DEFAULT_NUM_REGIONS = 5;

    //        private readonly ILogger<VideoSurfaceManager> logger;
    //        private readonly VeldridVideoManager video;

    //        private VSURFACE_NODE? gpVSurfaceHead = null;
    //        private VSURFACE_NODE? gpVSurfaceTail = null;
    //        private int guiVSurfaceIndex = 0;
    //        private int guiVSurfaceSize = 0;
    //        private int guiVSurfaceTotalAdded = 0;
    //        private int giMemUsedInSurfaces;

    //        private HVSURFACE? ghPrimary = null;
    //        private HVSURFACE? ghBackBuffer = null;
    //        private HVSURFACE? ghFrameBuffer = null;
    //        private HVSURFACE? ghMouseBuffer = null;

    //        public VideoSurfaceManager(
    //            ILogger<VideoSurfaceManager> logger,
    //            IVideoManager videoManager)
    //        {
    //            this.logger = logger;

    //            VeldridVideoManager = (videoManager as VeldridVideoManager)!;

    //            this.IsInitialized = this.Initialize().AsTask().Result;
    //        }

    //        public bool IsInitialized { get; }

    //        public ValueTask<bool> Initialize()
    //        {
    //            this.logger.LogDebug(LoggingEventId.VIDEOSURFACE, "Video Surface Manager");
    //            this.gpVSurfaceHead = this.gpVSurfaceTail = null;

    //            this.giMemUsedInSurfaces = 0;

    //            // Create primary and backbuffer from globals
    //            if (!this.SetPrimaryVideoSurfaces())
    //            {
    //                this.logger.LogDebug(LoggingEventId.VIDEOSURFACE, "Could not create primary surfaces");
    //                return ValueTask.FromResult(false);
    //            }

    //            return ValueTask.FromResult(true);
    //        }

    //        private bool SetPrimaryVideoSurfaces()
    //        {
    //            //Surface2 pSurface;

    //            // Delete surfaces if they exist
    //            this.DeletePrimaryVideoSurfaces();

    //            //
    //            // Get Primary surface
    //            //
    //            //pSurface = VeldridVideoManager.GetPrimarySurfaceObject();
    //            // CHECKF(pSurface != null);

    //            //ghPrimary = CreateVideoSurfaceFromDDSurface(pSurface);
    //            // CHECKF(ghPrimary != null);

    //            //
    //            // Get Backbuffer surface
    //            //

    //            //pSurface = GetBackBufferObject();
    //            // CHECKF(pSurface != null);

    //            //ghBackBuffer = CreateVideoSurfaceFromDDSurface(pSurface);
    //            // CHECKF(ghBackBuffer != null);

    //            //
    //            // Get mouse buffer surface
    //            //
    //            //pSurface = GetMouseBufferObject();
    //            //CHECKF(pSurface != null);

    //            //ghMouseBuffer = CreateVideoSurfaceFromDDSurface(pSurface);
    //            //CHECKF(ghMouseBuffer != null);

    //            //
    //            // Get frame buffer surface
    //            //

    //            //pSurface = GetFrameBufferObject();
    //            //CHECKF(pSurface != null);

    //            //ghFrameBuffer = CreateVideoSurfaceFromDDSurface(pSurface);
    //            //CHECKF(ghFrameBuffer != null);

    //            return true;
    //        }

    //        private void DeletePrimaryVideoSurfaces()
    //        {
    //            //
    //            // If globals are not null, delete them
    //            //

    //            if (this.ghPrimary != null)
    //            {
    //                this.DeleteVideoSurface(this.ghPrimary);
    //                this.ghPrimary = null;
    //            }

    //            if (this.ghBackBuffer != null)
    //            {
    //                this.DeleteVideoSurface(this.ghBackBuffer);
    //                this.ghBackBuffer = null;
    //            }

    //            if (this.ghFrameBuffer != null)
    //            {
    //                this.DeleteVideoSurface(this.ghFrameBuffer);
    //                this.ghFrameBuffer = null;
    //            }

    //            if (this.ghMouseBuffer != null)
    //            {
    //                this.DeleteVideoSurface(this.ghMouseBuffer);
    //                this.ghMouseBuffer = null;
    //            }
    //        }

    //        // Deletes all palettes, surfaces and region data
    //        public bool DeleteVideoSurface(HVSURFACE? hVSurface)
    //        {
    //            //            Surface2 lpDDSurface;

    //            // Assertions
    //            //CHECKF(hVSurface != null);

    //            // Release palette
    //            //      if (hVSurface.pPalette != null)
    //            //      {
    //            //          DDReleasePalette((LPDIRECTDRAWPALETTE)hVSurface.pPalette);
    //            //          hVSurface.pPalette = null;
    //            //      }

    //            //if ( hVSurface.pClipper != null )
    //            //{
    //            // Release Clipper
    //            //	DDReleaseClipper( (LPDIRECTDRAWCLIPPER)hVSurface.pClipper );
    //            //}

    //            // Get surface pointer
    //            //     lpDDSurface = (Surface2)hVSurface.pSurfaceData;

    //            // Release surface
    //            //    if (hVSurface.pSurfaceData1 != null)
    //            //    {
    //            //        DDReleaseSurface((Surface1)hVSurface.pSurfaceData1, lpDDSurface);
    //            //    }

    //            // Release backup surface
    //            //  if (hVSurface.pSavedSurfaceData != null)
    //            //  {
    //            //      DDReleaseSurface((Surface1)hVSurface.pSavedSurfaceData1,
    //            //                              (Surface2)hVSurface.pSavedSurfaceData);
    //            //  }

    //            // Release region data
    //            hVSurface?.RegionList.Clear();
    //            hVSurface?.Texture.Dispose();

    //            return true;
    //        }

    //        public HVSURFACE CreateVideoSurface(VSURFACE_DESC VSurfaceDesc, IFileManager fileManager)
    //        {
    //            HVSURFACE hVSurface;
    //            HIMAGE hImage = new();
    //            Rectangle? tempRect = null;
    //            int usHeight;
    //            int usWidth;
    //            int ubBitDepth;

    //            if (VSurfaceDesc.fCreateFlags.HasFlag(VSurfaceCreateFlags.VSURFACE_CREATE_FROMFILE))
    //            {
    //                var tmpHIMAGE = HIMAGE.CreateImage(VSurfaceDesc.ImageFile, HIMAGECreateFlags.IMAGE_ALLIMAGEDATA, fileManager);

    ////                ETRLEData TempETRLEData = new();
    ////                // Get TRLE data
    ////                VeldridVideoManager.GetETRLEImageData(hImage, ref TempETRLEData);
    ////
    ////                // Set values
    ////                //hVObject.usNumberOfObjects = TempETRLEData.usNumberOfObjects;
    ////                //hVObject.pETRLEObject = TempETRLEData.pETRLEObject;
    ////                //hVObject.pPixData = TempETRLEData.pPixData;
    ////                //hVObject.uiSizePixData = TempETRLEData.uiSizePixData;
    ////
    ////                // Set palette from himage
    ////                if (hImage.ubBitDepth == 8)
    ////                {
    ////                    hVObject.pShade8 = this.shading.ubColorTables[Shading.DEFAULT_SHADE_LEVEL, 0];
    ////                    hVObject.pGlow8 = this.shading.ubColorTables[0, 0];
    ////
    ////                    VeldridVideoManager.SetVideoObjectPalette(hVObject, hImage, hImage.pPalette);
    ////                }

    //                hImage = tmpHIMAGE;

    //                hImage.ParsedImages[0].SaveAsPng("C:\\assets\\temp.png");

    //                //
    //                // Set values from himage
    //                //
    //                usHeight = hImage.usHeight;
    //                usWidth = hImage.usWidth;
    //                ubBitDepth = hImage.ubBitDepth;
    //            }

    //            usHeight = VSurfaceDesc.usHeight;
    //            usWidth = VSurfaceDesc.usWidth;
    //            ubBitDepth = VSurfaceDesc.ubBitDepth;

    //            hVSurface.usHeight = usHeight;
    //            hVSurface.usWidth = usWidth;
    //            hVSurface.Texture = new ImageSharpTexture(hImage.ParsedImages[0], mipmap: false).CreateDeviceTexture(VeldridVideoManager.GraphicDevice, VeldridVideoManager.GraphicDevice.ResourceFactory);
    //            hVSurface.TransparentColor = 0;// FROMRGB(0, 0, 0);
    //            hVSurface.RegionList = new List<VSurfaceRegion>(DEFAULT_NUM_REGIONS);

    //            if (VSurfaceDesc.fCreateFlags.HasFlag(VSurfaceCreateFlags.VSURFACE_CREATE_FROMFILE))
    //            {
    //                tempRect = new()
    //                {
    //                    X = 0,
    //                    Y = 0,
    //                    Width = hImage.usWidth - 1,
    //                    Height = hImage.usHeight - 1,
    //                };

    //                this.SetVideoSurfaceDataFromHImage(hVSurface, hImage, 0, 0, ref tempRect);

    //                // this.DestroyImage(hImage);
    //            }

    //            //
    //            // All is well
    //            //

    //            hVSurface.usHeight = usHeight;
    //            hVSurface.usWidth = usWidth;

    //            return hVSurface;
    //        }

    //        private void DestroyImage(HIMAGE hImage)
    //        {
    //            for (int i = 0; i < hImage.ParsedImages.Count; i++)
    //            {
    //                var img = hImage.ParsedImages[i];
    //                img.Dispose();
    //                hImage.ParsedImages.RemoveAt(i);
    //            }
    //        }

    //        private bool SetVideoSurfaceDataFromHImage(HVSURFACE hVSurface, HIMAGE hImage, int usX, int usY, ref Rectangle? pSrcRect)
    //        {
    //            Rectangle aRect = new();

    //            // Blit Surface
    //            // If rect is null, use entire image size
    //            if (pSrcRect == null)
    //            {
    //                aRect.X = 0;
    //                aRect.Y = 0;
    //                aRect.Width = hImage.usWidth;
    //                aRect.Height = hImage.usHeight;
    //            }
    //            else
    //            {
    //                aRect.X = pSrcRect.Value.X;
    //                aRect.Y = pSrcRect.Value.Y;
    //                aRect.Width = pSrcRect.Value.Width;
    //                aRect.Height = pSrcRect.Value.Height;
    //            }

    //            hImage.ParsedImages[0].Mutate(ipc => ipc.Crop(aRect));

    //            hImage.ParsedImages![0].TryGetSinglePixelSpan(out var pixelSpan);

    //            if (hVSurface.Texture == null)
    //            {
    //                hVSurface.Texture = new ImageSharpTexture(hImage.ParsedImages[0], mipmap: false)
    //                    .CreateDeviceTexture(
    //                        VeldridVideoManager.GraphicDevice,
    //                        VeldridVideoManager.GraphicDevice.ResourceFactory);
    //            }
    //            else
    //            {
    //                VeldridVideoManager.GraphicDevice.UpdateTexture<Rgba32>(
    //                    hVSurface.Texture,
    //                    pixelSpan.ToArray(),
    //                    (uint)usX,
    //                    (uint)usY,
    //                    0,
    //                    (uint)aRect.Width,
    //                    (uint)aRect.Height,
    //                    0,
    //                    0,
    //                    0);
    //            }

    //            return true;
    //        }

    //        public void Dispose()
    //        {
    //        }

    //        public byte[] LockVideoSurface(uint fRAME_BUFFER, out int uiDestPitchBYTES)
    //        {
    //            uiDestPitchBYTES = 0;
    //            return Array.Empty<byte>();
    //        }

    //        public void UnLockVideoSurface(uint fRAME_BUFFER)
    //        {
    //        }

    //        public void ColorFillVideoSurfaceArea(uint fRAME_BUFFER, int v1, int v2, int v3, int v4, int v5)
    //        {
    //        }

    //        public void ShadowVideoSurfaceRectUsingLowPercentTable(uint fRAME_BUFFER, int v1, int v2, int v3, int v4)
    //        {
    //        }
    //    }

    public static bool BltVideoSurface(Surfaces uiDestVSurface, Surfaces uiSrcVSurface, int usRegionIndex, int iDestX, int iDestY, BlitTypes fBltFlags, blt_vs_fx? pBltFx)
    {
        HVSURFACE hDestVSurface;
        HVSURFACE hSrcVSurface;

        if (!VeldridVideoManager.GetVideoSurface(out hDestVSurface, uiDestVSurface))
        {
            return false;
        }

        if (!VeldridVideoManager.GetVideoSurface(out hSrcVSurface, uiSrcVSurface))
        {
            return false;
        }

        //if (!BltVideoSurfaceToVideoSurface(hDestVSurface, hSrcVSurface, usRegionIndex, iDestX, iDestY, fBltFlags, pBltFx))
        //{ // VO Blitter will set debug messages for error conditions
        //    return false;
        //}

        return true;
    }

    internal static void ShadowVideoSurfaceRect(Surfaces uiDestBuff, int sX, int sY, int v1, int v2)
    {
        throw new NotImplementedException();
    }
}

//
// Effects structure for specialized blitting
//

public struct blt_vs_fx
{
    //COLORVAL ColorFill;     // Used for fill effect
    public Rectangle SrcRect;            // Given SRC subrect instead of srcregion
    public Rectangle FillRect;       // Given SRC subrect instead of srcregion
    public int DestRegion;  // Given a DEST region for dest positions within the VO

}
