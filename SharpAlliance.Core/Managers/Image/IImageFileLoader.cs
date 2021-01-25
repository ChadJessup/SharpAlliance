using System;
using System.Buffers;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using SharpAlliance.Platform.Interfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Memory;
using SixLabors.ImageSharp.PixelFormats;

namespace SharpAlliance.Core.Managers.Image
{
    public interface IImageFileLoader
    {
        ValueTask<bool> LoadImage(ref HIMAGE hIMAGE, HIMAGECreateFlags flags, IFileManager fileManager);
    }

    public class TGAImageFileLoader : IImageFileLoader
    {
        public ValueTask<bool> LoadImage(ref HIMAGE hIMAGE, HIMAGECreateFlags flags, IFileManager fileManager)
        {
            return ValueTask.FromResult(false);
        }
    }

    public class PCXImageFileLoader : IImageFileLoader
    {
        public ValueTask<bool> LoadImage(ref HIMAGE hIMAGE, HIMAGECreateFlags flags, IFileManager fileManager)
        {
            return ValueTask.FromResult(false);
        }
    }

    public class STCIImageFileLoader : IImageFileLoader, IImageDecoder
    {
        public const int STCI_HEADER_SIZE = 64;
        public const string STCI_ID_STRING = "STCI";
        public const int STCI_ID_LEN = 4;
        public const int STCI_SUBIMAGE_SIZE = 16;
        public const int STCI_PALETTE_ELEMENT_SIZE = 3;
        public const int STCI_8BIT_PALETTE_SIZE = 768;

        public ValueTask<bool> LoadImage(ref HIMAGE hImage, HIMAGECreateFlags flags, IFileManager fileManager)
        {
            if (!fileManager.FileExists(hImage.ImageFile))
            {
                return ValueTask.FromResult(false);
            }

            using var stream = fileManager.FileOpen(hImage.ImageFile, FileAccess.Read, fDeleteOnClose: false);
            var config = SixLabors.ImageSharp.Configuration.Default;
            config.Properties.Add(stream, hImage);
            var i = Image<Rgba32>.Load(config, stream, this);

            hImage.ParsedImage = (Image<Rgba32>)i;
            hImage.usWidth = i.Width;
            hImage.usHeight = i.Height;

            return ValueTask.FromResult(true);
        }

        public Image<TPixel> Decode<TPixel>(Configuration configuration, Stream stream) where TPixel : unmanaged, IPixel<TPixel>
        {
            Span<byte> buffer = stackalloc byte[Marshal.SizeOf<STCIHeader>()];
            stream.Read(buffer);

            var header = MemoryMarshal.Read<STCIHeader>(buffer);
            Image<TPixel> image;

            if (header.fFlags.HasFlag(STCITypes.STCI_RGB))
            {
                image = this.DecodeRgba<TPixel>(header, configuration, stream);
            }
            else if (header.fFlags.HasFlag(STCITypes.STCI_INDEXED))
            {
                image = this.DecodeIndexed<TPixel>(header, configuration, stream);
            }
            else if (header.fFlags.HasFlag(STCITypes.STCI_ETRLE_COMPRESSED))
            {
                image = this.DecodeETRLECompressed<TPixel>(header, configuration, stream);
            }
            else
            { 
                image = new Image<TPixel>(1, 1);
            }

            HIMAGE hImage = (HIMAGE)configuration.Properties[stream];
            return image;
        }

