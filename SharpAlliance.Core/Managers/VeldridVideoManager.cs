using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SharpAlliance.Core.Interfaces;
using SharpAlliance.Core.Managers.Image;
using SharpAlliance.Core.Managers.VideoSurfaces;
using SharpAlliance.Core.Screens;
using SharpAlliance.Core.SubSystems;
using SharpAlliance.Platform;
using SharpAlliance.Platform.Interfaces;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Veldrid;
using Veldrid.ImageSharp;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;
using Veldrid.Utilities;
using Point = SixLabors.ImageSharp.Point;
using Rectangle = SixLabors.ImageSharp.Rectangle;

using static SharpAlliance.Core.Globals;
using static SharpAlliance.Core.SubSystems.FontSubSystem;
using FontStyle = SharpAlliance.Core.SubSystems.FontStyle;

namespace SharpAlliance.Core.Managers;

public class VeldridVideoManager : IVideoManager
{
    // TODO: These are temporary...
    const int DD_OK = 0;
    const int DDBLTFAST_NOCOLORKEY = 0x00000000;
    const int DDBLTFAST_SRCCOLORKEY = 0x00000001;
    const int DDBLTFAST_DESTCOLORKEY = 0x00000002;
    const int DDBLTFAST_WAIT = 0x00000010;
    const int DDERR_WASSTILLDRAWING = 0x8700000; // not real value
    const int DDERR_SURFACELOST = 0x9700000;
    private readonly SurfaceManager surfaces;
    private readonly ILogger<VeldridVideoManager> logger;

    private bool clearScreen;

    private Dictionary<string, HVOBJECT> loadedTextures = new();

    // private readonly WindowsSubSystem windows;
    private readonly RenderWorld renderWorld;
    private readonly ScreenManager screenManager;
    private readonly GameContext context;
    private readonly IFileManager files;
    private readonly Shading shading = new();
    private readonly MouseCursorBackground[] mouseCursorBackground = new MouseCursorBackground[2];

    private static Sdl2Window window;
    public static Sdl2Window Window { get => window; }
    public GraphicsDevice GraphicDevice { get; private set; }
    public ResourceFactory Factory { get; private set; }
    protected SpriteRenderer SpriteRenderer { get; private set; }

    private Swapchain mainSwapchain;
    private CommandList commandList;
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

    private RgbaFloat clearColor = new(1.0f, 0, 0.2f, 1f);

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

    public bool IsInitialized { get; private set; }
    public uint guiBOTTOMPANEL { get; set; }
    public uint guiRIGHTPANEL { get; set; }
//    public uint guiRENDERBUFFER { get; set; }
//    public uint guiSAVEBUFFER { get; set; }
//    public uint guiEXTRABUFFER { get; set; }
    public bool gfExtraBuffer { get; set; }
    public int gbPixelDepth { get; }

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

//    private Texture backBuffer;
    private Image<Rgba32> backBuffer;
 
    public VeldridVideoManager(
        ILogger<VeldridVideoManager> logger,
        GameContext context,
        IFileManager fileManager,
        RenderWorld renderWorld,
        IScreenManager screenManager,
        SurfaceManager surfaceManager,
        Shading shading)
    {
        this.surfaces = surfaceManager;
        this.logger = logger;
        this.context = context;
        this.files = fileManager;
        this.renderWorld = renderWorld;
        this.screenManager = (screenManager as ScreenManager)!;
        this.shading = shading;

        this.surfaces.InitializeSurfaces(SCREEN_WIDTH, SCREEN_HEIGHT);

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

        GraphicsDeviceOptions gdOptions = new(
            debug: false,
            swapchainDepthFormat: null,
            syncToVerticalBlank: false,
            resourceBindingModel: ResourceBindingModel.Improved,
            preferDepthRangeZeroToOne: true,
            preferStandardClipSpaceYDirection: false,
            swapchainSrgbFormat: _colorSrgb);

#if DEBUG
        gdOptions.Debug = true;
#endif
        SDL_WindowFlags GetWindowFlags(WindowState state)
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

        window = new Sdl2Window(
            windowCI.WindowTitle,
            windowCI.X,
            windowCI.Y,
            windowCI.WindowWidth,
            windowCI.WindowHeight,
            flags,
            threadedProcessing: true);

        

//        GraphicDevice = VeldridStartup.CreateGraphicsDevice(
//            Window,
//            gdOptions);

        Window.Resized += () => windowResized = true;
        //Window.PollIntervalInMs = 1000 / 30;
//        Factory = new DisposeCollectorResourceFactory(GraphicDevice.ResourceFactory);
//        mainSwapchain = GraphicDevice.MainSwapchain;
//        commandList = GraphicDevice.ResourceFactory.CreateCommandList();

//        SpriteRenderer = new SpriteRenderer(GraphicDevice);
//        IVideoManager.DebugRenderer = new DebugRenderer(GraphicDevice);

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

        backBuffer = new(SCREEN_WIDTH, SCREEN_HEIGHT);
//        backBuffer = new ImageSharpTexture(new Image<Rgba32>(SCREEN_WIDTH, SCREEN_HEIGHT), mipmap: false)
//            .CreateDeviceTexture(GraphicDevice, GraphicDevice.ResourceFactory);
//
        // fadeScreen = (screenManager.GetScreen(ScreenNames.FADE_SCREEN, activate: true).AsTask().Result as FadeScreen)!;
        IsInitialized = await files.Initialize();

        return IsInitialized;
    }

