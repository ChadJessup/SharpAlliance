using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpAlliance.Platform.Interfaces
{
    public interface IVideoManager : ISharpAllianceManager
    {
        public const int MAX_CURSOR_WIDTH = 64;
        public const int MAX_CURSOR_HEIGHT = 64;
        public const int VIDEO_NO_CURSOR = 0xFFFF;

        void DrawFrame();
        void RefreshScreen();
        void InvalidateScreen();
    }
}
