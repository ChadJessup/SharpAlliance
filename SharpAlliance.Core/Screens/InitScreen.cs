using System.Threading.Tasks;
using SharpAlliance.Core.Interfaces;
using SharpAlliance.Core.Managers;
using SharpAlliance.Core.SubSystems;
using SharpAlliance.Platform;
using SharpAlliance.Platform.Interfaces;
using Veldrid;
using Rectangle = SixLabors.ImageSharp.Rectangle;

using static SharpAlliance.Core.Globals;
using System;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using SharpAlliance.Core.Managers.VideoSurfaces;

namespace SharpAlliance.Core.Screens;

// jascreens.c = InitScreen = splashscreen
public class InitScreen : IScreen
{
    private const string LogoAsset = "ja2_logo.STI";

    private readonly SoldierProfileSubSystem soldierProfiles;
    private readonly Overhead overhead;
    private readonly GameContext context;
    private readonly CursorSubSystem cursor;
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
    private readonly IVideoManager video;
    private readonly Shading shading;
    private readonly SaveLoadGame saveLoadGame;
    private readonly TextureManager textures;
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
        TextureManager textureManager,
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
        SoldierProfileSubSystem soldierProfileSubSystem,
        LightingSystem lightingSystem,
        IVideoManager videoManager,
        DialogControl dialogControl,
        SaveLoadGame saveLoadGame,
        IMusicManager musicManager)
    {
        this.soldierProfiles = soldierProfileSubSystem;
        this.overhead = overhead;
        this.eventManager = eventManager;
        this.shading = shading;
        this.saveLoadGame = saveLoadGame;
        this.textures = textureManager;
        this.strategicMap = strategicMap;
        this.world = world;
        video = videoManager;
        this.context = context;
        this.cursor = cursorSubSystem;
//        this.videoSurface = videoSurfaceManager;
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
        hVObject = video.GetVideoObject(LogoAsset, out var key);
       // HVOBJECT logo = this.textures.LoadTexture(LogoAsset);

        return ValueTask.FromResult(true);
    }

    public static HVOBJECT hVObject;
    public static byte ubCurrentScreen = 255;

    public async ValueTask<ScreenName> Handle()
    {
        if (ubCurrentScreen == 255)
        {
            // TODO: read when smacker comes in.
            if (Globals.gfDoneWithSplashScreen)
            {
                ubCurrentScreen = 0;
            }
            else
            {
                CursorSubSystem.SetCurrentCursorFromDatabase(CURSOR.VIDEO_NO_CURSOR);
                return ScreenName.INTRO_SCREEN;
            }
        }

        if (ubCurrentScreen == 0)
        {
            // Load init screen and blit!
            //vs_desc.fCreateFlags = VideoObjectCreateFlags.VOBJECT_CREATE_FROMFILE;// | VSurfaceCreateFlags.VSURFACE_SYSTEM_MEM_USAGE;

            // vs_desc.ImageFile = "ja2_logo.STI";
            //var hVObject = video.GetVideoObject("ja2_logo.STI", out var key);
            // 
            // if (hVObject is null)
            // {
            //     //AssertMsg(0, "Failed to load ja2_logo.sti!");
            //     return ScreenName.ERROR_SCREEN;
            // }

            //video.BlitSurfaceToSurface(hVObject.Images[0], SurfaceType.FRAME_BUFFER, new(0, 0), VO_BLT.DESTTRANSPARENCY, false);
            ubCurrentScreen = 1;

            // Init screen

            // Set Font
            FontSubSystem.SetFont(FontStyle.TINYFONT1);
            FontSubSystem.SetFontBackground(FontColor.FONT_MCOLOR_BLACK);
            FontSubSystem.SetFontForeground(FontColor.FONT_MCOLOR_WHITE);

            //mprintf( 10, 420, zVersionLabel );

            //mprintf(10, 430, "%s: %s (Debug %S)", pMessageStrings[MSG.VERSION], zVersionLabel, czVersionNumber);


            //mprintf(10, 440, "SOLDIERTYPE: %d bytes", sizeof(SOLDIERTYPE));

            //if (gfDontUseDDBlits)
            //{
            //  //  mprintf(10, 450, "SOLDIERTYPE: %d bytes", sizeof(SOLDIERTYPE));
            //}

            video.InvalidateScreen();

            // Delete video Surface
            // this.videoSurface.DeleteVideoSurface(hVSurface);
            //ATE: Set to true to reset before going into main screen!

            CursorSubSystem.SetCurrentCursorFromDatabase(CURSOR.VIDEO_NO_CURSOR);

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

            CursorSubSystem.SetCurrentCursorFromDatabase(CURSOR.VIDEO_NO_CURSOR);
            return ScreenName.InitScreen;
        }

        if (ubCurrentScreen == 4)
        {
            CursorSubSystem.SetCurrentCursorFromDatabase(CURSOR.VIDEO_NO_CURSOR);
            // TODO: Strategic stuff
            await this.gameInit.InitNewGame(false);
        }

        return ScreenName.InitScreen;
    }

    public async ValueTask<ScreenName> InitializeJA2()
    {
        await this.textUtils.LoadAllExternalText();
        await this.sounds.InitSound();

        await this.LoadFiles();

        this.dialogs.InitalizeStaticExternalNPCFaces();

        gsRenderCenterX = 805;
        gsRenderCenterY = 805;

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
        StrategicMovementCosts.InitStrategicMovementCosts();

        // Init tactical engine
        if (!await Overhead.InitTacticalEngine())
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

        bool t = await MercTextBox.InitMercPopupBox();

        // Set global volume
        this.music.MusicSetVolume(1);// gGameSettings.ubMusicVolumeSetting);

        await this.shading.DetermineRGBDistributionSettings();


        return ScreenName.InitScreen;
    }

    private Task LoadFiles()
    {
        this.soldierProfiles.LoadMercProfiles();


        return Task.CompletedTask;
    }

    public void Dispose()
    {
    }

    public ValueTask Deactivate()
    {
        return ValueTask.CompletedTask;
    }
}