    /**********************************************************************************************
	Blt16BPPTo16BPP

	Copies a rect of 16 bit data from a video buffer to a buffer position of the brush
	in the data area, for later blitting. Used to copy background information for mercs
	etc. to their unblit buffer, for later reblitting. Does NOT clip.

**********************************************************************************************/
    public bool Blt16BPPTo16BPP(Image<Rgba32> pDest, int uiDestPitch, Image<Rgba32> pSrc, int uiSrcPitch, int iDestXPos, int iDestYPos, int iSrcXPos, int iSrcYPos, int uiWidth, int uiHeight)
    {
        int pSrcPtr, pDestPtr;
        int uiLineSkipDest, uiLineSkipSrc;

        //pSrcPtr = pSrc + (iSrcYPos * uiSrcPitch) + (iSrcXPos * 2);
        //pDestPtr = pDest + (iDestYPos * uiDestPitch) + (iDestXPos * 2);
        uiLineSkipDest = uiDestPitch - (uiWidth * 2);
        uiLineSkipSrc = uiSrcPitch - (uiWidth * 2);

        //        __asm {
        //            mov esi, pSrcPtr
        //        
        //    mov edi, pDestPtr
        //        
        //    mov ebx, uiHeight
        //        
        //    cld
        //
        //    mov     ecx, uiWidth
        //    test    ecx, 1
        //        
        //    jz BlitDwords
        //        
        //BlitNewLine:
        //
        //            mov ecx, uiWidth
        //        
        //    shr ecx, 1
        //        
        //    movsw
        //
        //    //BlitNL2:
        //
        //    rep     movsd
        //
        //    add     edi, uiLineSkipDest
        //    add     esi, uiLineSkipSrc
        //    dec     ebx
        //    jnz     BlitNewLine
        //
        //    jmp     BlitDone
        //
        //
        //BlitDwords:
        //	mov ecx, uiWidth
        //        
        //    shr ecx, 1
        //        
        //    rep movsd
        //        
        //
        //    add edi, uiLineSkipDest
        //        
        //    add esi, uiLineSkipSrc
        //        
        //    dec ebx
        //        
        //    jnz BlitDwords
        //        
        //BlitDone:
        //
        //
        //    }

        return true;
    }

