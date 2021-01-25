using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SharpAlliance.Core.Interfaces;
using SharpAlliance.Core.Managers.Image;
using SharpAlliance.Core.Managers.VideoSurfaces;
using SharpAlliance.Platform;
using SharpAlliance.Platform.Interfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Rectangle = Veldrid.Rectangle;

namespace SharpAlliance.Core.Managers
{
    public class VideoSurfaceManager : IVideoSurfaceManager
    {
        public const uint PRIMARY_SURFACE = 0xFFFFFFF0;
        public const uint BACKBUFFER = 0xFFFFFFF1;
        public const uint FRAME_BUFFER = 0xFFFFFFF2;
        public const uint MOUSE_BUFFER = 0xFFFFFFF3;

        private readonly ILogger<VideoSurfaceManager> logger;
        private readonly VeldridVideoManager video;

        private VSURFACE_NODE? gpVSurfaceHead = null;
        private VSURFACE_NODE? gpVSurfaceTail = null;
        private int guiVSurfaceIndex = 0;
        private int guiVSurfaceSize = 0;
        private int guiVSurfaceTotalAdded = 0;
        private int giMemUsedInSurfaces;

        private HVSURFACE? ghPrimary = null;
        private HVSURFACE? ghBackBuffer = null;
        private HVSURFACE? ghFrameBuffer = null;
        private HVSURFACE? ghMouseBuffer = null;

        public VideoSurfaceManager(
            ILogger<VideoSurfaceManager> logger,
            IVideoManager videoManager)
        {
            this.logger = logger;

            this.video = (videoManager as VeldridVideoManager)!;

            this.IsInitialized = this.Initialize().AsTask().Result;
        }

        public bool IsInitialized { get; }

        public ValueTask<bool> Initialize()
        {
            this.logger.LogDebug(LoggingEventId.VIDEOSURFACE, "Video Surface Manager");
            this.gpVSurfaceHead = this.gpVSurfaceTail = null;

            this.giMemUsedInSurfaces = 0;

            // Create primary and backbuffer from globals
            if (!this.SetPrimaryVideoSurfaces())
            {
                this.logger.LogDebug(LoggingEventId.VIDEOSURFACE, "Could not create primary surfaces");
                return ValueTask.FromResult(false);
            }

            return ValueTask.FromResult(true);
        }

        private bool SetPrimaryVideoSurfaces()
        {
            //Surface2 pSurface;

            // Delete surfaces if they exist
            this.DeletePrimaryVideoSurfaces();

            //
            // Get Primary surface
            //
            //pSurface = this.video.GetPrimarySurfaceObject();
            // CHECKF(pSurface != null);

            //ghPrimary = CreateVideoSurfaceFromDDSurface(pSurface);
            // CHECKF(ghPrimary != null);

            //
            // Get Backbuffer surface
            //

            //pSurface = GetBackBufferObject();
            // CHECKF(pSurface != null);

            //ghBackBuffer = CreateVideoSurfaceFromDDSurface(pSurface);
            // CHECKF(ghBackBuffer != null);

            //
            // Get mouse buffer surface
            //
            //pSurface = GetMouseBufferObject();
            //CHECKF(pSurface != null);

            //ghMouseBuffer = CreateVideoSurfaceFromDDSurface(pSurface);
            //CHECKF(ghMouseBuffer != null);

            //
            // Get frame buffer surface
            //

            //pSurface = GetFrameBufferObject();
            //CHECKF(pSurface != null);

            //ghFrameBuffer = CreateVideoSurfaceFromDDSurface(pSurface);
            //CHECKF(ghFrameBuffer != null);

            return true;
        }

        private void DeletePrimaryVideoSurfaces()
        {
            //
            // If globals are not null, delete them
            //

            if (this.ghPrimary != null)
            {
                this.DeleteVideoSurface(this.ghPrimary);
                this.ghPrimary = null;
            }

            if (this.ghBackBuffer != null)
            {
                this.DeleteVideoSurface(this.ghBackBuffer);
                this.ghBackBuffer = null;
            }

            if (this.ghFrameBuffer != null)
            {
                this.DeleteVideoSurface(this.ghFrameBuffer);
                this.ghFrameBuffer = null;
            }

            if (this.ghMouseBuffer != null)
            {
                this.DeleteVideoSurface(this.ghMouseBuffer);
                this.ghMouseBuffer = null;
            }
        }

