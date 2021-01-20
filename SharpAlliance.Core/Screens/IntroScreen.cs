using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpAlliance.Core.Interfaces;
using SharpAlliance.Core.Managers;
using SharpAlliance.Core.Managers.Library;
using SharpAlliance.Core.SubSystems;
using SharpAlliance.Platform;
using SharpAlliance.Platform.Interfaces;

namespace SharpAlliance.Core.Screens
{
    // intro.c
    public class IntroScreen : IScreen
    {
        private IntroScreenType gbIntroScreenMode = IntroScreenType.Unknown;
        private readonly GameContext context;
        private readonly IVideoManager video;
        private readonly MouseSubSystem mouse;
        private readonly CursorSubSystem cursor;
        private readonly RenderDirtySubSystem renderDirty;
        private readonly IMusicManager music;
        private readonly ILibraryManager library;
        private readonly CinematicsSubSystem cinematics;
        private bool gfIntroScreenEntry;
        private bool gfIntroScreenExit;
        private long guiSplashStartTime = 0;
        private int guiSplashFrameFade = 10;

        public IntroScreen(GameContext context,
            MouseSubSystem mouseSubSystem,
            CursorSubSystem cursorSubSystem,
            RenderDirtySubSystem renderDirtySubSystem,
            IMusicManager musicManager,
            ILibraryManager libraryManager,
            CinematicsSubSystem cinematics)
        {
            this.context = context;
            this.video = this.context.VideoManager;
            this.mouse = mouseSubSystem;
            this.cursor = cursorSubSystem;
            this.renderDirty = renderDirtySubSystem;
            this.music = musicManager;
            this.library = libraryManager;
            this.cinematics = cinematics;
        }

        public bool IsInitialized { get; set; }
        public ScreenState State { get; set; }
        public ScreenName guiIntroExitScreen { get; private set; } = ScreenName.INTRO_SCREEN;

        public ValueTask Activate()
        {
            return ValueTask.CompletedTask;
        }

        public ValueTask<ScreenName> Handle()
        {
            if (gfIntroScreenEntry)
            {
                EnterIntroScreen();
                gfIntroScreenEntry = false;
                gfIntroScreenExit = false;

                this.video.InvalidateRegion(0, 0, 640, 480);
            }

            this.renderDirty.RestoreBackgroundRects();


            GetIntroScreenUserInput();

            HandleIntroScreen();

            this.renderDirty.ExecuteBaseDirtyRectQueue();
            this.video.EndFrameBufferRender();

            if (gfIntroScreenExit)
            {
                this.ExitIntroScreen();
                gfIntroScreenExit = false;
                gfIntroScreenEntry = true;
            }

            return ValueTask.FromResult(guiIntroExitScreen);
        }

        private void ExitIntroScreen()
        {
        }

        private void HandleIntroScreen()
        {
        }

        private void GetIntroScreenUserInput()
        {
        }

        public const int SMKINTRO_FIRST_VIDEO = 255;
        public const int SMKINTRO_NO_VIDEO = -1;

        private bool EnterIntroScreen()
        {
            int iFirstVideoID = -1;

            var mm = new MainMenuScreen();
            mm.ClearMainMenu();


            this.cursor.SetCurrentCursorFromDatabase(0);// VIDEO_NO_CURSOR);

            // Don't play music....
            this.music.SetMusicMode(MusicMode.NONE);

            //if the library doesnt exist, exit
            if (!this.library.IsLibraryOpened(LibraryNames.INTRO))
            {
                PrepareToExitIntroScreen();
                return true;
            }

            //initialize smacker
            this.cinematics.SmkInitialize(this.video, 640, 480);


            //get the index opf the first video to watch
            iFirstVideoID = this.GetNextIntroVideo(SMKINTRO_FIRST_VIDEO);

            if (iFirstVideoID != -1)
            {
                this.StartPlayingIntroFlic(iFirstVideoID);

                guiIntroExitScreen = ScreenName.INTRO_SCREEN;
            }
            else
            {
                //Got no intro video, exit
                PrepareToExitIntroScreen();
            }

            return true;
        }

        private void StartPlayingIntroFlic(int iFirstVideoID)
        {
        }

        private int GetNextIntroVideo(int sMKINTRO_FIRST_VIDEO)
        {
            return 0;
        }

        private void PrepareToExitIntroScreen()
        {
        }

        public ValueTask<bool> Initialize()
        {
            //Set so next time we come in, we can set up
            this.gfIntroScreenEntry = true;

            return ValueTask.FromResult(true);
        }

        public void SetIntroType(IntroScreenType introType)
        {
            if (introType == IntroScreenType.BEGINING)
            {
                this.gbIntroScreenMode = IntroScreenType.BEGINING;
            }
            else if (introType == IntroScreenType.ENDING)
            {
                this.gbIntroScreenMode = IntroScreenType.ENDING;
            }
            else if (introType == IntroScreenType.SPLASH)
            {
                this.gbIntroScreenMode = IntroScreenType.SPLASH;
            }
        }

        public void Dispose()
        {
        }

        public enum IntroScreenType
        {
            Unknown = 0,
            BEGINING,         //set when viewing the intro at the begining of the game
            ENDING,               //set when viewing the end game video.

            SPLASH,
        };
    }
}
