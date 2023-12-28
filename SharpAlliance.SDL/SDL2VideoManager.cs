using System;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using SDL2;
using SharpAlliance.Core.Interfaces;
using SharpAlliance.Core.Managers.Image;
using SharpAlliance.Core.Managers.VideoSurfaces;
using SharpAlliance.Core.Screens;
using SharpAlliance.Core.SubSystems;
using SharpAlliance.Platform;
using SharpAlliance.Platform.Interfaces;
using SixLabors.ImageSharp.Drawing.Processing;
using static SharpAlliance.Core.Globals;
using FontStyle = SharpAlliance.Core.SubSystems.FontStyle;
using Point = SixLabors.ImageSharp.Point;
using Rectangle = SixLabors.ImageSharp.Rectangle;

namespace SharpAlliance.Core.Managers;

public class SDL2VideoManager : IVideoManager
{
    // TODO: These are temporary...
    const int DD_OK = 0;
    const int DDBLTFAST_NOCOLORKEY = 0x00000000;
    const int DDBLTFAST_SRCCOLORKEY = 0x00000001;
    const int DDBLTFAST_DESTCOLORKEY = 0x00000002;
    const int DDBLTFAST_WAIT = 0x00000010;
    const int DDERR_WASSTILLDRAWING = 0x8700000; // not real value
    const int DDERR_SURFACELOST = 0x9700000;
    private readonly ILogger<SDL2VideoManager> logger;

    public nint Renderer { get; private set; }

    private bool clearScreen;

    // private readonly WindowsSubSystem windows;
    private readonly RenderWorld renderWorld;
    private readonly ScreenManager screenManager;
    private readonly GameContext context;
    private readonly IFileManager files;
    private readonly ITextureManager textures;
    private readonly MouseCursorBackground[] mouseCursorBackground = new MouseCursorBackground[2];

    private static IWindow window;
    private uint WindowID;

    public IWindow Window { get => window; }
    private float _ticks;

    private bool _colorSrgb = true;

    private FadeScreen? fadeScreen;

    private Rgba32 clearColor = new(1.0f, 0, 0.2f, 1f);

    private Rectangle rcWindow;

    //void (* gpFrameBufferRefreshOverride) ();

    ///////////////////////////////////////////////////////////////////////////////////////////////////
    //
    // External Variables
    //
    ///////////////////////////////////////////////////////////////////////////////////////////////////

    ushort gusRedMask = 63488;
    ushort gusGreenMask = 2016;
    ushort gusBlueMask = 31;
    short gusRedShift = 8;
    short gusBlueShift = -3;
    short gusGreenShift = 3;
    //
    // Direct Draw objects for both the Primary and Backbuffer surfaces
    //

    // private LPDIRectangleDRAW? _gpDirectDrawObject = null;
    // private LPDIRectangleDRAW2 gpDirectDrawObject = null;

    //private Surface? _gpPrimarySurface = null;
    //private Surface2? gpPrimarySurface = null;
    //private Surface2? gpBackBuffer = null

    public ISurfaceManager Surfaces { get; }

    public bool IsInitialized { get; private set; }
    public uint guiBOTTOMPANEL { get; set; }
    public uint guiRIGHTPANEL { get; set; }
    //    public uint guiRENDERBUFFER { get; set; }
    //    public uint guiSAVEBUFFER { get; set; }
    //    public uint guiEXTRABUFFER { get; set; }
    public bool gfExtraBuffer { get; set; }
    public int gbPixelDepth { get; }
    public bool LimitPollRate { get; private set; }
    public double PollIntervalInMs { get; private set; }

    private const int SCREEN_WIDTH = 640;
    private const int SCREEN_HEIGHT = 480;
    private const int PIXEL_DEPTH = 16;

    ThreadState uiRefreshThreadState;
    int uiIndex;

    bool fShowMouse;
    Rectangle Region;
    Point MousePos;
    bool fFirstTime = true;
    private bool windowResized;

    private Dictionary<string, HVOBJECT> loadedObjects = [];

    public SDL2VideoManager(
        ILogger<SDL2VideoManager> logger,
        GameContext context,
        ITextureManager textureManager,
        IFileManager fileManager,
        RenderWorld renderWorld,
        IScreenManager screenManager,
        SurfaceManager surfaceManager,
        Shading shading)
    {
        this.textures = textureManager;
        this.Surfaces = surfaceManager;
        this.logger = logger;
        this.context = context;
        this.files = fileManager;
        this.renderWorld = renderWorld;
        this.screenManager = (screenManager as ScreenManager)!;

        Configuration.Default.MemoryAllocator = new SixLabors.ImageSharp.Memory.SimpleGcMemoryAllocator();
    }

    public async ValueTask<bool> Initialize()
    {
        WindowCreateInfo windowCI = new()
        {
            X = 50,
            Y = 50,
            WindowWidth = 640,
            WindowHeight = 480,
            WindowInitialState = WindowState.Normal,
            WindowTitle = "Sharp Alliance!!!",
        };

        SDL.SDL_WindowFlags GetWindowFlags(WindowState state)
            => state switch
            {
                WindowState.Normal => 0,
                WindowState.FullScreen => SDL.SDL_WindowFlags.SDL_WINDOW_FULLSCREEN,
                WindowState.Maximized => SDL.SDL_WindowFlags.SDL_WINDOW_MAXIMIZED,
                WindowState.Minimized => SDL.SDL_WindowFlags.SDL_WINDOW_MINIMIZED,
                WindowState.BorderlessFullScreen => SDL.SDL_WindowFlags.SDL_WINDOW_FULLSCREEN_DESKTOP,
                WindowState.Hidden => SDL.SDL_WindowFlags.SDL_WINDOW_HIDDEN,
                _ => throw new Exception("Invalid WindowState: " + state),
            };

        SDL.SDL_WindowFlags flags =
            SDL.SDL_WindowFlags.SDL_WINDOW_OPENGL
            | SDL.SDL_WindowFlags.SDL_WINDOW_RESIZABLE
            | SDL.SDL_WindowFlags.SDL_WINDOW_SHOWN
            | GetWindowFlags(windowCI.WindowInitialState);

        if (windowCI.WindowInitialState != WindowState.Hidden)
        {
            flags |= SDL.SDL_WindowFlags.SDL_WINDOW_SHOWN;
        }

        if (SDL.SDL_Init(SDL.SDL_INIT_VIDEO) < 0)
        {
            Console.WriteLine($"There was an issue initilizing SDL. {SDL.SDL_GetError()}");
        }

        window = new Sdl2Window(
            windowCI.WindowTitle,
            windowCI.X,
            windowCI.Y,
            windowCI.WindowWidth,
            windowCI.WindowHeight,
            flags,
            threadedProcessing: true);

        this.Renderer = SDL.SDL_CreateRenderer(
            (window as Sdl2Window).Handle,
            -1,
            SDL.SDL_RendererFlags.SDL_RENDERER_ACCELERATED |
            SDL.SDL_RendererFlags.SDL_RENDERER_PRESENTVSYNC);

        if (this.Renderer == IntPtr.Zero)
        {
            Console.WriteLine($"There was an issue creating the renderer. {SDL.SDL_GetError()}");
        }

        this.Surfaces.InitializeSurfaces(this.Renderer, SCREEN_WIDTH, SCREEN_HEIGHT);

        //        Window.Resized += () => windowResized = true;
        //Window.PollIntervalInMs = 1000 / 30;

        Globals.guiFrameBufferState = BufferState.DIRTY;
        Globals.guiMouseBufferState = BufferState.DISABLED;
        Globals.guiVideoManagerState = VideoManagerState.On;
        Globals.guiRefreshThreadState = ThreadState.Off;
        Globals.guiDirtyRegionCount = 0;
        Globals.gfForceFullScreenRefresh = true;
        Globals.gpFrameBufferRefreshOverride = null;
        //gpCursorStore = null;
        Globals.gfPrintFrameBuffer = false;
        Globals.guiPrintFrameBufferIndex = 0;

        //backBuffer = new(SCREEN_WIDTH, SCREEN_HEIGHT);
        //        backBuffer = new ImageSharpTexture(new Image<Rgba32>(SCREEN_WIDTH, SCREEN_HEIGHT), mipmap: false)
        //            .CreateDeviceTexture(GraphicDevice, GraphicDevice.ResourceFactory);
        //
        // fadeScreen = (screenManager.GetScreen(ScreenNames.FADE_SCREEN, activate: true).AsTask().Result as FadeScreen)!;
        this.IsInitialized = await this.files.Initialize();

        return this.IsInitialized;
    }

    /**********************************************************************************************
	Blt16BPPTo16BPP

	Copies a rect of 16 bit data from a video buffer to a buffer position of the brush
	in the data area, for later blitting. Used to copy background information for mercs
	etc. to their unblit buffer, for later reblitting. Does NOT clip.

**********************************************************************************************/
    public bool Blt16BPPTo16BPP(Image<Rgba32> pDest, Image<Rgba32> pSrc, Point iDestPos, Point iSrcPos, int uiWidth, int uiHeight)
    {
        Rectangle destRect = new(iDestPos.X, iDestPos.Y, uiWidth, uiHeight);

        //pDest.SaveAsPng($@"C:\temp\{nameof(Blt16BPPTo16BPP)}-dst.png");
        //pSrc.SaveAsPng($@"C:\temp\{nameof(Blt16BPPTo16BPP)}-src.png");

        pDest.Mutate(ctx =>
        {
            ctx.DrawImage(pSrc, iDestPos, destRect, 1.0f);
        });

        //pDest.SaveAsPng($@"C:\temp\{nameof(Blt16BPPTo16BPP)}-dst-after.png");
        //pSrc.SaveAsPng($@"C:\temp\{nameof(Blt16BPPTo16BPP)}-src-after.png");

        return true;
    }

