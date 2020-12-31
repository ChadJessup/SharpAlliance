using SharpAlliance.Platform.Interfaces;

namespace SharpAlliance.Platform.NullManagers
{
    public class NullVideoManager : IVideoManager
    {
        public void Dispose()
        {
        }

        public bool Initialize()
        {
            return true;
        }
    }
}
