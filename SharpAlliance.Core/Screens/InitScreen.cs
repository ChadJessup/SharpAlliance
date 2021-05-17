using System.Threading.Tasks;
using SharpAlliance.Core.Interfaces;
using SharpAlliance.Core.Managers;
using SharpAlliance.Core.SubSystems;
using SharpAlliance.Platform;
using SharpAlliance.Platform.Interfaces;
using Veldrid;
using Rectangle = SixLabors.ImageSharp.Rectangle;

namespace SharpAlliance.Core.Screens
{
    // jascreens.c = InitScreen = splashscreen
    public class InitScreen : IScreen
    {
        private readonly Overhead overhead;
        private readonly GameContext context;
        private readonly VeldridVideoManager video;
        private readonly CursorSubSystem cursor;
//        private readonly IVideoSurfaceManager videoSurface;
        private readonly FontSubSystem font;
        private readonly TileCache tileCache;
        private readonly MercTextBox mercTextBox;
        private readonly IScreenManager screen;
        private readonly IFileManager fileManager;
        private readonly TextUtils textUtils;
        private readonly ISoundManager sounds;
        private readonly RenderWorld renderWorld;
        private readonly AnimationData animationData;
        private readonly LightingSystem lighting;
        private readonly DialogControl dialogs;
        private readonly IMusicManager music;
        private readonly World world;
        private readonly Shading shading;
        private readonly StrategicMap strategicMap;
        private readonly EventManager eventManager;
        private readonly GameInit gameInit;

        public InitScreen(
            GameInit gameInit,
            Overhead overhead,
            StrategicMap strategicMap,
            World world,
            GameContext context,
            CursorSubSystem cursorSubSystem,
            IVideoManager videoManager,
            // IVideoSurfaceManager videoSurfaceManager,
            FontSubSystem fontSubSystem,
            EventManager eventManager,
            IScreenManager sm,
            IFileManager fileManager,
            Shading shading,
            TextUtils textUtils,
            ISoundManager soundManager,
            RenderWorld renderWorld,
            AnimationData animationData,
            TileCache tileCache,
            MercTextBox mercTextBox,
            LightingSystem lightingSystem,
            DialogControl dialogControl,
            IMusicManager musicManager)
        {
            this.overhead = overhead;
            this.eventManager = eventManager;
            this.shading = shading;
            this.strategicMap = strategicMap;
            this.world = world;
            this.context = context;
            this.video = (videoManager as VeldridVideoManager)!;
            this.cursor = cursorSubSystem;
            // this.videoSurface = videoSurfaceManager;
            this.font = fontSubSystem;
            this.tileCache = tileCache;
            this.mercTextBox = mercTextBox;
            this.screen = sm;
            this.fileManager = fileManager;
            this.textUtils = textUtils;
            this.lighting = lightingSystem;
            this.gameInit = gameInit;
            this.sounds = soundManager;
            this.renderWorld = renderWorld;
            this.music = musicManager;
            this.animationData = animationData;
            this.dialogs = dialogControl;
        }

        public bool IsInitialized { get; set; }
        public ScreenState State { get; set; } = ScreenState.Unknown;

        public ValueTask Activate()
        {
            return ValueTask.CompletedTask;
        }

        public ValueTask<bool> Initialize()
        {
            hVObject = this.video.AddVideoObject("ja2_logo.STI", out var key);

            return ValueTask.FromResult(true);
        }

        public static HVOBJECT hVObject;
        public static byte ubCurrentScreen = 255;

