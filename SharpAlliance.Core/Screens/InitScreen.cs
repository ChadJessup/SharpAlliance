using System;
using System.Threading.Tasks;
using SharpAlliance.Core.Interfaces;
using SharpAlliance.Core.Managers;
using SharpAlliance.Core.Managers.VideoSurfaces;
using SharpAlliance.Core.SubSystems;
using SharpAlliance.Platform;
using SharpAlliance.Platform.Interfaces;

namespace SharpAlliance.Core.Screens
{
    // jascreens.c = InitScreen = splashscreen
    public class InitScreen : IScreen
    {
        private readonly GameContext context;
        private readonly IVideoManager video;
        private readonly CursorSubSystem cursor;
        private readonly IVideoSurfaceManager videoSurface;
        private readonly FontSubSystem font;
        private readonly IScreenManager screen;
        private readonly IFileManager fileManager;

        public InitScreen(
            GameContext context,
            CursorSubSystem cursorSubSystem,
            IVideoSurfaceManager videoSurfaceManager,
            FontSubSystem fontSubSystem,
            IScreenManager sm,
            IFileManager fileManager)
        {
            this.context = context;
            this.video = this.context.VideoManager;
            this.cursor = cursorSubSystem;
            this.videoSurface = videoSurfaceManager;
            this.font = fontSubSystem;
            this.screen = sm;
            this.fileManager = fileManager;
        }

        public bool IsInitialized { get; set; }
        public ScreenState State { get; set; } = ScreenState.Unknown;

        public ValueTask Activate()
        {
            return ValueTask.CompletedTask;
        }

        public ValueTask<bool> Initialize()
        {

            return ValueTask.FromResult(true);
        }
        public static HVSURFACE? hVSurface;
        public static byte ubCurrentScreen = 255;

        public async ValueTask<ScreenName> Handle()
        {
            VSURFACE_DESC vs_desc = new();

            if (ubCurrentScreen == 255)
            {
                ubCurrentScreen = 0;
                // TODO: read when smacker comes in.
                //if (ScreenManager.gfDoneWithSplashScreen)
                //{
                //}
                //else
                //{
                //    this.cursor.SetCurrentCursorFromDatabase(IVideoManager.VIDEO_NO_CURSOR);
                //    return ScreenName.INTRO_SCREEN;
                //}
            }

            if (ubCurrentScreen == 0)
            {
                //if (strcmp(gzCommandLine, "-NODD") == 0)
                //{
                //    gfDontUseDDBlits = true;
                //}

                // Load version number....
                //HandleLimitedNumExecutions( );

                // Load init screen and blit!
                vs_desc.fCreateFlags = VSurfaceCreateFlags.VSURFACE_CREATE_FROMFILE | VSurfaceCreateFlags.VSURFACE_SYSTEM_MEM_USAGE;

                vs_desc.ImageFile = "ja2_logo.STI";

                hVSurface = await this.videoSurface.CreateVideoSurface(vs_desc, this.fileManager);
                if (hVSurface is null)
                {
                    //AssertMsg(0, "Failed to load ja2_logo.sti!");
                }

                //BltVideoSurfaceToVideoSurface( ghFrameBuffer, hVSurface, 0, 0, 0, VS_BLT_FAST, NULL );
                ubCurrentScreen = 1;

                // Init screen

                // Set Font
                this.font.SetFont(FontStyle.TINYFONT1);
                this.font.SetFontBackground(FontColor.FONT_MCOLOR_BLACK);
                this.font.SetFontForeground(FontColor.FONT_MCOLOR_WHITE);

                //mprintf( 10, 420, zVersionLabel );

                //mprintf(10, 430, L"%s: %s (Debug %S)", pMessageStrings[MSG_VERSION], zVersionLabel, czVersionNumber);


                //mprintf(10, 440, L"SOLDIERTYPE: %d bytes", sizeof(SOLDIERTYPE));

                //if (gfDontUseDDBlits)
                //{
                //  //  mprintf(10, 450, L"SOLDIERTYPE: %d bytes", sizeof(SOLDIERTYPE));
                //}

                this.video.InvalidateScreen();

                // Delete video Surface
                this.videoSurface.DeleteVideoSurface(hVSurface);
                //ATE: Set to true to reset before going into main screen!

                this.cursor.SetCurrentCursorFromDatabase(IVideoManager.VIDEO_NO_CURSOR);

                return ScreenName.InitScreen;
            }

            if (ubCurrentScreen == 1)
            {
                ubCurrentScreen = 2;
                return await this.InitializeJA2();
            }

            if (ubCurrentScreen == 2)
            {
                var mainMenuScreen = (await this.screen.GetScreen(ScreenName.MAINMENU_SCREEN, activate: true) as MainMenuScreen)!;
                mainMenuScreen.InitMainMenu();
                ubCurrentScreen = 3;
                return ScreenName.InitScreen;
            }

            // Let one frame pass....
            if (ubCurrentScreen == 3)
            {
                ubCurrentScreen = 4;

                this.cursor.SetCurrentCursorFromDatabase(IVideoManager.VIDEO_NO_CURSOR);
                return ScreenName.InitScreen;
            }

            if (ubCurrentScreen == 4)
            {
                this.cursor.SetCurrentCursorFromDatabase(IVideoManager.VIDEO_NO_CURSOR);
                // TODO: Strategic stuff
                // InitNewGame(false);
            }

            return ScreenName.InitScreen;
        }

        public ValueTask<ScreenName> InitializeJA2()
        {
            return ValueTask.FromResult(ScreenName.InitScreen);
        }

        public void Dispose()
        {
        }
    }
}
