using SharpAlliance.Platform.Interfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace SharpAlliance.Core.Managers;

public interface ITextureManager : ISharpAllianceManager
{
    nint CreateTexture(Surface surface);
    HVOBJECT LoadImage(string assetPath);
    bool TryGetImage(string key, out Image<Rgba32> hPixHandle);
}
