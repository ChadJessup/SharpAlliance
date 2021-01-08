using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vortice.Mathematics;

namespace SharpAlliance.Core.SubSystems
{
    public class RenderWorld : IDisposable
    {
        public bool gfRenderScroll { get; set; }
        public bool gfScrollStart { get; set; }
        public int gsScrollXIncrement { get; set; }
        public int gsScrollYIncrement { get; set; }
        public int guiScrollDirection { get; set; }
        public int gsRenderHeight { get; set; }
        // GLOBAL VARIABLES
        public int SCROLL_X_STEP;
        public int SCROLL_Y_STEP;

        public int gsVIEWPORT_START_X;
        public int gsVIEWPORT_START_Y;
        public int gsVIEWPORT_WINDOW_START_Y;
        public int gsVIEWPORT_END_Y;
        public int gsVIEWPORT_WINDOW_END_Y;
        public int gsVIEWPORT_END_X;

        public int gsRenderCenterX;
        public int gsRenderCenterY;
        public int gsRenderWorldOffsetX;
        public int gsRenderWorldOffsetY;

        // CURRENT VIEWPORT IN WORLD COORDS
        public int gsTopLeftWorldX, gsTopLeftWorldY;
        public int gsTopRightWorldX, gsTopRightWorldY;
        public int gsBottomLeftWorldX, gsBottomLeftWorldY;
        public int gsBottomRightWorldX, gsBottomRightWorldY;

        public Rectangle gSelectRegion;
        public Point gSelectAnchor;
        public int fSelectMode;

        // GLOBAL COORDINATES
        public int gTopLeftWorldLimitX, gTopLeftWorldLimitY;
        public int gTopRightWorldLimitX, gTopRightWorldLimitY;
        public int gBottomLeftWorldLimitX, gBottomLeftWorldLimitY;
        public int gBottomRightWorldLimitX, gBottomRightWorldLimitY;
        public int gCenterWorldX, gCenterWorldY;
        public int gsTLX, gsTLY, gsTRX, gsTRY;
        public int gsBLX, gsBLY, gsBRX, gsBRY;
        public int gsCX, gsCY;
        public float gdScaleX, gdScaleY;
        public bool fLandLayerDirty;
        public bool gfIgnoreScrollDueToCenterAdjust;

        public void Dispose()
        {
        }
    }
}
