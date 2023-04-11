using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using SharpAlliance.Core.Interfaces;
using SharpAlliance.Platform.Interfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;

using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core.Managers.Image;

public class STCIImageFileLoader : IImageFileLoader, IImageDecoder
{
    public const int STCI_HEADER_SIZE = 64;
    public const string STCI_ID_STRING = "STCI";
    public const int STCI_ID_LEN = 4;
    public const uint STCI_SUBIMAGE_SIZE = 16;
    public const int STCI_PALETTE_ELEMENT_SIZE = 3;
    public const int STCI_8BIT_PALETTE_SIZE = 768;

    public bool LoadImage(ref HIMAGE hImage, HIMAGECreateFlags flags, IFileManager fileManager)
    {
        if (!FileManager.FileExists(hImage.ImageFile))
        {
            return false;
        }

        using var stream = FileManager.FileOpen(hImage.ImageFile, FileAccess.Read, fDeleteOnClose: false);
        var config = SixLabors.ImageSharp.Configuration.Default;
        config.Properties.Clear();

        config.Properties.Add(stream, hImage);
        config.Properties.Add(typeof(IFileManager), fileManager);
        config.Properties.Add(typeof(HIMAGECreateFlags), flags);

        var dopt = new DecoderOptions()
        {
            Configuration = config,
        };

        var i = Image<Rgba32>.Load(dopt, stream);

        // parsing the image modifies the hImage, so reassign back.
        hImage = (HIMAGE)config.Properties[stream];
        hImage.ParsedImages.Add((Image<Rgba32>)i);
        hImage.usWidth = i.Width;
        hImage.usHeight = i.Height;

        return true;
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
            image = new Image<TPixel>(1, 1);// this.DecodeETRLECompressed<TPixel>(header, configuration, stream);
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

        Image<TPixel> image = new(configuration, pHeader.usWidth, pHeader.usHeight);

        uint uiFileSectionSize;
        int uiBytesRead;
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
//            if (!FileManager.FileRead(stream, ref pSTCIPalette, (int)uiFileSectionSize, out uiBytesRead) || uiBytesRead != uiFileSectionSize)
//            {
//                //DbgMessage(TOPIC_HIMAGE, DBG_LEVEL_3, "Problem loading palette!");
//                //FileClose(hFile);
//                //MemFree(pSTCIPalette);
//                return null;
//            }
//            else if (!this.STCISetPalette(ref pSTCIPalette, ref hImage))
//            {
//                // DbgMessage(TOPIC_HIMAGE, DBG_LEVEL_3, "Problem setting hImage-format palette!");
//                // FileClose(hFile);
//                // MemFree(pSTCIPalette);
//                return null;
//            }

            hImage.fFlags |= HIMAGECreateFlags.IMAGE_PALETTE;
        }
        else if (fContents.HasFlag(HIMAGECreateFlags.IMAGE_BITMAPDATA | HIMAGECreateFlags.IMAGE_APPDATA))
        {
            // seek past the palette
            uiFileSectionSize = pHeader.Indexed.uiNumberOfColours * STCI_PALETTE_ELEMENT_SIZE;
            if (FileManager.FileSeek(stream, ref uiFileSectionSize, SeekOrigin.Current) == false)
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

//                if (!FileManager.FileRead(stream, ref hImage.pETRLEObject, uiFileSectionSize, out uiBytesRead) || uiBytesRead != uiFileSectionSize)
//                {
//                    return null;
//                }

                hImage.uiSizePixData = pHeader.uiStoredSize;
                hImage.fFlags |= HIMAGECreateFlags.IMAGE_TRLECOMPRESSED;
            }

//            hImage.pImageData = new byte[pHeader.uiStoredSize];
//            if (!FileManager.FileRead(stream, ref hImage.pImageData, pHeader.uiStoredSize, out uiBytesRead) || uiBytesRead != pHeader.uiStoredSize)
//            {
//                return null;
//            }

