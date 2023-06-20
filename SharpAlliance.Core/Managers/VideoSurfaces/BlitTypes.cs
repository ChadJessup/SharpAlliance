using System;
using System.Collections.Generic;
using System.Drawing;
using Veldrid;
using SixLabors.ImageSharp;
using Rectangle = SixLabors.ImageSharp.Rectangle;
using Point = SixLabors.ImageSharp.Point;

using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core.Managers.VideoSurfaces;

public struct HVSURFACE
{
    public int usHeight;                            // Height of Video Surface
    public int usWidth;                         // Width of Video Surface
    public Texture Texture;
    public int TransparentColor;          // Defaults to 0,0,0
    public List<VSurfaceRegion> RegionList;                       // A List of regions within the video Surface
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
    public VSurfaceCreateFlags fCreateFlags;                        // Specifies creation flags like from file or not
    public string ImageFile;                          // Filename of image data to use
    public int usWidth;                             // Width, ignored if given from file
    public int usHeight;                                // Height, ignored if given from file
    public int ubBitDepth;                          // BPP, ignored if given from file
}

[Flags]
public enum VSurfaceCreateFlags
{
    VSURFACE_DEFAULT_MEM_USAGE = 0x00000001, // Default mem usage is same as DD, try video and then try system. Will usually work
    VSURFACE_VIDEO_MEM_USAGE = 0x00000002, // Will force surface into video memory and will fail if it can't
    VSURFACE_SYSTEM_MEM_USAGE = 0x00000004, // Will force surface into system memory and will fail if it can't
    VSURFACE_CREATE_DEFAULT = 0x00000020, // Creates and empty Surface of given width, height and BPP
    VSURFACE_CREATE_FROMFILE = 0x00000040, // Creates a video Surface from a file ( using HIMAGE )
    VSURFACE_RESERVED_SURFACE = 0x00000100,	// Reserved for special purposes, like a primary surface
}

public struct VSurfaceRegion
{
    public Rectangle RegionCoords;          // Rectangle describing coordinates of region
    public Point Origin;                    // Origin used for hot spots, etc
    public int ubHitMask;                   // Byte flags for hit detection
}

public enum Surfaces : uint
{
    Unknown         = 0x00000000,
    PRIMARY_SURFACE = 0xFFFFFFF0,
    BACKBUFFER      = 0xFFFFFFF1,
    FRAME_BUFFER    = 0xFFFFFFF2,
    MOUSE_BUFFER    = 0xFFFFFFF3,

    RENDER_BUFFER   = 0x00000001,
    SAVE_BUFFER     = 0x00000002,
    EXTRA_BUFFER    = 0x00000003,
}

[Flags]
public enum BlitTypes
{
    Unknown         = 0x000000000,
    USECOLORKEY     = 0x000000002,
    FAST            = 0x000000004,
    CLIPPED         = 0x000000008,
    SRCREGION       = 0x000000010,
    COLORFILL       = 0x000000020,
    SRCSUBRECT      = 0x000000040,
    DESTREGION      = 0x000000080,
    COLORFILLRECT   = 0x000000100,
    USEDESTCOLORKEY = 0x000000200,
    MIRROR_Y        = 0x000001000,
}
