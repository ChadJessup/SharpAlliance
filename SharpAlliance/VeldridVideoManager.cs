using System;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SharpAlliance.Core.Managers;
using SharpAlliance.Core.Managers.Image;
using SharpAlliance.Core.Screens;
using SharpAlliance.Core.SubSystems;
using SharpAlliance.Platform;
using SharpAlliance.Platform.Interfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Veldrid;
using Veldrid.ImageSharp;
using Veldrid.Sdl2;
using Veldrid.SPIRV;
using Veldrid.StartupUtilities;
using Veldrid.Utilities;
using Point = Veldrid.Point;
using Rectangle = Veldrid.Rectangle;

namespace SharpAlliance
{
    public class VeldridVideoManager : IVideoManager
    {
        public static class Constants
        {
            public const int MAX_DIRTY_REGIONS = 128;

            public const int CURRENT_MOUSE_DATA = 0;
            public const int PREVIOUS_MOUSE_DATA = 1;
            public const int MAX_NUM_FRAMES = 25;
        }

        // TODO: These are temporary...
        const int DD_OK = 0;
        const int DDBLTFAST_NOCOLORKEY = 0x00000000;
        const int DDBLTFAST_SRCCOLORKEY = 0x00000001;
        const int DDBLTFAST_DESTCOLORKEY = 0x00000002;
        const int DDBLTFAST_WAIT = 0x00000010;
        const int DDERR_WASSTILLDRAWING = 0x8700000; // not real value
        const int DDERR_SURFACELOST = 0x9700000;

        private readonly ILogger<VeldridVideoManager> logger;
        // private readonly WindowsSubSystem windows;
        private readonly InputManager inputs;
        private readonly MouseSubSystem mouse;
        private readonly RenderWorld renderWorld;
        private readonly ScreenManager screenManager;
        private readonly GameContext context;
        private readonly MouseCursorBackground[] mouseCursorBackground = new MouseCursorBackground[2];

        private Sdl2Window window;
        public Sdl2Window Window { get => this.window; }
        private GraphicsDevice graphicDevice;
        private ResourceFactory factory;
        private Swapchain mainSwapchain;
        private CommandList commandList;
        private SpriteRenderer sr;
        private DeviceBuffer _screenSizeBuffer;
        private DeviceBuffer _shiftBuffer;
        private DeviceBuffer _vertexBuffer;
        private DeviceBuffer _indexBuffer;
        private Shader _computeShader;
        private ResourceLayout _computeLayout;
        private Pipeline _computePipeline;
        private ResourceSet _computeResourceSet;
        private Pipeline _graphicsPipeline;
        private ResourceSet _graphicsResourceSet;

        private Texture _computeTargetTexture;
        private TextureView _computeTargetTextureView;
        private ResourceLayout _graphicsLayout;
        private float _ticks;

        private bool _colorSrgb = true;

        private FadeScreen? fadeScreen;
        private Action? gpFrameBufferRefreshOverride;

        private int gusScreenWidth = 640;
        private int gusScreenHeight = 480;
        private int gubScreenPixelDepth;

        private RgbaFloat clearColor = new(1.0f, 0, 0.2f, 1f);

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
        private BufferState guiFrameBufferState;    // BUFFER_READY, BUFFER_DIRTY
        private BufferState guiMouseBufferState;    // BUFFER_READY, BUFFER_DIRTY, BUFFER_DISABLED
        private VideoManagerState guiVideoManagerState;   // VIDEO_ON, VIDEO_OFF, Constants.VIDEO_SUSPENDED, Constants.VIDEO_SHUTTING_DOWN
        private ThreadState guiRefreshThreadState;  // Constants.THREAD_ON, Constants.THREAD_OFF, Constants.THREAD_SUSPENDED

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

        // private LPDIRectangleDRAW? _gpDirectDrawObject = null;
        // private LPDIRectangleDRAW2 gpDirectDrawObject = null;

        //private Surface? _gpPrimarySurface = null;
        //private Surface2? gpPrimarySurface = null;
        //private Surface2? gpBackBuffer = null

        public bool IsInitialized { get; private set; }

        private const int SCREEN_WIDTH = 640;
        private const int SCREEN_HEIGHT = 480;
        private const int PIXEL_DEPTH = 16;

        static ThreadState uiRefreshThreadState;
        static int uiIndex;
        static bool fShowMouse;
        static Rectangle Region;
        static Point MousePos;
        static bool fFirstTime = true;
        private bool windowResized;

        private Texture backBuffer;
        private Texture gpFrameBuffer;
        private Texture gpPrimarySurface;
        private Texture gpBackBuffer;

        public VeldridVideoManager(
            ILogger<VeldridVideoManager> logger,
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

            Configuration.Default.MemoryAllocator = new SixLabors.ImageSharp.Memory.SimpleGcMemoryAllocator();
        }

