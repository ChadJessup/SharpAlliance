using SharpAlliance.Core.Managers.VideoSurfaces;
using SharpAlliance.Platform.Interfaces;

namespace SharpAlliance.Core.Interfaces
{
    public interface IVideoSurfaceManager : ISharpAllianceManager
    {
        HVSURFACE? CreateVideoSurface(ref VSURFACE_DESC vs_desc);
        bool DeleteVideoSurface(HVSURFACE? hVSurface);
    }
}