        private Image<TPixel> DecodeIndexed<TPixel>(STCIHeader pHeader, Configuration configuration, Stream stream) where TPixel : unmanaged, IPixel<TPixel>
        {
            Image<TPixel> image = new Image<TPixel>(pHeader.usWidth, pHeader.usHeight);

            uint uiFileSectionSize;
            uint uiBytesRead;
            int? pSTCIPalette = null;

            if (pHeader.fFlags & IMAGE_PALETTE)
            { // Allocate memory for reading in the palette
                if (pHeader.Indexed.uiNumberOfColours != 256)
                {
                    //DbgMessage(TOPIC_HIMAGE, DBG_LEVEL_3, "Palettized image has bad palette size.");
                    return null;
                }
                
                uiFileSectionSize = pHeader.Indexed.uiNumberOfColours * STCI_PALETTE_ELEMENT_SIZE;
                //pSTCIPalette = MemAlloc(uiFileSectionSize);
                if (pSTCIPalette == null)
                {
                    // DbgMessage(TOPIC_HIMAGE, DBG_LEVEL_3, "Out of memory!");
                    // FileClose(hFile);
                    return null;
                }

                // ATE: Memset: Jan 16/99
                //memset(pSTCIPalette, 0, uiFileSectionSize);

                // Read in the palette
                if (!this.files.FileRead(hFile, pSTCIPalette, uiFileSectionSize, &uiBytesRead) || uiBytesRead != uiFileSectionSize)
                {
                    //DbgMessage(TOPIC_HIMAGE, DBG_LEVEL_3, "Problem loading palette!");
                    //FileClose(hFile);
                    //MemFree(pSTCIPalette);
                    return null;
                }
                else if (!STCISetPalette(pSTCIPalette, hImage))
                {
                    // DbgMessage(TOPIC_HIMAGE, DBG_LEVEL_3, "Problem setting hImage-format palette!");
                    // FileClose(hFile);
                    // MemFree(pSTCIPalette);
                    return null;
                }

                hImage.fFlags |= IMAGE_PALETTE;
                // Free the temporary buffer
                // MemFree(pSTCIPalette);
            }
            else if (fContents & (IMAGE_BITMAPDATA | IMAGE_APPDATA))
            { // seek past the palette
                uiFileSectionSize = pHeader.Indexed.uiNumberOfColours * STCI_PALETTE_ELEMENT_SIZE;
                if (FileSeek(hFile, uiFileSectionSize, FILE_SEEK_FROM_CURRENT) == false)
                {
                    // DbgMessage(TOPIC_HIMAGE, DBG_LEVEL_3, "Problem seeking past palette!");
                    // FileClose(hFile);
                    return null;
                }
            }
            if (fContents & IMAGE_BITMAPDATA)
            {
                if (pHeader.fFlags.HasFlag(STCITypes.STCI_ETRLE_COMPRESSED))
                {
                    // load data for the subimage (object) structures
                    // Assert(sizeof(ETRLEObject) == STCI_SUBIMAGE_SIZE);
                    hImage.usNumberOfObjects = pHeader.Indexed.usNumberOfSubImages;
                    uiFileSectionSize = hImage.usNumberOfObjects * STCI_SUBIMAGE_SIZE;
                    hImage.pETRLEObject = MemAlloc(uiFileSectionSize);
                    if (hImage.pETRLEObject == null)
                    {
                        // DbgMessage(TOPIC_HIMAGE, DBG_LEVEL_3, "Out of memory!");
                        // FileClose(hFile);
                        if (fContents & IMAGE_PALETTE)
                        {
                            // MemFree(hImage.pPalette);
                        }
                        return null;
                    }
                    if (!FileRead(hFile, hImage.pETRLEObject, uiFileSectionSize, &uiBytesRead) || uiBytesRead != uiFileSectionSize)
                    {
                        // DbgMessage(TOPIC_HIMAGE, DBG_LEVEL_3, "Error loading subimage structures!");
                        // FileClose(hFile);
                        if (fContents & IMAGE_PALETTE)
                        {
                           // MemFree(hImage.pPalette);
                        }
                        // MemFree(hImage.pETRLEObject);
                        return null;
                    }

                    hImage.uiSizePixData = pHeader.uiStoredSize;
                    hImage.fFlags |= IMAGE_TRLECOMPRESSED;
                }
                // allocate memory for and read in the image data
                //hImage.pImageData = MemAlloc(pHeader.uiStoredSize);
                if (hImage.pImageData == null)
                {
                    // DbgMessage(TOPIC_HIMAGE, DBG_LEVEL_3, "Out of memory!");
                    // FileClose(hFile);
                    if (fContents & IMAGE_PALETTE)
                    {
                       // MemFree(hImage.pPalette);
                    }
                    if (hImage.usNumberOfObjects > 0)
                    {
                        //MemFree(hImage.pETRLEObject);
                    }

                    return null;
                }
                else if (!FileRead(hFile, hImage.pImageData, pHeader.uiStoredSize, &uiBytesRead) || uiBytesRead != pHeader.uiStoredSize)
                { // Problem reading in the image data!
                    // DbgMessage(TOPIC_HIMAGE, DBG_LEVEL_3, "Error loading image data!");
                    // FileClose(hFile);
                    // MemFree(hImage.pImageData);
                    if (fContents & IMAGE_PALETTE)
                    {
                       // MemFree(hImage.pPalette);
                    }
                    if (hImage.usNumberOfObjects > 0)
                    {
                        // MemFree(hImage.pETRLEObject);
                    }
                    return null;
                }

                hImage.fFlags |= IMAGE_BITMAPDATA;
            }
            else if (fContents & IMAGE_APPDATA) // then there's a point in seeking ahead
            {
                if (FileSeek(hFile, pHeader.uiStoredSize, FILE_SEEK_FROM_CURRENT) == false)
                {
                    // DbgMessage(TOPIC_HIMAGE, DBG_LEVEL_3, "Problem seeking past image data!");
                    // FileClose(hFile);
                    return null;
                }
            }

            if (fContents & IMAGE_APPDATA && pHeader.uiAppDataSize > 0)
            {
                // load application-specific data
                hImage.pAppData = MemAlloc(pHeader.uiAppDataSize);
                if (hImage.pAppData == null)
                {
                    // DbgMessage(TOPIC_HIMAGE, DBG_LEVEL_3, "Out of memory!");
                    // FileClose(hFile);
                    // MemFree(hImage.pAppData);
                    if (fContents & IMAGE_PALETTE)
                    {
                       // MemFree(hImage.pPalette);
                    }
                    if (fContents & IMAGE_BITMAPDATA)
                    {
                        //MemFree(hImage.pImageData);
                    }
                    if (hImage.usNumberOfObjects > 0)
                    {
                        // MemFree(hImage.pETRLEObject);
                    }
                    return null;
                }

                if (!FileRead(hFile, hImage.pAppData, pHeader.uiAppDataSize, out uiBytesRead) || uiBytesRead != pHeader.uiAppDataSize)
                {
                    // DbgMessage(TOPIC_HIMAGE, DBG_LEVEL_3, "Error loading application-specific data!");
                    // FileClose(hFile);
                    // MemFree(hImage.pAppData);
                    if (fContents & IMAGE_PALETTE)
                    {
                       // MemFree(hImage.pPalette);
                    }
                    if (fContents & IMAGE_BITMAPDATA)
                    {
                        // MemFree(hImage.pImageData);
                    }
                    if (hImage.usNumberOfObjects > 0)
                    {
                        // MemFree(hImage.pETRLEObject);
                    }
                    return null;
                }

                hImage.uiAppDataSize = pHeader.uiAppDataSize;

                hImage.fFlags |= IMAGE_APPDATA;
            }
            else
            {
                hImage.pAppData = null;
                hImage.uiAppDataSize = 0;
            }

            return image;
        }

