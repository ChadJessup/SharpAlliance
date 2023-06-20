using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SharpAlliance.Core.Managers.VideoSurfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core.Managers;

public class SurfaceManager
{
    private readonly ILogger<SurfaceManager> logger;
    private readonly Dictionary<Surfaces, Image<Rgba32>> surfaces = new();
    private readonly Dictionary<Image<Rgba32>, bool> surfaceLocks = new();

    public SurfaceManager(
        ILogger<SurfaceManager> logger)
    {
        this.logger = logger;
    }

    private int width;
    private int height;

    public Image<Rgba32> this[Surfaces surface]
    {
        get => this.surfaces[surface];
        set => this.surfaces[surface] = value;
    }

    public void InitializeSurfaces(int width, int height)
    {
        this.width = width;
        this.height = height;

        this[Surfaces.PRIMARY_SURFACE] = new(this.width, this.height);
        this[Surfaces.FRAME_BUFFER] = new(this.width, this.height);
        this[Surfaces.RENDER_BUFFER] = new(this.width, this.height);
        this[Surfaces.SAVE_BUFFER] = new(this.width, this.height);
        this[Surfaces.EXTRA_BUFFER] = new(this.width, this.height);
    }

    public Image<Rgba32> LockSurface(Surfaces buffer)
    {
        if (this.surfaces.TryGetValue(buffer, out var image))
        {
            this.surfaceLocks[image] = true;
            return this.surfaces[buffer];
        }
        else
        {
            throw new KeyNotFoundException();
        }
    }

    public void UnlockSurface(Image<Rgba32> buffer)
    {
        this.surfaceLocks[buffer] = false;
    }

    public void UnlockSurface(Surfaces surface)
    {
        if (this.surfaces.TryGetValue(surface, out var image))
        {
            this.UnlockSurface(image);
        }
    }

    public Surfaces CreateSurface(int width, int height)
    {
        throw new NotImplementedException();
    }
}
