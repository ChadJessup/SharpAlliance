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
        public const uint STCI_SUBIMAGE_SIZE = 16;
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
            config.Properties.Clear();

            config.Properties.Add(stream, hImage);
            config.Properties.Add(typeof(IFileManager), fileManager);
            config.Properties.Add(typeof(HIMAGECreateFlags), flags);

            var i = Image<Rgba32>.Load(config, stream, this);

            // parsing the image modifies the hImage, so reassign back.
            hImage = (HIMAGE)config.Properties[stream];
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
            else if (header.fFlags.HasFlag(STCITypes.STCI_ZLIB_COMPRESSED))
            {
                image = this.DecodeETRLECompressed<TPixel>(header, configuration, stream);
            }
            else
            {
                image = new Image<TPixel>(1, 1);
            }

            return image;
        }

        private Image<TPixel> DecodeIndexed<TPixel>(STCIHeader pHeader, Configuration configuration, Stream stream) where TPixel : unmanaged, IPixel<TPixel>
        {
            IFileManager files = (IFileManager)configuration.Properties[typeof(IFileManager)];
            HIMAGE hImage = (HIMAGE)configuration.Properties[stream];
            HIMAGECreateFlags fContents = (HIMAGECreateFlags)configuration.Properties[typeof(HIMAGECreateFlags)];

            Image<TPixel> image = new Image<TPixel>(configuration, pHeader.usWidth, pHeader.usHeight);

            uint uiFileSectionSize;
            uint uiBytesRead;
            byte[]? pSTCIPalette = null;

            if (fContents.HasFlag(HIMAGECreateFlags.IMAGE_PALETTE))
            {
                // Allocate memory for reading in the palette
                if (pHeader.Indexed.uiNumberOfColours != 256)
                {
                    //DbgMessage(TOPIC_HIMAGE, DBG_LEVEL_3, "Palettized image has bad palette size.");
                    return null;
                }

                uiFileSectionSize = pHeader.Indexed.uiNumberOfColours * STCI_PALETTE_ELEMENT_SIZE;
                pSTCIPalette = new byte[uiFileSectionSize];

                // ATE: Memset: Jan 16/99
                //memset(pSTCIPalette, 0, uiFileSectionSize);

                // Read in the palette
                if (!files.FileRead(stream, ref pSTCIPalette, uiFileSectionSize, out uiBytesRead) || uiBytesRead != uiFileSectionSize)
                {
                    //DbgMessage(TOPIC_HIMAGE, DBG_LEVEL_3, "Problem loading palette!");
                    //FileClose(hFile);
                    //MemFree(pSTCIPalette);
                    return null;
                }
                else if (!STCISetPalette(ref pSTCIPalette, ref hImage))
                {
                    // DbgMessage(TOPIC_HIMAGE, DBG_LEVEL_3, "Problem setting hImage-format palette!");
                    // FileClose(hFile);
                    // MemFree(pSTCIPalette);
                    return null;
                }

                hImage.fFlags |= HIMAGECreateFlags.IMAGE_PALETTE;
            }
            else if (fContents.HasFlag(HIMAGECreateFlags.IMAGE_BITMAPDATA | HIMAGECreateFlags.IMAGE_APPDATA))
            {
                // seek past the palette
                uiFileSectionSize = pHeader.Indexed.uiNumberOfColours * STCI_PALETTE_ELEMENT_SIZE;
                if (files.FileSeek(stream, ref uiFileSectionSize, SeekOrigin.Current) == false)
                {
                    // DbgMessage(TOPIC_HIMAGE, DBG_LEVEL_3, "Problem seeking past palette!");
                    // FileClose(hFile);
                    return null;
                }
            }

            if (fContents.HasFlag(HIMAGECreateFlags.IMAGE_BITMAPDATA))
            {
                if (pHeader.fFlags.HasFlag(STCITypes.STCI_ETRLE_COMPRESSED))
                {
                    // load data for the subimage (object) structures
                    hImage.usNumberOfObjects = pHeader.Indexed.usNumberOfSubImages;
                    uiFileSectionSize = hImage.usNumberOfObjects * STCI_SUBIMAGE_SIZE;

                    if (!files.FileRead<ETRLEObject>(stream, ref hImage.pETRLEObject, uiFileSectionSize, out uiBytesRead) || uiBytesRead != uiFileSectionSize)
                    {
                        return null;
                    }

                    hImage.uiSizePixData = pHeader.uiStoredSize;
                    hImage.fFlags |= HIMAGECreateFlags.IMAGE_TRLECOMPRESSED;
                }

                hImage.pImageData = new byte[pHeader.uiStoredSize];
                if (!files.FileRead(stream, ref hImage.pImageData, pHeader.uiStoredSize, out uiBytesRead) || uiBytesRead != pHeader.uiStoredSize)
                {
                    return null;
                }

                hImage.fFlags |= HIMAGECreateFlags.IMAGE_BITMAPDATA;
            }
            else if (fContents.HasFlag(HIMAGECreateFlags.IMAGE_APPDATA)) // then there's a point in seeking ahead
            {
                if (files.FileSeek(stream, ref pHeader.uiStoredSize, SeekOrigin.Current) == false)
                {
                    // DbgMessage(TOPIC_HIMAGE, DBG_LEVEL_3, "Problem seeking past image data!");
                    // FileClose(hFile);
                    return null;
                }
            }

            if (fContents.HasFlag(HIMAGECreateFlags.IMAGE_APPDATA) && pHeader.uiAppDataSize > 0)
            {
                // load application-specific data
                hImage.pAppData = new byte[pHeader.uiAppDataSize];
                if (!files.FileRead(stream, ref hImage.pAppData, pHeader.uiAppDataSize, out uiBytesRead) || uiBytesRead != pHeader.uiAppDataSize)
                {

                }

                hImage.uiAppDataSize = pHeader.uiAppDataSize;

                hImage.fFlags |= HIMAGECreateFlags.IMAGE_APPDATA;
            }
            else
            {
                hImage.pAppData = null;
                hImage.uiAppDataSize = 0;
            }

            hImage.ubBitDepth = pHeader.ubDepth;
            configuration.Properties[stream] = hImage;

            return image;
        }

        public Image<TPixel> CreateIndexedImage<TPixel>(
            Configuration configuration,
            ref Image<TPixel> image,
            ref HIMAGE hImage,
            ref HVOBJECT hVObject)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            var rgba32 = new Rgba32();
            TPixel color = default;

            var numOfPixels = image.Height * image.Width;

            //using var byteBuffer = configuration.MemoryAllocator.AllocateManagedByteBuffer(numOfPixels * image.PixelType.BitsPerPixel);
            ReadOnlySpan<byte> indexSpan = new ReadOnlySpan<byte>(hVObject.pPixData);
            ReadOnlySpan<Rgba32> paletteSpan = new ReadOnlySpan<Rgba32>(hVObject.Palette);
            
            int idx = 0;

            try
            {
                for (int y = 0; y < image.Height; y++)
                {
                    Span<TPixel> pixelRow = image.GetPixelRowSpan(y);
                    for (int x = 0; x < image.Width; x++)
                    {
                        var pixel = paletteSpan[indexSpan[idx]];
                        color.FromRgba32(pixel);

                        pixelRow[x] = color;
                        idx++;
                    }
                }
            }
            catch (Exception e)
            {

            }
            return image;
        }

        private bool STCISetPalette(ref byte[] pSTCIPalette, ref HIMAGE hImage)
        {
            ushort usIndex;
            var paletteSpan = new ReadOnlySpan<byte>(pSTCIPalette);

            var pubPaletteIdx = 0;
            var pubPalette = MemoryMarshal.Cast<byte, STCIPaletteElement>(paletteSpan)
                .ToArray();
            //pubPalette = MemoryMarshal.Read<STCIPaletteElement>(paletteSpan);

            // Allocate memory for palette
            hImage.pPalette = new SGPPaletteEntry[256];

            if (hImage.pPalette == null)
            {
                return false;
            }

            // Initialize the proper palette entries
            for (usIndex = 0; usIndex < 256; usIndex++)
            {
                var paletteEntry = new SGPPaletteEntry
                {
                    peRed = pubPalette[pubPaletteIdx].ubRed,
                    peGreen = pubPalette[pubPaletteIdx].ubGreen,
                    peBlue = pubPalette[pubPaletteIdx].ubBlue,
                    peFlags = 0,
                };

                hImage.pPalette[usIndex] = paletteEntry;
                pubPaletteIdx++;
            }

            return true;
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