        private Image<TPixel> DecodeETRLECompressed<TPixel>(STCIHeader header, Configuration configuration, Stream stream) where TPixel : unmanaged, IPixel<TPixel>
        {
            return new Image<TPixel>(1, 1);
        }

        private Image<TPixel> DecodeRgba<TPixel>(STCIHeader header, Configuration configuration, Stream stream) where TPixel : unmanaged, IPixel<TPixel>
        {
            var rgba32 = new Rgba32();
            TPixel color = default;

            var numOfPixels = header.usHeight * header.usWidth;

            using var byteBuffer = configuration.MemoryAllocator.AllocateManagedByteBuffer(numOfPixels * header.ubDepth);
            stream.Read(byteBuffer.Array);
            Span<ushort> pixelSpan = MemoryMarshal.Cast<byte, ushort>(byteBuffer.Memory.Span);

            var image = new Image<TPixel>(configuration, header.usWidth, header.usHeight);

            int idx = 0;

            for (int y = 0; y < header.usHeight; y++)
            {
                Span<TPixel> pixelRow = image.GetPixelRowSpan(y);
                for (int x = 0; x < header.usWidth; x++)
                {
                    var bgr565 = new Bgr565
                    {
                        PackedValue = pixelSpan[idx],
                    };

                    bgr565.ToRgba32(ref rgba32);
                    color.FromRgba32(rgba32);

                    pixelRow[x] = color;
                    idx++;
                }
            }

            return image;
        }

        public SixLabors.ImageSharp.Image Decode(Configuration configuration, Stream stream)
            => this.Decode<Rgba32>(configuration, stream);

        public Task<Image<TPixel>> DecodeAsync<TPixel>(Configuration configuration, Stream stream, CancellationToken cancellationToken) where TPixel : unmanaged, IPixel<TPixel>
        {
            throw new NotImplementedException();
        }

        public async Task<SixLabors.ImageSharp.Image> DecodeAsync(Configuration configuration, Stream stream, CancellationToken cancellationToken)
            => await this.DecodeAsync<Rgba32>(configuration, stream, cancellationToken);
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
