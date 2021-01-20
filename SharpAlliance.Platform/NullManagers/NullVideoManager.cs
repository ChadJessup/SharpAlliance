using System.Threading.Tasks;
using SharpAlliance.Platform.Interfaces;

namespace SharpAlliance.Platform.NullManagers
{
    public class NullVideoManager : IVideoManager
    {
        public bool IsInitialized { get; } = true;

        public void Dispose()
        {
        }

        public void DrawFrame()
        {
        }

        public void EndFrameBufferRender()
        {
            throw new System.NotImplementedException();
        }

        public ValueTask<bool> Initialize()
        {
            return ValueTask.FromResult(true);
        }

        public void InvalidateRegion(int v1, int v2, int v3, int v4)
        {
            throw new System.NotImplementedException();
        }

        public void InvalidateScreen()
        {
        }

        public void RefreshScreen(object? dummy)
        {
        }

        public void RefreshScreen()
        {
        }
    }
}
