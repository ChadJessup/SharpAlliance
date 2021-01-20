using System.Threading.Tasks;
using SharpAlliance.Core.Managers;
using SharpAlliance.Core.Screens;

namespace SharpAlliance.Core
{
    internal class NullScreen : IScreen
    {
        public static IScreen Instance { get; } = new NullScreen();

        public bool IsInitialized { get; set; }
        public ScreenState State { get; set; }

        public ValueTask Activate()
        {
            return ValueTask.CompletedTask;
        }

        public void Dispose()
        {
        }

        public ValueTask<ScreenName> Handle()
        {
            return ValueTask.FromResult(ScreenName.NullScreen);
        }

        public ValueTask<bool> Initialize()
        {
            return ValueTask.FromResult(true);
        }
    }
}
