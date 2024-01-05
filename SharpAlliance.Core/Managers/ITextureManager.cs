namespace SharpAlliance.Core.Managers;

public interface ITextureManager : ISharpAllianceManager
{
    HVOBJECT LoadImage(string assetPath, bool debug = false);
}