        public async ValueTask<ScreenName> Handle()
        {
            if (ubCurrentScreen == 255)
            {
                // TODO: read when smacker comes in.
                if (ScreenManager.gfDoneWithSplashScreen)
                {
                    ubCurrentScreen = 0;
                }
                else
                {
                    this.cursor.SetCurrentCursorFromDatabase(Cursor.VIDEO_NO_CURSOR);
                    return ScreenName.INTRO_SCREEN;
                }
            }

            if (ubCurrentScreen == 0)
            {
                // Load init screen and blit!
                //vs_desc.fCreateFlags = VideoObjectCreateFlags.VOBJECT_CREATE_FROMFILE;// | VSurfaceCreateFlags.VSURFACE_SYSTEM_MEM_USAGE;

                // vs_desc.ImageFile = "ja2_logo.STI";
                // hVObject = this.video.AddVideoObject(ref vs_desc, out var key);
                // 
                // if (hVObject is null)
                // {
                //     //AssertMsg(0, "Failed to load ja2_logo.sti!");
                //     return ScreenName.ERROR_SCREEN;
                // }

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
                // this.videoSurface.DeleteVideoSurface(hVSurface);
                //ATE: Set to true to reset before going into main screen!

                this.cursor.SetCurrentCursorFromDatabase(Cursor.VIDEO_NO_CURSOR);

                return ScreenName.InitScreen;
            }

            if (ubCurrentScreen == 1)
            {
                ubCurrentScreen = 2;
                return await this.InitializeJA2();
            }

            if (ubCurrentScreen == 2)
            {
                var mainMenuScreen = await this.screen.GetScreen<MainMenuScreen>(ScreenName.MAINMENU_SCREEN, activate: true);
                await mainMenuScreen.InitMainMenu();

                ubCurrentScreen = 3;
                return ScreenName.InitScreen;
            }

            // Let one frame pass....
            if (ubCurrentScreen == 3)
            {
                ubCurrentScreen = 4;

                this.cursor.SetCurrentCursorFromDatabase(Cursor.VIDEO_NO_CURSOR);
                return ScreenName.InitScreen;
            }

            if (ubCurrentScreen == 4)
            {
                this.cursor.SetCurrentCursorFromDatabase(Cursor.VIDEO_NO_CURSOR);
                // TODO: Strategic stuff
                await this.gameInit.InitNewGame(false);
            }

            return ScreenName.InitScreen;
        }

        public async ValueTask<ScreenName> InitializeJA2()
        {
            await this.textUtils.LoadAllExternalText();
            await this.sounds.InitSound();

            this.renderWorld.gsRenderCenterX = 805;
            this.renderWorld.gsRenderCenterY = 805;

            if (!await this.animationData.InitAnimationSystem())
            {
                return ScreenName.ERROR_SCREEN;
            }

            await this.lighting.InitLightingSystem();

            // Init dialog queue system
            await this.dialogs.InitalizeDialogueControl();

            if (!await this.strategicMap.InitStrategicEngine())
            {
                return ScreenName.ERROR_SCREEN;
            }

            //needs to be called here to init the SectorInfo struct
            await this.strategicMap.InitStrategicMovementCosts();

            // Init tactical engine
            if (!await this.overhead.InitTacticalEngine())
            {
                return ScreenName.ERROR_SCREEN;
            }

            // Init timer system
            //Moved to the splash screen code.
            //InitializeJA2Clock( );

            // INit shade tables
            await this.shading.BuildShadeTable();

            // INit intensity tables
            await this.shading.BuildIntensityTable();

            // Init Event Manager
            if (!await this.eventManager.InitializeEventManager())
            {
                return ScreenName.ERROR_SCREEN;
            }

            // Initailize World
            if (!await this.world.InitializeWorld())
            {
                return ScreenName.ERROR_SCREEN;
            }

            bool s = await this.tileCache.InitTileCache();

            bool t = await this.mercTextBox.InitMercPopupBox();

            // Set global volume
            this.music.MusicSetVolume(1);// gGameSettings.ubMusicVolumeSetting);

            await this.shading.DetermineRGBDistributionSettings();


            return ScreenName.InitScreen;
        }

        public void Dispose()
        {
        }

        public void Draw(SpriteRenderer sr, GraphicsDevice gd, CommandList cl)
        {
            sr.AddSprite(new Rectangle(0, 0, 640, 480), hVObject.Textures[0], "SplashScreen");
        }

        public ValueTask Deactivate()
        {
            return ValueTask.CompletedTask;
        }
    }
}
