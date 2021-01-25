using System.Threading.Tasks;
using SharpAlliance.Core.Managers.VideoSurfaces;
using SharpAlliance.Platform.Interfaces;

namespace SharpAlliance.Core.Interfaces
{
    public interface IVideoSurfaceManager : ISharpAllianceManager
    {
        ValueTask<HVSURFACE?> CreateVideoSurface(VSURFACE_DESC vs_desc, IFileManager fileManager);
        bool DeleteVideoSurface(HVSURFACE? hVSurface);
        byte[] LockVideoSurface(uint fRAME_BUFFER, out int uiDestPitchBYTES);
        void UnLockVideoSurface(uint fRAME_BUFFER);
    }
}
