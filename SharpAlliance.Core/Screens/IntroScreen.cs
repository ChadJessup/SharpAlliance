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
        private readonly SoldierProfileSubSystem soldiers;
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
            CinematicsSubSystem cinematics,
            SoldierProfileSubSystem soldierSubSystem)
        {
            this.context = context;
            this.video = this.context.VideoManager;
            this.mouse = mouseSubSystem;
            this.cursor = cursorSubSystem;
            this.renderDirty = renderDirtySubSystem;
            this.music = musicManager;
            this.library = libraryManager;
            this.cinematics = cinematics;
            this.soldiers = soldierSubSystem;
        }

        public bool IsInitialized { get; set; }
        public ScreenState State { get; set; }
        public ScreenName guiIntroExitScreen { get; private set; } = ScreenName.INTRO_SCREEN;
        public SmackerFiles giCurrentIntroBeingPlayed = SmackerFiles.SMKINTRO_NO_VIDEO;

        public ValueTask Activate()
        {
            return ValueTask.CompletedTask;
        }

        public ValueTask<ScreenName> Handle()
        {
            if (this.gfIntroScreenEntry)
            {
                this.EnterIntroScreen();
                this.gfIntroScreenEntry = false;
                this.gfIntroScreenExit = false;

                this.video.InvalidateRegion(0, 0, 640, 480);
            }

            this.renderDirty.RestoreBackgroundRects();


            this.GetIntroScreenUserInput();

            this.HandleIntroScreen();

            this.renderDirty.ExecuteBaseDirtyRectQueue();
            this.video.EndFrameBufferRender();

            if (this.gfIntroScreenExit)
            {
                this.ExitIntroScreen();
                this.gfIntroScreenExit = false;
                this.gfIntroScreenEntry = true;
            }

            return ValueTask.FromResult(this.guiIntroExitScreen);
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

        private bool EnterIntroScreen()
        {
            SmackerFiles iFirstVideoID = SmackerFiles.SMKINTRO_NO_VIDEO;

            var mm = new MainMenuScreen();
            mm.ClearMainMenu();

            this.cursor.SetCurrentCursorFromDatabase(0);// VIDEO_NO_CURSOR);

            // Don't play music....
            this.music.SetMusicMode(MusicMode.NONE);

            //if the library doesnt exist, exit
            if (!this.library.IsLibraryOpened(LibraryNames.INTRO))
            {
                this.PrepareToExitIntroScreen();
                return true;
            }

            //initialize smacker
            this.cinematics.SmkInitialize(this.video, 640, 480);

            //get the index opf the first video to watch
            iFirstVideoID = this.GetNextIntroVideo(SmackerFiles.SMKINTRO_FIRST_VIDEO);

            if (iFirstVideoID != SmackerFiles.SMKINTRO_NO_VIDEO)
            {
                this.StartPlayingIntroFlic(iFirstVideoID);

                this.guiIntroExitScreen = ScreenName.INTRO_SCREEN;
            }
            else
            {
                //Got no intro video, exit
                this.PrepareToExitIntroScreen();
            }

            return true;
        }

        private void StartPlayingIntroFlic(SmackerFiles iFirstVideoID)
        {
        }

        private SmackerFiles GetNextIntroVideo(SmackerFiles uiCurrentVideo)
        {
            SmackerFiles iStringToUse = SmackerFiles.SMKINTRO_NO_VIDEO;

            //switch on whether it is the beginging or the end game video
            switch (gbIntroScreenMode)
            {
                //the video at the begining of the game
                case IntroScreenType.INTRO_BEGINING:
                    {
                        switch (uiCurrentVideo)
                        {
                            case SmackerFiles.SMKINTRO_FIRST_VIDEO:
                                iStringToUse = SmackerFiles.SMKINTRO_REBEL_CRDT;
                                break;
                            case SmackerFiles.SMKINTRO_REBEL_CRDT:
                                iStringToUse = SmackerFiles.SMKINTRO_OMERTA;
                                break;
                            case SmackerFiles.SMKINTRO_OMERTA:
                                iStringToUse = SmackerFiles.SMKINTRO_PRAGUE_CRDT;
                                break;
                            case SmackerFiles.SMKINTRO_PRAGUE_CRDT:
                                iStringToUse = SmackerFiles.SMKINTRO_PRAGUE;
                                break;
                            case SmackerFiles.SMKINTRO_PRAGUE:
                                iStringToUse = SmackerFiles.SMKINTRO_NO_VIDEO;
                                break;
                                //				case SMKINTRO_LAST_INTRO:
                                //					iStringToUse = -1;
                                //					break;
                        }
                    }
                    break;

                //end game
                case IntroScreenType.INTRO_ENDING:
                    {
                        switch (uiCurrentVideo)
                        {
                            case SmackerFiles.SMKINTRO_FIRST_VIDEO:
                                //if Miguel is dead, play the flic with out him in it
                                if (this.soldiers.gMercProfiles[NPCIDs.MIGUEL].bMercStatus == MercStatus.MERC_IS_DEAD)
                                {
                                    iStringToUse = SmackerFiles.SMKINTRO_END_END_SPEECH_NO_MIGUEL;
                                }
                                else
                                {
                                    iStringToUse = SmackerFiles.SMKINTRO_END_END_SPEECH_MIGUEL;
                                }

                                break;

                            case SmackerFiles.SMKINTRO_END_END_SPEECH_MIGUEL:
                            case SmackerFiles.SMKINTRO_END_END_SPEECH_NO_MIGUEL:
                                iStringToUse = SmackerFiles.SMKINTRO_END_HELI_FLYBY;
                                break;

                            //if SkyRider is dead, play the flic without him
                            case SmackerFiles.SMKINTRO_END_HELI_FLYBY:
                                if (this.soldiers.gMercProfiles[NPCIDs.SKYRIDER].bMercStatus == MercStatus.MERC_IS_DEAD)
                                {
                                    iStringToUse = SmackerFiles.SMKINTRO_END_NOSKYRIDER_HELICOPTER;
                                }
                                else
                                {
                                    iStringToUse = SmackerFiles.SMKINTRO_END_SKYRIDER_HELICOPTER;
                                }

                                break;
                        }
                    }
                    break;

                case IntroScreenType.INTRO_SPLASH:
                    switch (uiCurrentVideo)
                    {
                        case SmackerFiles.SMKINTRO_FIRST_VIDEO:
                            iStringToUse = SmackerFiles.SMKINTRO_SPLASH_SCREEN;
                            break;
                        case SmackerFiles.SMKINTRO_SPLASH_SCREEN:
                            //iStringToUse = SMKINTRO_SPLASH_TALONSOFT;
                            break;
                    }
                    break;
            }

            return iStringToUse;
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
            if (introType == IntroScreenType.INTRO_BEGINING)
            {
                this.gbIntroScreenMode = IntroScreenType.INTRO_BEGINING;
            }
            else if (introType == IntroScreenType.INTRO_ENDING)
            {
                this.gbIntroScreenMode = IntroScreenType.INTRO_ENDING;
            }
            else if (introType == IntroScreenType.INTRO_SPLASH)
            {
                this.gbIntroScreenMode = IntroScreenType.INTRO_SPLASH;
            }
        }

        public void Dispose()
        {
        }
    }

    public enum IntroScreenType
    {
        Unknown = 0,
        INTRO_BEGINING,         //set when viewing the intro at the begining of the game
        INTRO_ENDING,               //set when viewing the end game video.

        INTRO_SPLASH,
    };

    //enums for the various smacker files
    public enum SmackerFiles
    {
        SMKINTRO_REBEL_CRDT,
        SMKINTRO_OMERTA,
        SMKINTRO_PRAGUE_CRDT,
        SMKINTRO_PRAGUE,

        //there are no more videos shown for the begining

        SMKINTRO_END_END_SPEECH_MIGUEL,
        SMKINTRO_END_END_SPEECH_NO_MIGUEL,
        SMKINTRO_END_HELI_FLYBY,
        SMKINTRO_END_SKYRIDER_HELICOPTER,
        SMKINTRO_END_NOSKYRIDER_HELICOPTER,

        SMKINTRO_SPLASH_SCREEN,
        SMKINTRO_SPLASH_TALONSOFT,

        //there are no more videos shown for the endgame
        SMKINTRO_LAST_END_GAME,

        SMKINTRO_FIRST_VIDEO = 255,
        SMKINTRO_NO_VIDEO = -1,
    };
}
