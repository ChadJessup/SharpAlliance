using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using SharpAlliance.Platform.Interfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;

namespace SharpAlliance.Core.Managers.Image
{
    public interface IImageFileLoader
    {
        bool LoadImage(ref HIMAGE hIMAGE, HIMAGECreateFlags flags, IFileManager fileManager);
        List<Image<Rgba32>> ApplyPalette(ref HVOBJECT hVObject, ref HIMAGE hImage);
    }

    public class TGAImageFileLoader : IImageFileLoader
    {
        public List<Image<Rgba32>> ApplyPalette(ref HVOBJECT hVObject, ref HIMAGE hImage)
        {
            throw new NotImplementedException();
        }

        public bool LoadImage(ref HIMAGE hIMAGE, HIMAGECreateFlags flags, IFileManager fileManager)
        {
            return false;
        }
    }

    public class PCXImageFileLoader : IImageFileLoader
    {
        public List<Image<Rgba32>> ApplyPalette(ref HVOBJECT hVObject, ref HIMAGE hImage)
        {
            throw new NotImplementedException();
        }

        public bool LoadImage(ref HIMAGE hIMAGE, HIMAGECreateFlags flags, IFileManager fileManager)
        {
            return false;
        }
    }

    // NB if you're going to change the header definition:
    // - make sure that everything in this header is nicely aligned
    // - don't exceed the 64-byte maximum
    [StructLayout(LayoutKind.Explicit, Size = STCIImageFileLoader.STCI_HEADER_SIZE)]
    public unsafe struct STCIHeader
    {
        [FieldOffset(00)] public byte cID0;//[STCIImageFileLoader.STCI_ID_LEN];
        [FieldOffset(01)] public byte cID1;//[STCIImageFileLoader.STCI_ID_LEN];
        [FieldOffset(02)] public byte cID2;//[STCIImageFileLoader.STCI_ID_LEN];
        [FieldOffset(03)] public byte cID3;//[STCIImageFileLoader.STCI_ID_LEN];

        [FieldOffset(04)] public uint uiOriginalSize;
        [FieldOffset(08)] public uint uiStoredSize; // equal to uiOriginalSize if data uncompressed
        [FieldOffset(12)] public uint uiTransparentValue;
        [FieldOffset(16)] public STCITypes fFlags;
        [FieldOffset(20)] public ushort usHeight;
        [FieldOffset(22)] public ushort usWidth;
        [FieldOffset(24)] public RGB Rgb;
        [FieldOffset(24)] public Indexed Indexed;
        [FieldOffset(44)] public byte ubDepth;  // size in bits of one pixel as stored in the file
        [FieldOffset(45)] public uint uiAppDataSize;
        [FieldOffset(49)] public fixed byte cUnused[15];
    }

    [Flags]
    public enum STCITypes : uint
    {
        STCI_ETRLE_COMPRESSED = 0x0020,
        STCI_ZLIB_COMPRESSED = 0x0010,
        STCI_INDEXED = 0x0008,
        STCI_RGB = 0x0004,
        STCI_ALPHA = 0x0002,
        STCI_TRANSPARENT = 0x0001,
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct STCISubImage
    {
        public uint uiDataOffset;
        public uint uiDataLength;
        public short sOffsetX;
        public short sOffsetY;
        public ushort usHeight;
        public ushort usWidth;
    }

    [StructLayout(LayoutKind.Explicit, Size = 20)]
    public unsafe struct Indexed
    {
        // For indexed files, the palette will contain 3 separate bytes for red, green, and blue
        [FieldOffset(00)] public uint uiNumberOfColours;
        [FieldOffset(04)] public ushort usNumberOfSubImages;
        [FieldOffset(06)] public byte ubRedDepth;
        [FieldOffset(07)] public byte ubGreenDepth;
        [FieldOffset(08)] public byte ubBlueDepth;
        [FieldOffset(09)] public fixed byte cIndexedUnused[11];
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct STCIPaletteElement
    {
        public byte ubRed;
        public byte ubGreen;
        public byte ubBlue;
    }

    [StructLayout(LayoutKind.Explicit, Size = 20)]
    public struct RGB
    {
        [FieldOffset(00)] public uint uiRedMask;
        [FieldOffset(04)] public uint uiGreenMask;
        [FieldOffset(08)] public uint uiBlueMask;
        [FieldOffset(12)] public uint uiAlphaMask;
        [FieldOffset(16)] public byte ubRedDepth;
        [FieldOffset(17)] public byte ubGreenDepth;
        [FieldOffset(18)] public byte ubBlueDepth;
        [FieldOffset(19)] public byte ubAlphaDepth;
    }
}
