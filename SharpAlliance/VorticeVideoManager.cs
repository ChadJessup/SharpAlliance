using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SharpAlliance.Core.Managers;
using SharpAlliance.Core.Managers.Image;
using SharpAlliance.Core.Screens;
using SharpAlliance.Core.SubSystems;
using SharpAlliance.Platform;
using SharpAlliance.Platform.Interfaces;
using Vortice;
using Vortice.Mathematics;
using Vortice.Win32;

namespace SharpAlliance
{
    public class VorticeVideoManager : IVideoManager
    {
        public static class Constants
        {
            public const int MAX_DIRTY_REGIONS = 128;

            public const int VIDEO_OFF = 0x00;
            public const int VIDEO_ON = 0x01;
            public const int VIDEO_SHUTTING_DOWN = 0x02;
            public const int VIDEO_SUSPENDED = 0x04;

            public const int THREAD_OFF = 0x00;
            public const int THREAD_ON = 0x01;
            public const int THREAD_SUSPENDED = 0x02;

            public const int CURRENT_MOUSE_DATA = 0;
            public const int PREVIOUS_MOUSE_DATA = 1;
            public const int MAX_NUM_FRAMES = 25;
            public const int BUFFER_READY = 0x00;
            public const int BUFFER_BUSY = 0x01;
            public const int BUFFER_DIRTY = 0x02;
            public const int BUFFER_DISABLED = 0x03;
        }

        // TODO: These are temporary...
        const int DD_OK = 0;
        const int DDBLTFAST_NOCOLORKEY = 0x00000000;
        const int DDBLTFAST_SRCCOLORKEY = 0x00000001;
        const int DDBLTFAST_DESTCOLORKEY = 0x00000002;
        const int DDBLTFAST_WAIT = 0x00000010;
        const int DDERR_WASSTILLDRAWING = 0x8700000; // not real value
        const int DDERR_SURFACELOST = 0x9700000;

        private readonly ILogger<VorticeVideoManager> logger;
        private readonly WindowsSubSystem windows;
        private readonly InputManager inputs;
        private readonly MouseSubSystem mouse;
        private readonly RenderWorld renderWorld;
        private readonly ScreenManager screenManager;
        private readonly GameContext context;
        private readonly MouseCursorBackground[] gMouseCursorBackground = new MouseCursorBackground[2];
        
        private FadeScreen fadeScreen;
        private Action gpFrameBufferRefreshOverride;
        
        private IGraphicsDevice graphicsDevice;

        private int gusScreenWidth;
        private int gusScreenHeight;
        private int gubScreenPixelDepth;

        private Rectangle gScrollRegion;

        private bool gfVideoCapture = false;
        private int guiFramePeriod = (1000 / 15);
        private long guiLastFrame;
        private int[] gpFrameData = new int[Constants.MAX_NUM_FRAMES];
        private int giNumFrames = 0;
        private Rectangle rcWindow;

        private int gusMouseCursorWidth;
        private int gusMouseCursorHeight;
        private int gsMouseCursorXOffset;
        private int gsMouseCursorYOffset;
        private bool gfFatalError = false;
        private string gFatalErrorString;

        // 8-bit palette stuff
        private SGPPaletteEntry[] gSgpPalette = new SGPPaletteEntry[256];
        private int guiFrameBufferState;    // BUFFER_READY, BUFFER_DIRTY
        private int guiMouseBufferState;    // BUFFER_READY, BUFFER_DIRTY, BUFFER_DISABLED
        private int guiVideoManagerState;   // VIDEO_ON, VIDEO_OFF, Constants.VIDEO_SUSPENDED, Constants.VIDEO_SHUTTING_DOWN
        private int guiRefreshThreadState;  // Constants.THREAD_ON, Constants.THREAD_OFF, Constants.THREAD_SUSPENDED

        //void (* gpFrameBufferRefreshOverride) (void);
        private Rectangle[] gListOfDirtyRegions = new Rectangle[Constants.MAX_DIRTY_REGIONS];
        private int guiDirtyRegionCount;
        private bool gfForceFullScreenRefresh;

        private Rectangle[] gDirtyRegionsEx = new Rectangle[Constants.MAX_DIRTY_REGIONS];
        private int[] gDirtyRegionsFlagsEx = new int[Constants.MAX_DIRTY_REGIONS];
        private int guiDirtyRegionExCount;

        private Rectangle[] gBACKUPListOfDirtyRegions = new Rectangle[Constants.MAX_DIRTY_REGIONS];
        private int gBACKUPuiDirtyRegionCount;
        private bool gBACKUPfForceFullScreenRefresh;

        private bool gfPrintFrameBuffer;
        private int guiPrintFrameBufferIndex;

        ///////////////////////////////////////////////////////////////////////////////////////////////////
        //
        // External Variables
        //
        ///////////////////////////////////////////////////////////////////////////////////////////////////

