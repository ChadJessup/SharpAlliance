﻿using System;
using System.Collections.Generic;
using System.Drawing;

namespace SharpAlliance.Core.Managers.VideoSurfaces
{
    public struct HVSURFACE
    {
        public int usHeight;                            // Height of Video Surface
        public int usWidth;                         // Width of Video Surface
        public int ubBitDepth;                       // BPP ALWAYS 16!
        public object pSurfaceData;                   // A void pointer, but for this implementation, is really a lpDirectDrawSurface;
        public object pSurfaceData1;              // Direct Draw One Interface
        public object pSavedSurfaceData1;     // A void pointer, but for this implementation, is really a lpDirectDrawSurface;
                                              // pSavedSurfaceData is used to hold all video memory Surfaces so that they my be restored
        public object pSavedSurfaceData;      // A void pointer, but for this implementation, is really a lpDirectDrawSurface;
                                              // pSavedSurfaceData is used to hold all video memory Surfaces so that they my be restored
        public int fFlags;                              // Used to describe memory usage, etc
        public byte[]? pPalette;                         // A void pointer, but for this implementation a DDPalette
        public int? p16BPPPalette;              // A 16BPP palette used for 8->16 blits
        public int TransparentColor;          // Defaults to 0,0,0
        public byte[] pClipper;                           // A void pointer encapsolated as a clipper Surface
        public List<int> RegionList;                       // A List of regions within the video Surface
    }

    public class VSURFACE_NODE
    {
        public HVSURFACE hVSurface;
        public int uiIndex;

        public VSURFACE_NODE? next;
        public VSURFACE_NODE? prev;

        public int? pName;
        public int? pCode;
    }

    /// <summary>
    /// This structure describes the creation parameters for a Video Surface.
    /// </summary>
    public struct VSURFACE_DESC
    {
        public int fCreateFlags;                        // Specifies creation flags like from file or not
        public string ImageFile;                          // Filename of image data to use
        public int usWidth;                             // Width, ignored if given from file
        public int usHeight;                                // Height, ignored if given from file
        public int ubBitDepth;                          // BPP, ignored if given from file
    }

    public struct VSurfaceRegion
    {
        public Rectangle RegionCoords;          // Rectangle describing coordinates of region
        public Point Origin;                    // Origin used for hot spots, etc
        public int ubHitMask;                   // Byte flags for hit detection
    }

    public enum Surfaces : uint
    {
        PRIMARY_SURFACE = 0xFFFFFFF0,
        BACKBUFFER = 0xFFFFFFF1,
        FRAME_BUFFER = 0xFFFFFFF2,
        MOUSE_BUFFER = 0xFFFFFFF3,
    }

    [Flags]
    public enum BlitTypes
    {
        Unknown = 0x0000000,
        USECOLORKEY = 0x000000002,
        FAST = 0x000000004,
        CLIPPED = 0x000000008,
        SRCREGION = 0x000000010,
        COLORFILL = 0x000000020,
        SRCSUBRECT = 0x000000040,
        DESTREGION = 0x000000080,
        COLORFILLRECT = 0x000000100,
        USEDESTCOLORKEY = 0x000000200,
        MIRROR_Y = 0x000001000,
    }
}