        // Deletes all palettes, surfaces and region data
        public bool DeleteVideoSurface(HVSURFACE? hVSurface)
        {
            //            Surface2 lpDDSurface;

            // Assertions
            //CHECKF(hVSurface != null);

            // Release palette
            //      if (hVSurface.pPalette != null)
            //      {
            //          DDReleasePalette((LPDIRECTDRAWPALETTE)hVSurface.pPalette);
            //          hVSurface.pPalette = null;
            //      }

            //if ( hVSurface.pClipper != null )
            //{
            // Release Clipper
            //	DDReleaseClipper( (LPDIRECTDRAWCLIPPER)hVSurface.pClipper );
            //}

            // Get surface pointer
            //     lpDDSurface = (Surface2)hVSurface.pSurfaceData;

            // Release surface
            //    if (hVSurface.pSurfaceData1 != null)
            //    {
            //        DDReleaseSurface((Surface1)hVSurface.pSurfaceData1, lpDDSurface);
            //    }

            // Release backup surface
            //  if (hVSurface.pSavedSurfaceData != null)
            //  {
            //      DDReleaseSurface((Surface1)hVSurface.pSavedSurfaceData1,
            //                              (Surface2)hVSurface.pSavedSurfaceData);
            //  }

            // Release region data
            hVSurface.Value.RegionList.Clear();

            //If there is a 16bpp palette, free it
            if (hVSurface.Value.p16BPPPalette != null)
            {
                // MemFree(hVSurface.p16BPPPalette);
                //   hVSurface?.p16BPPPalette = null;
            }

            //giMemUsedInSurfaces -= hVSurface.usHeight * hVSurface.usWidth * (hVSurface.ubBitDepth / 8);

            // Release object
            //MemFree(hVSurface);

            return true;
        }