        int gusRedMask;
        int gusGreenMask;
        int gusBlueMask;
        int gusRedShift;
        int gusBlueShift;
        int gusGreenShift;
        //
        // Direct Draw objects for both the Primary and Backbuffer surfaces
        //

        // private LPDIRECTDRAW? _gpDirectDrawObject = null;
        // private LPDIRECTDRAW2 gpDirectDrawObject = null;

        //private Surface? _gpPrimarySurface = null;
        //private Surface2? gpPrimarySurface = null;
        //private Surface2? gpBackBuffer = null

        public bool IsInitialized { get; private set; }

        private const int SCREEN_WIDTH = 640;
        private const int SCREEN_HEIGHT = 480;
        private const int PIXEL_DEPTH = 16;

        public VorticeVideoManager(
            ILogger<VorticeVideoManager> logger,
            GameContext context,
            IInputManager inputManager,
            MouseSubSystem mouseSubSystem,
            RenderWorld renderWorld,
            IScreenManager screenManager)
        {
            this.logger = logger;
            this.context = context;
            this.inputs = (inputManager as InputManager)!;
            this.mouse = mouseSubSystem;
            this.renderWorld = renderWorld;
            this.screenManager = (screenManager as ScreenManager)!;
        }

        public async ValueTask<bool> Initialize()
        {
            if (!this.inputs.IsInitialized)
            {
                await this.inputs.Initialize();
            }

            // this.fadeScreen = (screenManager.GetScreen(ScreenNames.FADE_SCREEN, activate: true).AsTask().Result as FadeScreen)!;
            this.IsInitialized = true;
            return this.IsInitialized;
        }

        internal void SetGraphicsDevice(D3D12GraphicsDevice gDevice)
        {
            this.graphicsDevice = gDevice;
        }

        private void HookupInputs(IInputManager inputManager)
        {
            //if (this.form is not null)
            {
                //                this.form.KeyDown += this.KeyboardEvent;
                //                this.form.KeyUp += this.KeyboardEvent;
            }

            //form.mou
        }

        //      private void KeyboardEvent(object? o, KeyEventArgs e)
        //{

        //  }


        //    public Surface2 GetPrimarySurfaceObject()
        //    {
        //        //Assert(gpPrimarySurface != null);
        //
        //        return gpPrimarySurface;
        //    }

        public void Draw()
        {
            this.graphicsDevice.DrawFrame((w, h) =>
            {
            });
        }

        public void Dispose()
        {
            // Unhook events...
            //if (this.form is not null)
            {
                //      this.form.KeyUp -= this.KeyboardEvent;
                //      this.form.KeyDown -= this.KeyboardEvent;
            }

            GC.SuppressFinalize(this);
        }

        static int uiRefreshThreadState, uiIndex;
        static bool fShowMouse;
        static Rectangle Region;
        static Point MousePos;
        static bool fFirstTime = true;

