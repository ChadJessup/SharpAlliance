using SharpAlliance.Platform.Interfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace SharpAlliance.Core.Managers;

public interface ITextureManager : ISharpAllianceManager
{
    Image<Rgba32> LoadTexture(string assetPath);
    bool TryGetTexture(string key, out Image<Rgba32> hPixHandle);
}