        public async ValueTask<HVSURFACE?> CreateVideoSurface(VSURFACE_DESC VSurfaceDesc, IFileManager fileManager)
        {
            //LPDIRECTDRAW2 lpDD2Object;
            //DDSURFACEDESC SurfaceDescription;
            Image<Rgba32> SurfaceDescription;
            //DDPIXELFORMAT PixelFormat;
            //LPDIRECTDRAWSURFACE lpDDS;
            //LPDIRECTDRAWSURFACE2 lpDDS2;
            HVSURFACE hVSurface;
            HIMAGE hImage = new();
            Rectangle tempRect = new();
            int  usHeight;
            int  usWidth;
            int ubBitDepth;
            VSurfaceCreateFlags fMemUsage;

            int uiRBitMask;
            int uiGBitMask;
            int uiBBitMask;
            
            //
            // Get Direct Draw Object
            //
            //lpDD2Object = GetDirectDraw2Object();

            //
            // The description structure contains memory usage flag
            //
            fMemUsage = VSurfaceDesc.fCreateFlags;

            //
            // Check creation options
            //

            do
            {
                //
                // Check if creating from file
                //

                if (VSurfaceDesc.fCreateFlags.HasFlag(VSurfaceCreateFlags.VSURFACE_CREATE_FROMFILE))
                {
                    //
                    // Create himage object from file
                    //

                    var tmpHIMAGE = await HIMAGE.CreateImage(VSurfaceDesc.ImageFile, HIMAGECreateFlags.IMAGE_ALLIMAGEDATA, fileManager);

                    if (tmpHIMAGE == null)
                    {
                        // DbgMessage(TOPIC_VIDEOSURFACE, DBG_LEVEL_2, "Invalid Image Filename given");
                        return null;
                    }

                    hImage = tmpHIMAGE.Value;

                    //
                    // Set values from himage
                    //
                    usHeight = hImage.usHeight;
                    usWidth = hImage.usWidth;
                    ubBitDepth = hImage.ubBitDepth;
                    break;
                }

                //
                // If here, no special options given,
                // Set values from given description structure
                //

                usHeight = VSurfaceDesc.usHeight;
                usWidth = VSurfaceDesc.usWidth;
                ubBitDepth = VSurfaceDesc.ubBitDepth;

            } while (false);

            //
            // Assertions
            //

            // Assert(usHeight > 0);
            // Assert(usWidth > 0);

            //
            // Setup Direct Draw Description
            // First do Pixel Format
            //

            //memset(&PixelFormat, 0, sizeof(PixelFormat));
            //PixelFormat.dwSize = sizeof(DDPIXELFORMAT);

            switch (ubBitDepth)
            {

                case 8:

                    //PixelFormat.dwFlags = DDPF_RGB | DDPF_PALETTEINDEXED8;
                    //PixelFormat.dwRGBBitCount = 8;
                    break;

                case 16:

                    // PixelFormat.dwFlags = DDPF_RGB;
                    // PixelFormat.dwRGBBitCount = 16;

                    //
                    // Get current Pixel Format from DirectDraw
                    //

                    // We're using pixel formats too -- DB/Wiz

                    // CHECKF(GetPrimaryRGBDistributionMasks(&uiRBitMask, &uiGBitMask, &uiBBitMask));
                    // PixelFormat.dwRBitMask = uiRBitMask;
                    // PixelFormat.dwGBitMask = uiGBitMask;
                    // PixelFormat.dwBBitMask = uiBBitMask;
                    break;

                default:

                    //
                    // If Here, an invalid format was given
                    //

                    // DbgMessage(TOPIC_VIDEOSURFACE, DBG_LEVEL_2, "Invalid BPP value, can only be 8 or 16.");
                    return null;
            }

            //SurfaceDescription.dwFlags = DDSD_CAPS | DDSD_HEIGHT | DDSD_WIDTH | DDSD_PIXELFORMAT;

            //
            // Do memory description, based on specified flags
            //

            // do
            // {
                // if (fMemUsage & VSURFACE_DEFAULT_MEM_USAGE)
                // {
                //     SurfaceDescription.ddsCaps.dwCaps = DDSCAPS_OFFSCREENPLAIN;
                //     break;
                // }
                // if (fMemUsage & VSURFACE_VIDEO_MEM_USAGE)
                // {
                //     SurfaceDescription.ddsCaps.dwCaps = DDSCAPS_OFFSCREENPLAIN;
                //     break;
                // }
                // 
                // if (fMemUsage & VSURFACE_SYSTEM_MEM_USAGE)
                // {
                //     SurfaceDescription.ddsCaps.dwCaps = DDSCAPS_OFFSCREENPLAIN | DDSCAPS_SYSTEMMEMORY;
                //     break;
                // }

                //
                // Once here, no mem flags were given, use default
                //

                // SurfaceDescription.ddsCaps.dwCaps = DDSCAPS_OFFSCREENPLAIN;

            // } while (false);

            //
            // Set other, common structure elements
            //

            // SurfaceDescription.dwSize = sizeof(DDSURFACEDESC);
            // SurfaceDescription.dwWidth = usWidth;
            // SurfaceDescription.dwHeight = usHeight;
            // SurfaceDescription.ddpfPixelFormat = PixelFormat;

            //
            // Create Surface
            //

            //DDCreateSurface(lpDD2Object, &SurfaceDescription, &lpDDS, &lpDDS2);

            //
            // Allocate memory for Video Surface data and initialize
            //

            //hVSurface = MemAlloc(sizeof(SGPVSurface));
            //memset(hVSurface, 0, sizeof(SGPVSurface));
            //CHECKF(hVSurface != null);

            hVSurface.usHeight = usHeight;
            hVSurface.usWidth = usWidth;
            hVSurface.ubBitDepth = ubBitDepth;
            hVSurface.pSurfaceData1 = null;// (PTR)lpDDS;
            hVSurface.pSurfaceData = null;//(PTR)lpDDS2;
            hVSurface.pSavedSurfaceData1 = null;
            hVSurface.pSavedSurfaceData = null;
            hVSurface.pPalette = null;
            hVSurface.p16BPPPalette = null;
            hVSurface.TransparentColor = 0;// FROMRGB(0, 0, 0);
            hVSurface.RegionList = null;// CreateList(DEFAULT_NUM_REGIONS, sizeof(VSURFACE_REGION));
            hVSurface.fFlags = 0;
            hVSurface.pClipper = null;

            //
            // Determine memory and other attributes of newly created surface
            //

            //DDGetSurfaceDescription(lpDDS2, &SurfaceDescription);

            //
            // Fail if create tried for video but it's in system
            //

            if (VSurfaceDesc.fCreateFlags.HasFlag(VSurfaceCreateFlags.VSURFACE_VIDEO_MEM_USAGE))// && SurfaceDescription.ddsCaps.dwCaps & DDSCAPS_SYSTEMMEMORY)
            {
                //
                // Return failure due to not in video
                //

              //  DbgMessage(TOPIC_VIDEOSURFACE, DBG_LEVEL_2, String("Failed to create Video Surface in video memory"));
              //  DDReleaseSurface(&lpDDS, &lpDDS2);
              //  MemFree(hVSurface);
                return null;
            }

            //
            // Look for system memory
            //

            //if (SurfaceDescription.ddsCaps.dwCaps & DDSCAPS_SYSTEMMEMORY)
            //{
            //    hVSurface.fFlags |= VSURFACE_SYSTEM_MEM_USAGE;
            //}

            //
            // Look for video memory
            //

            //if (SurfaceDescription.ddsCaps.dwCaps & DDSCAPS_VIDEOMEMORY)
            //{
            //    hVSurface.fFlags |= VSURFACE_VIDEO_MEM_USAGE;
            //}

            //
            // If in video memory, create backup surface
            //

            SurfaceDescription = new Image<Rgba32>(usWidth, usHeight, new Rgba32(0, 0, 0));

            // if (hVSurface.fFlags & VSURFACE_VIDEO_MEM_USAGE)
            // {
            //     SurfaceDescription.dwFlags = DDSD_CAPS | DDSD_HEIGHT | DDSD_WIDTH | DDSD_PIXELFORMAT;
            //     SurfaceDescription.ddsCaps.dwCaps = DDSCAPS_OFFSCREENPLAIN | DDSCAPS_SYSTEMMEMORY;
            //     SurfaceDescription.dwSize = sizeof(DDSURFACEDESC);
            //     SurfaceDescription.dwWidth = usWidth;
            //     SurfaceDescription.dwHeight = usHeight;
            //     SurfaceDescription.ddpfPixelFormat = PixelFormat;
            // 
            //     //
            //     // Create Surface
            //     //
            // 
            //     DDCreateSurface(lpDD2Object, &SurfaceDescription, &lpDDS, &lpDDS2);
            // 
            //     //
            //     // Save surface to backup
            //     //
            // 
            //     hVSurface.pSavedSurfaceData1 = lpDDS;
            //     hVSurface.pSavedSurfaceData = lpDDS2;
            // }

            //
            // Initialize surface with hImage , if given
            //

            if (VSurfaceDesc.fCreateFlags.HasFlag(VSurfaceCreateFlags.VSURFACE_CREATE_FROMFILE))
            {
                tempRect.X = 0;
                tempRect.Y = 0;
                tempRect.Width = hImage.usWidth - 1;
                tempRect.Height = hImage.usHeight - 1;
                //SetVideoSurfaceDataFromHImage(hVSurface, hImage, 0, 0, ref tempRect);

                //
                // Set palette from himage
                //

                if (hImage.ubBitDepth == 8)
                {
                  //  SetVideoSurfacePalette(hVSurface, hImage.pPalette);
                }

                //
                // Delete himage object
                //

                //DestroyImage(hImage);
            }

            //
            // All is well
            //

            hVSurface.usHeight = usHeight;
            hVSurface.usWidth = usWidth;
            hVSurface.ubBitDepth = ubBitDepth;

            giMemUsedInSurfaces += hVSurface.usHeight * hVSurface.usWidth * (hVSurface.ubBitDepth / 8);

            //DbgMessage(TOPIC_VIDEOSURFACE, DBG_LEVEL_3, String("Success in Creating Video Surface"));

            return hVSurface;
        }

        public void Dispose()
        {
        }

        public byte[] LockVideoSurface(uint fRAME_BUFFER, out int uiDestPitchBYTES)
        {
            uiDestPitchBYTES = 0;
            return Array.Empty<byte>();
        }

        public void UnLockVideoSurface(uint fRAME_BUFFER)
        {
        }
    }
}
