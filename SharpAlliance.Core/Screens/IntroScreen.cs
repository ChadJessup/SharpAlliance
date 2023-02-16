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
using Veldrid;

namespace SharpAlliance.Core.Screens
{
    // intro.c
    public class IntroScreen : IScreen
    {
        private IntroScreenType gbIntroScreenMode = IntroScreenType.Unknown;
        private readonly GameInit gameInit;
        private readonly GameContext context;
        private readonly IScreenManager screens;
        private readonly IVideoManager video;
        private readonly MouseSubSystem mouse;
        private readonly CursorSubSystem cursor;
        private readonly RenderDirtySubSystem renderDirty;
        private readonly IMusicManager music;
        private readonly ILibraryManager library;
        private readonly CinematicsSubSystem cinematics;
        private readonly SoldierProfileSubSystem soldiers;
        private readonly IVideoObjectManager videoObject;
        private bool gfIntroScreenEntry;
        private bool gfIntroScreenExit;
        public long guiSplashStartTime { get; set; } = 0;
        public int guiSplashFrameFade { get; set; } = 10;

        private SMKFLIC? gpSmackFlic = null;
        private string[] gpzSmackerFileNames = new string[]
        {
        	//begining of the game
        	"INTRO\\Rebel_cr.smk",
            "INTRO\\Omerta.smk",
            "INTRO\\Prague_cr.smk",
            "INTRO\\Prague.smk",

        	//endgame
        	"INTRO\\Throne_Mig.smk",
            "INTRO\\Throne_NoMig.smk",
            "INTRO\\Heli_FlyBy.smk",
            "INTRO\\Heli_Sky.smk",
            "INTRO\\Heli_NoSky.smk",

            "INTRO\\SplashScreen.smk",
            "INTRO\\TalonSoftid_endhold.smk",
        };

        public IntroScreen(GameContext context,
            MouseSubSystem mouseSubSystem,
            CursorSubSystem cursorSubSystem,
            RenderDirtySubSystem renderDirtySubSystem,
            IMusicManager musicManager,
            ILibraryManager libraryManager,
            CinematicsSubSystem cinematics,
            SoldierProfileSubSystem soldierSubSystem,
            IScreenManager screenManager,
            GameInit gameInit,
            IVideoManager videoManager)
        {
            this.gameInit = gameInit;
            this.context = context;
            this.screens = screenManager;
            this.video = videoManager;
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

                this.video.InvalidateRegion(new(0, 0, 640, 480));
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
            bool fFlicStillPlaying = false;

            //if we are exiting this screen, this frame, dont update the screen
            if (this.gfIntroScreenExit)
            {
                return;
            }

            //handle smaker each frame
            fFlicStillPlaying = this.SmkPollFlics();

            //if the flic is not playing
            if (!fFlicStillPlaying)
            {
                var iNextVideoToPlay = this.GetNextIntroVideo(this.giCurrentIntroBeingPlayed);

                if (iNextVideoToPlay != SmackerFiles.SMKINTRO_NO_VIDEO)
                {
                    this.StartPlayingIntroFlic(iNextVideoToPlay);
                }
                else
                {
                    this.PrepareToExitIntroScreen();
                    this.giCurrentIntroBeingPlayed = SmackerFiles.SMKINTRO_NO_VIDEO;
                }
            }

            this.video.InvalidateScreen();
        }

        private bool SmkPollFlics()
        {
            return false;
        }

        private void GetIntroScreenUserInput()
        {
        }

        private bool EnterIntroScreen()
        {
            SmackerFiles iFirstVideoID = SmackerFiles.SMKINTRO_NO_VIDEO;

            // var mm = new MainMenuScreen();
            // mm.ClearMainMenu();

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

        private void StartPlayingIntroFlic(SmackerFiles iIndexOfFlicToPlay)
        {
            if (iIndexOfFlicToPlay != SmackerFiles.SMKINTRO_NO_VIDEO)
            {
                //start playing a flic
                // TODO: port libsmacker
                //gpSmackFlic = SmkPlayFlic(gpzSmackerFileNames[(int)iIndexOfFlicToPlay], 0, 0, true);

                this.gpSmackFlic = null;

                if (this.gpSmackFlic != null)
                {
                    this.giCurrentIntroBeingPlayed = iIndexOfFlicToPlay;
                }
                else
                {
                    //do a check
                    this.PrepareToExitIntroScreen();
                }
            }
        }

        private SmackerFiles GetNextIntroVideo(SmackerFiles uiCurrentVideo)
        {
            SmackerFiles iStringToUse = SmackerFiles.SMKINTRO_NO_VIDEO;

            //switch on whether it is the beginging or the end game video
            switch (this.gbIntroScreenMode)
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
                                if (Globals.gMercProfiles[NPCID.MIGUEL].bMercStatus == MercStatus.MERC_IS_DEAD)
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
                                if (Globals.gMercProfiles[NPCID.SKYRIDER].bMercStatus == MercStatus.MERC_IS_DEAD)
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
            //if its the intro at the begining of the game
            if (this.gbIntroScreenMode == IntroScreenType.INTRO_BEGINING)
            {
                //go to the init screen
                this.guiIntroExitScreen = ScreenName.InitScreen;
            }
            else if (this.gbIntroScreenMode == IntroScreenType.INTRO_SPLASH)
            {
                //display a logo when exiting
                this.DisplaySirtechSplashScreen();

                ScreenManager.gfDoneWithSplashScreen = true;
                this.guiIntroExitScreen = ScreenName.InitScreen;
            }
            else
            {
                //We want to reinitialize the game
                this.gameInit.ReStartingGame();

                //		guiIntroExitScreen = MAINMENU_SCREEN;
                this.guiIntroExitScreen = ScreenName.CREDIT_SCREEN;
            }

            this.gfIntroScreenExit = true;
        }

        private void DisplaySirtechSplashScreen()
        {
            HVOBJECT hPixHandle;
            string logoKey;

            int uiDestPitchBYTES;
            byte[] pDestBuf;

            // JA3Gold: do nothing until we have a graphic to replace Talonsoft's
            //return;

            //memset(&VObjectDesc, 0, sizeof(VOBJECT_DESC));
            
            //	FilenameForBPP("INTERFACE\\TShold.sti", VObjectDesc.ImageFile);
            var videoObject = this.video.AddVideoObject("INTERFACE\\SirtechSplash.sti", out logoKey);

            //this.video.BltVideoObject(
            //    0,
            //    videoObject,
            //    0,
            //    0,
            //    0,
            //    VideoObjectManager.VO_BLT_SRCTRANSPARENCY,
            //    null);

            this.video.DeleteVideoObjectFromIndex(logoKey);

            this.video.InvalidateScreen();
            this.video.RefreshScreen();
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

        public void Draw(SpriteRenderer sr, GraphicsDevice gd, CommandList cl)
        {
        }

        public ValueTask Deactivate()
        {
            return ValueTask.CompletedTask;
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