    public void DrawFrame()
    {
        commandList.Begin();

        commandList.SetFramebuffer(mainSwapchain.Framebuffer);

        if (Globals.gfForceFullScreenRefresh || clearScreen)
        {
            commandList.ClearColorTarget(0, clearColor);
            clearScreen = false;
        }

        commandList.ClearColorTarget(0, clearColor);

        ScreenManager.Draw(SpriteRenderer, GraphicDevice, commandList);
        MouseSubSystem.Draw(SpriteRenderer, GraphicDevice, commandList);

        // Everything above writes to this SpriteRenderer, so draw it now.
        SpriteRenderer.Draw(GraphicDevice, commandList);
        IVideoManager.DebugRenderer.Draw(GraphicDevice, commandList);

        SpriteRenderer.RenderText(GraphicDevice, commandList, FontSubSystem.TextRenderer.TextureView, new Vector2(0, 0));
        commandList.End();

        FontSubSystem.TextRenderer.RenderAllText();
        GraphicDevice.SubmitCommands(commandList);
        GraphicDevice.SwapBuffers(mainSwapchain);
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

    public HVOBJECT AddVideoObject(string assetPath, out string key)
    {
        key = assetPath;

        if (loadedTextures.TryGetValue(key, out var vObject))
        {
            return vObject;
        }

        // Create video object
        HVOBJECT hVObject = CreateVideoObject(assetPath);

        loadedTextures.Add(assetPath, hVObject);

        return hVObject;
    }

    public HVOBJECT CreateVideoObject(string assetPath)
    {
        HVOBJECT hVObject = new();
        hVObject.Name = assetPath;

        HIMAGE hImage;
        ETRLEData TempETRLEData = new();

        // Create himage object from file
        hImage = HIMAGE.CreateImage(assetPath, HIMAGECreateFlags.IMAGE_ALLIMAGEDATA, files);

        // Get TRLE data
        GetETRLEImageData(hImage, ref TempETRLEData);

        // Set values
        hVObject.usNumberOfObjects = TempETRLEData.usNumberOfObjects;
        hVObject.pETRLEObject = TempETRLEData.pETRLEObject;
        hVObject.pPixData = TempETRLEData.pPixData;
        hVObject.uiSizePixData = TempETRLEData.uiSizePixData;

        // Set palette from himage
        if (hImage.ubBitDepth == 8)
        {
            hVObject.pShade8 = shading.ubColorTables[Shading.DEFAULT_SHADE_LEVEL, 0];
            hVObject.pGlow8 = shading.ubColorTables[0, 0];

            SetVideoObjectPalette(hVObject, hImage, hImage.pPalette);
        }

        // Set values from himage
        hVObject.ubBitDepth = hImage.ubBitDepth;

        // All is well
        //  DbgMessage( TOPIC_VIDEOOBJECT, DBG_LEVEL_3, String("Success in Creating Video Object" ) );

        hVObject.hImage = hImage;
        hVObject.Textures = new Image<Rgba32>[hImage.ParsedImages.Count];

        for (int i = 0; i < hImage.ParsedImages.Count; i++)
        {
            hVObject.Textures[i] = hImage.ParsedImages[i];
                //new ImageSharpTexture(hImage.ParsedImages[i], mipmap: false)
                //.CreateDeviceTexture(GraphicDevice, GraphicDevice.ResourceFactory);

//            hVObject.Textures[i].Name = $"{hImage.ImageFile}_{i}";
        }

        return hVObject;
    }

    public bool SetVideoObjectPalette(HVOBJECT hVObject, HIMAGE hImage, List<SGPPaletteEntry> pSrcPalette)
    {
        // Create palette object if not already done so
        hVObject.pPaletteEntry = pSrcPalette;

        // Create 16BPP Palette
        hVObject.Palette = hImage.Create16BPPPalette(pSrcPalette);
        hVObject.ShadeCurrentPixels = hVObject.Palette;

        if (hImage.fFlags.HasFlag(HIMAGECreateFlags.IMAGE_PALETTE))
        {
            hImage.ParsedImages = hImage.iFileLoader.ApplyPalette(ref hVObject, ref hImage);
        }

        // If you want to output all the images to disk, uncomment ..makes startup take a lot longer.
        // for (int i = 0; i < (hImage.ParsedImages?.Count ?? 0); i++)
        // {
        //     var fileName = Path.GetFileNameWithoutExtension(hImage.ImageFile) + $"_{i}.png";
        //     var directory = Path.Combine("C:\\", "assets", Path.GetDirectoryName(hImage.ImageFile)!);
        //     Directory.CreateDirectory(directory);
        //     hImage.ParsedImages![i].SaveAsPng(Path.Combine(directory, fileName));
        // }

        //  DbgMessage(TOPIC_VIDEOOBJECT, DBG_LEVEL_3, String("Video Object Palette change successfull" ));
        return true;
    }

    public bool GetETRLEImageData(HIMAGE? hImage, ref ETRLEData pBuffer)
    {
        if (hImage is null)
        {
            return false;
        }

        // Create memory for data
        pBuffer.usNumberOfObjects = hImage.usNumberOfObjects;

        // Create buffer for objects
        pBuffer.pETRLEObject = new ETRLEObject[pBuffer.usNumberOfObjects];
        //CHECKF(pBuffer.pETRLEObject != null);

        // Copy into buffer
        pBuffer.pETRLEObject = hImage.pETRLEObject;

        // Allocate memory for pixel data
        pBuffer.pPixData = new byte[hImage.uiSizePixData];
        //CHECKF(pBuffer.pPixData != null);

        pBuffer.uiSizePixData = hImage.uiSizePixData;

        // Copy into buffer
        pBuffer.pPixData = hImage.pImageData;

        return true;
    }

    public void RefreshScreen()
    {
        int usScreenWidth;
        int usScreenHeight;
        long uiTime;

        usScreenWidth = usScreenHeight = 0;

        if (fFirstTime)
        {
            fShowMouse = false;
        }

        logger.LogDebug(LoggingEventId.VIDEO, "Looping in refresh");

        ///////////////////////////////////////////////////////////////////////////////////////////////
        // 
        // REFRESH_THREAD_MUTEX 
        //
        ///////////////////////////////////////////////////////////////////////////////////////////////

        switch (Globals.guiVideoManagerState)
        {
            case VideoManagerState.On:
                // Excellent, everything is cosher, we continue on
                uiRefreshThreadState = ThreadState.On;
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
                uiRefreshThreadState = Globals.guiRefreshThreadState = ThreadState.Suspended;
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
        if (mouseCursorBackground[Globals.CURRENT_MOUSE_DATA].fRestore == true)
        {
            Region.X = mouseCursorBackground[Globals.CURRENT_MOUSE_DATA].usLeft;
            Region.Y = mouseCursorBackground[Globals.CURRENT_MOUSE_DATA].usTop;
            Region.Width = mouseCursorBackground[Globals.CURRENT_MOUSE_DATA].usRight;
            Region.Height = mouseCursorBackground[Globals.CURRENT_MOUSE_DATA].usBottom;

            MouseSubSystem.Draw(
                mouseCursorBackground[Globals.CURRENT_MOUSE_DATA],
                Region,
                GraphicDevice,
                commandList);

            // Save position into other background region
            mouseCursorBackground[Globals.PREVIOUS_MOUSE_DATA] = mouseCursorBackground[Globals.CURRENT_MOUSE_DATA];
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

            if (fadeScreen?.gfFadeInitialized ?? false
                && fadeScreen.gfFadeInVideo)
            {
                fadeScreen!.gFadeFunction();
            }
            else
            {
                // Either Method (1) or (2)
                if (Globals.gfForceFullScreenRefresh == true)
                {
                    // Method (1) - We will be refreshing the entire screen
                    Region.X = 0;
                    Region.Y = 0;
                    Region.Width = usScreenWidth;
                    Region.Height = usScreenHeight;

                    // BlitRegion(
                    //     backBuffer,
                    //     new Point(0, 0),
                    //     Region,
                    //     gpFrameBuffer);
                }
                else
                {
                    for (uiIndex = 0; uiIndex < Globals.guiDirtyRegionCount; uiIndex++)
                    {
                        Region.X = Globals.gListOfDirtyRegions[uiIndex].Left;
                        Region.Y = Globals.gListOfDirtyRegions[uiIndex].Top;
                        Region.Width = Globals.gListOfDirtyRegions[uiIndex].Width;
                        Region.Height = Globals.gListOfDirtyRegions[uiIndex].Height;

                        BlitRegion(
                            backBuffer,
                            new Point(Region.X, Region.Y),
                            Region,
                            this.surfaces[Surfaces.PRIMARY_SURFACE]);
                    }

                    // Now do new, extended dirty regions
                    for (uiIndex = 0; uiIndex < Globals.guiDirtyRegionExCount; uiIndex++)
                    {
                        Region = Globals.gDirtyRegionsEx[uiIndex];

                        // Do some checks if we are in the process of scrolling!	
                        if (Globals.gfRenderScroll)
                        {
                            // Check if we are completely out of bounds
                            if (Region.Y <= Globals.gsVIEWPORT_WINDOW_END_Y
                                && Region.Height <= Globals.gsVIEWPORT_WINDOW_END_Y)
                            {
                                continue;
                            }
                        }

                        BlitRegion(
                            backBuffer,
                            Region.ToPoint(),
                            Region,
                            this.surfaces[Surfaces.FRAME_BUFFER]);
                    }
                }
            }

            if (Globals.gfRenderScroll)
            {
                ScrollJA2Background(
                    Globals.guiScrollDirection,
                    Globals.gsScrollXIncrement,
                    Globals.gsScrollYIncrement,
                    this.surfaces[Surfaces.PRIMARY_SURFACE],
                    backBuffer,
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
            Region.X = 0;
            Region.Y = 0;
            Region.Width = Globals.gusMouseCursorWidth;
            Region.Height = Globals.gusMouseCursorHeight;

            BlitRegion(
                Globals.gpMouseCursor,
                new Point(0, 0),
                Region,
                Globals.gpMouseCursorOriginal);

            Globals.guiMouseBufferState = BufferState.READY;
        }

        // Check current state of the mouse cursor
        if (fShowMouse == false)
        {
            fShowMouse = Globals.guiMouseBufferState == BufferState.READY;
        }
        else
        {
            fShowMouse = Globals.guiMouseBufferState == BufferState.DISABLED;
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
            Region.X = MousePos.X - Globals.gsMouseCursorXOffset;
            Region.Y = MousePos.Y - Globals.gsMouseCursorYOffset;
            Region.Width = Region.X + Globals.gusMouseCursorWidth;
            Region.Height = Region.Y + Globals.gusMouseCursorHeight;

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
                mouseCursorBackground[Globals.CURRENT_MOUSE_DATA].fRestore = true;
                mouseCursorBackground[Globals.CURRENT_MOUSE_DATA].usRight = Region.Width - Region.X;
                mouseCursorBackground[Globals.CURRENT_MOUSE_DATA].usBottom = Region.Height - Region.Y;

                if (Region.X < 0)
                {
                    mouseCursorBackground[Globals.CURRENT_MOUSE_DATA].usLeft = (0 - Region.X);
                    mouseCursorBackground[Globals.CURRENT_MOUSE_DATA].usMouseXPos = 0;
                    Region.X = 0;
                }
                else
                {
                    mouseCursorBackground[Globals.CURRENT_MOUSE_DATA].usMouseXPos = MousePos.X - Globals.gsMouseCursorXOffset;
                    mouseCursorBackground[Globals.CURRENT_MOUSE_DATA].usLeft = 0;
                }

                if (Region.Y < 0)
                {
                    mouseCursorBackground[Globals.CURRENT_MOUSE_DATA].usMouseYPos = 0;
                    mouseCursorBackground[Globals.CURRENT_MOUSE_DATA].usTop = (0 - Region.Y);
                    Region.Y = 0;
                }
                else
                {
                    mouseCursorBackground[Globals.CURRENT_MOUSE_DATA].usMouseYPos = MousePos.Y - Globals.gsMouseCursorYOffset;
                    mouseCursorBackground[Globals.CURRENT_MOUSE_DATA].usTop = 0;
                }

                if ((Region.Width > Region.X) && (Region.Height > Region.Y))
                {
                    // Save clipped region
                    mouseCursorBackground[Globals.CURRENT_MOUSE_DATA].Region = Region;

                    // Ok, do the actual data save to the mouse background
                    //BlitRegion(
                    //    mouseCursorBackground[Globals.CURRENT_MOUSE_DATA].pSurface,
                    //    new Point(
                    //        mouseCursorBackground[Globals.CURRENT_MOUSE_DATA].usLeft,
                    //        mouseCursorBackground[Globals.CURRENT_MOUSE_DATA].usTop),
                    //    Region,
                    //    backBuffer);

                    // Step (2) - Blit mouse cursor to back buffer
                    Region.X = mouseCursorBackground[Globals.CURRENT_MOUSE_DATA].usLeft;
                    Region.Y = mouseCursorBackground[Globals.CURRENT_MOUSE_DATA].usTop;
                    Region.Width = mouseCursorBackground[Globals.CURRENT_MOUSE_DATA].usRight;
                    Region.Height = mouseCursorBackground[Globals.CURRENT_MOUSE_DATA].usBottom;

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
                    mouseCursorBackground[Globals.CURRENT_MOUSE_DATA].fRestore = false;
                }
            }
            else
            {
                // Hum, the mouse was not blitted this round. Henceforth we will flag fRestore as false
                mouseCursorBackground[Globals.CURRENT_MOUSE_DATA].fRestore = false;
            }
        }
        else
        {
            // Well since there was no mouse handling this round, we disable the mouse restore
            mouseCursorBackground[Globals.CURRENT_MOUSE_DATA].fRestore = false;
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
        var fullRect = new Rectangle(0, 0, SCREEN_WIDTH, SCREEN_HEIGHT);

        //BlitRegion(
        //    gpPrimarySurface,
        //    rcWindow.ToPoint(),
        //    fullRect,
        //    backBuffer);

        // Step (2) - Copy Primary Surface to the Back Buffer
        if (Globals.gfRenderScroll)
        {
            Region.X = 0;
            Region.Y = 0;
            Region.Width = 640;
            Region.Height = 360;

            BlitRegion(
                backBuffer,
                new Point(0, 0),
                Region,
                this.surfaces[Surfaces.PRIMARY_SURFACE]);

            // Get new background for mouse
            // Ok, do the actual data save to the mouse background
            Globals.gfRenderScroll = false;
            Globals.gfScrollStart = false;
        }

        // COPY MOUSE AREAS FROM PRIMARY BACK!

        // FIRST OLD ERASED POSITION
        if (mouseCursorBackground[Globals.PREVIOUS_MOUSE_DATA].fRestore == true)
        {
            Region = mouseCursorBackground[Globals.PREVIOUS_MOUSE_DATA].Region;

            BlitRegion(
                backBuffer,
                new Point(
                    mouseCursorBackground[Globals.PREVIOUS_MOUSE_DATA].usMouseXPos,
                    mouseCursorBackground[Globals.PREVIOUS_MOUSE_DATA].usMouseYPos),
                Region,
                this.surfaces[Surfaces.PRIMARY_SURFACE]);
        }

        // NOW NEW MOUSE AREA
        if (mouseCursorBackground[Globals.CURRENT_MOUSE_DATA].fRestore == true)
        {
            Region = mouseCursorBackground[Globals.CURRENT_MOUSE_DATA].Region;

            BlitRegion(
                backBuffer,
                new Point(
                    mouseCursorBackground[Globals.CURRENT_MOUSE_DATA].usMouseXPos,
                    mouseCursorBackground[Globals.CURRENT_MOUSE_DATA].usMouseYPos),
                Region,
                this.surfaces[Surfaces.PRIMARY_SURFACE]);
        }

        if (Globals.gfForceFullScreenRefresh == true)
        {
            // Method (1) - We will be refreshing the entire screen
            Region.X = 0;
            Region.Y = 0;
            Region.Width = SCREEN_WIDTH;
            Region.Height = SCREEN_HEIGHT;

            BlitRegion(
                backBuffer,
                new Point(0, 0),
                Region,
                this.surfaces[Surfaces.PRIMARY_SURFACE]);

            Globals.guiDirtyRegionCount = 0;
            Globals.guiDirtyRegionExCount = 0;
            Globals.gfForceFullScreenRefresh = false;
        }
        else
        {
            for (uiIndex = 0; uiIndex < Globals.guiDirtyRegionCount; uiIndex++)
            {
                Region.X = Globals.gListOfDirtyRegions[uiIndex].Left;
                Region.Y = Globals.gListOfDirtyRegions[uiIndex].Top;
                Region.Width = Globals.gListOfDirtyRegions[uiIndex].Width;
                Region.Height = Globals.gListOfDirtyRegions[uiIndex].Height;

                BlitRegion(
                    backBuffer,
                    new Point(Region.X, Region.Y),
                    Region,
                    this.surfaces[Surfaces.PRIMARY_SURFACE]);
            }

            Globals.guiDirtyRegionCount = 0;
            Globals.gfForceFullScreenRefresh = false;
        }

        // Do extended dirty regions!
        for (uiIndex = 0; uiIndex < Globals.guiDirtyRegionExCount; uiIndex++)
        {
            Region.X = Globals.gDirtyRegionsEx[uiIndex].Left;
            Region.Y = Globals.gDirtyRegionsEx[uiIndex].Top;
            Region.Width = Globals.gDirtyRegionsEx[uiIndex].Width;
            Region.Height = Globals.gDirtyRegionsEx[uiIndex].Height;

            if ((Region.Y < Globals.gsVIEWPORT_WINDOW_END_Y)
                && Globals.gfRenderScroll)
            {
                continue;
            }

            BlitRegion(
                backBuffer,
                new Point(Region.X, Region.Y),
                Region,
                this.surfaces[Surfaces.PRIMARY_SURFACE]);
        }

        Globals.guiDirtyRegionExCount = 0;

        fFirstTime = false;
    }

    private void DrawRegion(
        Image<Rgba32> destinationTexture,
        int destinationPointX,
        int destinationPointY,
        Rectangle sourceRegion,
        Image<Rgba32> sourceTexture)
        => BlitRegion(
            destinationTexture,
            new Point(destinationPointX, destinationPointY),
            sourceRegion,
            sourceTexture);

    private void BlitRegion(
        Image<Rgba32> texture,
        Point destinationPoint,
        Rectangle sourceRegion,
        Image<Rgba32> srcImage)
    {
        srcImage.Mutate(ctx => ctx.Crop(sourceRegion));

        //var newTexture = new ImageSharpTexture(srcImage)
        //    .CreateDeviceTexture(GraphicDevice, GraphicDevice.ResourceFactory);

        var finalRect = new Rectangle(
            new Point(destinationPoint.X, destinationPoint.Y),
            new Size(sourceRegion.Width, sourceRegion.Height));

        //SpriteRenderer.AddSprite(finalRect, newTexture, srcImage.GetHashCode().ToString());
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
        int usWidth, usHeight;
        int ubBitDepth;
        Rectangle Region = new();
        int usMouseXPos, usMouseYPos;
        Rectangle[] StripRegions = new Rectangle[2];
        Rectangle MouseRegion = new();
        int usNumStrips = 0;
        int cnt;
        int sShiftX, sShiftY;
        int uiCountY;

        GetCurrentVideoSettings(out usWidth, out usHeight, out ubBitDepth);
        usHeight = Globals.gsVIEWPORT_WINDOW_END_Y - Globals.gsVIEWPORT_WINDOW_START_Y;

        StripRegions[0].X = Globals.gsVIEWPORT_START_X;
        StripRegions[0].Width = Globals.gsVIEWPORT_END_X;
        StripRegions[0].Y = Globals.gsVIEWPORT_WINDOW_START_Y;
        StripRegions[0].Height = Globals.gsVIEWPORT_WINDOW_END_Y;

        StripRegions[1].X = Globals.gsVIEWPORT_START_X;
        StripRegions[1].Width = Globals.gsVIEWPORT_END_X;
        StripRegions[1].Y = Globals.gsVIEWPORT_WINDOW_START_Y;
        StripRegions[1].Height = Globals.gsVIEWPORT_WINDOW_END_Y;

        MouseRegion.X = mouseCursorBackground[uiCurrentMouseBackbuffer].usLeft;
        MouseRegion.Y = mouseCursorBackground[uiCurrentMouseBackbuffer].usTop;
        MouseRegion.Width = mouseCursorBackground[uiCurrentMouseBackbuffer].usRight;
        MouseRegion.Height = mouseCursorBackground[uiCurrentMouseBackbuffer].usBottom;

        usMouseXPos = mouseCursorBackground[uiCurrentMouseBackbuffer].usMouseXPos;
        usMouseYPos = mouseCursorBackground[uiCurrentMouseBackbuffer].usMouseYPos;

        switch (uiDirection)
        {
            case ScrollDirection.SCROLL_LEFT:

                Region.X = 0;
                Region.Y = Globals.gsVIEWPORT_WINDOW_START_Y;
                Region.Width = usWidth - sScrollXIncrement;
                Region.Height = Globals.gsVIEWPORT_WINDOW_START_Y + usHeight;

                DrawRegion(
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

                DrawRegion(
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

                DrawRegion(
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

                DrawRegion(
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

                DrawRegion(
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

                BlitRegion(
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

                BlitRegion(
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

                BlitRegion(
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

                BlitRegion(
                    pDest,
                    new Point(StripRegions[cnt].X, StripRegions[cnt].Y),
                    StripRegions[cnt],
                    this.surfaces[Surfaces.FRAME_BUFFER]);
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
            RestoreShiftedVideoOverlays(sShiftX, sShiftY);

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
        clearScreen = true;

        Globals.guiDirtyRegionCount = 0;
        Globals.guiDirtyRegionExCount = 0;
        Globals.gfForceFullScreenRefresh = true;
        Globals.guiFrameBufferState = BufferState.DIRTY;
    }

    public void Dispose()
    {
        GraphicDevice.WaitForIdle();
        (Factory as DisposeCollectorResourceFactory)!.DisposeCollector.DisposeAll();
        GraphicDevice.Dispose();

        GC.SuppressFinalize(this);
    }

    public static void InvalidateRegion(Rectangle bounds)
    {

    }

    public void EndFrameBufferRender()
    {
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

    public HVOBJECT GetVideoObject(string key)
    {
        if (!loadedTextures.TryGetValue(key, out var hPixHandle))
        {
            //logger.LogError("Unable to retrive VideoObject with key: " + key);
        }

        return hPixHandle;
    }

    public void BltVideoObject(HVOBJECT hVObject, int regionIndex, int X, int Y, int textureIndex)
    {
        if (hVObject.Textures is null)
        {
            throw new NullReferenceException("Texture is null for: " + hVObject.Name);
        }

        //SpriteRenderer.AddSprite(
        //    new Rectangle(X, Y, (int)hVObject.Textures[textureIndex].Width, (int)hVObject.Textures[textureIndex].Height),
        //    hVObject.Textures[textureIndex],
        //    $"{hVObject.Name}_{textureIndex}");
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

    public bool GetVideoSurface(out HVSURFACE hSrcVSurface, uint uiTempMap)
    {
        throw new NotImplementedException();
    }

    public void AddVideoObject(out VSURFACE_DESC vs_desc, out uint uiTempMap)
    {
        vs_desc = new();
        uiTempMap = 0;
    }

    public void GetVSurfacePaletteEntries(HVSURFACE hSrcVSurface, List<SGPPaletteEntry> pPalette)
    {
    }

    public ushort[] Create16BPPPaletteShaded(List<SGPPaletteEntry> pPalette, int redScale, int greenScale, int blueScale, bool mono)
    {
        ushort[] p16BPPPalette = new ushort[256];
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

            if (gusRedShift < 0)
            {
                r16 = (ushort)(r >> (-gusRedShift));
            }
            else
            {
                r16 = (ushort)(r << gusRedShift);
            }

            if (gusGreenShift < 0)
            {
                g16 = (ushort)(g >> (-gusGreenShift));
            }
            else
            {
                g16 = (ushort)(g << gusGreenShift);
            }


            if (gusBlueShift < 0)
            {
                b16 = (ushort)(b >> (-gusBlueShift));
            }
            else
            {
                b16 = (ushort)(b << gusBlueShift);
            }

            // Prevent creation of pure black color
            usColor = (ushort)((r16 & gusRedMask) | (g16 & gusGreenMask) | (b16 & gusBlueMask));

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

    public void DeleteVideoSurfaceFromIndex(Surfaces uiTempMap)
    {
    }

    public void DeleteVideoObjectFromIndex(string key)
    {
        //loadedTextures.Remove(logoKey);
    }

    public void LineDraw(int v2, int v3, int v4, int v5, Color color, Image<Rgba32> image)
    {
    }

    public void SetClippingRegionAndImageWidth(uint uiDestPitchBYTES, int v1, int v2, int v3, int v4)
    {
    }

    public void Blt16BPPBufferHatchRect(ref byte[] pDestBuf, uint uiDestPitchBYTES, ref Rectangle clipRect)
    {
    }

    public void ColorFillVideoSurfaceArea(Surfaces buttonDestBuffer, int regionTopLeftX, int regionTopLeftY, int regionBottomRightX, int regionBottomRightY, Rgba32 rgba32)
    {
    }

    public void ImageFillVideoSurfaceArea(Surfaces buttonDestBuffer, int v1, int v2, int regionBottomRightX, int regionBottomRightY, HVOBJECT hVOBJECT, ushort v3, short v4, short v5)
    {
    }

    public void Blt8BPPDataTo8BPPBufferTransparentClip(ref byte[] pDestBuf, uint uiDestPitchBYTES, HVOBJECT bPic, int v, int yLoc, ushort imgNum, ref Rectangle clipRect)
    {
    }

    public void GetClippingRect(out Rectangle clipRect)
    {
        clipRect = new();
    }

    public void SetClippingRect(ref Rectangle newClip)
    {
    }

    public void ColorFillVideoSurfaceArea(Rectangle rectangle, Color color)
        => ColorFillVideoSurfaceArea(rectangle, color.ToPixel<Rgba32>());

    public void ColorFillVideoSurfaceArea(Rectangle region, Rgba32 rgba32)
    {

    }

    public void ImageFillVideoSurfaceArea(Rectangle region, HVOBJECT hVOBJECT, ushort v3, short v4, short v5)
    {
    }

    public bool ShadowVideoSurfaceRectUsingLowPercentTable(Surfaces destSurface, Rectangle rectangle)
    {
        return InternalShadowVideoSurfaceRect(destSurface, rectangle, true);

    }

    private bool InternalShadowVideoSurfaceRect(Surfaces destSurface, Rectangle rectangle, bool fLowPercentShadeTable)
    {
        Image<Rgba32> pBuffer;
        int uiPitch;
        Rectangle area;
        HVSURFACE hVSurface;

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
        CHECKF(GetVideoSurface(out hVSurface, destSurface));

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

        if (X2 >= hVSurface.usWidth)
        {
            X2 = hVSurface.usWidth - 1;
        }

        if (Y2 >= hVSurface.usHeight)
        {
            Y2 = hVSurface.usHeight - 1;
        }

        if (X1 >= hVSurface.usWidth)
        {
            return (false);
        }

        if (Y1 >= hVSurface.usHeight)
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
        pBuffer = LockVideoSurface(destSurface, out uiPitch);
        //UnLockVideoSurface( uiDestVSurface );


        if (!fLowPercentShadeTable)
        {
            // Now we have the video object and surface, call the shadow function
            if (!Blitters.Blt16BPPBufferShadowRect(pBuffer, uiPitch, area))
            {
                // Blit has failed if false returned
                return (false);
            }
        }
        else
        {
            // Now we have the video object and surface, call the shadow function
            if (!Blitters.Blt16BPPBufferShadowRectAlternateTable(pBuffer, uiPitch, area))
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

        UnLockVideoSurface(destSurface);
        return (true);
    }

    public void RestoreBackgroundRects()
    {
    }

    public void SaveBackgroundRects()
    {
    }

    public void ExecuteBaseDirtyRectQueue()
    {
    }

    public void DeleteVideoObject(HVOBJECT? vobj)
    {
    }

    public bool BlitBufferToBuffer(Surfaces srcBuffer, Surfaces dstBuffer, int srcX, int srcY, int width, int height)
    {
        int uiDestPitchBYTES, uiSrcPitchBYTES;
        Image<Rgba32> pDestBuf, pSrcBuf;
        bool fRetVal;

        pDestBuf = LockVideoSurface(dstBuffer, out uiDestPitchBYTES);
        pSrcBuf = LockVideoSurface(srcBuffer, out uiSrcPitchBYTES);

        fRetVal = Blt16BPPTo16BPP(pDestBuf, uiDestPitchBYTES, pSrcBuf, uiSrcPitchBYTES,
                srcX, srcY,
                srcX, srcY,
                width, height);

        UnLockVideoSurface(dstBuffer);
        UnLockVideoSurface(srcBuffer);

        return (fRetVal);
    }

    public int AddVideoObject(out VSURFACE_DESC vs_desc, out Surfaces uiTempMap)
    {
        vs_desc = new();
        uiTempMap = Surfaces.FRAME_BUFFER;

        return 0;
    }

    public void ColorFillVideoSurfaceArea(Image<Rgba32> surface, Rectangle region, Color rgba32)
    {
    }

    public void ColorFillVideoSurfaceArea(Image<Rgba32> surface, Rectangle region, Rgba32 rgba32)
    {
    }

    public void ColorFillVideoSurfaceArea(Surfaces surface, Rectangle rectangle, Color color)
    {
    }

    public void SetVideoSurfaceTransparency(Surfaces uiVideoSurfaceImage, Rgba32 pixel)
    {
    }

    public bool GetVideoSurface(out HVSURFACE hSrcVSurface, Surfaces uiTempMap)
    {
        hSrcVSurface = new();

        return true;
    }

    public void ClearElements()
    {
        FontSubSystem.TextRenderer.ClearText();
    }

    public Image<Rgba32> LockVideoSurface(Surfaces buffer, out int uiSrcPitchBYTES)
    {
        uiSrcPitchBYTES = buffer switch
        {
            Surfaces.PRIMARY_SURFACE => 128,
            Surfaces.BACKBUFFER => 128,
            Surfaces.FRAME_BUFFER => 1280,
            Surfaces.MOUSE_BUFFER => 128,
            Surfaces.Unknown => 0,
            _ => 0,
        };

        return this.surfaces.LockSurface(buffer);
    }

    public void UnLockVideoSurface(Surfaces surface)
    {
        this.surfaces.UnlockSurface(surface);
    }

    public void UnLockVideoSurface(Image<Rgba32> buffer)
    {
        this.surfaces.UnlockSurface(buffer);
    }

    public void InvalidateRegionEx(int sLeft, int sTop, int v1, int v2, int v3)
    {
    }

    public void Blt8BPPTo8BPP(Image<Rgba32> pDestBuf, int uiDestPitchBYTES, Image<Rgba32> pSrcBuf, int uiSrcPitchBYTES, int sLeft1, int sTop1, int sLeft2, int sTop2, int sWidth, int sHeight)
    {
        throw new NotImplementedException();
    }

    public void DeleteVideoObjectFromIndex(Surfaces guiWoodBackground)
    {
        throw new NotImplementedException();
    }

    public static void InvalidateRegion(int v1, int v2, int v3, int v4) => InvalidateRegion(new(v1, v2, v3, v4));

    public void Blt8BPPDataSubTo16BPPBuffer(Image<Rgba32> pDestBuf, int uiDestPitchBYTES, HVSURFACE hSrcVSurface, Image<Rgba32> pSrcBuf, int uiSrcPitchBYTES, int v1, int v2, out Rectangle clip)
    {
        clip = new Rectangle(0, 0, 100, 100);
    }

    public bool TryCreateVideoSurface(VSURFACE_DESC vs_desc, out Surfaces uiVideoSurfaceImage)
    {
        uiVideoSurfaceImage = this.surfaces.CreateSurface(width: vs_desc.usWidth, height: vs_desc.usHeight);

        return uiVideoSurfaceImage > 0;

    }

    public bool GetVideoObjectETRLEPropertiesFromIndex(string uiVideoObject, out ETRLEObject pETRLEObject, int usIndex)
    {

        var hVObject = GetVideoObject(uiVideoObject);

        GetVideoObjectETRLEProperties(hVObject, out pETRLEObject, usIndex);

        return true;
    }

    public bool GetVideoObjectETRLEProperties(HVOBJECT hVObject, out ETRLEObject pETRLEObject, int usIndex)
    {
        CHECKF(usIndex >= 0);
        CHECKF(usIndex < hVObject.usNumberOfObjects);

        pETRLEObject = (hVObject.pETRLEObject[usIndex]);
        return true;
    }

    public bool BltVideoObjectFromIndex(Surfaces uiDestVSurface, int uiSrcVObject, int usRegionIndex, int iDestX, int iDestY, VO_BLT fBltFlags, blt_fx? pBltFx)
    {
        Image<Rgba32> pBuffer;
        int uiPitch;
        HVOBJECT hSrcVObject;

        // Lock video surface
        pBuffer = LockVideoSurface(uiDestVSurface, out uiPitch);

        // Get video object
        if (!GetVideoObject(out hSrcVObject, uiSrcVObject))
        {
            UnLockVideoSurface(uiDestVSurface);
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
            UnLockVideoSurface(uiDestVSurface);
            // VO Blitter will set debug messages for error conditions
            return false;
        }

        UnLockVideoSurface(uiDestVSurface);
        return true;
    }

    public Image<Rgba32> AddVideoSurface(string assetPath, out Surfaces surface)
    {
        var hobj = this.CreateVideoObject(assetPath);
        surface = this.surfaces.CreateSurface(hobj.hImage.ParsedImages[0]);

        return this.surfaces[surface];
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

public static class RectangleHelpers
{
    public static Rectangle ToVeldridRectangle(this Rectangle rectangle)
        => new(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);

    public static Point ToPoint(this Rectangle rect) => new(rect.X, rect.Y);
    public static Vector2 ToVector2(this Point point) => new(point.X, point.Y);
}
