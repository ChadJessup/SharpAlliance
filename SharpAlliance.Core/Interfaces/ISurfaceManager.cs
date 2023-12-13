using System.Collections.Generic;
using SharpAlliance.Core.Managers.VideoSurfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace SharpAlliance.Core.Interfaces
{
    public interface ISurfaceManager
    {
        Image<Rgba32> this[SurfaceType surface] { get; }

        Dictionary<SurfaceType, Surface> SurfaceByTypes { get; }

        Surface CreateSurface(Image<Rgba32> image, SurfaceType? surfaceType = null);
        Texture CreateTextureFromSurface(nint renderer, Surface surface);
        void InitializeSurfaces(int width, int height);
        Image<Rgba32> LockSurface(SurfaceType buffer);
    }
}
