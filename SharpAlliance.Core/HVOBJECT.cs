using System.Collections.Generic;
using SharpAlliance.Core.Managers;
using SharpAlliance.Core.Managers.Image;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace SharpAlliance.Core;

// VOBJECT FLAGS
public enum VOBJECT_FLAG
{
    None = 0,
    SHADETABLE_SHARED = 0x00000100,
}

public class HVOBJECT
{
    public const int SHADE_TABLES = 48;

    public VOBJECT_FLAG fFlags;                                                 // Special flags
    public uint uiSizePixData;                                         // ETRLE data size
    public List<SGPPaletteEntry> pPaletteEntry = [];                // 8BPP Palette
    public int TransparentColor;                                       // Defaults to 0,0,0
    public ushort?[] p16BPPPalette = [];                                   // A 16BPP palette used for 8.16 blits
    public Rgba32[] Palette = [];

    public byte[] pPixData = [];                       // ETRLE pixel data
    public ETRLEObject[] pETRLEObject = [];              // Object offset data etc
    public SixteenBPPObjectInfo[] p16BPPObject = [];
    public ushort?[][] pShades = new ushort?[SHADE_TABLES][]; // Shading tables
    // public ushort[] pShadeCurrent;
    public Rgba32[] ShadeCurrentPixels = [];
    public int? pGlow;                              // glow highlight table
    public byte? pShade8;                         // 8-bit shading index table
    public byte? pGlow8;                          // 8-bit glow table
    public List<ZStripInfo> ppZStripInfo = [];              // Z-value strip info arrays

    public int usNumberOf16BPPObjects;
    public int usNumberOfObjects;   // Total number of objects
    public int ubBitDepth;                       // BPP 
    public ushort? pShadeCurrent;

    public string Name { get; set; } = string.Empty;
    public HIMAGE? hImage { get; set; }
    public Image<Rgba32>[] Images { get; set; } = [];
    //public Surface[] Surfaces { get; set; }
    //public Texture[] Textures { get; set; }
}
