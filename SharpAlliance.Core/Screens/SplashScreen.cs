using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpAlliance.Platform;

namespace SharpAlliance.Core.Screens
{
    public class SplashScreen : IScreen
    {
        private readonly GameContext context;
        private readonly int guiSplashFrameFade = 10;
        private readonly int guiSplashStartTime = 0;
        private IntroScreen gbIntroScreenMode = IntroScreen.Unknown;

        public SplashScreen(GameContext context)
        {
            this.context = context;
        }

        public bool IsInitialized { get; set; }
        public ScreenState State { get; set; } = ScreenState.Unknown;

        public void SetIntroType(IntroScreen introType)
        {
            if (introType == IntroScreen.BEGINING)
            {
                gbIntroScreenMode = IntroScreen.BEGINING;
            }
            else if (introType == IntroScreen.ENDING)
            {
                gbIntroScreenMode = IntroScreen.ENDING;
            }
            else if (introType == IntroScreen.SPLASH)
            {
                gbIntroScreenMode = IntroScreen.SPLASH;
            }
        }

        public ValueTask Activate()
        {
            return ValueTask.CompletedTask;
        }

        public ValueTask<bool> Initialize()
        {
            return ValueTask.FromResult(true);
        }

        public ValueTask<IScreen> Handle()
        {
            return ValueTask.FromResult<IScreen>(this);
        }

        public void Dispose()
        {
        }
    }

    public enum IntroScreen
    {
        Unknown = 0,
        BEGINING,         //set when viewing the intro at the begining of the game
        ENDING,               //set when viewing the end game video.

        SPLASH,
    };
}
