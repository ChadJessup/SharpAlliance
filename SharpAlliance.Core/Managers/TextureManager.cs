using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using SharpAlliance.Core.Interfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace SharpAlliance.Core.Managers;

public class TextureManager
{
    private readonly Dictionary<string, HVOBJECT> loadedTextures = new();
    private readonly ILogger<TextureManager> logger;
    private readonly ILibraryManager libraries;

    public TextureManager(ILogger<TextureManager> logger, ILibraryManager libraryManager)
    {
        this.logger = logger;
        this.libraries = libraryManager;
    }

    public HVOBJECT LoadTexture(string assetPath, out string assetKey)
    {
        assetKey = assetPath;

        return new HVOBJECT();
    }
}
