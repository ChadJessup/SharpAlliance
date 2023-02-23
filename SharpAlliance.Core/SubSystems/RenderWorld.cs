using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Veldrid;
using SixLabors.ImageSharp;
using Rectangle = SixLabors.ImageSharp.Rectangle;
using Point = SixLabors.ImageSharp.Point;

namespace SharpAlliance.Core.SubSystems
{
    public class RenderWorld : IDisposable
    {
        public int fSelectMode;
        
        public static TILES_DYNAMIC uiLayerUsedFlags = (TILES_DYNAMIC)0xffffffff;

        public void Dispose()
        {
        }

        public static void RenderStaticWorldRect(Rectangle rect, bool fDynamicsToo)
        {
        }

        public static void ResetSpecificLayerOptimizing(TILES_DYNAMIC uiRowFlag)
        {
            uiLayerUsedFlags |= uiRowFlag;
        }


        public static void SetRenderFlags(RenderingFlags full)
        {
            throw new NotImplementedException();
        }
    }

    [Flags]
    public enum RenderingFlags
    {
        FULL = 0x00000001,
        SHADOWS = 0x00000002,
        MARKED = 0x00000004,
        SAVEOFF = 0x00000008,
        NOZ = 0x00000010,
        ROOMIDS = 0x00000020,
        CHECKZ = 0x00000040,
        ONLYLAND = 0x00000080,
        ONLYSTRUCT = 0x00000100,
        FOVDEBUG = 0x00000200,
    }

    [Flags]
    public enum ScrollDirection
    {
        SCROLL_UP = 0x00000001,
        SCROLL_DOWN = 0x00000002,
        SCROLL_RIGHT = 0x00000004,
        SCROLL_LEFT = 0x00000008,
        SCROLL_UPLEFT = 0x00000020,
        SCROLL_UPRIGHT = 0x00000040,
        SCROLL_DOWNLEFT = 0x00000080,
        SCROLL_DOWNRIGHT = 0x00000200,
    }

    public enum TILES_DYNAMIC : uint
    {
        // highest bit value is rendered first!
        TILES_ALL_DYNAMICS = 0x00000fff,
        CHECKFOR_INT_TILE = 0x00000400,
        LAND = 0x00000200,
        OBJECTS = 0x00000100,
        SHADOWS = 0x00000080,
        STRUCT_MERCS = 0x00000040,
        MERCS = 0x00000020,
        STRUCTURES = 0x00000010,
        ROOF = 0x00000008,
        HIGHMERCS = 0x00000004,
        ONROOF = 0x00000002,
        TOPMOST = 0x00000001,
    }
}