        public void RefreshScreen(object p)
        {
            int usScreenWidth, usScreenHeight;
            int ReturnCode;
            long uiTime;

            usScreenWidth = usScreenHeight = 0;

            if (fFirstTime)
            {
                fShowMouse = false;
            }


            //DebugMsg(TOPIC_VIDEO, DBG_LEVEL_0, "Looping in refresh");

            ///////////////////////////////////////////////////////////////////////////////////////////////
            // 
            // REFRESH_THREAD_MUTEX 
            //
            ///////////////////////////////////////////////////////////////////////////////////////////////

            switch (guiVideoManagerState)
            {
                case Constants.VIDEO_ON
              : //
                // Excellent, everything is cosher, we continue on
                //
                    uiRefreshThreadState = guiRefreshThreadState = Constants.THREAD_ON;
                    usScreenWidth = gusScreenWidth;
                    usScreenHeight = gusScreenHeight;
                    break;
                case Constants.VIDEO_OFF
              : //
                // Hot damn, the video manager is suddenly off. We have to bugger out of here. Don't forget to
                // leave the critical section
                //
                    guiRefreshThreadState = Constants.THREAD_OFF;
                    return;
                case Constants.VIDEO_SUSPENDED
              : //
                // This are suspended. Make sure the refresh function does try to access any of the direct
                // draw surfaces
                //
                    uiRefreshThreadState = guiRefreshThreadState = Constants.THREAD_SUSPENDED;
                    break;
                case Constants.VIDEO_SHUTTING_DOWN
              : //
                // Well things are shutting down. So we need to bugger out of there. Don't forget to leave the
                // critical section before returning
                //
                    guiRefreshThreadState = Constants.THREAD_OFF;
                    return;
            }


            //
            // Get the current mouse position
            //

            this.inputs.GetCursorPosition(out var tmpMousePos);
            MousePos = new Point(tmpMousePos.X, tmpMousePos.Y);

            /////////////////////////////////////////////////////////////////////////////////////////////
            // 
            // FRAME_BUFFER_MUTEX 
            //
            /////////////////////////////////////////////////////////////////////////////////////////////


            // RESTORE OLD POSITION OF MOUSE
            if (gMouseCursorBackground[Constants.CURRENT_MOUSE_DATA].fRestore == true)
            {
                Region.Left = gMouseCursorBackground[Constants.CURRENT_MOUSE_DATA].usLeft;
                Region.Top = gMouseCursorBackground[Constants.CURRENT_MOUSE_DATA].usTop;
                Region.Width = gMouseCursorBackground[Constants.CURRENT_MOUSE_DATA].usRight;
                Region.Height = gMouseCursorBackground[Constants.CURRENT_MOUSE_DATA].usBottom;

                do
                {
                    ReturnCode = 0;
                    //// ReturnCode = IDirectDrawSurface2_SGPBltFast(gpBackBuffer, gMouseCursorBackground[Constants.CURRENT_MOUSE_DATA].usMouseXPos, gMouseCursorBackground[Constants.CURRENT_MOUSE_DATA].usMouseYPos, gMouseCursorBackground[Constants.CURRENT_MOUSE_DATA].pSurface, ref Region, DDBLTFAST_NOCOLORKEY);
                    if ((ReturnCode != DD_OK) && (ReturnCode != DDERR_WASSTILLDRAWING))
                    {
                        // DirectXAttempt(ReturnCode, __LINE__, __FILE__);

                        if (ReturnCode == DDERR_SURFACELOST)
                        {
                            goto ENDOFLOOP;
                        }
                    }
                } while (ReturnCode != DD_OK);

                // Save position into other background region
                //memcpy(&(gMouseCursorBackground[Constants.PREVIOUS_MOUSE_DATA]), &(gMouseCursorBackground[Constants.CURRENT_MOUSE_DATA]), sizeof(MouseCursorBackground));
            }


            //
            // Ok we were able to get a hold of the frame buffer stuff. Check to see if it needs updating
            // if not, release the frame buffer stuff right away
            //
            if (guiFrameBufferState == Constants.BUFFER_DIRTY)
            {

                // Well the frame buffer is dirty.
                //

                if (gpFrameBufferRefreshOverride != null)
                {
                    //
                    // Method (3) - We are using a function override to refresh the frame buffer. First we
                    // call the override function then we must set the override pointer to null
                    //

                    gpFrameBufferRefreshOverride();
                    gpFrameBufferRefreshOverride = null;

                }


                if (this.fadeScreen.gfFadeInitialized && this.fadeScreen.gfFadeInVideo)
                {
                    this.fadeScreen.gFadeFunction();
                }
                else
                //
                // Either Method (1) or (2)
                //
                {
                    if (gfForceFullScreenRefresh == true)
                    {
                        //
                        // Method (1) - We will be refreshing the entire screen
                        //

                        Region.Left = 0;
                        Region.Top = 0;
                        Region.Width = usScreenWidth;
                        Region.Height = usScreenHeight;

                        do
                        {
                            ReturnCode = DD_OK;
                            // ReturnCode = IDirectDrawSurface2_SGPBltFast(gpBackBuffer, 0, 0, gpFrameBuffer, ref Region, DDBLTFAST_NOCOLORKEY);
                            if ((ReturnCode != DD_OK) && (ReturnCode != DDERR_WASSTILLDRAWING))
                            {
                                //                                // DirectXAttempt(ReturnCode, __LINE__, __FILE__);

                                if (ReturnCode == DDERR_SURFACELOST)
                                {
                                    goto ENDOFLOOP;
                                }
                            }
                        } while (ReturnCode != DD_OK);


                    }
                    else
                    {
                        for (uiIndex = 0; uiIndex < guiDirtyRegionCount; uiIndex++)
                        {
                            Region.Left = gListOfDirtyRegions[uiIndex].Left;
                            Region.Top = gListOfDirtyRegions[uiIndex].Top;
                            Region.Width = gListOfDirtyRegions[uiIndex].Width;
                            Region.Height = gListOfDirtyRegions[uiIndex].Height;

                            do
                            {
                                ReturnCode = DD_OK;
                                // ReturnCode = IDirectDrawSurface2_SGPBltFast(gpBackBuffer, Region.Left, Region.Top, gpFrameBuffer, ref Region, DDBLTFAST_NOCOLORKEY);
                                if ((ReturnCode != DD_OK) && (ReturnCode != DDERR_WASSTILLDRAWING))
                                {
                                    // DirectXAttempt(ReturnCode, __LINE__, __FILE__);
                                }

                                if (ReturnCode == DDERR_SURFACELOST)
                                {
                                    goto ENDOFLOOP;
                                }
                            } while (ReturnCode != DD_OK);

                        }

                        // Now do new, extended dirty regions
                        for (uiIndex = 0; uiIndex < guiDirtyRegionExCount; uiIndex++)
                        {
                            Region.Left = gDirtyRegionsEx[uiIndex].Left;
                            Region.Top = gDirtyRegionsEx[uiIndex].Top;
                            Region.Width = gDirtyRegionsEx[uiIndex].Width;
                            Region.Height = gDirtyRegionsEx[uiIndex].Height;

                            // Do some checks if we are in the process of scrolling!	
                            if (this.renderWorld.gfRenderScroll)
                            {

                                // Check if we are completely out of bounds
                                if (Region.Top <= this.renderWorld.gsVIEWPORT_WINDOW_END_Y && Region.Height <= this.renderWorld.gsVIEWPORT_WINDOW_END_Y)
                                {
                                    continue;
                                }

                            }

                            do
                            {
                                ReturnCode = DD_OK;
                                // ReturnCode = IDirectDrawSurface2_SGPBltFast(gpBackBuffer, Region.Left, Region.Top, gpFrameBuffer, ref Region, DDBLTFAST_NOCOLORKEY);
                                //if ((ReturnCode != DD_OK) && (ReturnCode != DDERR_WASSTILLDRAWING))
                                //{
                                //    // DirectXAttempt(ReturnCode, __LINE__, __FILE__);
                                //}

                                if (ReturnCode == DDERR_SURFACELOST)
                                {
                                    goto ENDOFLOOP;
                                }
                            } while (ReturnCode != DD_OK);

                        }

                    }

                }

                //if (gfRenderScroll)
                //{
                //    ScrollJA2Background(guiScrollDirection, gsScrollXIncrement, gsScrollYIncrement, gpPrimarySurface, gpBackBuffer, true, Constants.PREVIOUS_MOUSE_DATA);
                //}
                //gfIgnoreScrollDueToCenterAdjust = false;




                //
                // Update the guiFrameBufferState variable to reflect that the frame buffer can now be handled
                //

                guiFrameBufferState = Constants.BUFFER_READY;
            }

            //
            // Do we want to print the frame stuff ??
            //

            if (gfVideoCapture)
            {
                uiTime = this.context.ClockManager.GetTickCount();
                if ((uiTime < guiLastFrame) || (uiTime > (guiLastFrame + guiFramePeriod)))
                {
                    //SnapshotSmall();
                    guiLastFrame = uiTime;
                }
            }


            if (gfPrintFrameBuffer == true)
            {
                //LPDIRECTDRAWSURFACE _pTmpBuffer;
                //LPDIRECTDRAWSURFACE2 pTmpBuffer;
                //DDSURFACEDESC SurfaceDescription;
                FileStream OutputFile;
                string? FileName;
                int iIndex;
                string ExecDir;
                int[] p16BPPData;

                //GetExecutableDirectory(ExecDir);
                //SetFileManCurrentDirectory(ExecDir);

                //
                // Create temporary system memory surface. This is used to correct problems with the backbuffer
                // surface which can be interlaced or have a funky pitch
                //

                //// ZEROMEM(SurfaceDescription);
                //SurfaceDescription.dwSize = sizeof(DDSURFACEDESC);
                //SurfaceDescription.dwFlags = DDSD_CAPS | DDSD_WIDTH | DDSD_HEIGHT;
                //SurfaceDescription.ddsCaps.dwCaps = DDSCAPS_OFFSCREENPLAIN | DDSCAPS_SYSTEMMEMORY;
                //SurfaceDescription.dwWidth = usScreenWidth;
                //SurfaceDescription.dwHeight = usScreenHeight;
                ReturnCode = DD_OK;
                //ReturnCode = IDirectDraw2_CreateSurface(gpDirectDrawObject, &SurfaceDescription, &_pTmpBuffer, null);
                if ((ReturnCode != DD_OK) && (ReturnCode != DDERR_WASSTILLDRAWING))
                {
                    // DirectXAttempt(ReturnCode, __LINE__, __FILE__);
                }

                // ReturnCode = IDirectDrawSurface_QueryInterface(_pTmpBuffer, &IID_IDirectDrawSurface2, &pTmpBuffer);
                if ((ReturnCode != DD_OK) && (ReturnCode != DDERR_WASSTILLDRAWING))
                {
                    // DirectXAttempt(ReturnCode, __LINE__, __FILE__);
                }

                //
                // Copy the primary surface to the temporary surface
                //

                Region.Left = 0;
                Region.Top = 0;
                Region.Width = usScreenWidth;
                Region.Height = usScreenHeight;

                do
                {
                    //// ReturnCode = IDirectDrawSurface2_SGPBltFast(pTmpBuffer, 0, 0, gpPrimarySurface, ref Region, DDBLTFAST_NOCOLORKEY);
                    if ((ReturnCode != DD_OK) && (ReturnCode != DDERR_WASSTILLDRAWING))
                    {
                        // DirectXAttempt(ReturnCode, __LINE__, __FILE__);
                    }
                } while (ReturnCode != DD_OK);

                //
                // Ok now that temp surface has contents of backbuffer, copy temp surface to disk
                //

                //sprintf(FileName, "SCREEN%03d.TGA", guiPrintFrameBufferIndex++);
                //if ((OutputFile = fopen(FileName, "wb")) != null)
                //{
                //    //fprintf(OutputFile, "%c%c%c%c%c%c%c%c%c%c%c%c%c%c%c%c%c%c", 0, 0, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0x80, 0x02, 0xe0, 0x01, 0x10, 0);
                //
                //    //
                //    // Lock temp surface
                //    //
                //
                //    // ZEROMEM(SurfaceDescription);
                //    SurfaceDescription.dwSize = sizeof(DDSURFACEDESC);
                //    ReturnCode = IDirectDrawSurface2_Lock(pTmpBuffer, null, &SurfaceDescription, 0, null);
                //    if ((ReturnCode != DD_OK) && (ReturnCode != DDERR_WASSTILLDRAWING))
                //    {
                //        // DirectXAttempt(ReturnCode, __LINE__, __FILE__);
                //    }
                //
                //    //
                //    // Copy 16 bit buffer to file
                //    //
                //
                //    // 5/6/5.. create buffer...
                //    if (gusRedMask == 0xF800 && gusGreenMask == 0x07E0 && gusBlueMask == 0x001F)
                //    {
                //        p16BPPData = MemAlloc(640 * 2);
                //    }
                //
                //    for (iIndex = 479; iIndex >= 0; iIndex--)
                //    {
                //        // ATE: OK, fix this such that it converts pixel format to 5/5/5
                //        // if current settings are 5/6/5....
                //        if (gusRedMask == 0xF800 && gusGreenMask == 0x07E0 && gusBlueMask == 0x001F)
                //        {
                //            // Read into a buffer...
                //            //memcpy(p16BPPData, (((int*SurfaceDescription.lpSurface) + (iIndex * 640 * 2)), 640 * 2);
                //
                //            // Convert....
                //            ConvertRGBDistribution565To555(p16BPPData, 640);
                //
                //            // Write
                //            //fwrite(p16BPPData, 640 * 2, 1, OutputFile);
                //        }
                //        else
                //        {
                //            //fwrite((void*)(((int*)SurfaceDescription.lpSurface) + (iIndex * 640 * 2)), 640 * 2, 1, OutputFile);
                //        }
                //    }
                //
                //    // 5/6/5.. Delete buffer...
                //    if (gusRedMask == 0xF800 && gusGreenMask == 0x07E0 && gusBlueMask == 0x001F)
                //    {
                //        // MemFree(p16BPPData);
                //    }
                //
                //    //fclose(OutputFile);
                //
                //    //
                //    // Unlock temp surface
                //    //
                //
                //    // ZEROMEM(SurfaceDescription);
                //    //SurfaceDescription.dwSize = sizeof(DDSURFACEDESC);
                //    //ReturnCode = IDirectDrawSurface2_Unlock(pTmpBuffer, &SurfaceDescription);
                //    if ((ReturnCode != DD_OK) && (ReturnCode != DDERR_WASSTILLDRAWING))
                //    {
                //        // DirectXAttempt(ReturnCode, __LINE__, __FILE__);
                //    }
                //}

                //
                // Release temp surface
                //

                gfPrintFrameBuffer = false;
                //IDirectDrawSurface2_Release(pTmpBuffer);

                //strcat(ExecDir, "\\Data");
                //SetFileManCurrentDirectory(ExecDir);
            }

            //
            // Ok we were able to get a hold of the frame buffer stuff. Check to see if it needs updating
            // if not, release the frame buffer stuff right away
            //

            if (guiMouseBufferState == Constants.BUFFER_DIRTY)
            {
                //
                // Well the mouse buffer is dirty. Upload the whole thing
                //

                Region.Left = 0;
                Region.Top = 0;
                Region.Width = gusMouseCursorWidth;
                Region.Height = gusMouseCursorHeight;

                do
                {
                    ReturnCode = DD_OK;
                    // ReturnCode = IDirectDrawSurface2_SGPBltFast(gpMouseCursor, 0, 0, gpMouseCursorOriginal, ref Region, DDBLTFAST_NOCOLORKEY);
                    if ((ReturnCode != DD_OK) && (ReturnCode != DDERR_WASSTILLDRAWING))
                    {
                        // DirectXAttempt(ReturnCode, __LINE__, __FILE__);
                    }
                } while (ReturnCode != DD_OK);

                guiMouseBufferState = Constants.BUFFER_READY;
            }

            //
            // Check current state of the mouse cursor
            //

            if (fShowMouse == false)
            {
                if (guiMouseBufferState == Constants.BUFFER_READY)
                {
                    fShowMouse = true;
                }
                else
                {
                    fShowMouse = false;
                }
            }
            else
            {
                if (guiMouseBufferState == Constants.BUFFER_DISABLED)
                {
                    fShowMouse = false;
                }
            }

            ///////////////////////////////////////////////////////////////////////////////////////////////
            // 
            // End of MOUSE_BUFFER_MUTEX
            //
            ///////////////////////////////////////////////////////////////////////////////////////////////


            ///////////////////////////////////////////////////////////////////////////////////////////////
            // 
            // If fMouseState == true
            //
            // (1) Save mouse background from gpBackBuffer to gpMouseCursorBackground
            // (2) If step (1) is successfull blit mouse cursor onto gpBackBuffer
            //
            ///////////////////////////////////////////////////////////////////////////////////////////////

            if (fShowMouse == true)
            {
                //
                // Step (1) - Save mouse background
                //                      

                Region.Left = MousePos.X - gsMouseCursorXOffset;
                Region.Top = MousePos.Y - gsMouseCursorYOffset;
                Region.Width = Region.Left + gusMouseCursorWidth;
                Region.Height = Region.Top + gusMouseCursorHeight;

                if (Region.Width > usScreenWidth)
                {
                    Region.Width = usScreenWidth;
                }

                if (Region.Height > usScreenHeight)
                {
                    Region.Height = usScreenHeight;
                }

                if ((Region.Width > Region.Left) && (Region.Height > Region.Top))
                {
                    //
                    // Make sure the mouse background is marked for restore and coordinates are saved for the
                    // future restore
                    //

                    gMouseCursorBackground[Constants.CURRENT_MOUSE_DATA].fRestore = true;
                    gMouseCursorBackground[Constants.CURRENT_MOUSE_DATA].usRight = (int)Region.Width - (int)Region.Left;
                    gMouseCursorBackground[Constants.CURRENT_MOUSE_DATA].usBottom = (int)Region.Height - (int)Region.Top;
                    if (Region.Left < 0)
                    {
                        gMouseCursorBackground[Constants.CURRENT_MOUSE_DATA].usLeft = (int)(0 - Region.Left);
                        gMouseCursorBackground[Constants.CURRENT_MOUSE_DATA].usMouseXPos = 0;
                        Region.Left = 0;
                    }
                    else
                    {
                        gMouseCursorBackground[Constants.CURRENT_MOUSE_DATA].usMouseXPos = (int)MousePos.X - gsMouseCursorXOffset;
                        gMouseCursorBackground[Constants.CURRENT_MOUSE_DATA].usLeft = 0;
                    }
                    if (Region.Top < 0)
                    {
                        gMouseCursorBackground[Constants.CURRENT_MOUSE_DATA].usMouseYPos = 0;
                        gMouseCursorBackground[Constants.CURRENT_MOUSE_DATA].usTop = (int)(0 - Region.Top);
                        Region.Top = 0;
                    }
                    else
                    {
                        gMouseCursorBackground[Constants.CURRENT_MOUSE_DATA].usMouseYPos = (int)MousePos.Y - gsMouseCursorYOffset;
                        gMouseCursorBackground[Constants.CURRENT_MOUSE_DATA].usTop = 0;
                    }

                    if ((Region.Width > Region.Left) && (Region.Height > Region.Top))
                    {
                        // Save clipped region
                        gMouseCursorBackground[Constants.CURRENT_MOUSE_DATA].Region = Region;

                        //
                        // Ok, do the actual data save to the mouse background
                        //

                        do
                        {
                            ReturnCode = DD_OK;
                            // ReturnCode = IDirectDrawSurface2_SGPBltFast(gMouseCursorBackground[Constants.CURRENT_MOUSE_DATA].pSurface, gMouseCursorBackground[Constants.CURRENT_MOUSE_DATA].usLeft, gMouseCursorBackground[Constants.CURRENT_MOUSE_DATA].usTop, gpBackBuffer, &Region, DDBLTFAST_NOCOLORKEY);
                            if ((ReturnCode != DD_OK) && (ReturnCode != DDERR_WASSTILLDRAWING))
                            {
                                // DirectXAttempt(ReturnCode, __LINE__, __FILE__);
                            }

                            if (ReturnCode == DDERR_SURFACELOST)
                            {
                                goto ENDOFLOOP;
                            }
                        } while (ReturnCode != DD_OK);

                        //
                        // Step (2) - Blit mouse cursor to back buffer
                        //

                        Region.Left = gMouseCursorBackground[Constants.CURRENT_MOUSE_DATA].usLeft;
                        Region.Top = gMouseCursorBackground[Constants.CURRENT_MOUSE_DATA].usTop;
                        Region.Width = gMouseCursorBackground[Constants.CURRENT_MOUSE_DATA].usRight;
                        Region.Height = gMouseCursorBackground[Constants.CURRENT_MOUSE_DATA].usBottom;

                        do
                        {
                            // ReturnCode = IDirectDrawSurface2_SGPBltFast(gpBackBuffer, gMouseCursorBackground[Constants.CURRENT_MOUSE_DATA].usMouseXPos, gMouseCursorBackground[Constants.CURRENT_MOUSE_DATA].usMouseYPos, gpMouseCursor, &Region, DDBLTFAST_SRCCOLORKEY);
                            if ((ReturnCode != DD_OK) && (ReturnCode != DDERR_WASSTILLDRAWING))
                            {
                                // DirectXAttempt(ReturnCode, __LINE__, __FILE__);
                            }

                            if (ReturnCode == DDERR_SURFACELOST)
                            {
                                goto ENDOFLOOP;
                            }
                        } while (ReturnCode != DD_OK);
                    }
                    else
                    {
                        //
                        // Hum, the mouse was not blitted this round. Henceforth we will flag fRestore as false
                        //

                        gMouseCursorBackground[Constants.CURRENT_MOUSE_DATA].fRestore = false;
                    }

                }
                else
                {
                    //
                    // Hum, the mouse was not blitted this round. Henceforth we will flag fRestore as false
                    //

                    gMouseCursorBackground[Constants.CURRENT_MOUSE_DATA].fRestore = false;

                }
            }
            else
            {
                //
                // Well since there was no mouse handling this round, we disable the mouse restore
                //        

                gMouseCursorBackground[Constants.CURRENT_MOUSE_DATA].fRestore = false;

            }

            ///////////////////////////////////////////////////////////////////////////////////////////////
            // 
            // (1) Flip Pages
            // (2) If the page flipping worked, then we copy the contents of the primary surface back
            //     to the backbuffer
            // (3) If step (2) was successfull we then restore the mouse background onto the backbuffer
            //     if fShowMouse is true
            //
            ///////////////////////////////////////////////////////////////////////////////////////////////

            //
            // Step (1) - Flip pages
            //
            //# ifdef WINDOWED_MODE

            do
            {

                //                ReturnCode = IDirectDrawSurface_Blt(
                //                              gpPrimarySurface,          // dest surface
                //                              &rcWindow,              // dest rect
                //                              gpBackBuffer,           // src surface
                //                              null,                   // src rect (all of it)
                //                              DDBLT_WAIT,
                //                              null);
                ReturnCode = DD_OK;
                if ((ReturnCode != DD_OK) && (ReturnCode != DDERR_WASSTILLDRAWING))
                {
                    // DirectXAttempt(ReturnCode, __LINE__, __FILE__);

                    if (ReturnCode == DDERR_SURFACELOST)
                    {
                        goto ENDOFLOOP;
                    }
                }

            } while (ReturnCode != DD_OK);


            //#else
            //
            //            do
            //            {
            //                ReturnCode = IDirectDrawSurface_Flip(_gpPrimarySurface, null, DDFLIP_WAIT);
            //                //    if ((ReturnCode != DD_OK)&&(ReturnCode != DDERR_WASSTILLDRAWING))
            //                if ((ReturnCode != DD_OK) && (ReturnCode != DDERR_WASSTILLDRAWING))
            //                {
            //                    // DirectXAttempt(ReturnCode, __LINE__, __FILE__);
            //
            //                    if (ReturnCode == DDERR_SURFACELOST)
            //                    {
            //                        goto ENDOFLOOP;
            //                    }
            //                }
            //
            //            } while (ReturnCode != DD_OK);
            //
            //#endif

            //
            // Step (2) - Copy Primary Surface to the Back Buffer
            //
            if (this.renderWorld.gfRenderScroll)
            {
                Region.Left = 0;
                Region.Top = 0;
                Region.Width = 640;
                Region.Height = 360;

                do
                {
                    //// ReturnCode = IDirectDrawSurface2_SGPBltFast(gpBackBuffer, 0, 0, gpPrimarySurface, ref Region, DDBLTFAST_NOCOLORKEY);
                    if ((ReturnCode != DD_OK) && (ReturnCode != DDERR_WASSTILLDRAWING))
                    {
                        // DirectXAttempt(ReturnCode, __LINE__, __FILE__);

                        if (ReturnCode == DDERR_SURFACELOST)
                        {
                            goto ENDOFLOOP;
                        }

                    }
                } while (ReturnCode != DD_OK);

                //Get new background for mouse
                //
                // Ok, do the actual data save to the mouse background

                //


                this.renderWorld.gfRenderScroll = false;
                this.renderWorld.gfScrollStart = false;

            }


            // COPY MOUSE AREAS FROM PRIMARY BACK!

            // FIRST OLD ERASED POSITION
            if (gMouseCursorBackground[Constants.PREVIOUS_MOUSE_DATA].fRestore == true)
            {
                Region = gMouseCursorBackground[Constants.PREVIOUS_MOUSE_DATA].Region;

                do
                {
                    //// ReturnCode = IDirectDrawSurface2_SGPBltFast(gpBackBuffer, gMouseCursorBackground[Constants.PREVIOUS_MOUSE_DATA].usMouseXPos, gMouseCursorBackground[Constants.PREVIOUS_MOUSE_DATA].usMouseYPos, gpPrimarySurface, ref Region, DDBLTFAST_NOCOLORKEY);
                    if (ReturnCode != DD_OK && ReturnCode != DDERR_WASSTILLDRAWING)
                    {
                        // DirectXAttempt(ReturnCode, __LINE__, __FILE__);

                        if (ReturnCode == DDERR_SURFACELOST)
                        {
                            goto ENDOFLOOP;
                        }
                    }
                } while (ReturnCode != DD_OK);
            }

            // NOW NEW MOUSE AREA
            if (gMouseCursorBackground[Constants.CURRENT_MOUSE_DATA].fRestore == true)
            {
                Region = gMouseCursorBackground[Constants.CURRENT_MOUSE_DATA].Region;


                do
                {
                    // ReturnCode = IDirectDrawSurface2_SGPBltFast(gpBackBuffer, gMouseCursorBackground[Constants.CURRENT_MOUSE_DATA].usMouseXPos, gMouseCursorBackground[Constants.CURRENT_MOUSE_DATA].usMouseYPos, gpPrimarySurface, ref Region, DDBLTFAST_NOCOLORKEY);
                    if ((ReturnCode != DD_OK) && (ReturnCode != DDERR_WASSTILLDRAWING))
                    {
                        // DirectXAttempt(ReturnCode, __LINE__, __FILE__);

                        if (ReturnCode == DDERR_SURFACELOST)
                        {
                            goto ENDOFLOOP;
                        }
                    }
                } while (ReturnCode != DD_OK);
            }

            if (gfForceFullScreenRefresh == true)
            {
                //
                // Method (1) - We will be refreshing the entire screen
                //
                Region.Left = 0;
                Region.Top = 0;
                Region.Width = SCREEN_WIDTH;
                Region.Height = SCREEN_HEIGHT;


                do
                {
                    // ReturnCode = IDirectDrawSurface2_SGPBltFast(gpBackBuffer, 0, 0, gpPrimarySurface, &Region, DDBLTFAST_NOCOLORKEY);
                    if ((ReturnCode != DD_OK) && (ReturnCode != DDERR_WASSTILLDRAWING))
                    {
                        // DirectXAttempt(ReturnCode, __LINE__, __FILE__);

                        if (ReturnCode == DDERR_SURFACELOST)
                        {
                            goto ENDOFLOOP;
                        }

                    }
                } while (ReturnCode != DD_OK);

                guiDirtyRegionCount = 0;
                guiDirtyRegionExCount = 0;
                gfForceFullScreenRefresh = false;
            }
            else
            {
                for (uiIndex = 0; uiIndex < guiDirtyRegionCount; uiIndex++)
                {
                    Region.Left = gListOfDirtyRegions[uiIndex].Left;
                    Region.Top = gListOfDirtyRegions[uiIndex].Top;
                    Region.Width = gListOfDirtyRegions[uiIndex].Width;
                    Region.Height = gListOfDirtyRegions[uiIndex].Height;

                    do
                    {
                        // ReturnCode = IDirectDrawSurface2_SGPBltFast(gpBackBuffer, Region.Left, Region.Top, gpPrimarySurface, ref Region, DDBLTFAST_NOCOLORKEY);
                        if ((ReturnCode != DD_OK) && (ReturnCode != DDERR_WASSTILLDRAWING))
                        {
                            // DirectXAttempt(ReturnCode, __LINE__, __FILE__);
                        }

                        if (ReturnCode == DDERR_SURFACELOST)
                        {
                            goto ENDOFLOOP;
                        }
                    } while (ReturnCode != DD_OK);
                }

                guiDirtyRegionCount = 0;
                gfForceFullScreenRefresh = false;

            }

            // Do extended dirty regions!
            for (uiIndex = 0; uiIndex < guiDirtyRegionExCount; uiIndex++)
            {
                Region.Left = gDirtyRegionsEx[uiIndex].Left;
                Region.Top = gDirtyRegionsEx[uiIndex].Top;
                Region.Width = gDirtyRegionsEx[uiIndex].Width;
                Region.Height = gDirtyRegionsEx[uiIndex].Height;

                if ((Region.Top < this.renderWorld.gsVIEWPORT_WINDOW_END_Y) && this.renderWorld.gfRenderScroll)
                {
                    continue;
                }

                do
                {
                    // ReturnCode = IDirectDrawSurface2_SGPBltFast(gpBackBuffer, Region.Left, Region.Top, gpPrimarySurface, ref Region, DDBLTFAST_NOCOLORKEY);
                    if ((ReturnCode != DD_OK) && (ReturnCode != DDERR_WASSTILLDRAWING))
                    {
                        // DirectXAttempt(ReturnCode, __LINE__, __FILE__);
                    }

                    if (ReturnCode == DDERR_SURFACELOST)
                    {
                        goto ENDOFLOOP;
                    }
                } while (ReturnCode != DD_OK);
            }

            guiDirtyRegionExCount = 0;


        ENDOFLOOP:


            fFirstTime = false;

        }
    }

    public struct MouseCursorBackground
    {
        public bool fRestore;
        public int usMouseXPos, usMouseYPos;
        public int usLeft, usTop, usRight, usBottom;
        public Rectangle Region;
        //LPDIRECTDRAWSURFACE _pSurface;
        //LPDIRECTDRAWSURFACE2 pSurface;
    }
}

