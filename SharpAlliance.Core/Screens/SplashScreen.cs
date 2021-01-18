using System.Threading.Tasks;
using SharpAlliance.Platform;
using SharpAlliance.Platform.Interfaces;

namespace SharpAlliance.Core.Screens
{
    public class SplashScreen : IScreen
    {
        private readonly GameContext context;
        private readonly int guiSplashFrameFade = 10;
        private readonly int guiSplashStartTime = 0;
        private IntroScreenType gbIntroScreenMode = IntroScreenType.Unknown;
        private bool gfIntroScreenEntry;
        private IVideoManager video;

        public SplashScreen(GameContext context)
        {
            this.context = context;
            this.video = this.context.VideoManager;
        }

        public bool IsInitialized { get; set; }
        public ScreenState State { get; set; } = ScreenState.Unknown;

        public void SetIntroType(IntroScreenType introType)
        {
            if (introType == IntroScreenType.BEGINING)
            {
                gbIntroScreenMode = IntroScreenType.BEGINING;
            }
            else if (introType == IntroScreenType.ENDING)
            {
                gbIntroScreenMode = IntroScreenType.ENDING;
            }
            else if (introType == IntroScreenType.SPLASH)
            {
                gbIntroScreenMode = IntroScreenType.SPLASH;
            }
        }

        public ValueTask Activate()
        {
            return ValueTask.CompletedTask;
        }

        public ValueTask<bool> Initialize()
        {
            //Set so next time we come in, we can set up
            gfIntroScreenEntry = true;

            return ValueTask.FromResult(true);
        }

        public ValueTask<IScreen> Handle()
        {
            this.video.RefreshScreen(null);

            return ValueTask.FromResult<IScreen>(this);
        }

        public void Dispose()
        {
        }
    }

    public enum IntroScreenType
    {
        Unknown = 0,
        BEGINING,         //set when viewing the intro at the begining of the game
        ENDING,               //set when viewing the end game video.

        SPLASH,
    };
}
