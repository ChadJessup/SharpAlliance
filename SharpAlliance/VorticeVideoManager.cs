using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SharpAlliance.Core.Managers.Image;
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
        }

        private readonly ILogger<VorticeVideoManager> logger;
        private readonly WindowsSubSystem windows;
        private readonly IInputManager inputManager;
        private readonly GameContext context;

        private IGraphicsDevice graphicsDevice;

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
        //
        // Direct Draw objects for both the Primary and Backbuffer surfaces
        //

        // private LPDIRECTDRAW? _gpDirectDrawObject = null;
        // private LPDIRECTDRAW2 gpDirectDrawObject = null;

        //private Surface? _gpPrimarySurface = null;
        //private Surface2? gpPrimarySurface = null;
        //private Surface2? gpBackBuffer = null

        public bool IsInitialized { get; private set; }

        public VorticeVideoManager(
            ILogger<VorticeVideoManager> logger,
            GameContext context,
            IInputManager inputManager)
        {
            this.logger = logger;
            this.context = context;
            this.inputManager = inputManager;
        }

        public async ValueTask<bool> Initialize()
        {
            if (!this.inputManager.IsInitialized)
            {
                await this.inputManager.Initialize();
            }

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
            this.graphicsDevice.DrawFrame((w,h) =>
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
    }
}
