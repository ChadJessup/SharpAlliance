using System;
using System.Buffers;
using SharpAlliance.Core.Managers.VideoSurfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace SharpAlliance.Core;

/// <summary>
/// We're not really using surfaces like the original code did, as we have moved to
/// the more hardware accelerated "Texture". But, to keep a lot of the method names similar
/// and to avoid confusion during porting, I'm keeping some of the nomenclature.
/// </summary>
public class Surface : IDisposable
{
    public IntPtr Pointer { get; set; }
    public Image<Rgba32> Image { get; set; }
    public SurfaceType SurfaceType { get; set; }
    public MemoryHandle Handle { get; set; }
    public Texture Texture { get; set; }

    public void Dispose()
    {
        // TODO: make it dispose of everything
    }
}