    public void SetClippingRegionAndImageWidth(int iImageWidth, Rectangle iClip)
    {
        Globals.giImageWidth = iImageWidth;
        Globals.giClip = new(iClip.X, iClip.Y, iClip.Width, iClip.Height);
    }


    public unsafe void DrawFrame()
    {
        if (Globals.gfForceFullScreenRefresh || this.clearScreen)
        {
            this.clearScreen = false;
        }

        if (SDL.SDL_SetRenderDrawColor(this.Renderer, 135, 206, 235, 255) < 0)
        {
            var error = SDL.SDL_GetError();
        }

        // Clears the current render surface.
        // if (SDL.SDL_RenderClear(Renderer) < 0)
        // {
        //     Console.WriteLine($"There was an issue with clearing the render surface. {SDL.SDL_GetError()}");
        // }

        ScreenManager.Draw(this);
        MouseSubSystem.Draw(this);

        var bb = this.Surfaces.SurfaceByTypes[SurfaceType.PRIMARY_SURFACE];
        //bb.Image.SaveAsPng(@"C:\temp\test.png");

        if (SDL.SDL_UpdateTexture(bb.Pointer, 0, (nint)bb.Handle.Pointer, 4 * bb.Image.Width) < 0)
        {
            var error = SDL.SDL_GetError();
        }

        if (SDL.SDL_RenderCopy(this.Renderer, bb.Pointer, 0, 0) < 0)
        {
            var error = SDL.SDL_GetError();
        }

        SDL.SDL_RenderPresent(this.Renderer);
    }

