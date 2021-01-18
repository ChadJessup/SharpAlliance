using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SharpAlliance.Core.Interfaces;
using SharpAlliance.Core.Managers.VideoSurfaces;
using SharpAlliance.Platform;
using SharpAlliance.Platform.Interfaces;

namespace SharpAlliance
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
            gpVSurfaceHead = gpVSurfaceTail = null;

            giMemUsedInSurfaces = 0;

            // Create primary and backbuffer from globals
            if (!SetPrimaryVideoSurfaces())
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
            DeletePrimaryVideoSurfaces();

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

            if (ghPrimary != null)
            {
                DeleteVideoSurface(ghPrimary);
                ghPrimary = null;
            }

            if (ghBackBuffer != null)
            {
                DeleteVideoSurface(ghBackBuffer);
                ghBackBuffer = null;
            }

            if (ghFrameBuffer != null)
            {
                DeleteVideoSurface(ghFrameBuffer);
                ghFrameBuffer = null;
            }

            if (ghMouseBuffer != null)
            {
                DeleteVideoSurface(ghMouseBuffer);
                ghMouseBuffer = null;
            }
        }

        // Deletes all palettes, surfaces and region data
        private bool DeleteVideoSurface(HVSURFACE? hVSurface)
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

        public void Dispose()
        {
        }
    }
}
