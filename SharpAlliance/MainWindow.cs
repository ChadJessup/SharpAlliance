using SharpAlliance.Platform;
using unvell.D2DLib.WinForm;

namespace SharpAlliance
{
    public class MainWindow : D2DForm
    {
        private readonly GameContext context;

        public MainWindow(GameContext context)
        {
            this.context = context;
        }
    }
}
