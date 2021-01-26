using SharpAlliance.Core.Managers;
using SharpAlliance.Platform.Interfaces;

namespace SharpAlliance.Core.Interfaces
{
    public interface IVideoManager : ISharpAllianceManager
    {
        public const int MAX_CURSOR_WIDTH = 64;
        public const int MAX_CURSOR_HEIGHT = 64;
        public const int VIDEO_NO_CURSOR = 0xFFFF;

        void DrawFrame();
        void RefreshScreen();
        void InvalidateScreen();
        void InvalidateRegion(int v1, int v2, int v3, int v4);
        void EndFrameBufferRender();
        bool AddVideoObject(ref VOBJECT_DESC vObjectDesc, out int uiLogoID);
    }
}