            hImage.fFlags |= HIMAGECreateFlags.IMAGE_BITMAPDATA;
        }
        else if (fContents.HasFlag(HIMAGECreateFlags.IMAGE_APPDATA)) // then there's a point in seeking ahead
        {
            if (FileManager.FileSeek(stream, ref pHeader.uiStoredSize, SeekOrigin.Current) == false)
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
//            if (!FileManager.FileRead(stream, ref hImage.pAppData, pHeader.uiAppDataSize, out uiBytesRead) || uiBytesRead != pHeader.uiAppDataSize)
//            {
//
//            }

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

    public List<Image<Rgba32>> CreateIndexedImages(
        ref HIMAGE hImage,
        HVOBJECT hVObject)
    {
        var rgba32 = new Rgba32();
        Rgba32 color = default;

        //using var byteBuffer = configuration.MemoryAllocator.AllocateManagedByteBuffer(numOfPixels * image.PixelType.BitsPerPixel);
        ReadOnlySpan<byte> indexSpan = new(hVObject.pPixData);
        hImage.ParsedImages = new(hImage.pETRLEObject.Length);

        foreach (var etrle in hImage.pETRLEObject)
        {
            int idx = 0;
            var numOfPixels = etrle.usHeight * etrle.usWidth;
            var image = new Image<Rgba32>(etrle.usWidth, etrle.usHeight);
            var imageSpan = indexSpan.Slice((int)etrle.uiDataOffset, (int)etrle.uiDataLength);

            var uncompressedData = this.DecompressETRLEBytes(imageSpan.ToArray());

            image.ProcessPixelRows(p =>
            {
                ReadOnlySpan<Rgba32> paletteSpan = new(hVObject.Palette);

                for (int y = 0; y < image.Height; y++)
                {
                    Span<Rgba32> pixelRow = p.GetRowSpan(y);
                    for (int x = 0; x < image.Width && idx < uncompressedData.Length; x++)
                    {
                        var pixel = paletteSpan[uncompressedData[idx]];

                        // This is the alpha pixel after all the conversions...so replace with
                        // RGBA32 alpha pixel.
                        if (uncompressedData[idx] == 0)
                        {
                            pixel = IVideoManager.AlphaPixel;
                        }

                        color.FromRgba32(pixel);

                        pixelRow[x] = color;
                        idx++;
                    }
                }
            });

            hImage.ParsedImages.Add(image);
        }

        return hImage.ParsedImages;
    }

    public const byte COMPRESSED_FLAG = 0x80;
    public const int MAX_COMPR_BYTES = 127;
    public const byte ALPHA_VALUE = 0;
    public const byte IS_COMPRESSED_BYTE_MASK = 0x80;
    public const byte NUMBER_OF_BYTES_MASK = 0x7F;

    public byte[] DecompressETRLEBytes(byte[] data)
    {
        var number_of_compressed_bytes = data.Length;
        var compressed_bytes = data;
        using MemoryStream extracted_buffer = new(number_of_compressed_bytes);
        var bytes_til_next_control_byte = 0;

        foreach (var current_byte in compressed_bytes)
        {
            if (bytes_til_next_control_byte == 0)
            {
                var is_compressed_alpha_byte = ((current_byte & IS_COMPRESSED_BYTE_MASK) >> 7) == 1;
                var length_of_subsequence = current_byte & NUMBER_OF_BYTES_MASK;
                if (is_compressed_alpha_byte)
                {
                    foreach (var s in Enumerable.Range(0, length_of_subsequence))
                    {
                        extracted_buffer.WriteByte(ALPHA_VALUE);
                    }
                }
                else
                {
                    bytes_til_next_control_byte = length_of_subsequence;
                }
            }
            else
            {
                extracted_buffer.WriteByte(current_byte);
                bytes_til_next_control_byte -= 1;
            }

            if (bytes_til_next_control_byte != 0)
            {
                //raise EtrleException('Not enough data to decompress')
            }
        }

        return extracted_buffer.ToArray();
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

    private Image<TPixel> DecodeRgba<TPixel>(STCIHeader header, Configuration configuration, Stream stream) where TPixel : unmanaged, IPixel<TPixel>
    {
        var rgba32 = new Rgba32();
        TPixel color = default;

        var numOfPixels = header.usHeight * header.usWidth;

        using var byteBuffer = configuration.MemoryAllocator.Allocate<byte>(numOfPixels * header.ubDepth);
        stream.Read(byteBuffer.Memory.Span);

        var image = new Image<TPixel>(configuration, header.usWidth, header.usHeight);

        int idx = 0;

        image.ProcessPixelRows(p =>
        {
            Span<ushort> pixelSpan = MemoryMarshal.Cast<byte, ushort>(byteBuffer.Memory.Span);

            for (int y = 0; y < header.usHeight; y++)
            {

                Span<TPixel> pixelRow = p.GetRowSpan(y);
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
        });

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

    public List<Image<Rgba32>> ApplyPalette(ref HVOBJECT hVObject, ref HIMAGE hImage)
    {
        var img = this.CreateIndexedImages(ref hImage, hVObject);

        return hImage.ParsedImages;
    }

    public Image<TPixel> Decode<TPixel>(Configuration configuration, Stream stream, CancellationToken cancellationToken) where TPixel : unmanaged, IPixel<TPixel>
    {
        throw new NotImplementedException();
    }

    public SixLabors.ImageSharp.Image Decode(Configuration configuration, Stream stream, CancellationToken cancellationToken)
    => Decode(configuration, stream);

    public ImageInfo Identify(DecoderOptions options, Stream stream)
    {
        throw new NotImplementedException();
    }

    public Task<ImageInfo> IdentifyAsync(DecoderOptions options, Stream stream, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Image<TPixel> Decode<TPixel>(DecoderOptions options, Stream stream) where TPixel : unmanaged, IPixel<TPixel>
    {
        throw new NotImplementedException();
    }

    public SixLabors.ImageSharp.Image Decode(DecoderOptions options, Stream stream)
    {
        throw new NotImplementedException();
    }

    public Task<Image<TPixel>> DecodeAsync<TPixel>(DecoderOptions options, Stream stream, CancellationToken cancellationToken = default) where TPixel : unmanaged, IPixel<TPixel>
    {
        throw new NotImplementedException();
    }

    public Task<SixLabors.ImageSharp.Image> DecodeAsync(DecoderOptions options, Stream stream, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
