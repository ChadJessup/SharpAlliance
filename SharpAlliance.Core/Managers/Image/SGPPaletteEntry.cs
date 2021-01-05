﻿namespace SharpAlliance.Core.Managers.Image
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

        // Defines for image charactoristics
        public const int IMAGE_COMPRESSED = 0x0001;
        public const int IMAGE_TRLECOMPRESSED = 0x0002;
        public const int IMAGE_PALETTE = 0x0004;
        public const int IMAGE_BITMAPDATA = 0x0008;
        public const int IMAGE_APPDATA = 0x0010;
        public const int IMAGE_ALLIMAGEDATA = 0x000C;
        public const int IMAGE_ALLDATA = 0x001C;

        // Palette structure, mimics that of Win32

        public const int AUX_FULL_TILE = 0x01;
        public const int AUX_ANIMATED_TILE = 0x02;
        public const int AUX_DYNAMIC_TILE = 0x04;
        public const int AUX_INTERACTIVE_TILE = 0x08;
        public const int AUX_IGNORES_HEIGHT = 0x10;
        public const int AUX_USES_LAND_Z = 0x20;
    }

    public struct AuxObjectData
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
        public int uiDataOffset;
        public int uiDataLength;
        public int sOffsetX;
        public int sOffsetY;
        public int usHeight;
        public int usWidth;
    }

    public struct ETRLEData
    {
        public byte[] pPixData;
        public int uiSizePixData;
        public ETRLEObject pETRLEObject;
        public int usNumberOfObjects;
    }

    // Image header structure
    public struct HIMAGE
    {
        public int usWidth;
        public int usHeight;
        public int ubBitDepth;
        public int fFlags;
        public string ImageFile;
        public int iFileLoader;
        public SGPPaletteEntry pPalette;
        public int pui16BPPPalette;
        public int pAppData;
        public int uiAppDataSize;
        // This union is used to describe each data type and is flexible to include the
        // data strucutre of the compresssed format, once developed.
        public byte[] pImageData;
        public byte[] pCompressedImageData;
        public int p8BPPData;
        public int p16BPPData;
        public int pPixData8;
        public int uiSizePixData;
        public ETRLEObject pETRLEObject;
        public int usNumberOfObjects;
    }

    public readonly struct SGPPaletteEntry
    {
        public readonly byte peRed;
        public readonly byte peGreen;
        public readonly byte peBlue;
        public readonly byte peFlags;
    }
}