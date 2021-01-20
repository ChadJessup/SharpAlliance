using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpAlliance.Core.Managers;
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

        private bool gfIntroScreenEntry;
        private long guiSplashStartTime = 0;
        private int guiSplashFrameFade = 10;
        
        public IntroScreen(GameContext context)
        {
            this.context = context;
            this.video = this.context.VideoManager;
        }

        public bool IsInitialized { get; set; }
        public ScreenState State { get; set; }

        public ValueTask Activate()
        {
            return ValueTask.CompletedTask;
        }

        public ValueTask<ScreenName> Handle()
        {
            this.video.InvalidateScreen();
            this.video.RefreshScreen();

            this.guiSplashStartTime = this.context.ClockManager.GetClock();

            return ValueTask.FromResult(ScreenName.INTRO_SCREEN);
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
