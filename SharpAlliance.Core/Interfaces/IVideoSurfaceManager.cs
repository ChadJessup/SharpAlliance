//using System.Threading.Tasks;
//using SharpAlliance.Core.Managers.VideoSurfaces;
//using SharpAlliance.Platform.Interfaces;

//namespace SharpAlliance.Core.Interfaces
//{
//    public interface IVideoSurfaceManager : ISharpAllianceManager
//    {
//        HVSURFACE CreateVideoSurface(VSURFACE_DESC vs_desc, IFileManager fileManager);
//        bool DeleteVideoSurface(HVSURFACE? hVSurface);
//        byte[] LockVideoSurface(uint fRAME_BUFFER, out int uiDestPitchBYTES);
//        void UnLockVideoSurface(uint fRAME_BUFFER);
//        void ColorFillVideoSurfaceArea(uint fRAME_BUFFER, int v1, int v2, int v3, int v4, int v5);
//        void ShadowVideoSurfaceRectUsingLowPercentTable(uint fRAME_BUFFER, int v1, int v2, int v3, int v4);
//    }
//}