        public ValueTask<bool> Initialize()
        {
            WindowCreateInfo windowCI = new()
            {
                X = 50,
                Y = 50,
                WindowWidth = 960,
                WindowHeight = 540,
                WindowInitialState = WindowState.Normal,
                WindowTitle = "Sharp Alliance!!!",
            };

            GraphicsDeviceOptions gdOptions = new(
                debug: false,
                swapchainDepthFormat: null,
                syncToVerticalBlank: false,
                resourceBindingModel: ResourceBindingModel.Improved,
                preferDepthRangeZeroToOne: true,
                preferStandardClipSpaceYDirection: true,
                swapchainSrgbFormat: this._colorSrgb);

#if DEBUG
            gdOptions.Debug = true;
#endif
            static SDL_WindowFlags GetWindowFlags(WindowState state)
                => state switch
                {
                    WindowState.Normal => 0,
                    WindowState.FullScreen => SDL_WindowFlags.Fullscreen,
                    WindowState.Maximized => SDL_WindowFlags.Maximized,
                    WindowState.Minimized => SDL_WindowFlags.Minimized,
                    WindowState.BorderlessFullScreen => SDL_WindowFlags.FullScreenDesktop,
                    WindowState.Hidden => SDL_WindowFlags.Hidden,
                    _ => throw new VeldridException("Invalid WindowState: " + state),
                };

            SDL_WindowFlags flags = SDL_WindowFlags.OpenGL
                | SDL_WindowFlags.Resizable
                | GetWindowFlags(windowCI.WindowInitialState);

            if (windowCI.WindowInitialState != WindowState.Hidden)
            {
                flags |= SDL_WindowFlags.Shown;
            }

            this.window = new Sdl2Window(
                windowCI.WindowTitle,
                windowCI.X,
                windowCI.Y,
                windowCI.WindowWidth,
                windowCI.WindowHeight,
                flags,
                threadedProcessing: true);

            this.graphicDevice = VeldridStartup.CreateGraphicsDevice(
                this.window,
                gdOptions);

            this.Window.Resized += () => this.windowResized = true;
            //this.Window.PollIntervalInMs = 1000 / 30;
            this.factory = new DisposeCollectorResourceFactory(this.graphicDevice.ResourceFactory);
            this.mainSwapchain = this.graphicDevice.MainSwapchain;
            this.commandList = this.graphicDevice.ResourceFactory.CreateCommandList();

            this.sr = new SpriteRenderer(this.graphicDevice);

            this.guiFrameBufferState = BufferState.DIRTY;
            this.guiMouseBufferState = BufferState.DISABLED;
            this.guiVideoManagerState = VideoManagerState.On;
            this.guiRefreshThreadState = ThreadState.Off;
            this.guiDirtyRegionCount = 0;
            this.gfForceFullScreenRefresh = true;
            this.gpFrameBufferRefreshOverride = null;
            //gpCursorStore = NULL;
            this.gfPrintFrameBuffer = false;
            this.guiPrintFrameBufferIndex = 0;

            // this.fadeScreen = (screenManager.GetScreen(ScreenNames.FADE_SCREEN, activate: true).AsTask().Result as FadeScreen)!;
            this.IsInitialized = true;

            return ValueTask.FromResult(this.IsInitialized);
        }

        public void DrawFrame()
        {
            this.commandList.Begin();

            this.commandList.SetFramebuffer(this.mainSwapchain.Framebuffer);
            this.commandList.ClearColorTarget(0, this.clearColor);

            this.sr.Draw(this.graphicDevice, this.commandList);

            this.commandList.End();

            this.graphicDevice.SubmitCommands(this.commandList);
            this.graphicDevice.SwapBuffers(this.mainSwapchain);
        }

        public static byte[] ReadEmbeddedAssetBytes(string name)
        {
            using Stream stream = OpenEmbeddedAssetStream(name);
            byte[] bytes = new byte[stream.Length];
            using MemoryStream ms = new(bytes);

            stream.CopyTo(ms);
            return bytes;
        }

        public static Stream OpenEmbeddedAssetStream(string name)
            => typeof(VeldridVideoManager).Assembly.GetManifestResourceStream(name)!;

