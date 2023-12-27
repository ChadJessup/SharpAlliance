using System.Collections.Generic;
using SharpAlliance.Core.Managers.VideoSurfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace SharpAlliance.Core.Interfaces
{
    public interface ISurfaceManager
    {
        Image<Rgba32> this[SurfaceType surface] { get; }

        Dictionary<SurfaceType, Texture> SurfaceByTypes { get; }

        Texture CreateSurface(nint renderer, Image<Rgba32> image, SurfaceType? surfaceType = null);
        Texture CreateSurface(VSURFACE_DESC vs_desc);
        SurfaceType CreateSurface(HVOBJECT vObjectDesc, int idx = 0);
        Texture CreateTextureFromSurface(nint renderer, Surface surface);
        void InitializeSurfaces(nint renderer, int width, int height);
        Image<Rgba32> LockSurface(SurfaceType buffer);
    }
}
