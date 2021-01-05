﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SharpAlliance.Core.Interfaces;
using SharpAlliance.Core.Managers.Image;
using SharpAlliance.Platform;

namespace SharpAlliance.Core.Managers
{
    public class VideoObjectManager : IVideoObjectManager
    {
        private static class Constants
        {
            public const int DEFAULT_VIDEO_OBJECT_LIST_SIZE = 10;

            public const int COMPRESS_TRANSPARENT = 0x80;
            public const int COMPRESS_RUN_MASK = 0x7F;

            public const int HVOBJECT_SHADE_TABLES = 48;

            public const int HVOBJECT_GLOW_GREEN = 0;
            public const int HVOBJECT_GLOW_BLUE = 1;
            public const int HVOBJECT_GLOW_YELLOW = 2;
            public const int HVOBJECT_GLOW_RED = 3;
        }

        private readonly ILogger<VideoObjectManager> logger;
        private List<int> ghVideoObjects;
        private bool gfVideoObjectsInit = false;
        VOBJECT_NODE? gpVObjectHead = null;
        VOBJECT_NODE? gpVObjectTail = null;
        private int guiVObjectIndex = 1;
        private int guiVObjectSize = 0;
        private int guiVObjectTotalAdded = 0;

        public VideoObjectManager(ILogger<VideoObjectManager> logger)
        {
            this.logger = logger;

            this.gpVObjectHead = null;
            this.gpVObjectTail = null;
            this.gfVideoObjectsInit = true;

            this.IsInitialized = true;
        }

        public bool IsInitialized { get; }

        public ValueTask<bool> Initialize()
        {
            this.logger.LogDebug(LoggingEventId.VIDEOOBJECT, "Video Object Manager");
            gpVObjectHead = gpVObjectTail = null;
            gfVideoObjectsInit = true;
            return ValueTask.FromResult(true);
        }

        public void Dispose()
        {
        }
    }

    public class VOBJECT_NODE
    {
        HVOBJECT hVObject;
        public int uiIndex;
        public VOBJECT_NODE next;
        public VOBJECT_NODE prev;

        public int? pName;
        public int? pCode;
    }

    public class HVOBJECT
    {
        public const int HVOBJECT_SHADE_TABLES = 48;

        public int fFlags;                              // Special flags
        public int uiSizePixData;           // ETRLE data size
        public SGPPaletteEntry? pPaletteEntry;             // 8BPP Palette						  
        public int TransparentColor;          // Defaults to 0,0,0
        public int? p16BPPPalette;              // A 16BPP palette used for 8->16 blits

        public List<int> pPixData;                       // ETRLE pixel data
        public ETRLEObject? pETRLEObject;              // Object offset data etc
        public SixteenBPPObjectInfo? p16BPPObject;
        public int[] pShades = new int[HVOBJECT_SHADE_TABLES]; // Shading tables
        public int? pShadeCurrent;
        public int? pGlow;                              // glow highlight table
        public int? pShade8;                         // 8-bit shading index table
        public int? pGlow8;                          // 8-bit glow table
        public ZStripInfo ppZStripInfo;              // Z-value strip info arrays

        public int usNumberOf16BPPObjects;
        public int usNumberOfObjects;   // Total number of objects
        public int ubBitDepth;                       // BPP 
    }

    // Effects structure for specialized blitting
    public struct blt_fx
    {
        public int uiShadowLevel;
        public Rectangle ClipRect;
    }


    // Z-buffer info structure for properly assigning Z values
    public struct ZStripInfo
    {
        public int bInitialZChange;       // difference in Z value between the leftmost and base strips
        public int ubFirstZStripWidth;   // # of pixels in the leftmost strip
        public int ubNumberOfZChanges;   // number of strips (after the first)
        public int pbZChange;            // change to the Z value in each strip (after the first)
    }

    public struct SixteenBPPObjectInfo
    {
        public int p16BPPData;
        public int usRegionIndex;
        public int ubShadeLevel;
        public int usWidth;
        public int usHeight;
        public int sOffsetX;
        public int sOffsetY;
    }
}