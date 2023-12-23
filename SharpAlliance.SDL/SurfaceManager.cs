using Microsoft.Extensions.Logging;
using SDL2;
using SharpAlliance.Core.Interfaces;
using SharpAlliance.Core.Managers.VideoSurfaces;

namespace SharpAlliance.Core.Managers;

public class SurfaceManager : ISurfaceManager
{
    private readonly ILogger<SurfaceManager> logger;
    private readonly Dictionary<Texture, Image<Rgba32>> surfaces = new();
    private readonly Dictionary<Image<Rgba32>, bool> surfaceLocks = new();

    public SurfaceManager(
        ILogger<SurfaceManager> logger)
    {
        this.logger = logger;
    }

    private int width;
    private int height;

    public Dictionary<SurfaceType, Texture> SurfaceByTypes { get; } = new();

    public Image<Rgba32> this[SurfaceType surface]
    {
        get => this.surfaces.FirstOrDefault(s => s.Key.SurfaceType == surface).Value;
    }

    public void InitializeSurfaces(nint renderer, int width, int height)
    {
        this.width = width;
        this.height = height;

        Image<Rgba32> blankImage = new(width, height, Rgba32.ParseHex("FF000000")); 

        var primarySurface = this.CreateSurface(renderer, blankImage.Clone(), SurfaceType.PRIMARY_SURFACE);
        var frameSurface = this.CreateSurface(renderer, blankImage.Clone(), SurfaceType.FRAME_BUFFER);
        var renderSurface = this.CreateSurface(renderer, blankImage.Clone(), SurfaceType.RENDER_BUFFER);
        var saveSurface = this.CreateSurface(renderer, blankImage.Clone(), SurfaceType.SAVE_BUFFER);
        var extraSurface = this.CreateSurface(renderer, blankImage.Clone(), SurfaceType.EXTRA_BUFFER);
        var backbufferSurface = this.CreateSurface(renderer, blankImage.Clone(), SurfaceType.BACKBUFFER);
        var zBuffer = this.CreateSurface(renderer, blankImage.Clone(), SurfaceType.Z_BUFFER);
//        frameSurface.Image.SaveAsPng($@"c:\temp\{nameof(InitializeSurfaces)}-frameSurface.png");

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

    public unsafe Texture CreateSurface(nint renderer, Image<Rgba32> image, SurfaceType? surfaceType = null)
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
      //  var surfacePtr = SDL.SDL_CreateRGBSurfaceFrom(
      //      (nint)pinHandle.Pointer,
      //      image.Width,
      //      image.Height,
      //      depth: 32,
      //      pitch: 4 * image.Width, // unsure of this, I'd expect at least gibberish if wrong.
      //      Rmask: 0x000000FF,
      //      Gmask: 0x0000FF00,
      //      Bmask: 0x00FF0000,
      //      Amask: 0xFF000000);

       var texturePtr = SDL.SDL_CreateTexture(
            renderer,
            SDL.SDL_PIXELFORMAT_ABGR8888,
            (int)SDL.SDL_TextureAccess.SDL_TEXTUREACCESS_TARGET,
            image.Width,
            image.Height);

        if (texturePtr == IntPtr.Zero)
        {
            string error = SDL.SDL_GetError();
            Console.WriteLine(error);
        }

        Texture texture = new()
        {
            Image = image,
            SurfaceType = idx,
            Handle = pinHandle,
            Pointer = texturePtr,
        };

        this.SurfaceByTypes[idx] = texture;
        this.surfaces[texture] = image;
        
        return texture;
    }

    public Texture CreateTextureFromSurface(nint renderer, Surface surface)
    {
        var texturePtr = SDL.SDL_CreateTextureFromSurface(renderer, surface.Pointer);

        if (texturePtr == IntPtr.Zero)
        {
            string error = SDL.SDL_GetError();
            Console.WriteLine(error);
        }

        Texture tex = new()
        {
            Pointer = texturePtr,
            Image = surface.Image,
        };

        surface.Texture = tex;

        return tex;
    }
}
