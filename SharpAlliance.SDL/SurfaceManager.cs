using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SDL2;
using SharpAlliance.Core.Managers.VideoSurfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core.Managers;

public class SurfaceManager
{
    private readonly ILogger<SurfaceManager> logger;
    private readonly Dictionary<Surface, Image<Rgba32>> surfaces = new();
    private readonly Dictionary<Image<Rgba32>, bool> surfaceLocks = new();

    public SurfaceManager(
        ILogger<SurfaceManager> logger)
    {
        this.logger = logger;
    }

    private int width;
    private int height;

    public Dictionary<SurfaceType, Surface> SurfaceByTypes { get; } = new();

    public Image<Rgba32> this[SurfaceType surface]
    {
        get => this.surfaces.FirstOrDefault(s => s.Key.SurfaceType == surface).Value;
    }

    public void InitializeSurfaces(int width, int height)
    {
        this.width = width;
        this.height = height;

        var primarySurface = this.CreateSurface(new(this.width, this.height), SurfaceType.PRIMARY_SURFACE);
        var frameSurface = this.CreateSurface(new(this.width, this.height), SurfaceType.FRAME_BUFFER);
        var renderSurface = this.CreateSurface(new(this.width, this.height), SurfaceType.RENDER_BUFFER);
        var saveSurface = this.CreateSurface(new(this.width, this.height), SurfaceType.SAVE_BUFFER);
        var extraSurface = this.CreateSurface(new(this.width, this.height), SurfaceType.EXTRA_BUFFER);

//        this.surfaces.Add(primarySurface, primarySurface.Image);
//        this.surfaces.Add(frameSurface, frameSurface.Image);
//        this.surfaces.Add(renderSurface, renderSurface.Image);
//        this.surfaces.Add(saveSurface, saveSurface.Image);
//        this.surfaces.Add(extraSurface, extraSurface.Image);
    }

    public Image<Rgba32> LockSurface(SurfaceType buffer)
    {
        return new Image<Rgba32>(100, 100);
    }

    public unsafe Surface CreateSurface(Image<Rgba32> image, SurfaceType? surfaceType = null)
    {
        if (this.surfaces.ContainsValue(image))
        {
            foreach (var surface in this.surfaces)
            {
                if (surface.Value == image)
                {
                    return surface.Key;
                }
            }
        }

        SurfaceType idx = (SurfaceType)this.surfaces.Count;

        if (surfaceType is not null)
        {
            idx = surfaceType.Value;
        }

        if (!image.DangerousTryGetSinglePixelMemory(out Memory<Rgba32> memory))
        {
            throw new Exception(
                "This can only happen with multi-GB images or when PreferContiguousImageBuffers is not set to true.");
        }

        var pinHandle = memory.Pin();
        var surfacePtr = SDL.SDL_CreateRGBSurfaceFrom(
            (nint)pinHandle.Pointer,
            image.Width,
            image.Height,
            depth: 32,
            pitch: 4 * image.Width, // unsure of this, I'd expect at least gibberish if wrong.
            Rmask: 0x000000FF,
            Gmask: 0x0000FF00,
            Bmask: 0x00FF0000,
            Amask: 0xFF000000);

        if (surfacePtr == IntPtr.Zero)
        {
            string error = SDL.SDL_GetError();
            Console.WriteLine(error);
        }

        Surface surf = new()
        {
            Image = image,
            SurfaceType = idx,
            Handle = pinHandle,
            Pointer = surfacePtr,
        };

        this.SurfaceByTypes[idx] = surf;
        this.surfaces[surf] = image;

        return surf;
    }

    public Texture CreateTextureFromSurface(nint renderer, Surface surface)
    {
        var texturePtr = SDL.SDL_CreateTextureFromSurface(renderer, surface.Pointer);

        if (texturePtr == IntPtr.Zero)
        {
            string error = SDL.SDL_GetError();
            Console.WriteLine(error);
        }

        return new()
        {
            Pointer = texturePtr,
        };
    }
}
