using SharpAlliance.Platform.Interfaces;
using System;
using System.Diagnostics;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Windows;

using AlphaMode = SharpDX.Direct2D1.AlphaMode;
using Device = SharpDX.Direct3D11.Device;
using Factory = SharpDX.DXGI.Factory;
using SharpDX.Mathematics.Interop;
using System.Collections.Generic;
using SharpAlliance.Platform;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Windows.Forms;

namespace SharpAlliance
{
    public class DirectDrawVideoManager : IVideoManager
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
        }

        private readonly ILogger<DirectDrawVideoManager> logger;
        private readonly IInputManager inputManager;
        private readonly GameContext context;

        private int gusScreenWidth;
        private int gusScreenHeight;
        private int gubScreenPixelDepth;

        private Rectangle gScrollRegion;

        private bool gfVideoCapture = false;
        private int guiFramePeriod = (1000 / 15);
        private int guiLastFrame;
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
        private int guiVideoManagerState;   // VIDEO_ON, VIDEO_OFF, VIDEO_SUSPENDED, VIDEO_SHUTTING_DOWN
        private int guiRefreshThreadState;  // THREAD_ON, THREAD_OFF, THREAD_SUSPENDED

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

        private RenderTarget? d2dRenderTarget;
        private RenderTargetView? renderView;
        private Texture2D? backBuffer;
        private SwapChain? swapChain;
        private RenderForm? form;
        private Factory? factory;
        private Device? device;

        public bool IsInitialized { get; private set; }

        public DirectDrawVideoManager(
            ILogger<DirectDrawVideoManager> logger,
            GameContext context,
            IInputManager inputManager)
        {
            this.logger = logger;
            this.context = context;
            this.inputManager = inputManager;
        }

        public async ValueTask<bool> Initialize()
        {
            this.form = new RenderForm("Sharp Alliance!")
            {
                Width = 640,
                Height = 480,
            };

            if (!this.inputManager.IsInitialized)
            {
                await this.inputManager.Initialize();
            }

            this.HookupInputs(this.inputManager);

            // SwapChain description
            var desc = new SwapChainDescription()
            {
                BufferCount = 1,
                ModeDescription = new ModeDescription(
                    this.form.ClientSize.Width,
                    this.form.ClientSize.Height,
                    new Rational(60, 1), Format.R8G8B8A8_UNorm),
                IsWindowed = true,
                OutputHandle = this.form.Handle,
                SampleDescription = new SampleDescription(1, 0),
                SwapEffect = SwapEffect.Discard,
                Usage = Usage.RenderTargetOutput
            };

            // Create Device and SwapChain
            Device.CreateWithSwapChain(DriverType.Hardware, DeviceCreationFlags.BgraSupport, new SharpDX.Direct3D.FeatureLevel[] { SharpDX.Direct3D.FeatureLevel.Level_10_0 }, desc, out this.device, out this.swapChain);

            var d2dFactory = new SharpDX.Direct2D1.Factory();

            int width = this.form.ClientSize.Width;
            int height = this.form.ClientSize.Height;

            // Ignore all windows events
            this.factory = this.swapChain.GetParent<Factory>();
            this.factory.MakeWindowAssociation(this.form.Handle, WindowAssociationFlags.None);

            // New RenderTargetView from the backbuffer
            this.backBuffer = Texture2D.FromSwapChain<Texture2D>(this.swapChain, 0);
            this.renderView = new(this.device, this.backBuffer);

            Surface surface = this.backBuffer.QueryInterface<Surface>();

            this.d2dRenderTarget = new RenderTarget(
                d2dFactory,
                surface,
                new RenderTargetProperties(new PixelFormat(Format.Unknown, AlphaMode.Premultiplied)));

            var solidColorBrush = new SolidColorBrush(this.d2dRenderTarget, new RawColor4(255, 255, 255, 255));

            Stopwatch stopwatch = new();
            stopwatch.Start();

            var rectangleGeometry = new RoundedRectangleGeometry(
                d2dFactory,
                new RoundedRectangle()
                {
                    RadiusX = 32,
                    RadiusY = 32,
                    Rect = new RectangleF(128, 128, width - 128 * 2, height - 128 * 2)
                });

            RenderLoop.Run(this.form, () =>
            {
                this.d2dRenderTarget.BeginDraw();
                this.d2dRenderTarget.Clear(new RawColor4(0, 0, 0, 255));
                solidColorBrush.Color = new Color4(1, 1, 1, (float)Math.Abs(Math.Cos(stopwatch.ElapsedMilliseconds * .001)));
                this.d2dRenderTarget.FillGeometry(rectangleGeometry, solidColorBrush, null);
                this.d2dRenderTarget.EndDraw();

                this.swapChain.Present(0, PresentFlags.None);
            });

            this.IsInitialized = true;
            return this.IsInitialized;
        }

        private void HookupInputs(IInputManager inputManager)
        {
            if (this.form is not null)
            {
                this.form.KeyDown += this.KeyboardEvent;
                this.form.KeyUp += this.KeyboardEvent;
            }

            //form.mou
        }
        private void KeyboardEvent(object? o, KeyEventArgs e)
        {

        }

        public void Dispose()
        {
            // Unhook events...
            if (this.form is not null)
            {
                this.form.KeyUp -= this.KeyboardEvent;
                this.form.KeyDown -= this.KeyboardEvent;
            }

            // Release all resources
            this.renderView?.Dispose();
            this.backBuffer?.Dispose();
            this.device?.ImmediateContext.ClearState();
            this.device?.ImmediateContext.Flush();
            this.device?.Dispose();
            this.device?.Dispose();
            this.swapChain?.Dispose();
            this.factory?.Dispose();

            GC.SuppressFinalize(this);
        }
    }
}
