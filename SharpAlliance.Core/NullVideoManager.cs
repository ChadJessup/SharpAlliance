using System.Threading.Tasks;
using SharpAlliance.Core.Interfaces;
using SharpAlliance.Core.Managers;
using SharpAlliance.Core.Managers.Image;
using SharpAlliance.Core.Managers.VideoSurfaces;
using SharpAlliance.Core.SubSystems;
using SharpAlliance.Platform.Interfaces;

namespace SharpAlliance.Core.NullManagers
{
    public class NullVideoManager : IVideoManager
    {
        public bool IsInitialized { get; } = true;
        public uint guiBOTTOMPANEL { get; set; }
        public uint guiRIGHTPANEL { get; set; }
        public uint guiRENDERBUFFER { get; set; }
        public uint guiSAVEBUFFER { get; set; }
        public uint guiEXTRABUFFER { get; set; }
        public bool gfExtraBuffer { get; set; }

        public bool AddVideoObject(ref VOBJECT_DESC vObjectDesc, out int uiLogoID)
        {
            throw new System.NotImplementedException();
        }

        public void AddVideoSurface(out VSURFACE_DESC vs_desc, out uint uiTempMap)
        {
            throw new System.NotImplementedException();
        }

        public void BltVideoObject(uint fRAME_BUFFER, HVOBJECT hPixHandle, int v1, int v2, int v3, int vO_BLT_SRCTRANSPARENCY, object p)
        {
            throw new System.NotImplementedException();
        }

        public ushort Create16BPPPaletteShaded(ref SGPPaletteEntry[] pPalette, int redScale, int greenScale, int blueScale, bool mono)
        {
            throw new System.NotImplementedException();
        }

        public void DeleteVideoSurfaceFromIndex(uint uiTempMap)
        {
            throw new System.NotImplementedException();
        }

        public void Dispose()
        {
        }

        public void DrawFrame()
        {
        }

        public void DrawTextToScreen(string v1, int v2, int v3, int v4, FontStyle fONT10ARIAL, FontColor fONT_MCOLOR_WHITE, FontColor fONT_MCOLOR_BLACK, bool v5, TextJustifies cENTER_JUSTIFIED)
        {
            throw new System.NotImplementedException();
        }

        public void EndFrameBufferRender()
        {
            throw new System.NotImplementedException();
        }

        public void GetVideoObject(out HVOBJECT hPixHandle, int guiMainMenuBackGroundImage)
        {
            throw new System.NotImplementedException();
        }

        public void GetVideoSurface(out HVSURFACE hSrcVSurface, uint uiTempMap)
        {
            throw new System.NotImplementedException();
        }

        public void GetVSurfacePaletteEntries(HVSURFACE hSrcVSurface, SGPPaletteEntry[] pPalette)
        {
            throw new System.NotImplementedException();
        }

        public ValueTask<bool> Initialize()
        {
            return ValueTask.FromResult(true);
        }

        public void InvalidateRegion(int v1, int v2, int v3, int v4)
        {
            throw new System.NotImplementedException();
        }

        public void InvalidateScreen()
        {
        }

        public void RefreshScreen(object? dummy)
        {
        }

        public void RefreshScreen()
        {
        }
    }
}
