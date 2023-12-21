using System;
using System.Buffers;
using SharpAlliance.Core.Managers.VideoSurfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
namespace SharpAlliance.Core;

public class Texture
{
    public IntPtr Pointer { get; set; }
    public Image<Rgba32> Image { get; set; }
    public SurfaceType SurfaceType { get; set; }
    public MemoryHandle Handle { get; set; }
}