    public void Draw()
    {
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
        => typeof(SDL2VideoManager).Assembly.GetManifestResourceStream(name)!;

    public HVOBJECT GetVideoObject(string assetPath) => this.GetVideoObject(assetPath, out var _);

    public HVOBJECT GetVideoObject(string assetPath, out string key) => this.AddVideoObject(assetPath, out key);

    private HVOBJECT AddVideoObject(string assetPath, out string key)
    {
        key = assetPath;

        if (!this.loadedObjects.TryGetValue(key, out var hvObj))
        {
            hvObj = this.textures.LoadImage(assetPath);

            //            if (hvObj.Images is null)
            //            {
            //                hvObj.Surfaces = this.CreateSurfaces(Renderer, hvObj.Images);
            //
            //                List<Texture> textures = [];
            //
            //                foreach (var surface in hvObj.Surfaces)
            //                {
            //                    var tex = this.Surfaces.CreateTextureFromSurface(Renderer, surface);
            //                    textures.Add(tex);
            //                }
            //
            //                hvObj.Textures = [.. textures];
            //            }

            this.loadedObjects.Add(assetPath, hvObj);
            Console.WriteLine($"{nameof(GetVideoObject)}: {assetPath}");
        }

        return hvObj;
    }

    private Rectangle fullScreenRect = new(0, 0, SCREEN_WIDTH, SCREEN_HEIGHT);
    private bool _exists;
    private bool _shouldClose;

    public void RefreshScreen()
    {
        int usScreenWidth;
        int usScreenHeight;
        long uiTime;

        usScreenWidth = usScreenHeight = 0;

        if (this.fFirstTime)
        {
            this.fShowMouse = false;
        }

        this.logger.LogDebug(LoggingEventId.VIDEO, "Looping in refresh");

        ///////////////////////////////////////////////////////////////////////////////////////////////
        // 
        // REFRESH_THREAD_MUTEX 
        //
        ///////////////////////////////////////////////////////////////////////////////////////////////

        switch (Globals.guiVideoManagerState)
        {
            case VideoManagerState.On:
                // Excellent, everything is cosher, we continue on
                this.uiRefreshThreadState = ThreadState.On;
                Globals.guiRefreshThreadState = ThreadState.On;
                usScreenWidth = Globals.gusScreenWidth;
                usScreenHeight = Globals.gusScreenHeight;
                break;
            case VideoManagerState.Off:
                // Hot damn, the video manager is suddenly off. We have to bugger out of here. Don't forget to
                // leave the critical section
                Globals.guiRefreshThreadState = ThreadState.Off;
                return;
            case VideoManagerState.Suspended:
                // This are suspended. Make sure the refresh function does try to access any of the direct
                // draw surfaces
                this.uiRefreshThreadState = Globals.guiRefreshThreadState = ThreadState.Suspended;
                break;
            case VideoManagerState.ShuttingDown:
                // Well things are shutting down. So we need to bugger out of there. Don't forget to leave the
                // critical section before returning
                Globals.guiRefreshThreadState = ThreadState.Off;
                return;
        }

        //
        // Get the current mouse position
        //

        // inputs.GetCursorPosition(out var tmpMousePos);
        // MousePos = new Point(tmpMousePos.X, tmpMousePos.Y);

        /////////////////////////////////////////////////////////////////////////////////////////////
        // 
        // FRAME_BUFFER_MUTEX 
        //
        /////////////////////////////////////////////////////////////////////////////////////////////

        // RESTORE OLD POSITION OF MOUSE
        if (this.mouseCursorBackground[Globals.CURRENT_MOUSE_DATA].fRestore == true)
        {
            this.Region.X = this.mouseCursorBackground[Globals.CURRENT_MOUSE_DATA].usLeft;
            this.Region.Y = this.mouseCursorBackground[Globals.CURRENT_MOUSE_DATA].usTop;
            this.Region.Width = this.mouseCursorBackground[Globals.CURRENT_MOUSE_DATA].usRight;
            this.Region.Height = this.mouseCursorBackground[Globals.CURRENT_MOUSE_DATA].usBottom;

            //            MouseSubSystem.Draw(
            //                mouseCursorBackground[Globals.CURRENT_MOUSE_DATA],
            //                Region,
            //                GraphicDevice,
            //                commandList);

            // Save position into other background region
            this.mouseCursorBackground[Globals.PREVIOUS_MOUSE_DATA] = this.mouseCursorBackground[Globals.CURRENT_MOUSE_DATA];
        }

        // Ok we were able to get a hold of the frame buffer stuff. Check to see if it needs updating
        // if not, release the frame buffer stuff right away
        if (Globals.guiFrameBufferState == BufferState.DIRTY)
        {
            // Well the frame buffer is dirty.
            if (Globals.gpFrameBufferRefreshOverride != null)
            {
                // Method (3) - We are using a function override to refresh the frame buffer. First we
                // call the override function then we must set the override pointer to null
                Globals.gpFrameBufferRefreshOverride();
                Globals.gpFrameBufferRefreshOverride = null;
            }

            if (this.fadeScreen?.gfFadeInitialized ?? false
                && this.fadeScreen.gfFadeInVideo)
            {
                this.fadeScreen!.gFadeFunction();
            }
            else
            {
                // Either Method (1) or (2)
                if (Globals.gfForceFullScreenRefresh == true)
                {
                    // Method (1) - We will be refreshing the entire screen
                    this.Region.X = 0;
                    this.Region.Y = 0;
                    this.Region.Width = usScreenWidth;
                    this.Region.Height = usScreenHeight;

                    this.BlitRegion(
                        this.Surfaces[SurfaceType.BACKBUFFER],
                        new Point(0, 0),
                        this.Region,
                        this.Surfaces[SurfaceType.FRAME_BUFFER]);
                }
                else
                {
                    for (this.uiIndex = 0; this.uiIndex < Globals.guiDirtyRegionCount; this.uiIndex++)
                    {
                        this.Region.X = Globals.gListOfDirtyRegions[this.uiIndex].Left;
                        this.Region.Y = Globals.gListOfDirtyRegions[this.uiIndex].Top;
                        this.Region.Width = Globals.gListOfDirtyRegions[this.uiIndex].Width;
                        this.Region.Height = Globals.gListOfDirtyRegions[this.uiIndex].Height;

                        this.BlitRegion(
                            this.Surfaces[SurfaceType.BACKBUFFER],
                            new Point(this.Region.X, this.Region.Y),
                            this.Region,
                            this.Surfaces[SurfaceType.FRAME_BUFFER]);
                    }

                    // Now do new, extended dirty regions
                    for (this.uiIndex = 0; this.uiIndex < Globals.guiDirtyRegionExCount; this.uiIndex++)
                    {
                        this.Region = Globals.gDirtyRegionsEx[this.uiIndex];

                        // Do some checks if we are in the process of scrolling!	
                        if (Globals.gfRenderScroll)
                        {
                            // Check if we are completely out of bounds
                            if (this.Region.Y <= Globals.gsVIEWPORT_WINDOW_END_Y
                                && this.Region.Height <= Globals.gsVIEWPORT_WINDOW_END_Y)
                            {
                                continue;
                            }
                        }

                        this.BlitRegion(
                            this.Surfaces[SurfaceType.BACKBUFFER],
                            this.Region.ToPoint(),
                            this.Region,
                            this.Surfaces[SurfaceType.FRAME_BUFFER]);
                    }
                }
            }

            if (Globals.gfRenderScroll)
            {
                this.ScrollJA2Background(
                    Globals.guiScrollDirection,
                    Globals.gsScrollXIncrement,
                    Globals.gsScrollYIncrement,
                    this.Surfaces[SurfaceType.PRIMARY_SURFACE],
                    this.Surfaces[SurfaceType.BACKBUFFER],
                    fRenderStrip: true,
                    Globals.PREVIOUS_MOUSE_DATA);
            }

            Globals.gfIgnoreScrollDueToCenterAdjust = false;

            // Update the guiFrameBufferState variable to reflect that the frame buffer can now be handled
            Globals.guiFrameBufferState = BufferState.READY;
        }

        // Do we want to print the frame stuff ??
        if (Globals.gfVideoCapture)
        {
            uiTime = ClockManager.GetTickCount();
            if ((uiTime < Globals.guiLastFrame) || (uiTime > (Globals.guiLastFrame + Globals.guiFramePeriod)))
            {
                //SnapshotSmall();
                Globals.guiLastFrame = uiTime;
            }
        }

        if (Globals.gfPrintFrameBuffer == true)
        {
            //FileStream? OutputFile;
            //string? FileName;
            //int iIndex;
            //string? ExecDir;
            //int[] p16BPPData;
            //
            ////GetExecutableDirectory(ExecDir);
            ////SetFileManCurrentDirectory(ExecDir);
            //
            //// Create temporary system memory surface. This is used to correct problems with the backbuffer
            //// surface which can be interlaced or have a funky pitch
            //Image<Rgba32> _pTmpBuffer = new(usScreenWidth, usScreenHeight);
            //
            //Image<Rgba32> pTmpBuffer = new(usScreenWidth, usScreenHeight);
            //
            //// Copy the primary surface to the temporary surface
            //Region.X = 0;
            //Region.Y = 0;
            //Region.Width = usScreenWidth;
            //Region.Height = usScreenHeight;
            //
            //BlitRegion(
            //    pTmpBuffer,
            //    new Point(0, 0),
            //    Region,
            //    gpPrimarySurface);

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
            Globals.gfPrintFrameBuffer = false;

            //_pTmpBuffer?.Dispose();
            //pTmpBuffer?.Dispose();

            //strcat(ExecDir, "\\Data");
            //SetFileManCurrentDirectory(ExecDir);
        }

        // Ok we were able to get a hold of the frame buffer stuff. Check to see if it needs updating
        // if not, release the frame buffer stuff right away
        if (Globals.guiMouseBufferState == BufferState.DIRTY)
        {
            // Well the mouse buffer is dirty. Upload the whole thing
            this.Region.X = 0;
            this.Region.Y = 0;
            this.Region.Width = Globals.gusMouseCursorWidth;
            this.Region.Height = Globals.gusMouseCursorHeight;

            this.BlitRegion(
                Globals.gpMouseCursor,
                new Point(0, 0),
                this.Region,
                Globals.gpMouseCursorOriginal);

            Globals.guiMouseBufferState = BufferState.READY;
        }

        // Check current state of the mouse cursor
        if (this.fShowMouse == false)
        {
            this.fShowMouse = Globals.guiMouseBufferState == BufferState.READY;
        }
        else
        {
            this.fShowMouse = Globals.guiMouseBufferState == BufferState.DISABLED;
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

        if (this.fShowMouse == true)
        {
            // Step (1) - Save mouse background
            this.Region.X = this.MousePos.X - Globals.gsMouseCursorXOffset;
            this.Region.Y = this.MousePos.Y - Globals.gsMouseCursorYOffset;
            this.Region.Width = this.Region.X + Globals.gusMouseCursorWidth;
            this.Region.Height = this.Region.Y + Globals.gusMouseCursorHeight;

            if (this.Region.Width > usScreenWidth)
            {
                this.Region.Width = usScreenWidth;
            }

            if (this.Region.Height > usScreenHeight)
            {
                this.Region.Height = usScreenHeight;
            }

            if ((this.Region.Width > this.Region.X) && (this.Region.Height > this.Region.Y))
            {
                // Make sure the mouse background is marked for restore and coordinates are saved for the
                // future restore
                this.mouseCursorBackground[Globals.CURRENT_MOUSE_DATA].fRestore = true;
                this.mouseCursorBackground[Globals.CURRENT_MOUSE_DATA].usRight = this.Region.Width - this.Region.X;
                this.mouseCursorBackground[Globals.CURRENT_MOUSE_DATA].usBottom = this.Region.Height - this.Region.Y;

                if (this.Region.X < 0)
                {
                    this.mouseCursorBackground[Globals.CURRENT_MOUSE_DATA].usLeft = (0 - this.Region.X);
                    this.mouseCursorBackground[Globals.CURRENT_MOUSE_DATA].usMouseXPos = 0;
                    this.Region.X = 0;
                }
                else
                {
                    this.mouseCursorBackground[Globals.CURRENT_MOUSE_DATA].usMouseXPos = this.MousePos.X - Globals.gsMouseCursorXOffset;
                    this.mouseCursorBackground[Globals.CURRENT_MOUSE_DATA].usLeft = 0;
                }

                if (this.Region.Y < 0)
                {
                    this.mouseCursorBackground[Globals.CURRENT_MOUSE_DATA].usMouseYPos = 0;
                    this.mouseCursorBackground[Globals.CURRENT_MOUSE_DATA].usTop = (0 - this.Region.Y);
                    this.Region.Y = 0;
                }
                else
                {
                    this.mouseCursorBackground[Globals.CURRENT_MOUSE_DATA].usMouseYPos = this.MousePos.Y - Globals.gsMouseCursorYOffset;
                    this.mouseCursorBackground[Globals.CURRENT_MOUSE_DATA].usTop = 0;
                }

                if ((this.Region.Width > this.Region.X) && (this.Region.Height > this.Region.Y))
                {
                    // Save clipped region
                    this.mouseCursorBackground[Globals.CURRENT_MOUSE_DATA].Region = this.Region;

                    // Ok, do the actual data save to the mouse background
                    //BlitRegion(
                    //    mouseCursorBackground[Globals.CURRENT_MOUSE_DATA].pSurface,
                    //    new Point(
                    //        mouseCursorBackground[Globals.CURRENT_MOUSE_DATA].usLeft,
                    //        mouseCursorBackground[Globals.CURRENT_MOUSE_DATA].usTop),
                    //    Region,
                    //    backBuffer);

                    // Step (2) - Blit mouse cursor to back buffer
                    this.Region.X = this.mouseCursorBackground[Globals.CURRENT_MOUSE_DATA].usLeft;
                    this.Region.Y = this.mouseCursorBackground[Globals.CURRENT_MOUSE_DATA].usTop;
                    this.Region.Width = this.mouseCursorBackground[Globals.CURRENT_MOUSE_DATA].usRight;
                    this.Region.Height = this.mouseCursorBackground[Globals.CURRENT_MOUSE_DATA].usBottom;

                    //BlitRegion(
                    //    backBuffer,
                    //    new Point(
                    //        mouseCursorBackground[Globals.CURRENT_MOUSE_DATA].usMouseXPos,
                    //        mouseCursorBackground[Globals.CURRENT_MOUSE_DATA].usMouseYPos),
                    //    Region,
                    //    mouse.gpMouseCursor);
                }
                else
                {
                    // Hum, the mouse was not blitted this round. Henceforth we will flag fRestore as false
                    this.mouseCursorBackground[Globals.CURRENT_MOUSE_DATA].fRestore = false;
                }
            }
            else
            {
                // Hum, the mouse was not blitted this round. Henceforth we will flag fRestore as false
                this.mouseCursorBackground[Globals.CURRENT_MOUSE_DATA].fRestore = false;
            }
        }
        else
        {
            // Well since there was no mouse handling this round, we disable the mouse restore
            this.mouseCursorBackground[Globals.CURRENT_MOUSE_DATA].fRestore = false;
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

        this.BlitRegion(
            this.Surfaces[SurfaceType.PRIMARY_SURFACE],
            this.rcWindow.ToPoint(),
            this.fullScreenRect,
            this.Surfaces[SurfaceType.BACKBUFFER]);

        // Step (2) - Copy Primary Surface to the Back Buffer
        if (Globals.gfRenderScroll)
        {
            this.Region.X = 0;
            this.Region.Y = 0;
            this.Region.Width = 640;
            this.Region.Height = 360;

            this.BlitRegion(
                this.Surfaces[SurfaceType.BACKBUFFER],
                new Point(0, 0),
                this.Region,
                this.Surfaces[SurfaceType.PRIMARY_SURFACE]);

            // Get new background for mouse
            // Ok, do the actual data save to the mouse background
            Globals.gfRenderScroll = false;
            Globals.gfScrollStart = false;
        }

        // COPY MOUSE AREAS FROM PRIMARY BACK!

        // FIRST OLD ERASED POSITION
        if (this.mouseCursorBackground[Globals.PREVIOUS_MOUSE_DATA].fRestore == true)
        {
            this.Region = this.mouseCursorBackground[Globals.PREVIOUS_MOUSE_DATA].Region;

            this.BlitRegion(
                this.Surfaces[SurfaceType.BACKBUFFER],
                new Point(
                    this.mouseCursorBackground[Globals.PREVIOUS_MOUSE_DATA].usMouseXPos,
                    this.mouseCursorBackground[Globals.PREVIOUS_MOUSE_DATA].usMouseYPos),
                this.Region,
                this.Surfaces[SurfaceType.PRIMARY_SURFACE]);
        }

        // NOW NEW MOUSE AREA
        if (this.mouseCursorBackground[Globals.CURRENT_MOUSE_DATA].fRestore == true)
        {
            this.Region = this.mouseCursorBackground[Globals.CURRENT_MOUSE_DATA].Region;

            this.BlitRegion(
                this.Surfaces[SurfaceType.BACKBUFFER],
                new Point(
                    this.mouseCursorBackground[Globals.CURRENT_MOUSE_DATA].usMouseXPos,
                    this.mouseCursorBackground[Globals.CURRENT_MOUSE_DATA].usMouseYPos),
                this.Region,
                this.Surfaces[SurfaceType.PRIMARY_SURFACE]);
        }

        if (Globals.gfForceFullScreenRefresh == true)
        {
            // Method (1) - We will be refreshing the entire screen
            this.Region.X = 0;
            this.Region.Y = 0;
            this.Region.Width = SCREEN_WIDTH;
            this.Region.Height = SCREEN_HEIGHT;

            this.BlitRegion(
                this.Surfaces[SurfaceType.BACKBUFFER],
                new Point(0, 0),
                this.Region,
                this.Surfaces[SurfaceType.PRIMARY_SURFACE]);

            Globals.guiDirtyRegionCount = 0;
            Globals.guiDirtyRegionExCount = 0;
            Globals.gfForceFullScreenRefresh = false;
        }
        else
        {
            for (this.uiIndex = 0; this.uiIndex < Globals.guiDirtyRegionCount; this.uiIndex++)
            {
                this.Region.X = Globals.gListOfDirtyRegions[this.uiIndex].Left;
                this.Region.Y = Globals.gListOfDirtyRegions[this.uiIndex].Top;
                this.Region.Width = Globals.gListOfDirtyRegions[this.uiIndex].Width;
                this.Region.Height = Globals.gListOfDirtyRegions[this.uiIndex].Height;

                this.BlitRegion(
                    this.Surfaces[SurfaceType.BACKBUFFER],
                    new Point(this.Region.X, this.Region.Y),
                    this.Region,
                    this.Surfaces[SurfaceType.PRIMARY_SURFACE]);
            }

            Globals.guiDirtyRegionCount = 0;
            Globals.gfForceFullScreenRefresh = false;
        }

        // Do extended dirty regions!
        for (this.uiIndex = 0; this.uiIndex < Globals.guiDirtyRegionExCount; this.uiIndex++)
        {
            this.Region.X = Globals.gDirtyRegionsEx[this.uiIndex].Left;
            this.Region.Y = Globals.gDirtyRegionsEx[this.uiIndex].Top;
            this.Region.Width = Globals.gDirtyRegionsEx[this.uiIndex].Width;
            this.Region.Height = Globals.gDirtyRegionsEx[this.uiIndex].Height;

            if ((this.Region.Y < Globals.gsVIEWPORT_WINDOW_END_Y)
                && Globals.gfRenderScroll)
            {
                continue;
            }

            this.BlitRegion(
                this.Surfaces[SurfaceType.BACKBUFFER],
                new Point(this.Region.X, this.Region.Y),
                this.Region,
                this.Surfaces[SurfaceType.PRIMARY_SURFACE]);
        }

        Globals.guiDirtyRegionExCount = 0;

        this.fFirstTime = false;
    }

    private void DrawRegion(
        Image<Rgba32> destinationTexture,
        int destinationPointX,
        int destinationPointY,
        Rectangle sourceRegion,
        Image<Rgba32> sourceTexture)
        => this.BlitRegion(
            destinationTexture,
            new Point(destinationPointX, destinationPointY),
            sourceRegion,
            sourceTexture);

    private void BlitRegion(
        Image<Rgba32> dstImage,
        Point destinationPoint,
        Rectangle sourceRegion,
        Image<Rgba32> srcImage)
    {
        var finalRect = new Rectangle(
            new Point(destinationPoint.X, destinationPoint.Y),
            new Size(sourceRegion.Width, sourceRegion.Height));

        //       srcImage.SaveAsPng($@"C:\temp\{nameof(BlitRegion)}-srcImage.png");
        //        dstImage.SaveAsPng($@"C:\temp\{nameof(BlitRegion)}-dstImage-before.png");

        try
        {
            dstImage.Mutate(ctx =>
            {
                ctx.DrawImage(
                    srcImage,
                    finalRect,
                    PixelColorBlendingMode.Normal,
                    1.0f);
            });
        }
        catch (Exception e)
        {

        }
        // dstImage.SaveAsPng($@"C:\temp\{nameof(BlitRegion)}-dstImage-after.png");
    }

    private void ScrollJA2Background(
        ScrollDirection uiDirection,
        int sScrollXIncrement,
        int sScrollYIncrement,
        Image<Rgba32> pSource,
        Image<Rgba32> pDest,
        bool fRenderStrip,
        int uiCurrentMouseBackbuffer)
    {
        Rectangle Region = new();
        int usMouseXPos, usMouseYPos;
        Rectangle[] StripRegions = new Rectangle[2];
        Rectangle MouseRegion = new();
        int usNumStrips = 0;
        int cnt;
        int sShiftX, sShiftY;
        int uiCountY;

        this.GetCurrentVideoSettings(out int usWidth, out int usHeight, out int ubBitDepth);
        usHeight = Globals.gsVIEWPORT_WINDOW_END_Y - Globals.gsVIEWPORT_WINDOW_START_Y;

        StripRegions[0].X = Globals.gsVIEWPORT_START_X;
        StripRegions[0].Width = Globals.gsVIEWPORT_END_X;
        StripRegions[0].Y = Globals.gsVIEWPORT_WINDOW_START_Y;
        StripRegions[0].Height = Globals.gsVIEWPORT_WINDOW_END_Y;

        StripRegions[1].X = Globals.gsVIEWPORT_START_X;
        StripRegions[1].Width = Globals.gsVIEWPORT_END_X;
        StripRegions[1].Y = Globals.gsVIEWPORT_WINDOW_START_Y;
        StripRegions[1].Height = Globals.gsVIEWPORT_WINDOW_END_Y;

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
                Region.Y = Globals.gsVIEWPORT_WINDOW_START_Y;
                Region.Width = usWidth - sScrollXIncrement;
                Region.Height = Globals.gsVIEWPORT_WINDOW_START_Y + usHeight;

                this.DrawRegion(
                    pDest,
                    sScrollXIncrement,
                    Globals.gsVIEWPORT_WINDOW_START_Y,
                    Region,
                    pSource);

                // memset z-buffer
                for (uiCountY = Globals.gsVIEWPORT_WINDOW_START_Y; uiCountY < Globals.gsVIEWPORT_WINDOW_END_Y; uiCountY++)
                {
                    // memset((int*)gpZBuffer + (uiCountY * 1280), 0, sScrollXIncrement * 2);
                }

                StripRegions[0].Width = (int)(Globals.gsVIEWPORT_START_X + sScrollXIncrement);
                usMouseXPos += sScrollXIncrement;

                usNumStrips = 1;
                break;

            case ScrollDirection.SCROLL_RIGHT:

                Region.X = sScrollXIncrement;
                Region.Y = Globals.gsVIEWPORT_WINDOW_START_Y;
                Region.Width = usWidth;
                Region.Height = Globals.gsVIEWPORT_WINDOW_START_Y + usHeight;

                this.DrawRegion(
                    pDest,
                    0,
                    Globals.gsVIEWPORT_WINDOW_START_Y,
                    Region,
                    pSource);

                // // memset z-buffer
                for (uiCountY = Globals.gsVIEWPORT_WINDOW_START_Y; uiCountY < Globals.gsVIEWPORT_WINDOW_END_Y; uiCountY++)
                {
                    // memset((int*)gpZBuffer + (uiCountY * 1280) + ((Globals.gsVIEWPORT_END_X - sScrollXIncrement) * 2), 0, sScrollXIncrement * 2);
                }

                //for(uiCountY=0; uiCountY < usHeight; uiCountY++)
                //{
                //	memcpy(pDestBuf+(uiCountY*uiDestPitchBYTES),
                //					pSrcBuf+(uiCountY*uiDestPitchBYTES)+sScrollXIncrement*uiBPP,
                //					uiDestPitchBYTES-sScrollXIncrement*uiBPP);
                //}

                StripRegions[0].X = Globals.gsVIEWPORT_END_X - sScrollXIncrement;
                usMouseXPos -= sScrollXIncrement;

                usNumStrips = 1;
                break;

            case ScrollDirection.SCROLL_UP:

                Region.X = 0;
                Region.Y = Globals.gsVIEWPORT_WINDOW_START_Y;
                Region.Width = usWidth;
                Region.Height = Globals.gsVIEWPORT_WINDOW_START_Y + usHeight - sScrollYIncrement;

                this.DrawRegion(
                    pDest,
                    0,
                    Globals.gsVIEWPORT_WINDOW_START_Y + sScrollYIncrement,
                    Region,
                    pSource);

                for (uiCountY = sScrollYIncrement - 1 + Globals.gsVIEWPORT_WINDOW_START_Y; uiCountY >= Globals.gsVIEWPORT_WINDOW_START_Y; uiCountY--)
                {
                    // memset((int*)gpZBuffer + (uiCountY * 1280), 0, 1280);
                }

                //for(uiCountY=usHeight-1; uiCountY >= sScrollYIncrement; uiCountY--)
                //{
                //	memcpy(pDestBuf+(uiCountY*uiDestPitchBYTES),
                //					pSrcBuf+((uiCountY-sScrollYIncrement)*uiDestPitchBYTES),
                //					uiDestPitchBYTES);
                //}
                StripRegions[0].Height = (int)(Globals.gsVIEWPORT_WINDOW_START_Y + sScrollYIncrement);
                usNumStrips = 1;

                usMouseYPos += sScrollYIncrement;

                break;

            case ScrollDirection.SCROLL_DOWN:

                Region.X = 0;
                Region.Y = Globals.gsVIEWPORT_WINDOW_START_Y + sScrollYIncrement;
                Region.Width = usWidth;
                Region.Height = Globals.gsVIEWPORT_WINDOW_START_Y + usHeight;

                this.DrawRegion(
                    pDest,
                    0,
                    Globals.gsVIEWPORT_WINDOW_START_Y,
                    Region,
                    pSource);

                // Zero out z
                for (uiCountY = Globals.gsVIEWPORT_WINDOW_END_Y - sScrollYIncrement; uiCountY < Globals.gsVIEWPORT_WINDOW_END_Y; uiCountY++)
                {
                    // memset((int*)gpZBuffer + (uiCountY * 1280), 0, 1280);
                }

                //for(uiCountY=0; uiCountY < (usHeight-sScrollYIncrement); uiCountY++)
                //{
                //	memcpy(pDestBuf+(uiCountY*uiDestPitchBYTES),
                //					pSrcBuf+((uiCountY+sScrollYIncrement)*uiDestPitchBYTES),
                //					uiDestPitchBYTES);
                //}

                StripRegions[0].Y = (int)(Globals.gsVIEWPORT_WINDOW_END_Y - sScrollYIncrement);
                usNumStrips = 1;

                usMouseYPos -= sScrollYIncrement;

                break;

            case ScrollDirection.SCROLL_UPLEFT:

                Region.X = 0;
                Region.Y = Globals.gsVIEWPORT_WINDOW_START_Y;
                Region.Width = usWidth - sScrollXIncrement;
                Region.Height = Globals.gsVIEWPORT_WINDOW_START_Y + usHeight - sScrollYIncrement;

                this.DrawRegion(
                    pDest,
                    sScrollXIncrement,
                    Globals.gsVIEWPORT_WINDOW_START_Y + sScrollYIncrement,
                    Region,
                    pSource);

                // // memset z-buffer
                for (uiCountY = Globals.gsVIEWPORT_WINDOW_START_Y; uiCountY < Globals.gsVIEWPORT_WINDOW_END_Y; uiCountY++)
                {
                    // memset((int*)gpZBuffer + (uiCountY * 1280), 0, sScrollXIncrement * 2);

                }
                for (uiCountY = Globals.gsVIEWPORT_WINDOW_START_Y + sScrollYIncrement - 1; uiCountY >= Globals.gsVIEWPORT_WINDOW_START_Y; uiCountY--)
                {
                    // memset((int*)gpZBuffer + (uiCountY * 1280), 0, 1280);
                }

                StripRegions[0].Width = (int)(Globals.gsVIEWPORT_START_X + sScrollXIncrement);
                StripRegions[1].Height = (int)(Globals.gsVIEWPORT_WINDOW_START_Y + sScrollYIncrement);
                StripRegions[1].X = (int)(Globals.gsVIEWPORT_START_X + sScrollXIncrement);
                usNumStrips = 2;

                usMouseYPos += sScrollYIncrement;
                usMouseXPos += sScrollXIncrement;

                break;

            case ScrollDirection.SCROLL_UPRIGHT:

                Region.X = sScrollXIncrement;
                Region.Y = Globals.gsVIEWPORT_WINDOW_START_Y;
                Region.Width = usWidth;
                Region.Height = Globals.gsVIEWPORT_WINDOW_START_Y + usHeight - sScrollYIncrement;

                this.BlitRegion(
                    pDest,
                    new Point(0, Globals.gsVIEWPORT_WINDOW_START_Y + sScrollYIncrement),
                    Region,
                    pSource);

                // // memset z-buffer
                for (uiCountY = Globals.gsVIEWPORT_WINDOW_START_Y; uiCountY < Globals.gsVIEWPORT_WINDOW_END_Y; uiCountY++)
                {
                    // memset((int*)gpZBuffer + (uiCountY * 1280) + ((Globals.gsVIEWPORT_END_X - sScrollXIncrement) * 2), 0, sScrollXIncrement * 2);
                }
                for (uiCountY = Globals.gsVIEWPORT_WINDOW_START_Y + sScrollYIncrement - 1; uiCountY >= Globals.gsVIEWPORT_WINDOW_START_Y; uiCountY--)
                {
                    // memset((int*)gpZBuffer + (uiCountY * 1280), 0, 1280);
                }

                StripRegions[0].X = (int)(Globals.gsVIEWPORT_END_X - sScrollXIncrement);
                StripRegions[1].Height = (int)(Globals.gsVIEWPORT_WINDOW_START_Y + sScrollYIncrement);
                StripRegions[1].Width = (int)(Globals.gsVIEWPORT_END_X - sScrollXIncrement);
                usNumStrips = 2;

                usMouseYPos += sScrollYIncrement;
                usMouseXPos -= sScrollXIncrement;

                break;

            case ScrollDirection.SCROLL_DOWNLEFT:

                Region.X = 0;
                Region.Y = Globals.gsVIEWPORT_WINDOW_START_Y + sScrollYIncrement;
                Region.Width = usWidth - sScrollXIncrement;
                Region.Height = Globals.gsVIEWPORT_WINDOW_START_Y + usHeight;

                this.BlitRegion(
                    pDest,
                    new Point(sScrollXIncrement, Globals.gsVIEWPORT_WINDOW_START_Y),
                    Region,
                    pSource);

                // // memset z-buffer
                for (uiCountY = Globals.gsVIEWPORT_WINDOW_START_Y; uiCountY < Globals.gsVIEWPORT_WINDOW_END_Y; uiCountY++)
                {
                    // memset((int*)gpZBuffer + (uiCountY * 1280), 0, sScrollXIncrement * 2);

                }
                for (uiCountY = Globals.gsVIEWPORT_WINDOW_END_Y - sScrollYIncrement; uiCountY < Globals.gsVIEWPORT_WINDOW_END_Y; uiCountY++)
                {
                    // memset((int*)gpZBuffer + (uiCountY * 1280), 0, 1280);
                }

                StripRegions[0].Width = (Globals.gsVIEWPORT_START_X + sScrollXIncrement);

                StripRegions[1].Y = (Globals.gsVIEWPORT_WINDOW_END_Y - sScrollYIncrement);
                StripRegions[1].X = (Globals.gsVIEWPORT_START_X + sScrollXIncrement);
                usNumStrips = 2;

                usMouseYPos -= sScrollYIncrement;
                usMouseXPos += sScrollXIncrement;

                break;

            case ScrollDirection.SCROLL_DOWNRIGHT:

                Region.X = sScrollXIncrement;
                Region.Y = Globals.gsVIEWPORT_WINDOW_START_Y + sScrollYIncrement;
                Region.Width = usWidth;
                Region.Height = Globals.gsVIEWPORT_WINDOW_START_Y + usHeight;

                this.BlitRegion(
                    pDest,
                    new Point(0, Globals.gsVIEWPORT_WINDOW_START_Y),
                    Region,
                    pSource);

                // // memset z-buffer
                for (uiCountY = Globals.gsVIEWPORT_WINDOW_START_Y; uiCountY < Globals.gsVIEWPORT_WINDOW_END_Y; uiCountY++)
                {
                    // memset((int*)gpZBuffer + (uiCountY * 1280) + ((Globals.gsVIEWPORT_END_X - sScrollXIncrement) * 2), 0, sScrollXIncrement * 2);
                }

                for (uiCountY = Globals.gsVIEWPORT_WINDOW_END_Y - sScrollYIncrement; uiCountY < Globals.gsVIEWPORT_WINDOW_END_Y; uiCountY++)
                {
                    // memset((int*)gpZBuffer + (uiCountY * 1280), 0, 1280);
                }

                StripRegions[0].X = (Globals.gsVIEWPORT_END_X - sScrollXIncrement);
                StripRegions[1].Y = (Globals.gsVIEWPORT_WINDOW_END_Y - sScrollYIncrement);
                StripRegions[1].Width = (Globals.gsVIEWPORT_END_X - sScrollXIncrement);
                usNumStrips = 2;

                usMouseYPos -= sScrollYIncrement;
                usMouseXPos -= sScrollXIncrement;

                break;
        }

        if (fRenderStrip)
        {
            for (cnt = 0; cnt < usNumStrips; cnt++)
            {
                RenderWorld.RenderStaticWorldRect(StripRegions[cnt], true);
                // Optimize Redundent tiles too!
                //ExamineZBufferRect( (int)StripRegions[ cnt ].X, (int)StripRegions[ cnt ].Y, (int)StripRegions[ cnt ].Width, (int)StripRegions[ cnt ].Height );

                this.BlitRegion(
                    pDest,
                    new Point(StripRegions[cnt].X, StripRegions[cnt].Y),
                    StripRegions[cnt],
                    this.Surfaces[SurfaceType.FRAME_BUFFER]);
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
            //SaveVideoOverlaysArea(VideoSurfaceManager.BACKBUFFER);

            // BLIT NEW
            //ExecuteVideoOverlaysToAlternateBuffer(VideoSurfaceManager.BACKBUFFER);


#if false

		// Erase mouse from old position
		if (gMouseCursorBackground[ uiCurrentMouseBackbuffer ].fRestore == true )
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

    public void GetCurrentVideoSettings(out int usWidth, out int usHeight, out int ubBitDepth)
    {
        usWidth = 0;
        usHeight = 0;
        ubBitDepth = 0;
    }

    public void InvalidateScreen()
    {
        //
        // W A R N I N G ---- W A R N I N G ---- W A R N I N G ---- W A R N I N G ---- W A R N I N G ----
        //
        // This function is intended to be called by a thread which has already locked the
        // FRAME_BUFFER_MUTEX mutual exclusion section. Anything else will cause the application to
        // yack
        //
        this.clearScreen = true;

        Globals.guiDirtyRegionCount = 0;
        Globals.guiDirtyRegionExCount = 0;
        Globals.gfForceFullScreenRefresh = true;
        Globals.guiFrameBufferState = BufferState.DIRTY;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    public void InvalidateRegion(Rectangle bounds)
    {
        if (gfForceFullScreenRefresh)
        {
            //
            // There's no point in going on since we are forcing a full screen refresh
            //

            return;
        }

        if (guiDirtyRegionCount < MAX_DIRTY_REGIONS)
        {
            //
            // Well we haven't broken the MAX_DIRTY_REGIONS limit yet, so we register the new region
            //

            // DO SOME PREMIMARY CHECKS FOR VALID RECTS
            //bounds.Deconstruct(out int iLeft, out int iTop, out int iRight, out int iBottom);
            //if (iLeft < 0)
            //{
            //    iLeft = 0;
            //}

            //if (iTop < 0)
            //{
            //    iTop = 0;
            //}

            //if (iRight > SCREEN_WIDTH)
            //{
            //    iRight = SCREEN_WIDTH;
            //}

            //if (iBottom > SCREEN_HEIGHT)
            //{
            //    iBottom = SCREEN_HEIGHT;
            //}

            //if ((iRight - iLeft) <= 0)
            //{
            //    return;
            //}

            //if ((iBottom - iTop) <= 0)
            //{
            //    return;
            //}

            gListOfDirtyRegions[guiDirtyRegionCount] = bounds;
            guiDirtyRegionCount++;
        }
        else
        {
            //
            // The MAX_DIRTY_REGIONS limit has been exceeded. Therefore we arbitrarely invalidate the entire
            // screen and force a full screen refresh
            //
            guiDirtyRegionExCount = 0;
            guiDirtyRegionCount = 0;
            gfForceFullScreenRefresh = true;
        }
    }

    public void EndFrameBufferRender()
    {
        guiFrameBufferState = BufferState.DIRTY;
    }

    public bool GetVideoObject(out HVOBJECT? hVObject, int uiIndex)
    {
        hVObject = null;
        VOBJECT_NODE? curr;

        curr = Globals.gpVObjectHead;
        while (curr is not null)
        {
            if (curr.uiIndex == uiIndex)
            {
                hVObject = curr.hVObject;
                return true;
            }

            curr = curr.next;
        }

        return false;
    }

    public bool DrawTextToScreen(
        string text,
        int usLocX,
        int usLocY,
        int usWidth,
        FontStyle ulFont,
        FontColor ubColor,
        FontColor ubBackGroundColor,
        TextJustifies ulFlags)
    {
        //int usPosX = 0;
        //int usPosY = 0;
        //int usFontHeight = 0;
        //int usStringWidth = 0;
        //fonts.FontSubSystem.SetFont(FontStyle.ulFont);
        //
        ////if (USE_WINFONTS())
        ////{
        ////    COLORVAL Color = FROMRGB(255, 255, 255);
        ////    SetWinFontForeColor(GET_WINFONT(), &Color);
        ////}
        ////else
        //{
        //    fonts.SetFontForeground(ubColor);
        //    fonts.SetFontBackground(ubBackGroundColor);
        //}
        //
        //if (ulFlags.HasFlag(TextJustifies.TEXT_SHADOWED))
        //{
        //    fonts.ShadowText(text, ulFont, usPosX - 1, usPosY - 1);
        //}

        // if (USE_WINFONTS())
        // {
        //     if (fDirty)
        //     {
        //         // gprintfdirty(usPosX, usPosY, pStr);
        //         // WinFont_mprintf(GET_WINFONT(), usPosX, usPosY, pStr);
        //     }
        //     else
        //     {
        //         // WinFont_mprintf(GET_WINFONT(), usPosX, usPosY, pStr);
        //     }
        // }
        // else
        //{
        //    if (fDirty)
        //    {
        //        // gprintfdirty(usPosX, usPosY, pStr);
        //        fonts.mprintf(usPosX, usPosY, text);
        //    }
        //    else
        //    {
        //         fonts.mprintf(usPosX, usPosY, text);
        //    }
        //}
        //
        //if ((fonts.IAN_WRAP_NO_SHADOW & ulFlags) != 0)
        //{
        //    // reset shadow
        //    fonts.SetFontShadow(FontShadow.DEFAULT_SHADOW);
        //}
        //
        //if (ulFlags.HasFlag(TextJustifies.INVALIDATE_TEXT))
        //{
        //    usFontHeight = fonts.WFGetFontHeight(ulFont);
        //    usStringWidth = fonts.WFStringPixLength(text, ulFont);
        //
        //    InvalidateRegion(new(usPosX, usPosY, usPosX + usStringWidth, usPosY + usFontHeight));
        //}

        return true;
    }

    public void GetVSurfacePaletteEntries(HVSURFACE hSrcVSurface, List<SGPPaletteEntry> pPalette)
    {
    }

    public ushort?[] Create16BPPPaletteShaded(List<SGPPaletteEntry> pPalette, int redScale, int greenScale, int blueScale, bool mono)
    {
        ushort?[] p16BPPPalette = new ushort?[256];
        ushort r16, g16, b16, usColor;
        int cnt;
        uint lumin;
        uint rmod, gmod, bmod;
        byte r, g, b;

        Debug.Assert(pPalette != null);

        for (cnt = 0; cnt < 256; cnt++)
        {
            if (mono)
            {
                lumin = (uint)(pPalette[cnt].peRed * 299 / 1000) + (uint)(pPalette[cnt].peGreen * 587 / 1000) + (uint)(pPalette[cnt].peBlue * 114 / 1000);
                rmod = (uint)(redScale * lumin) / 256;
                gmod = (uint)(greenScale * lumin) / 256;
                bmod = (uint)(blueScale * lumin) / 256;
            }
            else
            {
                rmod = (uint)(redScale * pPalette[cnt].peRed / 256);
                gmod = (uint)(greenScale * pPalette[cnt].peGreen / 256);
                bmod = (uint)(blueScale * pPalette[cnt].peBlue / 256);
            }

            r = (byte)Math.Min(rmod, 255);
            g = (byte)Math.Min(gmod, 255);
            b = (byte)Math.Min(bmod, 255);

            if (this.gusRedShift < 0)
            {
                r16 = (ushort)(r >> (-this.gusRedShift));
            }
            else
            {
                r16 = (ushort)(r << this.gusRedShift);
            }

            if (this.gusGreenShift < 0)
            {
                g16 = (ushort)(g >> (-this.gusGreenShift));
            }
            else
            {
                g16 = (ushort)(g << this.gusGreenShift);
            }


            if (this.gusBlueShift < 0)
            {
                b16 = (ushort)(b >> (-this.gusBlueShift));
            }
            else
            {
                b16 = (ushort)(b << this.gusBlueShift);
            }

            // Prevent creation of pure black color
            usColor = (ushort)((r16 & this.gusRedMask) | (g16 & this.gusGreenMask) | (b16 & this.gusBlueMask));

            if (usColor == 0)
            {
                if ((r + g + b) != 0)
                {
                    usColor = (ushort)(HIMAGE.BLACK_SUBSTITUTE | gusAlphaMask);
                }
            }
            else
            {
                usColor |= gusAlphaMask;
            }

            p16BPPPalette[cnt] = usColor;
        }

        return (p16BPPPalette);
    }

    public void DeleteVideoSurfaceFromIndex(SurfaceType uiTempMap)
    {
    }

    public void DeleteVideoObjectFromIndex(string key)
    {
        //loadedTextures.Remove(logoKey);
    }

    public void LineDraw(bool fClip, PointF startPoint, PointF endPoint, Color Color, Image<Rgba32> dst)
    {
        dst.Mutate(ctx =>
        {
            ctx.DrawLine(Color, 1.0f, [startPoint, endPoint]);
        });
    }

    public void GetClippingRect(out Rectangle clipRect)
    {
        clipRect = new();
    }

    public void SetClippingRect(ref Rectangle newClip)
    {
    }

    public void ColorFillVideoSurfaceArea(Rectangle rectangle, Color color)
        => this.ColorFillVideoSurfaceArea(rectangle, color.ToPixel<Rgba32>());

    public void ColorFillVideoSurfaceArea(Rectangle region, Rgba32 rgba32)
    {

    }

    public void ImageFillVideoSurfaceArea(Rectangle region, HVOBJECT hVOBJECT, ushort v3, short v4, short v5)
    {
    }

    public bool ShadowVideoSurfaceRectUsingLowPercentTable(SurfaceType destSurface, Rectangle rectangle)
    {
        return this.InternalShadowVideoSurfaceRect(destSurface, rectangle, true);
    }

    private bool InternalShadowVideoSurfaceRect(SurfaceType destSurface, Rectangle rectangle, bool fLowPercentShadeTable)
    {
        Image<Rgba32> pBuffer;
        Rectangle area;

        int X1 = rectangle.X;
        int X2 = rectangle.X + rectangle.Width;
        int Y1 = rectangle.Y;
        int Y2 = rectangle.Height - rectangle.Y;

        // CLIP IT!
        // FIRST GET SURFACE

        //
        // Get Video Surface
        //
# if _DEBUG
        gubVSDebugCode = DEBUGSTR_SHADOWVIDEOSURFACERECT;
#endif
        pBuffer = this.Surfaces[destSurface];
        //   CHECKF(this.GetVideoSurface(out HVSURFACE hVSurface, destSurface));

        if (X1 < 0)
        {
            X1 = 0;
        }

        if (X2 < 0)
        {
            return (false);
        }

        if (Y2 < 0)
        {
            return (false);
        }

        if (Y1 < 0)
        {
            Y1 = 0;
        }

        if (X2 >= pBuffer.Width)
        {
            X2 = pBuffer.Width - 1;
        }

        if (Y2 >= pBuffer.Height)
        {
            Y2 = pBuffer.Height - 1;
        }

        if (X1 >= pBuffer.Width)
        {
            return (false);
        }

        if (Y1 >= pBuffer.Height)
        {
            return (false);
        }

        if ((X2 - X1) <= 0)
        {
            return (false);
        }

        if ((Y2 - Y1) <= 0)
        {
            return (false);
        }

        area = new(X1, Y1, X2, Y2);

        // Lock video surface
        //pBuffer = this.LockVideoSurface(destSurface, out int uiPitch);
        //UnLockVideoSurface( uiDestVSurface );


        if (!fLowPercentShadeTable)
        {
            // Now we have the video object and surface, call the shadow function
            if (!Blitters.Blt16BPPBufferShadowRect(pBuffer, area))
            {
                // Blit has failed if false returned
                return (false);
            }
        }
        else
        {
            // Now we have the video object and surface, call the shadow function
            if (!Blitters.Blt16BPPBufferShadowRectAlternateTable(pBuffer, area))
            {
                // Blit has failed if false returned
                return (false);
            }
        }

        // Mark as dirty if it's the backbuffer
        //if ( uiDestVSurface == BACKBUFFER )
        //{
        //	InvalidateBackbuffer( );
        //}

        // UnLockVideoSurface(destSurface);
        return (true);
    }

    public void RestoreBackgroundRects()
    {
        RenderDirty.RestoreBackgroundRects();
    }

    public void SaveBackgroundRects()
    {
        int uiCount;
        Image<Rgba32> pDestBuf = this.Surfaces[SurfaceType.RENDER_BUFFER];
        Image<Rgba32> pSrcBuf = this.Surfaces[SurfaceType.SAVE_BUFFER];

        for (uiCount = 0; uiCount < guiNumBackSaves; uiCount++)
        {
            if (gBackSaves[uiCount].fAllocated && (!gBackSaves[uiCount].fDisabled))
            {
                if (gBackSaves[uiCount].uiFlags.HasFlag(BGND_FLAG.SAVERECT))
                {
                    if (gBackSaves[uiCount].pSaveArea != null)
                    {
                        Blt16BPPTo16BPP(
                            gBackSaves[uiCount].pSaveArea,
                            pSrcBuf,
                            new(0, 0),
                            new(gBackSaves[uiCount].sLeft, gBackSaves[uiCount].sTop),
                            gBackSaves[uiCount].sWidth,
                            gBackSaves[uiCount].sHeight);
                    }

                }
                else if (gBackSaves[uiCount].fZBuffer)
                {
                    Blt16BPPTo16BPP(
                        gBackSaves[uiCount].pZSaveArea,
                        this.Surfaces[SurfaceType.Z_BUFFER],
                        new(0, 0),
                        new(gBackSaves[uiCount].sLeft, gBackSaves[uiCount].sTop),
                        gBackSaves[uiCount].sWidth, gBackSaves[uiCount].sHeight);
                }
                else
                {
                    RenderDirty.AddBaseDirtyRect(new(gBackSaves[uiCount].sLeft,
                        gBackSaves[uiCount].sTop,
                        gBackSaves[uiCount].sRight,
                        gBackSaves[uiCount].sBottom));
                }

                gBackSaves[uiCount].fFilled = true;


            }
        }
    }

    public void ExecuteBaseDirtyRectQueue()
    {
        if (RenderDirty.gfViewportDirty)
        {
            //InvalidateRegion(gsVIEWPORT_START_X, gsVIEWPORT_START_Y, gsVIEWPORT_END_X, gsVIEWPORT_END_Y);
            InvalidateScreen();
            RenderDirty.EmptyDirtyRectQueue();
            RenderDirty.gfViewportDirty = false;
        }

    }

    public bool DeleteVideoObject(HVOBJECT hVObject)
    {
        int usLoop;

        // Assertions
        CHECKF(hVObject != null);

        DestroyObjectPaletteTables(hVObject);

        // Release palette
        if (hVObject?.pPaletteEntry != null)
        {
            MemFree(hVObject.pPaletteEntry);
            //		hVObject.pPaletteEntry = null;
        }


        if (hVObject?.pPixData != null)
        {
            MemFree(hVObject.pPixData);
            //		hVObject.pPixData = null;
        }

        if (hVObject?.pETRLEObject != null)
        {
            MemFree(hVObject.pETRLEObject);
            //		hVObject.pETRLEObject = null;
        }

        if (hVObject?.ppZStripInfo != null)
        {
            for (usLoop = 0; usLoop < hVObject?.ppZStripInfo.Count; usLoop++)
            {
                if (hVObject?.ppZStripInfo[usLoop] is not null)
                {
                    MemFree(hVObject.ppZStripInfo[usLoop].pbZChange);
                    MemFree(hVObject.ppZStripInfo[usLoop]);
                }
            }
            MemFree(hVObject?.ppZStripInfo);
            //		hVObject.ppZStripInfo = null;
        }

        if (hVObject?.usNumberOf16BPPObjects > 0)
        {
            for (usLoop = 0; usLoop < hVObject?.usNumberOf16BPPObjects; usLoop++)
            {
                MemFree(hVObject?.p16BPPObject[usLoop].p16BPPData);
            }

            MemFree(hVObject?.p16BPPObject);
        }

        // Release object
        MemFree(hVObject);

        return true;
    }

    /**********************************************************************************************
        DestroyObjectPaletteTables

        Destroys the palette tables of a video object. All memory is deallocated, and
        the pointers set to null. Be careful not to try and blit this object until new
        tables are calculated, or things WILL go boom.

    **********************************************************************************************/
    private bool DestroyObjectPaletteTables(HVOBJECT hVObject)
    {
        int x;
        bool f16BitPal;

        for (x = 0; x < HVOBJECT.SHADE_TABLES; x++)
        {
            if (!(hVObject.fFlags.HasFlag(VOBJECT_FLAG.SHADETABLE_SHARED)))
            {
                if (hVObject.pShades[x] != null)
                {
                    f16BitPal = hVObject.pShades[x] == hVObject.p16BPPPalette;

                    MemFree(hVObject.pShades[x]);
                    hVObject.pShades[x] = null;

                    if (f16BitPal)
                    {
                        hVObject.p16BPPPalette = null;
                    }
                }
            }
        }

        if (hVObject.p16BPPPalette != null)
        {
            MemFree(hVObject.p16BPPPalette);
            hVObject.p16BPPPalette = null;
        }

        hVObject.pShadeCurrent = null;
        hVObject.pGlow = null;

        return (true);
    }


    public bool BlitBufferToBuffer(SurfaceType srcBuffer, SurfaceType dstBuffer, Rectangle srcRect)
    {
        bool fRetVal;

        var src = this.Surfaces[srcBuffer];
        var dst = this.Surfaces[dstBuffer];

        fRetVal = this.Blt16BPPTo16BPP(
            dst,
            src,
            srcRect.ToPoint(),
            srcRect.ToPoint(),
            srcRect.Width,
            srcRect.Height);

        return (fRetVal);
    }

    public void ColorFillVideoSurfaceArea(Image<Rgba32> surface, Rectangle region, Color rgba32)
    {
    }

    public void ColorFillVideoSurfaceArea(Image<Rgba32> surface, Rectangle region, Rgba32 rgba32)
    {
    }

    public void ColorFillVideoSurfaceArea(SurfaceType surface, Rectangle rectangle, Color color)
    {
    }

    public void SetVideoSurfaceTransparency(SurfaceType uiVideoSurfaceImage, Rgba32 pixel)
    {
    }

    public bool GetVideoSurface(out HVSURFACE hSrcVSurface, SurfaceType uiTempMap)
    {
        hSrcVSurface = new();

        return true;
    }

    public void ClearElements()
    {
        //        FontSubSystem.TextRenderer.ClearText();
    }

    public Image<Rgba32> LockVideoSurface(SurfaceType buffer, out int uiSrcPitchBYTES)
    {
        uiSrcPitchBYTES = buffer switch
        {
            SurfaceType.PRIMARY_SURFACE => 128,
            SurfaceType.BACKBUFFER => 128,
            SurfaceType.FRAME_BUFFER => 1280,
            SurfaceType.MOUSE_BUFFER => 128,
            SurfaceType.Unknown => 0,
            _ => 0,
        };

        return this.Surfaces.LockSurface(buffer);
    }

    public void InvalidateRegionEx(Rectangle bounds, int uiFlags)
    {
        (var iLeft, var iTop, var iRight, var iBottom) = bounds;
        int iOldBottom = bounds.Bottom;

        // Check if we are spanning the rectangle - if so slit it up!
        if (iTop <= gsVIEWPORT_WINDOW_END_Y && iBottom > gsVIEWPORT_WINDOW_END_Y)
        {
            // Add new top region
            iBottom = gsVIEWPORT_WINDOW_END_Y;
            this.AddRegionEx(iLeft, iTop, iRight, iBottom, uiFlags);

            // Add new bottom region
            iTop = gsVIEWPORT_WINDOW_END_Y;
            iBottom = iOldBottom;
            this.AddRegionEx(iLeft, iTop, iRight, iBottom, uiFlags);

        }
        else
        {
            this.AddRegionEx(iLeft, iTop, iRight, iBottom, uiFlags);
        }
    }

    private void AddRegionEx(int iLeft, int iTop, int iRight, int iBottom, int uiFlags)
    {

        if (guiDirtyRegionExCount < MAX_DIRTY_REGIONS)
        {
            // DO SOME PREMIMARY CHECKS FOR VALID RECTS
            if (iLeft < 0)
            {
                iLeft = 0;
            }

            if (iTop < 0)
            {
                iTop = 0;
            }

            if (iRight > SCREEN_WIDTH)
            {
                iRight = SCREEN_WIDTH;
            }

            if (iBottom > SCREEN_HEIGHT)
            {
                iBottom = SCREEN_HEIGHT;
            }

            if ((iRight - iLeft) <= 0)
            {
                return;
            }

            if ((iBottom - iTop) <= 0)
            {
                return;
            }

            gDirtyRegionsEx[guiDirtyRegionExCount] = new(iLeft, iTop, iRight - iLeft, iBottom - iTop);
            gDirtyRegionsFlagsEx[guiDirtyRegionExCount] = uiFlags;

            guiDirtyRegionExCount++;
        }
        else
        {
            guiDirtyRegionExCount = 0;
            guiDirtyRegionCount = 0;
            gfForceFullScreenRefresh = true;
        }
    }

    public void Blt8BPPTo8BPP(Image<Rgba32> pDestBuf, int uiDestPitchBYTES, Image<Rgba32> pSrcBuf, int uiSrcPitchBYTES, int sLeft1, int sTop1, int sLeft2, int sTop2, int sWidth, int sHeight)
    {
        throw new NotImplementedException();
    }

    public void DeleteVideoObjectFromIndex(SurfaceType guiWoodBackground)
    {
        throw new NotImplementedException();
    }

    public void InvalidateRegion(int v1, int v2, int v3, int v4) => this.InvalidateRegion(new(v1, v2, v3, v4));

    public bool Blt8BPPDataSubTo16BPPBuffer(Image<Rgba32> pDestBuf, Size size, Image<Rgba32> pSrcBuf, int iX, int iY, out Rectangle clip)
    {
        clip = new Rectangle(0, 0, 100, 100);

        int p16BPPPalette;
        int usHeight, usWidth;
        int SrcPtr, DestPtr;
        int LineSkip, LeftSkip, RightSkip, TopSkip, BlitLength, SrcSkip, BlitHeight;
        int iTempX, iTempY;

        // Get Offsets from Index into structure
        usHeight = size.Height;
        usWidth = size.Width;

        // Add to start position of dest buffer
        iTempX = iX;
        iTempY = iY;

        // Validations
        CHECKF(iTempX >= 0);
        CHECKF(iTempY >= 0);

        //LeftSkip = pRect.iLeft;
        //RightSkip = usWidth - pRect.iRight;
        //TopSkip = pRect.iTop * uiSrcPitch;
        //BlitLength = pRect.iRight - pRect.iLeft;
        //BlitHeight = pRect.iBottom - pRect.iTop;
        //SrcSkip = uiSrcPitch - BlitLength;
        //
        //SrcPtr = (pSrcBuffer + TopSkip + LeftSkip);
        //DestPtr = (pBuffer + (uiDestPitchBYTES * iTempY) + (iTempX * 2));
        //p16BPPPalette = hSrcVSurface.p16BPPPalette;
        //LineSkip = (uiDestPitchBYTES - (BlitLength * 2));

        return true;
    }

    public bool GetVideoObjectETRLEPropertiesFromIndex(string uiVideoObject, out ETRLEObject pETRLEObject, int usIndex)
    {

        HVOBJECT hVObject = null;// GetVideoObject(uiVideoObject);

        this.GetVideoObjectETRLEProperties(hVObject, out pETRLEObject, usIndex);

        return true;
    }

    public bool GetVideoObjectETRLEProperties(HVOBJECT hVObject, out ETRLEObject pETRLEObject, int usIndex)
    {
        CHECKF(usIndex >= 0);
        CHECKF(usIndex < hVObject.usNumberOfObjects);

        pETRLEObject = (hVObject.pETRLEObject[usIndex]);
        return true;
    }

    public bool BltVideoObjectFromIndex(SurfaceType uiDestVSurface, int uiSrcVObject, int usRegionIndex, int iDestX, int iDestY, VO_BLT fBltFlags, blt_fx? pBltFx)
    {
        Image<Rgba32> pBuffer;

        // Lock video surface
        pBuffer = this.LockVideoSurface(uiDestVSurface, out int uiPitch);

        // Get video object
        if (!this.GetVideoObject(out HVOBJECT hSrcVObject, uiSrcVObject))
        {
            // UnLockVideoSurface(uiDestVSurface);
            return false;
        }

        // Now we have the video object and surface, call the VO blitter function
        if (!VideoObjectManager.BltVideoObjectToBuffer(
            out pBuffer,
            (uint)uiPitch,
            hSrcVObject,
            (ushort)usRegionIndex,
            iDestX,
            iDestY,
            fBltFlags,
            pBltFx))
        {
            // UnLockVideoSurface(uiDestVSurface);
            // VO Blitter will set debug messages for error conditions
            return false;
        }

        // UnLockVideoSurface(uiDestVSurface);
        return true;
    }

    public void InvalidateRegionEx(int sLeft, int sTop, int v1, int v2, int flags)
        => this.InvalidateRegionEx(new(sLeft, sTop, v1, v2), flags);

    public HVOBJECT LoadImage(string assetPath) => this.textures.LoadImage(assetPath);

    public Texture[] CreateSurfaces(nint renderer, Image<Rgba32>[] images)
    {
        List<Texture> surfaces = [];

        foreach (var image in images)
        {
            surfaces.Add(this.Surfaces.CreateSurface(renderer, image));
        }

        return [.. surfaces];
    }

    public void BlitSurfaceToSurface(Image<Rgba32> src, SurfaceType dst, Point dstPoint, VO_BLT bltFlags = VO_BLT.SRCTRANSPARENCY)
    {
        var dstSurface = this.Surfaces.SurfaceByTypes[dst];

        Rectangle dstRectangle = new()
        {
            Height = src.Height,
            Width = src.Width,
            X = dstPoint.X,
            Y = dstPoint.Y,
        };

//        src.SaveAsPng(@$"C:\temp\{nameof(BlitSurfaceToSurface)}-src.png");
//        dstSurface.Image.SaveAsPng(@$"C:\temp\{nameof(BlitSurfaceToSurface)}-dstSurface-before.png");

        dstSurface.Image.Mutate(ctx =>
        {
            ctx.DrawImage(
                src,
                dstPoint,
                dstRectangle,
                colorBlending: PixelColorBlendingMode.Normal,
                //                alphaComposition: PixelAlphaCompositionMode.Dest,
                1.0f);
        });

//        dstSurface.Image.SaveAsPng(@$"C:\temp\{nameof(BlitSurfaceToSurface)}-dstSurface.png");
    }

    public bool ShadowVideoSurfaceRect(SurfaceType buffer, Rectangle rectangle)
    {
        return InternalShadowVideoSurfaceRect(buffer, rectangle, false);
    }

    public void BltVideoSurface(SurfaceType dstSurf, SurfaceType srcSurf, int usRegionIndex, Point sDest, BlitTypes blitTypes, object value)
    {
        var dst = this.Surfaces[dstSurf];
        var src = this.Surfaces[srcSurf];

        this.BlitSurfaceToSurface(src, dstSurf, sDest, VO_BLT.DESTTRANSPARENCY);
    }

    public void StartFrameBufferRender()
    {
    }
}
