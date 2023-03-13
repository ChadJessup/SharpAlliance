using System;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace SharpAlliance.Core.Managers.Image
{
    public static class ImageConstants
    {
        // Defines for type of file readers
        public const int PCX_FILE_READER = 0x1;
        public const int TGA_FILE_READER = 0x2;
        public const int STCI_FILE_READER = 0x4;
        public const int TRLE_FILE_READER = 0x8;
        public const int UNKNOWN_FILE_READER = 0x200;

        // Defines for buffer bit depth
        public const int BUFFER_8BPP = 0x1;
        public const int BUFFER_16BPP = 0x2;

        // Palette structure, mimics that of Win32

        public const int AUX_FULL_TILE = 0x01;
        public const int AUX_ANIMATED_TILE = 0x02;
        public const int AUX_DYNAMIC_TILE = 0x04;
        public const int AUX_INTERACTIVE_TILE = 0x08;
        public const int AUX_IGNORES_HEIGHT = 0x10;
        public const int AUX_USES_LAND_Z = 0x20;
    }

    public class AuxObjectData
    {
        public int ubWallOrientation;
        public int ubNumberOfTiles;
        public int usTileLocIndex;
        public int[] ubUnused1;//[3];
        public int ubCurrentFrame;
        public int ubNumberOfFrames;
        public int fFlags;
        public int[] ubUnused;//[6];
    }

    public struct RelTileLoc
    {
        public int bTileOffsetX;
        public int bTileOffsetY;
    }// relative tile location

    // TRLE subimage structure, mirroring that of ST(C)I
    public struct ETRLEObject
    {
        public uint uiDataOffset;
        public uint uiDataLength;
        public short sOffsetX;
        public short sOffsetY;
        public int usHeight;
        public int usWidth;
    }

    public struct ETRLEData
    {
        public byte[] pPixData;
        public uint uiSizePixData;
        public ETRLEObject[] pETRLEObject;
        public int usNumberOfObjects;
    }

    [Flags]
    public enum HIMAGECreateFlags
    {
        // Defines for image charactoristics
        IMAGE_COMPRESSED = 0x0001,
        IMAGE_TRLECOMPRESSED = 0x0002,
        IMAGE_PALETTE = 0x0004,
        IMAGE_BITMAPDATA = 0x0008,
        IMAGE_APPDATA = 0x0010,
        IMAGE_ALLIMAGEDATA = 0x000C,
        IMAGE_ALLDATA = 0x001C,
    }

    public struct SGPPaletteEntry
    {
        public int peRed { get; set; }
        public int peGreen { get; set; }
        public int peBlue { get; set; }
        public readonly int peFlags { get; init; }
    }
}
