using SharpAlliance.Platform.Interfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace SharpAlliance.Core.Managers;

public interface ITextureManager : ISharpAllianceManager
{
    HVOBJECT LoadImage(string assetPath);
}