        public void RefreshScreen(object? dummy)
        {
            int usScreenWidth;
            int usScreenHeight;
            int ReturnCode;
            long uiTime;

            usScreenWidth = usScreenHeight = 0;

            if (fFirstTime)
            {
                fShowMouse = false;
            }

            this.logger.LogDebug(LoggingEventId.VIDEO, "Looping in refresh");

            ///////////////////////////////////////////////////////////////////////////////////////////////
            // 
            // REFRESH_THREAD_MUTEX 
            //
            ///////////////////////////////////////////////////////////////////////////////////////////////

            switch (this.guiVideoManagerState)
            {
                case VideoManagerState.On:
                    // Excellent, everything is cosher, we continue on
                    uiRefreshThreadState = this.guiRefreshThreadState = ThreadState.On;
                    usScreenWidth = this.gusScreenWidth;
                    usScreenHeight = this.gusScreenHeight;
                    break;
                case VideoManagerState.Off:
                    // Hot damn, the video manager is suddenly off. We have to bugger out of here. Don't forget to
                    // leave the critical section
                    this.guiRefreshThreadState = ThreadState.Off;
                    return;
                case VideoManagerState.Suspended:
                    // This are suspended. Make sure the refresh function does try to access any of the direct
                    // draw surfaces
                    uiRefreshThreadState = this.guiRefreshThreadState = ThreadState.Suspended;
                    break;
                case VideoManagerState.ShuttingDown:
                    // Well things are shutting down. So we need to bugger out of there. Don't forget to leave the
                    // critical section before returning
                    this.guiRefreshThreadState = ThreadState.Off;
                    return;
            }

            //
            // Get the current mouse position
            //

            // this.inputs.GetCursorPosition(out var tmpMousePos);
            // MousePos = new Point(tmpMousePos.X, tmpMousePos.Y);

            /////////////////////////////////////////////////////////////////////////////////////////////
            // 
            // FRAME_BUFFER_MUTEX 
            //
            /////////////////////////////////////////////////////////////////////////////////////////////

            // RESTORE OLD POSITION OF MOUSE
            if (this.mouseCursorBackground[Constants.CURRENT_MOUSE_DATA].fRestore == true)
            {
                Region.X = this.mouseCursorBackground[Constants.CURRENT_MOUSE_DATA].usLeft;
                Region.Y = this.mouseCursorBackground[Constants.CURRENT_MOUSE_DATA].usTop;
                Region.Width = this.mouseCursorBackground[Constants.CURRENT_MOUSE_DATA].usRight;
                Region.Height = this.mouseCursorBackground[Constants.CURRENT_MOUSE_DATA].usBottom;

                this.mouse.Draw(
                    this.mouseCursorBackground[Constants.CURRENT_MOUSE_DATA],
                    ref Region,
                    this.graphicDevice,
                    this.commandList);

                // Save position into other background region
                this.mouseCursorBackground[Constants.PREVIOUS_MOUSE_DATA] = this.mouseCursorBackground[Constants.CURRENT_MOUSE_DATA];
            }

            // Ok we were able to get a hold of the frame buffer stuff. Check to see if it needs updating
            // if not, release the frame buffer stuff right away
            if (this.guiFrameBufferState == BufferState.DIRTY)
            {
                // Well the frame buffer is dirty.
                if (this.gpFrameBufferRefreshOverride != null)
                {
                    // Method (3) - We are using a function override to refresh the frame buffer. First we
                    // call the override function then we must set the override pointer to null
                    this.gpFrameBufferRefreshOverride();
                    this.gpFrameBufferRefreshOverride = null;
                }

                if (this.fadeScreen?.gfFadeInitialized ?? false
                    && this.fadeScreen.gfFadeInVideo)
                {
                    this.fadeScreen!.gFadeFunction();
                }
                else
                {
                    // Either Method (1) or (2)
                    if (this.gfForceFullScreenRefresh == true)
                    {
                        // Method (1) - We will be refreshing the entire screen
                        Region.X = 0;
                        Region.Y = 0;
                        Region.Width = usScreenWidth;
                        Region.Height = usScreenHeight;

                        this.DrawRegion(
                            this.gpBackBuffer,
                            new Point(0, 0),
                            ref Region,
                            this.gpFrameBuffer);
                    }
                    else
                    {
                        for (uiIndex = 0; uiIndex < this.guiDirtyRegionCount; uiIndex++)
                        {
                            Region.X = this.gListOfDirtyRegions[uiIndex].Left;
                            Region.Y = this.gListOfDirtyRegions[uiIndex].Top;
                            Region.Width = this.gListOfDirtyRegions[uiIndex].Width;
                            Region.Height = this.gListOfDirtyRegions[uiIndex].Height;

                            this.DrawRegion(
                                this.backBuffer,
                                Region.Position,
                                ref Region,
                                this.gpPrimarySurface);
                        }

                        // Now do new, extended dirty regions
                        for (uiIndex = 0; uiIndex < this.guiDirtyRegionExCount; uiIndex++)
                        {
                            Region.X = this.gDirtyRegionsEx[uiIndex].Left;
                            Region.Y = this.gDirtyRegionsEx[uiIndex].Top;
                            Region.Width = this.gDirtyRegionsEx[uiIndex].Width;
                            Region.Height = this.gDirtyRegionsEx[uiIndex].Height;

                            // Do some checks if we are in the process of scrolling!	
                            if (this.renderWorld.gfRenderScroll)
                            {
                                // Check if we are completely out of bounds
                                if (Region.Y <= this.renderWorld.gsVIEWPORT_WINDOW_END_Y && Region.Height <= this.renderWorld.gsVIEWPORT_WINDOW_END_Y)
                                {
                                    continue;
                                }
                            }

                            this.DrawRegion(
                                this.backBuffer,
                                Region.Position,
                                ref Region,
                                this.gpFrameBuffer);
                        }
                    }
                }

                if (this.renderWorld.gfRenderScroll)
                {
                    this.ScrollJA2Background(
                        this.renderWorld.guiScrollDirection,
                        this.renderWorld.gsScrollXIncrement,
                        this.renderWorld.gsScrollYIncrement,
                        this.gpPrimarySurface,
                        this.gpBackBuffer,
                        true,
                        Constants.PREVIOUS_MOUSE_DATA);
                }

                this.renderWorld.gfIgnoreScrollDueToCenterAdjust = false;

                // Update the guiFrameBufferState variable to reflect that the frame buffer can now be handled
                this.guiFrameBufferState = BufferState.READY;
            }

            // Do we want to print the frame stuff ??
            if (this.gfVideoCapture)
            {
                uiTime = this.context.ClockManager.GetTickCount();
                if ((uiTime < this.guiLastFrame) || (uiTime > (this.guiLastFrame + this.guiFramePeriod)))
                {
                    //SnapshotSmall();
                    this.guiLastFrame = uiTime;
                }
            }

            if (this.gfPrintFrameBuffer == true)
            {
                FileStream? OutputFile;
                string? FileName;
                int iIndex;
                string? ExecDir;
                int[] p16BPPData;

                //GetExecutableDirectory(ExecDir);
                //SetFileManCurrentDirectory(ExecDir);

                // Create temporary system memory surface. This is used to correct problems with the backbuffer
                // surface which can be interlaced or have a funky pitch
                Texture _pTmpBuffer = new ImageSharpTexture(
                    new Image<Rgba32>(usScreenWidth, usScreenHeight), false)
                        .CreateDeviceTexture(this.graphicDevice, this.graphicDevice.ResourceFactory);

                Texture pTmpBuffer = new ImageSharpTexture(
                    new Image<Rgba32>(usScreenWidth, usScreenHeight), false)
                        .CreateDeviceTexture(this.graphicDevice, this.graphicDevice.ResourceFactory);

                // Copy the primary surface to the temporary surface
                Region.X = 0;
                Region.Y = 0;
                Region.Width = usScreenWidth;
                Region.Height = usScreenHeight;

                this.DrawRegion(pTmpBuffer,
                    new Point(0, 0),
                    ref Region,
                    this.gpPrimarySurface);

                // Ok now that temp surface has contents of backbuffer, copy temp surface to disk
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
                //    //ReturnCode = IDirectDrawSurface2_Unlock(
                //    pTmpBuffer,
                //    &SurfaceDescription);
                //    if ((ReturnCode != DD_OK) && (ReturnCode != DDERR_WASSTILLDRAWING))
                //    {
                //        // DirectXAttempt(ReturnCode, __LINE__, __FILE__);
                //    }
                //}

                // Release temp surface
                this.gfPrintFrameBuffer = false;

                _pTmpBuffer?.Dispose();
                pTmpBuffer?.Dispose();

                //strcat(ExecDir, "\\Data");
                //SetFileManCurrentDirectory(ExecDir);
            }

            // Ok we were able to get a hold of the frame buffer stuff. Check to see if it needs updating
            // if not, release the frame buffer stuff right away
            if (this.guiMouseBufferState == BufferState.DIRTY)
            {
                // Well the mouse buffer is dirty. Upload the whole thing
                Region.X = 0;
                Region.Y = 0;
                Region.Width = this.gusMouseCursorWidth;
                Region.Height = this.gusMouseCursorHeight;

                this.DrawRegion(
                    this.mouse.gpMouseCursor,
                    new Point(0, 0),
                    ref Region,
                    this.mouse.gpMouseCursorOriginal);

                this.guiMouseBufferState = BufferState.READY;
            }

            // Check current state of the mouse cursor
            if (fShowMouse == false)
            {
                if (this.guiMouseBufferState == BufferState.READY)
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
                if (this.guiMouseBufferState == BufferState.DISABLED)
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
                // Step (1) - Save mouse background
                Region.X = MousePos.X - this.gsMouseCursorXOffset;
                Region.Y = MousePos.Y - this.gsMouseCursorYOffset;
                Region.Width = Region.X + this.gusMouseCursorWidth;
                Region.Height = Region.Y + this.gusMouseCursorHeight;

                if (Region.Width > usScreenWidth)
                {
                    Region.Width = usScreenWidth;
                }

                if (Region.Height > usScreenHeight)
                {
                    Region.Height = usScreenHeight;
                }

                if ((Region.Width > Region.X) && (Region.Height > Region.Y))
                {
                    // Make sure the mouse background is marked for restore and coordinates are saved for the
                    // future restore
                    this.mouseCursorBackground[Constants.CURRENT_MOUSE_DATA].fRestore = true;
                    this.mouseCursorBackground[Constants.CURRENT_MOUSE_DATA].usRight = (int)Region.Width - (int)Region.X;
                    this.mouseCursorBackground[Constants.CURRENT_MOUSE_DATA].usBottom = (int)Region.Height - (int)Region.Y;
                    if (Region.X < 0)
                    {
                        this.mouseCursorBackground[Constants.CURRENT_MOUSE_DATA].usLeft = (int)(0 - Region.X);
                        this.mouseCursorBackground[Constants.CURRENT_MOUSE_DATA].usMouseXPos = 0;
                        Region.X = 0;
                    }
                    else
                    {
                        this.mouseCursorBackground[Constants.CURRENT_MOUSE_DATA].usMouseXPos = (int)MousePos.X - this.gsMouseCursorXOffset;
                        this.mouseCursorBackground[Constants.CURRENT_MOUSE_DATA].usLeft = 0;
                    }
                    if (Region.Y < 0)
                    {
                        this.mouseCursorBackground[Constants.CURRENT_MOUSE_DATA].usMouseYPos = 0;
                        this.mouseCursorBackground[Constants.CURRENT_MOUSE_DATA].usTop = (int)(0 - Region.Y);
                        Region.Y = 0;
                    }
                    else
                    {
                        this.mouseCursorBackground[Constants.CURRENT_MOUSE_DATA].usMouseYPos = (int)MousePos.Y - this.gsMouseCursorYOffset;
                        this.mouseCursorBackground[Constants.CURRENT_MOUSE_DATA].usTop = 0;
                    }

                    if ((Region.Width > Region.X) && (Region.Height > Region.Y))
                    {
                        // Save clipped region
                        this.mouseCursorBackground[Constants.CURRENT_MOUSE_DATA].Region = Region;

                        // Ok, do the actual data save to the mouse background
                        this.DrawRegion(
                            this.mouseCursorBackground[Constants.CURRENT_MOUSE_DATA].pSurface,
                            new Point(this.mouseCursorBackground[Constants.CURRENT_MOUSE_DATA].usLeft,
                                this.mouseCursorBackground[Constants.CURRENT_MOUSE_DATA].usTop),
                            ref Region,
                            this.gpBackBuffer);

                        // Step (2) - Blit mouse cursor to back buffer
                        Region.X = this.mouseCursorBackground[Constants.CURRENT_MOUSE_DATA].usLeft;
                        Region.Y = this.mouseCursorBackground[Constants.CURRENT_MOUSE_DATA].usTop;
                        Region.Width = this.mouseCursorBackground[Constants.CURRENT_MOUSE_DATA].usRight;
                        Region.Height = this.mouseCursorBackground[Constants.CURRENT_MOUSE_DATA].usBottom;

                        this.DrawRegion(
                            this.gpBackBuffer,
                            new Point(this.mouseCursorBackground[Constants.CURRENT_MOUSE_DATA].usMouseXPos,
                                this.mouseCursorBackground[Constants.CURRENT_MOUSE_DATA].usMouseYPos),
                            ref Region,
                            this.mouse.gpMouseCursor);
                    }
                    else
                    {
                        // Hum, the mouse was not blitted this round. Henceforth we will flag fRestore as false
                        this.mouseCursorBackground[Constants.CURRENT_MOUSE_DATA].fRestore = false;
                    }
                }
                else
                {
                    // Hum, the mouse was not blitted this round. Henceforth we will flag fRestore as false
                    this.mouseCursorBackground[Constants.CURRENT_MOUSE_DATA].fRestore = false;
                }
            }
            else
            {
                // Well since there was no mouse handling this round, we disable the mouse restore
                this.mouseCursorBackground[Constants.CURRENT_MOUSE_DATA].fRestore = false;
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
            var emptyRect = new Rectangle();

            this.DrawRegion(
                this.gpPrimarySurface,
                this.rcWindow.Position,
                ref emptyRect,
                this.gpBackBuffer);

            //#else

            //ReturnCode = IDirectDrawSurface_Flip(
            //_gpPrimarySurface,
            //null,
            //DDFLIP_WAIT);

            //#endif

            // Step (2) - Copy Primary Surface to the Back Buffer
            if (this.renderWorld.gfRenderScroll)
            {
                Region.X = 0;
                Region.Y = 0;
                Region.Width = 640;
                Region.Height = 360;

                this.DrawRegion(
                    this.gpBackBuffer,
                    new Point(0, 0),
                    ref Region,
                    this.gpPrimarySurface);

                // Get new background for mouse
                // Ok, do the actual data save to the mouse background
                this.renderWorld.gfRenderScroll = false;
                this.renderWorld.gfScrollStart = false;
            }

            // COPY MOUSE AREAS FROM PRIMARY BACK!

            // FIRST OLD ERASED POSITION
            if (this.mouseCursorBackground[Constants.PREVIOUS_MOUSE_DATA].fRestore == true)
            {
                Region = this.mouseCursorBackground[Constants.PREVIOUS_MOUSE_DATA].Region;

                this.DrawRegion(
                    this.gpBackBuffer,
                    new Point(this.mouseCursorBackground[Constants.PREVIOUS_MOUSE_DATA].usMouseXPos,
                        this.mouseCursorBackground[Constants.PREVIOUS_MOUSE_DATA].usMouseYPos),
                    ref Region,
                    this.gpPrimarySurface);
            }

            // NOW NEW MOUSE AREA
            if (this.mouseCursorBackground[Constants.CURRENT_MOUSE_DATA].fRestore == true)
            {
                Region = this.mouseCursorBackground[Constants.CURRENT_MOUSE_DATA].Region;

                this.DrawRegion(
                    this.gpBackBuffer,
                    new Point(this.mouseCursorBackground[Constants.CURRENT_MOUSE_DATA].usMouseXPos,
                        this.mouseCursorBackground[Constants.CURRENT_MOUSE_DATA].usMouseYPos),
                    ref Region,
                    this.gpPrimarySurface);
            }

            if (this.gfForceFullScreenRefresh == true)
            {
                // Method (1) - We will be refreshing the entire screen
                Region.X = 0;
                Region.Y = 0;
                Region.Width = SCREEN_WIDTH;
                Region.Height = SCREEN_HEIGHT;

                this.DrawRegion(this.gpBackBuffer,
                    new Point(0, 0),
                    ref Region,
                    this.gpPrimarySurface);

                this.guiDirtyRegionCount = 0;
                this.guiDirtyRegionExCount = 0;
                this.gfForceFullScreenRefresh = false;
            }
            else
            {
                for (uiIndex = 0; uiIndex < this.guiDirtyRegionCount; uiIndex++)
                {
                    Region.X = this.gListOfDirtyRegions[uiIndex].Left;
                    Region.Y = this.gListOfDirtyRegions[uiIndex].Top;
                    Region.Width = this.gListOfDirtyRegions[uiIndex].Width;
                    Region.Height = this.gListOfDirtyRegions[uiIndex].Height;

                    this.DrawRegion(this.gpBackBuffer,
                        new Point(Region.X, Region.Y),
                        ref Region,
                        this.gpPrimarySurface);
                }

                this.guiDirtyRegionCount = 0;
                this.gfForceFullScreenRefresh = false;

            }

            // Do extended dirty regions!
            for (uiIndex = 0; uiIndex < this.guiDirtyRegionExCount; uiIndex++)
            {
                Region.X = this.gDirtyRegionsEx[uiIndex].Left;
                Region.Y = this.gDirtyRegionsEx[uiIndex].Top;
                Region.Width = this.gDirtyRegionsEx[uiIndex].Width;
                Region.Height = this.gDirtyRegionsEx[uiIndex].Height;

                if ((Region.Y < this.renderWorld.gsVIEWPORT_WINDOW_END_Y) && this.renderWorld.gfRenderScroll)
                {
                    continue;
                }

                this.DrawRegion(this.gpBackBuffer,
                    new Point(Region.X, Region.Y),
                    ref Region,
                    this.gpPrimarySurface);
            }

            this.guiDirtyRegionExCount = 0;

            fFirstTime = false;
        }

        private void DrawRegion(
            Texture destinationTexture,
            Vector2 destinationPoint,
            ref Rectangle sourceRegion,
            Texture sourceTexture)
            => this.DrawRegion(
                destinationTexture,
                new Point((int)destinationPoint.X, (int)destinationPoint.Y),
                ref sourceRegion,
                sourceTexture);

        private void DrawRegion(
            Texture destinationTexture,
            int destinationPointX,
            int destinationPointY,
            ref Rectangle sourceRegion,
            Texture sourceTexture)
            => this.DrawRegion(
                destinationTexture,
                new Point(destinationPointX, destinationPointY),
                ref sourceRegion,
                sourceTexture);

        private void DrawRegion(
            Texture destinationTexture,
            Point destinationPoint,
            ref Rectangle sourceRegion,
            Texture sourceTexture)
        {
        }

        private void ScrollJA2Background(
            ScrollDirection uiDirection,
            int sScrollXIncrement,
            int sScrollYIncrement,
            Texture pSource,
            Texture pDest,
            bool fRenderStrip,
            int uiCurrentMouseBackbuffer)
        {
            int usWidth, usHeight;
            int ubBitDepth;
            int ReturnCode = DD_OK;
            Rectangle Region;
            int usMouseXPos, usMouseYPos;
            Rectangle[] StripRegions = new Rectangle[2];
            Rectangle MouseRegion;
            int usNumStrips = 0;
            int cnt;
            int sShiftX, sShiftY;
            int uiCountY;

            this.GetCurrentVideoSettings(out usWidth, out usHeight, out ubBitDepth);
            usHeight = (this.renderWorld.gsVIEWPORT_WINDOW_END_Y - this.renderWorld.gsVIEWPORT_WINDOW_START_Y);

            StripRegions[0].X = this.renderWorld.gsVIEWPORT_START_X;
            StripRegions[0].Width = this.renderWorld.gsVIEWPORT_END_X;
            StripRegions[0].Y = this.renderWorld.gsVIEWPORT_WINDOW_START_Y;
            StripRegions[0].Height = this.renderWorld.gsVIEWPORT_WINDOW_END_Y;
            StripRegions[1].X = this.renderWorld.gsVIEWPORT_START_X;
            StripRegions[1].Width = this.renderWorld.gsVIEWPORT_END_X;
            StripRegions[1].Y = this.renderWorld.gsVIEWPORT_WINDOW_START_Y;
            StripRegions[1].Height = this.renderWorld.gsVIEWPORT_WINDOW_END_Y;

            MouseRegion.X = this.mouseCursorBackground[uiCurrentMouseBackbuffer].usLeft;
            MouseRegion.Y = this.mouseCursorBackground[uiCurrentMouseBackbuffer].usTop;
            MouseRegion.Width = this.mouseCursorBackground[uiCurrentMouseBackbuffer].usRight;
            MouseRegion.Height = this.mouseCursorBackground[uiCurrentMouseBackbuffer].usBottom;

            usMouseXPos = this.mouseCursorBackground[uiCurrentMouseBackbuffer].usMouseXPos;
            usMouseYPos = this.mouseCursorBackground[uiCurrentMouseBackbuffer].usMouseYPos;

            switch (uiDirection)
            {
                case ScrollDirection.SCROLL_LEFT:

                    Region.X = 0;
                    Region.Y = this.renderWorld.gsVIEWPORT_WINDOW_START_Y;
                    Region.Width = usWidth - (sScrollXIncrement);
                    Region.Height = this.renderWorld.gsVIEWPORT_WINDOW_START_Y + usHeight;

                    this.DrawRegion(
                        pDest,
                        sScrollXIncrement,
                        this.renderWorld.gsVIEWPORT_WINDOW_START_Y,
                        ref Region,
                        pSource);

                    // memset z-buffer
                    for (uiCountY = this.renderWorld.gsVIEWPORT_WINDOW_START_Y; uiCountY < this.renderWorld.gsVIEWPORT_WINDOW_END_Y; uiCountY++)
                    {
                        // memset((int*)gpZBuffer + (uiCountY * 1280), 0, sScrollXIncrement * 2);
                    }

                    StripRegions[0].Width = (int)(this.renderWorld.gsVIEWPORT_START_X + sScrollXIncrement);
                    usMouseXPos += sScrollXIncrement;

                    usNumStrips = 1;
                    break;

                case ScrollDirection.SCROLL_RIGHT:


                    Region.X = sScrollXIncrement;
                    Region.Y = this.renderWorld.gsVIEWPORT_WINDOW_START_Y;
                    Region.Width = usWidth;
                    Region.Height = this.renderWorld.gsVIEWPORT_WINDOW_START_Y + usHeight;

                    this.DrawRegion(
                        pDest,
                        0,
                        this.renderWorld.gsVIEWPORT_WINDOW_START_Y,
                        ref Region,
                        pSource);

                    // // memset z-buffer
                    for (uiCountY = this.renderWorld.gsVIEWPORT_WINDOW_START_Y; uiCountY < this.renderWorld.gsVIEWPORT_WINDOW_END_Y; uiCountY++)
                    {
                        // memset((int*)gpZBuffer + (uiCountY * 1280) + ((this.renderWorld.gsVIEWPORT_END_X - sScrollXIncrement) * 2), 0, sScrollXIncrement * 2);
                    }

                    //for(uiCountY=0; uiCountY < usHeight; uiCountY++)
                    //{
                    //	memcpy(pDestBuf+(uiCountY*uiDestPitchBYTES),
                    //					pSrcBuf+(uiCountY*uiDestPitchBYTES)+sScrollXIncrement*uiBPP,
                    //					uiDestPitchBYTES-sScrollXIncrement*uiBPP);
                    //}

                    StripRegions[0].X = (int)(this.renderWorld.gsVIEWPORT_END_X - sScrollXIncrement);
                    usMouseXPos -= sScrollXIncrement;

                    usNumStrips = 1;
                    break;

                case ScrollDirection.SCROLL_UP:

                    Region.X = 0;
                    Region.Y = this.renderWorld.gsVIEWPORT_WINDOW_START_Y;
                    Region.Width = usWidth;
                    Region.Height = this.renderWorld.gsVIEWPORT_WINDOW_START_Y + usHeight - sScrollYIncrement;

                    this.DrawRegion(
                        pDest,
                        0,
                        this.renderWorld.gsVIEWPORT_WINDOW_START_Y + sScrollYIncrement,
                        ref Region,
                        pSource);

                    for (uiCountY = sScrollYIncrement - 1 + this.renderWorld.gsVIEWPORT_WINDOW_START_Y; uiCountY >= this.renderWorld.gsVIEWPORT_WINDOW_START_Y; uiCountY--)
                    {
                        // memset((int*)gpZBuffer + (uiCountY * 1280), 0, 1280);
                    }

                    //for(uiCountY=usHeight-1; uiCountY >= sScrollYIncrement; uiCountY--)
                    //{
                    //	memcpy(pDestBuf+(uiCountY*uiDestPitchBYTES),
                    //					pSrcBuf+((uiCountY-sScrollYIncrement)*uiDestPitchBYTES),
                    //					uiDestPitchBYTES);
                    //}
                    StripRegions[0].Height = (int)(this.renderWorld.gsVIEWPORT_WINDOW_START_Y + sScrollYIncrement);
                    usNumStrips = 1;

                    usMouseYPos += sScrollYIncrement;

                    break;

                case ScrollDirection.SCROLL_DOWN:

                    Region.X = 0;
                    Region.Y = this.renderWorld.gsVIEWPORT_WINDOW_START_Y + sScrollYIncrement;
                    Region.Width = usWidth;
                    Region.Height = this.renderWorld.gsVIEWPORT_WINDOW_START_Y + usHeight;

                    this.DrawRegion(
                        pDest,
                        0,
                        this.renderWorld.gsVIEWPORT_WINDOW_START_Y,
                        ref Region,
                        pSource);

                    // Zero out z
                    for (uiCountY = (this.renderWorld.gsVIEWPORT_WINDOW_END_Y - sScrollYIncrement); uiCountY < this.renderWorld.gsVIEWPORT_WINDOW_END_Y; uiCountY++)
                    {
                        // memset((int*)gpZBuffer + (uiCountY * 1280), 0, 1280);
                    }

                    //for(uiCountY=0; uiCountY < (usHeight-sScrollYIncrement); uiCountY++)
                    //{
                    //	memcpy(pDestBuf+(uiCountY*uiDestPitchBYTES),
                    //					pSrcBuf+((uiCountY+sScrollYIncrement)*uiDestPitchBYTES),
                    //					uiDestPitchBYTES);
                    //}

                    StripRegions[0].Y = (int)(this.renderWorld.gsVIEWPORT_WINDOW_END_Y - sScrollYIncrement);
                    usNumStrips = 1;

                    usMouseYPos -= sScrollYIncrement;

                    break;

                case ScrollDirection.SCROLL_UPLEFT:

                    Region.X = 0;
                    Region.Y = this.renderWorld.gsVIEWPORT_WINDOW_START_Y;
                    Region.Width = usWidth - (sScrollXIncrement);
                    Region.Height = this.renderWorld.gsVIEWPORT_WINDOW_START_Y + usHeight - sScrollYIncrement;

                    this.DrawRegion(
                        pDest,
                        sScrollXIncrement,
                        this.renderWorld.gsVIEWPORT_WINDOW_START_Y + sScrollYIncrement,
                        ref Region,
                        pSource);

                    // // memset z-buffer
                    for (uiCountY = this.renderWorld.gsVIEWPORT_WINDOW_START_Y; uiCountY < this.renderWorld.gsVIEWPORT_WINDOW_END_Y; uiCountY++)
                    {
                        // memset((int*)gpZBuffer + (uiCountY * 1280), 0, sScrollXIncrement * 2);

                    }
                    for (uiCountY = this.renderWorld.gsVIEWPORT_WINDOW_START_Y + sScrollYIncrement - 1; uiCountY >= this.renderWorld.gsVIEWPORT_WINDOW_START_Y; uiCountY--)
                    {
                        // memset((int*)gpZBuffer + (uiCountY * 1280), 0, 1280);
                    }

                    StripRegions[0].Width = (int)(this.renderWorld.gsVIEWPORT_START_X + sScrollXIncrement);
                    StripRegions[1].Height = (int)(this.renderWorld.gsVIEWPORT_WINDOW_START_Y + sScrollYIncrement);
                    StripRegions[1].X = (int)(this.renderWorld.gsVIEWPORT_START_X + sScrollXIncrement);
                    usNumStrips = 2;

                    usMouseYPos += sScrollYIncrement;
                    usMouseXPos += sScrollXIncrement;

                    break;

                case ScrollDirection.SCROLL_UPRIGHT:

                    Region.X = sScrollXIncrement;
                    Region.Y = this.renderWorld.gsVIEWPORT_WINDOW_START_Y;
                    Region.Width = usWidth;
                    Region.Height = this.renderWorld.gsVIEWPORT_WINDOW_START_Y + usHeight - sScrollYIncrement;

                    this.DrawRegion(
                        pDest,
                        new Point(0, this.renderWorld.gsVIEWPORT_WINDOW_START_Y + sScrollYIncrement),
                        ref Region,
                        pSource);

                    // // memset z-buffer
                    for (uiCountY = this.renderWorld.gsVIEWPORT_WINDOW_START_Y; uiCountY < this.renderWorld.gsVIEWPORT_WINDOW_END_Y; uiCountY++)
                    {
                        // memset((int*)gpZBuffer + (uiCountY * 1280) + ((this.renderWorld.gsVIEWPORT_END_X - sScrollXIncrement) * 2), 0, sScrollXIncrement * 2);
                    }
                    for (uiCountY = this.renderWorld.gsVIEWPORT_WINDOW_START_Y + sScrollYIncrement - 1; uiCountY >= this.renderWorld.gsVIEWPORT_WINDOW_START_Y; uiCountY--)
                    {
                        // memset((int*)gpZBuffer + (uiCountY * 1280), 0, 1280);
                    }

                    StripRegions[0].X = (int)(this.renderWorld.gsVIEWPORT_END_X - sScrollXIncrement);
                    StripRegions[1].Height = (int)(this.renderWorld.gsVIEWPORT_WINDOW_START_Y + sScrollYIncrement);
                    StripRegions[1].Width = (int)(this.renderWorld.gsVIEWPORT_END_X - sScrollXIncrement);
                    usNumStrips = 2;

                    usMouseYPos += sScrollYIncrement;
                    usMouseXPos -= sScrollXIncrement;

                    break;

                case ScrollDirection.SCROLL_DOWNLEFT:

                    Region.X = 0;
                    Region.Y = this.renderWorld.gsVIEWPORT_WINDOW_START_Y + sScrollYIncrement;
                    Region.Width = usWidth - (sScrollXIncrement);
                    Region.Height = this.renderWorld.gsVIEWPORT_WINDOW_START_Y + usHeight;

                    this.DrawRegion(
                        pDest,
                        new Point(sScrollXIncrement, this.renderWorld.gsVIEWPORT_WINDOW_START_Y),
                        ref Region,
                        pSource);

                    // // memset z-buffer
                    for (uiCountY = this.renderWorld.gsVIEWPORT_WINDOW_START_Y; uiCountY < this.renderWorld.gsVIEWPORT_WINDOW_END_Y; uiCountY++)
                    {
                        // memset((int*)gpZBuffer + (uiCountY * 1280), 0, sScrollXIncrement * 2);

                    }
                    for (uiCountY = (this.renderWorld.gsVIEWPORT_WINDOW_END_Y - sScrollYIncrement); uiCountY < this.renderWorld.gsVIEWPORT_WINDOW_END_Y; uiCountY++)
                    {
                        // memset((int*)gpZBuffer + (uiCountY * 1280), 0, 1280);
                    }

                    StripRegions[0].Width = (int)(this.renderWorld.gsVIEWPORT_START_X + sScrollXIncrement);

                    StripRegions[1].Y = (int)(this.renderWorld.gsVIEWPORT_WINDOW_END_Y - sScrollYIncrement);
                    StripRegions[1].X = (int)(this.renderWorld.gsVIEWPORT_START_X + sScrollXIncrement);
                    usNumStrips = 2;

                    usMouseYPos -= sScrollYIncrement;
                    usMouseXPos += sScrollXIncrement;

                    break;

                case ScrollDirection.SCROLL_DOWNRIGHT:

                    Region.X = sScrollXIncrement;
                    Region.Y = this.renderWorld.gsVIEWPORT_WINDOW_START_Y + sScrollYIncrement;
                    Region.Width = usWidth;
                    Region.Height = this.renderWorld.gsVIEWPORT_WINDOW_START_Y + usHeight;

                    this.DrawRegion(
                        pDest,
                        new Point(0, this.renderWorld.gsVIEWPORT_WINDOW_START_Y),
                        ref Region,
                        pSource);

                    // // memset z-buffer
                    for (uiCountY = this.renderWorld.gsVIEWPORT_WINDOW_START_Y; uiCountY < this.renderWorld.gsVIEWPORT_WINDOW_END_Y; uiCountY++)
                    {
                        // memset((int*)gpZBuffer + (uiCountY * 1280) + ((this.renderWorld.gsVIEWPORT_END_X - sScrollXIncrement) * 2), 0, sScrollXIncrement * 2);
                    }
                    for (uiCountY = (this.renderWorld.gsVIEWPORT_WINDOW_END_Y - sScrollYIncrement); uiCountY < this.renderWorld.gsVIEWPORT_WINDOW_END_Y; uiCountY++)
                    {
                        // memset((int*)gpZBuffer + (uiCountY * 1280), 0, 1280);
                    }

                    StripRegions[0].X = (int)(this.renderWorld.gsVIEWPORT_END_X - sScrollXIncrement);
                    StripRegions[1].Y = (int)(this.renderWorld.gsVIEWPORT_WINDOW_END_Y - sScrollYIncrement);
                    StripRegions[1].Width = (int)(this.renderWorld.gsVIEWPORT_END_X - sScrollXIncrement);
                    usNumStrips = 2;

                    usMouseYPos -= sScrollYIncrement;
                    usMouseXPos -= sScrollXIncrement;

                    break;
            }

            if (fRenderStrip)
            {

                // Memset to 0
# if SCROLL_TEST
                {
                    DDBLTFX BlitterFX;

                    BlitterFX.dwSize = sizeof(DDBLTFX);
                    BlitterFX.dwFillColor = 0;

                    DDBltSurface((LPDIRectangleDRAWSURFACE2)pDest, NULL, NULL, NULL, DDBLT_COLORFILL, &BlitterFX);
                }
#endif


                for (cnt = 0; cnt < usNumStrips; cnt++)
                {
                    this.renderWorld.RenderStaticWorldRect(StripRegions[cnt], true);
                    // Optimize Redundent tiles too!
                    //ExamineZBufferRect( (int)StripRegions[ cnt ].X, (int)StripRegions[ cnt ].Y, (int)StripRegions[ cnt ].Width, (int)StripRegions[ cnt ].Height );

                    this.DrawRegion(
                        pDest,
                        new Point(StripRegions[cnt].X, StripRegions[cnt].Y),
                        ref StripRegions[cnt],
                        this.gpFrameBuffer);
                }

                sShiftX = 0;
                sShiftY = 0;

                switch (uiDirection)
                {
                    case ScrollDirection.SCROLL_LEFT:

                        sShiftX = sScrollXIncrement;
                        sShiftY = 0;
                        break;

                    case ScrollDirection.SCROLL_RIGHT:

                        sShiftX = -sScrollXIncrement;
                        sShiftY = 0;
                        break;

                    case ScrollDirection.SCROLL_UP:

                        sShiftX = 0;
                        sShiftY = sScrollYIncrement;
                        break;

                    case ScrollDirection.SCROLL_DOWN:

                        sShiftX = 0;
                        sShiftY = -sScrollYIncrement;
                        break;

                    case ScrollDirection.SCROLL_UPLEFT:

                        sShiftX = sScrollXIncrement;
                        sShiftY = sScrollYIncrement;
                        break;

                    case ScrollDirection.SCROLL_UPRIGHT:

                        sShiftX = -sScrollXIncrement;
                        sShiftY = sScrollYIncrement;
                        break;

                    case ScrollDirection.SCROLL_DOWNLEFT:

                        sShiftX = sScrollXIncrement;
                        sShiftY = -sScrollYIncrement;
                        break;

                    case ScrollDirection.SCROLL_DOWNRIGHT:

                        sShiftX = -sScrollXIncrement;
                        sShiftY = -sScrollYIncrement;
                        break;
                }

                // RESTORE SHIFTED
                this.RestoreShiftedVideoOverlays(sShiftX, sShiftY);

                // SAVE NEW
                this.SaveVideoOverlaysArea(VideoSurfaceManager.BACKBUFFER);

                // BLIT NEW
                this.ExecuteVideoOverlaysToAlternateBuffer(VideoSurfaceManager.BACKBUFFER);


#if false

		// Erase mouse from old position
		if (gMouseCursorBackground[ uiCurrentMouseBackbuffer ].fRestore == TRUE )
		{

			do
			{
				ReturnCode = IDirectDrawSurface2_SGPBltFast(gpBackBuffer, usMouseXPos, usMouseYPos, gMouseCursorBackground[uiCurrentMouseBackbuffer].pSurface, (LPRectangle)&MouseRegion, DDBLTFAST_NOCOLORKEY);
				if ((ReturnCode != DD_OK)&&(ReturnCode != DDERR_WASSTILLDRAWING))
				{
					DirectXAttempt ( ReturnCode, __LINE__, __FILE__ );

					if (ReturnCode == DDERR_SURFACELOST)
					{

					}
				}
			} while (ReturnCode != DD_OK);
		}

#endif

            }

            //InvalidateRegion( sLeftDraw, sTopDraw, sRightDraw, sBottomDraw );

            //UpdateSaveBuffer();
            //SaveBackgroundRects();
        }

        private void RestoreShiftedVideoOverlays(int sShiftX, int sShiftY)
        {
        }

        private void SaveVideoOverlaysArea(uint bACKBUFFER)
        {
        }

        private void ExecuteVideoOverlaysToAlternateBuffer(uint bACKBUFFER)
        {
        }

        private void GetCurrentVideoSettings(out int usWidth, out int usHeight, out int ubBitDepth)
        {
            usWidth = 0;
            usHeight = 0;
            ubBitDepth = 0;
        }

        public void Dispose()
        {
            this.graphicDevice.WaitForIdle();
            (this.factory as DisposeCollectorResourceFactory)!.DisposeCollector.DisposeAll();
            this.graphicDevice.Dispose();

            GC.SuppressFinalize(this);
        }
    }

    public enum BufferState
    {
        READY = 0x00,
        BUSY = 0x01,
        DIRTY = 0x02,
        DISABLED = 0x03,
    }

    public enum VideoManagerState
    {
        Off = 0x00,
        On = 0x01,
        ShuttingDown = 0x02,
        Suspended = 0x04,
    }

    public enum ThreadState
    {
        Unknown = 0,
        Off,
        On,
        Suspended,
    }
}
